using System;

namespace CargoWise.Data.Testing
{
	sealed class DbRoleTest : DbRoleTestCase<DbRoleForTest>
	{
		public void TestName()
		{
			AssertEquals("testDbRole", DbRole.Name);
		}

		public void TestDbDatabasePermissions()
		{
			var databasePermissions = DbRole.DbDatabasePermissions;

			AssertEquals(2, databasePermissions.Length);

			AssertContainsExactElementsInAnyOrder(new[] { "DP1", "DP2" }, databasePermissions);
		}

		public void TestGetDbSchemas()
		{
			var dbSchemas = DbRole.GetDbSchemas(TestConnection);

			AssertEquals(3, dbSchemas.Length);

			AssertContainsExactElementsInAnyOrder(new[] { "schema1", "schema2", "schema3" }, dbSchemas);
		}

		public void TestDbSchemaPermissions()
		{
			var dbSchemasPermissions = DbRole.DbSchemaPermissions;

			AssertEquals(2, dbSchemasPermissions.Length);

			AssertContainsExactElementsInAnyOrder(new[] { "SP1", "SP2" }, dbSchemasPermissions);
		}

		public void TestCreateRoleSQL()
		{
			AssertEquals("CREATE ROLE [testDbRole];", DbRole.CreateRoleSQL);
		}

		public void TestGrantDbPermissionSQL()
		{
			AssertEquals("GRANT permissionA, permissionB TO [testDbRole];", DbRole.GrantDatabasePermissionsSQL(new[] { "permissionA", "permissionB" }));
		}

		public void TestRevokeDbPermissionsSQL()
		{
			AssertEquals("REVOKE permissionA, permissionB FROM [testDbRole];", DbRole.RevokeDatabasePermissionsSQL(new[] { "permissionA", "permissionB" }));
		}

		public void TestGrantSchemaPermissionSQL()
		{
			AssertEquals("GRANT permissionA, permissionB ON SCHEMA::[schemaA] TO [testDbRole];", DbRole.GrantSchemaPermissionSQL("schemaA", new[] { "permissionA", "permissionB" }));
		}

		public void TestRevokeSchemaPermissionSQL()
		{
			AssertEquals("REVOKE permissionA, permissionB ON SCHEMA::[schemaA] FROM [testDbRole];", DbRole.RevokeSchemaPermissionSQL("schemaA", new[] { "permissionA", "permissionB" }));
		}

		public void TestGrantObjectPermissionSQL()
		{
			AssertEquals("GRANT permissionA ON objectA TO [testDbRole];", DbRole.GrantObjectPermissionSQL("permissionA", "objectA"));
		}

		public void TestRevokeObjectPermissionSQL()
		{
			AssertEquals("REVOKE permissionA ON objectA FROM [testDbRole];", DbRole.RevokeObjectPermissionSQL("permissionA", "objectA"));
		}

