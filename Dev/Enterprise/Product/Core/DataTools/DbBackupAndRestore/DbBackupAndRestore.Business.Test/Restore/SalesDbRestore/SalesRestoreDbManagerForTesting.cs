namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class SalesRestoreDbManagerForTesting : SalesRestoreDbManager
	{
		public string SearchBackupFile_Exposed(string searchDirectory)
		{
			return SearchBackupFile(searchDirectory);
		}
	}
}
