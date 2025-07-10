using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;

namespace Enterprise.Server.Setup
{
	public class CargowiseSetupSqlContextManager : IProtectedDataAdministrationSqlExecutionContextManager
	{
		readonly ISqlConnectionProvider sqlConnectionProvider;
		readonly IProtectedDataServiceFactory protectedDataServiceFactory;

		public CargowiseSetupSqlContextManager(ISqlConnectionProvider sqlConnectionProvider, IProtectedDataServiceFactory protectedDataServiceFactory)
		{
			this.sqlConnectionProvider = sqlConnectionProvider;
			this.protectedDataServiceFactory = protectedDataServiceFactory;
		}

		IDbConnection connection;
		public IDbConnection Connection => connection;

		ISqlExecutionContext currentContext;
		public ISqlExecutionContext CurrentContext => currentContext;

		public void OpenConnection(string serverName)
		{
			if (connection is not null)
			{
				return;
			}

			List<Exception> errors = null;

			IEnumerable<IDbConnection> PossibleConnections()
			{
				yield return AttemptOpenEnterpriseConnection<SysAdminCredentials>(serverName, ref errors);
				yield return AttemptOpenEnterpriseConnection<OdysseyAdminCredentials>(serverName, ref errors);
				yield return AttemptOpenCustomConnection(serverName, b => b.IntegratedSecurity = true, ref errors);
				yield return AttemptOpenCustomConnection(serverName, b => { b.UserID = "sa"; b.Password = string.Empty; }, ref errors);
			}

			connection = PossibleConnections().FirstOrDefault(connection => IsAdminOrDispose(connection, ref errors));

			if (connection is null)
			{
				throw new InvalidOperationException("Unable to open a SysAdmin connection to the specified server. Please make sure you have access to the specified sql server instance and you are a member of the SysAdmin server role.", new AggregateException(errors));
			}

			currentContext = new SqlExecutionContext(connection, null);
		}

		bool IsAdminOrDispose(IDbConnection connection, ref List<Exception> errors)
		{
			if (connection is not null)
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "SELECT IS_SRVROLEMEMBER('sysadmin')";
					if ((int)cmd.ExecuteScalar() == 1)
					{
						return true;
					}
					else
					{
						connection.Close();
						connection.Dispose();
						connection = null;
						errors ??= new List<Exception>();
						errors.Add(new Exception("User is not a member of the SysAdmin group."));
						return false;
					}
				}
			}
			else
			{
				return false;
			}
		}

		public void SwitchToSysAdminAccount(string serverName)
		{
			connection?.Close();
			connection.Dispose();
			connection = null;

			var pds = protectedDataServiceFactory.CreateEnterpriseService(serverName);
			connection = sqlConnectionProvider.OpenNewSqlConnection<SysAdminCredentials>(pds, (builder) => ConfigureConnectionString(builder, serverName));

			currentContext = new SqlExecutionContext(connection, null);
		}

		IDbConnection AttemptOpenEnterpriseConnection<TCredentials>(string serverName, ref List<Exception> errors) where TCredentials : DBCredentials
		{
			var pds = protectedDataServiceFactory.CreateEnterpriseService(serverName);
			try
			{
				return sqlConnectionProvider.OpenNewSqlConnection<TCredentials>(pds, (builder) => ConfigureConnectionString(builder, serverName));
			}
			catch (Exception ex)
			{
				errors ??= new List<Exception>();
				errors.Add(ex);
				return null;
			}
		}

		internal virtual IDbConnection AttemptOpenCustomConnection(string serverName, Action<SqlConnectionStringBuilder> configureConnectionString, ref List<Exception> errors)
		{
#pragma warning disable CW1065 // Encrypt SQL Connection Rule - Bad Anylizer is not able to detect Encrypt = true
			var builder = new SqlConnectionStringBuilder();
#pragma warning restore CW1065 // Encrypt SQL Connection Rule
			try
			{
				ConfigureConnectionString(builder, serverName);
				configureConnectionString(builder);
#pragma warning disable CW1116 // The existing CargoWise.Data.Db.Connection should be used rather than creating a new one - think twice if you really need a new connection.
				var con = new SqlConnection(builder.ConnectionString);
#pragma warning restore CW1116
				con.Open();
				return con;
			}
			catch (Exception ex)
			{
				errors ??= new List<Exception>();
				errors.Add(new Exception($"Error connecting using connection string: {builder.ConnectionString}", ex));
				errors.Add(ex);
				return null;
			}
		}

		void ConfigureConnectionString(SqlConnectionStringBuilder builder, string serverName)
		{
			builder.DataSource = serverName;
			builder.InitialCatalog = "master";
			builder.TrustServerCertificate = true;
			builder.Pooling = false;
			builder.ApplicationName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
			builder.Encrypt = true;
		}

		protected ISqlExecutionContext CreateProtectedDataAdministrationSqlContext(string serverName)
		{
			if (currentContext == null)
			{
				throw new InvalidOperationException("No connection has been established yet.");
			}

			return currentContext;
		}

		public void Disconnect()
		{
			connection?.Dispose();
			connection = null;
			currentContext = null;
		}

		public ISqlExecutionContext GetSqlExecutionContext(string serverName, string databaseName)
		{
			if (currentContext == null)
			{
				CreateProtectedDataAdministrationSqlContext(serverName);
			}
			return currentContext;
		}
	}
}
