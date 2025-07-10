using System;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class BarcodeRuleUpgradeTaskTest : TransactionedTestCase
	{
		#region TestMixOfInsertUpdateDelete

		public void TestMixOfInsertUpdateDelete()
		{
			// deleting existing system rule component
			BarcodeRuleComponent.DeleteInDB(TestConnection, brc => brc.PK == new Guid("6219a27f-ad50-4be7-91d3-9d84d4546a80"));

			// change existing system rule field
			BarcodeRule
				.UpdateWhere(bru => bru.PK == new Guid("d45590d2-2b4e-49e1-b44e-0913ae413f42"))
				.Set(bru => bru.BRU_Name, "Changed Property")
				.Set(bru => bru.BRU_SystemLastEditTimeUtc, DateTime.UtcNow)
				.Set(bru => bru.BRU_SystemLastEditUser, "~BP")
				.Post(TestConnection);

			// Create new rule pointing to system ruleset
			var barcodeRuleSet = BarcodeRuleSet.ShallowLoadFromDB(TestConnection, brs => brs.PK == new Guid("65279bec-7880-47fd-b889-d91c5f2890db")).Single();
			var newBarcodeRule = new BarcodeRule(barcodeRuleSet, "Test rule", 6).InsertAndReturnObject(TestConnection);

			var tempFile = new BarcodeRuleDataFile();
			var data = tempFile.LoadDataFromDatabase();
			AssertEquals("Table Count", 3, data.Tables.Count);

			var deletedRows = BarcodeRuleComponent.ShallowLoadFromDB(TestConnection, brc => brc.PK == new Guid("6219a27f-ad50-4be7-91d3-9d84d4546a80"));
			AssertEquals("Deleted row", 0, deletedRows.Length);

			var updatedRows = BarcodeRule.ShallowLoadFromDB(TestConnection, bru => bru.PK == new Guid("d45590d2-2b4e-49e1-b44e-0913ae413f42"));
			AssertEquals("Updated row", "Changed Property", updatedRows[0].BRU_Name);

			var insertedRows = BarcodeRule.ShallowLoadFromDB(TestConnection, bru => bru.PK == newBarcodeRule.PK);
			AssertEquals("Inserted row", 1, insertedRows.Length);

			new BarcodeRuleUpgradeTask().Run();
			data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 3, data.Tables.Count);

			deletedRows = BarcodeRuleComponent.ShallowLoadFromDB(TestConnection, brc => brc.PK == new Guid("6219a27f-ad50-4be7-91d3-9d84d4546a80"));
			AssertEquals("Deleted row should have been re-inserted", 1, deletedRows.Length);

			updatedRows = BarcodeRule.ShallowLoadFromDB(TestConnection, bru => bru.PK == new Guid("d45590d2-2b4e-49e1-b44e-0913ae413f42"));
			AssertEquals("Property should have been changed back", "Count of Trade Items Contained in a Logistic Unit", updatedRows[0].BRU_Name);

			insertedRows = BarcodeRule.ShallowLoadFromDB(TestConnection, bru => bru.PK == newBarcodeRule.PK);
			AssertEquals("Unmatching Rows should have been deleted", 0, insertedRows.Length);
		}

		#endregion
	}
}
