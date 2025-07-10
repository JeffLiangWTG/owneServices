using Newtonsoft.Json;

namespace Enterprise.ComplianceRisk.Business
{
	public partial class ComplianceCheckRequestCommodityModel
	{
		[System.ComponentModel.DataAnnotations.Required]
		public string HsCode { get; set; }

		public System.Collections.Generic.ICollection<string> Origin { get; set; }

		[System.ComponentModel.DataAnnotations.Required]
		public string HsCodeDescription { get; set; }

		[System.ComponentModel.DataAnnotations.Required]
		public string GoodsDescription { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class ComplianceCheckRequestModel
	{
		public string JobNumber { get; set; }

		[System.ComponentModel.DataAnnotations.Required]
		public System.Collections.Generic.ICollection<ComplianceCheckRequestCommodityModel> Commodities { get; set; } = new System.Collections.ObjectModel.Collection<ComplianceCheckRequestCommodityModel>();

		[System.ComponentModel.DataAnnotations.Required]
		public System.Collections.Generic.ICollection<ComplianceCheckRequestPointPairModel> PointPairs { get; set; } = new System.Collections.ObjectModel.Collection<ComplianceCheckRequestPointPairModel>();

		[System.ComponentModel.DataAnnotations.Required]
		public ComplianceCheckRequestAuthenticationModel Authentication { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class ComplianceCheckRequestPointPairLocationModel
	{
		[System.ComponentModel.DataAnnotations.Required]
		public string Country { get; set; }

		public string Unloco { get; set; }

		[System.ComponentModel.DataAnnotations.Required]
		public string MovementType { get; set; }

		public string MovementDescription { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class ComplianceCheckRequestPointPairModel
	{
		[JsonProperty("originPoint", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public ComplianceCheckRequestPointPairLocationModel OriginPoint { get; set; }

		[JsonProperty("destinationPoint", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public ComplianceCheckRequestPointPairLocationModel DestinationPoint { get; set; }

		[JsonProperty("mode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
		public string Mode { get; set; }

		[JsonProperty("estimatedTimeOfDeparture", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public System.DateTimeOffset EstimatedTimeOfDeparture { get; set; }

		[JsonProperty("estimatedTimeOfArrival", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
		public System.DateTimeOffset EstimatedTimeOfArrival { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class ComplianceCheckRequestAuthenticationModel
	{
		public string OrgCode { get; set; }

		public string StaffCode { get; set; }

		public string DataBaseNumber { get; set; }
	}
}
