namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class StmServiceHostToPreserveForTesting : StmServiceHostToPreserve
	{
		public string MainTableName_Exposed
		{
			get { return TargetTableName; }
		}
	}
}
