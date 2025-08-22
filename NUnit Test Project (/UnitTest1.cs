using System.Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit_Test_Project__.DTOs;
using RestSharp;
using RestSharp.Authenticators;

namespace RevueCraftersApiTests
{
    [TestFixture]
    public class Tests
    {
        protected RestClient client;
        protected static string baseUrl = "https://d2925tksfvgq8c.cloudfront.net/api";
        protected static string token;
        protected static string lastRevueId;

        [SetUp]
        public void Setup()
        {
            // First login request (no authenticator yet)
            var loginClient = new RestClient(baseUrl);

            var request = new RestRequest("/User/Authentication", Method.Post);
            request.AddJsonBody(new
            {
                email = "user@example.com",   
                password = "string"
            });

            var response = loginClient.Execute<AuthDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Login failed");

            token = response.Data.AccessToken;

            // Now create a RestClient with JWT authenticator
            client = new RestClient(new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            });
        }

        [TearDown]
        public void TearDown()
        {
            client?.Dispose();
        }

        [Test, Order(1)]
        public void CreateRevue_Success()
        {
            var request = new RestRequest("/Revue/Create", Method.Post);
            var revue = new RevueDTO
            {
                Title = "Exam Test Revue",
                Description = "This is created during exam",
                Url = ""
            };
            request.AddJsonBody(revue);

            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data.Msg, Is.EqualTo("Successfully created!"));
        }

        [Test, Order(2)]
        public void GetAllRevues_ShouldReturnList()
        {
            var request = new RestRequest("/Revue/All", Method.Get);
            var response = client.Execute<List<Dictionary<string, object>>>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data.Count, Is.GreaterThan(0));

            // Save last created revueId
            lastRevueId = response.Data.Last()["id"].ToString();
        }

        [Test, Order(3)]
        public void EditRevue_Success()
        {
            var request = new RestRequest($"/Revue/Edit?revueId={lastRevueId}", Method.Put);
            var revue = new RevueDTO
            {
                Title = "Edited Title",
                Description = "Edited description",
                Url = ""
            };
            request.AddJsonBody(revue);

            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data.Msg, Is.EqualTo("Edited successfully"));
        }

        [Test, Order(4)]
        public void DeleteRevue_Success()
        {
            var request = new RestRequest($"/Revue/Delete?revueId={lastRevueId}", Method.Delete);
            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data.Msg, Is.EqualTo("The revue is deleted!"));
        }

        [Test, Order(5)]
        public void CreateRevue_MissingFields_ShouldFail()
        {
            var request = new RestRequest("/Revue/Create", Method.Post);
            var revue = new { Title = "", Description = "" };
            request.AddJsonBody(revue);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingRevue_ShouldFail()
        {
            var request = new RestRequest("/Revue/Edit?revueId=00000000-0000-0000-0000-000000000000", Method.Put);
            var revue = new RevueDTO
            {
                Title = "Non-existing",
                Description = "Invalid",
                Url = ""
            };
            request.AddJsonBody(revue);

            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Data.Msg, Is.EqualTo("There is no such revue!"));
        }

        [Test, Order(7)]
        public void DeleteNonExistingRevue_ShouldFail()
        {
            var request = new RestRequest("/Revue/Delete?revueId=00000000-0000-0000-0000-000000000000", Method.Delete);
            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Data.Msg, Is.EqualTo("There is no such revue!"));
        }
    }
}

