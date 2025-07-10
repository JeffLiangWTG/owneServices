using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	[UseSnapshotProtection]
	sealed class DbSecurityAdminConnectionTest : TestCase
	{
		#region Refresh Database Reader Role

		public void TestRefreshDbReaderRolePermissions_ReferencestheCorrectEDWServer()
		{
			var origBIAuditServer = DbRegistry.BiAuditServer.LoadValue(TestConnection);
			var origBIDatawarehouseServer = DbRegistry.BiDataWarehouseServer.LoadValue(TestConnection);
			try
			{
				DbRegistry.BiAuditServer.SaveValue("goofy", TestConnection);
				DbRegistry.BiDataWarehouseServer.SaveValue("goober", TestConnection);

				var testSecurity = new DbSecurityForTest();

				CombineAssertions(() =>
				{
					AssertEquals(true, testSecurity.AuditServer_Exposed(TestConnection).Equals("goofy"));
					AssertEquals(true, testSecurity.DataWarehouseServer_Exposed(TestConnection).Equals("goober"));
				});
			}
			finally
			{
				RestoreRegistryValues(origBIAuditServer, origBIDatawarehouseServer);
			}
		}

		void RestoreRegistryValues(string origBIAuditServer, string origBIDatawarehouseServer)
		{
			if (origBIAuditServer != null)
			{
				DbRegistry.BiAuditServer.SaveValue(origBIAuditServer, TestConnection);
			}
			if (origBIDatawarehouseServer != null)
			{
				DbRegistry.BiDataWarehouseServer.SaveValue(origBIDatawarehouseServer, TestConnection);
			}
		}

		public void TestRefreshDbReaderRolePermissions_ExcludesEDWServerWhenRegistryNotSet()
		{
			var origBIAuditServer = DbRegistry.BiAuditServer.LoadValue(TestConnection);
			var origBIDatawarehouseServer = DbRegistry.BiDataWarehouseServer.LoadValue(TestConnection);
			try
			{
				var testSecurity = new DbSecurityForTest();

				DbRegistry.BiAuditServer.SaveValue(TestConnection.ServerName, TestConnection);
				DbRegistry.BiDataWarehouseServer.SaveValue(string.Empty, TestConnection);

				RenameDbToSimulateDBNotExist($"{TestConnection.CurrentDatabase}_EDW", $"{TestConnection.CurrentDatabase}_EDW_RENAMED");

				var message = string.Empty;

				try
				{
					testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });
				}
				catch (SqlException ex)
				{
					message = ex.Message;
				}
				AssertNotEquals("EDW should not be included in the list of DB's for role refresh", $"Cannot open database \"{TestConnection.CurrentDatabase}_EDW\" requested by the login. The login failed.\r\nLogin failed for user 'OdysseyAdmin'.".Equals(message));
			}
			finally
			{
				RestoreRegistryValues(origBIAuditServer, origBIDatawarehouseServer);

				RenameDbToSimulateDBNotExist($"{TestConnection.CurrentDatabase}_EDW_RENAMED", $"{TestConnection.CurrentDatabase}_EDW");
			}
		}

		void RenameDbToSimulateDBNotExist(string oldDbName, string newDbName)
		{
			var renameEDWForTestToSimulateDBNotExist = string.Format(@"
											ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE

											ALTER DATABASE {0} MODIFY NAME = {1};

											ALTER DATABASE {1} SET MULTI_USER
											", oldDbName, newDbName);
			TestConnection.ExecuteNonQuery(renameEDWForTestToSimulateDBNotExist);
		}

		public void TestRefreshDbReaderRolePermissions_ExcludesAuditServerWhenRegistryNotSet()
		{
			var origBIAuditServer = DbRegistry.BiAuditServer.LoadValue(TestConnection);
			var origBIDatawarehouseServer = DbRegistry.BiDataWarehouseServer.LoadValue(TestConnection);
			try
			{
				var testSecurity = new DbSecurityForTest();

				DbRegistry.BiAuditServer.SaveValue(string.Empty, TestConnection);
				DbRegistry.BiDataWarehouseServer.SaveValue(TestConnection.ServerName, TestConnection);

				RenameDbToSimulateDBNotExist($"{TestConnection.CurrentDatabase}_AUDIT", $"{TestConnection.CurrentDatabase}_AUDIT_RENAMED");

				var message = string.Empty;

				try
				{
					testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });
				}
				catch (SqlException ex)
				{
					message = ex.Message;
				}
				AssertNotEquals("AUDIT should not be included in the list of DB's for role refresh", $"Cannot open database \"{TestConnection.CurrentDatabase}_AUDIT\" requested by the login. The login failed.\r\nLogin failed for user 'OdysseyAdmin'.".Equals(message));
			}
			finally
			{
				if (origBIAuditServer != null)
				{
					DbRegistry.BiAuditServer.SaveValue(origBIAuditServer, TestConnection);
				}
				if (origBIDatawarehouseServer != null)
				{
					DbRegistry.BiDataWarehouseServer.SaveValue(origBIDatawarehouseServer, TestConnection);
				}

				RenameDbToSimulateDBNotExist($"{TestConnection.CurrentDatabase}_AUDIT_RENAMED", $"{TestConnection.CurrentDatabase}_AUDIT");
			}
		}

		public void TestRefreshDbReaderRolePermissions_fixesCargoWiseReaderLogin()
		{
			var testSecurity = new DbSecurity();
			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });

			var cwReaderRoleUid = TestConnection.ExecuteScalar("select Principal_Id from sys.Database_Principals where name = 'cwReaderRole'");
			var cargoWiseReaderLoginUid = TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
				"select Principal_Id from sys.database_Principals where name = '{0}_CargoWiseReaderLogin'", TestConnection.CurrentDatabase));
			TestConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture,
				"ALTER ROLE [{0}] DROP MEMBER [{1}]", "cwReaderRole", TestConnection.CurrentDatabase + "_CargoWiseReaderLogin"));
			AssertEquals(0, TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
				"select count(*) from sys.database_role_members where role_principal_id = {0} and member_principal_id = {1}", cwReaderRoleUid, cargoWiseReaderLoginUid)));

			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });
			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });

			AssertEquals(1, TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
				"select count(*) from sys.database_role_members where role_principal_id = {0} and member_principal_id = {1}", cwReaderRoleUid, cargoWiseReaderLoginUid)));
		}

		public void TestRefreshDbReaderRolePermissions_FixesCargoWiseReaderLogin_AuditDatabase()
		{
			var testSecurity = new DbSecurity();
			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });

			int cwReaderRoleUid, cargoWiseReaderLoginUid;
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				cwReaderRoleUid = Convert.ToInt32(TestConnection.ExecuteScalar("select Principal_Id from sys.Database_Principals where name = 'cwReaderRole'"));
				cargoWiseReaderLoginUid = Convert.ToInt32(TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
					"select Principal_Id from sys.database_Principals where name = '{0}_CargoWiseReaderLogin'", Db.DatabaseName)));

				AssertEquals(1, TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
				"select count(*) from sys.database_role_members where role_principal_id = {0} and member_principal_id = {1}", cwReaderRoleUid, cargoWiseReaderLoginUid)));

				TestConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture,
					"ALTER ROLE [{0}] DROP MEMBER [{1}]", "cwReaderRole", Db.DatabaseName + "_CargoWiseReaderLogin"));
				AssertEquals(0, TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
					"select count(*) from sys.database_role_members where role_principal_id = {0} and member_principal_id = {1}", cwReaderRoleUid, cargoWiseReaderLoginUid)));
			}

			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });

			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				AssertEquals(1, TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
				"select count(*) from sys.database_role_members where role_principal_id = {0} and member_principal_id = {1}", cwReaderRoleUid, cargoWiseReaderLoginUid)));
			}
		}

		public void TestRefreshDbReaderRolePermissions_FixesCargoWiseReaderLogin_EdwDatabase()
		{
			var testSecurity = new DbSecurity();
			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });

			int cwReaderRoleUid, cargoWiseReaderLoginUid;
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				cwReaderRoleUid = Convert.ToInt32(TestConnection.ExecuteScalar("select Principal_Id from sys.Database_Principals where name = 'cwReaderRole'"));
				cargoWiseReaderLoginUid = Convert.ToInt32(TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
					"select Principal_Id from sys.database_Principals where name = '{0}_CargoWiseReaderLogin'", Db.DatabaseName)));

				AssertEquals(1, TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
				"select count(*) from sys.database_role_members where role_principal_id = {0} and member_principal_id = {1}", cwReaderRoleUid, cargoWiseReaderLoginUid)));

				TestConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture,
					"ALTER ROLE [{0}] DROP MEMBER [{1}]", "cwReaderRole", Db.DatabaseName + "_CargoWiseReaderLogin"));
				AssertEquals(0, TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
					"select count(*) from sys.database_role_members where role_principal_id = {0} and member_principal_id = {1}", cwReaderRoleUid, cargoWiseReaderLoginUid)));
			}

			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });

			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				AssertEquals(1, TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
				"select count(*) from sys.database_role_members where role_principal_id = {0} and member_principal_id = {1}", cwReaderRoleUid, cargoWiseReaderLoginUid)));
			}
		}

		public static void AssertDatabaseRoleExists(AdminConnection connection, string dbName, string roleName, bool expected)
		{
			var sqlText = string.Format(@"
				SELECT count(*)
				FROM [{0}].sys.database_principals dr
				WHERE dr.type = 'R'
				AND dr.name = '{1}'",
				dbName, roleName);
			var actual = ((int)connection.ExecuteScalar(sqlText) == 1);
			AssertEquals(string.Format("Does role [{0}] exist on database [{1}]?", roleName, dbName), expected, actual);
		}

		#endregion

		#region Stored Procedure Authorization

		public void TestStoredProcedureAuthorization()
		{
			var schemaName = Db.SqlDbOwnerSchema;
			var tableName = "_Test_Table";
			var procedureName = "_Test_Procedure";
			var readonlyUser = "EnterpriseDbUser_##ReadonlyUser##";

			// Prepare data
			var dbSecurity = new DbSecurity();
			dbSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });
			CreateTestObjects(TestConnection, schemaName, tableName, procedureName, readonlyUser);

			var expected = new string[]
				{
					"id = 1, value = 0",
					"id = 2, value = 0",
					"id = 3, value = 0",
				};
			AssertContainsExactElementsInAnyOrder("PRECONDITION", expected, GetTestValues(TestConnection, schemaName, tableName));

			// running procedure in current user context (Application)
			var sql = string.Format(CultureInfo.InvariantCulture, @"EXEC [{0}].[{1}] @id = 1;", schemaName, procedureName);
			TestConnection.ExecuteNonQuery(sql);
			expected = new string[]
				{
					"id = 1, value = 1",
					"id = 2, value = 0",
					"id = 3, value = 0",
				};
			AssertContainsExactElementsInAnyOrder("Application user", expected, GetTestValues(TestConnection, schemaName, tableName));

			// running procedure in read-only user context
			sql = string.Format(CultureInfo.InvariantCulture, "EXEC [{0}].[{1}] @id = 2;", schemaName, procedureName);
			try
			{
				TestConnection.ExecuteNonQuery($"EXECUTE AS USER = '{readonlyUser}'");
				TestConnection.ImpersonatedLogin = readonlyUser;

				TestConnection.ExecuteNonQuery(sql);
				Fail("Exception expected");
			}
			catch (SqlException ex)
			{
				AssertEquals(string.Format("The UPDATE permission was denied on the object '{0}', database '{1}', schema '{2}'.", tableName, TestConnection.CurrentDatabase, schemaName), ex.Message);
			}
			finally
			{
				TestConnection.ExecuteNonQuery("REVERT");
				TestConnection.ImpersonatedLogin = null;
			}

			// no changes done
			AssertContainsExactElementsInAnyOrder("Read-only user", expected, GetTestValues(TestConnection, schemaName, tableName));

			// running procedure in current user context (Application)
			sql = string.Format(CultureInfo.InvariantCulture, "EXEC [{0}].[{1}] @id = 3;", schemaName, procedureName);
			TestConnection.ExecuteNonQuery(sql);
			expected = new string[]
				{
					"id = 1, value = 1",
					"id = 2, value = 0",
					"id = 3, value = 1",
				};
			AssertContainsExactElementsInAnyOrder("Admin user", expected, GetTestValues(TestConnection, schemaName, tableName));
		}

		void CreateTestObjects(AdminConnection connection, string schemaName, string tableName, string procedureName, string readonlyUser)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
