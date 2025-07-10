using System.Linq;

namespace CargoWise.Data.Testing
{
	sealed class CwRestrictedReaderRoleTest : DbRoleTestCase<CwRestrictedReaderRole>
	{
		public void TestRoleName()
		{
			AssertEquals("cwRestrictedReaderRole", DbRole.Name);
		}

		public void TestDbDatabasePermissions()
		{
			var dbDatabasePermissions = DbRole.DbDatabasePermissions;

			AssertContainsExactElementsInAnyOrder(new[] { "SHOWPLAN", "VIEW DEFINITION" }, dbDatabasePermissions);
		}

		public void TestGetDbSchemasIsEquivalentToAllSchemaNamesWithoutLockedSchemas()
		{
			var blockedSchemas = new string[] { DbSecurity.SqlHrmSchema, DbSecurity.SqlCdcSchema, DbSecurity.SqlStagingSchema };
			var expectedSchemas = AllSchemaNames.Where(schema => !blockedSchemas.Contains(schema)).ToArray();
			var dbSchemas = DbRole.GetDbSchemas(TestConnection);

			AssertEquals(expectedSchemas.Length, dbSchemas.Length);
			AssertContainsExactElementsInAnyOrder(expectedSchemas, dbSchemas);
		}

		public void TestDbSchemaPermissions()
		{
			var dbSchemaPermissions = DbRole.DbSchemaPermissions;

			AssertEquals(2, dbSchemaPermissions.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "SELECT", "EXECUTE" }, dbSchemaPermissions);
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

			AssertContains("Need to have create role command", "CREATE ROLE [cwRestrictedReaderRole];", sql);
			AssertNotContains("Should not have hrm schema", "SCHEMA::[hrm] TO [cwRestrictedReaderRole]", sql);

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
					AssertContains($"Should have {schemaName} schema", $"GRANT SELECT, EXECUTE ON SCHEMA::[{schemaName}] TO [cwRestrictedReaderRole];", sql);
				}
			});

			AssertContains($"GRANT SELECT ON sys.sql_expression_dependencies TO [cwRestrictedReaderRole];", sql);
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

			TestConnection.ExecuteNonQuery("GRANT SELECT ON sys.sql_modules TO [cwRestrictedReaderRole];");
			TestConnection.ExecuteNonQuery("REVOKE SELECT ON sys.sql_expression_dependencies FROM [cwRestrictedReaderRole];");

			DbRole.CreateRoleAndPermissionGrantCommandsIfRequired(TestConnection, TestConnection.CurrentDatabase, out var cmdBuilder2);
			var sql2 = cmdBuilder2.ToString();

			AssertContains("REVOKE SELECT ON sys.sql_modules FROM [cwRestrictedReaderRole];", sql2);
			AssertContains("GRANT SELECT ON sys.sql_expression_dependencies TO [cwRestrictedReaderRole];", sql2);

			TestConnection.ExecuteNonQuery(sql2);

			DbRole.CreateRoleAndPermissionGrantCommandsIfRequired(TestConnection, TestConnection.CurrentDatabase, out var cmdBuilder3);
			var sql3 = cmdBuilder3.ToString();
			Assert(string.IsNullOrWhiteSpace(sql3));
		}

		[UseSnapshotProtection]
		public void TestViewUserDefinedFunction()
		{
			var sqlCreateUserDefindedFunction = @" 
CREATE FUNCTION dbo.TestUfnGetGlbStaffCount ()
RETURNS int   
AS   
-- Returns the stock level for the product.  
BEGIN  
    DECLARE @ret int;  
    SELECT @ret = COUNT(*)   
    FROM dbo.GlbStaff p; 
    IF (@ret IS NULL)   
        SET @ret = 0;  
    RETURN @ret;  
END; 
";
			TestConnection.ExecuteNonQuery(sqlCreateUserDefindedFunction);

			var restricedReaderConnection = Db.NewExtraConnectionToMainDbWithReaderCredentials();
			var resultFunctionDefinition = restricedReaderConnection.ExecuteScalar<string>("SELECT OBJECT_DEFINITION (OBJECT_ID('dbo.TestUfnGetGlbStaffCount')) AS ObjectDefinition;");

			var sqlViewFunctionDependency = @"SELECT OBJECT_NAME(sed.referencing_id) AS referencing_entity_name    
FROM sys.sql_expression_dependencies AS sed  
INNER JOIN sys.objects AS o ON sed.referencing_id = o.object_id 
WHERE sed.referencing_id = OBJECT_ID('dbo.TestUfnGetGlbStaffCount'); ";

			var resultDependency = restricedReaderConnection.ExecuteScalar<string>(sqlViewFunctionDependency);

			AssertEquals(sqlCreateUserDefindedFunction, resultFunctionDefinition);
			AssertEquals("TestUfnGetGlbStaffCount", resultDependency);
		}

		protected override bool CleanUpTestDbRole => false;
		protected override CwRestrictedReaderRole GetTestRole() => new CwRestrictedReaderRole();
	}
}
