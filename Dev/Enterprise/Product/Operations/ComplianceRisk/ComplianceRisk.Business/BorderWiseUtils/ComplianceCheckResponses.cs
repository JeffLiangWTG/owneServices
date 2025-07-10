using Newtonsoft.Json;

namespace Enterprise.ComplianceRisk.Business
{
	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class ComplianceCheckResponseCommodityModel
	{
		[JsonProperty("hsCode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string HsCode { get; set; }

		[JsonProperty("origin", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<string> Origin { get; set; }

		[JsonProperty("hsCodeDescription", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string HsCodeDescription { get; set; }

		[JsonProperty("goodsDescription", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string GoodsDescription { get; set; }

		[JsonProperty("nomenclatureWideConditionsApply", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string NomenclatureWideConditionsApply { get; set; }

		[JsonProperty("commoditySpecificConditionsApply", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string CommoditySpecificConditionsApply { get; set; }

		[JsonProperty("pointPairs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<ComplianceCheckResponsePointPairModel> PointPairs { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class ComplianceCheckResponseLocationModel
	{
		[Newtonsoft.Json.JsonProperty("country", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public string Country { get; set; }

		[Newtonsoft.Json.JsonProperty("involvement", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public string Involvement { get; set; }

		[Newtonsoft.Json.JsonProperty("isSupportedCountry", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public bool IsSupportedCountry { get; set; }
	}


	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class ComplianceCheckResponseModel
	{
		[JsonProperty("jobNumber", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string JobNumber { get; set; }

		[JsonProperty("requestId", Required = Required.Always)]
		public System.Guid RequestId { get; set; }

		[JsonProperty("nomenclatureWideConditionsApply", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string NomenclatureWideConditionsApply { get; set; }

		[JsonProperty("commoditySpecificConditionsApply", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string CommoditySpecificConditionsApply { get; set; }

		[JsonProperty("commodities", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<ComplianceCheckResponseCommodityModel> Commodities { get; set; }

		[Newtonsoft.Json.JsonProperty("locations", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<ComplianceCheckResponseLocationModel> Locations { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class ComplianceCheckResponsePointPairLocationModel
	{
		[JsonProperty("country", Required = Required.Default)]
		[System.ComponentModel.DataAnnotations.Required]
		public string Country { get; set; }

		[JsonProperty("unloco", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string Unloco { get; set; }

		[JsonProperty("movementType", Required = Required.Default)]
		[System.ComponentModel.DataAnnotations.Required]
		public string MovementType { get; set; }

		[JsonProperty("movementDescription", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string MovementDescription { get; set; }

		[JsonProperty("nomenclatureWideConditions", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<string> NomenclatureWideConditions { get; set; }

		[JsonProperty("commoditySpecificConditions", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<string> CommoditySpecificConditions { get; set; }

		[JsonProperty("complianceCodes", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<string> ComplianceCodes { get; set; }

		[JsonProperty("nomenclatureWideConditionsApply", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string NomenclatureWideConditionsApply { get; set; }

		[JsonProperty("commoditySpecificConditionsApply", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string CommoditySpecificConditionsApply { get; set; }

		[JsonProperty("isValidHsCode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public bool IsValidHsCode { get; set; }

		[JsonProperty("matchedHsCode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string MatchedHsCode { get; set; }

	}

	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class ComplianceCheckResponsePointPairModel
	{
		[JsonProperty("originPoint", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public ComplianceCheckResponsePointPairLocationModel OriginPoint { get; set; }

		[JsonProperty("destinationPoint", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public ComplianceCheckResponsePointPairLocationModel DestinationPoint { get; set; }

		[JsonProperty("mode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string Mode { get; set; }

		[JsonProperty("estimatedTimeOfDeparture", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public System.DateTimeOffset EstimatedTimeOfDeparture { get; set; }

		[JsonProperty("estimatedTimeOfArrival", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public System.DateTimeOffset EstimatedTimeOfArrival { get; set; }

		[JsonProperty("nomenclatureWideConditionsApply", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string NomenclatureWideConditionsApply { get; set; }

		[JsonProperty("commoditySpecificConditionsApply", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string CommoditySpecificConditionsApply { get; set; }
	}

	public class ErrorMessage
	{
		public string Message { get; set; }
	}
}
