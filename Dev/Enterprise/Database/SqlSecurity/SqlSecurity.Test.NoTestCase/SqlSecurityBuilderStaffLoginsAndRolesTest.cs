using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	[Property("DAT:CapabilityRequirements", "SQL2019")]
	class SqlSecurityBuilderStaffLoginsAndRolesTest : SqlSecurityTestFixture
	{
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestIdenticalSidsAreGeneratedForStaffSqlLogins: Hosted on CargoWise Dedicated Server")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestIdenticalSidsAreGeneratedForStaffSqlLogins: Hosted on CargoWise Shared Server")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestIdenticalSidsAreGeneratedForStaffSqlLogins: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestIdenticalSidsAreGeneratedForStaffSqlLogins: Self hosted locked")]
		public void TestIdenticalSidsAreGeneratedForStaffSqlLogins(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var staffInfoProviderMock = new Mock<IStaffInfoProvider>();
			var staffInfoCollection = new List<DbUserManager.StaffLoginInfo>();
			staffInfoProviderMock
				.Setup(provider => provider.GetStaffLoginsInfo())
				.Returns(staffInfoCollection);

			var staffSqlLoginInfo = new DbUserManager.StaffLoginInfo()
			{
				LoginName = Helper.GetEnterpriseLoginFullName("Bob1", Db.DatabaseName),
				StaffDatabaseAccessGroupRoles = new HashSet<string>() { "test_role" },
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				HashedPassword = "0X0200A7945F61A9BD3B168BC5A2D132F5151A12F927FC06D0B04C18AA515EC62B3B093F886D5D474AFE9202F3F875A6460B06BDBA1F8670A20FE29EB0D6CBED71773B629ED053",
			};

			staffInfoCollection.Add(staffSqlLoginInfo);

			adminConnetion.ExecuteNonQuery($@"
CREATE LOGIN [{staffSqlLoginInfo.LoginName}] WITH PASSWORD = N'SOME[]pasword123';
");
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, staffInfoProviderMock.Object);

				var existingSqlLoginSid = string.Empty;

				using (var command = adminConnetion.Command($"SELECT sid FROM sys.sql_logins WHERE name = @loginName"))
				{
					command.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, staffSqlLoginInfo.LoginName);
					existingSqlLoginSid = DataUtils.BytesToHexString((byte[])command.ExecuteScalar());
				}

				// Act
				var result = securityBuilder.GetServerLevelInfo(adminConnetion);
				var actualPrincipalsList = Helper.ServerPrincipalsList(adminConnetion, result.ProposedPrincipalsAndMemberships);

				// Assert
				Assert.That(actualPrincipalsList.Keys, Does.Contain(staffSqlLoginInfo.LoginName), $"'{staffSqlLoginInfo.LoginName}' with no roles must be listed.");

				AssertionsHelper.AssertSqlLoginSidEquals(adminConnetion, staffSqlLoginInfo.LoginName, existingSqlLoginSid);
			}
		}

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestPasswordHashesForMissingStaffLogin: Hosted on CargoWise Dedicated Server")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestPasswordHashesForMissingStaffLogin: Hosted on CargoWise Shared Server")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestPasswordHashesForMissingStaffLogin: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestPasswordHashesForMissingStaffLogin: Self hosted locked")]
		public void TestPasswordHashesForMissingStaffLogin(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var staffInfoProviderMock = new Mock<IStaffInfoProvider>();
			var staffInfoCollection = new List<DbUserManager.StaffLoginInfo>();
			staffInfoProviderMock
				.Setup(provider => provider.GetStaffLoginsInfo())
				.Returns(staffInfoCollection);

			var staffSqlLoginInfo = new DbUserManager.StaffLoginInfo()
			{
				LoginName = Helper.GetEnterpriseLoginFullName("Bob1", Db.DatabaseName),
				StaffDatabaseAccessGroupRoles = new HashSet<string>() { "test_role" },
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				HashedPassword = "0X0200A7945F61A9BD3B168BC5A2D132F5151A12F927FC06D0B04C18AA515EC62B3B093F886D5D474AFE9202F3F875A6460B06BDBA1F8670A20FE29EB0D6CBED71773B629ED053",
			};

			staffInfoCollection.Add(staffSqlLoginInfo);

			adminConnetion.ExecuteNonQuery($@"
CREATE LOGIN [{staffSqlLoginInfo.LoginName}] WITH PASSWORD = N'SOME[]pasword123';
");
			var existingSqlLoginPasswordHash = string.Empty;
			using (var command = adminConnetion.Command($"SELECT password_hash FROM sys.sql_logins WHERE name = @loginName"))
			{
				command.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, staffSqlLoginInfo.LoginName);
				existingSqlLoginPasswordHash = DataUtils.BytesToHexString((byte[])command.ExecuteScalar());
			}

			Assume.That(existingSqlLoginPasswordHash, Is.Not.EqualTo(staffSqlLoginInfo).IgnoreCase);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, staffInfoProviderMock.Object);

				// Act
				var result = securityBuilder.GetServerLevelInfo(adminConnetion);
				var actualPrincipalsList = Helper.ServerPrincipalsList(adminConnetion, result.ProposedPrincipalsAndMemberships);

				// Assert
				Assert.That(actualPrincipalsList.Keys, Does.Contain(staffSqlLoginInfo.LoginName), $"'{staffSqlLoginInfo.LoginName}' with no roles must be listed.");

				Assert.That(actualPrincipalsList[staffSqlLoginInfo.LoginName].passwordHash_create, Is.EqualTo(Helper.HexStringToBytes(staffSqlLoginInfo.HashedPassword)), $"Password hash for create must match staff info PasswordHash for '{staffSqlLoginInfo.LoginName}'");
				Assert.That(actualPrincipalsList[staffSqlLoginInfo.LoginName].passwordHash_alter, Is.EqualTo(Helper.HexStringToBytes(staffSqlLoginInfo.HashedPassword)), $"Password hash for alter must match staff info PasswordHash for '{staffSqlLoginInfo.LoginName}'");
			}
		}

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestPasswordHashesForExistingStaffLogin: Hosted on CargoWise Dedicated Server")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestPasswordHashesForExistingStaffLogin: Hosted on CargoWise Shared Server")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestPasswordHashesForExistingStaffLogin: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestPasswordHashesForExistingStaffLogin: Self hosted locked")]
		public void TestPasswordHashesForExistingStaffLogin(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var staffInfoProviderMock = new Mock<IStaffInfoProvider>();
			var staffInfoCollection = new List<DbUserManager.StaffLoginInfo>();
			staffInfoProviderMock
				.Setup(provider => provider.GetStaffLoginsInfo())
				.Returns(staffInfoCollection);

			var staffSqlLoginInfo = new DbUserManager.StaffLoginInfo()
			{
				LoginName = Helper.GetEnterpriseLoginFullName("Bob1", Db.DatabaseName),
				StaffDatabaseAccessGroupRoles = new HashSet<string>() { "test_role" },
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				HashedPassword = "0X0200A7945F61A9BD3B168BC5A2D132F5151A12F927FC06D0B04C18AA515EC62B3B093F886D5D474AFE9202F3F875A6460B06BDBA1F8670A20FE29EB0D6CBED71773B629ED053",
			};

			staffInfoCollection.Add(staffSqlLoginInfo);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, staffInfoProviderMock.Object);

				// Act
				var result = securityBuilder.GetServerLevelInfo(adminConnetion);
				var actualPrincipalsList = Helper.ServerPrincipalsList(adminConnetion, result.ProposedPrincipalsAndMemberships);

				// Assert
				Assert.That(actualPrincipalsList.Keys, Does.Contain(staffSqlLoginInfo.LoginName), $"'{staffSqlLoginInfo.LoginName}' with no roles must be listed.");

				Assert.That(actualPrincipalsList[staffSqlLoginInfo.LoginName].passwordHash_create, Is.EqualTo(Helper.HexStringToBytes(staffSqlLoginInfo.HashedPassword)), $"Password hash for create must match staff info PasswordHash for '{staffSqlLoginInfo.LoginName}'");
				Assert.That(actualPrincipalsList[staffSqlLoginInfo.LoginName].passwordHash_alter, Is.EqualTo(Helper.HexStringToBytes(staffSqlLoginInfo.HashedPassword)), $"Password hash for alter must match staff info PasswordHash for '{staffSqlLoginInfo.LoginName}'");
			}
		}

		[TestCaseSource(nameof(ProposedPrincipalsAndMembershipsForStaffLoginsTestCaseSource))]
		public void TestCorrectProposedPrincipalsAndMembershipsListIsGeneratedForStaffLogins(DatabaseTestMode databaseTestMode, string expectedStaffPrincipalsAndMemberships, IStaffInfoProvider staffInfoProvider)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var expectedApplicationLoginsPrincipalsAndMemberships =
				$@"-- Expected principals and memberships
