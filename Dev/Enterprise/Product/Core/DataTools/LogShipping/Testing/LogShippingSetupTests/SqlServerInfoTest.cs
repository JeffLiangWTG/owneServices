using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using Enterprise.LogShipping.Setup;
using NUnit.Framework;

namespace Enterprise.LogShipping.Testing
{
	abstract class SqlServerInfoTestBase : LogShippingTestFixture
	{
		[ExpectException(typeof(SqlServerInfoException))]
		public void TestSqlServerDoesNotExist()
		{
			SqlServerInfo.GetSqlServersInstances(Db.ServerName + Guid.NewGuid());
		}

		public void TestConstructor()
		{
			string testServer = "TestServer";
			SqlServerInfo info = new SqlServerInfo(testServer, "", "11.0.0.0");
			AssertEquals("Server name", testServer.ToUpper(), info.ServerName);
			AssertEquals("Server version", 11, info.Version.Major);
			AssertEquals("Default instance name", "MSSQLSERVER", info.InstanceName);
			AssertEquals("Server name should be without instance", testServer.ToUpper(), info.FullInstanceName);
			AssertEquals("Default SQL Server Agent name", "SQLSERVERAGENT", info.SQLServerAgentName);
			AssertSqlServerInfoToString(info);

			string testInstance = "TestInstance";
			info = new SqlServerInfo(".", testInstance, "10.50.0.0");
			AssertEquals("Server name", System.Environment.MachineName, info.ServerName);
			AssertEquals("Server major version", 10, info.Version.Major);
			AssertEquals("Server minor version", 50, info.Version.Minor);
			AssertEquals("Instance name", testInstance.ToUpper(), info.InstanceName);
			AssertEquals("<ServerName>\\<Instance>", System.Environment.MachineName + "\\" + testInstance.ToUpper(), info.FullInstanceName);
			AssertEquals("SQL Server Agent name", "SQLAgent$" + testInstance.ToUpper(), info.SQLServerAgentName);
			AssertSqlServerInfoToString(info);

			info = new SqlServerInfo(System.Environment.MachineName, "TestInstance", "10.0.0.0");
			AssertEquals("Server name", System.Environment.MachineName, info.ServerName);

			info = new SqlServerInfo("Test2005", "", "9.0.0.0");
			AssertEquals("Server name", "TEST2005", info.ServerName);
			AssertEquals("Server version", 9, info.Version.Major);
		}

		public void TestToString()
		{
			AssertSqlServerInfoToString(new SqlServerInfo("TestServer", "", Db.Connection.ProductVersion));
		}

		[SnailTest]
		public void TestGetSqlServersInstances()
		{
			SqlServerInfo[] servers = null;
			AssertNoExceptionThrown(
				"SQL Server Brower service is supposed to run",
				() => servers = SqlServerInfo.GetSqlServersInstances(Environment.MachineName));

			var expectedServer = Db.Connection.ServerNameReportedByDatabase;
			var runningServer = servers.FirstOrDefault(x => string.Equals(x.FullInstanceName, expectedServer, StringComparison.OrdinalIgnoreCase));
			AssertNotNull($"'{expectedServer}' should be detected at least", runningServer);
			AssertSqlServerInfo(runningServer);
		}

		public void TestDbManagerGetSqlServerInfoFromServerName()
		{
			foreach (var server in GetServers())
			{
				AssertSqlServerInfo(new DbManager().GetSqlServerInfoFromServerName(server));
			}

			IEnumerable<string> GetServers()
			{
				yield return Db.Connection.ServerNameReportedByDatabase;

				const string Backslash = "\\";
				if (!Db.Connection.ServerNameReportedByDatabase.Contains(Backslash)) // Default instance
				{
					yield return Db.Connection.ServerNameReportedByDatabase + Backslash; // Server name with back slash
				}
			}
		}

		static void AssertSqlServerInfo(SqlServerInfo testServerInfo)
		{
			try
			{
				using (var connection = Db.NewExtraConnectionWithMainDbCredentials(testServerInfo.FullInstanceName, Db.SqlMasterDb))
				{
					connection.EnsureIsOpen();

					AssertEquals("Server name", connection.ServerNameWithoutInstance, testServerInfo.ServerName);
					AssertEquals(
						$"{connection.ServerNameReportedByDatabase} != {testServerInfo.FullInstanceName}",
						connection.ServerNameReportedByDatabase,
						testServerInfo.FullInstanceName);

					if (IsInstanceDefault(testServerInfo.InstanceName))
					{
						AssertEquals("Default SQL Server Agent name", "SQLSERVERAGENT", testServerInfo.SQLServerAgentName);
					}
					else
					{
						AssertEquals("Instance name", connection.ServerInstanceName, testServerInfo.InstanceName);
						AssertEquals("SQL Server Agent name", "SQLAgent$" + connection.ServerInstanceName, testServerInfo.SQLServerAgentName);
					}

					AssertSqlServerInfoToString(testServerInfo);
				}
			}
			catch (System.Data.Common.DbException ex)
			{
				DbErrorMatch match = new DbErrorMatch(ex);
				if (match.ExceptionType == DbErrorType.ServerDoesNotExist)
				{
					Fail(string.Format("Server does not exist: [{0}].\r\n {1}", testServerInfo.FullInstanceName, ex.Message));
				}
				else
				{
					Assert("Server exists. But cannot connect to it with application login.", true);
				}
			}
		}

		static void AssertSqlServerInfoToString(SqlServerInfo info)
		{
			string pattern = string.Format(@"^{0}(\s\(default\sinstance\))?\s-\sSQL\s(2005|2008( R2)?|2012|2014|2016|2017|2019|2022)$", info.InstanceName);
			string message = string.Format("Description does not match: [{0}]", info.ToString());
			Assert(message, Regex.IsMatch(info.ToString(), pattern));

			AssertEndsWith(
				"Make use of SqlServerVersionNumber to make sure the generation returned by ToString is correct",
				GetExpectedGeneration(),
				info.ToString());

			string GetExpectedGeneration()
			{
				var generation = new SqlServerVersionNumber(info.Version.ToString()).SqlServerGeneration;
				return $"- {generation.Substring(0, 3)} {generation.Substring(3, 4)}{(generation.Length > 7 ? " " + generation.Substring(7) : string.Empty)}";
			}
		}

		static bool IsInstanceDefault(string instanceName)
		{
			return instanceName == "MSSQLSERVER";
		}

		sealed class CurrentSqlServerTest : SqlServerInfoTestBase
		{
		}

		[DatCapabilityRequirementLatestAvailableSqlServer]
		sealed class LatestAvailableSqlServerTest : SqlServerInfoTestBase
		{
		}
	}

	class SqlServerInfoForTesting : SqlServerInfo
	{
		public SqlServerInfoForTesting(string serverName, string instance)
			: base(serverName, string.IsNullOrEmpty(instance) ? "MSSQLSERVER" : instance, "0.0.0.0")
		{ }

		public SqlServerInfoForTesting(string serverName, string instance, string version)
			: base(serverName, string.IsNullOrEmpty(instance) ? "MSSQLSERVER" : instance, version)
		{ }
	}
}
