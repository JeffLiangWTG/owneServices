using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DataProtection;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ReportDbConnectionManagerTest : TestCase
	{
		public void TestNewConnectionWhenReportServerSpecified()
		{
			var serverName = Db.Connection.ServerNameReportedByDatabase;
			var reportDb = new ReportDbForTesting(serverName, Db.DatabaseName);
			var provider = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);

			using (var connectionWrapper = provider.GetNewConnectionWrapper(null))
			{
				AssertNotNull("Report connection shouldn't be null when report server specified", connectionWrapper);
				AssertEquals("SQL Session ID", reportDb.CreatedConnectionSpid, connectionWrapper.Connection.SPID);
				AssertEquals("ServerName", serverName, connectionWrapper.Connection.ServerNameReportedByDatabase);
				AssertEquals("DatabaseName", Db.DatabaseName, connectionWrapper.Connection.CurrentDatabase);
				AssertEquals("SPID same as main connection?", false, reportDb.CreatedConnectionSpid.Value == Db.Connection.SPID);
			}
		}

		[UseSnapshotProtection]
		public void TestNewConnectionWhenReportServerSpecified_Impersonate()
		{
			var serverName = Db.Connection.ServerNameReportedByDatabase;
			var reportDb = new ReportDbForTesting(serverName, Db.DatabaseName);
			var provider = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);

			using (var testAdminConnection = Db.NewAdminConnection(serverName, Db.DatabaseName))
			{
				testAdminConnection.ExecuteNonQuery("CREATE USER User_TestNewConnectionWhenReportServerSpecified_Impersonate WITHOUT LOGIN;");
				testAdminConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER:: User_TestNewConnectionWhenReportServerSpecified_Impersonate TO {UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)};");
			}

			using (var connectionWrapper = provider.GetNewConnectionWrapper("User_TestNewConnectionWhenReportServerSpecified_Impersonate"))
			{
				var currentDbUser = connectionWrapper.Connection.ExecuteScalar("SELECT USER_NAME()") as string;

				AssertNotNull("Report connection shouldn't be null when report server specified", connectionWrapper);
				AssertEquals("SQL Session ID", reportDb.CreatedConnectionSpid, connectionWrapper.Connection.SPID);
				AssertEquals("ServerName", serverName, connectionWrapper.Connection.ServerNameReportedByDatabase);
				AssertEquals("DatabaseName", Db.DatabaseName, connectionWrapper.Connection.CurrentDatabase);
				AssertEquals("SPID same as main connection?", false, reportDb.CreatedConnectionSpid.Value == Db.Connection.SPID);
				AssertEquals("Connection use UnrestrictedWriterLogin", UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), connectionWrapper.Connection.UserLogin);
				AssertEquals("Connection must be impersonated", "User_TestNewConnectionWhenReportServerSpecified_Impersonate", currentDbUser);
			}
		}

		/// <summary>
		/// New connection to report database should be opened straight away
		/// to ensure it is respected by the report database restore process
		/// even before it's first used to render a report.
		/// </summary>
		public void TestNewConnectionReturnsOpenConnection()
		{
			var reportDb = new ReportDbForTesting(Db.ServerName, Db.DatabaseName);
			var provider = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);

			using (var testConnection = provider.GetNewConnectionWrapper(null))
			{
				AssertEquals("Connection State", ConnectionState.Open, testConnection.Connection.State);
			}
		}

		[UseSnapshotProtection]
		public void TestNewConnectionReturnsOpenConnection_Impersonate()
		{
			var reportDb = new ReportDbForTesting(Db.ServerName, Db.DatabaseName);
			var provider = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);

			using (var testAdminConnection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
			{
				testAdminConnection.ExecuteNonQuery("CREATE USER User_TestNewConnectionReturnsOpenConnection_Impersonate WITHOUT LOGIN;");
				testAdminConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER:: User_TestNewConnectionReturnsOpenConnection_Impersonate TO {UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)};");
			}

			using (var connectionWrapper = provider.GetNewConnectionWrapper("User_TestNewConnectionReturnsOpenConnection_Impersonate"))
			{
				var currentDbUser = connectionWrapper.Connection.ExecuteScalar("SELECT USER_NAME()") as string;

				AssertEquals("Connection State", ConnectionState.Open, connectionWrapper.Connection.State);
				AssertEquals("Connection use UnrestrictedWriterLogin", UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), connectionWrapper.Connection.UserLogin);
				AssertEquals("Connection must be impersonated", "User_TestNewConnectionReturnsOpenConnection_Impersonate", currentDbUser);
			}
		}

		public void TestIsReportingIsFalseWhenHosted()
		{
			var reportDb = new ReportDbForTesting("neverland", Db.DatabaseName);
			var provider = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);

			SystemDataRegistryForTest.Get().UseReportingDbServerNames = true;
			AssertEquals("Report database enabled?", true, reportDb.IsReportingDbEnabled);

			SystemDataRegistryForTest.Get().UseReportingDbServerNames = false;
			AssertEquals("Report database enabled?", false, reportDb.IsReportingDbEnabled);

			using (var newConnection = provider.GetNewConnectionWrapper(null))
			{
				AssertNotNull("Provider provides working connection to primary because reporting is disabled", newConnection);
				AssertEquals(true, newConnection.IsMainServer);
			}
		}

		[UseSnapshotProtection]
		public void TestIsReportingIsFalseWhenHosted_Impersonate()
		{
			var reportDb = new ReportDbForTesting("neverland", Db.DatabaseName);
			var provider = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);

			using (var testAdminConnection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
			{
				testAdminConnection.ExecuteNonQuery("CREATE USER User_TestIsReportingIsFalseWhenHosted_Impersonate WITHOUT LOGIN;");
				testAdminConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER:: User_TestIsReportingIsFalseWhenHosted_Impersonate TO {UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)};");
			}

			using (var connectionWrapper = provider.GetNewConnectionWrapper("User_TestIsReportingIsFalseWhenHosted_Impersonate"))
			{
				var currentDbUser = connectionWrapper.Connection.ExecuteScalar("SELECT USER_NAME()") as string;

				AssertEquals("Connection use UnrestrictedWriterLogin", UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), connectionWrapper.Connection.UserLogin);
				AssertEquals("Connection must be impersonated", "User_TestIsReportingIsFalseWhenHosted_Impersonate", currentDbUser);
			}
		}

		public void TestThrowsExceptionWhenReportServerNotSpecified()
		{
			var reportDb = new ReportDbForTesting("", Db.DatabaseName);
			var provider = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);

			AssertEquals("Report database enabled?", false, reportDb.IsReportingDbEnabled);

			using (var newConnection = provider.GetNewConnectionWrapper(null))
			{
				AssertNotNull("Provider provides working connection to primary because reporting is disabled", newConnection);
				AssertEquals(true, newConnection.IsMainServer);
			}
		}

		[UseSnapshotProtection]
		public void TestThrowsExceptionWhenReportServerNotSpecified_Impersonate()
		{
			var reportDb = new ReportDbForTesting("", Db.DatabaseName);
			var provider = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);

			using (var testAdminConnection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
			{
				testAdminConnection.ExecuteNonQuery("CREATE USER User_TestThrowsExceptionWhenReportServerNotSpecified_Impersonate WITHOUT LOGIN;");
				testAdminConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER:: User_TestThrowsExceptionWhenReportServerNotSpecified_Impersonate TO {UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)};");
			}

			using (var connectionWrapper = provider.GetNewConnectionWrapper("User_TestThrowsExceptionWhenReportServerNotSpecified_Impersonate"))
			{
				var currentDbUser = connectionWrapper.Connection.ExecuteScalar("SELECT USER_NAME()") as string;

				AssertEquals("Connection use UnrestrictedWriterLogin", UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), connectionWrapper.Connection.UserLogin);
				AssertEquals("Connection must be impersonated", "User_TestThrowsExceptionWhenReportServerNotSpecified_Impersonate", currentDbUser);
			}
		}

		public void TestGetDelayBetweenPrimaryAndReportDatabaseInMinutes()
		{
			ReportDbForTesting reportDb = new ReportDbForTesting(Db.ServerName, Db.DatabaseName);
			AssertEquals("There shouldn't be a delay if both databases are the same", 0, reportDb.GetDelayBetweenPrimaryAndReportDatabaseInMinutes());
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Invalid attempt to access the report DB when report server is not defined.")]
		public void TestGetDelayBetweenPrimaryAndReportDatabaseInMinutes_WithUnespecifiedReportServer()
		{
			var reportDb = new ReportDbForTesting("", Db.DatabaseName);
			reportDb.GetDelayBetweenPrimaryAndReportDatabaseInMinutes();
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Invalid attempt to access the report DB when report server is not defined.")]
		public void TestGetDelayBetweenPrimaryAndReportDatabaseInMinutes_WhenServerNotPartOfAlwaysOn()
		{
			var reportDb = new ReportDbForTesting("", Db.DatabaseName, false);
			reportDb.GetDelayBetweenPrimaryAndReportDatabaseInMinutes();
		}

		public void TestGetDelayBetweenPrimaryAndReportDatabaseInMinutes_WhenServerNotRespond()
		{
			var reportDb = new ReportDbForTesting("neverland", Db.DatabaseName, true);
			reportDb.ReportServerRespondTime = long.MaxValue;
			AssertEquals(Db.ServerName, reportDb.GetAnyUpToDateReportServer().ServerName);
			reportDb.ReportServerRespondTime = 0;
			AssertEquals("neverland", reportDb.GetAnyUpToDateReportServer().ServerName);
		}

		public void TestUseReportingDbServerNamesInRegistry()
		{
			SystemDataRegistryForTest.Get().ReportingDbServerNames = new string[] { Db.ServerName };
			SystemDataRegistryForTest.Get().UseReportingDbServerNames = true;
			var reportDb = new ReportDbForTestingWithoutOverrideAllReportServerNames(Array.Empty<string>(), Db.DatabaseName);
			AssertEquals("There shouldn't be a delay if both databases are the same", 0, reportDb.GetDelayBetweenPrimaryAndReportDatabaseInMinutes());
			SystemDataRegistryForTest.Get().UseReportingDbServerNames = false;
		}
	}
}
