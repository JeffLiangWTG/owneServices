using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public class DbSecurityNonTransactionedTest : TestCase
	{
		public void AssertDatabasePrincipal(AdminConnection connection, string dbName, string principalName, bool expectedExists, string message = "")
		{
			var sqlText = string.Format(@"
				SELECT count(*)
				FROM [{0}].sys.database_principals dl
				WHERE dl.name = '{1}'",
				dbName, principalName);
			var actualExists = ((int)connection.ExecuteScalar(sqlText) == 1);
			AssertEquals(string.Format("{0}: Principal [{1}] exists on database [{2}]?", message, principalName, dbName), expectedExists, actualExists);
		}

		#region Refresh Database Reader Role

		public void TestRefreshDbReaderRolePermissionsSkipsReadonlyDatabases()
		{
			var testMainDb = "TestRefreshDbReaderRolePermissionsSkipsReadonlyDatabases";
			var testEdoc1Db = testMainDb + "_SD001";
			var testEdoc2Db = testMainDb + "_SD002";

			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, testMainDb);
				AdoTestUtils.DropDbIfExists(adminConnection, testEdoc1Db);
				AdoTestUtils.DropDbIfExists(adminConnection, testEdoc2Db);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConnection, testMainDb);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, testEdoc1Db);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, testEdoc2Db);

					adminConnection.AlterDbWriteableState(testEdoc2Db, false);

					using (var connection = Db.NewAdminConnection(Db.ServerName, testMainDb))
					{
						new DbSecurity().RefreshDbReaderRolePermissions(connection, msg => { });

						DbSecurityAdminConnectionTest.AssertDatabaseRoleExists(connection, testMainDb, DbSecurity.CwReaderRole, true);
						DbSecurityAdminConnectionTest.AssertDatabaseRoleExists(connection, testEdoc1Db, DbSecurity.CwReaderRole, true);
						DbSecurityAdminConnectionTest.AssertDatabaseRoleExists(connection, testEdoc2Db, DbSecurity.CwReaderRole, false);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, testMainDb);
					AdoTestUtils.DropDbIfExists(adminConnection, testEdoc1Db);
					AdoTestUtils.DropDbIfExists(adminConnection, testEdoc2Db);
				}
			}
		}

		#endregion
	}
}
