namespace Enterprise.ComplianceRisk.Business
{
	public partial class CommodityLevelModel
	{
		[Newtonsoft.Json.JsonProperty("import", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<string> Import { get; set; }

		[Newtonsoft.Json.JsonProperty("export", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<string> Export { get; set; }

		[Newtonsoft.Json.JsonProperty("originOfGoods", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<string> OriginOfGoods { get; set; }
	}

	public partial class LocationLevelModel
	{
		[Newtonsoft.Json.JsonProperty("transshipment", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<string> Transshipment { get; set; }

		[Newtonsoft.Json.JsonProperty("location", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public System.Collections.Generic.ICollection<string> Location { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v11.0.0.0))")]
	public partial class SupportedCountriesCheckResponseModel
	{
		[Newtonsoft.Json.JsonProperty("commodityLevel", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public CommodityLevelModel CommodityLevel { get; set; }

		[Newtonsoft.Json.JsonProperty("locationLevel", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public LocationLevelModel LocationLevel { get; set; }

	}
}