if (OBJECT_ID(N'[{0}].[{2}]', N'P') is NOT NULL) DROP PROCEDURE [{0}].[{2}];
if (OBJECT_ID(N'[{0}].[{1}]', N'U') is NOT NULL) DROP TABLE [{0}].[{1}];

CREATE TABLE [{0}].[{1}]
(
	id    int,
	value int,
);

INSERT [{0}].[{1}] (id, value) VALUES
-- (id, value)
   (1 , 0    ),
   (2 , 0    ),
   (3 , 0    );
"
				, schemaName    // 0
				, tableName     // 1
				, procedureName // 2
				);
			connection.ExecuteNonQuery(sql);

			sql = string.Format(CultureInfo.InvariantCulture, @"
CREATE PROCEDURE [{0}].[{2}]
	@id int
AS

UPDATE [{0}].[{1}] SET
	value = value + 1
WHERE
	id = @id
"
				, schemaName    // 0
				, tableName     // 1
				, procedureName // 2
				);
			connection.ExecuteNonQuery(sql);

			sql = string.Format(CultureInfo.InvariantCulture, @" -- CreateTestUser
CREATE USER [{0}] WITHOUT LOGIN;
ALTER ROLE [{1}] ADD MEMBER [{0}];
ALTER AUTHORIZATION ON [{2}].[{3}] TO [{1}];
"
				, readonlyUser            // 0
				, DbSecurity.CwReaderRole // 1
				, schemaName              // 2
				, procedureName           // 3
				);
			connection.ExecuteNonQuery(sql);
		}

		IEnumerable<string> GetTestValues(AdminConnection connection, string schemaName, string tableName)
		{
			var values = new List<string>(3);

			var sql = string.Format(CultureInfo.InvariantCulture, "SELECT id, value FROM [{0}].[{1}] ORDER BY id", schemaName, tableName);
			using (var cmd = connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					values.Add(string.Format(CultureInfo.InvariantCulture, "id = {0}, value = {1}", reader["id"], reader["value"]));
				}
			}

			return values;
		}

		#endregion // Stored Procedure Authorization

		#region Implementation

		AdminConnection TestConnection;

		#endregion

		protected override void SetUp()
		{
			TestConnection = Db.NewAdminConnection();

			base.SetUp();
		}

		protected override void TearDown()
		{
			TestConnection.Dispose();
			TestConnection = null;

			base.TearDown();
		}
	}
}
