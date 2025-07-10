namespace Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts
{
	public class ClearPrintAndMailJobsScript : IClearDataScript
	{
		public void BuildScript(ClearDataScriptBuilder builder)
		{
			// CLEAR PRINT AND MAIL JOBS
			builder.DeleteRecords().From("StmPrintJobCopyRecipient");
			builder.DeleteRecords().From("StmPrintJob");
			builder.UpdateRecords().From("MailDBItems").Set("MI_Status", "'FAL'").Where("MI_Status in ('QUE', 'QWA')");
		}
	}
}
