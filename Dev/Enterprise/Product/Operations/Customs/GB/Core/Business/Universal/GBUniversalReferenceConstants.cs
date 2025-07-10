namespace Enterprise.Customs.GB.Business
{
	public static class GBUniversalReferenceConstants
	{
		public static class TaxOrFeeTypeCode
		{
			public const string ReducedRate = "650";
			public const string StandardRate = "666";
			public const string VATExempt = "654";
			public const string ZeroRated = "673";
		}

		public static class Preferences
		{
			public const string PreferenceCode_100_NormalTariffDuty = "100";
			public const string PreferenceCode_200_GSPRateWithoutConditionsOrLimitsIncludingCeilings = "200";
			public const string PreferenceCode_300_TariffPreferenceWithoutConditionsOrLimitsIncludingCeilings = "300";
			public const string PreferenceCode_400_NonImpositionOfCustomsDutiesUnderTheProvisionsOfCustomsUnionAgreementsConcludedByTheCommunity = "400";
			public const string PreferenceCode_420 = "420";
		}

		public static class ProcedureCodes
		{
			public static class Concession
			{
				public const string F48 = "F48";
			}
		}
	}
}
