using Enterprise.Integration;
using Newtonsoft.Json;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public class ErrorResponse : IErrorResponse
	{
		[JsonProperty("error")]
		public string Error { get; set; }
		[JsonProperty("error_description")]
		public string ErrorDescription { get; set; }
		[JsonProperty("error_uri")]
		public string ErrorURI { get; set; }
	}
}
