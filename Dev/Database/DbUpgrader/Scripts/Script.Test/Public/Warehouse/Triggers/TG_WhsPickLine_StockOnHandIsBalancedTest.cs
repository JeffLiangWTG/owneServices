using System;
using System.Linq;
using System.Text;
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
	[TestedType(typeof(TG_WhsPickLine_StockOnHandIsBalanced))]
	class TG_WhsPickLine_StockOnHandIsBalancedTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_WhsPickLine_StockOnHandIsBalancedTest : TestCase
	{
		#region TestTrigger_IsDeferred

		[UseSnapshotProtection]
		public void TestTrigger_IsDeferred()
		{
			TestTrigger_DeferralCore(isDeferred: true);
		}

		[UseSnapshotProtection]
		public void TestTrigger_IsNotDeferred()
		{
			TestTrigger_DeferralCore(isDeferred: false);
		}

		void TestTrigger_DeferralCore(bool isDeferred)
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				// adjust out stock
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "AD1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, -10m, locationA1.PK).AppendInsertAndReturnObject(sql);

				// suspend trigger to save bad data into DB (no pick line)
				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName))
				{
					ExecuteSqlInTransaction(mainConnection, sql);
				}

				var pickLine = new WhsPickLine(receiveLine, adjustmentLine, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today };
				if (isDeferred)
				{
					var insertPickLineSql = new StringBuilder();
					insertPickLineSql
						.AppendLine($"SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced}'")
						.Append(pickLine.GetInsertStatement());

					AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.",
							() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced, insertPickLineSql.ToString()));
				}
				else
				{
					AssertCheckProcedureRanForTheInventoryLines(
						"If trigger is not suspended then check procedure should be run.",
						mainConnection,
						pickLine.GetInsertStatement(),
						receiveLine);
				}
			}
		}

		#endregion

		#region TestTrigger_Insert

		[UseSnapshotProtection]
		public void TestTrigger_Insert()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				// adjust out stock
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "AD1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, -10m, locationA1.PK).AppendInsertAndReturnObject(sql);

				// suspend trigger to save bad data into DB (no pick line)
				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName))
				{
					ExecuteSqlInTransaction(mainConnection, sql);
				}

				var pickLine = new WhsPickLine(receiveLine, adjustmentLine, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today };
				AssertCheckProcedureRanForTheInventoryLines(
					"If trigger is not suspended then check procedure should be run.",
					mainConnection,
					pickLine.GetInsertStatement(),
					receiveLine);
			}
		}

		#endregion

		#region TestTrigger_Update

		[UseSnapshotProtection]
		public void TestTrigger_Update_WZ_WE_InventoryLine()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine1 = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);
				var receiveLine2 = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				// adjust out stock
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);
				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, -10m, locationA1.PK).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine1, adjustmentLine, 10m).AppendInsertAndReturnObject(sql);

				// defer triggers to save data into DB
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				{
					ExecuteSqlInTransaction(mainConnection, sql);
				}

				// update assigned user - should not effect trigger
				AssertNoExceptionThrown("Update of not related field should NOT trigger the trigger.", () =>
					TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced,
					WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_GS_NKAssignedTo, "~BP").AsSQL()));

				AssertCheckProcedureRanForTheInventoryLines(
					"When re-linking Pick Line to new Inventory, the Check Procedure should be run for both old and new inventory.",
					mainConnection,
					WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_WE_InventoryLine, receiveLine2).AsSQL(),
					new[] { receiveLine1, receiveLine2 });
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_WZ_Units()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 45m }.AppendInsertAndReturnObject(sql);

				// adjust out stock
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "AD1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, -10m, locationA1.PK).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, adjustmentLine, 5m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
				// suspend triggers to save bad data into DB (under picked)
				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName))
				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName))
				{
					ExecuteSqlInTransaction(mainConnection, sql);
				}

				AssertCheckProcedureRanForTheInventoryLines(
					"When changing WZ_Units the Check Procedure should be run for the inventory.",
					mainConnection,
					WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_Units, 10m).AsSQL(), receiveLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_WZ_PickedDateTime()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				// adjust out stock
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "AD1").AppendInsertAndReturnObject(sql);
				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, -10m, locationA1.PK).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, adjustmentLine, 10m) { WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);

				// defer triggers to save data into DB
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				{
					ExecuteSqlInTransaction(mainConnection, sql);
				}

				AssertCheckProcedureRanForTheInventoryLines(
					"When changing WZ_PickedDateTime the Check Procedure should be run for the inventory.",
					mainConnection,
					WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_PickedDateTime, today).AsSQL(),
					receiveLine);
			}
		}

		#endregion

		#region TestTrigger_Delete

		[UseSnapshotProtection]
		public void TestTrigger_Delete()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 40m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(whs, "P1", "PIS").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, orderLine, 10m) { WZ_GS_NKAssignedTo = "~BP", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);

				// defer triggers to save data into DB
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
				{
					ExecuteSqlInTransaction(mainConnection, sql);
				}

				AssertCheckProcedureRanForTheInventoryLines(
					"When deleting Pick Line the Check Procedure should be run for the inventory.",
					mainConnection,
					$"delete dbo.WhsPickLine where WZ_PK = '{pickLine.PK}'",
					receiveLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Delete_NotRunOnZeroOrNotPickedLines()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(whs, "P1", "PIS").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var pickLine1 = new WhsPickLine(receiveLine, orderLine, 0m) { WZ_OriginalReservedQty = 7m }.AppendInsertAndReturnObject(sql); // reserve some stock, but deallocate allocations so can save 0 Qty pick line
				var pickLine2 = new WhsPickLine(receiveLine, orderLine, 1m).AppendInsertAndReturnObject(sql); // allocate but do NOT pick some stock

				ExecuteSqlInTransaction(mainConnection, sql);

				AssertNoExceptionThrown("The check procedure should not be run when deleting 0 Qty pick lines.", () =>
					TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced, $"delete dbo.WhsPickLine where WZ_PK = '{pickLine1.PK}'"));

				AssertNoExceptionThrown("The check procedure should not be run when deleting not picked pick lines.", () =>
					TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced, $"delete dbo.WhsPickLine where WZ_PK = '{pickLine2.PK}'"));
			}
		}

		#endregion

		#region Implementations

		void ExecuteSqlInTransaction(DbConnection connection, SqlQueryBuilder sql)
		{
			ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());
		}

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		void AssertCheckProcedureRanForTheInventoryLines(string errorMessage, DbConnection connection, string sqlToRun, params WhsDocketLine[] expectedDocketLinesInTheProcedure)
		{
			var actualPKsInTheProcedure = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced, sqlToRun, expectedDocketLinesInTheProcedure);
			if (actualPKsInTheProcedure == null)
			{
				Fail(errorMessage);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(errorMessage, expectedDocketLinesInTheProcedure.Select(l => l.PK), actualPKsInTheProcedure);
			}
		}

		#endregion
	}
}

