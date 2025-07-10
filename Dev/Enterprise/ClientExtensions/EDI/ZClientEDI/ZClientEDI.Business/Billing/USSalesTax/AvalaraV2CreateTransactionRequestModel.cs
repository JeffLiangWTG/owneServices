using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Enterprise.Client.EDI.Billing.Business.USSalesTax
{
	/// <summary>
	/// Request object model for a Transaction & Line items, based on requirements to calculate US Sales Tax for an AR / AP transaction.
	/// </summary>
	/// <remarks>
	/// https://developer.avalara.com/api-reference/avatax/rest/v2/methods/Transactions/CreateTransaction/
	/// </remarks>
	public sealed class AvalaraV2CreateTransactionRequestModel
	{
		[JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "companyCode", NullValueHandling = NullValueHandling.Ignore)]
		public string CompanyCode { get; set; }

		[JsonProperty(PropertyName = "date", NullValueHandling = NullValueHandling.Ignore)]
		public string Date { get; set; }

		[JsonProperty(PropertyName = "code", NullValueHandling = NullValueHandling.Ignore)]
		public string Code { get; set; }

		[JsonProperty(PropertyName = "customerCode", NullValueHandling = NullValueHandling.Ignore)]
		public string CustomerCode { get; set; }

		[JsonProperty(PropertyName = "purchaseOrderNo", NullValueHandling = NullValueHandling.Ignore)]
		public string PurchaseOrderNo { get; set; }

		[JsonProperty(PropertyName = "commit", NullValueHandling = NullValueHandling.Ignore)]
		public bool Commit { get; set; }

		[JsonProperty(PropertyName = "currencyCode", NullValueHandling = NullValueHandling.Ignore)]
		public string CurrencyCode { get; set; }

		[JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "addresses")]
		public Dictionary<AvalaraV2AddressType, AvalaraV2AddressLocationModel> Addresses { get; set; } = new Dictionary<AvalaraV2AddressType, AvalaraV2AddressLocationModel>();

		[JsonProperty(PropertyName = "taxOverride", NullValueHandling = NullValueHandling.Ignore)]
		public AvalaraV2TaxOverrideModel TaxOverride { get; set; }

		[JsonProperty(PropertyName = "lines")]
		public List<AvalaraV2TransactionLineModel> Lines { get; set; } = new List<AvalaraV2TransactionLineModel>();
	}

	public sealed class AvalaraV2AddressLocationModel
	{
		[JsonProperty(PropertyName = "locationCode", NullValueHandling = NullValueHandling.Ignore)]
		public string LocationCode { get; set; }

		[JsonProperty(PropertyName = "line1", NullValueHandling = NullValueHandling.Ignore)]
		public string Line1 { get; set; }

		[JsonProperty(PropertyName = "line2", NullValueHandling = NullValueHandling.Ignore)]
		public string Line2 { get; set; }

		[JsonProperty(PropertyName = "line3", NullValueHandling = NullValueHandling.Ignore)]
		public string Line3 { get; set; }

		[JsonProperty(PropertyName = "city", NullValueHandling = NullValueHandling.Ignore)]
		public string City { get; set; }

		[JsonProperty(PropertyName = "region", NullValueHandling = NullValueHandling.Ignore)]
		public string Region { get; set; }

		[JsonProperty(PropertyName = "country", NullValueHandling = NullValueHandling.Ignore)]
		public string Country { get; set; }

		[JsonProperty(PropertyName = "postalCode", NullValueHandling = NullValueHandling.Ignore)]
		public string PostalCode { get; set; }

		[JsonProperty(PropertyName = "latitude", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? Latitude { get; set; }

		[JsonProperty(PropertyName = "longitude", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? Longitude { get; set; }
	}

	public enum AvalaraV2AddressType
	{
		singleLocation = 1,
		shipFrom,
		shipTo,
	}

	public sealed class AvalaraV2TaxOverrideModel
	{
		[JsonProperty(PropertyName = "type")]
		[JsonConverter(typeof(StringEnumConverter))]
		public AvalaraV2TaxOverrideType Type { get; set; }

		[JsonProperty(PropertyName = "taxDate", NullValueHandling = NullValueHandling.Ignore)]
		public string TaxDate { get; set; }

		[JsonProperty(PropertyName = "reason", NullValueHandling = NullValueHandling.Ignore)]
		public string Reason { get; set; }
	}

	public enum AvalaraV2TaxOverrideType
	{
		TaxDate = 1,
	}

	public sealed class AvalaraV2TransactionLineModel
	{
		[JsonProperty(PropertyName = "number")]
		public int Number { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public decimal Amount { get; set; }

		[JsonProperty(PropertyName = "itemCode", NullValueHandling = NullValueHandling.Ignore)]
		public string ItemCode { get; set; }

		[JsonProperty(PropertyName = "taxCode", NullValueHandling = NullValueHandling.Ignore)]
		public string TaxCode { get; set; }

		[JsonProperty(PropertyName = "ref1", NullValueHandling = NullValueHandling.Ignore)]
		public string Ref1 { get; set; }

		[JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
		public string Description { get; set; }
	}
}
