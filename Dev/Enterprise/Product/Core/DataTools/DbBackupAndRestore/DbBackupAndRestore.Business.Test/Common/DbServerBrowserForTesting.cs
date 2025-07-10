using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class DbServerBrowserForTesting : DbServerBrowser
	{
		public DbServerBrowserForTesting(bool isFolderOnly)
			: base(Db.ServerName, isFolderOnly)
		{
		}
	}
}
