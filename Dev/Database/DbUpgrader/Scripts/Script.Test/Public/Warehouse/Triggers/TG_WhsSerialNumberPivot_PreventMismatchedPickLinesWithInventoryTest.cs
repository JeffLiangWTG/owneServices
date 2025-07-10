using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsSerialNumberPivot_PreventMismatchedPickLinesWithInventory))]
	class TG_WhsSerialNumberPivot_PreventMismatchedPickLinesWithInventoryTest : DBCreateTriggerScriptTest
	{
		#region Test_TG_WhsSerialNumberPivot_PreventMismatchedPickLinesWithInventory

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchedPickLinesWithInventory_ValidInventory()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 15m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 15m) { WZ_OriginalReservedQty = 15m }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivot = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlUpdate = WhsSerialNumberPivot.UpdateWhere(whsSerialNumberPivot.PK)
				.Set(s => s.WSV_WZ_PickingLine, pickLine.PK)
				.AsSQL();

			AssertNoExceptionThrown("Should be able to save.", () => SaveToDB(sqlUpdate));
		}

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchedPickLinesWithInventory_InvalidInventory_Update()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 1m, location.PK) { WE_ClientOrderedUnits = 1m, WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R2").AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive2, product.PK, 1m, location.PK) { WE_ClientOrderedUnits = 1m, WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivot = new WhsSerialNumberPivot(receiveLine1.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine2, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlUpdate = WhsSerialNumberPivot.UpdateWhere(whsSerialNumberPivot.PK)
				.Set(s => s.WSV_WZ_PickingLine, pickLine.PK)
				.AsSQL();

			AssertExceptionThrown("Should not be able to save.", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(sqlUpdate), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchedPickLinesWithInventory_InvalidInventory_Insert()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 1m, location.PK) { WE_ClientOrderedUnits = 1m, WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R2").AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive2, product.PK, 1m, location.PK) { WE_ClientOrderedUnits = 1m, WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine2, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlInsert = new SqlQueryBuilder();
			new WhsSerialNumberPivot(receiveLine1.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_IsReleaseCaptured = false, WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sqlInsert);

			AssertExceptionThrown("Trigger should prevent updated.", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(sqlInsert), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchedPickLinesWithInventory_RC_Valid()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 1m, location.PK) { WE_ClientOrderedUnits = 1m, WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlInsert = new SqlQueryBuilder();
			var whsSerialNumberPivot = new WhsSerialNumberPivot(pickLine.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_IsReleaseCaptured = true, WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sqlInsert);

			AssertNoExceptionThrown("Should be able to save.", () => SaveToDB(sqlInsert));
		}

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchedPickLinesWithInventory_RC_Invalid()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 1m, location.PK) { WE_ClientOrderedUnits = 1m, WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R2").AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive2, product.PK, 1m, location.PK) { WE_ClientOrderedUnits = 1m, WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 2m).AppendInsertAndReturnObject(sql);
			var pickLine1 = new WhsPickLine(receiveLine1, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlInsert = new SqlQueryBuilder();
			new WhsSerialNumberPivot(pickLine1.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_WZ_PickingLine = pickLine2.PK }.AppendInsertAndReturnObject(sqlInsert);

			var expectedExceptionMsg = "The INSERT statement conflicted with the CHECK constraint \"Constraint_WSV_ParentID_TableCode_PickingLine";
			NUnit.Framework.Assert.That(() => SaveToDB(sqlInsert), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMsg, true), "Trigger is not reach based on the Constraint_WSV_ParentID_TableCode_PickingLine.");
		}

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchedPickLinesWithInventory_ASNLine()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 1m, location.PK) { WE_ClientOrderedUnits = 1m, WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var asnLine = new WhsAsnLine(receive, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlInsert = new SqlQueryBuilder();
			new WhsSerialNumberPivot(asnLine.PK, WhsAsnLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sqlInsert);

			var expectedExceptionMsg = "The INSERT statement conflicted with the CHECK constraint \"Constraint_WSV_ParentTableCode_ASNLine";
			NUnit.Framework.Assert.That(() => SaveToDB(sqlInsert), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMsg, true), "Trigger is not reach based on the Constraint_WSV_ParentTableCode_ASNLine.");
		}

		#endregion

		#region Helper

		void SaveToDB(SqlQueryBuilder sql) => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		void SaveToDB(string sql) => TestConnection.ExecuteNonQuery(sql);

		string TriggerErrorMessage => "Attempt to assign a pickLine that does not belong to the inventory.";

		#endregion
	}
}
