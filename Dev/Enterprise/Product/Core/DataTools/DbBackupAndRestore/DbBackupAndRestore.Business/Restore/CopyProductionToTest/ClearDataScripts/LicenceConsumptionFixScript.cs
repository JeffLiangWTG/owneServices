namespace Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts
{
	public class LicenceConsumptionFixScript : IClearDataScript
	{
		public void BuildScript(ClearDataScriptBuilder builder)
		{
			///  Adjust Licence Consumption report settings to force all existing usage
			///  to be treated as already reported so it won't get reported twice.
			builder.UpdateRecords().From("StmData")
				.Set("SD_BinaryValue", "cast(convert(nvarchar, GetUTCDate(), 121) as varbinary(50))")
				.Where("SD_Name = 'WarehouseCountPackages'");
		}
	}
}
