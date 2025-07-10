using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStock))]
	class TG_WhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockTest : DBCreateTriggerScriptTest
	{
		#region TestTrigger_IgnoresSaving

		[UseSnapshotProtection]
		public void TestTrigger_IgnoresSaving()
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
			var pick = new WhsPick(whs, "Pick", "NEW").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 15m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 15m) { WZ_OriginalReservedQty = 15m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends()));

			var decreaseStockSQL = WhsDocketLine
				.UpdateWhere(receiveLine.PK)
				.Set(l => l.WE_StockOnHand, 5m)
				.Set(l => l.WE_TransactionQuantity, 5m).AsSQL();
			AssertNoExceptionThrown("Can reserve 15 units for order1 when 15 units are expected without trigger exceptions.", () => SaveToDB(decreaseStockSQL));
			WhsDocketLine.AssertFromDB(Db.Connection, receiveLine.PK)
				.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 15)
				.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 5)
				.VerifyAll();
		}

		#endregion

		#region TestTrigger_IgnoresPickLinesThatArentCrossDocks

		[UseSnapshotProtection]
		public void TestTrigger_ConsidersPickLinesThatArentCrossDocks()
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 6m, location.PK) { WE_ClientOrderedUnits = 10m, WE_StockOnHand = 6m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1") { WD_IsPutawayTransfer = true }.AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 6m, location.PK)
			{
				WE_WL_TransferFrom = location.PK,
				WE_AdjustmentArrivalDate = DateTime.Today,
				WE_DocketLineStatus = "ENT",
			}.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine, 6m).AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "Pick", "NEW").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 4m).AppendInsertAndReturnObject(sql);
			var reservePickLine = new WhsPickLine(receiveLine, orderLine, 4m) { WZ_OriginalReservedQty = 4m }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				AssertNoExceptionThrown("Can allocate 10 units (4 reserved units) from inventory without trigger exceptions.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends()));
			}
			WhsDocketLine.AssertFromDB(Db.Connection, receiveLine.PK)
				.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 10)
				.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 6)
				.VerifyAll();

			var finaliseSQL = CreateFinaliseDocketLineSQL(receive, receiveLine);

			AssertExceptionThrown("Cannot finalise a docket with shortage of stock for picks.", typeof(SqlException), () => SaveToDB(finaliseSQL));

			Db.Connection.CommitTransaction();
		}

		#endregion

		#region TestTrigger_PreventFinalisationWithShortageOfCrossDockStock

		#region TestTrigger_PreventFinalisationWithShortageOfCrossDockStock_SingleOrder

		[UseSnapshotProtection]
		public void TestTrigger_PreventFinalisationWithShortageOfCrossDockStock_SingleOrder()
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

			AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends()));

			var decreaseStockSQL = WhsDocketLine
				.UpdateWhere(receiveLine.PK)
				.Set(l => l.WE_StockOnHand, 5m)
				.Set(l => l.WE_TransactionQuantity, 5m).AsSQL();
			AssertNoExceptionThrown("Can commit 15 units for order1 when 15 units are expected without trigger exceptions.", () => SaveToDB(decreaseStockSQL));
			WhsDocketLine.AssertFromDB(Db.Connection, receiveLine.PK)
				.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 15)
				.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 5)
				.ExpectEquals("Stock on Hand Quantity", r => r.WE_StockOnHand, 5)
				.VerifyAll();

			var finaliseSQL = CreateFinaliseDocketLineSQL(receive, receiveLine);

			AssertExceptionThrown("Cannot finalise receive with shortage of stock for picks.", typeof(SqlException), () => SaveToDB(finaliseSQL));
			Db.Connection.CommitTransaction();
		}

		#endregion

		#region TestTrigger_PreventFinalisationWithShortageOfCrossDockStock_IgnoresDocketLineStatusUpdateOtherThanFinalise

		[UseSnapshotProtection]
		public void TestTrigger_PreventFinalisationWithShortageOfCrossDockStock_IgnoresDocketLineStatusUpdateOtherThanFinalise()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 15m, locationA1.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A", WE_DocketLineStatus = "" }.AppendInsertAndReturnObject(sql);

			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 15m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 15m) { WZ_OriginalReservedQty = 15m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends()));

			using (TestWhsDataSetupHelper.SuspendTrigger("TG_PreventOverReduceOfStockViaInventoryLine", WhsDocketLineSchema.Constants.TableName))
			// defer triggers to save data into DB
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				var decreaseStockSQL = new SqlQueryBuilder();
				decreaseStockSQL.Append(WhsDocketLine
					.UpdateWhere(receiveLine.PK)
					.Set(l => l.WE_StockOnHand, 0m)
					.Set(l => l.WE_DocketLineStatus, "PFU").AsSQL());

				var today = DateTime.Today;
				var putawayTransfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "HFT", "T1") { WD_IsPutawayTransfer = true }.AppendInsertAndReturnObject(decreaseStockSQL);
				var putawayTransferLine = new WhsDocketLine(putawayTransfer, product.PK, 15m, locationA2.PK)
				{
					WE_StockOnHand = 15m,
					WE_DocketLineStatus = "HFT",
					WE_WL_TransferFrom = locationA1.PK,
					WE_OriginalInventoryStatus = "PTA",
					WE_CurrentInventoryStatus = "PTA",
					WE_AdjustmentArrivalDate = today,
				}.AppendInsertAndReturnObject(decreaseStockSQL);
				new WhsPickLine(receiveLine, putawayTransferLine, 15m) { WZ_PickedDateTime = today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(decreaseStockSQL);

				AssertNoExceptionThrown("Status can be updated and not set off the trigger as long as status is not 'FIN'.", () => SaveToDB(decreaseStockSQL.ToStringWithNewLineBetweenAppends()));
				WhsDocketLine.AssertFromDB(Db.Connection, receiveLine.PK)
					.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 15)
					.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 15)
					.ExpectEquals("Stock on Hand Quantity", r => r.WE_StockOnHand, 0)
					.VerifyAll();
			}

			var finaliseSQL = CreateFinaliseDocketLineSQL(receive, receiveLine);
			AssertExceptionThrown("Exception thrown when finalised.", typeof(SqlException), () => SaveToDB(finaliseSQL));
			Db.Connection.CommitTransaction();
		}

		#endregion

		#region TestTrigger_PreventFinalisationWithShortageOfCrossDockStock_MultipleOrders

		[UseSnapshotProtection]
		public void TestTrigger_PreventFinalisationWithShortageOfCrossDockStock_MultipleOrders()
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

			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 5m).AppendInsertAndReturnObject(sql);
			var pickLine1 = new WhsPickLine(receiveLine, orderLine1, 5m) { WZ_OriginalReservedQty = 5m }.AppendInsertAndReturnObject(sql);

			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O2").AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order2, product.PK, 5m).AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine, orderLine2, 5m) { WZ_OriginalReservedQty = 5m }.AppendInsertAndReturnObject(sql);

			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O3").AppendInsertAndReturnObject(sql);
			var orderLine3 = new WhsDocketLine(order3, product.PK, 5m).AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(receiveLine, orderLine3, 5m) { WZ_OriginalReservedQty = 5m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends()));

			var decreaseStockSQL = WhsDocketLine
					.UpdateWhere(receiveLine.PK)
					.Set(l => l.WE_StockOnHand, 0m)
					.Set(l => l.WE_TransactionQuantity, 0m).AsSQL();
			AssertNoExceptionThrown("Can commit 15 units for order1 when 15 units are expected without trigger exceptions.", () => SaveToDB(decreaseStockSQL));
			WhsDocketLine.AssertFromDB(Db.Connection, receiveLine.PK)
				.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 15)
				.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 0)
				.ExpectEquals("Stock on Hand Quantity", r => r.WE_StockOnHand, 0)
				.VerifyAll();

			var finaliseSQL = CreateFinaliseDocketLineSQL(receive, receiveLine);

			AssertExceptionThrown("Cannot finalise receive with shortage of stock for picks.", typeof(SqlException), () => SaveToDB(finaliseSQL));
			Db.Connection.CommitTransaction();
		}

		#endregion

		#region TestTrigger_PreventFinalisationWithShortageOfCrossDockStock_MultipleReceiveLines

		[UseSnapshotProtection]
		public void TestTrigger_PreventFinalisationWithShortageOfCrossDockStock_MultipleReceiveLines()
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
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 5m, location.PK) { WE_ClientOrderedUnits = 5m, WE_StockOnHand = 5m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);

			var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R2").AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive2, product.PK, 5m, location.PK) { WE_ClientOrderedUnits = 5m, WE_StockOnHand = 5m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);

			var receive3 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R3").AppendInsertAndReturnObject(sql);
			var receiveLine3 = new WhsDocketLine(receive3, product.PK, 5m, location.PK) { WE_ClientOrderedUnits = 5m, WE_StockOnHand = 5m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);

			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 15m).AppendInsertAndReturnObject(sql);
			var pickLine1 = new WhsPickLine(receiveLine1, orderLine, 5m) { WZ_OriginalReservedQty = 5m }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, orderLine, 5m) { WZ_OriginalReservedQty = 5m }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(receiveLine3, orderLine, 5m) { WZ_OriginalReservedQty = 5m }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends()));

			var decreaseStockSQL = WhsDocketLine
					.UpdateWhere(receiveLine3.PK)
					.Set(l => l.WE_StockOnHand, 0m)
					.Set(l => l.WE_TransactionQuantity, 0m).AsSQL();
			AssertNoExceptionThrown("Can commit 15 units for order1 when 15 units total are expected without trigger exceptions.", () => SaveToDB(decreaseStockSQL));
			WhsDocketLine.AssertFromDB(Db.Connection, receiveLine3.PK)
				.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 5)
				.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 0)
				.ExpectEquals("Stock on Hand Quantity", r => r.WE_StockOnHand, 0)
				.VerifyAll();

			var finaliseReceive1SQL = CreateFinaliseDocketLineSQL(receive1, receiveLine1);
			var finaliseReceive2SQL = CreateFinaliseDocketLineSQL(receive2, receiveLine2);
			var finaliseReceive3SQL = CreateFinaliseDocketLineSQL(receive3, receiveLine3);

			AssertNoExceptionThrown("Can finalise receive with sufficient stock for picks.", () => SaveToDB(finaliseReceive1SQL));
			AssertNoExceptionThrown("Can finalise receive with sufficient stock for picks.", () => SaveToDB(finaliseReceive2SQL));
			AssertExceptionThrown("Cannot finalise receive with shortage of stock for picks.", typeof(SqlException), () => SaveToDB(finaliseReceive3SQL));
			Db.Connection.CommitTransaction();
		}

		#endregion

		#region TestTrigger_PreventFinalisationWithShortageOfCrossDockStock_SingleOrder

		[UseSnapshotProtection]
		public void TestTrigger_PreventFinalisationWithShortageOfCrossDockStock_TransactionQuantityGreaterThanExpectedQuantity()
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

			AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends()));

			var decreaseStockSQL = WhsDocketLine
					.UpdateWhere(receiveLine.PK)
					.Set(l => l.WE_ClientOrderedUnits, 5m).AsSQL();
			AssertNoExceptionThrown("Can commit 15 units for order1 when 15 units available.", () => SaveToDB(decreaseStockSQL));
			WhsDocketLine.AssertFromDB(Db.Connection, receiveLine.PK)
				.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 5)
				.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 15)
				.ExpectEquals("Stock on Hand Quantity", r => r.WE_StockOnHand, 15)
				.VerifyAll();

			var finaliseSQL = CreateFinaliseDocketLineSQL(receive, receiveLine);
			AssertNoExceptionThrown("Receive can be finalised.", () => SaveToDB(finaliseSQL));
		}

		#endregion

		#endregion

		#region TestTrigger_ConcurrencyHandling

		#region TestTrigger_ConcurrencyHandling_CantFinaliseWhileAlteringStock

		[UseSnapshotProtection]
		public void TestTrigger_ConcurrencyHandling_CantFinaliseWhileAlteringStock()
		{
			using (var conn1 = Db.NewExtraConnectionToMainDb())
			using (var conn2 = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(conn1);
				var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(conn1);
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

				AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends(), conn1));

				var finaliseSQL = CreateFinaliseDocketLineSQL(receive, receiveLine);

				bool finaliseQueryStarted = false;
				bool finaliseEnded = false;

				conn1.BeginTransaction();
				conn2.BeginTransaction();

				using (var finaliseCommand = conn2.Command(finaliseSQL))
				{
					var task = new Task(() =>
					{
						finaliseQueryStarted = true;
						AssertExceptionThrown("Command to finalise should fail due to shortage of stock.", typeof(SqlException), () => finaliseCommand.ExecuteNonQuery());
						finaliseEnded = true;
					});

					WhsDocketLine.UpdateWhere(receiveLine.PK).Set(l => l.WE_StockOnHand, 0).Set(l => l.WE_TransactionQuantity, 0).Post(conn1);

					task.Start();
					Thread.Sleep(5000);
					AssertEquals("SQL Command to finalise the should have executed.", true, finaliseQueryStarted);
					AssertEquals("SQL Command to finalise should be blocked by the SQL Lock.", false, finaliseEnded);
					conn1.CommitTransaction();

					task.Wait();
					AssertEquals("SQL Command to finalise should have finished and been prevented by the Trigger.", true, finaliseEnded);
				}

				WhsDocketLine.AssertFromDB(Db.Connection, receiveLine.PK)
					.ExpectEquals("Docket Line Status", r => r.WE_DocketLineStatus, string.Empty)
					.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 15)
					.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 0)
					.ExpectEquals("Stock on Hand Quantity", r => r.WE_StockOnHand, 0)
					.VerifyAll();
			}
		}

		#endregion

		#region TestTrigger_ConcurrencyHandling_CanFinaliseBeforeAlteringStock

		[UseSnapshotProtection]
		public void TestTrigger_ConcurrencyHandling_CanFinaliseBeforeAlteringStock()
		{
			using (var conn1 = Db.NewExtraConnectionToMainDb())
			using (var conn2 = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(conn1);
				var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(conn1);
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

				AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends(), conn1));

				var finaliseSQL = CreateFinaliseDocketLineSQL(receive, receiveLine);
				var alterSQL = WhsDocketLine
					.UpdateWhere(receiveLine.PK)
					.Set(l => l.WE_StockOnHand, 0m)
					.Set(l => l.WE_TransactionQuantity, 0m).AsSQL();

				bool alterStockQueryStarted = false;
				bool alterStockEnded = false;

				conn1.BeginTransaction();
				conn2.BeginTransaction();

				using (var finaliseCommand = conn1.Command(finaliseSQL))
				using (var alterCommand = conn2.Command(alterSQL))
				{
					var task = new Task(() =>
					{
						alterStockQueryStarted = true;
						AssertExceptionThrown<SqlException>("Command to alter stock should fail due to Finalised Docket.", () => alterCommand.ExecuteNonQuery());
						alterStockEnded = true;
					});

					AssertNoExceptionThrown("Command to finalise should succeed.", () => finaliseCommand.ExecuteNonQuery());

					task.Start();
					Thread.Sleep(5000);
					AssertEquals("SQL Command to finalise the should have executed.", true, alterStockQueryStarted);
					AssertEquals("SQL Command to finalise should be blocked by the SQL Lock.", false, alterStockEnded);
					conn1.CommitTransaction();

					task.Wait();
					AssertEquals("SQL Command to finalise should have finished and been prevented by the Trigger.", true, alterStockEnded);
				}

				WhsDocketLine.AssertFromDB(Db.Connection, receiveLine.PK)
					.ExpectEquals("Docket Line Status", r => r.WE_DocketLineStatus, "FIN")
					.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 15)
					.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 15)
					.ExpectEquals("Stock on Hand Quantity", r => r.WE_StockOnHand, 15)
					.VerifyAll();
			}
		}

		#endregion

		#region TestTrigger_ConcurrencyHandling_CannotFinaliseWhileAttachingOrder

		[UseSnapshotProtection]
		public void TestTrigger_ConcurrencyHandling_CannotFinaliseWhileAttachingOrder()
		{
			using (var conn1 = Db.NewExtraConnectionToMainDb())
			using (var conn2 = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(conn1);
				var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(conn1);
				var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
				var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

				var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
				var receiveLine1 = new WhsDocketLine(receive1, product.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
				var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R2").AppendInsertAndReturnObject(sql);
				var receiveLine2 = new WhsDocketLine(receive2, product.PK, 10m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 10m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);

				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 15m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine1, orderLine, 15m) { WZ_OriginalReservedQty = 15m }.AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends(), conn1));

				var finaliseSQL = CreateFinaliseDocketLineSQL(receive2, receiveLine2);

				bool finaliseQueryStarted = false;
				bool finaliseEnded = false;

				conn1.BeginTransaction();
				conn2.BeginTransaction();

				using (var finaliseCommand = conn2.Command(finaliseSQL))
				using (TestWhsDataSetupHelper.SuspendTrigger("TG_PreventOverCommitOfStockViaPickLine", WhsPickLineSchema.Constants.TableName, conn1))
				{
					var task = new Task(() =>
					{
						finaliseQueryStarted = true;
						TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(ex => ex.Message.Contains("Cannot Finalise while Cross Dock Orders Unfufilled."));
						AssertExceptionThrown("Command to finalise should fail due to shortage of stock.", typeof(SqlException), () => finaliseCommand.ExecuteNonQuery());
						finaliseEnded = true;
					});

					WhsPickLine.UpdateWhere(pickLine.PK).Set(l => l.WZ_WE_InventoryLine, (ForeignKey)receiveLine2.PK).Post(conn1);

					task.Start();
					Thread.Sleep(5000);
					AssertEquals("SQL Command to finalise the should have executed.", true, finaliseQueryStarted);
					AssertEquals("SQL Command to finalise should be blocked by the SQL Lock.", false, finaliseEnded);
					conn1.CommitTransaction();

					task.Wait();
					AssertEquals("SQL Command to finalise should have finished and been prevented by the Trigger.", true, finaliseEnded);
				}

				WhsDocketLine.AssertFromDB(Db.Connection, receiveLine2.PK)
					.ExpectEquals("Docket Line Status", r => r.WE_DocketLineStatus, string.Empty)
					.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 15)
					.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 10)
					.ExpectEquals("Stock on Hand Quantity", r => r.WE_StockOnHand, 10)
					.VerifyAll();
			}
		}

		#endregion

		#region TestTrigger_ConcurrencyHandling_CanFinaliseBeforeAttachingOrder

		[UseSnapshotProtection]
		public void TestTrigger_ConcurrencyHandling_CanFinaliseBeforeAttachingOrder()
		{
			using (var conn1 = Db.NewExtraConnectionToMainDb())
			using (var conn2 = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(conn1);
				var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(conn1);
				var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
				var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);

				var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
				var receiveLine1 = new WhsDocketLine(receive1, product.PK, 15m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 15m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
				var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R2").AppendInsertAndReturnObject(sql);
				var receiveLine2 = new WhsDocketLine(receive2, product.PK, 10m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 10m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);

				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 15m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine1, orderLine, 15m) { WZ_OriginalReservedQty = 15m }.AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends(), conn1));

				var finaliseSQL = CreateFinaliseDocketLineSQL(receive2, receiveLine2);
				var attachSQL = string.Format(@"UPDATE dbo.WhsPickLine SET WZ_WE_TransactionLine = '{0}', WZ_SystemLastEditTimeUtc = SYSUTCDATETIME(), WZ_SystemLastEditUser = '~BP' WHERE WZ_PK = '{1}'", receiveLine2.PK, pickLine.PK);

				bool attachQueryStarted = false;
				bool attachEnded = false;

				conn1.BeginTransaction();
				conn2.BeginTransaction();
				using (var finaliseCommand = conn1.Command(finaliseSQL))
				using (var attachCommand = conn2.Command(attachSQL))
				{
					var task = new Task(() =>
					{
						attachQueryStarted = true;
						AssertExceptionThrown("Cannot attach reserve line to a finalised docket.", typeof(SqlException), () => attachCommand.ExecuteNonQuery());
						attachEnded = true;
					});

					AssertNoExceptionThrown("Command to finalise should succeed.", () => finaliseCommand.ExecuteNonQuery());
					task.Start();
					Thread.Sleep(5000);
					AssertEquals("SQL Command to attach the order should have executed.", true, attachQueryStarted);
					AssertEquals("SQL Command to attach the order should be blocked by the SQL Lock.", false, attachEnded);
					conn1.CommitTransaction();

					task.Wait();
					AssertEquals("SQL Command to finalise should have finished and been prevented by the Trigger.", true, attachEnded);
				}

				WhsDocketLine.AssertFromDB(Db.Connection, receiveLine2.PK)
					.ExpectEquals("Docket Line Status", r => r.WE_DocketLineStatus, "FIN")
					.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 15)
					.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 10)
					.ExpectEquals("Stock on Hand Quantity", r => r.WE_StockOnHand, 10)
					.VerifyAll();
			}
		}

		#endregion

		#region TestTrigger_ConcurrencyHandling_CannotFinaliseWhileAlteringPickLine

		[UseSnapshotProtection]
		public void TestTrigger_ConcurrencyHandling_CannotFinaliseWhileAlteringPickLine()
		{
			using (var conn1 = Db.NewExtraConnectionToMainDb())
			using (var conn2 = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(conn1);
				var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(conn1);
				var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
				var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 10m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 20m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, orderLine, 15m) { WZ_OriginalReservedQty = 10m }.AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends(), conn1));

				var finaliseSQL = CreateFinaliseDocketLineSQL(receive, receiveLine);

				bool finaliseQueryStarted = false;
				bool finaliseEnded = false;

				conn1.BeginTransaction();
				conn2.BeginTransaction();

				using (var finaliseCommand = conn2.Command(finaliseSQL))
				{
					var task = new Task(() =>
					{
						finaliseQueryStarted = true;
						AssertExceptionThrown("Command to finalise should fail due to shortage of stock.", typeof(SqlException), () => finaliseCommand.ExecuteNonQuery());
						finaliseEnded = true;
					});

					WhsPickLine.UpdateWhere(pickLine.PK).Set(l => l.WZ_OriginalReservedQty, 15).Post(conn1);

					task.Start();
					Thread.Sleep(5000);
					AssertEquals("SQL Command to finalise the receive should have executed.", true, finaliseQueryStarted);
					AssertEquals("SQL Command to finalise should be blocked by the SQL Lock.", false, finaliseEnded);
					conn1.CommitTransaction();

					task.Wait();
					AssertEquals("SQL Command to finalise should have finished and been prevented by the Trigger.", true, finaliseEnded);
				}

				WhsDocketLine.AssertFromDB(Db.Connection, receiveLine.PK)
					.ExpectEquals("Docket Line Status", r => r.WE_DocketLineStatus, string.Empty)
					.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 15)
					.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 10)
					.ExpectEquals("Stock on Hand Quantity", r => r.WE_StockOnHand, 10)
					.VerifyAll();
			}
		}

		#endregion

		#region TestTrigger_ConcurrencyHandling_CanFinaliseBeforeAlteringPickLine

		[UseSnapshotProtection]
		public void TestTrigger_ConcurrencyHandling_CanFinaliseBeforeAlteringPickLine()
		{
			using (var conn1 = Db.NewExtraConnectionToMainDb())
			using (var conn2 = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(conn1);
				var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(conn1);
				var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
				var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK) { WE_ClientOrderedUnits = 15m, WE_StockOnHand = 10m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, product.PK, 15m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, orderLine, 10m) { WZ_OriginalReservedQty = 10m }.AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Setup.", () => SaveToDB(sql.ToStringWithNewLineBetweenAppends(), conn1));

				var finaliseSQL = CreateFinaliseDocketLineSQL(receive, receiveLine);
				var alterSQL = WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_Units, 15m)
					.Set(l => l.WZ_OriginalReservedQty, 15m).AsSQL();

				bool finaliseQueryStarted = false;
				bool finaliseEnded = false;

				conn1.BeginTransaction();
				conn2.BeginTransaction();

				using (var finaliseCommand = conn1.Command(finaliseSQL))
				using (var alterCommand = conn2.Command(alterSQL))
				{
					var task = new Task(() =>
					{
						finaliseQueryStarted = true;
						AssertExceptionThrown("Command to alter the pickLine should fail due to insufficient stock.", typeof(SqlException), () => alterCommand.ExecuteNonQuery());
						finaliseEnded = true;
					});

					AssertNoExceptionThrown("Command to finalise should succeed.", () => finaliseCommand.ExecuteNonQuery());

					task.Start();
					Thread.Sleep(5000);
					AssertEquals("SQL Command to finalise the should have executed.", true, finaliseQueryStarted);
					AssertEquals("SQL Command to finalise should be blocked by the SQL Lock.", false, finaliseEnded);
					conn1.CommitTransaction();

					task.Wait();
					AssertEquals("SQL Command to finalise should have finished and been prevented by the Trigger.", true, finaliseEnded);
				}

				WhsDocketLine.AssertFromDB(Db.Connection, receiveLine.PK)
					.ExpectEquals("Docket Line Status", r => r.WE_DocketLineStatus, "FIN")
					.ExpectEquals("Client Ordered Units", r => r.WE_ClientOrderedUnits, 15)
					.ExpectEquals("Transaction Quantity", r => r.WE_TransactionQuantity, 10)
					.ExpectEquals("Stock on Hand Quantity", r => r.WE_StockOnHand, 10)
					.VerifyAll();

				WhsPickLine.AssertFromDB(Db.Connection, pickLine.PK)
					.ExpectEquals("Original Reserved Qty", r => r.WZ_OriginalReservedQty, 10)
					.ExpectEquals("Inventory Line", r => r.WZ_WE_InventoryLine, (ForeignKey)receiveLine.PK)
					.VerifyAll();
			}
		}

		#endregion

		#endregion

		#region Implementation

		void SaveToDB(string sql, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;

			connection.BeginTransactionWithManager();
			connection.ExecuteNonQuery(sql);
			connection.CommitTransaction();
		}

		string CreateFinaliseDocketLineSQL(WhsDocket docket, WhsDocketLine docketLine)
		{
			StringBuilder finaliseQuery = new StringBuilder();
			//Suspend Trigger to allow finalise to succeed when updating docket + docketline
			finaliseQuery.AppendLine($"IF OBJECT_ID('{"TG_CheckDocketLineStatusAndDateForDocketLine"}', 'TR') IS NOT NULL DISABLE TRIGGER {"TG_CheckDocketLineStatusAndDateForDocketLine"} ON {WhsDocketLineSchema.Constants.TableName}");

			finaliseQuery.AppendFormat(@"
UPDATE dbo.WhsDocketLine SET WE_FinalisedDate = '{0}', WE_DocketLineStatus = '{1}', WE_SystemLastEditTimeUtc = SYSUTCDATETIME(), WE_SystemLastEditUser = '~BP' WHERE WE_PK = '{2}'
UPDATE dbo.WhsDocket SET WD_DocketStatus = '{1}', WD_FinalisedDate = '{0}', WD_GS_NKFinalizedBy = 'A', WD_UnloadCompletedTime = '{0}', WD_SystemLastEditTimeUtc = SYSUTCDATETIME(), WD_SystemLastEditUser = '~BP' WHERE WD_PK = '{3}'",
DateTime.Today.ToSqlFormat(), "FIN", docketLine.PK.ToString(), docket.PK.ToString())
				.AppendLine();

			finaliseQuery.Append($"IF OBJECT_ID('{"TG_CheckDocketLineStatusAndDateForDocketLine"}', 'TR') IS NOT NULL ENABLE TRIGGER {"TG_CheckDocketLineStatusAndDateForDocketLine"}  ON {WhsDocketLineSchema.Constants.TableName}");
			return finaliseQuery.ToString();
		}
		#endregion
	}
}
