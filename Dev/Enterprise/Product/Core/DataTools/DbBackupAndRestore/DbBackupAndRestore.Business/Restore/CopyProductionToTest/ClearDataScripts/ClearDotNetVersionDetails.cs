namespace Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts
{
	public class ClearDotNetVersionDetails : IClearDataScript
	{
		public void BuildScript(ClearDataScriptBuilder builder)
		{
			// CLEAR DotNetVersionDetails
			builder.DeleteRecords().From("StmData").Where("SD_Name like 'DOTNET[_]VERSION|%'");
		}
	}
}
