namespace Enterprise.ArchiveManager.Business
{
	public static class ArchiveManagerConstants
	{
		public static class Codes
		{
			public const string OPS = "OPS";
			public const string IPS = "IPS";
			public const string PDR = "PDR";
			public const string PAR = "PAR";
			public const string STA = "STA";
			public const string OFL = "OFL";
			public const string PDO = "PDO";
			public const string HAR = "HAR";
			public const string RED = "RED";
			public const string EST = "EST";
			public const string PAL = "PAL";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Literal string is safe to use in this context.")]
		public static class Names
		{
			public const string OPS = "Operational Jobs Archive System";
			public const string IPS = "Inactive Operational Jobs Archive System";
			public const string PDR = "Purge Documents and Records";
			public const string PAR = "Purge Archived Records";
			public const string STA = "Standalone Records Archive System";
			public const string OFL = "Offline Storage";
			public const string PDO = "Purge Documents of Operational Records";
			public const string HAR = "HVLV Archive System";
			public const string RED = "Purge Expired Rates";
			public const string EST = "Purge ediProd Specific Tables";
			public const string PAL = "Purge Activity Logs";
		}

		public static class MinimumDataRetentionRequirementYears
		{
			public const int Default = 7;
			public const int PDO = 10;
			public const int HAR = 1;
		}

		public const string ARCServiceTaskCode = "ARC";
		public const string ACLServiceTaskCode = "ACL";
	}
}
