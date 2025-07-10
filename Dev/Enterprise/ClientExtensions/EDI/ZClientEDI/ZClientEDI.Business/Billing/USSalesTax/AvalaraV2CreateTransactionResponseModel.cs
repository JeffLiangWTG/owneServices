using System.Collections.Generic;
using Newtonsoft.Json;

namespace Enterprise.Client.EDI.Billing.Business.USSalesTax
{
	/// <summary>
	/// Response object model for a Transaction & Line items, based on requirements to calculate US Sales Tax for an AR / AP transaction.
	/// </summary>
	/// <remarks>
	/// https://developer.avalara.com/api-reference/avatax/rest/v2/methods/Transactions/CreateTransaction/
	///
	/// Note that only properties we need are contained in this class.
	/// </remarks>
	public sealed class AvalaraV2CreateTransactionResponseModel
	{
		[JsonProperty(PropertyName = "totalTax", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? TotalTax { get; set; }

		[JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "messages", NullValueHandling = NullValueHandling.Ignore)]
		public List<AvalaraV2MessageModel> Messages { get; set; } = new List<AvalaraV2MessageModel>();
	}
}
