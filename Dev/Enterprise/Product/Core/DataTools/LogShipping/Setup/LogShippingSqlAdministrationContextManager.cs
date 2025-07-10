using System;
using System.Data;
using CargoWise.Common;
using CargoWise.DataProtection.Administration.SqlServer;

namespace Enterprise.LogShipping.Setup
{
	public class LogShippingSqlAdministrationContextManager : ProtectedDataAdministrationSqlExecutionContextManagerBase, IDisposable
	{
		IDbConnection connection;
		string currentServerName;

		public LogShippingSqlAdministrationContextManager()
		{
		}

		static SqlConnection OpenNewIntegratedSecurityConnection(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

#pragma warning disable CW1065 // Encrypt SQL Connection Rule
			var builder = new SqlConnectionStringBuilder();
#pragma warning restore CW1065 // Encrypt SQL Connection Rule
			DbManager.ConfigureConnectionString(builder, serverName);
			builder.IntegratedSecurity = true;
#pragma warning disable CW1116 // The existing CargoWise.Data.Db.Connection should be used rather than creating a new one
			var connection = new SqlConnection(builder.ConnectionString);
#pragma warning restore CW1116
			connection.Open();
			return connection;
		}

		protected override ISqlExecutionContext CreateProtectedDataAdministrationSqlContext(string serverName, string databaseName)
		{
			if (currentServerName == serverName)
			{
				return new SqlExecutionContext(connection, null);
			}
			if (string.IsNullOrEmpty(currentServerName))
			{
				connection = OpenNewIntegratedSecurityConnection(serverName);
				var context = new SqlExecutionContext(connection, null);

				if ((int)context.ExecuteScalar("SELECT IS_SRVROLEMEMBER('sysadmin')", CommandType.Text, null) == 1)
				{
					currentServerName = serverName;
				}
				else
				{
					CloseConnection();
					throw new InvalidOperationException("Current Windows user is not system administrator.");
				}

				return context;
			}
			else
			{
				throw new InvalidOperationException($"{nameof(LogShippingSqlAdministrationContextManager)} only supports connection to a single server at each time.");
			}
		}
		public void CloseConnection()
		{
			currentServerName = string.Empty;
			connection?.Dispose();
			connection = null;
		}

		public void Dispose()
		{
			CloseConnection();
		}
	}
}
