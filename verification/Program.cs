using System;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace csharp
{

	class MainClass
	{
		const string AUTH_HOST = "http://auth.staging.govx.com";
		const string API_HOST = "http://api.staging.govx.com";
		const string CLIENT_ID = "YOUR_CLIENT_ID_REQUIRED";
		const string CLIENT_SECRET = "YOUR_CLIENT_SECRET_REQUIRED";

		/// <summary>
		/// Gets an access token from GovX Authorization server.
		/// </summary>
		/// <returns>The access token.</returns>
		public static string FetchAccessToken() {
			var client = new RestClient(AUTH_HOST);
			var request = new RestRequest("api/oauth/token", Method.POST);
			request.AddObject(new {
				client_id = CLIENT_ID,
				client_secret = CLIENT_SECRET,
				grant_type = "client_credentials",
				scope = "rwx:verification"
			});

			// execute the request
			IRestResponse response = client.Execute(request);
			var body = response.Content; // raw content as string

			JObject res = JObject.Parse(body);
			return res["access_token"].ToString();
		}

		/// <summary>
		/// Submits a verification request.
		/// </summary>
		public static JObject SubmitVerificationRequest(string accessToken, object verification) {
			var client = new RestClient(API_HOST);
			var request = new RestRequest("api/verifications", Method.POST);

			request.RequestFormat = DataFormat.Json;
			request.AddHeader("Content-Type", "application/json");
			request.AddHeader("Authorization", "Bearer " + accessToken);
			request.AddBody(verification);

			// execute the request
			IRestResponse response = client.Execute(request);
			JObject json = JObject.Parse(response.Content);
			return json;
		}

		/// <summary>
		/// Polls for status.
		/// </summary>
		/// <returns>The status.</returns>
		/// <param name="id">The verification ID.</param>
		public static JObject PollForStatus(int id) {
			var accessToken = FetchAccessToken();
			var client = new RestClient(API_HOST);
			var request = new RestRequest("api/verifications/{id}", Method.GET);

			request.AddUrlSegment("id", id.ToString());
			request.AddHeader("Content-Type", "application/json");
			request.AddHeader("Authorization", "Bearer " + accessToken);

			// execute the request
			IRestResponse response = client.Execute(request);
			JObject json = JObject.Parse(response.Content);
			return json;
		}


		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main(string[] args)
		{
			var accessToken = FetchAccessToken();
			Console.WriteLine("Access Token: {0}", accessToken);

			var verification = new {
				firstName = "Mario",
				lastName = "Gutierrez",
				dob = "11/9/1966",
				reference = "order_no=1234",
				email = "mario@govx.com",
				joinGovX = false,
				affiliation = new {
					name = "MILITARY",
					branch = "US NAVY",
					fromDate = "11/1/1988"
				}
			};

			var json = SubmitVerificationRequest(accessToken, verification);
			Console.WriteLine("Request reply: {0}", json);

			var id = int.Parse(json["id"].ToString());

			System.Threading.Thread.Sleep(5000);
			var status = PollForStatus(id);
			Console.WriteLine("Status reply: {0}", status);
		}
	}
}
