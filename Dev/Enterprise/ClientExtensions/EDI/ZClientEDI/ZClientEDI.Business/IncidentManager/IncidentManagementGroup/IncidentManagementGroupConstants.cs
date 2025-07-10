using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public static class IncidentManagementGroupConstants
	{
		public static class ServiceOutageCodes
		{
			public const string Investigating = "INV";
			public const string Active = "ACT";
			public const string Downgraded = "DNG";
			public const string Restored = "RST";
		}

		public static class BusinessImpactCodes
		{
			public const string Emergency = "EMG";
			public const string HighImpact = "HIG";
			public const string Significant = "SIG";
			public const string Moderate = "MOD";
			public const string MinorLocalised = "MIN";
		}

		public static class UrgencyCodes
		{
			public const string Critical = "CRI";
			public const string VeryHigh = "VHI";
			public const string High = "HIG";
			public const string Medium = "MED";
			public const string Low = "LOW";
		}

		public static class AutoReplyDescriptions
		{
			public const string Manual = "MANUAL";
			public const string AutoReplyOnce = "AUTO-REPLY ONCE";
		}

		public static class StageChangeEventLogCodes
		{
			public const string NewStatus = "STA";
			public const string OldStatus = "PRE";
			public const string NewStageIncidentCompleted  = "ICM";
			public const string NewStageGroupCompleted = "GCM";
			public const string Detail = "DES";
		}

		public static CodeDescriptionPair WorkflowDescriptorInformation => new CodeDescriptionPair("ING", "Incident Management Group");
	}
}
