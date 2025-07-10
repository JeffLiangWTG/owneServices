using System;
using System.Linq.Expressions;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalized))]
	[UseSnapshotProtection]
	class TG_WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalizedTest : DBCreateTriggerScriptTest
	{
		#region TestTrigger_Update

		public void TestTrigger_Update_WZ_WE_TransactionLine()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var today = DateTime.Now;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 40m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "P1", "PIC").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order, part.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order, part.PK, 20m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine1, 10m) { WZ_GS_NKAssignedTo = "R", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);
			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to TransactionLine should be allowed.", pickLine.PK,  (l) => l.WZ_WE_TransactionLine, orderLine2, TriggerExpectedBehavior.AllowUpdate);
		}

		public void TestTrigger_Update_WZ_Units()
		{
			var sql = new SqlQueryBuilder();
			var pickLine = CreateWhsPickLineWithTestData(sql, 50m, 10m, 10m, "R");

			SaveDataToDB(sql);
			// suspend other trigger to check tested trigger will fail
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			{
				AssertTriggerUpdate("Changes to Quantity should be allowed.", pickLine.PK,  (l) => l.WZ_Units, 8m, TriggerExpectedBehavior.AllowUpdate);
			}
		}

		public void TestTrigger_Update_WZ_PickedDateTime()
		{
			var sql = new SqlQueryBuilder();
			var pickLine = CreateWhsPickLineWithTestData(sql, 50m, 10m, 10m, "R");

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to Picked Date should be prevented.", pickLine.PK,  (l) => l.WZ_PickedDateTime, DateTime.Now.AddDays(-1), TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_Update_GS_NKAssignedTo()
		{
			var sql = new SqlQueryBuilder();
			var pickLine = CreateWhsPickLineWithTestData(sql, 50m, 10m, 10m, "R");

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to Picker should be prevented.", pickLine.PK,  (l) => l.WZ_GS_NKAssignedTo, "F", TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_Update_VerifiedEmpty()
		{
			var sql = new SqlQueryBuilder();
			var pickLine = CreateWhsPickLineWithTestData(sql, 50m, 10m, 10m, "R");

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to VerifiedEmpty field should be prevented.", pickLine.PK,  (l) => l.WZ_VerifiedEmpty, "Y", TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_Update_InventoryLine()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var today = DateTime.Now;
			var receiveLine1 = CreateInventoryLine(sql, client, whs, product, locationA1, "R1", 60m, 50m);
			var receiveLine2 = CreateInventoryLine(sql, client, whs, product, locationA1, "R2", 100m, 100m);
			var pickLine = CreateOrderWithPickLine(sql, client, whs, product, receiveLine1, "ORD1", 10m, 10m, today, "R");

			SaveDataToDB(sql);
			// Suspend trigger to save bad data into DB for another trigger to fail. 
			// Cannot temporary suspend the triggers because transaction is killed by another failed trigger.
			TestWhsDataSetupHelper.SuspendTrigger_ForPreUpgradeTransformations(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName);
			AssertTriggerUpdate("Changes to InventoryLine should be prevented.", pickLine.PK,  (l) => l.WZ_WE_InventoryLine, receiveLine2, TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_Update_OriginalReservedQuantity()
		{
			var sql = new SqlQueryBuilder();
			var pickLine = CreateWhsPickLineWithTestData(sql, 50m, 10m, 10m, "R");

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to Original Reserved Quantity should be prevented.", pickLine.PK,  (l) => l.WZ_OriginalReservedQty, 10m, TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_Update_F3_NKAllocatedPackType()
		{
			var sql = new SqlQueryBuilder();
			var pickLine = CreateWhsPickLineWithTestData(sql, 50m, 10m, 10m, "R");

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to AllocatedPackType should be prevented.", pickLine.PK,  (l) => l.WZ_F3_NKAllocatedPackType, "PLT", TriggerExpectedBehavior.ThrowException);
		}

		public void TestTrigger_Update_IsPicking()
		{
			var sql = new SqlQueryBuilder();
			var tableName = WhsPickLineSchema.Constants.TableName;
			var pickLine = CreateWhsPickLineWithTestData(sql, 50m, 10m, 10m, "R");

			// Disable constraint since it would prevent us from updating "WZ_IsPicking" for trigger testing.
			sql.Append($"IF OBJECT_ID('Constraint_WZ_IsPicking', 'C') IS NOT NULL ALTER TABLE {tableName} NOCHECK CONSTRAINT[Constraint_WZ_IsPicking]");

			SaveDataToDB(sql);
			AssertTriggerUpdate("Changes to IsPicking should be prevented.", pickLine.PK,  (l) => l.WZ_IsPicking, true, TriggerExpectedBehavior.ThrowException);
		}

		#endregion

		#region TestAssertions

		void AssertTriggerUpdate<T>(string errorMessage, Guid pickLinePK, Expression<Func<WhsPickLine, T>> property, T value, TriggerExpectedBehavior expectedBehavior)
		{
			var updateSQL = WhsPickLine
				.UpdateWhere(pickLinePK)
				.Set(property, value).AsSQL();

			if (expectedBehavior == TriggerExpectedBehavior.AllowUpdate)
			{
				AssertNoExceptionThrown(errorMessage, () => Db.Connection.ExecuteNonQuery(updateSQL));
			}
			else
			{
				AssertExceptionThrown(typeof(SqlException), "Attempt to change critical data for a finalized Pick Line.", () => Db.Connection.ExecuteNonQuery(updateSQL), true);
			}
		}

		#endregion

		#region TestDataSetupMethods

		WhsPickLine CreateWhsPickLineWithTestData(SqlQueryBuilder sql, decimal inStockUnits, decimal orderUnits, decimal pickedUnits, string picker = "", DateTime? pickedDate = null)
		{
			var pickedDateForTest = pickedDate ?? DateTime.Now;
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var receiveLine = CreateInventoryLine(sql, client, whs, product, locationA1, "R1", inStockUnits + pickedUnits, inStockUnits);
			var pickLine = CreateOrderWithPickLine(sql, client, whs, product, receiveLine, "ORD1", orderUnits, pickedUnits, pickedDateForTest, picker);

			return pickLine;
		}

		WhsDocketLine CreateInventoryLine(SqlQueryBuilder sql, OrgHeader client, WhsWarehouse whs, OrgSupplierPart product, WhsLocation location, string docketID, decimal units, decimal stockOnHand)
		{
			var today = DateTime.Now;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", docketID) { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, units, location.PK) { WE_StockOnHand = stockOnHand }.AppendInsertAndReturnObject(sql);

			return receiveLine;
		}

		WhsPickLine CreateOrderWithPickLine(SqlQueryBuilder sql, OrgHeader client, WhsWarehouse whs, OrgSupplierPart product, WhsDocketLine inventoryLine, string docketID, decimal orderUnits, decimal pickedUnits, DateTime? pickedDate = null, string pickedBy = "")
		{
			var pick = new WhsPick(whs, string.Format("P1_{0}", docketID), "PIC").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", docketID) { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, orderUnits).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(inventoryLine, orderLine, pickedUnits)
			{
				WZ_GS_NKAssignedTo = string.IsNullOrEmpty(pickedBy) ? "" : pickedBy,
				WZ_PickedDateTime = pickedDate
			}.AppendInsertAndReturnObject(sql);

			return pickLine;
		}

		#endregion

		#region Implementation

		static void SaveDataToDB(SqlQueryBuilder sql)
		{
			// defer triggers to run check in the end
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		enum TriggerExpectedBehavior
		{
			ThrowException,
			AllowUpdate
		}
		#endregion
	}
}

