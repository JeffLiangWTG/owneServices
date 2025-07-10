namespace Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts
{
	public class ClearStmUsageDataScript : IClearDataScript
	{
		public void BuildScript(ClearDataScriptBuilder builder)
		{
			// CLEAR Billing Transactions(StmUsageData)
			builder.DeleteRecords().From("StmUsageData");
		}
	}
}
