namespace CargoWise.Common.Data
{
	public static class DatabaseConstants
	{
		public const int LogFileIntialSizeMb = 4096;
		public const int LogFileGrowthMb = 4096;
		public const string TriggerNameToBlockInsertUpdateDeleteForDocManager = "SD_Block_Insert_Update_Delete_StorageDocs";
		public const string ColumnNameForStatisticsXmlOn = "Microsoft SQL Server 2005 XML Showplan"; // No need Res.GetString
	}
}
