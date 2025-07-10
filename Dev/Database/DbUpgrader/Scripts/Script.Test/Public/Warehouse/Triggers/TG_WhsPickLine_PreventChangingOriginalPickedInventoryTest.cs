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
	[TestedType(typeof(TG_WhsPickLine_PreventChangingOriginalPickedInventory))]
	class TG_WhsPickLine_PreventChangingOriginalPickedInventoryTest : DBCreateTriggerScriptTest
	{
	}

	[UseSnapshotProtection]
	class Trigger_TG_WhsPickLine_PreventChangingOriginalPickedInventoryTest : TestCase
	{
		#region TestTrigger_OriginalPickedInventory_CanBeSet

		public void TestTrigger_OriginalPickedInventory_CanBeSet()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var testData = CreateNewTestData(mainConnection, sql);

				using (mainConnection.BeginTransactionWithManager())
				{
					using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
					using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, mainConnection))
					{
						mainConnection.ExecuteScalar(sql.ToString());
					}
					mainConnection.CommitTransaction();
					AssertNoExceptionThrown("Should be valid to set OriginalPickedInventory from empty.", () => UpdateOriginalPickedInventory(mainConnection, testData.PickLine, testData.OtherInventoryLine1));
				}
			}
		}

		#endregion

		#region TestTrigger_OriginalOrderLine_CanBeSet

		public void TestTrigger_OriginalOrderLine_CanBeSet()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var testData = CreateNewTestDataWithOutboundTransfer(mainConnection, sql);

				using (mainConnection.BeginTransactionWithManager())
				{
					using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
					using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, mainConnection))
					using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
					{
						mainConnection.ExecuteNonQuery(sql.ToString());
					}
					mainConnection.CommitTransaction();
					AssertNoExceptionThrown("Should be valid to set OriginalOrderLine from empty.",
						() => ExecuteSqlInTransaction(mainConnection, WhsPickLine.UpdateWhere(testData.TransferPickLine.PK).Set(l => l.WZ_WE_OriginalOrderLine, testData.OrderLine).AsSQL()));
				}
			}
		}

		#endregion

		#region TestTrigger_OriginalPickedInventory_CannotBeChanged

		public void TestTrigger_OriginalPickedInventory_CannotBeChanged()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var testData = CreateNewTestData(mainConnection, sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToString());

				UpdateOriginalPickedInventory(mainConnection, testData.PickLine, testData.OtherInventoryLine1);

				AssertInnermostException("Should *not* be valid to change OriginalPickedInventory.", typeof(SqlException),
					"Attempt to change WZ_WE_OriginalPickedInventoryLine or WZ_WE_OriginalOrderLine after it was set.", () => UpdateOriginalPickedInventory(mainConnection, testData.PickLine, testData.OtherInventoryLine2), assertMessageStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_OriginalOrderLine_CannotBeChanged

		public void TestTrigger_OriginalOrderLine_CannotBeChanged()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var testData = CreateNewTestDataWithOutboundTransfer(mainConnection, sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToString());

				ExecuteSqlInTransaction(mainConnection, WhsPickLine.UpdateWhere(testData.TransferPickLine.PK).Set(l => l.WZ_WE_OriginalOrderLine, testData.OrderLine).AsSQL());

				AssertInnermostException("Should *not* be valid to change OriginalOrderLine.", typeof(SqlException),
					"Attempt to change WZ_WE_OriginalPickedInventoryLine or WZ_WE_OriginalOrderLine after it was set.",
					() => ExecuteSqlInTransaction(mainConnection, WhsPickLine.UpdateWhere(testData.TransferPickLine.PK).Set(l => l.WZ_WE_OriginalOrderLine, testData.OtherOrderLine).AsSQL()),
					assertMessageStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_OriginalPickedInventory_CannotBeCleared

		public void TestTrigger_OriginalPickedInventory_CannotBeCleared()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var testData = CreateNewTestData(mainConnection, sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToString());

				UpdateOriginalPickedInventory(mainConnection, testData.PickLine, testData.OtherInventoryLine1);

				AssertInnermostException("Should *not* be valid to clear OriginalPickedInventory.", typeof(SqlException),
					"Attempt to change WZ_WE_OriginalPickedInventoryLine or WZ_WE_OriginalOrderLine after it was set.", () => UpdateOriginalPickedInventory(mainConnection, testData.PickLine, null), assertMessageStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_OriginalOrderLine_CannotBeCleared

		public void TestTrigger_OriginalOrderLine_CannotBeCleared()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var testData = CreateNewTestDataWithOutboundTransfer(mainConnection, sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToString());

				ExecuteSqlInTransaction(mainConnection, WhsPickLine.UpdateWhere(testData.TransferPickLine.PK).Set(l => l.WZ_WE_OriginalOrderLine, testData.OrderLine).AsSQL());

				AssertInnermostException("Should *not* be valid to clear OriginalOrderLine.", typeof(SqlException),
					"Attempt to change WZ_WE_OriginalPickedInventoryLine or WZ_WE_OriginalOrderLine after it was set.",
					() => ExecuteSqlInTransaction(mainConnection, WhsPickLine.UpdateWhere(testData.TransferPickLine.PK).Set(l => l.WZ_WE_OriginalOrderLine, null).AsSQL()),
					assertMessageStartsWith: true);
			}
		}

		#endregion

		#region TestTrigger_DoesNotBlowUpIfUpdatedButUnchanged

		public void TestTrigger_DoesNotBlowUpIfUpdatedButUnchanged()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var testData = CreateNewTestData(mainConnection, sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToString());

				UpdateOriginalPickedInventory(mainConnection, testData.PickLine, testData.OtherInventoryLine1);

				AssertNoExceptionThrown("Should *not* blowup if unchanged.", () => UpdateOriginalPickedInventory(mainConnection, testData.PickLine, testData.OtherInventoryLine1));
			}
		}

		public void TestTrigger_DoesNotBlowUpIfUpdatedButUnchanged_OriginalOrderLine()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var testData = CreateNewTestDataWithOutboundTransfer(mainConnection, sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToString());

				ExecuteSqlInTransaction(mainConnection, WhsPickLine.UpdateWhere(testData.TransferPickLine.PK).Set(l => l.WZ_WE_OriginalOrderLine, testData.OrderLine).AsSQL());

				AssertNoExceptionThrown("Should *not* blowup if unchanged.",
					() => ExecuteSqlInTransaction(mainConnection, WhsPickLine.UpdateWhere(testData.TransferPickLine.PK).Set(l => l.WZ_WE_OriginalOrderLine, testData.OrderLine).AsSQL()));
			}
		}

		#endregion

		#region TestTrigger_DoesNotBlowUpIfUpdatedButUnchanged_Null

		public void TestTrigger_DoesNotBlowUpIfUpdatedButUnchanged_Null()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var testData = CreateNewTestData(mainConnection, sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToString());

				AssertNoExceptionThrown("Should *not* blowup if unchanged.", () => UpdateOriginalPickedInventory(mainConnection, testData.PickLine, null));
			}
		}

		public void TestTrigger_DoesNotBlowUpIfUpdatedButUnchanged_Null_OriginalOrderLine()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var testData = CreateNewTestDataWithOutboundTransfer(mainConnection, sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToString());

				AssertNoExceptionThrown("Should *not* blowup if unchanged.",
					() => ExecuteSqlInTransaction(mainConnection, WhsPickLine.UpdateWhere(testData.TransferPickLine.PK).Set(l => l.WZ_WE_OriginalOrderLine, null).AsSQL()));
			}
		}

		#endregion

		#region Implementation

		class TestData
		{
			internal TestData(WhsPickLine pickLine, WhsDocketLine otherInventoryLine1, WhsDocketLine otherInventoryLine2)
			{
				PickLine = pickLine;
				OtherInventoryLine1 = otherInventoryLine1;
				OtherInventoryLine2 = otherInventoryLine2;
			}

			public readonly WhsPickLine PickLine;
			public readonly WhsDocketLine OtherInventoryLine1;
			public readonly WhsDocketLine OtherInventoryLine2;
		}

		class TestDataWithOutboundTransfers
		{
			public TestDataWithOutboundTransfers(WhsPickLine transferPickLine, WhsDocketLine orderLine, WhsDocketLine otherOrderLine)
			{
				TransferPickLine = transferPickLine;
				OrderLine = orderLine;
				OtherOrderLine = otherOrderLine;
			}

			public readonly WhsPickLine TransferPickLine;
			public readonly WhsDocketLine OrderLine;
			public readonly WhsDocketLine OtherOrderLine;
		}

		TestDataWithOutboundTransfers CreateNewTestDataWithOutboundTransfer(DbConnection connection, SqlQueryBuilder sql)
		{
			var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(connection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			// receive stock
			var now = DateTime.Now.ToSmallDateTimeFloor();
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK) { WE_StockOnHand = 0m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "P1", "PIS").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);
			var otherOrderLine = new WhsDocketLine(order, product.PK, 1m).AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1") { WD_WP_ParentPickForTransfer = pick }.AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 10m, whs.WW_DefaultOutboundDockDoor.FK)
			{
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = locationA1.PK,
				WE_OriginalInventoryStatus = "INT",
				WE_CurrentInventoryStatus = "INT",
				WE_StockOnHand = 10m
			}.AppendInsertAndReturnObject(sql);

			var transferPickLine = new WhsPickLine(receiveLine, transferLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = DateTimeOffset.Now }.AppendInsertAndReturnObject(sql);
			new WhsPickLine(transferLine, orderLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_WE_OriginalPickedInventoryLine = receiveLine }.AppendInsertAndReturnObject(sql);

			return new TestDataWithOutboundTransfers(transferPickLine, orderLine, otherOrderLine);
		}

		TestData CreateNewTestData(DbConnection connection, SqlQueryBuilder sql)
		{
			var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(connection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			// receive stock
			var now = DateTime.Now.ToSmallDateTimeFloor();
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK) { WE_StockOnHand = 0m }.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);
			var receiveLine3 = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

			// order stock
			var pick = new WhsPick(whs, "P1", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1") { WD_FinalisedDate = now, WD_WP = pick.PK, WD_GS_NKFinalizedBy = "E" }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 50m) { WE_DocketLineStatus = "DEP", WE_FinalisedDate = now }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine1, orderLine, 50m) { WZ_PickedDateTime = now, WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);

			return new TestData(pickLine, receiveLine2, receiveLine3);
		}

		void UpdateOriginalPickedInventory(DbConnection connection, WhsPickLine pickLine, WhsDocketLine inventoryLine)
		{
			ExecuteSqlInTransaction(connection,
				WhsPickLine
				.UpdateWhere(pickLine.PK)
				.Set(l => l.WZ_WE_OriginalPickedInventoryLine, inventoryLine).AsSQL());
		}

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, connection))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		#endregion
	}
}
