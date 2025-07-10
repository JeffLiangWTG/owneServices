using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsInventoryHoldChangeLog_PreventDelete))]
	class TG_WhsInventoryHoldChangeLog_PreventDeleteTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsInventoryHoldChangeLog_PreventDelete : TransactionedTestCase
	{
		public void TestTrigger_DeletingRowFails()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTime.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK)
			{
				WE_StockOnHand = 50m
			}.AppendInsertAndReturnObject(sql);

			var inventoryHoldChangeLog = new WhsInventoryHoldChangeLog(receiveLine1).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert("Line must exist in DB", WhsInventoryHoldChangeLog.ExistsInDB(TestConnection, inventoryHoldChangeLog.PK));

			AssertExceptionThrown(
				"Expected trigger to prevent updating.",
				typeof(SqlException),
				"Deleting rows from WhsInventoryHoldChangeLog is not allowed for non-virtual Warehouse.",
				() => WhsInventoryHoldChangeLog.DeleteInDB(TestConnection, inventoryHoldChangeLog.PK),
				assertStartsWith: true
			);
		}

		public void TestTrigger_DeletingRowFailsWhenDocketLineIsNotFinalised()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var whs = new WhsWarehouse("WH1", "PRW", branch.PK) { WW_IsVirtualWarehouse = true }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "PUT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK)
			{
				WE_OriginalInventoryStatus = "PUT",
				WE_CurrentInventoryStatus = "PUT",
				WE_StockOnHand = 50m
			}.AppendInsertAndReturnObject(sql);

			var inventoryHoldChangeLog = new WhsInventoryHoldChangeLog(receiveLine1).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert("Line must exist in DB", WhsInventoryHoldChangeLog.ExistsInDB(TestConnection, inventoryHoldChangeLog.PK));

			AssertExceptionThrown(
				"Expected trigger to prevent updating.",
				typeof(SqlException),
				"Deleting rows from WhsInventoryHoldChangeLog is not allowed for non-virtual Warehouse.",
				() => WhsInventoryHoldChangeLog.DeleteInDB(TestConnection, inventoryHoldChangeLog.PK),
				assertStartsWith: true
			);
		}

		public void TestTrigger_DeletingRowSucceedsIfWarehouseIsVirtualAndInventoryLineFinalised()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var whs = new WhsWarehouse("WH1", "PRW", branch.PK) { WW_IsVirtualWarehouse = true }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTime.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK)
			{
				WE_StockOnHand = 50m
			}.AppendInsertAndReturnObject(sql);

			var inventoryHoldChangeLog = new WhsInventoryHoldChangeLog(receiveLine1).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert("Line must exist in DB", WhsInventoryHoldChangeLog.ExistsInDB(TestConnection, inventoryHoldChangeLog.PK));
			AssertNoExceptionThrown(() => WhsInventoryHoldChangeLog.DeleteInDB(TestConnection, inventoryHoldChangeLog.PK));
		}
	}
}
