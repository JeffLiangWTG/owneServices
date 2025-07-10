using System;
using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class SessionInfoUserKeyEntryForTesting : SessionInfoUserKeyEntry
	{
		public SessionInfoUserKeyEntryForTesting(string dbServer, string databaseName, string latestLog, DateTime serverDateTime)
			: base(dbServer, databaseName, latestLog, serverDateTime)
		{
		}

		public string GetReleaseKey_Exposed(string sessionIdHash)
		{
			return this.GetReleaseKey(sessionIdHash);
		}

		public string GetSessionIdHashForDate_Exposed(DateTime sessionDate)
		{
			return this.GetSessionIdHashForDate(sessionDate);
		}

		public static string GetLatestLogKey_Exposed(DbConnection connection, string dbServer, string databaseName)
		{
			return GetLatestLogKey(connection, dbServer, databaseName);
		}
	}
}
