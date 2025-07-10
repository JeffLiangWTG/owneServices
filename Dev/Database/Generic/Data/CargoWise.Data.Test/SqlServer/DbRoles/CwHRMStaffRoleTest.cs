namespace CargoWise.Data.Testing
{
	sealed class CwHRMStaffRoleTest : DbRoleTestCase<CwHRMStaffRole>
	{
		public void TestRoleName()
		{
			AssertEquals("cwHRMStaffRole", DbRole.Name);
		}

		public void TestGetDbSchemas()
		{
			var dbSchemas = DbRole.GetDbSchemas(TestConnection);

			AssertEquals(1, dbSchemas.Length);
			AssertEquals(DbSecurity.SqlHrmSchema, dbSchemas[0]);
		}

		public void TestDbSchemaPermissions()
		{
			var dbSchemaPermissions = DbRole.DbSchemaPermissions;

			AssertEquals(1, dbSchemaPermissions.Length);
			AssertEquals("SELECT", dbSchemaPermissions[0]);
		}

		public void TestCreateDbRoleAndGrantPermissionsSQL()
		{
			var sql = DbRole.CreateDbRoleAndGrantPermissionsSQL(TestConnection);

			AssertContains("CREATE ROLE [cwHRMStaffRole];", sql);

			foreach (var schemaName in AllSchemaNames)
			{
				if (schemaName == DbSecurity.SqlHrmSchema)
				{
					AssertContains($"GRANT SELECT ON SCHEMA::[{schemaName}] TO [cwHRMStaffRole];", sql);
				}
				else
				{
					AssertNotContains(schemaName, sql);
				}
			}
		}

		protected override bool CleanUpTestDbRole => false;
		protected override CwHRMStaffRole GetTestRole() => new CwHRMStaffRole();
	}
}
