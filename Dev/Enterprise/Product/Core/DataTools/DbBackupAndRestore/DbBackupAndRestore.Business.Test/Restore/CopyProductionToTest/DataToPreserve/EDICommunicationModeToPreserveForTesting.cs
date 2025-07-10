namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class EDICommunicationModeToPreserveForTesting : EDICommunicationModeToPreserve
	{
		public string MainTableName_Exposed
		{
			get { return TargetTableName; }
		}
	}
}
