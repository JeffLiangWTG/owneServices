using System;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocketLine_StockOnHandIsBalanced_Insert))]
	class TG_WhsDocketLine_StockOnHandIsBalanced_InsertTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_WhsDocketLine_StockOnHandIsBalanced_InsertTest : TestCase
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

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_ClientOrderedUnits = 10m, WE_StockOnHand = 49m };
				if (isDeferred)
				{
					var insertReceiveLineSql = new StringBuilder();
					insertReceiveLineSql.AppendLine($"SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert}'");
					insertReceiveLineSql.Append(receiveLine.GetInsertStatement());

					AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.",
							() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert, insertReceiveLineSql.ToString()));
				}
				else
				{
					AssertCheckProcedureRanForTheInventoryLines(connection, "If trigger is not suspended then check procedure should be run.", receiveLine.GetInsertStatement(), receiveLine);
				}
			}
		}

		#endregion

		#region TestTrigger_Insert

		[UseSnapshotProtection]
		public void TestTrigger_Insert()
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

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_ClientOrderedUnits = 10m, WE_StockOnHand = 49m };
				AssertCheckProcedureRanForTheInventoryLines(connection, "Should fire trigger when inserting docket lines.", receiveLine.GetInsertStatement(), receiveLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DoesNotRunForReceiveLineWithFullStockOnHand()
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

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_ClientOrderedUnits = 10m, WE_StockOnHand = 50m };
				AssertNoExceptionThrown("Trigger should not run for receive line with full stock on hand.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert, receiveLine.GetInsertStatement()));
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DoesNotRunForCancelledReceiveLine()
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

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "CAN", "R1") { WD_CanceledTimeUtc = DateTime.UtcNow, WD_GS_NKCanceledBy = "C" }.AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_ClientOrderedUnits = 10m, WE_StockOnHand = 0m, WE_DocketLineStatus = "CAN", WE_OriginalInventoryStatus = "PUT", WE_CurrentInventoryStatus = "PUT" };
				AssertNoExceptionThrown("Trigger should not run for cancelled receive line.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert, receiveLine.GetInsertStatement()));
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DoesNotRunForAdjustmentOutLine()
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

				var today = DateTime.Today;

				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "A1") { WD_FinalisedDate = DateTime.Now, WD_GS_NKFinalizedBy = "F" }.AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var statementToRun = new SqlQueryBuilder($"EXEC dbo.SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert}'");
				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, -50m, locationA1.PK) { WE_StockOnHand = 0m }.AppendInsertAndReturnObject(statementToRun);
				AssertNoExceptionThrown("Trigger should not run for adjustment out.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert, statementToRun.ToStringWithNewLineBetweenAppends()));
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DoesNotRunForUnfinalisedAdjustmentLine()
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

				var today = DateTime.Today;
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 0m };
				AssertNoExceptionThrown("Trigger should not run for unfinalised adjustment in.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert, adjustmentLine.GetInsertStatement()));
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DoesRunForFinalisedAdjustmentLine()
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

				var today = DateTime.Today;
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "A1") { WD_FinalisedDate = DateTime.Now, WD_GS_NKFinalizedBy = "F" }.AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 0m };
				AssertCheckProcedureRanForTheInventoryLines(connection, "Should fire trigger when inserting finalised Adjustment lines.", adjustmentLine.GetInsertStatement(), adjustmentLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DoesNotRunForFinalisedAdjustmentLineWithFullStockOnHand()
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

				var today = DateTime.Today;
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "A1") { WD_FinalisedDate = DateTime.Now, WD_GS_NKFinalizedBy = "F" }.AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m };
				AssertNoExceptionThrown("Trigger should not run for finalised adjustment line with full stock on hand.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert, adjustmentLine.GetInsertStatement()));
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DoesNotRunForUnpickedTransferLine()
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

				var today = DateTime.Today;
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				using (connection.BeginTransactionWithManager())
				{
					connection.ExecuteNonQuery($"EXEC dbo.SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert}'");
					var transferLine = new WhsDocketLine(transfer, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = locationA1.PK, WE_DocketLineStatus = "ENT" };
					AssertNoExceptionThrown("Trigger should not run for unpicked transfer line.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert, transferLine.GetInsertStatement()));
				}
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DoesRunForPickedTransferLine()
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

				var today = DateTime.Today;
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var statementToRun = new SqlQueryBuilder($"EXEC dbo.SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert}'");
				var transferLine = new WhsDocketLine(transfer, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = locationA1.PK, WE_DocketLineStatus = "HFT" }.AppendInsertAndReturnObject(statementToRun);
				AssertCheckProcedureRanForTheInventoryLines(connection, "Should fire trigger when inserting picked Transfer lines.", statementToRun.ToStringWithNewLineBetweenAppends(), transferLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DoesRunForFinalisedTransferLine()
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

				var today = DateTime.Today;
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var statementToRun = new SqlQueryBuilder($"EXEC dbo.SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert}'");
				var transferLine = new WhsDocketLine(transfer, part.PK, 50m, locationA1.PK)
				{
					WE_StockOnHand = 0m,
					WE_WL_TransferFrom = locationA1.PK,
					WE_DocketLineStatus = "FIN",
					WE_PutawayTime = today,
					WE_FinalisedDate = today,
					WE_GS_NKPutawayBy = "P"
				}.AppendInsertAndReturnObject(statementToRun);
				AssertCheckProcedureRanForTheInventoryLines(connection, "Should fire trigger when inserting finalised Transfer lines.", statementToRun.ToStringWithNewLineBetweenAppends(), transferLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DoesNotRunForFinalisedTransferLineWithFullStockOnHand()
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

				var today = DateTime.Today;
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var statementToRun = new SqlQueryBuilder($"EXEC dbo.SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert}'");
				var transferLine = new WhsDocketLine(transfer, part.PK, 50m, locationA1.PK)
				{
					WE_StockOnHand = 50m,
					WE_WL_TransferFrom = locationA1.PK,
					WE_DocketLineStatus = "FIN",
					WE_PutawayTime = today,
					WE_FinalisedDate = today,
					WE_GS_NKPutawayBy = "P"
				}.AppendInsertAndReturnObject(statementToRun);
				AssertNoExceptionThrown("Trigger should not run for finalised transfer line with full stock on hand.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert, statementToRun.ToStringWithNewLineBetweenAppends()));
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_NotRunForOrdersAndWorkOrders()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(connection);
				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				// order
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, part.PK, 10m).AppendInsertAndReturnObject(sql);

				// work order
				var workOrder = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ENT", "WO1").AppendInsertAndReturnObject(sql);
				var workOrderLine = new WhsDocketLine(workOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);

				// dynamic work order
				var dynamicWorkOrder = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "ENT", "DO1").AppendInsertAndReturnObject(sql);
				var dynamicWorkOrderLine = new WhsDocketLine(dynamicWorkOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("The check procedure should not be run when inserting / updating Order or Work Order.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced, sql.ToStringWithNewLineBetweenAppends()));
			}
		}

		#endregion

		#region TestTrigger_Delete

		[UseSnapshotProtection]
		public void TestTrigger_Delete_WithoutNoParentDocketLineAndNoTriggerDeferral()
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

				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				AssertNoExceptionThrown("Deleting docket line with no parent docket line and no trigger deferral should not cause exceptions.",
					() => ExecuteSqlInTransaction(connection, $"delete dbo.WhsDocketLine where WE_PK = '{receiveLine.PK}'"));
			}
		}

		#endregion

		#region Implementations

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		void AssertCheckProcedureRanForTheInventoryLines(DbConnection connection, string errorMessage, string sqlToRun, params WhsDocketLine[] expectedDocketLinesInTheProcedure)
		{
			var actualPKsInTheProcedure = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert, sqlToRun, expectedDocketLinesInTheProcedure);
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
