using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin.ServerConfiguration;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin.ServerConfiguaration
{
	[TestedType(typeof(ConfigureLoopbackLinkedServer))]

	public class ConfigureLoopbackLinkedServerTest : TestCase, IDisposable
	{
		readonly string testLinkedServerName = "LOOPBACK_TEST_" + Guid.NewGuid().ToString().Replace("-", "");
		readonly AdminConnection connection = Db.NewAdminConnection();

		protected override void SetUp()
		{
			base.SetUp();
			DeleteLinkedServerIfExists();
		}

		void DeleteLinkedServerIfExists()
		{
			connection.ExecuteNonQuery(@"
				IF EXISTS (SELECT * FROM sys.servers WHERE [name] = @LINKEDSERVERNAME) 
				BEGIN 
					EXEC sys.sp_dropserver @LINKEDSERVERNAME, @droplogins='droplogins'
				END",
			cmd => cmd.AddParameter("@LINKEDSERVERNAME", System.Data.SqlDbType.NVarChar, 128, testLinkedServerName));
		}

		protected override void TearDown()
		{
			DeleteLinkedServerIfExists();
			base.TearDown();
		}

		public void TestEnsureLoopbackLinkedServerIsConfigured()
		{
			var exists = QueryLinkedExists(testLinkedServerName);
			AssertEquals(false, exists);

			ExecuteConfigureLoopbackLinkedServer(connection, testLinkedServerName);

			exists = QueryLinkedExists(testLinkedServerName);
			AssertEquals(true, exists);
		}

		public void TestEnsureLoopbackLinkedServerIsConfiguredParameters()
		{
			ExecuteConfigureLoopbackLinkedServer(connection, testLinkedServerName);

			SetBadParametersOnLinkedServer();

			ExecuteConfigureLoopbackLinkedServer(connection, testLinkedServerName);

			using (var cmd = connection.Command(""))
			{
				cmd.CommandText = @"
				SELECT 
					@connectionTimeout = [connect_timeout],
					@queryTimeout = [query_timeout],
					@isRpcOutEnabled = [is_rpc_out_enabled],
					@isDataAccessEnabled = [is_data_access_enabled],
					@isRemoteProcTransactionPromotionEnabled = [is_remote_proc_transaction_promotion_enabled]
				FROM sys.servers 
				WHERE [name] = @LINKEDSERVERNAME";

				cmd.AddParameter("@LINKEDSERVERNAME", SqlDbType.NVarChar, 128, testLinkedServerName);
				cmd.AddOutputParameter("@connectionTimeout", SqlDbType.Int, 0, 0, 0, null);
				cmd.AddOutputParameter("@queryTimeout", SqlDbType.Int, 0, 0, 0, null);
				cmd.AddOutputParameter("@isRpcOutEnabled", SqlDbType.Bit, 0, 0, 0, null);
				cmd.AddOutputParameter("@isDataAccessEnabled", SqlDbType.Bit, 0, 0, 0, null);
				cmd.AddOutputParameter("@isRemoteProcTransactionPromotionEnabled", SqlDbType.Bit, 0, 0, 0, null);

				cmd.ExecuteNonQuery();

				var connectionTimeout = (int)cmd.GetParameterValue("@connectionTimeout");
				var queryTimeout = (int)cmd.GetParameterValue("@queryTimeout");
				var isRpcOutEnabled = (bool)cmd.GetParameterValue("@isRpcOutEnabled");
				var isDataAccessEnabled = (bool)cmd.GetParameterValue("@isDataAccessEnabled");
				var isRemoteProcTransactionPromotionEnabled = (bool)cmd.GetParameterValue("@isRemoteProcTransactionPromotionEnabled");

				CombineAssertions(() =>
				{
					AssertEquals("connectionTimeout ", 0, connectionTimeout);
					AssertEquals("queryTimeout ", 60, queryTimeout);
					AssertEquals("isRpcOutEnabled ", true, isRpcOutEnabled);
					AssertEquals("isDataAccessEnabled ", true, isDataAccessEnabled);
					AssertEquals("isRemoteProcTransactionPromotionEnabled ", false, isRemoteProcTransactionPromotionEnabled);
				});
			}
		}

		public void TestLoopbackLinkerServerIsRecreatedWhenDataSourceIsWrong()
		{
			// Arrange
			connection.ExecuteNonQuery(
				$"EXEC master.dbo.sp_addlinkedserver @server = @LINKEDSERVERNAME, @srvproduct=N'SQL_Server', @provider=N'MSOLEDBSQL', @datasrc=N'Whatever'",
				cmd => cmd.AddParameter("@LINKEDSERVERNAME", SqlDbType.NVarChar, 128, testLinkedServerName));

			// Act
			ExecuteConfigureLoopbackLinkedServer(connection, testLinkedServerName);

			// Assert
			var dataSource = connection.ExecuteScalar<string>(
				"SELECT data_source FROM sys.servers WHERE name = @LoopbackServerName",
				cmd => cmd.AddParameter("@LoopbackServerName", SqlDbType.NVarChar, 128, testLinkedServerName));
			AssertNoExceptionThrown(
				$"LOOPBACK linked server [{testLinkedServerName}] is inaccessible. Its current data source is {dataSource}.",
				() => connection.ExecuteNonQuery($"EXEC sp_testlinkedserver {testLinkedServerName}"));
		}

		[DeveloperOnlyTest]
		public void TestEnsureLoopbackLinkedServerIsConfiguredRaceCreate()
		{
			var raceStart = new ManualResetEvent(false);

			for (int i = 0; i < 100; i++)
			{
				raceStart.Reset();
				DeleteLinkedServerIfExists();

				var t1 = Task.Run(() =>
				{
					var connection = Db.NewAdminConnection();
					raceStart.WaitOne();
					ExecuteConfigureLoopbackLinkedServer(connection, testLinkedServerName);
				});

				var t2 = Task.Run(() =>
				{
					var connection = Db.NewAdminConnection();
					raceStart.WaitOne();
					ExecuteConfigureLoopbackLinkedServer(connection, testLinkedServerName);
				});
				raceStart.Set();
				Task.WaitAll(t1, t2);
			}
			Assert(true);
		}

		[DeveloperOnlyTest]
		public void TestEnsureLoopbackLinkedServerIsConfiguredRaceFixParameters()
		{
			ExecuteConfigureLoopbackLinkedServer(connection, testLinkedServerName);
			var raceStart = new ManualResetEvent(false);

			for (int i = 0; i < 100; i++)
			{
				raceStart.Reset();
				SetBadParametersOnLinkedServer();

				var t1 = Task.Run(() =>
				{
					var connection = Db.NewAdminConnection();
					raceStart.WaitOne();
					ExecuteConfigureLoopbackLinkedServer(connection, testLinkedServerName);
				});

				var t2 = Task.Run(() =>
				{
					var connection = Db.NewAdminConnection();
					raceStart.WaitOne();
					ExecuteConfigureLoopbackLinkedServer(connection, testLinkedServerName);
				});
				raceStart.Set();
				Task.WaitAll(t1, t2);
			}
			Assert(true);
		}

		void ExecuteConfigureLoopbackLinkedServer(AdminConnection connection, string linkedServerName)
		{
			connection.ExecuteNonQuery(
				@"ConfigureLoopbackLinkedServer",
				cmd =>
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					cmd.AddParameter("@LINKEDSERVERNAME", System.Data.SqlDbType.NVarChar, 128, linkedServerName);
				}
				);
		}
		void SetBadParametersOnLinkedServer()
		{
			connection.ExecuteNonQuery(@"
				EXEC sys.sp_serveroption @server=@LINKEDSERVERNAME, @optname=N'remote proc transaction promotion', @optvalue=N'true'
				EXEC sys.sp_serveroption @server=@LINKEDSERVERNAME, @optname=N'rpc out', @optvalue=N'false'
				EXEC sys.sp_serveroption @server=@LINKEDSERVERNAME, @optname=N'data access', @optvalue=N'false'
				EXEC sys.sp_serveroption @server=@LINKEDSERVERNAME, @optname=N'connect timeout', @optvalue=N'100'
				EXEC sys.sp_serveroption @server=@LINKEDSERVERNAME, @optname=N'query timeout', @optvalue=N'20'",
			cmd => cmd.AddParameter("@LINKEDSERVERNAME", System.Data.SqlDbType.NVarChar, 128, testLinkedServerName));
		}
		bool QueryLinkedExists(string name)
		{
			return connection.Exists("FROM sys.servers WHERE [name] = @LINKEDSERVERNAME",
				cmd => cmd.AddParameter("@LINKEDSERVERNAME", System.Data.SqlDbType.NVarChar, 128, name));
		}

		public void Dispose()
		{
			connection?.Dispose();
		}
	}
}
