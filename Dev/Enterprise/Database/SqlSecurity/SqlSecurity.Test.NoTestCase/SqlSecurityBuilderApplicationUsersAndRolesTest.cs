using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using Moq;
using NUnit.Framework;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	class SqlSecurityBuilderApplicationUsersAndRolesTest
	{
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestExecutePermissionIsGeneratedOnEachUserDefinedTableValuedTypeInMainDatabaseForCwReaderRole: Hosted on CargoWise Dedicated Server")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestExecutePermissionIsGeneratedOnEachUserDefinedTableValuedTypeInMainDatabaseForCwReaderRole: Hosted on CargoWise Shared Server")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestExecutePermissionIsGeneratedOnEachUserDefinedTableValuedTypeInMainDatabaseForCwReaderRole: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestExecutePermissionIsGeneratedOnEachUserDefinedTableValuedTypeInMainDatabaseForCwReaderRole: Self hosted locked")]
		public void TestExecutePermissionIsGeneratedOnEachUserDefinedTableValuedTypeInMainDatabaseForCwReaderRole(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var databaseName = Db.DatabaseName;

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				Helper.CreateType(adminConnection, "dbo", randomTypeName);
				Helper.EnsureSchema(adminConnection, randomSchemaName);
				Helper.CreateType(adminConnection, randomSchemaName, randomTypeName);

				var expectedPermissions =
					$@"
SELECT * FROM (VALUES
	('G', N'EXECUTE', N'TYPE', N'dbo', N'{randomTypeName}', N'', N'{DbRoleTypes.CwReaderRole}', N''),
	('G', N'EXECUTE', N'TYPE', N'{randomSchemaName}', N'{randomTypeName}', N'', N'{DbRoleTypes.CwReaderRole}', N'')
 ) AS PER (State, Permission, SecurableType, SecurableSchema, Securable, SecurableColumn, Grantee, Grantor)
";
				var expectedPermissioinsList = Helper.DatabasePermissionsList(adminConnection, expectedPermissions);
				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, DatabaseType.Main);
				var proposedPermissionsList = Helper.DatabasePermissionsList(adminConnection, result.ProposedPermissions);

				// Assert
				foreach (var expectedPermission in expectedPermissioinsList)
				{
					Assert.That(proposedPermissionsList, Does.Contain(expectedPermission), $"Expected permission '{expectedPermission}' on custom table value type must be present in the proposed permissions list.");
				}
			}
		}

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestExecutePermissionIsGeneratedOnEachFAAndAFAAndFSTypeFunctionInMainDatabaseForCwReaderRole: Hosted on CargoWise Dedicated Server")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestExecutePermissionIsGeneratedOnEachFAAndAFAAndFSTypeFunctionInMainDatabaseForCwReaderRole: Hosted on CargoWise Shared Server")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestExecutePermissionIsGeneratedOnEachFAAndAFAAndFSTypeFunctionInMainDatabaseForCwReaderRole: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestExecutePermissionIsGeneratedOnEachFAAndAFAAndFSTypeFunctionInMainDatabaseForCwReaderRole: Self hosted locked")]
		public void TestExecutePermissionIsGeneratedOnEachFAAndAFAAndFSTypeFunctionInMainDatabaseForCwReaderRole(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var databaseName = Db.DatabaseName;

			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			// This assembly location is implemented as Enterprise.SqlSecurity.Test.SqlClrAssembly cannot be multi-targeted to Net8
			var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
#if NET
			assemblyLocation = Path.GetDirectoryName(assemblyLocation);
#endif

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				adminConnection.ExecuteNonQuery($"ALTER DATABASE {databaseName.QuoteName()} SET TRUSTWORTHY ON;");

				adminConnection.ExecuteNonQuery($@"
CREATE ASSEMBLY [{assemblyName}]
FROM '{assemblyLocation}\\Enterprise.SqlSecurity.Test.SqlClrAssembly.dll'
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

				var expectedPermissions =
$@"
SELECT * FROM (VALUES
	('G', N'EXECUTE', N'OBJECT', N'dbo', N'{randomClrAggregateFnName}', N'', N'{DbRoleTypes.CwReaderRole}', N''),
	('G', N'EXECUTE', N'OBJECT', N'{randomSchemaName}', N'{randomClrAggregateFnName}', N'', N'{DbRoleTypes.CwReaderRole}', N''),
	('G', N'EXECUTE', N'OBJECT', N'dbo', N'{randomClrScalarFnName}', N'', N'{DbRoleTypes.CwReaderRole}', N''),
	('G', N'EXECUTE', N'OBJECT', N'{randomSchemaName}', N'{randomClrScalarFnName}', N'', N'{DbRoleTypes.CwReaderRole}', N''),
	('G', N'EXECUTE', N'OBJECT', N'dbo', N'{randomScalarFnName}', N'', N'{DbRoleTypes.CwReaderRole}', N''),
	('G', N'EXECUTE', N'OBJECT', N'{randomSchemaName}', N'{randomScalarFnName}', N'', N'{DbRoleTypes.CwReaderRole}', N'')
 ) AS PER (State, Permission, SecurableType, SecurableSchema, Securable, SecurableColumn, Grantee, Grantor)
";
				var expectedPermissioinsList = Helper.DatabasePermissionsList(adminConnection, expectedPermissions);
				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, DatabaseType.Main);
				var proposedPermissionsList = Helper.DatabasePermissionsList(adminConnection, result.ProposedPermissions);

				// Assert
				foreach (var expectedPermission in expectedPermissioinsList)
				{
					Assert.That(proposedPermissionsList, Does.Contain(expectedPermission), $"Expected permission '{expectedPermission}' on object of one of the types 'FN', N'FS' or 'AF' must be present in the proposed permissions list.");
				}
			}
		}

		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SharedRef })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Main })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Audit })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.EDW })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SD })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.UserRepository })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.ExclusiveRef })]
		public void TestCorrectPrincipalsAndMembershipsListIsGeneratedForApplicationUsersAndRoles(DatabaseTestMode databaseTestMode, DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			var expectedPrincipalsAndMemberships =
				$@"-- Expected principals and memberships
SELECT * FROM (VALUES
		{string.Join($",{System.Environment.NewLine}\t\t", ApplicationUsersAndRoles.PrincipalsAndMemberships(Db.DatabaseName))}
) AS PER (member_name, member_type, parent_role, default_schema_name)
";
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				var expectedPrincipalsList = Helper.DatabasePrincipalsList(adminConnection, expectedPrincipalsAndMemberships);
				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, databaseType);
				var proposedPrincipalsList = Helper.DatabasePrincipalsList(adminConnection, result.ProposedPrincipalsAndMemberships);

				// Assert
				Assert.That(proposedPrincipalsList, Is.EquivalentTo(expectedPrincipalsList), "Expected and proposed principals and memberships should be the same.");
			}
		}

		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SharedRef })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Audit })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.EDW })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SD })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.UserRepository })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.ExclusiveRef })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Main })]
		public void TestCorrectPermissionsListIsGeneratedForApplicationUsersAndRoles(DatabaseTestMode databaseTestMode, DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				Assume.That(Helper.GetFnAndFsAndAFObjectsNames(adminConnection), Is.Empty, "There should be no custom objects of type 'FN', 'FS' or 'AF' in the database");
				Assume.That(Helper.GetCustomTypeNames(adminConnection), Is.Empty, "There should be no cutom types in the database");

				DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, "OrderTracking");
				DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, DbSecurity.SqlHrmSchema);
				DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, DbSecurity.SqlCdcSchema);
				DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, DbSecurity.SqlStagingSchema);

				adminConnection.ExecuteReader(
					$@"
SELECT name
FROM sys.schemas
WHERE 1=1
	AND name not in (N'sys', N'guest', N'INFORMATION_SCHEMA')
	AND name not like N'db[_]%'
",
					record =>
					{
						Assume.That(
							new[] { "dbo", "OrderTracking", DbSecurity.SqlHrmSchema, DbSecurity.SqlCdcSchema, DbSecurity.SqlStagingSchema },
							Does.Contain(record["name"]),
							$@"
Schema {record["name"]} in not accounted for in testing.
Test assumes that there are no schemas in the database other than in-built schemas and 'OrderTracking', 'hrm', cdc', 'Staging'.
If schema {record["name"]} should now always be present in the database,
please add assertions for permissions on this schema in this test.
E.g. {DbRoleTypes.CwRestrictedReaderRole} should have the same permisisons on ALL schemas in the database, except 'sys', N'guest', N'INFORNATION_SCHEAM', and schemas starting from 'db_'.
Then add this schema name to the list of schemas for this assumption.
");
					});

				var expectedPermissions = ApplicationUsersAndRoles.Permissions.ByDatabaseType(databaseType, Db.DatabaseName);
				var expectedPermissioinsList = Helper.DatabasePermissionsList(adminConnection,
									$@"
SELECT * FROM (VALUES
{string.Join($",{System.Environment.NewLine}\t\t", expectedPermissions)}
 ) AS PER (State, Permission, SecurableType, SecurableSchema, Securable, SecurableColumn, Grantee, Grantor)
");
				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, databaseType);
				var proposedPermissionsList = Helper.DatabasePermissionsList(adminConnection, result.ProposedPermissions);

				// Assert
				Assert.That(proposedPermissionsList, Is.EquivalentTo(expectedPermissioinsList).IgnoreCase, "Expected permissions should be the same as proposed permissionis.");
			}
		}

		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SingleSharedRef })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SharedRef })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Audit })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.EDW })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SD })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.UserRepository })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.ExclusiveRef })]
		public void TestNoPermissionIsGeneratedOnUserDefinedTableValuedTypeInNonMainDatabases(DatabaseTestMode databaseTestMode, DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				Helper.CreateType(adminConnection, "dbo", randomTypeName);
				Helper.EnsureSchema(adminConnection, randomSchemaName);
				Helper.CreateType(adminConnection, randomSchemaName, randomTypeName);

				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, databaseType);
				var proposedPermissionsList = Helper.DatabasePermissionsList(adminConnection, result.ProposedPermissions);

				// Assert

				Assert.That(
					proposedPermissionsList.Where(permission => permission.Securable == randomTypeName && permission.SecurableType == "TYPE"),
					Is.Empty,
					$"There should be no permissions on custom types.");
			}
		}

		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SingleSharedRef })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SharedRef })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Audit })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.EDW })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SD })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.UserRepository })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.ExclusiveRef })]
		public void TestNoPermissionIsGeneratedOnFAAndAFAAndFSTypeFunctionInNonMainDatabases(DatabaseTestMode databaseTestMode, DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			// This assembly location is implemented as Enterprise.SqlSecurity.Test.SqlClrAssembly cannot be multi-targeted to Net8
			var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
#if NET
			assemblyLocation = Path.GetDirectoryName(assemblyLocation);
#endif

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				adminConnection.ExecuteNonQuery($"ALTER DATABASE {databaseName.QuoteName()} SET TRUSTWORTHY ON;");

				adminConnection.ExecuteNonQuery($@"
CREATE ASSEMBLY [{assemblyName}]
FROM '{assemblyLocation}\\Enterprise.SqlSecurity.Test.SqlClrAssembly.dll'
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

				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, databaseType);
				var proposedPermissionsList = Helper.DatabasePermissionsList(adminConnection, result.ProposedPermissions);

				// Assert
				Assert.That(
					proposedPermissionsList.Where(permission => (permission.Securable == randomClrScalarFnName || permission.Securable == randomScalarFnName || permission.Securable == randomClrAggregateFnName) && permission.SecurableType == "OBJECT"),
					Is.Empty,
					$"There should be no permissions on functions of type FN, FS or AF.");
			}
		}

		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SharedRef })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Audit })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.EDW })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SD })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.UserRepository })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.ExclusiveRef })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Main })]
		public void TestCorrectPermissionsListIsGeneratedForAnyRandomSchema(DatabaseTestMode databaseTestMode, DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				var randomSchemaName = $"RandomSchema_{databaseType}";
				Helper.EnsureSchemasForDatabase(adminConnection, databaseName, randomSchemaName, DbSecurity.SqlCdcSchema, DbSecurity.SqlHrmSchema, DbSecurity.SqlStagingSchema);

				DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, DbSecurity.SqlCdcSchema);
				DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, randomSchemaName);
				DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, DbSecurity.SqlHrmSchema);
				DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, DbSecurity.SqlStagingSchema);

				var expectedPermissions = ApplicationUsersAndRoles.Permissions.ByDatabaseType(databaseType, Db.DatabaseName);

				expectedPermissions = expectedPermissions.Concat(new[]
				{
					$"('G', N'EXECUTE', N'SCHEMA', N'', N'{randomSchemaName}', N'', N'{DbRoleTypes.CwRestrictedReaderRole}', N'')",
					$"('G', N'SELECT', N'SCHEMA', N'','{randomSchemaName}', N'', N'{DbRoleTypes.CwRestrictedReaderRole}', N'')",

					$"('G', N'EXECUTE', N'SCHEMA', N'', N'{randomSchemaName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'SELECT', N'SCHEMA', N'', N'{randomSchemaName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'INSERT', N'SCHEMA', N'', N'{randomSchemaName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'DELETE', N'SCHEMA', N'', N'{randomSchemaName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'UPDATE', N'SCHEMA', N'', N'{randomSchemaName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'ALTER', N'SCHEMA', N'', N'{randomSchemaName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'{randomSchemaName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",

					$"('G', N'VIEW CHANGE TRACKING', N'SCHEMA', N'', N'{randomSchemaName}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'VIEW CHANGE TRACKING', N'SCHEMA', N'', N'{DbSecurity.SqlCdcSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'VIEW CHANGE TRACKING', N'SCHEMA', N'', N'{DbSecurity.SqlStagingSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'VIEW CHANGE TRACKING', N'SCHEMA', N'', N'{DbSecurity.SqlHrmSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'{randomSchemaName}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'{DbSecurity.SqlCdcSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'{DbSecurity.SqlStagingSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'{DbSecurity.SqlHrmSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
				});

				var expectedPermissioinsList = Helper.DatabasePermissionsList(
					adminConnection,
					$@"
SELECT * FROM (VALUES
{string.Join($",{System.Environment.NewLine}\t\t", expectedPermissions)}
 ) AS PER (State, Permission, SecurableType, SecurableSchema, Securable, SecurableColumn, Grantee, Grantor)
");

				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, databaseType);
				var proposedPermissionsList = Helper.DatabasePermissionsList(adminConnection, result.ProposedPermissions);

				// Assert
				Assert.That(proposedPermissionsList, Is.EquivalentTo(expectedPermissioinsList).IgnoreCase, "Expected permissions should be the same as proposed permissionis.");
			}
		}

		#region Implementation

		AdminConnection adminConnection;
		readonly SqlConnectionStringBuilder connectionStringBuilderToMasterDatabase;
		const string assemblyName = "TestSqlClrAssembly";

		readonly string dbReaderLogin = CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbWriterLogin = CargoWiseWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedReaderLogin = RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedWriterLogin = RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbUnrestrictedWriterLogin = UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);

		readonly string randomScalarFnName = "b5f923a9-d01d-4b75-b808-c4c14c58411a";
		readonly string randomClrScalarFnName = "d093d880-2b9b-4947-ae0b-bc5973eef21e";
		readonly string randomSchemaName = "f4abdbee-8957-495d-8d4e-0426173b2b7f";
		readonly string randomClrAggregateFnName = "05fa6101-4235-4ba9-9e4b-1436f0101884";
		readonly string randomTypeName = "TestTVType";

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			adminConnection = Db.NewAdminConnection();
			Helper.EnsureExtraDatabasesWithSchemas(adminConnection);
		}

		[SetUp]
		public void Setup()
		{
			CleanUp();
		}

		[TearDown]
		public void TearDown()
		{
			CleanUp();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			adminConnection?.Dispose();
		}

		void CleanUp()
		{
			var schemasToEnsureMissing = new[] { "RandomSchema" };

			foreach (var database in Helper.Databases())
			{
				Helper.EnsureSchemasMissingForDatabase(adminConnection, database, schemasToEnsureMissing);

				using (((ICurrentDbControl)adminConnection).UseDatabase(database))
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
		}

		#endregion Implementation
	}
}
