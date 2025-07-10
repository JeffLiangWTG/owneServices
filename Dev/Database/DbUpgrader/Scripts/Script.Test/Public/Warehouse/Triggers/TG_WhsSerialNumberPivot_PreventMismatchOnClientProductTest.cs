using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsSerialNumberPivot_PreventMismatchOnClientProduct))]
	class TG_WhsSerialNumberPivot_PreventMismatchOnClientProductTest : DBCreateTriggerScriptTest
	{
		#region Test_TG_WhsSerialNumberPivot_PreventMismatchOnClientProduct

		public void Test_Test_TG_WhsSerialNumberPivot_PreventMismatchOnClientProduct_Match()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 15m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 15m) { WZ_OriginalReservedQty = 15m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlAddSerialNumber = new SqlQueryBuilder();
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sqlAddSerialNumber);
			var whsSerialNumberPivotRL = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sqlAddSerialNumber);
			var whsSerialNumberPivotPL = new WhsSerialNumberPivot(pickLine.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_IsReleaseCaptured = true }.AppendInsertAndReturnObject(sqlAddSerialNumber);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sqlAddSerialNumber));

			WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivotRL.PK)
			.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, receiveLine.PK)
			.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
			.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
			.ExpectEquals("WSV_WSN_SerialNumber: ", i => i.WSV_WSN_SerialNumber, whsSerialNumber.PK)
			.ExpectEquals("WSV_WSN_SerialNumber: ", i => i.WSV_WZ_PickingLine , pickLine.PK)
			.VerifyAll();

			WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivotPL.PK)
			.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, pickLine.PK)
			.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsPickLineSchema.Constants.Prefix)
			.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, true)
			.ExpectEquals("WSV_WSN_SerialNumber: ", i => i.WSV_WSN_SerialNumber, whsSerialNumber.PK)
			.VerifyAll();
		}

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchOnClientProduct_Client_DocketLineParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client1 = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client1.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlAddSerialNumber = new SqlQueryBuilder();
			var whsSerialNumber = new WhsSerialNumber(client2, product, "SN1").AppendInsertAndReturnObject(sqlAddSerialNumber);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sqlAddSerialNumber);

			AssertExceptionThrown("trigger should prevent insert.", typeof(SqlException),
				TriggerErrorMessage, () => SaveToDB(sqlAddSerialNumber), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchOnClientProduct_Client_ASNLineParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client1 = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client1.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var asnLine = new WhsAsnLine(receive, product.PK, 15m).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlAddSerialNumber = new SqlQueryBuilder();
			var whsSerialNumber = new WhsSerialNumber(client2, product, "SN1").AppendInsertAndReturnObject(sqlAddSerialNumber);
			new WhsSerialNumberPivot(asnLine.PK, WhsAsnLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sqlAddSerialNumber);

			AssertExceptionThrown("trigger should prevent insert.", typeof(SqlException),
				TriggerErrorMessage, () => SaveToDB(sqlAddSerialNumber), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchOnClientProduct_Client_PickLineLineParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client1 = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client1.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client1.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 15m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 15m) { WZ_OriginalReservedQty = 15m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlAddSerialNumber = new SqlQueryBuilder();
			var whsSerialNumber = new WhsSerialNumber(client2, product, "SN1").AppendInsertAndReturnObject(sqlAddSerialNumber);
			new WhsSerialNumberPivot(pickLine.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_IsReleaseCaptured = true, WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sqlAddSerialNumber);

			AssertExceptionThrown("trigger should prevent insert.", typeof(SqlException),
				TriggerErrorMessage, () => SaveToDB(sqlAddSerialNumber), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchOnClientProduct_Product_DocketLineParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product1.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlAddSerialNumber = new SqlQueryBuilder();
			var whsSerialNumber = new WhsSerialNumber(client, product2, "SN1").AppendInsertAndReturnObject(sqlAddSerialNumber);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sqlAddSerialNumber);

			AssertExceptionThrown("trigger should prevent insert.", typeof(SqlException),
				TriggerErrorMessage, () => SaveToDB(sqlAddSerialNumber), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchOnClientProduct_Product_ASNLineParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var asnLine = new WhsAsnLine(receive, product1.PK, 15m).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlAddSerialNumber = new SqlQueryBuilder();
			var whsSerialNumber = new WhsSerialNumber(client, product2, "SN1").AppendInsertAndReturnObject(sqlAddSerialNumber);
			new WhsSerialNumberPivot(asnLine.PK, WhsAsnLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sqlAddSerialNumber);

			AssertExceptionThrown("trigger should prevent insert.", typeof(SqlException),
				TriggerErrorMessage, () => SaveToDB(sqlAddSerialNumber), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventMismatchOnClientProduct_Product_PickLineLineParent()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product1.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product1.PK, 15m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 15m) { WZ_OriginalReservedQty = 15m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var sqlAddSerialNumber = new SqlQueryBuilder();
			var whsSerialNumber = new WhsSerialNumber(client, product2, "SN1").AppendInsertAndReturnObject(sqlAddSerialNumber);
			new WhsSerialNumberPivot(pickLine.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_IsReleaseCaptured = true, WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sqlAddSerialNumber);

			AssertExceptionThrown("trigger should prevent insert.", typeof(SqlException),
				TriggerErrorMessage, () => SaveToDB(sqlAddSerialNumber), assertStartsWith: true);
		}

		#endregion

		#region Helper

		void SaveToDB(SqlQueryBuilder sql) => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

		string TriggerErrorMessage => "Attempt to insert mismatch between Client or Product and the related Job.";

		#endregion
	}
}
