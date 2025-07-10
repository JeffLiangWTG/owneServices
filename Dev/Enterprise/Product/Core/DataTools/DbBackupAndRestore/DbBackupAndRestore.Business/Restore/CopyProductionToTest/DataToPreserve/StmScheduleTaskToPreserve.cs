namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class StmScheduleTaskToPreserve : PreserveTestValueScripts
	{
		#region PreserveTestValueScripts Members

		protected override string GetDataToPreserveFilter(string targetDbName)
		{
			return "WHERE S5_ParentTableCode = 'SH'";
		}

		protected override string GetPasteTempDataFilter(string targetDbName)
		{
			return string.Format("WHERE S5_GB IN (SELECT GB_PK FROM [{0}]..GlbBranch)", targetDbName);
		}

		protected override string TargetTableName => "StmScheduleTask";

		#endregion
	}
}
