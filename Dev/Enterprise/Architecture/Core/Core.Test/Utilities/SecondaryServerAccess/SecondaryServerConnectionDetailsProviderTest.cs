using System;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DataProtection;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class SecondaryServerConnectionDetailsProviderTest : TestCase
	{
		public void TestGetAnyUpToDateReportServerAddLogs()
		{
			var logs = new StringBuilder();
			var provider = new ReportDbForTesting(new[] { "TestServer" }, Db.DatabaseName, addLogs: (message) => logs.AppendLine(message));
			provider.ReportServerRespondTime = provider.SuitableDelay + 1;
			provider.GetAnyUpToDateReportServer();

			AssertEquals("Unable to connect to the reporting database servers, report will be run on primary server.\r\n", logs.ToString());

			logs.Clear();
			provider.ReportServerRespondTime = null;
			provider.ReportConnectionException = SqlExceptionBuilder.CreateSqlException(6005, "SHUTDOWN is in progress.");
			provider.GetAnyUpToDateReportServer();

			var expectedMessage = @"Unable to connect to the reporting database server TestServer, error message: SHUTDOWN is in progress.
Unable to connect to the reporting database servers, report will be run on primary server.
";
			AssertEquals(expectedMessage, logs.ToString());
		}

		public void TestCacheRefreshed()
		{
			// This shows the call increments every time
			var provider = new ReportDbForTesting(new[] { System.Environment.MachineName }, Db.DatabaseName);
			provider.GetAnyUpToDateReportServer();
			AssertEquals(1, provider.GetDelayCallCount);
			provider.GetAnyUpToDateReportServer();
			AssertEquals(2, provider.GetDelayCallCount);
			provider.GetAnyUpToDateReportServer();
			AssertEquals(3, provider.GetDelayCallCount);

			// this shows cache exceeded
			provider = new ReportDbForTesting(new[] { System.Environment.MachineName }, Db.DatabaseName)
			{
				ReportServerRespondTime = 600
			};
			using (provider.GetNewConnectionCore(null))
			{ }
			AssertEquals(1, provider.GetDelayCallCount);
			using (provider.GetNewConnectionCore(null))
			{ }
			AssertEquals(1, provider.GetDelayCallCount);

			// this shows cache initially not exceeded, but now exceeded due to incrementing delay
			provider = new ReportDbForTesting(new[] { System.Environment.MachineName }, Db.DatabaseName)
			{
				ReportServerRespondTime = 0
			};
			using (provider.GetNewConnectionCore(null))
			{ }
			AssertEquals(1, provider.GetDelayCallCount);
			using (provider.GetNewConnectionCore(null))
			{ }
			AssertEquals(1, provider.GetDelayCallCount);
			Thread.Sleep(1000);
			using (provider.GetNewConnectionCore(null))
			{ }
			AssertEquals(2, provider.GetDelayCallCount);
		}

		[UseSnapshotProtection]
		public void TestGetNewConnectionCore_WithDbUserName()
		{
			var provider = new ReportDbForConnectionTesting(new[] { System.Environment.MachineName }, Db.DatabaseName);

			using (var testAdminConnection = Db.NewAdminConnection(System.Environment.MachineName, Db.DatabaseName))
			{
				testAdminConnection.ExecuteNonQuery("CREATE USER User_TestGetNewConnectionCore WITHOUT LOGIN;");
				testAdminConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER:: User_TestGetNewConnectionCore TO {UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)};");
			}

			using (var connectionWrapper = provider.GetNewConnectionCore("User_TestGetNewConnectionCore"))
			{
				var currentDbUser = connectionWrapper.Connection.ExecuteScalar<string>("SELECT USER_NAME()");

				AssertEquals(UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), connectionWrapper.Connection.UserLogin);
				AssertEquals("User_TestGetNewConnectionCore", currentDbUser);
			}
		}

		public void TestGetNewConnectionCore_WithoutDbUserName()
		{
			var provider = new ReportDbForConnectionTesting(new[] { System.Environment.MachineName }, Db.DatabaseName);

			using (var connectionWrapper = provider.GetNewConnectionCore(null))
			{
				var currentDbUser = connectionWrapper.Connection.ExecuteScalar<string>("SELECT USER_NAME()");

				AssertEquals(RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), connectionWrapper.Connection.UserLogin);
				AssertEquals(RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), currentDbUser);
			}
		}

		class ReportDbForConnectionTesting : SecondaryServerConnectionDetailsProvider
		{
			public ReportDbForConnectionTesting(string[] serverNames, string dbName, Action<Exception, string> showException = null)
				: base(showException)
			{
				this.serverNames = serverNames;
				this.dbName = dbName;
				ClearCache();
			}

			protected override string[] GetAllReportServerNames() => serverNames;
			readonly string[] serverNames;

			protected override string DatabaseName => dbName;
			readonly string dbName;

			protected override bool IsPartOfAlwaysOn => true;

			internal bool TryGetSystemIdleForTest(string reportServerName, out int systemIdle)
			{
				return base.TryGetSystemIdle(reportServerName, out systemIdle);
			}
		}

		public void TestDelayIncrementsBasedOnAge()
		{
			var provider = new ReportDbForLoadBalanceTesting(new[] { "1", "2", "3" }, Db.DatabaseName)
			{
				ReportServerRespondTime = 50
			};
			var delay1 = provider.GetAnyUpToDateReportServer();
			var initialDelay = delay1.Delay;
			Thread.Sleep(1000);
			Assert(delay1.Delay != initialDelay);
		}

		public void TestServerNameIncrementsOnEachCallForSingleDB()
		{
			var provider = new ReportDbForLoadBalanceTesting(new[] { "1" }, "XX")
			{
				ReportServerRespondTime = 0
			};
			AssertEquals("1", provider.GetAnyUpToDateReportServer().ServerName);
			AssertEquals("1", provider.GetAnyUpToDateReportServer().ServerName);
			AssertEquals("1", provider.GetAnyUpToDateReportServer().ServerName);
		}

		public void TestShowMessageOfSecondaryServerConnectionException()
		{
			SystemDataRegistryForTest.Get().ReportingDbServerNames = new string[] { Db.Connection.ServerName };
			SystemDataRegistryForTest.Get().UseReportingDbServerNames = true;
			var message = string.Empty;
			var serverName = string.Empty;

			var reportDb = new ReportDbForTestingWithoutOverrideAllReportServerNames(new string[] { Db.Connection.ServerName }, "InvalidDatabase", true, (exception, sn) =>
			{
				message = exception.Message;
				serverName = sn;
			});
			reportDb.GetDelayBetweenPrimaryAndReportDatabaseInMinutes();

			AssertEquals(true, message.Equals($"Login failed for user '{RestrictedReaderLoginCredentials.UserNameFor("InvalidDatabase")}'.",StringComparison.OrdinalIgnoreCase));
			AssertEquals("Cannot return the name of the server for which an exception occurred", Db.Connection.ServerName, serverName);

			SystemDataRegistryForTest.Get().UseReportingDbServerNames = false;
		}

		public void TestGetIdlestInSecondaryServers()
		{
			var serverNames = new[] { "10", "20", "50" };
			SystemDataRegistryForTest.Get().ReportingDbServerNames = serverNames;
			SystemDataRegistryForTest.Get().UseReportingDbServerNames = true;

			var reportDb = new ReportDbForLoadBalanceTesting(serverNames, Db.DatabaseName) { ReportServerRespondTime = 10 };
			var serverDelay = reportDb.GetAnyUpToDateReportServer();
			AssertEquals("Should use the idlest server to run report.", "50", serverDelay.ServerName);

			SystemDataRegistryForTest.Get().UseReportingDbServerNames = false;
		}

		public void TestReportExceptionInGetSystemIdle()
		{
			var message = string.Empty;
			var serverName = string.Empty;
			var serverNames = new[] { "10", "20", "50" };

			var reportDb = new ReportDbForLoadBalanceTesting(serverNames, "InvalidDatabase", (exception, sn) =>
			{
				message = exception.Message;
				serverName = sn;
			});

			reportDb.ReportServerRespondTime = 10;
			reportDb.ReportServerException = new Exception("ExceptionForTest");

			var serverDelay = reportDb.GetAnyUpToDateReportServer();

			AssertEquals("Should log exception message.", "ExceptionForTest", message);
			AssertCollectionContains("Should choose a random server if we cannot get system idle for all report servers.", serverDelay.ServerName, serverNames);
		}

		#region TestTryGetSystemIdle_ShouldReturnMostRecentRecord

		[UseSnapshotProtection]
		public void TestTryGetSystemIdle_ShouldReturnMostRecentRecord()
		{
			var tableName = "TestTable";
			var createTableSql = $"CREATE TABLE {tableName} (timestamp BIGINT, record NVARCHAR(500), ring_buffer_type NVARCHAR(60))";
			var insertDataSql = $@"INSERT INTO {tableName} VALUES
('832694965', '<Record id = ""1616"" type =""RING_BUFFER_SCHEDULER_MONITOR"" time =""832694965""><SchedulerMonitorEvent><SystemHealth><ProcessUtilization>0</ProcessUtilization><SystemIdle>91</SystemIdle><UserModeTime>156250</UserModeTime><KernelModeTime>0</KernelModeTime><PageFaults>40</PageFaults><WorkingSetDelta>135168</WorkingSetDelta><MemoryUtilization>98</MemoryUtilization></SystemHealth></SchedulerMonitorEvent></Record>', 'RING_BUFFER_SCHEDULER_MONITOR'),
('832634880', '<Record id = ""1615"" type =""RING_BUFFER_SCHEDULER_MONITOR"" time =""832634880""><SchedulerMonitorEvent><SystemHealth><ProcessUtilization>0</ProcessUtilization><SystemIdle>92</SystemIdle><UserModeTime>13125000</UserModeTime><KernelModeTime>468750</KernelModeTime><PageFaults>753</PageFaults><WorkingSetDelta>2215936</WorkingSetDelta><MemoryUtilization>98</MemoryUtilization></SystemHealth></SchedulerMonitorEvent></Record>', 'RING_BUFFER_SCHEDULER_MONITOR'),
('832755082', '<Record id = ""1617"" type =""RING_BUFFER_SCHEDULER_MONITOR"" time =""832755082""><SchedulerMonitorEvent><SystemHealth><ProcessUtilization>0</ProcessUtilization><SystemIdle>93</SystemIdle><UserModeTime>312500</UserModeTime><KernelModeTime>0</KernelModeTime><PageFaults>16</PageFaults><WorkingSetDelta>40960</WorkingSetDelta><MemoryUtilization>98</MemoryUtilization></SystemHealth></SchedulerMonitorEvent></Record>', 'RING_BUFFER_SCHEDULER_MONITOR')";
			var dropTableSql = $"DROP TABLE IF EXISTS {tableName}";

			using (var connection = Db.NewAdminConnection(System.Environment.MachineName, Db.DatabaseName))
			using (connection.UseMasterDb())
			{
				connection.ExecuteNonQuery(dropTableSql);
				connection.ExecuteNonQuery(createTableSql);
				connection.ExecuteNonQuery(insertDataSql);

				var exceptionMessage = string.Empty;
				var provider = new ReportDbForLoadBalanceOrderTesting(new[] { System.Environment.MachineName }, Db.DatabaseName,
					(exception, s) => { exceptionMessage = $"{s}: {exception.Message} {exception.StackTrace}"; });
				var flag = provider.TryGetSystemIdleForTest(System.Environment.MachineName, out var systemIdle);

				AssertEquals($"{System.Environment.MachineName} - {Db.DatabaseName}", string.Empty, exceptionMessage);
				AssertEquals("Should return true if we can get system idle.", true, flag);
				AssertEquals("System idle equals to 93 is the most recent record.", 93, systemIdle);

				connection.ExecuteNonQuery(dropTableSql);
			}
		}

		class ReportDbForLoadBalanceOrderTesting : ReportDbForConnectionTesting
		{
			public ReportDbForLoadBalanceOrderTesting(string[] serverNames, string dbName, Action<Exception, string> showException = null) : base(serverNames, dbName, showException)
			{
			}

			protected override string TableName => "TestTable";
		}

		#endregion

		class ReportDbForLoadBalanceTesting : ReportDbForTesting
		{
			public ReportDbForLoadBalanceTesting(string[] serverNames, string dbName, Action<Exception, string> showException = null)
				: base(serverNames, dbName, showException: showException)
			{
			}

			protected override bool TryGetSystemIdle(string reportServerName, out int systemIdle)
			{
				if (ReportServerException != null)
				{
					ShowException(ReportServerException, reportServerName);
					systemIdle = -1;
					return false;
				}
				else
				{
					int.TryParse(reportServerName, out systemIdle);
					return true;
				}
			}
		}
	}
}
