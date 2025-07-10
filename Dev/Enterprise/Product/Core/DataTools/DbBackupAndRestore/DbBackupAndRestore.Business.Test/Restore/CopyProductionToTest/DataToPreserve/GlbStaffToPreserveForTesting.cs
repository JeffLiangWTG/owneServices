namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class GlbStaffToPreserveForTesting : GlbStaffToPreserve
	{
		public string MainTableName_Exposed
		{
			get { return TargetTableName; }
		}
	}
}
