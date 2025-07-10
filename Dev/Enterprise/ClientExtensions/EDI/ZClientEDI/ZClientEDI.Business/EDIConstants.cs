namespace Enterprise.Client.EDI
{
	public static class EDIConstants
	{
		public static class OrgPatternMatchOverrideRelationships
		{
			public const string EHubClientID = "EHC";
		}

		public static class DatabaseConnectionStatus
		{
			public const string Ok = "INFO(Database): OK";
			public const string Error = "ERROR(Database): Cannot access database";
			public const string VersionMismatch = "ERROR(Database): Database version mismatch";
		}

		public enum BusinessContext
		{
			VersionReportContactImporter,
			ImportBuildsServiceTask
		}

		public const string ClientDisplayName = "WiseTech Global";
	}
}
