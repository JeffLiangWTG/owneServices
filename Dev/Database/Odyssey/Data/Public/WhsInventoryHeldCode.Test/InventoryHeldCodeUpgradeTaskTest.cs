using System;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class InventoryHeldCodeUpgradeTaskTest : TransactionedTestCase
	{
		#region TestContainsAllSystemDefinedHoldCode

		public void TestContainsAllSystemDefinedHoldCode()
		{
			new InventoryHeldCodeUpgradeTask().Run();
			var data = new InventoryHeldCodeDataFile().LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);

			var heldRow = WhsInventoryHeldCode.ShallowLoadFromDB(TestConnection, lt => lt.WHC_Code == "HEL" && lt.WHC_Description == "Held" && lt.WHC_IsSystem).Single();

			var damagedRow = WhsInventoryHeldCode.ShallowLoadFromDB(TestConnection, lt => lt.WHC_Code == "DAM" && lt.WHC_Description == "Damaged" && lt.WHC_IsSystem).Single();

			var lccRow = WhsInventoryHeldCode.ShallowLoadFromDB(TestConnection, lt => lt.WHC_Code == "LCC" && lt.WHC_Description == "Lost in Cycle Count" && lt.WHC_IsSystem).Single();

			var shortRow = WhsInventoryHeldCode.ShallowLoadFromDB(TestConnection, lt => lt.WHC_Code == "SHORT" && lt.WHC_Description == "Short Picked" && lt.WHC_IsSystem).Single();
		}

		#endregion

		#region TestMixOfInsertUpdateDelete

		public void TestMixOfInsertUpdateDelete()
		{
			// deleting existing system InventoryGrade component
			WhsInventoryHeldCode.DeleteInDB(TestConnection, lt => lt.PK == new Guid("87e90cb0-2e34-4a76-ad6e-e2b312bc9093"));

			// change existing system InventoryGrade  Description field
			WhsInventoryHeldCode
				.UpdateWhere(lt => lt.PK == new Guid("DBE59DAC-B855-4619-B8B9-D16CE7869379"))
				.Set(lt => lt.WHC_Description, "Changed Description")
				.Set(lt => lt.WHC_SystemLastEditTimeUtc, DateTime.UtcNow)
				.Set(lt => lt.WHC_SystemLastEditUser, "~BP")
				.Post(TestConnection);

			// Create new system InventoryGrade
			var newHeldCode = new WhsInventoryHeldCode("AAA", "Test Desc", true).InsertAndReturnObject(TestConnection);

			var tempFile = new InventoryHeldCodeDataFile();
			var data = tempFile.LoadDataFromDatabase();
			AssertEquals("Table Count", 1, data.Tables.Count);

			var deletedRows = WhsInventoryHeldCode.ShallowLoadFromDB(TestConnection, lt => lt.PK == new Guid("87e90cb0-2e34-4a76-ad6e-e2b312bc9093"));
			AssertEquals("Deleted row", 0, deletedRows.Length);

			var updatedRows = WhsInventoryHeldCode.ShallowLoadFromDB(TestConnection, lt => lt.PK == new Guid("DBE59DAC-B855-4619-B8B9-D16CE7869379"));
			AssertEquals("Updated row", "Changed Description", updatedRows[0].WHC_Description);

			var insertedRows = WhsInventoryHeldCode.ShallowLoadFromDB(TestConnection, lt => lt.PK == newHeldCode.PK);
			AssertEquals("Inserted row", 1, insertedRows.Length);

			new InventoryHeldCodeUpgradeTask().Run();
			data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);

			deletedRows =  WhsInventoryHeldCode.ShallowLoadFromDB(TestConnection, lt => lt.PK == new Guid("87e90cb0-2e34-4a76-ad6e-e2b312bc9093"));
			AssertEquals("Deleted row should have been re-inserted", 1, deletedRows.Length);

			updatedRows = WhsInventoryHeldCode.ShallowLoadFromDB(TestConnection, lt => lt.PK == new Guid("DBE59DAC-B855-4619-B8B9-D16CE7869379"));
			AssertEquals("Property should have been changed back", "Damaged", updatedRows[0].WHC_Description);

			insertedRows = WhsInventoryHeldCode.ShallowLoadFromDB(TestConnection, lt => lt.PK == newHeldCode.PK);
			AssertEquals("Unmatching Rows should have been deleted", 0, insertedRows.Length);
		}

		#endregion
	}
}
