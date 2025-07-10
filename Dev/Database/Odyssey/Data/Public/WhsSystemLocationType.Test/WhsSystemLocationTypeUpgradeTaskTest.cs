using System;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class WhsSystemLocationTypeUpgradeTaskTest : TransactionedTestCase
	{
		#region TestInsert

		public void TestInsert_IfLocationTypePKExists()
		{
			var locationTypePK = new Guid("409037a9-41ce-449e-8032-c33fcbe0e534");

			new WhsSystemLocationTypeUpgradeTask().Run();
			AssertEquals("Should not fail if system location Type already exists in the DB.", 1, WhsLocationType.CountInDB(TestConnection, l => l.PK == locationTypePK));
		}

		public void TestInsert_IfLocationTypePKDoesNotExist()
		{
			var locationTypePK = new Guid("409037a9-41ce-449e-8032-c33fcbe0e534");
			WhsLocationType.DeleteInDB(TestConnection, locationTypePK);

			new WhsSystemLocationTypeUpgradeTask().Run();
			AssertEquals("Should insert the system location Type if it does not exist in DB.", 1, WhsLocationType.CountInDB(TestConnection, l => l.PK == locationTypePK));
		}

		public void TestInsert_IfLocationTypeCodeExist()
		{
			WhsLocationType.DeleteInDB(TestConnection, l => l.WLT_Code == "DOC");

			var locationType = new WhsLocationType("DOC") { WLT_IsSystem = true }.InsertAndReturnObject(TestConnection);

			new WhsSystemLocationTypeUpgradeTask().Run();
			AssertEquals("Should update the system location Type code if key exists in DB.", 1,
				WhsLocationType.CountInDB(TestConnection, l => l.WLT_Code != "DOC" && l.PK == new Guid("409037a9-41ce-449e-8032-c33fcbe0e534")));
		}

		#endregion

		#region TestUpdate

		public void TestUpdate_IfLocationTypePKExist()
		{
			var locationTypePK = new Guid("409037a9-41ce-449e-8032-c33fcbe0e534");
			WhsLocationType.DeleteInDB(TestConnection, locationTypePK);

			var locationType = new WhsLocationType("xxx") { WLT_IsSystem = true }.InsertAndReturnObject(TestConnection);

			new WhsSystemLocationTypeUpgradeTask().Run();
			AssertEquals("Cannot override system location Types that their key already exists.", 1,
				WhsLocationType.CountInDB(TestConnection, l => l.PK == locationType.PK && l.WLT_Code == "xxx"));
		}

		#endregion

		#region TestDelete

		public void TestDelete_IfRedundantSystemTypeLocationTypes_ChangeToNonSystem()
		{
			var locationType = new WhsLocationType("BBB") { WLT_IsSystem = true }.InsertAndReturnObject(TestConnection);

			new WhsSystemLocationTypeUpgradeTask().Run();
			AssertEquals("Should change the redundant system location Type to Non System.", 1,
				WhsLocationType.CountInDB(TestConnection, l => l.WLT_Code == "BBB" && !l.WLT_IsSystem));
		}

		public void TestDelete_ExistingLocationTypes_AreNotDeleted()
		{
			var sql = new SqlQueryBuilder();
			new WhsLocationType("AAA") { WLT_IsSystem = true }.Insert(TestConnection);
			new WhsLocationType("BBB") { WLT_IsSystem = false }.Insert(TestConnection);

			new WhsSystemLocationTypeUpgradeTask().Run();
			AssertEquals("Should not delete existing system location types.", 1, WhsLocationType.CountInDB(TestConnection, l => l.WLT_Code == "AAA"));
			AssertEquals("Should not delete existing non system location types.", 1, WhsLocationType.CountInDB(TestConnection, l => l.WLT_Code == "BBB"));
		}

		#endregion
	}
}
