namespace Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts
{
	public class CancelAnyQueuedMessages : IClearDataScript
	{
		public void BuildScript(ClearDataScriptBuilder builder)
		{
			// CLEAR QUEUED MESSAGES
			builder.UpdateRecords().From("EdiMessage")
				.Set("EM_Status", "'CAN'")
				.Where("EM_Status in ('PND', 'QUE', 'HPN', 'HQU', 'HPU', 'AQU')");

			// CLEAR QUEUED INTERCHANGES
			builder.UpdateRecords().From("EdiInterchange")
				.Set("EI_Status", "'CAN'")
				.Where("EI_Status in ('PND', 'QUE', 'HPN', 'HQU', 'HPU', 'AQU')");
		}
	}
}
