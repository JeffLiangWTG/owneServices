using System.Collections.Generic;
using Newtonsoft.Json;

namespace Enterprise.Client.EDI.Billing.Business.USSalesTax
{
	/// <summary>
	/// Response object model for errors.
	/// </summary>
	/// <remarks>
	/// https://github.com/avadev/AvaTax-REST-V2-DotNet-SDK/blob/master/src/models/ErrorResult.cs
	/// https://developer.avalara.com/api-reference/avatax/rest/v2/models/ErrorDetail/
	/// </remarks>
	public sealed class AvalaraV2ErrorRootModel
	{
		[JsonProperty(PropertyName = "error")]
		public AvalaraV2ErrorInfoModel Error { get; set; }
	}

	public sealed class AvalaraV2ErrorInfoModel
	{
		[JsonProperty(PropertyName = "code")]
		public string Code { get; set; }

		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "target")]
		public string Target { get; set; }

		[JsonProperty(PropertyName = "details")]
		public List<AvalaraV2ErrorDetailModel> Details { get; set; } = new List<AvalaraV2ErrorDetailModel>();
	}

	public sealed class AvalaraV2ErrorDetailModel
	{
		[JsonProperty(PropertyName = "code")]
		public string Code { get; set; }

		[JsonProperty(PropertyName = "number")]
		public int? Number { get; set; }

		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "faultCode")]
		public string FaultCode { get; set; }

		[JsonProperty(PropertyName = "faultSubCode")]
		public string FaultSubCode { get; set; }

		[JsonProperty(PropertyName = "helpLink")]
		public string HelpLink { get; set; }

		[JsonProperty(PropertyName = "refersTo")]
		public string RefersTo { get; set; }

		[JsonProperty(PropertyName = "severity")]
		public string Severity { get; set; }
	}
}
