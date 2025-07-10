using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.AlwaysOn.Setup
{
	public class AlwaysOnSqlExecutionContextManager : IProtectedDataAdministrationSqlExecutionContextManager, IDisposable
	{
		readonly IServiceProvider serviceProvider;
		readonly ISqlConnectionProvider sqlConnectionProvider;
		readonly IProtectedDataServiceFactory pdsFactory;
		bool disposedValue;

		public AlwaysOnSqlExecutionContextManager(IServiceProvider serviceProvider, ISqlConnectionProvider sqlConnectionProvider, IProtectedDataServiceFactory pdsFactory)
		{
			this.serviceProvider = serviceProvider;
			this.sqlConnectionProvider = sqlConnectionProvider;
			this.pdsFactory = pdsFactory;
		}

		public void InitializeServerSecurityUsingIntegratedSecurity(SqlServerInfo serverInfo, DbLoginInfo primaryServerAdminLoginInfo)
		{
			using (var winAuthConnection = OpenNewIntegratedSecurityConnection(serverInfo))
			{
				winAuthConnection.Open();
				var sqlContext = new SqlExecutionContext(winAuthConnection, null);
				serverInfo.LoadDetailsFromServer(sqlContext);
				CurrentExecutionScope.DisposeAllConnections();
				CurrentExecutionScope.scopeConnections.Add(serverInfo.ServerInstanceFQDN, winAuthConnection);
				if (!string.IsNullOrWhiteSpace(primaryServerAdminLoginInfo.LoginSid))
				{
					// We copy the login from primary server to secondary server if it is available on primary but not on secondary.
					DbSecurity.CreateAdminLogin(sqlContext, primaryServerAdminLoginInfo.LoginName, primaryServerAdminLoginInfo.PwdHash, true, primaryServerAdminLoginInfo.LoginSid);
				}
				else
				{
					var initService = serviceProvider.GetRequiredService<ISqlServerSecurityInitializationService>();
					initService.InitializeServer(serverInfo.FullDataSource);
				}
				CurrentExecutionScope.DisposeAllConnections();
			}
		}

		public ISqlExecutionContext GetSqlExecutionContext(string serverName, string databaseName)
		{
			if (databaseName != "master")
			{
				throw new InvalidOperationException("Connection to the requested database is not allowed by this application.");
			}
			return GetSqlExecutionContext(serverName);
		}

		public ISqlExecutionContext GetSqlExecutionContext(string serverName)
		{
			if (CurrentExecutionScope is null)
			{
				throw new InvalidOperationException("Not in an execution scope.");
			}

			var key = serverName.Split(',')[0];
			IDbConnection sqlConnection = null;
			if (!CurrentExecutionScope.scopeConnections.TryGetValue(key, out sqlConnection))
			{
				sqlConnection = OpenNewAdminConnection(serverName);
				CurrentExecutionScope.scopeConnections[key] = sqlConnection;
			}

			return new SqlExecutionContext(sqlConnection, null);
		}

		public ISqlExecutionContext GetSqlExecutionContext(SqlServerInfo serverInfo, DbLoginInfo primaryServerAdminLoginInfo = default)
		{
			try
			{
				if (serverInfo.DetailsLoaded)
				{
					return GetSqlExecutionContext(serverInfo.FullDataSource);
				}
				else
				{
					return GetSqlExecutionContext(serverInfo.AliasDataSource);
				}
			}
			catch (SqlException ex)
			{
				if (ex.Number == DbManager.LoginFailedForUserErrorNumber)
				{
					try
					{
						InitializeServerSecurityUsingIntegratedSecurity(serverInfo, primaryServerAdminLoginInfo);
						return GetSqlExecutionContext(serverInfo, primaryServerAdminLoginInfo);
					}
					catch (SqlException createLoginEx)
					{
						if (createLoginEx.Number == DbManager.LoginAlreadyExistsErrorNumber)
						{
							throw new Exception("Database Server must allow both Windows and SQL Server authentication (mixed mode).");
						}
					}
				}
				throw;
			}
		}

		SqlConnection OpenNewAdminConnection(string dataSource)
		{
			var pds = pdsFactory.CreateEnterpriseService(dataSource);
			return sqlConnectionProvider.OpenNewSqlConnection<OdysseyAdminCredentials>(pds, b => ConfigureConnectionString(b, dataSource));
		}

		SqlConnection OpenNewIntegratedSecurityConnection(SqlServerInfo serverInfo)
		{
			var builder = new SqlConnectionStringBuilder();
			ConfigureConnectionString(builder, serverInfo);
			builder.IntegratedSecurity = true;
#pragma warning disable CW1116 // The existing CargoWise.Data.Db.Connection should be used rather than creating a new one - think twice if you really need a new connection.
			return new SqlConnection(builder.ConnectionString);
#pragma warning restore CW1116
		}

		static void ConfigureConnectionString(SqlConnectionStringBuilder builder, SqlServerInfo serverInfo)
		{
			ConfigureConnectionString(builder, serverInfo.DetailsLoaded ? serverInfo.FullDataSource : serverInfo.AliasDataSource);
		}

		public static void ConfigureConnectionString(SqlConnectionStringBuilder builder, string fullDataSourceName)
		{
			builder.ApplicationName = "cwAlwaysOnSetup";
			builder.DataSource = fullDataSourceName;
			var encrypt = false;
			SqlTlsSetting.ShouldEncryptSqlConnection(fullDataSourceName);
			builder.Encrypt = encrypt;
			builder.TrustServerCertificate = !encrypt;

			if (SqlFailoverSettings.ShouldSpecifyMultiSubnetFailover(fullDataSourceName.Split(',')[0]))
			{
				builder.MultiSubnetFailover = true;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					CurrentExecutionScope.Dispose();
					CurrentExecutionScope = null;
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public ExecutionScope CurrentExecutionScope { get; private set; }
		public IExecutionScope NewExecutionScope()
		{
			if (CurrentExecutionScope != null)
			{
				throw new InvalidOperationException("There is alread an execution scope running. Nested scopes are not supported.");
			}
			this.CurrentExecutionScope = new ExecutionScope(this);
			return CurrentExecutionScope;
		}
		public interface IExecutionScope : IDisposable { }
		public class ExecutionScope : IExecutionScope
		{
			public readonly Dictionary<string, IDbConnection> scopeConnections;
			readonly AlwaysOnSqlExecutionContextManager contextManager;
			public ExecutionScope(AlwaysOnSqlExecutionContextManager contextManager)
			{
				scopeConnections = new Dictionary<string, IDbConnection>();
				this.contextManager = contextManager;
			}

			public void DisposeAllConnections()
			{
				foreach (var connection in scopeConnections.Values)
				{
					connection.Dispose();
				}
				scopeConnections.Clear();
			}

			public void DisposeConnection(string key)
			{
				IDbConnection sqlConnection = null;
				if (!scopeConnections.TryGetValue(key, out sqlConnection))
				{
					scopeConnections.Remove(key);
					sqlConnection?.Dispose();
				}
			}

			public void Dispose()
			{
				foreach (var connection in scopeConnections.Values)
				{
					connection.Dispose();
				}
				scopeConnections.Clear();
				contextManager.CurrentExecutionScope = null;
			}
		}
	}
}
