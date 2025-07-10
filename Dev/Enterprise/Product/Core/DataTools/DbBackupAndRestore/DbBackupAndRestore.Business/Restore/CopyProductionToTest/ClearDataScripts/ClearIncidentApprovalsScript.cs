namespace Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts
{
	public class ClearIncidentApprovalsScript : IClearDataScript
	{
		public void BuildScript(ClearDataScriptBuilder builder)
		{
			// CLEAR IncidentApproval(eRequests)
			builder.DeleteRecords().From("IncidentApproval");
		}
	}
}
