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
	[TestedType(typeof(TG_WhsPickLine_PreventTransactionsPickingFromDifferentWarehouse))]
	class TG_WhsPickLine_PreventTransactionsPickingFromDifferentWarehouseTest : DBCreateTriggerScriptTest
	{
	}

	[UseSnapshotProtection]
	class Trigger_TG_WhsPickLine_PreventTransactionsPickingFromDifferentWarehouseTest : TestCase
	{
		#region TestTrigger_DoesNotFail

		public void TestTrigger_DoesNotFail_UpdatePickLineNonRelatedFields()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive = new WhsDocket(data.ClientPK, data.Whs2.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs2PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(data.Whs1, "P001", "PIS").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(data.ClientPK, data.Whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, orderLine, 10m).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger("TG_WhsPickLine_PreventTransactionsPickingFromDifferentWarehouse", WhsPickLineSchema.Constants.TableName, mainConnection))
				{
					SaveToDB(mainConnection, sql);
					AssertDocketLinesExistInDB("Precondition: DocketLines must exist in database.", mainConnection, receiveLine.PK, orderLine.PK);
					AssertPickLinesExistInDB("Precondition: PickLines must exist in database.", mainConnection, pickLine.PK);
				}

				var updateSql = new SqlQueryBuilder(WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_GS_NKAssignedTo, "E").AsSQL());
				AssertNoExceptionThrown("Trigger should *not* fail when modifying fields different from inventoryLine and transactionLine.", () => SaveToDB(mainConnection, updateSql));
				WhsPickLine.AssertFromDB(mainConnection, pickLine.PK).ExpectEquals("AssignedTo must be modified.", pl => pl.WZ_GS_NKAssignedTo, "E").VerifyAll();
			}
		}

		public void TestTrigger_DoesNotFail_ChangePickLineTriggeringField()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive1 = new WhsDocket(data.ClientPK, data.Whs2.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine1 = new WhsDocketLine(receive1, data.ProductPK, 10m, data.LocationWhs2PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var receive2 = new WhsDocket(data.ClientPK, data.Whs1.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine2 = new WhsDocketLine(receive2, data.ProductPK, 10m, data.LocationWhs1PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(data.Whs1, "P001", "PIS").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(data.ClientPK, data.Whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine1, orderLine, 10m).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger("TG_WhsPickLine_PreventTransactionsPickingFromDifferentWarehouse", WhsPickLineSchema.Constants.TableName, mainConnection))
				{
					SaveToDB(mainConnection, sql);
					AssertDocketLinesExistInDB("Precondition: DocketLines must exist in database.", mainConnection, receiveLine1.PK, receiveLine2.PK, orderLine.PK);
					AssertPickLinesExistInDB("Precondition: PickLines must exist in database.", mainConnection, pickLine.PK);
				}

				var updateSql = new SqlQueryBuilder(WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_WE_InventoryLine, receiveLine2).AsSQL());
				AssertNoExceptionThrown("Trigger should *not* fail when changing to another correct inventoryLine.", () => SaveToDB(mainConnection, updateSql));
				WhsPickLine.AssertFromDB(mainConnection, pickLine.PK).ExpectEquals("InventoryLine in PickLine must be modified.", pl => pl.WZ_WE_InventoryLine.FK, receiveLine2.PK).VerifyAll();
			}
		}

		public void TestTrigger_DoesNotFail_DeletePickLine()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive = new WhsDocket(data.ClientPK, data.Whs2.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs2PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(data.Whs1, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var order = new WhsDocket(data.ClientPK, data.Whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, orderLine, 10m).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger("TG_WhsPickLine_PreventTransactionsPickingFromDifferentWarehouse", WhsPickLineSchema.Constants.TableName, mainConnection))
				{
					SaveToDB(mainConnection, sql);
					AssertDocketLinesExistInDB("Precondition: DocketLines must exist in database.", mainConnection, receiveLine.PK, orderLine.PK);
					AssertPickLinesExistInDB("Precondition: PickLines must exist in database.", mainConnection, pickLine.PK);
				}

				var deleteSql = new SqlQueryBuilder($"DELETE FROM dbo.WhsPickLine WHERE WZ_PK = '{pickLine.PK}'");
				AssertNoExceptionThrown("Trigger should *not* fail when trying to delete an incorrect pickline.", () => SaveToDB(mainConnection, deleteSql));
				AssertEquals("Deleted pickline should not exist in DB", 0, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsPickLine WHERE WZ_PK = '{pickLine.PK}'"));
			}
		}

		#endregion

		#region TestTrigger_PickFromWrongWarehouse

		#region TestTrigger_PickFromWrongWarehouse_Order

		public void TestTrigger_PickFromWrongWarehouse_Order_Insert()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive = new WhsDocket(data.ClientPK, data.Whs1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs1PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				// Assumed all problems with pick vs order warehouses were solved in another transform
				var pick = new WhsPick(data.Whs2, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var order = new WhsDocket(data.ClientPK, data.Whs2.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, orderLine, 10m).AppendInsertAndReturnObject(sql);

				AssertTriggerFails(mainConnection, sql);
			}
		}

		public void TestTrigger_PickFromWrongWarehouse_Order_UpdateToWrongInventory()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive_whs1 = new WhsDocket(data.ClientPK, data.Whs1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine_whs1 = new WhsDocketLine(receive_whs1, data.ProductPK, 10m, data.LocationWhs1PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var receive_whs2 = new WhsDocket(data.ClientPK, data.Whs2.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine_whs2 = new WhsDocketLine(receive_whs2, data.ProductPK, 10m, data.LocationWhs2PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(data.Whs1, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var order = new WhsDocket(data.ClientPK, data.Whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine_whs1, orderLine, 10m).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertNumberOfLinesInDB(mainConnection, expectedReceiveLines: 2, expectedTransactionDockets: 1, expectedPickLines: 1);

				AssertTriggerFails(mainConnection, new SqlQueryBuilder(WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_WE_InventoryLine, receiveLine_whs2).AsSQL()));
			}
		}

		public void TestTrigger_PickFromWrongWarehouse_Order_UpdateToWrongOrderLine()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive = new WhsDocket(data.ClientPK, data.Whs1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs1PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(data.Whs1, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var order = new WhsDocket(data.ClientPK, data.Whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, orderLine, 10m).AppendInsertAndReturnObject(sql);

				var pick_bad = new WhsPick(data.Whs2, "P002", "PIS").AppendInsertAndReturnObject(sql).PK;
				var order_bad = new WhsDocket(data.ClientPK, data.Whs2.PK, "ORD", "ORD", "PIC", "O2") { WD_WP = pick_bad }.AppendInsertAndReturnObject(sql);
				var orderLine_bad = new WhsDocketLine(order_bad, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertNumberOfLinesInDB(mainConnection, expectedReceiveLines: 1, expectedTransactionDockets: 2, expectedPickLines: 1);

				AssertTriggerFails(mainConnection, new SqlQueryBuilder(WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_WE_TransactionLine, orderLine_bad).AsSQL()));
			}
		}

		public void TestTrigger_PickFromWrongWarehouse_Order_WithCorrectJobWarehouse()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;

				// Job in Whs2 with location in Whs1
				var receive = new WhsDocket(data.ClientPK, data.Whs2.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs1PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				// Order in Whs1
				var pick = new WhsPick(data.Whs2, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var order = new WhsDocket(data.ClientPK, data.Whs2.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, orderLine, 10m).AppendInsertAndReturnObject(sql);

				// This trigger will fail, however, cases like this have to be tested since the mentioned trigger did not have a transform.
				TestWhsDataSetupHelper.SuspendTrigger("TG_WhsDocketLine_LocationIsInCorrectWarehouse", WhsDocketLineSchema.Constants.TableName, mainConnection);
				AssertTriggerFails(mainConnection, sql);
			}
		}

		public void TestTrigger_PickFromWrongWarehouse_Order_WithCorrectLocationWarehouse()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;

				// Job in Whs2 with location in Whs1
				var receive = new WhsDocket(data.ClientPK, data.Whs1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs2PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				// Order in Whs1
				var pick = new WhsPick(data.Whs2, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var order = new WhsDocket(data.ClientPK, data.Whs2.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, orderLine, 10m).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger("TG_WhsDocketLine_LocationIsInCorrectWarehouse", WhsDocketLineSchema.Constants.TableName, mainConnection))
				{
					AssertNoExceptionThrown("Trigger should *not* fail for inventory lines with correct location.", () => SaveToDB(mainConnection, sql));
					AssertDocketLinesExistInDB("Precondition: DocketLines must exist in database.", mainConnection, receiveLine.PK, orderLine.PK);
					AssertPickLinesExistInDB("Precondition: PickLines must exist in database.", mainConnection, pickLine.PK);
				}
			}
		}

		#endregion

		#region TestTrigger_PickFromWrongWarehouse_WorkOrder

		public void TestTrigger_PickFromWrongWarehouse_WorkOrder_Insert()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive = new WhsDocket(data.ClientPK, data.Whs1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs1PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(data.Whs2, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var workOrder = new WhsDocket(data.ClientPK, data.Whs2.PK, "WOR", "DIS", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var workOrderLine = new WhsDocketLine(workOrder, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, workOrderLine, 10m).AppendInsertAndReturnObject(sql);

				AssertTriggerFails(mainConnection, sql);
			}
		}

		public void TestTrigger_PickFromWrongWarehouse_DynamicWorkOrder_Insert()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive = new WhsDocket(data.ClientPK, data.Whs1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs1PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(data.Whs2, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var workOrder = new WhsDocket(data.ClientPK, data.Whs2.PK, "DWO", "ASS", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var kitOrderLine = new WhsDocketLine(workOrder, data.KitProductPK, 1m).AppendInsertAndReturnObject(sql);
				var workOrderLine = new WhsDocketLine(workOrder, data.ProductPK, 10m) { WE_WE_ParentDocketLine = kitOrderLine }.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, workOrderLine, 10m).AppendInsertAndReturnObject(sql);

				AssertTriggerFails(mainConnection, sql);
			}
		}

		public void TestTrigger_PickFromWrongWarehouse_WorkOrder_UpdateToWrongInventory()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive = new WhsDocket(data.ClientPK, data.Whs1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs1PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var receive_bad = new WhsDocket(data.ClientPK, data.Whs2.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine_bad = new WhsDocketLine(receive_bad, data.ProductPK, 10m, data.LocationWhs2PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(data.Whs1, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var workOrder = new WhsDocket(data.ClientPK, data.Whs1.PK, "WOR", "DIS", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var workOrderLine = new WhsDocketLine(workOrder, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, workOrderLine, 10m).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertNumberOfLinesInDB(mainConnection, expectedReceiveLines: 2, expectedTransactionDockets: 1, expectedPickLines: 1);

				AssertTriggerFails(mainConnection, new SqlQueryBuilder(WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_WE_InventoryLine, receiveLine_bad).AsSQL()));
			}
		}

		public void TestTrigger_PickFromWrongWarehouse_DynamicWorkOrder_UpdateToWrongInventory()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive = new WhsDocket(data.ClientPK, data.Whs1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs1PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var receive_bad = new WhsDocket(data.ClientPK, data.Whs2.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine_bad = new WhsDocketLine(receive_bad, data.ProductPK, 10m, data.LocationWhs2PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(data.Whs1, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var workOrder = new WhsDocket(data.ClientPK, data.Whs1.PK, "DWO", "ASS", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var kitOrderLine = new WhsDocketLine(workOrder, data.KitProductPK, 1m).AppendInsertAndReturnObject(sql);
				var workOrderLine = new WhsDocketLine(workOrder, data.ProductPK, 10m) { WE_WE_ParentDocketLine = kitOrderLine }.AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, workOrderLine, 10m).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertNumberOfLinesInDB(mainConnection, expectedReceiveLines: 2, expectedTransactionDockets: 1, expectedPickLines: 1);

				AssertTriggerFails(mainConnection, new SqlQueryBuilder(WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_WE_InventoryLine, receiveLine_bad).AsSQL()));
			}
		}

		public void TestTrigger_PickFromWrongWarehouse_WorkOrder_UpdateToWrongWorkOrderLine()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive = new WhsDocket(data.ClientPK, data.Whs1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs1PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(data.Whs1, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var workOrder = new WhsDocket(data.ClientPK, data.Whs1.PK, "WOR", "DIS", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var workOrderLine = new WhsDocketLine(workOrder, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, workOrderLine, 10m).AppendInsertAndReturnObject(sql);

				var pick_bad = new WhsPick(data.Whs2, "P002", "PIS").AppendInsertAndReturnObject(sql).PK;
				var workOrder_bad = new WhsDocket(data.ClientPK, data.Whs2.PK, "WOR", "DIS", "PIC", "O2") { WD_WP = pick_bad }.AppendInsertAndReturnObject(sql);
				var workOrderLine_bad = new WhsDocketLine(workOrder_bad, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertNumberOfLinesInDB(mainConnection, expectedReceiveLines: 1, expectedTransactionDockets: 2, expectedPickLines: 1);

				AssertTriggerFails(mainConnection, new SqlQueryBuilder(WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_WE_TransactionLine, workOrderLine_bad).AsSQL()));
			}
		}

		public void TestTrigger_PickFromWrongWarehouse_DynamicWorkOrder_UpdateToWrongWorkOrderLine()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive = new WhsDocket(data.ClientPK, data.Whs1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs1PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(data.Whs1, "P001", "PIS").AppendInsertAndReturnObject(sql).PK;
				var workOrder = new WhsDocket(data.ClientPK, data.Whs1.PK, "DWO", "ASS", "PIC", "O1") { WD_WP = pick }.AppendInsertAndReturnObject(sql);
				var kitOrderLine = new WhsDocketLine(workOrder, data.KitProductPK, 1m).AppendInsertAndReturnObject(sql);
				var workOrderLine = new WhsDocketLine(workOrder, data.ProductPK, 10m) { WE_WE_ParentDocketLine = kitOrderLine }.AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, workOrderLine, 10m).AppendInsertAndReturnObject(sql);

				var pick_bad = new WhsPick(data.Whs2, "P002", "PIS").AppendInsertAndReturnObject(sql).PK;
				var workOrder_bad = new WhsDocket(data.ClientPK, data.Whs2.PK, "DWO", "ASS", "PIC", "O2") { WD_WP = pick_bad }.AppendInsertAndReturnObject(sql);
				var kitOrderLine_bad = new WhsDocketLine(workOrder_bad, data.KitProductPK, 1m).AppendInsertAndReturnObject(sql);
				var workOrderLine_bad = new WhsDocketLine(workOrder_bad, data.ProductPK, 10m) { WE_WE_ParentDocketLine = kitOrderLine_bad }.AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertNumberOfLinesInDB(mainConnection, expectedReceiveLines: 1, expectedTransactionDockets: 2, expectedPickLines: 1);

				AssertTriggerFails(mainConnection, new SqlQueryBuilder(WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_WE_TransactionLine, workOrderLine_bad).AsSQL()));
			}
		}

		#endregion

		#region TestTrigger_PickFromWrongWarehouse_AdjustmentOut

		public void TestTrigger_PickFromWrongWarehouse_AdjustmentOut()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;

				var receive = new WhsDocket(data.ClientPK, data.Whs2.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs2PK) { WE_StockOnHand = 0m }.AppendInsertAndReturnObject(sql);

				var adjustmentOut = new WhsDocket(data.ClientPK, data.Whs1.PK, "ADJ", "NEA", "FIN", "O1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var adjustmentOutLine = new WhsDocketLine(adjustmentOut, data.ProductPK, -10m, data.LocationWhs1PK).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, adjustmentOutLine, 10m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);

				// defer unrelated triggers
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				{
					AssertNoExceptionThrown("Trigger should *not* fail for incorrect adjustments-out.", () => SaveToDB(mainConnection, sql));
					AssertDocketLinesExistInDB("Precondition: DocketLines must exist in database.", mainConnection, receiveLine.PK, adjustmentOutLine.PK);
					AssertPickLinesExistInDB("Precondition: PickLines must exist in database.", mainConnection, pickLine.PK);
				}
			}
		}

		#endregion

		#region TestTrigger_PickFromWrongWarehouse_InternalTransfer

		public void TestTrigger_PickFromWrongWarehouse_InternalTransfer()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var data = new BasicWarehouseDataForTesting(mainConnection);
				var sql = data.Sql;
				var receive = new WhsDocket(data.ClientPK, data.Whs2.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, data.ProductPK, 10m, data.LocationWhs2PK) { WE_StockOnHand = 0m }.AppendInsertAndReturnObject(sql);

				var transfer = new WhsDocket(data.ClientPK, data.Whs1.PK, "TFR", "TFR", "FIN", "O1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
				var transferLine = new WhsDocketLine(transfer, data.ProductPK, 10m, data.LocationWhs1PK) { WE_WL_TransferFrom = data.LocationWhs1PK, WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, transferLine, 10m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);

				// defer unrelated triggers
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				{
					AssertNoExceptionThrown("Trigger should *not* fail for incorrect internal transfers.", () => SaveToDB(mainConnection, sql));
					AssertDocketLinesExistInDB("Precondition: DocketLines must exist in database.", mainConnection, receiveLine.PK, transferLine.PK);
					AssertPickLinesExistInDB("Precondition: PickLines must exist in database.", mainConnection, pickLine.PK);
				}
			}
		}

		#endregion

		#endregion

		#region AssertionHelpers

		void AssertNumberOfLinesInDB(DbConnection connection, int expectedReceiveLines, int expectedTransactionDockets, int expectedPickLines)
		{
			CombineAssertions("Following rows in DB were expected but not found:", () =>
			{
				AssertEquals("Precondition: Incorrect Number of Receive Lines.", expectedReceiveLines, connection.ExecuteScalar("SELECT COUNT(*) from dbo.WhsDocketLine WHERE WE_DocketLineType = 'INW'"));
				AssertEquals("Precondition: Incorrect Number of Transaction Dockets.", expectedTransactionDockets, connection.ExecuteScalar("SELECT COUNT(*) from dbo.WhsDocket WHERE WD_DocketType IN ('ORD', 'WOR', 'DWO', 'TFR', 'ADJ')"));
				AssertEquals("Precondition: Incorrect Number of PickLines.", expectedPickLines, connection.ExecuteScalar("SELECT COUNT(*) from dbo.WhsPickLine"));
			});
		}

		void AssertDocketLinesExistInDB(string message, DbConnection connection, params Guid[] expectedDocketLinePKs)
		{
			AssertEquals(message, expectedDocketLinePKs.Length, connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsDocketLine WHERE WE_PK IN('{string.Join("','", expectedDocketLinePKs)}')"));
		}

		void AssertPickLinesExistInDB(string message, DbConnection connection, params Guid[] expectedPickLinePKs)
		{
			AssertEquals(message, expectedPickLinePKs.Length, connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsPickLine WHERE WZ_PK IN('{string.Join("','", expectedPickLinePKs)}')"));
		}

		void AssertTriggerFails(DbConnection connection, SqlQueryBuilder actionSql)
		{
			AssertExceptionThrown(
				"Expected trigger to fail: TG_WhsPickLine_PreventTransactionsPickingFromDifferentWarehouse",
				typeof(SqlException),
				"Attempt to pick from a Warehouse different from where the transaction was registered.",
				() => SaveToDB(connection, actionSql),
				assertStartsWith: true
			);
		}

		#endregion

		#region Implementation

		class BasicWarehouseDataForTesting
		{
			public BasicWarehouseDataForTesting(DbConnection connection)
			{
				Sql = new SqlQueryBuilder();
				Today = DateTime.Today;
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(connection);
				Whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(connection);
				var area1 = new WhsArea(Whs1.PK, "AREA1").AppendInsertAndReturnObject(Sql);
				var row1 = new WhsRow(Whs1, "A").AppendInsertAndReturnObject(Sql);
				LocationWhs1PK = new WhsLocation(row1.PK, area1.PK, area1.PK).AppendInsertAndReturnObject(Sql).PK;

				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(connection);
				Whs2 = new WhsWarehouse("WH2", "TRW", branch2.PK).WithDockDoor(connection);
				var area2 = new WhsArea(Whs2.PK, "AREA2").AppendInsertAndReturnObject(Sql);
				var row2 = new WhsRow(Whs2, "B").AppendInsertAndReturnObject(Sql);
				LocationWhs2PK = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(Sql).PK;

				ClientPK = new OrgHeader("CLIENT").AppendInsertAndReturnObject(Sql).PK;
				ProductPK = new OrgSupplierPart("P1").AppendInsertAndReturnObject(Sql).PK;
				KitProductPK = new OrgSupplierPart("KIT").AppendInsertAndReturnObject(Sql).PK;
			}

			public readonly SqlQueryBuilder Sql;
			public readonly DateTime Today;
			public readonly WhsWarehouse Whs1;
			public readonly WhsWarehouse Whs2;
			public readonly Guid LocationWhs1PK;
			public readonly Guid LocationWhs2PK;
			public readonly Guid ClientPK;
			public readonly Guid ProductPK;
			public readonly Guid KitProductPK;
		}

		void SaveToDB(DbConnection connection, SqlQueryBuilder sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				connection.CommitTransaction();
			}
		}
		#endregion
	}
}

