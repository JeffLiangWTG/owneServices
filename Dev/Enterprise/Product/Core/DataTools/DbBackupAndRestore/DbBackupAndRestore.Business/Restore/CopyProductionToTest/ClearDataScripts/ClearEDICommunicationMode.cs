namespace Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts
{
	public class ClearEDICommunicationMode : IClearDataScript
	{
		public void BuildScript(ClearDataScriptBuilder builder)
		{
			// CLEAR live contact details from EDICommunicationsMode
			builder.UpdateRecords().From("EDICommunicationsMode").Set("EK_Destination", "''");
			builder.UpdateRecords().From("EDICommunicationsMode").Set("EK_LoginName", "''");
			builder.UpdateRecords().From("EDICommunicationsMode").Set("EK_Password", "''");
		}
	}
}
