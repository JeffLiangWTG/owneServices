using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Registry
{
	public class CdsCredentialChecker
	{
		public ICspDownloadResult checkCdsCredentials(ICredentials credentials, string printer, string baseUrl, string userAgent)
		{
			var response = new CDSCredentialCheckerResponse();
			var client = GetAndConfigureWebClient(credentials, userAgent);
			string pathForTopic = string.Format(baseUrl + "notifications/{0}", printer);
			string pathWithConsumer = string.Format(pathForTopic + "/consumer");
			try
			{
				var result = client.DownloadString(pathWithConsumer);
				response = ParseGoodResponse(result);
			}
			catch (WebException ex)
			{
				var errorStatusCode = client.GetStatusCode(ex);
				switch (errorStatusCode)
				{
					case HttpStatusCode.NotFound:
					case HttpStatusCode.Forbidden:
						response = MakeSecondRequestWithoutConsumer(pathForTopic, credentials, userAgent);
						break;
					case HttpStatusCode.Unauthorized:
						response.errorText = errorStatusCode.ToString() + ";  The CSP has refused your username/password.";
						break;

					default:
						response.errorText = errorStatusCode.ToString();
						break;
				}
			}
			return response;
		}

		CDSCredentialCheckerResponse MakeSecondRequestWithoutConsumer(string pathWithConsumer, ICredentials credentials, string userAgent)
		{
			var client = GetAndConfigureWebClient(credentials, userAgent); // Need a new instance because otherwise the list of HTTP headers gets lost
			CDSCredentialCheckerResponse secondResponse = null;
			string secondResult = "";
			try
			{
				secondResult = client.DownloadString(pathWithConsumer);
				secondResponse = ParseGoodResponse(secondResult);
			}
			catch (WebException ex)
			{
				var errorStatusCode = client.GetStatusCode(ex);
				secondResponse = new CDSCredentialCheckerResponse();
				switch (errorStatusCode)
				{
					case HttpStatusCode.NotFound:
					case HttpStatusCode.Forbidden:
						secondResponse.errorText = errorStatusCode.ToString() + ";  This may mean that the topic was not recognised by the CSP.";
						break;

					default:
						secondResponse.errorText = errorStatusCode.ToString();
						break;
				}
			}
			return secondResponse;
		}

		static CDSCredentialCheckerResponse ParseGoodResponse(string response)
		{
			var cdsCredentialCheckerResponse = new CDSCredentialCheckerResponse();
			string returnedEndpointUrl;
			if (response.ToLower().Contains("<consumer"))
			{
				try
				{
					var doc = XDocument.Parse(response);
					returnedEndpointUrl = doc.XPathSelectElement("/Consumer/endpointUrl").Value; // CNS format response
				}
				catch (NullReferenceException)
				{
					try
					{
						var doc = XDocument.Parse(response);
						var eval = doc.XPathEvaluate("/consumer/@endpointUrl"); // MCP format response
						returnedEndpointUrl = ((IEnumerable)eval).OfType<XAttribute>().Single().Value;
					}
					catch (NullReferenceException)
					{
						returnedEndpointUrl = ZString.Empty;
					}
				}
				cdsCredentialCheckerResponse.MessagesArray = returnedEndpointUrl.IsNullOrEmpty() ? null : [returnedEndpointUrl];
			}
			else if (response.ToLower().Contains("<notifications "))
			{
				cdsCredentialCheckerResponse.MessagesArray = ["Notifications are awaiting topic registration."];
			}
			return cdsCredentialCheckerResponse;
		}

		protected virtual IWebClient GetAndConfigureWebClient(ICredentials credentials, string userAgent)
		{
			var credential = credentials.GetCredential(null, null);
			var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{credential.UserName}:{credential.Password}"));

#if NET8_0_OR_GREATER
			var httpClient = new System.Net.Http.HttpClient();
			httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);
			httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.csp.1.0+xml"));
			httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

			return new CdsWebClient(httpClient);
#else // net48
			var client = new CdsWebClient();
			client.Headers[HttpRequestHeader.Authorization] = "Basic " + auth;
			client.Headers.Add("Accept", "application/vnd.csp.1.0+xml");
			client.Headers.Add("User-Agent", userAgent);
			return client;
#endif
		}
	}

	public class CDSCredentialCheckerResponse : ICspDownloadResult
	{
		public string errorText { get; set; }

		public string[] MessagesArray { get; set; }

		public int batchId { get; set; }
	}
}
