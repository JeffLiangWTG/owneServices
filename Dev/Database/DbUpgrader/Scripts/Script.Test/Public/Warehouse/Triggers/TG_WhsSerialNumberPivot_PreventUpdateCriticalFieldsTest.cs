using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsSerialNumberPivot_PreventUpdateCriticalFields))]
	class TG_WhsSerialNumberPivot_PreventUpdateCriticalFieldsTest : DBCreateTriggerScriptTest
	{
		#region Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields

		public void Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields_ValidCase()
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
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivot = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			var now = DateTime.UtcNow.ToSmallDateTimeFloor();
			var updateBuilder = WhsSerialNumberPivot.UpdateWhere(whsSerialNumberPivot.PK)
				.Set(s => s.WSV_ParentID, receiveLine.PK)
				.Set(s => s.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
				.Set(s => s.WSV_SystemLastEditTimeUtc, now)
				.Set(s => s.WSV_SystemLastEditUser, "TST");

			AssertNoExceptionThrown("Should save without issue.", () => updateBuilder.Post(TestConnection));

			WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivot.PK)
				.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, receiveLine.PK)
				.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
				.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
				.ExpectEquals("WSV_WSN_SerialNumber: ", i => i.WSV_WSN_SerialNumber, whsSerialNumber.PK)
				.ExpectEquals("WSV_SystemLastEditTimeUtc: ", i => i.WSV_SystemLastEditTimeUtc, now)
				.ExpectEquals("WSV_SystemLastEditUser: ", i => i.WSV_SystemLastEditUser, "TST")
				.VerifyAll();
		}

		public void Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields_ParentID()
		{
			Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields_Core((pk, _, pickLine) =>
				WhsSerialNumberPivot.UpdateWhere(pk).Set(p => p.WSV_ParentID, pickLine.PK).AsSQL());
		}

		public void Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields_ParentTableCode()
		{
			Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields_Core((pk, _, pickLine) =>
				WhsSerialNumberPivot.UpdateWhere(pk).Set(p => p.WSV_ParentTableCode, WhsAsnLineSchema.Constants.Prefix).AsSQL());
		}

		public void Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields_IsReleaseCaptured()
		{
			Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields_Core((pk, _, pickLine) =>
				WhsSerialNumberPivot.UpdateWhere(pk)
				.Set(p => p.WSV_IsReleaseCaptured, true) // Constraint does not let you to change without other changes
				.Set(p => p.WSV_ParentTableCode, WhsPickLineSchema.Constants.Prefix)
				.Set(p => p.WSV_ParentID, pickLine.PK)
				.Set(p => p.WSV_WZ_PickingLine, pickLine.PK)
				.AsSQL());
		}

		public void Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields_WSN_SerialNumber()
		{
			Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields_Core((pk, serialNumber, _) =>
				WhsSerialNumberPivot.UpdateWhere(pk).Set(p => p.WSV_WSN_SerialNumber, serialNumber.PK).AsSQL());
		}

		void Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields_Core(Func<Guid, WhsSerialNumber, WhsPickLine, string> getUpdateClause)
		{
			var now = DateTime.Now;
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
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 2m).AppendInsertAndReturnObject(sql);
			var pickLine1 = new WhsPickLine(receiveLine1, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(sql);

			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivot = new WhsSerialNumberPivot(receiveLine1.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivot.PK)
				.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, receiveLine1.PK)
				.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
				.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
				.ExpectEquals("WSV_WSN_SerialNumber: ", i => i.WSV_WSN_SerialNumber, whsSerialNumber1.PK)
				.VerifyAll();

			var update = getUpdateClause(whsSerialNumberPivot.PK, whsSerialNumber2, pickLine2);
			var updateSQL = new SqlQueryBuilder();
			updateSQL.AppendLine("DISABLE TRIGGER TG_WhsSerialNumberPivot_PreventMismatchedPickLinesWithInventory ON WhsSerialNumberPivot;");
			updateSQL.AppendLine(update + ";");
			updateSQL.AppendLine("ENABLE TRIGGER TG_WhsSerialNumberPivot_PreventMismatchedPickLinesWithInventory ON WhsSerialNumberPivot;");

			AssertExceptionThrown("Trigger should prevent updated.", typeof(SqlException), TriggerErrorMessage, () => SaveToDB(updateSQL), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventUpdateCriticalFields_CanChangeNonCriticalFields()
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
			var receiveLine1 = new WhsDocketLine(receive, product.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive, product.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivot1 = new WhsSerialNumberPivot(receiveLine1.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 2m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine1, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivot1.PK)
				.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, receiveLine1.PK)
				.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
				.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
				.ExpectEquals("WSV_WSN_SerialNumber: ", i => i.WSV_WSN_SerialNumber, whsSerialNumber1.PK)
				.ExpectEquals("WSV_WZ_PickingLine: ", i => i.WSV_WZ_PickingLine, null)

				.VerifyAll();

			var now = DateTime.UtcNow.ToSmallDateTimeFloor();
			var update1 = WhsSerialNumberPivot.UpdateWhere(whsSerialNumberPivot1.PK)
				.Set(s => s.WSV_WZ_PickingLine, pickLine.PK)
				.Set(s => s.WSV_SystemLastEditTimeUtc, now)
				.Set(s => s.WSV_SystemLastEditUser, "TST")
				.AsSQL();

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(update1));

			WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivot1.PK)
				.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, receiveLine1.PK)
				.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
				.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
				.ExpectEquals("WSV_WSN_SerialNumber: ", i => i.WSV_WSN_SerialNumber, whsSerialNumber1.PK)
				.ExpectEquals("WSV_SystemLastEditTimeUtc: ", i => i.WSV_SystemLastEditTimeUtc, now)
				.ExpectEquals("WSV_SystemLastEditUser: ", i => i.WSV_SystemLastEditUser, "TST")
				.ExpectEquals("WSV_WZ_PickingLine: ", i => i.WSV_WZ_PickingLine, pickLine.PK)
				.VerifyAll();

			var update2 = WhsSerialNumberPivot.UpdateWhere(whsSerialNumberPivot1.PK)
				.Set(s => s.WSV_WZ_PickingLine, null)
				.AsSQL();

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(update2));

			WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivot1.PK)
				.ExpectEquals("WSV_WZ_PickingLine: ", i => i.WSV_WZ_PickingLine, null)
				.VerifyAll();
		}

		#endregion

		#region Helper

		void SaveToDB(SqlQueryBuilder sql) => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		void SaveToDB(string sql) => TestConnection.ExecuteNonQuery(sql);

		string TriggerErrorMessage => "Attempt to update ParentID, ParentTableCode, IsReleaseCaptured or Serial Number columns which is not allowed.";

		#endregion
	}
}
