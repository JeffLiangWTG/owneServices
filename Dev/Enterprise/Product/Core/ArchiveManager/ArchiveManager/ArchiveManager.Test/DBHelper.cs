using Enterprise.DocumentScanning.Business;

namespace Enterprise.ArchiveManager.Test
{
	public class DBHelper : DocManagerDBHelper
	{
		public int RecreateDatabase(int dbNumber)
		{
			DropDatabase(dbNumber);
			return CreateDatabase(dbNumber);
		}

		public void DropDatabase(int dbNumber)
		{
			if (DatabaseExists(dbNumber))
			{
				var dbName = GetDatabaseName(dbNumber);
				DropDatabase(dbName);
			}
		}
	}
}
