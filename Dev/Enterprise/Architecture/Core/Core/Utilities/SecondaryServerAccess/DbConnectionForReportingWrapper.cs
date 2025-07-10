using System.Linq;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Core
{
	public sealed class DbConnectionForReportingWrapper : IDbConnectionForReportingWrapper
	{
		public DbConnectionForReportingWrapper(DbConnection connection, bool shouldDisposeConnection = true)
		{
			this.connection = connection;
			this.shouldDisposeConnection = shouldDisposeConnection;
		}

		readonly DbConnection connection;
		readonly bool shouldDisposeConnection;

		public DbConnection Connection
		{
			get { return connection; }
		}

		public bool IsMainServer
		{
			get { return connection.ServerName == Db.ServerName; }
		}

		public DbConnectionForReportingWrapper ImpersonateDbUser(string dbUsername)
		{
			if (!string.IsNullOrWhiteSpace(dbUsername))
			{
				var sqlExecuteAsUser = $"EXECUTE AS USER = '{dbUsername}' WITH NO REVERT"; // sql query

				try
				{
					connection.ExecuteNonQuery(sqlExecuteAsUser);
					connection.ImpersonatedLogin = dbUsername;
				}
				catch (System.Data.Common.DbException ex)
				{
					var errorHandler = new DbErrorHandler(ex, connection);

					if (errorHandler.ExceptionType == DbErrorType.CannotExecuteAsDatabasePrincipal)
					{
						using (var adminConnection = Db.NewAdminConnection(connection.ServerName, connection.CurrentDatabase))
						{
							var currentApplicationLogin = adminConnection.Logins.FirstOrDefault(login => login.LoginName == connection.UserLogin);
							if (currentApplicationLogin != null && currentApplicationLogin.IsImpersonateEnterpriseDbUser)
							{
								adminConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER:: {dbUsername.QuoteName()} TO {connection.UserLogin.QuoteName()};");    // sql query

								connection.ExecuteNonQuery(sqlExecuteAsUser);
								connection.ImpersonatedLogin = dbUsername;

								return this;
							}
						}
					}

					throw;
				}
			}

			return this;
		}

		public void Dispose()
		{
			if (shouldDisposeConnection)
			{
				connection.Dispose();
			}
		}
	}
}
