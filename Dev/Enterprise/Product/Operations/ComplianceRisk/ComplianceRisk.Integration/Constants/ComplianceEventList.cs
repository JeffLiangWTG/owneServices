using Enterprise.ZArchitecture.Core;

namespace Enterprise.ComplianceRisk.Integration
{
	public class ComplianceEventList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string PartyRisk = "PTY";
			public const string LocationRisk = "LOC";
			public const string CommodityRisk = "CMD";
			public const string AssessmentRisk = "ASM";
			public const string OverallRisk = "OVL";
			public const string Override = "OVR";
			public const string AssessmentInitialized = "CAI";
			public const string AssessmentDeclined = "CAD";
			public const string LegalBooksViewed = "LBV";
			public const string ComplianceDecisionChanged = "CDC";
			public const string CommodityLineDeleted = "CLD";
			public const string ComplianceWiseCommoditiesValueObtained = "CPC";
			public const string ComplianceWiseValueObtained = "CVO";
			public const string AssessmentDecisionRequired = "REQ";
		}

		public static class EventType
		{
			public const string BorderWiseIntegration = "BWI";
			public const string ComplianceCommodityInteraction = "CCI";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static class Descriptions
		{
			public const string PartyRisk = "Party Compliance Risk status";
			public const string LocationRisk = "Location Compliance Risk status";
			public const string CommodityRisk = "Commodities Compliance Risk status";
			public const string AssessmentRisk = "Assessment Compliance Risk status";
			public const string OverallRisk = "Job Compliance status";
			public const string AssessmentInitialized = "Compliance Assessment Initialized";
			public const string AssessmentDeclined = "Compliance Assessment Declined";
			public const string DocumentHoldStatusOverriden = "Document Hold Status Overridden";
			public const string AssessmentDecisionRequired = "Compliance Assessment Decision Required by User";
		}

		public static class InitializedByType
		{
			public const string User = "USR";
			public const string Rule = "RUL";
		}
	}
}
