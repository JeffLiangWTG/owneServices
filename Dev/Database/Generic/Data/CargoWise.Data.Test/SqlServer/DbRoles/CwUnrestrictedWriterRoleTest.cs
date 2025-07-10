namespace CargoWise.Data.Testing
{
	sealed class CwUnrestrictedWriterRoleTest : DbRoleTestCase<CwUnrestrictedWriterRole>
	{
		public void TestRoleName()
		{
			AssertEquals("cwUnrestrictedWriterRole", DbRole.Name);
		}

		public void TestGetDbSchemas()
		{
			var expectedSchemas = AllSchemaNames;
			var dbSchemas = DbRole.GetDbSchemas(TestConnection);

			AssertEquals(expectedSchemas.Length, dbSchemas.Length);
			AssertContainsExactElementsInAnyOrder(expectedSchemas, dbSchemas);
		}

		public void TestDbDatabasePermissions()
		{
			var dbDatabasePermissions = DbRole.DbDatabasePermissions;

			AssertContainsExactElementsInAnyOrder(new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "EXECUTE", "VIEW DATABASE STATE", "VIEW DEFINITION" }, dbDatabasePermissions);
		}

		public void TestDbDbSchemaPermissions()
		{
			AssertArrayEqualsByElements(new[] { "VIEW CHANGE TRACKING", "CREATE SEQUENCE" }, DbRole.DbSchemaPermissions);
		}

		public void TestCreateDbRoleAndGrantPermissionsSQL()
		{
			var sql = DbRole.CreateDbRoleAndGrantPermissionsSQL(TestConnection);

			AssertContains("CREATE ROLE [cwUnrestrictedWriterRole];\r\nGRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE, VIEW DATABASE STATE, VIEW DEFINITION TO [cwUnrestrictedWriterRole];", sql);

			foreach (var schemaName in AllSchemaNames)
			{
				AssertContains($"GRANT VIEW CHANGE TRACKING, CREATE SEQUENCE ON SCHEMA::[{schemaName}] TO [cwUnrestrictedWriterRole];", sql);
			}
		}

		protected override bool CleanUpTestDbRole => false;
		protected override CwUnrestrictedWriterRole GetTestRole() => new CwUnrestrictedWriterRole();
	}
}
