using Newtonsoft.Json;

namespace Enterprise.Client.EDI.Billing.Business.USSalesTax
{
	/// <summary>
	/// Response object model for non-error messages.
	/// </summary>
	/// <remarks>
	/// https://github.com/avadev/AvaTax-REST-V2-DotNet-SDK/blob/master/src/models/Message.cs
	/// https://developer.avalara.com/api-reference/avatax/rest/v2/models/Message/
	/// </remarks>
	public sealed class AvalaraV2MessageModel
	{
		[JsonProperty(PropertyName = "details")]
		public string Details { get; set; }

		[JsonProperty(PropertyName = "helpLink")]
		public string HelpLink { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "refersTo")]
		public string RefersTo { get; set; }

		[JsonProperty(PropertyName = "severity")]
		public string Severity { get; set; }

		[JsonProperty(PropertyName = "source")]
		public string Source { get; set; }

		[JsonProperty(PropertyName = "summary")]
		public string Summary { get; set; }
	}
}
