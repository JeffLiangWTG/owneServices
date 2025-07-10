using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.MasterFiles.Business;
using Enterprise.SqlSecurity.Test;
using WTG.TestHelpers.SpecTesting;
using TestHelper = Enterprise.SqlSecurity.Test.Helper;

namespace Enterprise.SqlSecurity.SpecTesting
{
	public class DatabaseLevelInfoSpecBuilder : ISpecProvider
	{
		public string SpecFolderPath => "Enterprise/Database/SqlSecurity/SqlSecurity.SpecTesting/Spec/Full/DatabaseLevelInfo";
		public string SpecNamespacePrefix => "Enterprise.SqlSecurity.SpecTesting.Spec.Full.DatabaseLevelInfo";
		public string RegenExecutableName => "SqlSecurity.SpecTesting.Regenerate.exe";

		readonly string randomScalarFnName = "TestScalarFn";
		readonly string randomClrScalarFnName = "TestClrScalarFn";
		readonly string randomSchemaName = "TestSchema";
		readonly string randomClrAggregateFnName = "TestClrAggregateFn";
		readonly string randomTypeName = "TestTVType";
		readonly string assemblyName = "TestSqlClrAssembly";

		void TeardownMockEntities(AdminConnection adminConnection, string databaseName)
		{
			var schemasToEnsureMissing = new[] { "RandomSchema" };

			TestHelper.EnsureSchemasMissingForDatabase(adminConnection, databaseName, schemasToEnsureMissing);

			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				adminConnection.ExecuteNonQuery($@"
DROP TYPE IF EXISTS dbo.[{randomTypeName}];
DROP TYPE IF EXISTS [{randomSchemaName}].[{randomTypeName}];

DROP SCHEMA IF EXISTS [{randomSchemaName};]

DROP FUNCTION IF EXISTS [dbo].[{randomScalarFnName}];

DROP FUNCTION IF EXISTS [{randomSchemaName}].[{randomScalarFnName}];

DROP AGGREGATE IF EXISTS [dbo].[{randomClrAggregateFnName}];

DROP AGGREGATE IF EXISTS [{randomSchemaName}].[{randomClrAggregateFnName}];

DROP FUNCTION IF EXISTS [dbo].[{randomClrScalarFnName}];

DROP FUNCTION IF EXISTS [{randomSchemaName}].[{randomClrScalarFnName}];

DROP SCHEMA IF EXISTS [{randomSchemaName}];

DROP ASSEMBLY IF EXISTS [{assemblyName}];

DROP USER IF EXISTS [{assemblyName}];
");
			}
		}

		void SetupMockEntities(AdminConnection adminConnection, string databaseName)
		{
			adminConnection.ExecuteNonQuery($"ALTER DATABASE {databaseName.QuoteName()} SET TRUSTWORTHY ON;");

			adminConnection.ExecuteNonQuery($@"
CREATE ASSEMBLY [{assemblyName}]
FROM '{System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\\Enterprise.SqlSecurity.Test.SqlClrAssembly.dll'
WITH PERMISSION_SET = EXTERNAL_ACCESS
");

			adminConnection.ExecuteNonQuery($@"
CREATE SCHEMA [{randomSchemaName}];
");

			adminConnection.ExecuteNonQuery($@"
CREATE AGGREGATE dbo.[{randomClrAggregateFnName}](@param int)
RETURNS int
EXTERNAL NAME [{assemblyName}].[Enterprise.SqlSecurity.Test.SqlClrAssembly.SqlClrAggregateTest]");

			adminConnection.ExecuteNonQuery($@"
CREATE AGGREGATE [{randomSchemaName}].[{randomClrAggregateFnName}](@param int)
RETURNS int
EXTERNAL NAME [{assemblyName}].[Enterprise.SqlSecurity.Test.SqlClrAssembly.SqlClrAggregateTest]");

			adminConnection.ExecuteNonQuery($@"
CREATE FUNCTION dbo.[{randomClrScalarFnName}]()
RETURNS int
EXTERNAL NAME {assemblyName}.SqlClrScalarTest.ScalarFunction
");

			adminConnection.ExecuteNonQuery($@"
CREATE FUNCTION [{randomSchemaName}].[{randomClrScalarFnName}]()
RETURNS int
EXTERNAL NAME {assemblyName}.SqlClrScalarTest.ScalarFunction");

			adminConnection.ExecuteNonQuery($@"
CREATE FUNCTION dbo.[{randomScalarFnName}]()
RETURNS int
BEGIN
	RETURN 0
END");

			adminConnection.ExecuteNonQuery($@"
CREATE FUNCTION [{randomSchemaName}].[{randomScalarFnName}]()
RETURNS int
BEGIN
	RETURN 0
END");

			TestHelper.CreateType(adminConnection, "dbo", randomTypeName);
			TestHelper.CreateType(adminConnection, randomSchemaName, randomTypeName);
		}

		public IEnumerable<SpecFile> GetSpecFiles()
		{
			using (SpecSetupFixture.SetupSpecEnvironment())
			{
				var databaseTypes = new[]
				{
					DatabaseType.SharedRef,
					DatabaseType.SingleSharedRef,
					DatabaseType.Main,
					DatabaseType.SD,
					DatabaseType.EDW,
					DatabaseType.Audit,
					DatabaseType.ExclusiveRef,
					DatabaseType.UserRepository,
				};
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

				foreach (var databaseType in databaseTypes)
				{
					foreach (var databaseTestMode in databaseTestModes)
					{
						using (SpecSetupFixture.SetupSpecTest(out var adminConnection))
						{
							var databaseName = TestHelper.DatabaseNameFromDatabaseType(databaseType);
							using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
							{
								SetupMockEntities(adminConnection, databaseName);
								var spec = SpecFileBuilder.GetSpecTextForDatabasePrincipals(adminConnection, databaseType, databaseTestMode, databaseName, staffInfoCollection);
								var filename = $"{databaseType}_{databaseTestMode}.txt";
								TeardownMockEntities(adminConnection, databaseName);

								yield return new SpecFile(filename, spec);
							}
						}
					}
				}
			}
		}
	}
}
