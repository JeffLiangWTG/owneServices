using CargoWise.Application;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using Moq;
using NUnit.Framework;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	[Property("DAT:CapabilityRequirements", "SQL2019")]
	[WTG.StaticAnalysis.Annotation.CodeAlive("Test Class")]
	class SqlSecurityBuilderApplicationLoginsAndRolesTest : SqlSecurityTestFixture
	{
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestCorrectPasswordHashesAreGeneratedForApplicationLoginsWhenMissing: Hosted on CargoWise Dedicated Server")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestCorrectPasswordHashesAreGeneratedForApplicationLoginsWhenMissing: Hosted on CargoWise Shared Server")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestCorrectPasswordHashesAreGeneratedForApplicationLoginsWhenMissing: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestCorrectPasswordHashesAreGeneratedForApplicationLoginsWhenMissing: Self hosted locked")]
		public void TestCorrectPasswordHashesAreGeneratedForApplicationLoginsWhenMissing(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			AssertionsHelper.AssumePrincipalMissing(testConnection, dbReaderLogin);
			AssertionsHelper.AssumePrincipalMissing(testConnection, dbWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(testConnection, dbRestrictedReaderLogin);
			AssertionsHelper.AssumePrincipalMissing(testConnection, dbRestrictedWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(testConnection, dbUnrestrictedWriterLogin);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetServerLevelInfo(testConnection);

				// Assert
				var actualPrincipalsList = Helper.ServerPrincipalsList(testConnection, result.ProposedPrincipalsAndMemberships);

				Assert.That(actualPrincipalsList.Keys, Does.Contain(dbReaderLogin), $"'{dbReaderLogin}' must be listed");

				var dbReaderMembershipActual = actualPrincipalsList[dbReaderLogin];
				AssertionsHelper.AssertHashMatchesPassword(testConnection, $"Password hash for create must match for '{dbReaderLogin}'", testReaderCredentials.Password, dbReaderMembershipActual.passwordHash_create);
				AssertionsHelper.AssertHashMatchesPassword(testConnection, $"Password hash for alter must match for '{dbReaderLogin}'", testReaderCredentials.Password, dbReaderMembershipActual.passwordHash_alter);

				Assert.That(actualPrincipalsList.Keys, Does.Contain(dbReaderLogin), $"'{dbReaderLogin}' must be listed");

				var dbWriterMembershipActual = actualPrincipalsList[dbWriterLogin];
				AssertionsHelper.AssertHashMatchesPassword(testConnection, $"Password hash for create must match for '{dbWriterLogin}'", testWriterCredentials.Password, dbWriterMembershipActual.passwordHash_create);
				AssertionsHelper.AssertHashMatchesPassword(testConnection, $"Password hash for alter must match for '{dbWriterLogin}'", testWriterCredentials.Password, dbWriterMembershipActual.passwordHash_alter);

				Assert.That(actualPrincipalsList.Keys, Does.Contain(dbWriterLogin), $"'{dbWriterLogin}' must be listed");

				var dbRestrictedReaderMembershipActual = actualPrincipalsList[dbRestrictedReaderLogin];
				AssertionsHelper.AssertHashMatchesPassword(testConnection, $"Password hash for create must match for '{dbRestrictedReaderLogin}'", testRestrictedReaderCredentials.Password, dbRestrictedReaderMembershipActual.passwordHash_create);
				AssertionsHelper.AssertHashMatchesPassword(testConnection, $"Password hash for alter must match for '{dbRestrictedReaderLogin}'", testRestrictedReaderCredentials.Password, dbRestrictedReaderMembershipActual.passwordHash_alter);

				Assert.That(actualPrincipalsList.Keys, Does.Contain(dbRestrictedReaderLogin), $"'{dbRestrictedReaderLogin}' must be listed");

				var dbRestrictedWriterMembershipActual = actualPrincipalsList[dbRestrictedWriterLogin];
				AssertionsHelper.AssertHashMatchesPassword(testConnection, $"Password hash for create must match for '{dbRestrictedWriterLogin}'", testRestrictedWriterCredentials.Password, dbRestrictedWriterMembershipActual.passwordHash_create);
				AssertionsHelper.AssertHashMatchesPassword(testConnection, $"Password hash for alter must match for '{dbRestrictedWriterLogin}'", testRestrictedWriterCredentials.Password, dbRestrictedWriterMembershipActual.passwordHash_alter);

				Assert.That(actualPrincipalsList.Keys, Does.Contain(dbRestrictedWriterLogin), $"'{dbRestrictedWriterLogin}' must be listed");

				var dbUnrestrictedWriterMembershipActual = actualPrincipalsList[dbUnrestrictedWriterLogin];
				AssertionsHelper.AssertHashMatchesPassword(testConnection, $"Password hash for create must match for '{dbUnrestrictedWriterLogin}'", testUnrestrictedWriterCredentials.Password, dbUnrestrictedWriterMembershipActual.passwordHash_create);
				AssertionsHelper.AssertHashMatchesPassword(testConnection, $"Password hash for alter must match for '{dbUnrestrictedWriterLogin}'", testUnrestrictedWriterCredentials.Password, dbUnrestrictedWriterMembershipActual.passwordHash_alter);

				Assert.That(actualPrincipalsList.Keys, Does.Contain(dbUnrestrictedWriterLogin), $"'{dbUnrestrictedWriterLogin}' must be listed");
			}
		}

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestIdenticalPasswordHashesAreGeneratedForApplicationLoginsWithCorrectSids: Hosted on CargoWise Dedicated Server")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestIdenticalPasswordHashesAreGeneratedForApplicationLoginsWithCorrectSids: Hosted on CargoWise Shared Server")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestIdenticalPasswordHashesAreGeneratedForApplicationLoginsWithCorrectSids: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestIdenticalPasswordHashesAreGeneratedForApplicationLoginsWithCorrectSids: Self hosted locked")]
		public void TestIdenticalPasswordHashesAreGeneratedForApplicationLoginsWithCorrectSids(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());
				testConnection.ExecuteNonQuery($@"
CREATE LOGIN [{dbReaderLogin}] WITH PASSWORD = N'{testReaderCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbReaderLogin)};
CREATE LOGIN [{dbWriterLogin}] WITH PASSWORD = N'{testWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbWriterLogin)};
CREATE LOGIN [{dbRestrictedReaderLogin}] WITH PASSWORD = N'{testRestrictedReaderCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedReaderLogin)};
CREATE LOGIN [{dbRestrictedWriterLogin}] WITH PASSWORD = N'{testRestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedWriterLogin)};
CREATE LOGIN [{dbUnrestrictedWriterLogin}] WITH PASSWORD = N'{testUnrestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbUnrestrictedWriterLogin)};
");

				AssertionsHelper.AssumeLoginHashMatchesPassword(testConnection, $"Password hash for login should match expected password for login '{dbReaderLogin}'", dbReaderLogin, testReaderCredentials.Password);
				AssertionsHelper.AssumeLoginHashMatchesPassword(testConnection, $"Password hash for login should match expected password for login '{dbWriterLogin}'", dbWriterLogin, testWriterCredentials.Password);
				AssertionsHelper.AssumeLoginHashMatchesPassword(testConnection, $"Password hash for login should match expected password for login '{dbRestrictedReaderLogin}'", dbRestrictedReaderLogin, testRestrictedReaderCredentials.Password);
				AssertionsHelper.AssumeLoginHashMatchesPassword(testConnection, $"Password hash for login should match expected password for login '{dbRestrictedWriterLogin}'", dbRestrictedWriterLogin, testRestrictedWriterCredentials.Password);
				AssertionsHelper.AssumeLoginHashMatchesPassword(testConnection, $"Password hash for login should match expected password for login '{dbUnrestrictedWriterLogin}'", dbUnrestrictedWriterLogin, testUnrestrictedWriterCredentials.Password);

				// Act
				var result = sqlSecurityBuilder.GetServerLevelInfo(testConnection);

				// Assert
				var actualPrincipalsList = Helper.ServerPrincipalsList(testConnection, result.ProposedPrincipalsAndMemberships);

				Assert.That(actualPrincipalsList.Keys, Does.Contain(dbReaderLogin), $"'{dbReaderLogin}' must be listed");

				var dbReaderMembershipActual = actualPrincipalsList[dbReaderLogin];
				AssertionsHelper.AssertHashMatchesLoginPasswordHash(testConnection, $"Password hash for create must match existing hash for login '{dbReaderLogin}'", dbReaderLogin, dbReaderMembershipActual.passwordHash_create);
				AssertionsHelper.AssertHashMatchesLoginPasswordHash(testConnection, $"Password hash for alter must match existing hash for login '{dbReaderLogin}'", dbReaderLogin, dbReaderMembershipActual.passwordHash_alter);

				Assert.That(actualPrincipalsList.Keys, Does.Contain(dbWriterLogin), $"'{dbReaderLogin}' must be listed");

				var dbWriterMembershipActual = actualPrincipalsList[dbWriterLogin];
				AssertionsHelper.AssertHashMatchesLoginPasswordHash(testConnection, $"Password hash for create must match existing hash for login '{dbWriterLogin}'", dbWriterLogin, dbWriterMembershipActual.passwordHash_create);
				AssertionsHelper.AssertHashMatchesLoginPasswordHash(testConnection, $"Password hash for alter must match existing hash for login '{dbWriterLogin}'", dbWriterLogin, dbWriterMembershipActual.passwordHash_alter);

				Assert.That(actualPrincipalsList.Keys, Does.Contain(dbRestrictedReaderLogin), $"'{dbRestrictedReaderLogin}' must be listed");

				var dbRestrictedReaderMembershipActual = actualPrincipalsList[dbRestrictedReaderLogin];
				AssertionsHelper.AssertHashMatchesLoginPasswordHash(testConnection, $"Password hash for create must match existing hash for login '{dbRestrictedReaderLogin}'", dbRestrictedReaderLogin, dbRestrictedReaderMembershipActual.passwordHash_create);
				AssertionsHelper.AssertHashMatchesLoginPasswordHash(testConnection, $"Password hash for alter must match existing hash for login '{dbRestrictedReaderLogin}'", dbRestrictedReaderLogin, dbRestrictedReaderMembershipActual.passwordHash_alter);

				Assert.That(actualPrincipalsList.Keys, Does.Contain(dbRestrictedWriterLogin), $"'{dbRestrictedWriterLogin}' must be listed");

				var dbRestrictedWriterMembershipActual = actualPrincipalsList[dbRestrictedWriterLogin];
				AssertionsHelper.AssertHashMatchesLoginPasswordHash(testConnection, $"Password hash for create must match existing hash for login '{dbRestrictedWriterLogin}'", dbRestrictedWriterLogin, dbRestrictedWriterMembershipActual.passwordHash_create);
				AssertionsHelper.AssertHashMatchesLoginPasswordHash(testConnection, $"Password hash for alter must match existing hash for login '{dbRestrictedWriterLogin}'", dbRestrictedWriterLogin, dbRestrictedWriterMembershipActual.passwordHash_alter);

				Assert.That(actualPrincipalsList.Keys, Does.Contain(dbUnrestrictedWriterLogin), $"'{dbUnrestrictedWriterLogin}' must be listed");

				var dbUnrestrictedWriterMembershipActual = actualPrincipalsList[dbUnrestrictedWriterLogin];
				AssertionsHelper.AssertHashMatchesLoginPasswordHash(testConnection, $"Password hash for create must match existing hash for login '{dbUnrestrictedWriterLogin}'", dbUnrestrictedWriterLogin, dbUnrestrictedWriterMembershipActual.passwordHash_create);
				AssertionsHelper.AssertHashMatchesLoginPasswordHash(testConnection, $"Password hash for alter must match existing hash for login '{dbUnrestrictedWriterLogin}'", dbUnrestrictedWriterLogin, dbUnrestrictedWriterMembershipActual.passwordHash_alter);
			}
		}

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestCorrectProposedPrincipalsAndMembershipsListIsGeneratedForApplicationLogins: Hosted on CargoWise Dedicated Server")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestCorrectProposedPrincipalsAndMembershipsListIsGeneratedForApplicationLogins: Hosted on CargoWise Shared Server")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestCorrectProposedPrincipalsAndMembershipsListIsGeneratedForApplicationLogins: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestCorrectProposedPrincipalsAndMembershipsListIsGeneratedForApplicationLogins: Self hosted locked")]
		public void TestCorrectProposedPrincipalsAndMembershipsListIsGeneratedForApplicationLogins(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var expectedPrincipalsAndMemberships =
			$@"-- Expected principals and memberships
SELECT * FROM (VALUES
		(N'{dbReaderLogin}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}', N'us_english', PWDENCRYPT(N'{testReaderCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(dbReaderLogin)}),
		(N'{dbWriterLogin}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}', N'us_english', PWDENCRYPT(N'{testWriterCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(dbWriterLogin)}),
		(N'{dbRestrictedReaderLogin}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}', N'us_english', PWDENCRYPT(N'{testRestrictedWriterCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedReaderLogin)}),
		(N'{dbRestrictedWriterLogin}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}', N'us_english', PWDENCRYPT(N'{testRestrictedWriterCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedWriterLogin)}),
		(N'{dbUnrestrictedWriterLogin}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}', N'us_english', PWDENCRYPT(N'{testUnrestrictedWriterCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(dbUnrestrictedWriterLogin)})
) AS PER (member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
";

			var expectedPrincipalsList = Helper.ServerPrincipalsList(testConnection, expectedPrincipalsAndMemberships);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetServerLevelInfo(testConnection);
				var actualPrincipalsList = Helper.ServerPrincipalsList(testConnection, result.ProposedPrincipalsAndMemberships);

				// Assert

				Assert.That(actualPrincipalsList.Count, Is.EqualTo(expectedPrincipalsList.Count), "Number of expected and actual principals and memberships should be the same.");

				foreach (var actualPrincipalMembership in actualPrincipalsList.Values)
				{
					Assert.That(expectedPrincipalsList.Keys, Does.Contain(actualPrincipalMembership.member_name + actualPrincipalMembership.parent_role), "Actual principal membership must be present in the expected list.");

					var expectedPrincipalMembership = expectedPrincipalsList[actualPrincipalMembership.member_name + actualPrincipalMembership.parent_role];
					Assert.That(actualPrincipalMembership.sid, Is.EqualTo(expectedPrincipalMembership.sid), $"Expected and actual 'sid' for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.member_type, Is.EqualTo(expectedPrincipalMembership.member_type).IgnoreCase, $"Expected and actual 'member type' for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.default_language, Is.EqualTo(expectedPrincipalMembership.default_language).IgnoreCase, $"Expected and actual 'default language' for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.default_database, Is.EqualTo(expectedPrincipalMembership.default_database).IgnoreCase, $"Expected and actual 'default database name' for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.is_expiration_checked, Is.EqualTo(expectedPrincipalMembership.is_expiration_checked), $"Expected and actual 'is expiration checked' flag for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.is_policy_checked, Is.EqualTo(expectedPrincipalMembership.is_policy_checked), $"Expected and actual 'is policy checked' flag for '{actualPrincipalMembership.member_name}' must be identical");
				}
			}
		}

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestCorrectProposedPermissionsListIsGeneratedForApplicationLogins: Hosted on CargoWise Dedicated Server")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestCorrectProposedPermissionsListIsGeneratedForApplicationLogins: Hosted on CargoWise Shared Server")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestCorrectProposedPermissionsListIsGeneratedForApplicationLogins: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestCorrectProposedPermissionsListIsGeneratedForApplicationLogins: Self hosted locked")]
		public void TestCorrectProposedPermissionsListIsGeneratedForApplicationLogins(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var expectedPermissions =
			$@"
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{dbReaderLogin}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{dbWriterLogin}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{dbRestrictedReaderLogin}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{dbRestrictedWriterLogin}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{dbUnrestrictedWriterLogin}', N'')
 ) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
";
			var expectedPermissionsList = Helper.ServerPermissionsList(testConnection, expectedPermissions);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetServerLevelInfo(testConnection);
				var actualPermissionsList = Helper.ServerPermissionsList(testConnection, result.ProposedPermissions);

				// Assert
				Assert.That(actualPermissionsList, Is.EqualTo(expectedPermissionsList).IgnoreCase, "Expected and actual permissions for application logins must be the same.");
			}
		}

		#region Implementation

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

		#endregion Implementation
	}
}

