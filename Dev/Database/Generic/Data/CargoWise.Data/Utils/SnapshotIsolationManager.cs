using CargoWise.Common;

namespace CargoWise.Data
{
	public class SnapshotIsolationManager
	{
		public void EnableSnapshotIsolationForDatabase(DbConnection conn, string dbName, bool checkAlreadyEnabled = false)
		{
			Argument.NotNull(conn, nameof(conn));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			if (!checkAlreadyEnabled || !conn.IsDbUsingSnapshotIsolation(dbName))
			{
				SetSnapshotIsolationForDatabase(conn, dbName, enable: true);
			}
		}

		/// <summary>
		/// The "WITH ROLLBACK IMMEDIATE" clause eliminates the need to kill other connections in a separate thread
		/// </summary>
		protected void SetSnapshotIsolationForDatabase(DbConnection conn, string dbName, bool enable)
		{
			Argument.NotNull(conn, nameof(conn)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			const string sqlTemplate = @"
					ALTER DATABASE [{0}]
						SET ALLOW_SNAPSHOT_ISOLATION {1};
					ALTER DATABASE [{0}]
						SET READ_COMMITTED_SNAPSHOT {1} WITH ROLLBACK IMMEDIATE;";

			var sql = string.Format(sqlTemplate, dbName, enable ? "ON" : "OFF");

			conn.ExecuteNonQueryWithRetry(sql);

			DbConnection.RemoveCacheEntryFromSnapshotEnabledDictionary(dbName);
		}
	}
}
