using Enterprise.ZArchitecture.Core;

namespace Enterprise.ComplianceRisk.Integration
{
	public class ComplianceRiskStatusCodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Clear = "CLR";
			public const string PotentialRisk = "PSK";
			public const string OverrideClear = "OVR";
			public const string Incomplete = "INC";
			public const string Unknown = "UNK";
			public const string PossibleRisk = "PRS";
			public const string NotAssessed = "NAS";

			/// <summary>
			/// Internal status for a domestic job and/or a specific job type does not require a risk factor to contribute to the job compliance status.
			/// </summary>
			public const string NotApplicable = "NAP";
			public const string Blocked = "BLK";
			public const string Released = "REL";
			public const string NotChecked = "NCH";

			public const string Held = "HLD";
			public const string HighRisk = "HSK";
		}

		public static class Descriptions
		{
			public static MultilingualString Clear => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|Clear", "Clear");
			public static MultilingualString Held => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|Held", "Held");
			public static MultilingualString HighRisk => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|HighRisk", "High Risk");
			public static MultilingualString PossibleRisk => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|PossibleRisk", "Possible Risk");
			public static MultilingualString Unknown => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|Unknown", "Unknown");
			public static MultilingualString PotentialRisk => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|PotentialRisk", "Potential Risk");
			public static MultilingualString OverrideClear => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|Override Clear", "Override Clear");
			public static MultilingualString Incomplete => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|Incomplete", "Incomplete");
			public static MultilingualString NotApplicable => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|Not Applicable", "Not Applicable");
			public static MultilingualString Blocked => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|Blocked", "Blocked");
			public static MultilingualString Released => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|Released", "Released");
			public static MultilingualString NotChecked => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|Not Checked", "Not Checked");
			public static MultilingualString AssessmentNotInitialized => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|NotInitiateAssessment", "** INITIATE ASSESSMENT TO VIEW **");
			public static MultilingualString NotAssessed => ResString.GetMultilingualString("ComplianceRiskStatusCodeList|Not Assessed", "Not Assessed");
		}

		public ComplianceRiskStatusCodeList()
		{
			AddPair(Codes.Clear, Descriptions.Clear);
			AddPair(Codes.NotApplicable, Descriptions.NotApplicable);
			AddPair(Codes.OverrideClear, Descriptions.OverrideClear);
			AddPair(Codes.PossibleRisk, Descriptions.PossibleRisk);
			AddPair(Codes.PotentialRisk, Descriptions.PotentialRisk);
			AddPair(Codes.Unknown, Descriptions.Unknown);
			AddPair(Codes.Incomplete, Descriptions.Incomplete);
			AddPair(Codes.Held, Descriptions.Held);
			AddPair(Codes.HighRisk, Descriptions.HighRisk);
			AddPair(Codes.Blocked, Descriptions.Blocked);
			AddPair(Codes.NotAssessed, Descriptions.NotAssessed);
		}

		public static CodeDescriptionPairList GetOverallRiskStatusList()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(Codes.Clear, Descriptions.Clear);
			result.AddPair(Codes.PotentialRisk, Descriptions.PotentialRisk);
			result.AddPair(Codes.OverrideClear, Descriptions.OverrideClear);
			result.AddPair(Codes.Held, Descriptions.Held);
			result.AddPair(Codes.Blocked, Descriptions.Blocked);

			return result;
		}

		public static CodeDescriptionPairList GetPartyRiskStatusList()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(Codes.Clear, Descriptions.Clear);
			result.AddPair(Codes.PotentialRisk, Descriptions.PotentialRisk);
			result.AddPair(Codes.HighRisk, Descriptions.HighRisk);
			result.AddPair(Codes.Blocked, Descriptions.Blocked);

			return result;
		}

		public static CodeDescriptionPairList GetLocationRiskStatusList()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(Codes.Clear, Descriptions.Clear);
			result.AddPair(Codes.PotentialRisk, Descriptions.PotentialRisk);
			result.AddPair(Codes.Blocked, Descriptions.Blocked);

			return result;
		}

		public static CodeDescriptionPairList GetComplianceCommodityRiskStatusList()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(Codes.Clear, Descriptions.Clear);
			result.AddPair(Codes.PotentialRisk, Descriptions.PotentialRisk);
			result.AddPair(Codes.Incomplete, Descriptions.Incomplete);
			result.AddPair(Codes.NotApplicable, Descriptions.NotApplicable);
			result.AddPair(Codes.PossibleRisk, Descriptions.PossibleRisk);
			result.AddPair(Codes.HighRisk, Descriptions.HighRisk);
			result.AddPair(Codes.Blocked, Descriptions.Blocked);
			result.AddPair(Codes.Unknown, Descriptions.Unknown);
			result.AddPair(Codes.NotAssessed, Descriptions.NotAssessed);

			return result;
		}

		public static CodeDescriptionPairList GetCommodityPotentialRiskStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.PotentialRisk, Descriptions.PotentialRisk);
			result.AddPair(Codes.Blocked, Descriptions.Blocked);
			result.AddPair(Codes.Released, Descriptions.Released);
			return result;
		}

		public static CodeDescriptionPairList GetCommodityClearRiskStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.Clear, Descriptions.Clear);
			result.AddPair(Codes.Blocked, Descriptions.Blocked);
			return result;
		}

		public static CodeDescriptionPairList GetCommodityBlockedReleasedStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.Blocked, Descriptions.Blocked);
			result.AddPair(Codes.Released, Descriptions.Released);
			return result;
		}

		public static CodeDescriptionPairList GetCommodityNotCheckedStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.NotChecked, Descriptions.NotChecked);
			result.AddPair(Codes.Blocked, Descriptions.Blocked);
			result.AddPair(Codes.Released, Descriptions.Released);
			return result;
		}

		public static CodeDescriptionPairList GetCommodityPossibleRiskStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.PossibleRisk, Descriptions.PossibleRisk);
			result.AddPair(Codes.Blocked, Descriptions.Blocked);
			result.AddPair(Codes.Released, Descriptions.Released);
			return result;
		}

		public static CodeDescriptionPairList GetCommodityHighRiskStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.HighRisk, Descriptions.HighRisk);
			result.AddPair(Codes.Blocked, Descriptions.Blocked);
			result.AddPair(Codes.Released, Descriptions.Released);
			return result;
		}

		public static CodeDescriptionPairList GetCommodityAllStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.NotChecked, Descriptions.NotChecked);
			result.AddPair(Codes.Clear, Descriptions.Clear);
			result.AddPair(Codes.PotentialRisk, Descriptions.PotentialRisk);
			result.AddPair(Codes.Blocked, Descriptions.Blocked);
			result.AddPair(Codes.Released, Descriptions.Released);
			result.AddPair(Codes.PossibleRisk, Descriptions.PossibleRisk);
			result.AddPair(Codes.HighRisk, Descriptions.HighRisk);
			result.AddPair(Codes.NotAssessed, Descriptions.NotAssessed);

			return result;
		}

		public static CodeDescriptionPairList GetCommodityNomenclatureRiskStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.PossibleRisk, Descriptions.PossibleRisk);
			result.AddPair(Codes.HighRisk, Descriptions.HighRisk);

			return result;
		}

		public static CodeDescriptionPairList GetImportAlertForExportJobStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.Clear, Descriptions.Clear);
			result.AddPair(Codes.PossibleRisk, Descriptions.PossibleRisk);
			result.AddPair(Codes.HighRisk, Descriptions.HighRisk);

			return result;
		}
	}
}
