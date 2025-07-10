using System;
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
	[TestedType(typeof(TG_WhsDocket_PreventCriticalFieldChangeWhenHasLines))]
	class TG_WhsDocket_PreventCriticalFieldChangeWhenHasLinesTest : DBCreateTriggerScriptTest
	{
	}

	[UseSnapshotProtection]
	class Trigger_TG_WhsDocket_PreventCriticalFieldChangeWhenHasLinesTest : TestCase
	{
		#region TestTrigger_DocketSubType_Update

		public void TestTrigger_DocketSubType_Update()
		{
			TestTrigger_DocketSubType_WhsDocketCore("ORD", "ORD", "BAK", "", needLocation: false);
		}

		public void TestTrigger_DocketSubType_WhsReceiveUpdate()
		{
			TestTrigger_DocketSubType_WhsDocketCore("INW", "REC", "RET", "PUT");
		}

		public void TestTrigger_DocketSubType_WhsAdjustmentUpdate()
		{
			TestTrigger_DocketSubType_WhsDocketCore("ADJ", "NEA", "IWA", "AVL");
		}

		public void TestTrigger_DocketSubType_WhsWorkOrderUpdate()
		{
			TestTrigger_DocketSubType_WhsDocketCore("WOR", "ASS", "DIS", "", needLocation: false);
		}

		void TestTrigger_DocketSubType_WhsDocketCore(string docketType, string docketFromSubType, string docketToSubType, string inventoryStatus, bool needLocation = true, bool needTransferFrom = false)
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(connection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var docketA = new WhsDocket(client.PK, whs.PK, docketType, docketFromSubType, "ENT", "DocketA").AppendInsertAndReturnObject(sql);
				var docketB = new WhsDocket(client.PK, whs.PK, docketType, docketFromSubType, "", "DocketB").AppendInsertAndReturnObject(sql);

				new WhsDocketLine(docketB, part.PK, 10m, needLocation ? locationA1.PK : null)
				{
					WE_OriginalInventoryStatus = inventoryStatus,
					WE_CurrentInventoryStatus = inventoryStatus,
					WE_StockOnHand = docketType == "INW" ? 10m : 0m
				}.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update(connection, "Update must be allowed when the docket has no lines.", docketA.PK, TestTriggerResult.Success, WhsDocketSchema.Constants.WD_DocketSubType, docketToSubType);
				AssertTrigger_Update(connection, "Update must be prevented when the docket has captured lines.", docketB.PK, TestTriggerResult.Fail, WhsDocketSchema.Constants.WD_DocketSubType, docketToSubType);
				AssertTrigger_Update(connection, "Update must be allowed when the docket has captured lines but the value has no changes.", docketB.PK, TestTriggerResult.Success, WhsDocketSchema.Constants.WD_DocketSubType, docketFromSubType);
			}
		}

		#endregion

		#region TestTrigger_DocketSubType_WhsTransferUpdate

		public void TestTrigger_DocketSubType_WhsTransferUpdate()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(connection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var org1 = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive = new WhsDocket(org1.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				var transferA = new WhsDocket(org1.PK, whs.PK, "TFR", "TFR", "ENT", "TransferA").AppendInsertAndReturnObject(sql);

				var transferB = new WhsDocket(org1.PK, whs.PK, "TFR", "TFR", "ENT", "TransferB").AppendInsertAndReturnObject(sql);
				var transferBLine = new WhsDocketLine(transferB, part.PK, 10m, locationA1.PK) { WE_WL_TransferFrom = locationA1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, transferBLine, 10).AppendInsertAndReturnObject(sql);

				// defer docket line trigger so pick line is also in DB at the time trigger is run
				using (connection.BeginTransactionWithManager())
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				{
					connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					connection.CommitTransaction();
				}

				AssertTrigger_Update(connection, "Update must be allowed when the docket has no lines.", transferA.PK, TestTriggerResult.Success, WhsDocketSchema.Constants.WD_DocketSubType, "IWD");
				AssertTrigger_Update(connection, "Update must be prevented when the docket has captured lines.", transferB.PK, TestTriggerResult.Fail, WhsDocketSchema.Constants.WD_DocketSubType, "IWD");
				AssertTrigger_Update(connection, "Update must be allowed when the docket has captured lines but the value has no changes.", transferB.PK, TestTriggerResult.Success, WhsDocketSchema.Constants.WD_DocketSubType, "TFR");
			}
		}

		#endregion

		#region TestTrigger_Warehouse

		public void TestTrigger_Warehouse()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(connection);
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(connection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(connection);
				var area = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var docketA = new WhsDocket(client.PK, whs1.PK, "INW", "REC", "ENT", "DocketA").AppendInsertAndReturnObject(sql);
				var docketB = new WhsDocket(client.PK, whs1.PK, "INW", "REC", "ENT", "DocketB").AppendInsertAndReturnObject(sql);
				new WhsDocketLine(docketB, part.PK, 10m, locationA1.PK) { WE_StockOnHand = 10m, WE_OriginalInventoryStatus = "PUT", WE_CurrentInventoryStatus = "PUT" }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update(connection, "Update must be allowed when the docket has no lines.", docketA.PK, TestTriggerResult.Success, WhsDocketSchema.Constants.WD_WW_Whs, whs2.PK.ToString());
				AssertTrigger_Update(connection, "Update must be prevented when the docket has captured lines.", docketB.PK, TestTriggerResult.Fail, WhsDocketSchema.Constants.WD_WW_Whs, whs2.PK.ToString());
				AssertTrigger_Update(connection, "Update must be allowed when the docket has captured lines, but the value has not changed.", docketB.PK, TestTriggerResult.Success, WhsDocketSchema.Constants.WD_WW_Whs, whs1.PK.ToString());
			}
		}

		public void TestTrigger_Warehouse_Order()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(connection);
				var warehouse1 = new WhsWarehouse("W1", branch1.PK).WithDockDoor(connection);
				var warehouse2 = new WhsWarehouse("W2", branch2.PK).WithDockDoor(connection);
				var client = new OrgHeader("C1").InsertAndReturnObject(connection);
				var product = new OrgSupplierPart("P1").InsertAndReturnObject(connection);
				var row = new WhsRow(warehouse1, "R1").InsertAndReturnObject(connection);
				var area = new WhsArea(warehouse1.PK, "A1").InsertAndReturnObject(connection);
				var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(connection);

				var docketDoorLocationPK = warehouse1.WW_DefaultOutboundDockDoor.FK;

				var finalisedDate = DateTime.Today;
				WhsDocket order1, order2, order3, order4, order5, order6;
				using (connection.BeginTransactionWithManager())
				{
					var receive = new WhsDocket(client.PK, warehouse1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = finalisedDate }.InsertAndReturnObject(connection);
					var sql = new SqlQueryBuilder();
					var receiveLine = new WhsDocketLine(receive, product.PK, 100m, location.PK) { WE_StockOnHand = 100m }.AppendInsertAndReturnObject(sql);

					order1 = new WhsDocket(client.PK, warehouse1.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
					var orderLine1 = new WhsDocketLine(order1, product.PK, 10m).AppendInsertAndReturnObject(sql);

					order2 = new WhsDocket(client.PK, warehouse1.PK, "ORD", "ORD", "ENT", "O2") { WD_WL_CrossDock = docketDoorLocationPK }.AppendInsertAndReturnObject(sql);

					order3 = new WhsDocket(client.PK, warehouse1.PK, "ORD", "ORD", "ENT", "O3") { WD_WL_CrossDock = docketDoorLocationPK }.AppendInsertAndReturnObject(sql);

					order4 = new WhsDocket(client.PK, warehouse1.PK, "ORD", "ORD", "ENT", "O4").AppendInsertAndReturnObject(sql);
					var orderLine4 = new WhsDocketLine(order4, product.PK, 10m).AppendInsertAndReturnObject(sql);
					var reservedLineForOrderLine4 = new WhsPickLine(receiveLine, orderLine4, 10m) { WZ_OriginalReservedQty = 10m }.AppendInsertAndReturnObject(sql);

					var pick5 = new WhsPick(warehouse1, "P001", "NEW").AppendInsertAndReturnObject(sql);
					order5 = new WhsDocket(client.PK, warehouse1.PK, "ORD", "ORD", "PIC", "O5") { WD_WP = pick5.PK }.AppendInsertAndReturnObject(sql);
					var orderLine5 = new WhsDocketLine(order5, product.PK, 10m).AppendInsertAndReturnObject(sql);

					order6 = new WhsDocket(client.PK, warehouse1.PK, "ORD", "ORD", "CAN", "O6") { WD_GS_NKCanceledBy = "~BP", WD_CanceledTimeUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(sql);
					using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
					using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, connection))
					{
						connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					}
					connection.CommitTransaction();
				}

				var exceptedExceptionMessage = "Attempt to change warehouse for an order with Cross Dock Location/Reserved Stock or is processing/processed.";
				AssertTrigger_Update(connection, "Update must be allowed when the order has lines.", order1.PK, TestTriggerResult.Success, WhsDocketSchema.Constants.WD_WW_Whs, warehouse2.PK.ToString());
				AssertTrigger_Update(connection, "Update must be allowed when the cross dock location is added newly.", order1.PK, TestTriggerResult.Success, WhsDocketSchema.Constants.WD_WL_CrossDock, docketDoorLocationPK.ToString());
				AssertTrigger_Update(connection, "Update must be allowed when the updated order has no cross dock location.", order2.PK, TestTriggerResult.Success, WhsDocketSchema.Constants.WD_WW_Whs, warehouse2.PK.ToString(), WhsDocketSchema.Constants.WD_WL_CrossDock, "NULL");
				AssertTrigger_Update(connection, "Update must be prevented when the order has cross dock location.", order3.PK, TestTriggerResult.Fail, WhsDocketSchema.Constants.WD_WW_Whs, warehouse2.PK.ToString(), expectedExceptionMessage: exceptedExceptionMessage);
				AssertTrigger_Update(connection, "Update must be prevented when the order has reserved stock.", order4.PK, TestTriggerResult.Fail, WhsDocketSchema.Constants.WD_WW_Whs, warehouse2.PK.ToString(), expectedExceptionMessage: exceptedExceptionMessage);
				AssertTrigger_Update(connection, "Update must be prevented when the order has pick.", order5.PK, TestTriggerResult.Fail, WhsDocketSchema.Constants.WD_WW_Whs, warehouse2.PK.ToString(), expectedExceptionMessage: exceptedExceptionMessage);
				AssertTrigger_Update(connection, "Update must be allowed when the order is cancelled but without pick.", order6.PK, TestTriggerResult.Success, WhsDocketSchema.Constants.WD_WW_Whs, warehouse2.PK.ToString());
			}
		}

		#endregion

		#region TestTrigger_DocketSubTypeAndWarehouseAllChanged

		public void TestTrigger_DocketSubTypeAndWarehouseAllChanged_Receive()
		{
			TestTrigger_DocketSubTypeAndWarehouseAllChangedCore("Change SubType and Warehouse on Receive", "INW", "REC", "'RET'", "PUT");
		}

		public void TestTrigger_DocketSubTypeAndWarehouseAllChanged_Adjustment()
		{
			TestTrigger_DocketSubTypeAndWarehouseAllChangedCore("Change SubType and Warehouse on Adjustment", "ADJ", "NEA", "'IWA'", "AVL");
		}

		public void TestTrigger_DocketSubTypeAndWarehouseAllChanged_WorkOrder()
		{
			TestTrigger_DocketSubTypeAndWarehouseAllChangedCore("Change SubType and Warehouse on WorkOrder", "WOR", "ASS", "'DIS'", "", needLocation: false);
		}

		public void TestTrigger_DocketSubTypeAndWarehouseAllChanged_Order()
		{
			TestTrigger_DocketSubTypeAndWarehouseAllChangedCore("Change SubType and Warehouse on Order", "ORD", "ORD", "'CUS'", "", needLocation: false);
		}

		void TestTrigger_DocketSubTypeAndWarehouseAllChangedCore(string errorMessage, string docketType, string docketFromSubType, string docketToSubType, string inventoryStatus, bool needLocation = true)
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(connection);
				var warehouse1 = new WhsWarehouse("W1", branch1.PK).WithDockDoor(connection);
				var warehouse2 = new WhsWarehouse("W2", branch2.PK).WithDockDoor(connection);
				var client1 = new OrgHeader("C1").InsertAndReturnObject(connection);
				var product = new OrgSupplierPart("P1").InsertAndReturnObject(connection);
				var row = new WhsRow(warehouse1, "R1").InsertAndReturnObject(connection);
				var area = new WhsArea(warehouse1.PK, "A1").InsertAndReturnObject(connection);
				var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(connection);

				var docket = new WhsDocket(client1.PK, warehouse1.PK, docketType, docketFromSubType, "CAN", "DOCKET") { WD_GS_NKCanceledBy = "~BP", WD_CanceledTimeUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(sql);
				var docketLine = new WhsDocketLine(docket, product.PK, 10m, needLocation ? location.PK : null)
				{
					WE_DocketLineStatus = "CAN",
					WE_OriginalInventoryStatus = inventoryStatus,
					WE_CurrentInventoryStatus = inventoryStatus
				}.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update(connection, "Change SubType and Warehouse on Transfer", docket.PK, TestTriggerResult.Fail, WhsDocketSchema.Constants.WD_WW_Whs, warehouse2.PK.ToString(), WhsDocketSchema.Constants.WD_DocketSubType, docketToSubType);
			}
		}

		public void TestTrigger_DocketSubTypeAndWarehouseAllChanged_Transfer()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(connection);
				var warehouse1 = new WhsWarehouse("W1", branch1.PK).WithDockDoor(connection);
				var warehouse2 = new WhsWarehouse("W2", branch2.PK).WithDockDoor(connection);
				var client1 = new OrgHeader("C1").InsertAndReturnObject(connection);
				var product = new OrgSupplierPart("P1").InsertAndReturnObject(connection);
				var row = new WhsRow(warehouse1, "R1").InsertAndReturnObject(connection);
				var area = new WhsArea(warehouse1.PK, "A1").InsertAndReturnObject(connection);
				var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(connection);
				var today = DateTime.Today;

				var receive = new WhsDocket(client1.PK, warehouse1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, product.PK, 50m, location.PK) { WE_StockOnHand = 50m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);

				var transfer = new WhsDocket(client1.PK, warehouse1.PK, "TFR", "TFR", "ENT", "TFR").AppendInsertAndReturnObject(sql);
				var transferLine = new WhsDocketLine(transfer, product.PK, 10m, location.PK) { WE_DocketLineStatus = "ENT", WE_WL_TransferFrom = location.PK }.AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, transferLine, 10).AppendInsertAndReturnObject(sql);

				// defer docket line trigger so pick line is also in DB at the time trigger is run
				using (connection.BeginTransactionWithManager())
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				{
					connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					connection.CommitTransaction();
				}
				AssertTrigger_Update(connection, "Change SubType and Warehouse on Transfer", transfer.PK, TestTriggerResult.Fail, WhsDocketSchema.Constants.WD_WW_Whs, warehouse2.PK.ToString(), WhsDocketSchema.Constants.WD_DocketSubType, "'IWD'");
			}
		}

		#endregion

		#region TestAssertions

		void AssertTrigger_Update(DbConnection connection, string errorMessage, Guid docketPK, TestTriggerResult expectedResult, string columnToUpdate, string updateToString,
			string columnToUpdate2 = "", string updateToString2 = "",
			string expectedExceptionMessage = "Attempt to change docket subtype or warehouse for a job with captured lines.")
		{
			var columnUpdate2 = !string.IsNullOrEmpty(columnToUpdate2)
				? $@",
{columnToUpdate2} = {updateToString2}
" : string.Empty;
			var sql = $@"
			UPDATE dbo.WhsDocket
			SET
				{columnToUpdate} = '{updateToString}'{columnUpdate2},
				WD_SystemLastEditTimeUtc = SYSUTCDATETIME(),
				WD_SystemLastEditUser = '~BP'
			WHERE
				WD_PK = '{docketPK}'";

			if (expectedResult == TestTriggerResult.Success)
			{
				AssertNoExceptionThrown(errorMessage, () => ExecuteSqlInTransaction(connection, sql));
			}
			else
			{
				AssertExceptionThrown(typeof(SqlException), expectedExceptionMessage, () => ExecuteSqlInTransaction(connection, sql), true);
			}
		}

		#endregion

		#region Implementation
		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		enum TestTriggerResult
		{
			Success,
			Fail
		}

		#endregion
	}
}
