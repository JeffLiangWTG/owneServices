using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_CheckProductAndClientAreMatchWithCountEmptyLocationCategory))]
	class TG_CheckProductAndClientAreMatchWithCountEmptyLocationCategoryTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_Update_NoException()
		{
			var client = new OrgHeader("CL1").InsertAndReturnObject(TestConnection);
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(whs, "A").InsertAndReturnObject(TestConnection);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var stockTake = new WhsStocktake("123", "OMT").InsertAndReturnObject(TestConnection);
			var stockTakeLine = new WhsStocktakeLine(stockTake.PK, client.PK, product.PK, locationA1.PK).InsertAndReturnObject(TestConnection);

			AssertNoExceptionThrown("Client and Product can be null when countEmptyLocationsCategory != EMT.",
				() => TestConnection.ExecuteNonQuery(WhsStocktakeLine.UpdateWhere(stockTakeLine.PK).Set(l => l.WU_OP, null).Set(l => l.WU_OH_Client, null).AsSQL()));
		}

		public void TestTrigger_Update_ExceptionThrown()
		{
			var client = new OrgHeader("CL1").InsertAndReturnObject(TestConnection);
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(whs, "A").InsertAndReturnObject(TestConnection);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var stockTake = new WhsStocktake("123", "EMT").InsertAndReturnObject(TestConnection);
			var stockTakeLine = new WhsStocktakeLine(stockTake.PK, client.PK, product.PK, locationA1.PK).InsertAndReturnObject(TestConnection);

			AssertExceptionThrown(
				typeof(SqlException),
				"Client or Product cannot be null when exclude empty locations.\r\nThe transaction ended in the trigger. The batch has been aborted.",
				() => TestConnection.ExecuteNonQuery(WhsStocktakeLine.UpdateWhere(stockTakeLine.PK).Set(l => l.WU_OP, null).Set(l => l.WU_OH_Client, null).AsSQL()));
		}
	}
}

