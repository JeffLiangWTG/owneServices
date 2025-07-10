using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Diagnostics.Testing
{
	sealed class DbSizeInfoTest : TestCase
	{
		public void TestDbGroup()
		{
			AssertDbGroup(dbName: Db.DatabaseName, expectedDbGroup: DbGroupEnum.Main);
			AssertDbGroup(dbName: Db.DatabaseName + "_SD001", expectedDbGroup: DbGroupEnum.eDocs);
			AssertDbGroup(dbName: Db.DatabaseName + "_SD002", expectedDbGroup: DbGroupEnum.eDocs);
			AssertDbGroup(dbName: Db.DatabaseName + "_UserRepository", expectedDbGroup: DbGroupEnum.UserRepository);
			AssertDbGroup(dbName: Db.DatabaseName + "_SomethingElse", expectedDbGroup: DbGroupEnum.Other);
		}

		void AssertDbGroup(string dbName, DbGroupEnum expectedDbGroup)
		{
			var testMainDbInfo = new DbSizeInfo(dbName, 0, 0);
			AssertEquals("DbName", dbName, testMainDbInfo.DbName);
			AssertEquals("DbGroup", expectedDbGroup, testMainDbInfo.DbGroup);
		}

		public void TestSizeProperties()
		{
			var testDbInfo = new DbSizeInfo(Db.DatabaseName, 1, 2);
			AssertEquals("UsedSizeMb", 1, testDbInfo.UsedDataSizeMb);
			AssertEquals("DiskSizeMb", 2, testDbInfo.DiskSizeMb);
		}
	}
}
