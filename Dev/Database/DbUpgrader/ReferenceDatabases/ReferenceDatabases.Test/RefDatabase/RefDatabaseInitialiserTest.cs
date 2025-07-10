using System;
using System.Globalization;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases.Testing
{
	sealed class RefDatabaseInitialiserTest : TestCase
	{
		public void TestCreateRefDbIfNotExistsAndGuestPermission()
		{
			using (var connection = Db.NewAdminConnection())
			{
				RefDatabaseInitialiser.CreateRefDbIfNotExists(connection, testDbName);

				AssertEquals(true, connection.DatabaseExists(testDbName));
				AssertEquals("0", DataUtils.LoadDbExtendedProperty(connection, RefDbVersionProperty, testDbName));
				AssertEquals(DbRecoveryModel.Simple, DbRecoveryModelManager.GetActual(connection, testDbName));
			}

			TestGrantGuestUserPermission();
		}

		void TestGrantGuestUserPermission()
		{
			var sql = @"
SELECT COUNT(*) FROM sys.database_principals princ
INNER JOIN sys.database_permissions perm
ON princ.principal_id = perm.grantee_principal_id
WHERE name = 'guest'";
			using (var adminConnection = Db.NewAdminConnection(testDbName))
			{
				RefDatabaseInitialiser.GrantGuestUserPermission(adminConnection);
				AssertEquals(3, adminConnection.ExecuteScalar<int>(sql));
			}
		}

		public void TestCreateRefDbWithSuffix()
		{
			using (var connection = Db.NewAdminConnection())
			{
				RefDatabaseInitialiser.CreateRefDbIfNotExists(connection, testDbName);

				AssertEquals(true, connection.DatabaseExists(testDbName));
				AssertContains("Database file expected to have a date string as suffix.", testDbName + DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture), connection.GetDBDataFiles(testDbName)[0], true);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();

			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, testDbName, Db.DatabaseName);
			}
		}

		const string RefDbVersionProperty = "RefDatabaseVersion";
		readonly string testDbName = RefDbTableNameResolver.SingleRefDatabaseName + "RefDatabaseInitialiserTestRandom";
	}
}
