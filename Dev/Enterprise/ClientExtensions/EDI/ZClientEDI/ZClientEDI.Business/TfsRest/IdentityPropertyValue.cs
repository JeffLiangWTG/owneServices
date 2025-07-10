using Newtonsoft.Json;

namespace Enterprise.Client.EDI.TfsRest
{
	public class IdentityPropertyValue
	{
		[JsonProperty("$type")]
		public string Type { get; set; }

		[JsonProperty("$value")]
		public string Value { get; set; }
	}
}
