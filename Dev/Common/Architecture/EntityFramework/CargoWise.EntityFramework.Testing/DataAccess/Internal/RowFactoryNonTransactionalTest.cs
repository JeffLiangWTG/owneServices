using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class RowFactoryNonTransactionalTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestChangeDatabaseNameToAWriteableOne()
		{
			const string testDbName = "TestChangeDatabaseNameToAWriteableOneAAECE0CE68B24AC79E5088E1B7DA313D";
			string testDocDbName = Db.DatabaseName + "_SD456";

			var testRowFactory = new RowFactory();
			AssertNull("DatabaseName", testRowFactory.DatabaseName);
			AssertChangingDbThrowsException(testRowFactory, Db.DatabaseName);

			using (AdminConnection auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(auxConnection, testDbName);
				AdoTestUtils.DropDbIfExists(auxConnection, testDocDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(auxConnection, testDbName);
					AdoTestUtils.CreateDbIfNotExists(auxConnection, testDocDbName);

					testRowFactory = new RowFactory(testDbName);
					AssertEquals("DatabaseName", testDbName, testRowFactory.DatabaseName);
					AssertChangingDbThrowsException(testRowFactory, Db.DatabaseName);

					// Make test database read-only
					auxConnection.ExecuteNonQuery(String.Format("ALTER DATABASE [{0}] SET READ_ONLY WITH ROLLBACK IMMEDIATE;", testDbName));

					AssertEquals("DatabaseName", testDbName, testRowFactory.DatabaseName);
					((IReadonlyDatabaseSupport)testRowFactory).ChangeDatabaseNameToAWriteableOne(Db.DatabaseName);
					AssertEquals("DatabaseName", Db.DatabaseName, testRowFactory.DatabaseName);

					//Make test doc database read-only
					testRowFactory = new RowFactory(testDocDbName);
					auxConnection.AlterDbWriteableStateForDocManager(testDocDbName, false);
					AssertEquals("DatabaseName", testDocDbName, testRowFactory.DatabaseName);
					((IReadonlyDatabaseSupport)testRowFactory).ChangeDatabaseNameToAWriteableOne(Db.DatabaseName);
					AssertEquals("DatabaseName", Db.DatabaseName, testRowFactory.DatabaseName);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(auxConnection, testDbName);
					AdoTestUtils.DropDbIfExists(auxConnection, testDocDbName);
				}
			}
		}

		void AssertChangingDbThrowsException(RowFactory testRowFactory, string newDbName)
		{
			try
			{
				((IReadonlyDatabaseSupport)testRowFactory).ChangeDatabaseNameToAWriteableOne(newDbName);
				Fail("Should throw exception");
			}
			catch (InvalidOperationException ex)
			{
				AssertEquals("Caught Exception", "[RowFactory] Changing the database is only allowed if the original one is readonly.", ex.Message);
			}
		}
	}
}
