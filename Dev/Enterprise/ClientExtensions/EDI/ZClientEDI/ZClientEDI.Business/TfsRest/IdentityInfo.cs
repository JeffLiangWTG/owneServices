using System.Collections.Generic;
using Newtonsoft.Json;

namespace Enterprise.Client.EDI.TfsRest
{
	public class IdentityInfo
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("providerDisplayName")]
		public string ProviderDisplayName { get; set; }

		[JsonProperty]
		public Dictionary<string, IdentityPropertyValue> Properties { get; set; }
	}
}
