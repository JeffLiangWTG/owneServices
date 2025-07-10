namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public delegate void ConfirmationPromptDelegate(ConfirmationPromptArgs promptArgs);

	public class ConfirmationPromptArgs
	{
		public string PromptTitle { get; internal set; }
		public string PromptMessage { get; internal set; }
		public PromptType PromptType { get; internal set; }
		public ConfirmationPromptResult Result { get; set; }
	}

	public enum ConfirmationPromptResult : ushort
	{
		Default = 0,
		Yes = 1,
		No = 2
	}
	public enum PromptType
	{
		ConfirmAuditDBExcluded,
		ConfirmNoBackupOfTestDb,
		ConfirmRestoreWithoutOperationalDatabases,
		ConfirmOverwriteDbInAvailabilityGroup,
		ConfirmRemoveDbFromAvailabilityGroup
	}
}
