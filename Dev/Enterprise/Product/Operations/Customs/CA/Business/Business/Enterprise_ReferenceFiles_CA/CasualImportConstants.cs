namespace Enterprise.Customs.CA.Business
{
	public static class CasualImportConstants
	{
		public static class CasualImpRatesProcessingType
		{
			public const string ProcessingTypeHSTCode = "H";
			public const string ProcessingTypePSTCode = "P";
			public const string ProcessingTypeTobaccoCode = "T";
			public const string ProcessingTypeAlcoholCode = "A";
		}

		public static class CasualImpRatesCommodityType
		{
			public const string CommodityTypeHSTCode = "HST";
			public const string CommodityTypePSTCode = "PST";
			public const string CommodityTypePSTACode = "PSTA";
			public const string CommodityTypePSTTCode = "PSTT";
		}

		public static class CasualImpCommodityType
		{
			public const string Tobacco = "T";
			public const string Alcohol = "A";
		}
	}
}
