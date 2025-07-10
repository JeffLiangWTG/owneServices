namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class HelpErrorLogOccurrenceLookups : AutoHelpErrorLogOccurrenceLookups
	{
		public HelpErrorLogOccurrenceLookups(AutoHelpErrorLogOccurrence parent)
			: base(parent)
		{
		}

		#region Error Types

		public static class ErrorType
		{
			public const string Unspecified = "USP";
			public const string DatabaseUpgradeFailure = "DUF";
		}

		#endregion
	}
}

