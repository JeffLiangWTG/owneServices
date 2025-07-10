using System;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.Data.Utils.Testing
{
	sealed class DocManagerUtilsTest : TestCase
	{
		public void TestIsDocManagerDatabase()
		{
			Assert(DocManagerUtils.IsDocManagerDatabase(Db.DatabaseName + "_SD001"));
			Assert(DocManagerUtils.IsDocManagerDatabase(Db.DatabaseName + "_SD456"));
			Assert(DocManagerUtils.IsDocManagerDatabase(Db.DatabaseName + "_sd456"));
			Assert(DocManagerUtils.IsDocManagerDatabase(Db.DatabaseName.ToLowerInvariant() + "_SD456"));
			Assert(DocManagerUtils.IsDocManagerDatabase(Db.DatabaseName.ToLowerInvariant() + "_sd456"));
			Assert(!DocManagerUtils.IsDocManagerDatabase(Db.DatabaseName + "_SDD456"));
			Assert(!DocManagerUtils.IsDocManagerDatabase(Db.DatabaseName + "_SD4566"));
			Assert(!DocManagerUtils.IsDocManagerDatabase(Db.DatabaseName + "ABCD"));
		}

		[UseSnapshotProtection]
		public void TestAlterDbWriteableStateForDocManager()
		{
			var dbNameNotDocManager = Db.DatabaseName + "ABC";
			var dbNameDocManager = Db.DatabaseName + "_SD123";
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(adminConnection, dbNameNotDocManager);
				AdoTestUtils.CreateDbIfNotExists(adminConnection, dbNameDocManager);

				AssertExceptionThrown<ArgumentException>("dbName should be included in exception message",
					$"dbName should be a DocManager database, something like {Db.DatabaseName}_SDXXX, but it is {dbNameNotDocManager}.",
					() => adminConnection.AlterDbWriteableStateForDocManager(dbNameNotDocManager, false));

				try
				{
					adminConnection.AlterDbWriteableStateForDocManager(dbNameDocManager, true);
					Assert(DocManagerUtils.IsDbWriteableForDocManager(dbNameDocManager));

					adminConnection.AlterDbWriteableStateForDocManager(dbNameDocManager, false);
					Assert(!DocManagerUtils.IsDbWriteableForDocManager(dbNameDocManager));
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, dbNameNotDocManager);
					AdoTestUtils.DropDbIfExists(adminConnection, dbNameDocManager);
				}
			}
		}
	}
}
