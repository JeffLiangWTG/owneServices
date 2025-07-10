using System;
using System.Text.RegularExpressions;
using CargoWise.Data;
using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Testing
{
	class SqlServerInfoTest : AlwaysOnTestFixture
	{
		public void TestConstructor()
		{
			var testServer = "TestServer";
			var info = new SqlServerInfo(testServer, default);
			AssertEquals("Server name", testServer, info.ServerAlias);
			AssertEquals("Default instance name", "MSSQLSERVER", info.InstanceName);
			AssertEquals("Server name should be without instance", testServer, info.ServerAlias);
			AssertSqlServerInfoToString(info);

			var testInstance = "TestInstance";
			info = new SqlServerInfo(".\\TestInstance");
			AssertEquals("Server name", System.Environment.MachineName, info.ServerMachineName);
			AssertEquals("Instance name", testInstance, info.InstanceName);
			AssertEquals("<ServerName>\\<Instance>", System.Environment.MachineName + "\\" + testInstance, info.ServerAlias);
			AssertSqlServerInfoToString(info);

			info = new SqlServerInfo($"{System.Environment.MachineName}\\TestInstance");
			AssertEquals("Server name", System.Environment.MachineName, info.ServerMachineName);

			info = new SqlServerInfo("Test2005");
			AssertEquals("Server name", "Test2005", info.ServerAlias);
		}

		public void TestLoadDetailsFromServer_CurrentSqlServer()
		{
			TestLoadDetailsFromServer();
		}

		[DatCapabilityRequirementLatestAvailableSqlServer]
		public void TestLoadDetailsFromServer_LatestAvailableSqlServer()
		{
			TestLoadDetailsFromServer();
		}

		public void TestLoadDetailsFromServer()
		{
			var serverInfo = new SqlServerInfo(Db.ServerName);
			var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName);
			serverInfo.LoadDetailsFromServer(sqlContext);
			Assert(serverInfo.DetailsLoaded);

			var expectedInstanceName = string.IsNullOrEmpty(Db.Connection.ServerInstanceName) ? "MSSQLSERVER" : Db.Connection.ServerInstanceName;
			AssertEquals(expectedInstanceName, serverInfo.InstanceName); // instanceName

			var expectedServerAlias = Db.Connection.ServerName;
			AssertEquals(expectedServerAlias, serverInfo.ServerAlias); // machineName\instanceName

			var expectedDomainName = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().Name;
			AssertEquals(expectedDomainName, serverInfo.ServerDomain); // domain.net

			var expectedServerFQDN = Environment.MachineName + "." + expectedDomainName;
			AssertEquals(expectedServerFQDN, serverInfo.ServerFQDN); // machineName.domain.net

			var expectedInstanceFQDN = string.IsNullOrEmpty(Db.Connection.ServerInstanceName) ? expectedServerFQDN : expectedServerFQDN + "\\" + Db.Connection.ServerInstanceName;
			AssertEquals(expectedInstanceFQDN, serverInfo.ServerInstanceFQDN); // machineName.domain.net\instanceName

			var expectedMachineName = Environment.MachineName;
			AssertEquals(expectedMachineName, serverInfo.ServerMachineName); // machineName
		}

		public void TestDataSourcePortNumber()
		{
			var serverInfo = new SqlServerInfo("MachineName\\InstanceName", "MachineName", "InstanceName", "Domain.Net", "MachineName.Domain.Net", 1010);

			AssertEquals($"MachineName\\InstanceName,1010", serverInfo.AliasDataSource);

			AssertEquals($"MachineName.Domain.Net\\InstanceName,1010", serverInfo.FullDataSource);
		}

		void AssertSqlServerInfoToString(SqlServerInfo info)
		{
			var pattern = Invariant($@"^{info.InstanceName}(\s\(default\sinstance\))?$");
			Assert("Description does not match", Regex.IsMatch(info.ToString(), pattern));
		}
	}
}