SELECT * FROM (VALUES
		(N'{dbReaderLogin}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}', N'us_english', PWDENCRYPT(N'{testReaderCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(dbReaderLogin)}),
		(N'{dbWriterLogin}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}', N'us_english', PWDENCRYPT(N'{testWriterCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(dbWriterLogin)}),
		(N'{dbRestrictedReaderLogin}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}', N'us_english', PWDENCRYPT(N'{testRestrictedWriterCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedReaderLogin)}),
		(N'{dbRestrictedWriterLogin}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}', N'us_english', PWDENCRYPT(N'{testRestrictedWriterCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedWriterLogin)}),
		(N'{dbUnrestrictedWriterLogin}', 'S', N'', 0, 0, N'{Db.SqlMasterDb}', N'us_english', PWDENCRYPT(N'{testUnrestrictedWriterCredentials.Password}'), NULL, {SqlServerLoginUtilities.ComputeSqlLoginSid(dbUnrestrictedWriterLogin)})
) AS PER (member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
";
			var expectedApplicationsPrincipalsAndMembershipList = Helper.ServerPrincipalsList(adminConnetion, expectedApplicationLoginsPrincipalsAndMemberships);
			var expectedStaffPrincipalsList = Helper.ServerPrincipalsList(adminConnetion, expectedStaffPrincipalsAndMemberships);
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, staffInfoProvider);

				// Act
				var result = securityBuilder.GetServerLevelInfo(adminConnetion);
				var actualPrincipalsList = Helper.ServerPrincipalsList(adminConnetion, result.ProposedPrincipalsAndMemberships);

				// Assert
				Assert.That(actualPrincipalsList.Count, Is.EqualTo(expectedStaffPrincipalsList.Count + expectedApplicationsPrincipalsAndMembershipList.Count), "Number of expected and actual principals and memberships should be the same.");

				foreach (var expectedPrincipalMembership in expectedApplicationsPrincipalsAndMembershipList.Values)
				{
					Assert.That(actualPrincipalsList.Keys, Does.Contain(expectedPrincipalMembership.member_name + expectedPrincipalMembership.parent_role).IgnoreCase, "Actual principal membership must be present in the expected list.");

					var actualPrincipalMembership = actualPrincipalsList[expectedPrincipalMembership.member_name + expectedPrincipalMembership.parent_role];

					Assert.That(actualPrincipalMembership.sid, Is.EqualTo(expectedPrincipalMembership.sid), $"Expected and actual 'sid' for '{actualPrincipalMembership.member_name}' must be identical");

					Assert.That(actualPrincipalMembership.member_type, Is.EqualTo(expectedPrincipalMembership.member_type).IgnoreCase, $"Expected and actual 'member type' for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.default_language, Is.EqualTo(expectedPrincipalMembership.default_language).IgnoreCase, $"Expected and actual 'default language' for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.default_database, Is.EqualTo(expectedPrincipalMembership.default_database).IgnoreCase, $"Expected and actual 'default database name' for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.is_expiration_checked, Is.EqualTo(expectedPrincipalMembership.is_expiration_checked), $"Expected and actual 'is expiration checked' flag for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.is_policy_checked, Is.EqualTo(expectedPrincipalMembership.is_policy_checked), $"Expected and actual 'is policy checked' flag for '{actualPrincipalMembership.member_name}' must be identical");
				}

				foreach (var expectedPrincipalMembership in expectedStaffPrincipalsList.Values)
				{
					Assert.That(actualPrincipalsList.Keys, Does.Contain(expectedPrincipalMembership.member_name + expectedPrincipalMembership.parent_role).IgnoreCase, "Actual principal membership must be present in the expected list.");

					var actualPrincipalMembership = actualPrincipalsList[expectedPrincipalMembership.member_name + expectedPrincipalMembership.parent_role];

					Assert.That(actualPrincipalMembership.member_type, Is.EqualTo(expectedPrincipalMembership.member_type).IgnoreCase, $"Expected and actual 'member type' for '{actualPrincipalMembership.member_name}' must be identical");

					Assert.That(
						actualPrincipalMembership.sid is null,
						Is.True,
						$"Actual sid must be null for missing staff member login '{actualPrincipalMembership.member_name}'.");

					Assert.That(actualPrincipalMembership.default_language, Is.EqualTo(expectedPrincipalMembership.default_language).IgnoreCase, $"Expected and actual 'default language' for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.default_database, Is.EqualTo(expectedPrincipalMembership.default_database).IgnoreCase, $"Expected and actual 'default database name' for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.is_expiration_checked, Is.EqualTo(expectedPrincipalMembership.is_expiration_checked), $"Expected and actual 'is expiration checked' flag for '{actualPrincipalMembership.member_name}' must be identical");
					Assert.That(actualPrincipalMembership.is_policy_checked, Is.EqualTo(expectedPrincipalMembership.is_policy_checked), $"Expected and actual 'is policy checked' flag for '{actualPrincipalMembership.member_name}' must be identical");
				}
			}
		}

		[TestCaseSource(nameof(ProposedPermissionsForStaffLoginsTestCaseSource))]
		public void TestCorrectPropsedPermissionsListIsGeneratedForStaffLogins(DatabaseTestMode databaseTestMode, string expectedStaffPermissions, IStaffInfoProvider staffInfoProvider)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var expectedApplicationLoginsPermissions =
			$@"
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{dbReaderLogin}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{dbWriterLogin}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{dbRestrictedReaderLogin}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{dbRestrictedWriterLogin}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{dbUnrestrictedWriterLogin}', N'')
 ) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
";
			var expectedApplicationsPermissionsList = Helper.ServerPermissionsList(adminConnetion, expectedApplicationLoginsPermissions);
			var expectedStaffPermissionsList = Helper.ServerPermissionsList(adminConnetion, expectedStaffPermissions);
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, staffInfoProvider);

				// Act
				var result = securityBuilder.GetServerLevelInfo(adminConnetion);
				var actualPermissionsList = Helper.ServerPermissionsList(adminConnetion, result.ProposedPermissions);

				// Assert
				Assert.That(actualPermissionsList, Is.EquivalentTo(expectedApplicationsPermissionsList.Concat(expectedStaffPermissionsList)).IgnoreCase, "Expected and actual permissions for application and staff logins must be the same.");
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
			adminConnetion = Db.NewAdminConnection(System.Environment.MachineName, Db.SqlMasterDb);
			Helper.DropServerTestEntities(adminConnetion, Db.DatabaseName);
		}

		[TearDown]
		public void TearDown()
		{
			Helper.DropServerTestEntities(adminConnetion, Db.DatabaseName);
			adminConnetion.Dispose();
		}

		AdminConnection adminConnetion;

		static IEnumerable<TestCaseData> ProposedPrincipalsAndMembershipsForStaffLoginsTestCaseSource()
		{
			var staffInfoCollection = new List<DbUserManager.StaffLoginInfo>();

			var staffInfoReaderWindowsAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
				LoginName = @"Domain\BobReader",
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwRestrictedReaderRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffInfoDeveloperWindowsAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
				LoginName = @"Domain\BobDeveloper",
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbDataWriterRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffInfoBackupOperatorWindowsAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
				LoginName = @"Domain\BobBackupOperator",
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbBackupOperatorRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
				LoginName = @"Domain\BobDeveloperBackupOperatorReaderHrmUser",
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwHRMStaffRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffWithRandomRoleWindowsAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
				LoginName = @"Domain\BobRandomRole",
				StaffDatabaseAccessGroupRoles = new HashSet<string> { "SomeRandomRole" },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffInfoReaderSqlAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				LoginName = Helper.GetEnterpriseLoginFullName("BobReader", Helper.MainDatabaseNameOutsideTestCase),
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwRestrictedReaderRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffInfoDeveloperSqlAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				LoginName = Helper.GetEnterpriseLoginFullName("BobDeveloper", Helper.MainDatabaseNameOutsideTestCase),
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbDataWriterRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffInfoBackupOperatorSqlAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				LoginName = Helper.GetEnterpriseLoginFullName("BobBackupOperator", Helper.MainDatabaseNameOutsideTestCase),
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbBackupOperatorRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				LoginName = Helper.GetEnterpriseLoginFullName("BobDeveloperBackupOperatorReaderHrmUser", Helper.MainDatabaseNameOutsideTestCase),
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwHRMStaffRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffWithRandomRoleSqlAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				LoginName = Helper.GetEnterpriseLoginFullName("BobRandomRole", Helper.MainDatabaseNameOutsideTestCase),
				StaffDatabaseAccessGroupRoles = new HashSet<string> { "SomeRandomRole" },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			staffInfoCollection.Add(staffInfoReaderWindowsAuthentication);
			staffInfoCollection.Add(staffInfoDeveloperWindowsAuthentication);
			staffInfoCollection.Add(staffInfoBackupOperatorWindowsAuthentication);
			staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication);
			staffInfoCollection.Add(staffWithRandomRoleWindowsAuthentication);

			staffInfoCollection.Add(staffInfoReaderSqlAuthentication);
			staffInfoCollection.Add(staffInfoDeveloperSqlAuthentication);
			staffInfoCollection.Add(staffInfoBackupOperatorSqlAuthentication);
			staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication);
			staffInfoCollection.Add(staffWithRandomRoleSqlAuthentication);

			foreach (DatabaseTestMode databaseTestMode in Enum.GetValues(typeof(DatabaseTestMode)))
			{
				yield return new TestCaseData(
					databaseTestMode,
					$@"-- Expected staff principals and memberships
SELECT * FROM (VALUES
		(N'{staffInfoReaderWindowsAuthentication.LoginName}', 'U', N'', NULL, NULL, N'{Helper.MainDatabaseNameOutsideTestCase}', N'us_english', NULL, NULL, NULL),
		(N'{staffInfoReaderSqlAuthentication.LoginName}', 'S', N'', 0, 0, N'{Helper.MainDatabaseNameOutsideTestCase}', N'us_english', NULL, NULL, NULL)
) AS PER (member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
",
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoReaderWindowsAuthentication, staffInfoReaderSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, reader");

				yield return new TestCaseData(
					databaseTestMode,
					$@"-- Expected staff principals and memberships
SELECT * FROM (VALUES
		(N'{staffInfoDeveloperWindowsAuthentication.LoginName}', 'U', N'', NULL, NULL, N'{Helper.MainDatabaseNameOutsideTestCase}', N'us_english', NULL, NULL, NULL),
		(N'{staffInfoDeveloperSqlAuthentication.LoginName}', 'S', N'', 0, 0, N'{Helper.MainDatabaseNameOutsideTestCase}', N'us_english', NULL, NULL, NULL)

) AS PER (member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
",
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoDeveloperWindowsAuthentication, staffInfoDeveloperSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, developer");

				yield return new TestCaseData(
					databaseTestMode,
					$@"-- Expected staff principals and memberships
SELECT * FROM (VALUES
		(N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', 'U', N'', NULL, NULL, N'{Helper.MainDatabaseNameOutsideTestCase}', N'us_english', NULL, NULL, NULL),
		(N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', 'S', N'', 0, 0, N'{Helper.MainDatabaseNameOutsideTestCase}', N'us_english', NULL, NULL, NULL)
) AS PER (member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
",
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoBackupOperatorWindowsAuthentication, staffInfoBackupOperatorSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, backup operator");

				yield return new TestCaseData(
					databaseTestMode,
					$@"-- Expected staff principals and memberships
SELECT * FROM (VALUES
		(N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', 'U', N'', NULL, NULL, N'{Helper.MainDatabaseNameOutsideTestCase}', N'us_english', NULL, NULL, NULL),
		(N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', 'S', N'', 0, 0, N'{Helper.MainDatabaseNameOutsideTestCase}', N'us_english', NULL, NULL, NULL)

) AS PER (member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
",
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication, staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, developer, reader, backup operator and hrm user");

				yield return new TestCaseData(
					databaseTestMode,
					$@"-- Expected staff principals and memberships
SELECT * FROM (VALUES
		(N'{staffWithRandomRoleWindowsAuthentication.LoginName}', 'U', N'', NULL, NULL, N'{Helper.MainDatabaseNameOutsideTestCase}', N'us_english', NULL, NULL, NULL),
		(N'{staffWithRandomRoleSqlAuthentication.LoginName}', 'S', N'', 0, 0, N'{Helper.MainDatabaseNameOutsideTestCase}', N'us_english', NULL, NULL, NULL)

) AS PER (member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
",
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffWithRandomRoleWindowsAuthentication, staffWithRandomRoleSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, staff with random role");
			}
		}

		static IEnumerable<TestCaseData> ProposedPermissionsForStaffLoginsTestCaseSource()
		{
			var staffInfoCollection = new List<DbUserManager.StaffLoginInfo>();

			var staffInfoReaderWindowsAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
				LoginName = @"Domain\BobReader",
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwRestrictedReaderRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffInfoDeveloperWindowsAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
				LoginName = @"Domain\BobDeveloper",
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbDataWriterRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffInfoBackupOperatorWindowsAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
				LoginName = @"Domain\BobBackupOperator",
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbBackupOperatorRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
				LoginName = @"Domain\BobDeveloperBackupOperatorReaderHrmUser",
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwHRMStaffRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffWithRandomRoleWindowsAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
				LoginName = @"Domain\BobRandomRole",
				StaffDatabaseAccessGroupRoles = new HashSet<string> { "SomeRandomRole" },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffInfoReaderSqlAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				LoginName = Helper.GetEnterpriseLoginFullName("BobReader", Helper.MainDatabaseNameOutsideTestCase),
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwRestrictedReaderRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffInfoDeveloperSqlAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				LoginName = Helper.GetEnterpriseLoginFullName("BobDeveloper", Helper.MainDatabaseNameOutsideTestCase),
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbDataWriterRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffInfoBackupOperatorSqlAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				LoginName = Helper.GetEnterpriseLoginFullName("BobBackupOperator", Helper.MainDatabaseNameOutsideTestCase),
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbBackupOperatorRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				LoginName = Helper.GetEnterpriseLoginFullName("BobDeveloperBackupOperatorReaderHrmUser", Helper.MainDatabaseNameOutsideTestCase),
				StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwHRMStaffRole },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			var staffWithRandomRoleSqlAuthentication = new DbUserManager.StaffLoginInfo()
			{
				DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
				LoginName = Helper.GetEnterpriseLoginFullName("BobRandomRole", Helper.MainDatabaseNameOutsideTestCase),
				StaffDatabaseAccessGroupRoles = new HashSet<string> { "SomeRandomRole" },
				HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
			};

			staffInfoCollection.Add(staffInfoReaderWindowsAuthentication);
			staffInfoCollection.Add(staffInfoDeveloperWindowsAuthentication);
			staffInfoCollection.Add(staffInfoBackupOperatorWindowsAuthentication);
			staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication);
			staffInfoCollection.Add(staffWithRandomRoleWindowsAuthentication);

			staffInfoCollection.Add(staffInfoReaderSqlAuthentication);
			staffInfoCollection.Add(staffInfoDeveloperSqlAuthentication);
			staffInfoCollection.Add(staffInfoBackupOperatorSqlAuthentication);
			staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication);
			staffInfoCollection.Add(staffWithRandomRoleSqlAuthentication);

			yield return new TestCaseData(
			DatabaseTestMode.HostedInWiseCloudSharedServer,
			$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoReaderWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoReaderSqlAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffWithRandomRoleWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffWithRandomRoleSqlAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N''),

	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoReaderWindowsAuthentication.LoginName}', N''),
	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoReaderSqlAuthentication.LoginName}', N''),

	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', N''),
	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', N''),

	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),

	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffWithRandomRoleWindowsAuthentication.LoginName}', N''),
	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffWithRandomRoleSqlAuthentication.LoginName}', N''),

	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N'')
) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
",
			NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(staffInfoCollection))
			.SetName("{m}: Shared Wise cloud, multiple staff members");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoReaderWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoReaderSqlAuthentication.LoginName}', N''),

	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoReaderWindowsAuthentication.LoginName}', N''),
	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoReaderSqlAuthentication.LoginName}', N'')
) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
",
				NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoReaderWindowsAuthentication, staffInfoReaderSqlAuthentication }))
				.SetName("{m}: Shared Wise cloud, reader");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', N''),

	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', N''),
	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', N'')
) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)",
				NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoBackupOperatorWindowsAuthentication, staffInfoBackupOperatorSqlAuthentication }))
				.SetName("{m}: Shared Wise cloud, backup perator");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffWithRandomRoleWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffWithRandomRoleSqlAuthentication.LoginName}', N''),

	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffWithRandomRoleWindowsAuthentication.LoginName}', N''),
	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffWithRandomRoleSqlAuthentication.LoginName}', N'')

) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
",
				NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffWithRandomRoleWindowsAuthentication, staffWithRandomRoleSqlAuthentication }))
				.SetName("{m}: Shared Wise cloud, staff with some random role");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),

	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N'')

) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
",
				NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoDeveloperWindowsAuthentication, staffInfoDeveloperSqlAuthentication }))
				.SetName("{m}: Shared Wise cloud, developer");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N''),

	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('D', N'VIEW ANY DATABASE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N'')
) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
",
				NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication, staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication }))
				.SetName("{m}: Shared Wise cloud, developer, reader and backup operator");

			foreach (var selfHostedOption in new[] { DatabaseTestMode.SelfHostedOpen, DatabaseTestMode.SelfHostedLocked, DatabaseTestMode.HostedInWiseCloudDedicatedServer })
			{
				yield return new TestCaseData(
				selfHostedOption,
				$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoReaderWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoReaderSqlAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffWithRandomRoleWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffWithRandomRoleSqlAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('G', N'ALTER TRACE', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('G', N'VIEW SERVER STATE', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('G', N'ALTER ANY EVENT SESSION', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('G', N'VIEW ANY DEFINITION', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),
	('G', N'ALTER TRACE', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),
	('G', N'VIEW SERVER STATE', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),
	('G', N'ALTER ANY EVENT SESSION', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),
	('G', N'VIEW ANY DEFINITION', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('G', N'ALTER TRACE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('G', N'VIEW SERVER STATE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('G', N'ALTER ANY EVENT SESSION', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('G', N'VIEW ANY DEFINITION', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N''),
	('G', N'ALTER TRACE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N''),
	('G', N'VIEW SERVER STATE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N''),
	('G', N'ALTER ANY EVENT SESSION', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N''),
	('G', N'VIEW ANY DEFINITION', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N'')

) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
",
				NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(staffInfoCollection))
				.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(selfHostedOption)}, multiple staff members");

				yield return new TestCaseData(
					selfHostedOption,
					$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoReaderWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoReaderSqlAuthentication.LoginName}', N'')
) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
",
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoReaderWindowsAuthentication, staffInfoReaderSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(selfHostedOption)}, reader");

				yield return new TestCaseData(
					selfHostedOption,
					$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', N'')
) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
",
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoBackupOperatorWindowsAuthentication, staffInfoBackupOperatorSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(selfHostedOption)}, backup operator");

				yield return new TestCaseData(
					selfHostedOption,
					$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffWithRandomRoleWindowsAuthentication.LoginName}', N''),
	('G', N'CONNECT SQL', N'', N'', N'{staffWithRandomRoleSqlAuthentication.LoginName}', N'')

) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
",
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffWithRandomRoleWindowsAuthentication, staffWithRandomRoleSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(selfHostedOption)}, staff with some random role");

				yield return new TestCaseData(
					selfHostedOption,
					$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('G', N'ALTER TRACE', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('G', N'VIEW SERVER STATE', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('G', N'ALTER ANY EVENT SESSION', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),
	('G', N'VIEW ANY DEFINITION', N'', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),
	('G', N'ALTER TRACE', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),
	('G', N'VIEW SERVER STATE', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),
	('G', N'ALTER ANY EVENT SESSION', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N''),
	('G', N'VIEW ANY DEFINITION', N'', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N'')

) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
",
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoDeveloperWindowsAuthentication, staffInfoDeveloperSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(selfHostedOption)}, developer");

				yield return new TestCaseData(
					selfHostedOption,
					$@"-- Expected staff permissions
SELECT * FROM (VALUES
	('G', N'CONNECT SQL', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('G', N'ALTER TRACE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('G', N'VIEW SERVER STATE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('G', N'ALTER ANY EVENT SESSION', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),
	('G', N'VIEW ANY DEFINITION', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N''),

	('G', N'CONNECT SQL', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N''),
	('G', N'ALTER TRACE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N''),
	('G', N'VIEW SERVER STATE', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N''),
	('G', N'ALTER ANY EVENT SESSION', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N''),
	('G', N'VIEW ANY DEFINITION', N'', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N'')

) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
",
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication, staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(selfHostedOption)}, developer, reader and backup operator");
			}
		}

		#endregion Implementation
	}
}

