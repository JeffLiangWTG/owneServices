namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class StmScheduleTaskToPreserveForTesting : StmScheduleTaskToPreserve
	{
		public string MainTableName_Exposed => TargetTableName;
	}
}
