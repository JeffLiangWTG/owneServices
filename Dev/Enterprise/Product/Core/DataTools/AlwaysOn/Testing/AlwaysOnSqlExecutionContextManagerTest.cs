using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Providers.Common;
using Enterprise.AlwaysOn.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.AlwaysOn.Testing
{
	class AlwaysOnSqlExecutionContextManagerTest : AlwaysOnTestFixture
	{
		public void TestCreateSqlExecutionContextFromServerInfo()
		{
			// Arrange
			var serverInfo = new SqlServerInfo(Db.ServerName);
			var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(serverInfo);
			AssertEquals("OK", sqlContext.ExecuteScalar("SELECT 'OK'"));
		}

		public void TestEnabledMultiSubnetFailover()
		{
			const string serverName = "server1";
			var builder = new SqlConnectionStringBuilder();
			using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: new[] { serverName }))
			{
				AlwaysOnSqlExecutionContextManager.ConfigureConnectionString(builder, serverName);
				Assert(builder.MultiSubnetFailover);
			}

			var builder2 = new SqlConnectionStringBuilder();
			AlwaysOnSqlExecutionContextManager.ConfigureConnectionString(builder2, serverName);
			Assert(!builder2.MultiSubnetFailover);
		}

		public void TestContextManager_OutOfScopeExecution_Throws()
		{
			var contextManager = ActivatorUtilities.CreateInstance<AlwaysOnSqlExecutionContextManager>(Program.ServiceProvider);

			using (var scope = contextManager.NewExecutionScope())
			{
				AssertNoExceptionThrown(() => contextManager.GetSqlExecutionContext(Db.ServerName));
			}

			AssertExceptionThrown<InvalidOperationException>(() => contextManager.GetSqlExecutionContext(Db.ServerName));
		}

		public void TestContextManager_EndOfScope_ConnectionsAreDisposed()
		{
			var contextManager = ActivatorUtilities.CreateInstance<AlwaysOnSqlExecutionContextManager>(Program.ServiceProvider);

			AssertNull(contextManager.CurrentExecutionScope);
			WeakReference<IDbConnection> connectionReference;
			using (var scope = (AlwaysOnSqlExecutionContextManager.ExecutionScope)contextManager.NewExecutionScope())
			{
				AssertNotNull(contextManager.CurrentExecutionScope);
				AssertNoExceptionThrown(() => contextManager.GetSqlExecutionContext(Db.ServerName));
				AssertEquals(1, scope.scopeConnections.Count);
				connectionReference = new WeakReference<IDbConnection>(scope.scopeConnections.First().Value);
				Assert(connectionReference.TryGetTarget(out var notYetDisposedConnection));
				AssertEquals(ConnectionState.Open, notYetDisposedConnection.State);
			}
			AssertNull(contextManager.CurrentExecutionScope);
			connectionReference.TryGetTarget(out var disposedConnection);
			AssertEquals(ConnectionState.Closed, disposedConnection.State);
		}
	}
}
