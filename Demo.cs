using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoDocumentDB
{
    public class Program
    {
        private static readonly string endpoint = "https://dsodocumentdb.documents.azure.com:443/";
        private static readonly string primaryKey = "zdztF757TrQ0walwRd8Sa1FKfHJD9ESY0ECqwbN3BLIhy4xum27zuDS9F2KKiloPGHUv4nuJPRHQEM6X48Puxg==";

        private static Database database;
        private static DocumentCollection collection;
        public static void Main(string[] args)
        {
            try
            {
                CreateDocumentClient().Wait();
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            Console.ReadKey();
        }

        private static async Task CreateDocumentClient()
        {
            // Create a new instance of the DocumentClient

            using (var client = new DocumentClient(new Uri(endpoint), primaryKey))
            {
                database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = 'demodatabase'").AsEnumerable().First();

                collection = client.CreateDocumentCollectionQuery(database.CollectionsLink,
                   "SELECT * FROM c WHERE c.id = 'testdocs'").AsEnumerable().First();

                // await CreateDocuments(client);
                // await QueryDocumentsWithPaging(client);
                // await ReplaceDocuments(client);
                // await ReadDocument();
                await ReadFromDocument(client);

            }
        }

        private async static Task CreateDocuments(DocumentClient client)
        {
            Console.WriteLine();
            Console.WriteLine("**** Create Documents ****");
            Console.WriteLine();

            dynamic documentExample = new
            {
                name = "Yulong",
                alias = "Kingmaker",
                address = new
                {
                    addressType = "Manor",
                    addressLine1 = "123 S Alaska Dr",
                    location = new
                    {
                        city = "Skywatch",
                        stateProvinceName = "Auridon"
                    },
                    postalCode = "90210",
                    countryRegionName = "Tamriel"
                },
            };

            Document doc = await CreateDocument(client, documentExample);
            Console.WriteLine("Created document {0} from dynamic object", doc.Id);
            Console.WriteLine();
        }

        // Create document
        private async static Task<Document> CreateDocument(DocumentClient client, object documentObject)
        {

            var result = await client.CreateDocumentAsync(collection.SelfLink, documentObject);
            var document = result.Resource;

            Console.WriteLine("Created new document: {0}\r\n{1}", document.Id, document);
            return result;
        }

        // Querying a document
        private async static Task QueryDocumentsWithPaging(DocumentClient client)
        {
            Console.WriteLine();
            Console.WriteLine("**** Query Documents (paged results) ****");
            Console.WriteLine();
            Console.WriteLine("Querying documents");

            // select * from c where c.id = "AndersenFamily" would return the document that has this specific id
            var sql = "SELECT * FROM c where c.name = 'Hi Everyone'";
            var query = client.CreateDocumentQuery(collection.SelfLink, sql).AsDocumentQuery();

            while (query.HasMoreResults)
            {
                var documents = await query.ExecuteNextAsync();

                foreach (var document in documents)
                {
                    Console.WriteLine(" Id: {0};", document.id);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Query has finished.");
        }

        public async static Task ReadFromDocument(DocumentClient client)
        {
            Console.WriteLine();
            Console.WriteLine(">>> Reading from document with id of "  + " <<<");
            Console.WriteLine();

            // Inserting documentId parameter here does not seem to work 
            // var sql = "SELECT * from c where c.id = 'ParkerFamily'";
            var sql = "Select * from c where c.name = 'Yulong'";
            var document = client.CreateDocumentQuery(collection.SelfLink, sql);
            Console.WriteLine("Document toString: " + document.ToString());
            String content = JsonConvert.SerializeObject(document);
            Console.WriteLine("Json content: " + content);
            Console.WriteLine();
            Console.WriteLine();

            dynamic results = JsonConvert.DeserializeObject<dynamic>(content);
            Console.WriteLine(results);
           
        }
    }
}