		public void TestCreateDbRoleAndGrantPermissionsSQL()
		{
			AssertEquals(@"CREATE ROLE [testDbRole];
GRANT DP1, DP2 TO [testDbRole];
GRANT SP1, SP2 ON SCHEMA::[schema1] TO [testDbRole];
GRANT SP1, SP2 ON SCHEMA::[schema2] TO [testDbRole];
GRANT SP1, SP2 ON SCHEMA::[schema3] TO [testDbRole];
GRANT DP1 ON O1 TO [testDbRole];
GRANT DP2 ON O2 TO [testDbRole];", DbRole.CreateDbRoleAndGrantPermissionsSQL(TestConnection));
		}

		public void TestCreateRoleAndPermissionGrantCommandsIfRequired()
		{
			DbRole.CreateRoleAndPermissionGrantCommandsIfRequired(TestConnection, TestConnection.CurrentDatabase, out var cmdBuilder);

			AssertEquals(@"CREATE ROLE [testDbRole];
GRANT DP1, DP2 TO [testDbRole];
GRANT SP1, SP2 ON SCHEMA::[schema1] TO [testDbRole];
GRANT SP1, SP2 ON SCHEMA::[schema2] TO [testDbRole];
GRANT SP1, SP2 ON SCHEMA::[schema3] TO [testDbRole];
GRANT DP1 ON O1 TO [testDbRole];
GRANT DP2 ON O2 TO [testDbRole];", cmdBuilder.ToString().Trim());
		}

		public void TestDbObjectPermissions()
		{
			var dbObjectPermissions = DbRole.DbObjectPermissions;

			AssertEquals(2, dbObjectPermissions.Count);
			AssertEquals("DP1", dbObjectPermissions[0].Permission);
			AssertEquals("O1", dbObjectPermissions[0].ObjectFullName);
			AssertEquals("DP2", dbObjectPermissions[1].Permission);
			AssertEquals("O2", dbObjectPermissions[1].ObjectFullName);
		}

		public void TestGetDbRoleId_RoleExists()
		{
			TestConnection.ExecuteNonQuery(DbRole.CreateRoleSQL);

			AssertGreaterThanOrEqualTo(DbRole.GetDbRoleId(Db.Connection, Db.DatabaseName).Value, 1);
		}

		public void TestGetDbRoleId_RoleNotExists()
		{
			TestConnection.ExecuteNonQuery($@"IF EXISTS (SELECT * FROM sys.database_principals WHERE name = N'{DbRole.Name}' and type = 'R')
				DROP ROLE [{DbRole.Name}]");

			AssertNull(DbRole.GetDbRoleId(Db.Connection, Db.DatabaseName));
		}

		public void TestEnsureExistsArguments()
		{
			AssertExceptionThrown<ArgumentException>(() => DbRole.EnsureExists(null, TestConnection.CurrentDatabase));
			AssertExceptionThrown<ArgumentException>(() => DbRole.EnsureExists(TestConnection, null));
			AssertExceptionThrown<ArgumentException>(() => DbRole.EnsureExists(TestConnection, string.Empty));
		}

		public void TestEnsureExists()
		{
			SqlSecurityUtils.AssertDbRoleExists(TestConnection, DbRole.Name, expected: false);

			DbRole.EnsureExists(TestConnection, TestConnection.CurrentDatabase);

			SqlSecurityUtils.AssertDbRoleExists(TestConnection, DbRole.Name, expected: true);
		}

		public void TestIsDatabasePermissionExpected()
		{
			AssertEquals(true, DbRole.IsDatabasePermissionExpected("DP1"));
			AssertEquals(true, DbRole.IsDatabasePermissionExpected("DP2"));
			AssertEquals(false, DbRole.IsDatabasePermissionExpected("DP3"));
		}

		public void TestIsSchemaPermissionExpected()
		{
			AssertEquals(true, DbRole.IsSchemaPermissionExpected("SP1", "schema1", TestConnection));
			AssertEquals(true, DbRole.IsSchemaPermissionExpected("SP1", "schema2", TestConnection));
			AssertEquals(true, DbRole.IsSchemaPermissionExpected("SP1", "schema3", TestConnection));
			AssertEquals(false, DbRole.IsSchemaPermissionExpected("SP1", "schema4", TestConnection));

			AssertEquals(true, DbRole.IsSchemaPermissionExpected("SP2", "schema1", TestConnection));
			AssertEquals(true, DbRole.IsSchemaPermissionExpected("SP2", "schema2", TestConnection));
			AssertEquals(true, DbRole.IsSchemaPermissionExpected("SP2", "schema3", TestConnection));
			AssertEquals(false, DbRole.IsSchemaPermissionExpected("SP2", "schema4", TestConnection));

			AssertEquals(false, DbRole.IsSchemaPermissionExpected("SP3", "schema1", TestConnection));
			AssertEquals(false, DbRole.IsSchemaPermissionExpected("SP3", "schema2", TestConnection));
			AssertEquals(false, DbRole.IsSchemaPermissionExpected("SP3", "schema3", TestConnection));
			AssertEquals(false, DbRole.IsSchemaPermissionExpected("SP3", "schema4", TestConnection));
		}

		protected override bool CleanUpTestDbRole => true;
		protected override DbRoleForTest GetTestRole() => new DbRoleForTest();
	}
}
