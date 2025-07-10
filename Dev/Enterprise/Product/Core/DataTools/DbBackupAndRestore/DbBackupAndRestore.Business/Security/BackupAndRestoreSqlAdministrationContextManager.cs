using System;
using System.Data;
using CargoWise.DataProtection.Administration.SqlServer;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Security
{
	public class BackupAndRestoreSqlAdministrationContextManager : IProtectedDataAdministrationSqlExecutionContextManager
	{
		IDbConnection connection;
		string serverName;

		public ISqlExecutionContext GetSqlExecutionContext(string serverName, string databaseName)
		{
			if (this.serverName != serverName)
			{
				throw new InvalidOperationException($"{nameof(BackupAndRestoreSqlAdministrationContextManager)} does not support multiple connections.");
			}
			return new SqlExecutionContext(connection, null);
		}

		public void UseConnection(IDbConnection connection , string serverName)
		{
			this.serverName = serverName;
			this.connection = connection;
		}
		public void ReleaseConnection()
		{
			serverName = null;
			connection = null;
		}
	}
}
