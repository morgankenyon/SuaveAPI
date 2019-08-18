open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.ServerErrors
open Suave.Writers
open Newtonsoft.Json

//types
type NewAnswer =
    {
        Text: string
    }
type Answer = 
    {
        Text: string
        AnswerId: int
    }
    
//low level functions
let getString (rawForm: byte[]) =
    System.Text.Encoding.UTF8.GetString(rawForm)

let fromJson<'a> json =
    JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

//fake db functions
let getAnswerFromDb id =
    { Text = "Sample Text"; AnswerId = id}

let createAnswerDb (newAnswer: NewAnswer) =
    { Text = newAnswer.Text; AnswerId = 10}

let updateAnswerDb answer =
    answer

let deleteAnswerFromDb id =
    true

//bridge functions between routes and fake db
let getAnswer id =
    getAnswerFromDb id
    |> JsonConvert.SerializeObject
    |> OK
    >=> setMimeType "application/json"

let createAnswer =
    request (fun r ->
    r.rawForm
    |> getString
    |> fromJson<NewAnswer>
    |> createAnswerDb
    |> JsonConvert.SerializeObject
    |> CREATED )
    >=> setMimeType "application/json"

let updateAnswer =
    request (fun r ->
    r.rawForm
    |> getString
    |> fromJson<Answer>
    |> updateAnswerDb
    |> JsonConvert.SerializeObject
    |> OK )
    >=> setMimeType "application/json"


let deleteAnswer id =
    let successful = deleteAnswerFromDb id
    if successful then
        NO_CONTENT
    else
        INTERNAL_ERROR "Could not delete resource"
        

//setup app routes
let app =
    choose
        [ GET >=> choose
            [ path "/" >=> OK "Hello World" 
              pathScan "/answer/%d" (fun id -> getAnswer id) ]

          POST >=> choose
            [ path "/answer" >=> createAnswer ]

          PUT >=> choose
            [ path "/answer" >=> updateAnswer ]

          DELETE >=> choose
            [ pathScan "/answer/%d" (fun id -> deleteAnswer id) ]
        ]

[<EntryPoint>]
let main argv =
    startWebServer defaultConfig app
    0
