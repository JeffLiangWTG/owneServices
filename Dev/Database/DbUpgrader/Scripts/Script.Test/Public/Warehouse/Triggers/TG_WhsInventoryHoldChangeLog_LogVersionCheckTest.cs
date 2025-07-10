using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsInventoryHoldChangeLog_LogVersionCheck))]
	class TG_WhsInventoryHoldChangeLog_LogVersionCheckTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsInventoryHoldChangeLog_LogVersionCheckTest : TransactionedTestCase
	{
		#region TestTrigger_MultipleLogChangesInSingleTransactionFails

		public void TestTrigger_MultipleLogChangesInSingleTransactionFails()
		{
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.InsertAndReturnObject(TestConnection);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.InsertAndReturnObject(TestConnection);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTime.Now }.InsertAndReturnObject(TestConnection);
			var receiveLine1 = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK)
			{
				WE_StockOnHand = 50m
			}.InsertAndReturnObject(TestConnection);

			var logs = new[]
			{
				new WhsInventoryHoldChangeLog(receiveLine1)
				{
					WHL_LogVersion = 1
				},
				new WhsInventoryHoldChangeLog(receiveLine1)
				{
					WHL_LogVersion = 2
				}
			};

			AssertExceptionThrown(
				"Only one WHL_LogVersion per ParentDocketLine can be inserted in a single transaction.",
				typeof(SqlException),
				"Only one WHL_LogVersion per ParentDocketLine can be inserted in a single transaction.",
				() => TestConnection.ExecuteNonQuery(WhsInventoryHoldChangeLog.GetBulkInsertStatement(logs)),
				assertStartsWith: true
			);
		}

		#endregion

		#region TestTrigger_UniqueInventoryHoldChangeLogEqualsOne

		public void TestTrigger_UniqueInventoryHoldChangeLogEqualsOne()
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

			var inventoryHoldChangeLog = new WhsInventoryHoldChangeLog(receiveLine1)
			{
				WHL_LogVersion = 2
			}.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
				"The WHL_LogVersion must be sequential and must be 1 for each new and unique WhsInventoryHoldChangeLog.",
				typeof(SqlException),
				"The WHL_LogVersion must be sequential and must be 1 for each new and unique WhsInventoryHoldChangeLog.",
				() => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()),
				assertStartsWith: true
			);
		}

		#endregion

		#region TestTrigger_SequentialLogVersionCheck

		public void TestTrigger_SequentialLogVersionCheck()
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

			var inventoryHoldChangeLog2 = new WhsInventoryHoldChangeLog(receiveLine1)
			{
				WHL_LogVersion = 5
			}.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
				"The WHL_LogVersion must be sequential and must be 1 for each new and unique WhsInventoryHoldChangeLog.",
				typeof(SqlException),
				"The WHL_LogVersion must be sequential and must be 1 for each new and unique WhsInventoryHoldChangeLog.",
				() => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()),
				assertStartsWith: true
			);
		}

		#endregion

		#region TestTrigger_SequentialLogVersionCheckMultipleDocketLines

		public void TestTrigger_SequentialLogVersionCheckMultipleDocketlines()
		{
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);

			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.InsertAndReturnObject(TestConnection);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.InsertAndReturnObject(TestConnection);

			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTime.Now }.InsertAndReturnObject(TestConnection);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 50m, locationA1.PK)
			{
				WE_StockOnHand = 50m
			}.InsertAndReturnObject(TestConnection);

			var inventoryHoldChangeLog1 = new WhsInventoryHoldChangeLog(receiveLine1).InsertAndReturnObject(TestConnection);

			var inventoryHoldChangeLog2 = new WhsInventoryHoldChangeLog(receiveLine1)
			{
				WHL_LogVersion = 2
			}.InsertAndReturnObject(TestConnection);

			var inventoryHoldChangeLog3 = new WhsInventoryHoldChangeLog(receiveLine1)
			{
				WHL_LogVersion = 3
			}.InsertAndReturnObject(TestConnection);

			var sql = new SqlQueryBuilder();
			var branch2 = new GlbBranch("BR2").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("CLIENT2").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);

			var whs2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(sql);
			var area2 = new WhsArea(whs2.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA12 = new WhsLocation(row2.PK, area2.PK, area2.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var receive2 = new WhsDocket(client2.PK, whs2.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = DateTime.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive2, product2.PK, 50m, locationA12.PK)
			{
				WE_StockOnHand = 50m
			}.AppendInsertAndReturnObject(sql);

			var inventoryHoldChangeLog11 = new WhsInventoryHoldChangeLog(receiveLine2).AppendInsertAndReturnObject(sql);

			var inventoryHoldChangeLog22 = new WhsInventoryHoldChangeLog(receiveLine2)
			{
				WHL_LogVersion = 2
			}.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(
				"The WHL_LogVersion must be sequential.",
				() => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends())
			);
		}

		#endregion

	}
}
