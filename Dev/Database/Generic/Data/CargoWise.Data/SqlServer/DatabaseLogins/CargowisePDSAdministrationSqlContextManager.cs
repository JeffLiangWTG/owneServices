using System;
using System.Threading;
using CargoWise.DataProtection.Administration.SqlServer;

namespace CargoWise.Data
{
	public class CargowisePDSAdministrationSqlContextManager : IProtectedDataAdministrationSqlExecutionContextManager
	{
		static readonly AutoResetEvent serializeEvent = new AutoResetEvent(true);

		CargowisePDSAdministrationExecutionScope currentScope;
		public CargowisePDSAdministrationSqlContextManager()
		{
		}

		public ISqlExecutionContext GetSqlExecutionContext(string serverName, string databaseName)
		{
			if (serverName == currentScope.mainDbConnection.ServerName)
			{
				return new SqlExecutionContext(((IDbConnectionInternals)currentScope.mainDbConnection).ADOConnection, ((IDbConnectionInternals)currentScope.mainDbConnection).ADOTransaction);
			}
			else if (serverName == currentScope.targetDbConnection.ServerName)
			{
				return new SqlExecutionContext(((IDbConnectionInternals)currentScope.targetDbConnection).ADOConnection, ((IDbConnectionInternals)currentScope.targetDbConnection).ADOTransaction);
			}
			else
			{
				throw new InvalidOperationException("Server name does not match any of the connections in the current scope.");
			}
		}

		public CargowisePDSAdministrationExecutionScope BeginScope(DbConnection mainDbConnection, DbConnection targetServerConnection)
		{
			if (!serializeEvent.WaitOne(5000))
			{
				throw new InvalidOperationException("Failed to create a scope because another one is being used.");
			}
			return new CargowisePDSAdministrationExecutionScope(this, mainDbConnection, targetServerConnection);
		}

		public class CargowisePDSAdministrationExecutionScope : IDisposable
		{
			public readonly DbConnection mainDbConnection;
			public readonly DbConnection targetDbConnection;
			readonly CargowisePDSAdministrationSqlContextManager owner;
			public CargowisePDSAdministrationExecutionScope(CargowisePDSAdministrationSqlContextManager owner, DbConnection mainDbConnection, DbConnection targetDbConnection)
			{
				this.mainDbConnection = mainDbConnection;
				this.targetDbConnection = targetDbConnection;
				this.owner = owner;
				if (owner.currentScope != null)
				{
					throw new InvalidOperationException("An execution scope already exists.");
				}
				owner.currentScope = this;
			}

			public void Dispose()
			{
				owner.currentScope = null;
				serializeEvent.Set();
			}
		}
	}
}
