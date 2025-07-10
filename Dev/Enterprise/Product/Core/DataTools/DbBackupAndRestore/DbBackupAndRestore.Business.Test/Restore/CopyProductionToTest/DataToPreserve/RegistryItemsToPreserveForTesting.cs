namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class RegistryItemsToPreserveForTesting : RegistryItemsToPreserve
	{
		public string MainTableName_Exposed
		{
			get { return TargetTableName; }
		}
	}
}
