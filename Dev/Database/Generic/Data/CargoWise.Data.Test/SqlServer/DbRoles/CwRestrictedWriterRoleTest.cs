using System.Linq;

namespace CargoWise.Data.Testing
{
	sealed class CwRestrictedWriterRoleTest : DbRoleTestCase<CwRestrictedWriterRole>
	{
		public void TestRoleName()
		{
			AssertEquals("cwRestrictedWriterRole", DbRole.Name);
		}

		public void TestDbDatabasePermissions()
		{
			var dbDatabasePermissions = DbRole.DbDatabasePermissions;

			AssertContainsExactElementsInAnyOrder(new[] { "ALTER", "CREATE SCHEMA", "VIEW DATABASE STATE", "VIEW DEFINITION", "SHOWPLAN", "REFERENCES" }, dbDatabasePermissions);
		}

		public void TestGetDbSchemasIsEquivalentToAllSchemaNamesWithoutLockedSchemas()
		{
			var lockedSchemas = new string[] { DbSecurity.SqlHrmSchema, DbSecurity.SqlCdcSchema, DbSecurity.SqlStagingSchema };
			var expectedSchemas = AllSchemaNames.Where(schema => !lockedSchemas.Contains(schema)).ToArray();
			var actualSchemas = DbRole.GetDbSchemas(TestConnection);

			AssertEquals(expectedSchemas.Length, actualSchemas.Length);
			AssertContainsExactElementsInAnyOrder(expectedSchemas, actualSchemas);
		}

		public void TestDbSchemaPermissions()
		{
			var dbSchemaPermissions = DbRole.DbSchemaPermissions;

			AssertEquals(7, dbSchemaPermissions.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "EXECUTE", "ALTER", "CREATE SEQUENCE" }, dbSchemaPermissions);
		}

		public void TestDbObjectPermissions()
		{
			var dbObjectPermissions = DbRole.DbObjectPermissions;

			AssertEquals(1, dbObjectPermissions.Count);
			AssertEquals("SELECT", dbObjectPermissions[0].Permission);
			AssertEquals("sys.sql_expression_dependencies", dbObjectPermissions[0].ObjectFullName);
		}

		public void TestCreateDbRoleAndGrantPermissionsSQL()
		{
			var lockedSchemas = new string[] { DbSecurity.SqlHrmSchema, DbSecurity.SqlCdcSchema, DbSecurity.SqlStagingSchema };

			var sql = DbRole.CreateDbRoleAndGrantPermissionsSQL(TestConnection);
			var dbSchemas = DbRole.GetDbSchemas(TestConnection);

			AssertNotContains("Should not have hrm schema", "SCHEMA::[hrm] TO [cwRestrictedWriterRole]", sql);
			AssertContains("CREATE ROLE [cwRestrictedWriterRole];\r\nGRANT ALTER, CREATE SCHEMA, VIEW DATABASE STATE, VIEW DEFINITION, SHOWPLAN, REFERENCES TO [cwRestrictedWriterRole];", sql);

			CombineAssertions("Locked schemas should not be included in the query to grant permission, but the following were:\r\n", () =>
			{
				foreach (var lockedSchemaName in lockedSchemas)
				{
					AssertNotContains(lockedSchemaName, lockedSchemaName, sql);
				}
			});

			CombineAssertions("The following required schemas are not included in the role creation script:\r\n", () =>
			{
				foreach (var schemaName in dbSchemas)
				{
					AssertContains($"GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE, ALTER, CREATE SEQUENCE ON SCHEMA::[{schemaName}] TO [cwRestrictedWriterRole];", sql);
				}
			});

			AssertContains($"GRANT SELECT ON sys.sql_expression_dependencies TO [cwRestrictedWriterRole];", sql);
		}

		[UseSnapshotProtection]
		public void TestCreateRoleAndPermissionGrantCommandsIfRequired()
		{
			DbRole.EnsureExists(TestConnection, TestConnection.CurrentDatabase);
			DbRole.CreateRoleAndPermissionGrantCommandsIfRequired(TestConnection, TestConnection.CurrentDatabase, out var cmdBuilder1);
			var sql1 = cmdBuilder1.ToString();
			if (!string.IsNullOrWhiteSpace(sql1))
			{
				TestConnection.ExecuteNonQuery(sql1);
			}

			TestConnection.ExecuteNonQuery("GRANT SELECT ON sys.sql_modules TO [cwRestrictedWriterRole];");
			TestConnection.ExecuteNonQuery("REVOKE SELECT ON sys.sql_expression_dependencies FROM [cwRestrictedWriterRole];");

			DbRole.CreateRoleAndPermissionGrantCommandsIfRequired(TestConnection, TestConnection.CurrentDatabase, out var cmdBuilder2);
			var sql2 = cmdBuilder2.ToString();

			AssertContains("REVOKE SELECT ON sys.sql_modules FROM [cwRestrictedWriterRole];", sql2);
			AssertContains("GRANT SELECT ON sys.sql_expression_dependencies TO [cwRestrictedWriterRole];", sql2);

			TestConnection.ExecuteNonQuery(sql2);

			DbRole.CreateRoleAndPermissionGrantCommandsIfRequired(TestConnection, TestConnection.CurrentDatabase, out var cmdBuilder3);
			var sql3 = cmdBuilder3.ToString();
			Assert(string.IsNullOrWhiteSpace(sql3));
		}

		protected override bool CleanUpTestDbRole => false;
		protected override CwRestrictedWriterRole GetTestRole() => new CwRestrictedWriterRole();
	}
}
