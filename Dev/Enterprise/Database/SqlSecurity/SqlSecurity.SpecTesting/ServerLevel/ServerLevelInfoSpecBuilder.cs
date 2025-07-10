using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.MasterFiles.Business;
using Enterprise.SqlSecurity.Test;
using WTG.TestHelpers.SpecTesting;
using TestHelper = Enterprise.SqlSecurity.Test.Helper;

namespace Enterprise.SqlSecurity.SpecTesting
{
	public class ServerLevelInfoSpecBuilder : ISpecProvider
	{
		public string SpecFolderPath => "Enterprise/Database/SqlSecurity/SqlSecurity.SpecTesting/Spec/Full/ServerLevelInfo";
		public string SpecNamespacePrefix => "Enterprise.SqlSecurity.SpecTesting.Spec.Full.ServerLevelInfo";
		public string RegenExecutableName => "SqlSecurity.SpecTesting.Regenerate.exe";

		public IEnumerable<SpecFile> GetSpecFiles()
		{
			using (SpecSetupFixture.SetupSpecEnvironment())
			{
				var databaseTestModes = new[]
				{
					DatabaseTestMode.HostedInWiseCloudSharedServer,
					DatabaseTestMode.HostedInWiseCloudDedicatedServer,
					DatabaseTestMode.SelfHostedLocked,
					DatabaseTestMode.SelfHostedOpen
				};

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
					LoginName = TestHelper.GetEnterpriseLoginFullName("BobReader", TestHelper.MainDatabaseNameOutsideTestCase),
					StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwRestrictedReaderRole },
					HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
				};

				var staffInfoDeveloperSqlAuthentication = new DbUserManager.StaffLoginInfo()
				{
					DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
					LoginName = TestHelper.GetEnterpriseLoginFullName("BobDeveloper", TestHelper.MainDatabaseNameOutsideTestCase),
					StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbDataWriterRole },
					HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
				};

				var staffInfoBackupOperatorSqlAuthentication = new DbUserManager.StaffLoginInfo()
				{
					DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
					LoginName = TestHelper.GetEnterpriseLoginFullName("BobBackupOperator", TestHelper.MainDatabaseNameOutsideTestCase),
					StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbBackupOperatorRole },
					HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
				};

				var staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication = new DbUserManager.StaffLoginInfo()
				{
					DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
					LoginName = TestHelper.GetEnterpriseLoginFullName("BobDeveloperBackupOperatorReaderHrmUser", TestHelper.MainDatabaseNameOutsideTestCase),
					StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwHRMStaffRole },
					HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
				};

				var staffWithRandomRoleSqlAuthentication = new DbUserManager.StaffLoginInfo()
				{
					DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
					LoginName = TestHelper.GetEnterpriseLoginFullName("BobRandomRole", TestHelper.MainDatabaseNameOutsideTestCase),
					StaffDatabaseAccessGroupRoles = new HashSet<string> { "SomeRandomRole" },
					HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
				};

				staffInfoCollection.Add(staffInfoReaderWindowsAuthentication);
				staffInfoCollection.Add(staffInfoDeveloperWindowsAuthentication);
				staffInfoCollection.Add(staffInfoBackupOperatorWindowsAuthentication);
				staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserWindowsAuthentication);

				staffInfoCollection.Add(staffInfoReaderSqlAuthentication);
				staffInfoCollection.Add(staffInfoDeveloperSqlAuthentication);
				staffInfoCollection.Add(staffInfoBackupOperatorSqlAuthentication);
				staffInfoCollection.Add(staffDeveloperReaderBackupOperatorHrmUserSqlAuthentication);

				foreach (var databaseTestMode in databaseTestModes)
				{
					using (SpecSetupFixture.SetupSpecTest(out var adminConnection))
					{
						var spec = SpecFileBuilder.GetSpecTextForServerPrincipals(adminConnection, databaseTestMode, staffInfoCollection);
						var filename = $"{databaseTestMode}.txt";
						yield return new SpecFile(filename, spec);
					}
				}
			}
		}
	}
}
