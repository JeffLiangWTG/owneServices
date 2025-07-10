using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using Moq;
using NUnit.Framework;
using IntegrationLogging = Enterprise.Integration;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	[Property("DAT:CapabilityRequirements", "SQL2019")]
	class SqlSecurityManagerApplicationLoginsAndRolesTest : SqlSecurityTestFixture
	{
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestApplicationLoginsAreCreatedWithCorrectPermissionsAndRoles: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestApplicationLoginsAreCreatedWithCorrectPermissionsAndRoles: Self hosted locked")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestApplicationLoginsAreCreatedWithCorrectPermissionsAndRoles: Shared Wise Cloud shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestApplicationLoginsAreCreatedWithCorrectPermissionsAndRoles: Shared Wise Cloud dedicated")]
		public void TestApplicationLoginsAreCreatedWithCorrectPermissionsAndRoles(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			AssertionsHelper.AssumePrincipalMissing(testConnection, dbReaderLogin);
			AssertionsHelper.AssumePrincipalMissing(testConnection, dbWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(testConnection, dbRestrictedReaderLogin);
			AssertionsHelper.AssumePrincipalMissing(testConnection, dbRestrictedWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(testConnection, dbUnrestrictedWriterLogin);

			// Act
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				sqlSecurityManager.BuildServerSecurity(testConnection, trialRun: true);
				AssertionsHelper.AssertPrincipalMissing(testConnection, dbReaderLogin);
				AssertionsHelper.AssertPrincipalMissing(testConnection, dbWriterLogin);
				AssertionsHelper.AssertPrincipalMissing(testConnection, dbRestrictedReaderLogin);
				AssertionsHelper.AssertPrincipalMissing(testConnection, dbRestrictedWriterLogin);
				AssertionsHelper.AssertPrincipalMissing(testConnection, dbUnrestrictedWriterLogin);

				sqlSecurityManager.BuildServerSecurity(testConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(testConnection, dbReaderLogin, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(testConnection, dbReaderLogin);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(testConnection, dbReaderLogin, new Dictionary<string, string[]>() { { "G", new[] { "CONNECT SQL" } } });
				AssertionsHelper.AssertLoginHashMatchesPassword(testConnection, $"Login '{dbReaderLogin}' should have correct password.", dbReaderLogin, testReaderCredentials.Password);
				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(testConnection, dbReaderLogin, Db.SqlMasterDb);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(testConnection, dbReaderLogin);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(testConnection, dbReaderLogin);
				AssertionsHelper.AssertSqlLoginLanguageEquals(testConnection, dbReaderLogin, "us_english");
				AssertionsHelper.AssertSqlLoginSidEquals(testConnection, dbReaderLogin, SqlServerLoginUtilities.ComputeSqlLoginSid(dbReaderLogin));

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(testConnection, dbWriterLogin, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(testConnection, dbWriterLogin);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(testConnection, dbWriterLogin, new Dictionary<string, string[]>() { { "G", new[] { "CONNECT SQL" } } });
				AssertionsHelper.AssertLoginHashMatchesPassword(testConnection, $"Login '{dbWriterLogin}' should have correct password.", dbWriterLogin, testWriterCredentials.Password);
				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(testConnection, dbWriterLogin, Db.SqlMasterDb);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(testConnection, dbWriterLogin);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(testConnection, dbWriterLogin);
				AssertionsHelper.AssertSqlLoginLanguageEquals(testConnection, dbWriterLogin, "us_english");
				AssertionsHelper.AssertSqlLoginSidEquals(testConnection, dbWriterLogin, SqlServerLoginUtilities.ComputeSqlLoginSid(dbWriterLogin));

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(testConnection, dbRestrictedReaderLogin, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(testConnection, dbRestrictedReaderLogin);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(testConnection, dbRestrictedReaderLogin, new Dictionary<string, string[]>() { { "G", new[] { "CONNECT SQL" } } });
				AssertionsHelper.AssertLoginHashMatchesPassword(testConnection, $"Login '{dbRestrictedReaderLogin}' should have correct password.", dbRestrictedReaderLogin, testRestrictedReaderCredentials.Password);
				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(testConnection, dbRestrictedReaderLogin, Db.SqlMasterDb);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(testConnection, dbRestrictedReaderLogin);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(testConnection, dbRestrictedReaderLogin);
				AssertionsHelper.AssertSqlLoginLanguageEquals(testConnection, dbRestrictedReaderLogin, "us_english");
				AssertionsHelper.AssertSqlLoginSidEquals(testConnection, dbRestrictedReaderLogin, SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedReaderLogin));

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(testConnection, dbRestrictedWriterLogin, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(testConnection, dbRestrictedWriterLogin);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(testConnection, dbRestrictedWriterLogin, new Dictionary<string, string[]>() { { "G", new[] { "CONNECT SQL" } } });
				AssertionsHelper.AssertLoginHashMatchesPassword(testConnection, $"Login '{dbRestrictedWriterLogin}' should have correct password.", dbRestrictedWriterLogin, testRestrictedWriterCredentials.Password);
				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(testConnection, dbRestrictedWriterLogin, Db.SqlMasterDb);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(testConnection, dbRestrictedWriterLogin);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(testConnection, dbRestrictedWriterLogin);
				AssertionsHelper.AssertSqlLoginLanguageEquals(testConnection, dbRestrictedWriterLogin, "us_english");
				AssertionsHelper.AssertSqlLoginSidEquals(testConnection, dbRestrictedWriterLogin, SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedWriterLogin));

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(testConnection, dbUnrestrictedWriterLogin, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(testConnection, dbUnrestrictedWriterLogin);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(testConnection, dbUnrestrictedWriterLogin, new Dictionary<string, string[]>() { { "G", new[] { "CONNECT SQL" } } });
				AssertionsHelper.AssertLoginHashMatchesPassword(testConnection, $"Login '{dbUnrestrictedWriterLogin}' should have correct password.", dbUnrestrictedWriterLogin, testUnrestrictedWriterCredentials.Password);
				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(testConnection, dbUnrestrictedWriterLogin, Db.SqlMasterDb);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(testConnection, dbUnrestrictedWriterLogin);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(testConnection, dbUnrestrictedWriterLogin);
				AssertionsHelper.AssertSqlLoginLanguageEquals(testConnection, dbUnrestrictedWriterLogin, "us_english");
				AssertionsHelper.AssertSqlLoginSidEquals(testConnection, dbUnrestrictedWriterLogin, SqlServerLoginUtilities.ComputeSqlLoginSid(dbUnrestrictedWriterLogin));
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestLoginsStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreDropped: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestLoginsStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreDropped: Self hosted locked")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestLoginsStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreDropped: Shared Wise Cloud shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestLoginsStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreDropped: Shared Wise Cloud dedicated")]
		public void TestLoginsStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreDropped(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			try
			{
				if (!Helper.ServerPrincipalExists(testConnection, $"{Db.DatabaseName}_LoginWithUnderscoreToDrop", "S"))
				{
					testConnection.ExecuteNonQuery($@"
CREATE LOGIN [{Db.DatabaseName}_LoginWithUnderscoreToDrop] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
");
				}

				if (!Helper.ServerPrincipalExists(testConnection, $"{Db.DatabaseName}LoginWithoutUnderscoreToKeep", "S"))
				{
					testConnection.ExecuteNonQuery($@"
CREATE LOGIN [{Db.DatabaseName}LoginWithoutUnderscoreToKeep] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
");
				}

				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(testConnection, $"{Db.DatabaseName}_LoginWithUnderscoreToDrop", "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(testConnection, $"{Db.DatabaseName}LoginWithoutUnderscoreToKeep", "S");

				// Act
				using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
				using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
				{
					sqlSecurityManager.BuildServerSecurity(testConnection, trialRun: true);
					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(testConnection, $"{Db.DatabaseName}_LoginWithUnderscoreToDrop", "S");
					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(testConnection, $"{Db.DatabaseName}LoginWithoutUnderscoreToKeep", "S");

					sqlSecurityManager.BuildServerSecurity(testConnection, trialRun: false);

					// Assert
					AssertionsHelper.AssertPrincipalMissing(testConnection, $"{Db.DatabaseName}_LoginWithUnderscoreToDrop");
					AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(testConnection, $"{Db.DatabaseName}LoginWithoutUnderscoreToKeep", "S");
				}
			}
			finally
			{
				Helper.DropLoginIfExists(testConnection, $"{Db.DatabaseName}_LoginWithUnderscoreToDrop", "S");
				Helper.DropLoginIfExists(testConnection, $"{Db.DatabaseName}LoginWithoutUnderscoreToKeep", "S");
			}
		}

		#region Setup

		readonly string dbReaderLogin = CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbWriterLogin = CargoWiseWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedReaderLogin = RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedWriterLogin = RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbUnrestrictedWriterLogin = UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);

		[SetUp]
		public void SetUp()
		{
			testConnection = Db.NewAdminConnection();
			Helper.DropServerTestEntities(testConnection, Db.DatabaseName);
		}

		[TearDown]
		public void TearDown()
		{
			Helper.DropServerTestEntities(testConnection, Db.DatabaseName);

			testConnection.Dispose();
		}

		AdminConnection testConnection;

		#endregion Setup
	}
}
