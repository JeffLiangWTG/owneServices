using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.DataProtection;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	[Property("DAT:CapabilityRequirements", "SQL2019")]
	class SqlSecurityBuilderStaffUsersAndRolesTest
	{
		[TestCaseSource(nameof(PrincipalsAndMembershipsForStaffUsersTestCaseSource), new object[] { DatabaseType.SharedRef })]
		[TestCaseSource(nameof(PrincipalsAndMembershipsForStaffUsersTestCaseSource), new object[] { DatabaseType.Main })]
		[TestCaseSource(nameof(PrincipalsAndMembershipsForStaffUsersTestCaseSource), new object[] { DatabaseType.SD })]
		[TestCaseSource(nameof(PrincipalsAndMembershipsForStaffUsersTestCaseSource), new object[] { DatabaseType.EDW })]
		[TestCaseSource(nameof(PrincipalsAndMembershipsForStaffUsersTestCaseSource), new object[] { DatabaseType.Audit })]
		[TestCaseSource(nameof(PrincipalsAndMembershipsForStaffUsersTestCaseSource), new object[] { DatabaseType.ExclusiveRef })]
		[TestCaseSource(nameof(PrincipalsAndMembershipsForStaffUsersTestCaseSource), new object[] { DatabaseType.UserRepository })]
		public void TestCorrectPrincipalsAndMembershipsListIsGeneratedWhenThereAreStaffUsers(DatabaseType databaseType, DatabaseTestMode databaseTestMode, IEnumerable<string> expectedStaffPrincipalsAndMemberships, IStaffInfoProvider staffInfoProvider)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var expectedPrincipalsAndMemberships =
				$@"-- Expected principals and memberships
SELECT * FROM (VALUES
{string.Join($",{System.Environment.NewLine}\t\t", expectedStaffPrincipalsAndMemberships.Concat(ApplicationUsersAndRoles.PrincipalsAndMemberships(Db.DatabaseName)))}
) AS PER (member_name, member_type, parent_role, default_schema_name)
";
			var expectedPrincipalsAndMembershipsList = Helper.DatabasePrincipalsList(adminConnection, expectedPrincipalsAndMemberships);
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, staffInfoProvider);

				// Act
				var result = securityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, databaseType);
				var actualPrincipalsList = Helper.DatabasePrincipalsList(adminConnection, result.ProposedPrincipalsAndMemberships);

				// Assert

				Assert.That(actualPrincipalsList.Count, Is.EqualTo(expectedPrincipalsAndMembershipsList.Count), "Number of expected and actual principals and memberships should be the same.");

				foreach (var expectedPrincipalMembership in expectedPrincipalsAndMembershipsList.Values)
				{
					Assert.That(actualPrincipalsList.Keys, Does.Contain(expectedPrincipalMembership.member_name + expectedPrincipalMembership.parent_role).IgnoreCase, $"Actual principal membership '{expectedPrincipalMembership}' must be present in the expected list.");

					var actualPrincipalMembership = actualPrincipalsList[expectedPrincipalMembership.member_name + expectedPrincipalMembership.parent_role];

					Assert.That(actualPrincipalMembership.member_type, Is.EqualTo(expectedPrincipalMembership.member_type).IgnoreCase, $"Expected and actual 'member type' for '{actualPrincipalMembership.member_name}' must be identical.");
					Assert.That(actualPrincipalMembership.default_schema, Is.Null, $"Actual 'default schema' for '{actualPrincipalMembership.member_name}' must be null.");
				}
			}
		}

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer)]
		[TestCase(DatabaseType.Main, DatabaseTestMode.HostedInWiseCloudSharedServer)]
		[TestCase(DatabaseType.SD, DatabaseTestMode.HostedInWiseCloudSharedServer)]
		[TestCase(DatabaseType.EDW, DatabaseTestMode.HostedInWiseCloudSharedServer)]
		[TestCase(DatabaseType.Audit, DatabaseTestMode.HostedInWiseCloudSharedServer)]
		[TestCase(DatabaseType.ExclusiveRef, DatabaseTestMode.HostedInWiseCloudSharedServer)]
		[TestCase(DatabaseType.UserRepository, DatabaseTestMode.HostedInWiseCloudSharedServer)]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer)]
		[TestCase(DatabaseType.Main, DatabaseTestMode.HostedInWiseCloudDedicatedServer)]
		[TestCase(DatabaseType.SD, DatabaseTestMode.HostedInWiseCloudDedicatedServer)]
		[TestCase(DatabaseType.EDW, DatabaseTestMode.HostedInWiseCloudDedicatedServer)]
		[TestCase(DatabaseType.Audit, DatabaseTestMode.HostedInWiseCloudDedicatedServer)]
		[TestCase(DatabaseType.ExclusiveRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer)]
		[TestCase(DatabaseType.UserRepository, DatabaseTestMode.HostedInWiseCloudDedicatedServer)]
		public void TestBackupOperatorRoleIsExcludedWhenHostedInWiseCloud(DatabaseType databaseType, DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var staffInfoCollection = new List<DbUserManager.StaffLoginInfo>();

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

			staffInfoCollection.Add(staffInfoBackupOperatorWindowsAuthentication);
			staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication);

			staffInfoCollection.Add(staffInfoBackupOperatorSqlAuthentication);
			staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(staffInfoCollection));

				// Act
				var result = securityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, databaseType);
				var actualPrincipalsList = Helper.DatabasePrincipalsList(adminConnection, result.ProposedPrincipalsAndMemberships);

				// Assert

				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffInfoBackupOperatorWindowsAuthentication.LoginName}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with no parent role.");
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffInfoBackupOperatorSqlAuthentication.LoginName}"), $"There should be a record for '{staffInfoBackupOperatorSqlAuthentication.LoginName}' with no parent role.");

				Assert.That(actualPrincipalsList, Does.Not.ContainKey($"{staffInfoBackupOperatorWindowsAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be no record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.Not.ContainKey($"{staffInfoBackupOperatorSqlAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be no record for '{staffInfoBackupOperatorSqlAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");

				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.CwRestrictedReaderRole}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.CwRestrictedReaderRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.CwRestrictedReaderRole}"), $"There should be a record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.CwRestrictedReaderRole}' parent role.");

				Assert.That(actualPrincipalsList, Does.Not.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be no record for '{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.Not.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be no record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");

				if (databaseType == DatabaseType.UserRepository)
				{
					Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.DbDataWriterRole}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.DbDataWriterRole}' parent role in UserRepository database.");
					Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.DbDataWriterRole}"), $"There should be a record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.DbDataWriterRole}' parent role in UserRepository database.");
				}

				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.CwHRMStaffRole}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.CwHRMStaffRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.CwHRMStaffRole}"), $"There should be a record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.CwHRMStaffRole}' parent role.");
			}
		}

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.Main, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.SD, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.EDW, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.Audit, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.ExclusiveRef, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.UserRepository, DatabaseTestMode.SelfHostedLocked)]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.Main, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.SD, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.EDW, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.Audit, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.ExclusiveRef, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.UserRepository, DatabaseTestMode.SelfHostedOpen)]

		public void TestBackupOperatorRoleIsExcludedWhenSelfHostedButIsWiseTechGlobalDatabaseServer(DatabaseType databaseType, DatabaseTestMode databaseTestMode)
		{
			// Arrange
			DataUtils.IsWiseTechGlobalDatabaseServerForTest = true;
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var staffInfoCollection = new List<DbUserManager.StaffLoginInfo>();

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

			staffInfoCollection.Add(staffInfoBackupOperatorWindowsAuthentication);
			staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication);

			staffInfoCollection.Add(staffInfoBackupOperatorSqlAuthentication);
			staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(staffInfoCollection));

				// Act
				var result = securityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, databaseType);
				var actualPrincipalsList = Helper.DatabasePrincipalsList(adminConnection, result.ProposedPrincipalsAndMemberships);

				// Assert
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffInfoBackupOperatorWindowsAuthentication.LoginName}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with no parent role.");
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffInfoBackupOperatorSqlAuthentication.LoginName}"), $"There should be a record for '{staffInfoBackupOperatorSqlAuthentication.LoginName}' with no parent role.");

				Assert.That(actualPrincipalsList, Does.Not.ContainKey($"{staffInfoBackupOperatorWindowsAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be no record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.Not.ContainKey($"{staffInfoBackupOperatorSqlAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be no record for '{staffInfoBackupOperatorSqlAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");

				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.CwRestrictedReaderRole}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.CwRestrictedReaderRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.CwRestrictedReaderRole}"), $"There should be a record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.CwRestrictedReaderRole}' parent role.");

				Assert.That(actualPrincipalsList, Does.Not.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be no record for '{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.Not.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be no record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");

				if (databaseType == DatabaseType.UserRepository)
				{
					Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.DbDataWriterRole}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.DbDataWriterRole}' parent role in UserRepository database.");
					Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.DbDataWriterRole}"), $"There should be a record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.DbDataWriterRole}' parent role in UserRepository database.");
				}

				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.CwHRMStaffRole}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.CwHRMStaffRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.CwHRMStaffRole}"), $"There should be a record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.CwHRMStaffRole}' parent role.");
			}
		}

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.Main, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.SD, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.EDW, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.Audit, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.ExclusiveRef, DatabaseTestMode.SelfHostedLocked)]
		[TestCase(DatabaseType.UserRepository, DatabaseTestMode.SelfHostedLocked)]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.Main, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.SD, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.EDW, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.Audit, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.ExclusiveRef, DatabaseTestMode.SelfHostedOpen)]
		[TestCase(DatabaseType.UserRepository, DatabaseTestMode.SelfHostedOpen)]
		public void TestBackupOperatorRoleIsKeptWhenSelfHosted(DatabaseType databaseType, DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var staffInfoCollection = new List<DbUserManager.StaffLoginInfo>();

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

			staffInfoCollection.Add(staffInfoBackupOperatorWindowsAuthentication);
			staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication);

			staffInfoCollection.Add(staffInfoBackupOperatorSqlAuthentication);
			staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(staffInfoCollection));

				// Act
				var result = securityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, databaseType);
				var actualPrincipalsList = Helper.DatabasePrincipalsList(adminConnection, result.ProposedPrincipalsAndMemberships);

				// Assert
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffInfoBackupOperatorWindowsAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffInfoBackupOperatorSqlAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be a record for '{staffInfoBackupOperatorSqlAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");

				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.CwRestrictedReaderRole}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.CwRestrictedReaderRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.CwRestrictedReaderRole}"), $"There should be a record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.CwRestrictedReaderRole}' parent role.");

				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be a record for '{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.DbBackupOperatorRole}"), $"There should be a record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.DbBackupOperatorRole}' parent role.");

				if (databaseType == DatabaseType.UserRepository)
				{
					Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.DbDataWriterRole}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.DbDataWriterRole}' parent role in UserRepository database.");
					Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.DbDataWriterRole}"), $"There should be a record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.DbDataWriterRole}' parent role in UserRepository database.");
				}

				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}{DbRoleTypes.CwHRMStaffRole}"), $"There should be a record for '{staffInfoBackupOperatorWindowsAuthentication.LoginName}' with '{DbRoleTypes.CwHRMStaffRole}' parent role.");
				Assert.That(actualPrincipalsList, Does.ContainKey($"{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}{DbRoleTypes.CwHRMStaffRole}"), $"There should be a record for '{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}' with '{DbRoleTypes.CwHRMStaffRole}' parent role.");
			}
		}

		[TestCaseSource(nameof(PermissionsForStaffUsersTestCaseSource), new object[] { DatabaseType.SharedRef })]
		[TestCaseSource(nameof(PermissionsForStaffUsersTestCaseSource), new object[] { DatabaseType.Main })]
		[TestCaseSource(nameof(PermissionsForStaffUsersTestCaseSource), new object[] { DatabaseType.SD })]
		[TestCaseSource(nameof(PermissionsForStaffUsersTestCaseSource), new object[] { DatabaseType.EDW })]
		[TestCaseSource(nameof(PermissionsForStaffUsersTestCaseSource), new object[] { DatabaseType.Audit })]
		[TestCaseSource(nameof(PermissionsForStaffUsersTestCaseSource), new object[] { DatabaseType.ExclusiveRef })]
		[TestCaseSource(nameof(PermissionsForStaffUsersTestCaseSource), new object[] { DatabaseType.UserRepository })]
		public void TestCorrectPermissionsListIsGeneratedWhenThereAreStaffUsers(DatabaseType databaseType, DatabaseTestMode databaseTestMode, IEnumerable<string> expectedStaffPermissions, IStaffInfoProvider staffInfoProvider)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var expectedPermissionsList = Helper.DatabasePermissionsList(adminConnection,
								$@"
SELECT * FROM (VALUES
{string.Join($",{System.Environment.NewLine}\t\t", expectedStaffPermissions.Concat(ApplicationUsersAndRoles.Permissions.ByDatabaseType(databaseType, Db.DatabaseName)))}
 ) AS PER (State, Permission, SecurableType, SecurableSchema, Securable, SecurableColumn, Grantee, Grantor)
");
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, staffInfoProvider);

				// Act
				var result = securityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, databaseType);
				var actualPermissionsList = Helper.DatabasePermissionsList(adminConnection, result.ProposedPermissions);

				// Assert
				Assert.That(
					actualPermissionsList,
					Is.EquivalentTo(expectedPermissionsList).IgnoreCase, "Actual permissions should match expected.");
			}
		}

		#region Implementation

		readonly string dbReaderLogin = CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbWriterLogin = CargoWiseWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedReaderLogin = RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedWriterLogin = RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbUnrestrictedWriterLogin = UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			adminConnection = Db.NewAdminConnection();
			Helper.EnsureExtraDatabasesWithSchemas(adminConnection);
		}

		[SetUp]
		public void SetUp()
		{
			DataUtils.IsWiseTechGlobalDatabaseServerForTest = false;
			Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);
		}

		[TearDown]
		public void TearDown()
		{
			DataUtils.IsWiseTechGlobalDatabaseServerForTest = false;
			Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			adminConnection?.Dispose();
		}

		AdminConnection adminConnection;

		static IEnumerable<TestCaseData> PrincipalsAndMembershipsForStaffUsersTestCaseSource(DatabaseType databaseType)
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
					databaseType,
					databaseTestMode,
					new[]
					{
						$"(N'{staffInfoReaderWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.CwRestrictedReaderRole}', NULL)",
						$"(N'{staffInfoReaderSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.CwRestrictedReaderRole}', NULL)",
						$"(N'{staffInfoDeveloperWindowsAuthentication.LoginName}', 'U', N'{(databaseType == DatabaseType.UserRepository ? DbRoleTypes.DbDataWriterRole : string.Empty)}', NULL)", // db_datawriter role is only for UserRepository, so no user roles here
						$"(N'{staffInfoDeveloperSqlAuthentication.LoginName}', 'S', N'{(databaseType == DatabaseType.UserRepository ? DbRoleTypes.DbDataWriterRole : string.Empty)}', NULL)", // db_datawriter role is only for UserRepository, so no user roles here

						$"(N'{staffWithRandomRoleWindowsAuthentication.LoginName}', 'U', N'SomeRandomRole', NULL)",
						$"(N'{staffWithRandomRoleSqlAuthentication.LoginName}', 'S', N'SomeRandomRole', NULL)",

						$"(N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.CwRestrictedReaderRole}', NULL)",
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.CwRestrictedReaderRole}', NULL)",
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.CwHRMStaffRole}', NULL)",
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.CwHRMStaffRole}', NULL)",
					}
					.AppendIf(
						databaseTestMode == DatabaseTestMode.SelfHostedOpen || databaseTestMode == DatabaseTestMode.SelfHostedLocked,
						$"(N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.DbBackupOperatorRole}', NULL)",
						$"(N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.DbBackupOperatorRole}', NULL)",

						$"(N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.DbBackupOperatorRole}', NULL)",
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.DbBackupOperatorRole}', NULL)")
					.AppendIf(
						databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer,
						$"(N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', 'U', N'', NULL)",
						$"(N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', 'S', N'', NULL)")
					.AppendIf(
						databaseType == DatabaseType.UserRepository,
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.DbDataWriterRole}', NULL)",
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.DbDataWriterRole}', NULL)"),
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(staffInfoCollection))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, multiple staff members, {databaseType} database");

				yield return new TestCaseData(
					databaseType,
					databaseTestMode,
					new[]
					{
						$"(N'{staffInfoReaderWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.CwRestrictedReaderRole}', NULL)",
						$"(N'{staffInfoReaderSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.CwRestrictedReaderRole}', NULL)",
					},
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoReaderWindowsAuthentication, staffInfoReaderSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, reader, {databaseType} database");

				yield return new TestCaseData(
					databaseType,
					databaseTestMode,
					new[]
					{
						$"(N'{staffInfoDeveloperWindowsAuthentication.LoginName}', 'U', N'{(databaseType == DatabaseType.UserRepository ? DbRoleTypes.DbDataWriterRole : string.Empty)}', NULL)", // db_datawriter role is only for UserRepository, so no user roles here
						$"(N'{staffInfoDeveloperSqlAuthentication.LoginName}', 'S', N'{(databaseType == DatabaseType.UserRepository ? DbRoleTypes.DbDataWriterRole : string.Empty)}', NULL)", // db_datawriter role is only for UserRepository, so no user roles here
					},
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoDeveloperWindowsAuthentication, staffInfoDeveloperSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, developer, {databaseType} database");

				yield return new TestCaseData(
					databaseType,
					databaseTestMode,
					Array.Empty<string>()
					.AppendIf(
						databaseTestMode == DatabaseTestMode.SelfHostedOpen || databaseTestMode == DatabaseTestMode.SelfHostedLocked,
						$"(N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.DbBackupOperatorRole}', NULL)",
						$"(N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.DbBackupOperatorRole}', NULL)")
					.AppendIf(
						databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer,
						$"(N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', 'U', N'', NULL)",
						$"(N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', 'S', N'', NULL)"),
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoBackupOperatorWindowsAuthentication, staffInfoBackupOperatorSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, backup operator, {databaseType} database");

				yield return new TestCaseData(
					databaseType,
					databaseTestMode,
					new[]
					{
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.CwRestrictedReaderRole}', NULL)",
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.CwRestrictedReaderRole}', NULL)",
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.CwHRMStaffRole}', NULL)",
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.CwHRMStaffRole}', NULL)",
					}
					.AppendIf(
						databaseTestMode == DatabaseTestMode.SelfHostedOpen || databaseTestMode == DatabaseTestMode.SelfHostedLocked,
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.DbBackupOperatorRole}', NULL)",
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.DbBackupOperatorRole}', NULL)")
					.AppendIf(
						databaseType == DatabaseType.UserRepository,
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', 'U', N'{DbRoleTypes.DbDataWriterRole}', NULL)")
					.AppendIf(
						databaseType == DatabaseType.UserRepository,
						$"(N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', 'S', N'{DbRoleTypes.DbDataWriterRole}', NULL)"),
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication, staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, developer, reader, backup operator and hrm user, {databaseType} database");

				yield return new TestCaseData(
					databaseType,
					databaseTestMode,
					new[]
					{
						$"(N'{staffWithRandomRoleWindowsAuthentication.LoginName}', 'U', N'SomeRandomRole', NULL)",
						$"(N'{staffWithRandomRoleSqlAuthentication.LoginName}', 'S', N'SomeRandomRole', NULL)",
					},
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffWithRandomRoleWindowsAuthentication, staffWithRandomRoleSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, staff with random role, {databaseType} database");
			}
		}

		static IEnumerable<TestCaseData> PermissionsForStaffUsersTestCaseSource(DatabaseType databaseType)
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
					databaseType,
					databaseTestMode,
					new[]
					{
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoReaderWindowsAuthentication.LoginName}', N'')",
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoReaderSqlAuthentication.LoginName}', N'')",
					}
					.AppendIf(
						(databaseType & DatabaseType.BI) == 0,
						new[]
						{
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoReaderWindowsAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoReaderSqlAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
						})
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.CwRestrictedReaderRole, staffInfoReaderWindowsAuthentication.LoginName))
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.CwRestrictedReaderRole, staffInfoReaderSqlAuthentication.LoginName))
					.Concat(
						new[]
						{
							$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N'')",
							$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N'')",
						})
					.AppendIf(
						(databaseType & DatabaseType.BI) == 0,
						new[]
						{
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
						})
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.DbDataWriterRole, staffInfoDeveloperWindowsAuthentication.LoginName))
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.DbDataWriterRole, staffInfoDeveloperSqlAuthentication.LoginName))
					.Concat(
						new[]
						{
							$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', N'')",
							$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', N'')",

							$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N'')",
							$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N'')",
						})
					.AppendIf(
						(databaseType & DatabaseType.BI) == 0,
						new[]
						{
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",

							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
						})
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.CwRestrictedReaderRole, staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName))
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.CwRestrictedReaderRole, staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName))
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.DbDataWriterRole, staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName))
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.DbDataWriterRole, staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName))
					.Concat(
						new[]
						{
							$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffWithRandomRoleWindowsAuthentication.LoginName}', N'')",
							$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffWithRandomRoleSqlAuthentication.LoginName}', N'')",
						})
					.AppendIf(
						(databaseType & DatabaseType.BI) == 0,
						new[]
						{
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffWithRandomRoleWindowsAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffWithRandomRoleSqlAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
						}),
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(staffInfoCollection))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, multiple staff members, {databaseType} database");

				yield return new TestCaseData(
					databaseType,
					databaseTestMode,
					new[]
					{
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoReaderWindowsAuthentication.LoginName}', N'')",
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoReaderSqlAuthentication.LoginName}', N'')",
					}
					.AppendIf(
						(databaseType & DatabaseType.BI) == 0,
						new[]
						{
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoReaderWindowsAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoReaderSqlAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
						})
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.CwRestrictedReaderRole, staffInfoReaderWindowsAuthentication.LoginName))
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.CwRestrictedReaderRole, staffInfoReaderSqlAuthentication.LoginName)),
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoReaderWindowsAuthentication, staffInfoReaderSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, reader, {databaseType} database");

				yield return new TestCaseData(
					databaseType,
					databaseTestMode,
					new[]
					{
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', N'')",
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', N'')",
					}
					.AppendIf(
						(databaseType & DatabaseType.BI) == 0,
						new[]
						{
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoBackupOperatorWindowsAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoBackupOperatorSqlAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
						}),
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoBackupOperatorWindowsAuthentication, staffInfoBackupOperatorSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, backup operator, {databaseType} database");

				yield return new TestCaseData(
					databaseType,
					databaseTestMode,
					new[]
					{
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffWithRandomRoleWindowsAuthentication.LoginName}', N'')",
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffWithRandomRoleSqlAuthentication.LoginName}', N'')",
					}
					.AppendIf(
						(databaseType & DatabaseType.BI) == 0,
						new[]
						{
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffWithRandomRoleWindowsAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffWithRandomRoleSqlAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
						}),
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffWithRandomRoleWindowsAuthentication, staffWithRandomRoleSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, staff with some random role, {databaseType} database");

				yield return new TestCaseData(
					databaseType,
					databaseTestMode,
					new[]
					{
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N'')",
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N'')",
					}
					.AppendIf(
						(databaseType & DatabaseType.BI) == 0,
						new[]
						{
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoDeveloperWindowsAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffInfoDeveloperSqlAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
						})
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.DbDataWriterRole, staffInfoDeveloperWindowsAuthentication.LoginName))
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.DbDataWriterRole, staffInfoDeveloperSqlAuthentication.LoginName)),
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffInfoDeveloperWindowsAuthentication, staffInfoDeveloperSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, developer, {databaseType} database");

				yield return new TestCaseData(
					databaseType,
					databaseTestMode,
					new[]
					{
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N'')",
						$"('G', N'CONNECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(databaseType)}', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N'')",
					}
					.AppendIf(
						(databaseType & DatabaseType.BI) == 0,
						new[]
						{
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
							$"('G', N'IMPERSONATE', N'USER', N'', N'{staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase)}', N'')",
						})
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.CwRestrictedReaderRole, staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName))
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.CwRestrictedReaderRole, staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName))
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.DbDataWriterRole, staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication.LoginName))
					.Concat(StaffUser.Permissions.SpecificToRoleByDatabaseType(databaseType, DbRoleTypes.DbDataWriterRole, staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication.LoginName)),
					NoTestCaseHelper.GetStaffInfoProviderMockFromStaffInfoCollection(new[] { staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication, staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication }))
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}, developer, reader and backup operator, {databaseType} database");
			}
		}

		#endregion Implementation
	}
}

