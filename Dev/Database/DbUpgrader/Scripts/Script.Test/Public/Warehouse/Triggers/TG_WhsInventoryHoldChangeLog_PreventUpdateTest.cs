using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsInventoryHoldChangeLog_PreventUpdate))]
	class TG_WhsInventoryHoldChangeLog_PreventUpdateTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsInventoryHoldChangeLog_PreventUpdate : TransactionedTestCase
	{
		#region TestTrigger_UpdatingHoldChangeReasonFails

		public void TestTrigger_UpdatingHoldChangeReasonFails()
		{
			TestTriggerCore((pk, connection) => WhsInventoryHoldChangeLog.UpdateWhere(pk).Set(p => p.WHL_Reason, "NewReason").Post(connection));
		}

		#endregion

		#region TestTrigger_UpdatingLogVersionFails

		public void TestTrigger_UpdatingLogVersionFails()
		{
			TestTriggerCore((pk, connection) => WhsInventoryHoldChangeLog.UpdateWhere(pk).Set(p => p.WHL_LogVersion, (short)2).Post(connection));
		}

		#endregion

		#region TestTrigger_UpdatingSystemLastEditTimeUtcFails

		public void TestTrigger_UpdatingSystemLastEditTimeUtcFails()
		{
			TestTriggerCore((pk, connection) => WhsInventoryHoldChangeLog.UpdateWhere(pk).Set(p => p.WHL_SystemLastEditTimeUtc, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Post(connection));
		}

		#endregion

		#region TestTrigger_UpdatingSystemCreateTimeUtcFails

		public void TestTrigger_UpdatingSystemCreateTimeUtcFails()
		{
			TestTriggerCore((pk, connection) => WhsInventoryHoldChangeLog.UpdateWhere(pk).Set(p => p.WHL_SystemCreateTimeUtc, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Post(connection));
		}

		#endregion

		#region TestTrigger_UpdatingSystemCreateUserFails

		public void TestTrigger_UpdatingSystemCreateUserFails()
		{
			TestTriggerCore((pk, connection) => WhsInventoryHoldChangeLog.UpdateWhere(pk).Set(p => p.WHL_SystemCreateUser, "~BB").Post(connection));
		}

		#endregion

		#region TestTriggerCore

		void TestTriggerCore(Action<Guid, CargoWise.Data.DbConnection> action)
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
				"Updating rows from WhsInventoryHoldChangeLog is not allowed.",
				() => action(inventoryHoldChangeLog.PK, TestConnection),
				assertStartsWith: true
			);
		}

		#endregion

		#region TestTrigger_UpdatingHoldChangePKFails

		public void TestTrigger_UpdatingHoldChangePKFails()
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
			var receiveLine2 = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK)
			{
				WE_StockOnHand = 50m
			}.AppendInsertAndReturnObject(sql);

			var inventoryHoldChangeLog = new WhsInventoryHoldChangeLog(receiveLine1).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert("Line must exist in DB", WhsInventoryHoldChangeLog.ExistsInDB(TestConnection, inventoryHoldChangeLog.PK));

			AssertExceptionThrown(
				"Expected trigger to prevent updating.",
				typeof(SqlException),
				"Updating rows from WhsInventoryHoldChangeLog is not allowed.",
				() => WhsInventoryHoldChangeLog.UpdateWhere(inventoryHoldChangeLog.PK).Set(p => p.WHL_WE_ParentDocketLine.FK, receiveLine2.PK).Post(TestConnection),
				assertStartsWith: true
			);
		}

		#endregion
	}
}
