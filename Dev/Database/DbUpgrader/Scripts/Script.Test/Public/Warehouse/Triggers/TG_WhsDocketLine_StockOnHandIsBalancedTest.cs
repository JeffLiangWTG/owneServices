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
	[TestedType(typeof(TG_WhsDocketLine_StockOnHandIsBalanced))]
	class TG_WhsDocketLine_StockOnHandIsBalancedTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_WhsDocketLine_StockOnHandIsBalancedTest : TestCase
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
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_ClientOrderedUnits = 10m, WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				var updateStatement = WhsDocketLine.UpdateWhere(receiveLine.PK).Set(l => l.WE_StockOnHand, 49m).AsSQL();
				if (isDeferred)
				{
					var insertReceiveLineSql = new StringBuilder();
					insertReceiveLineSql.AppendLine($"SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced}'");
					insertReceiveLineSql.Append(updateStatement);

					AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.",
							() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced, insertReceiveLineSql.ToString()));
				}
				else
				{
					AssertCheckProcedureRanForTheInventoryLines(connection, "If trigger is not suspended then check procedure should be run.", updateStatement, receiveLine);
				}
			}
		}

		#endregion

		#region TestTrigger_Update

		[UseSnapshotProtection]
		public void TestTrigger_Update_WE_TransactionQuantity()
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
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_ClientOrderedUnits = 50m, WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				AssertNoExceptionThrown("Update of not related field should NOT trigger the trigger.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect,
						WhsDocketLine
						.UpdateWhere(receiveLine.PK)
						.Set(l => l.WE_LineComment, "bla").AsSQL()));

				// update receive line qty - should call check procedure
				AssertCheckProcedureRanForTheInventoryLines(connection, "Update of WE_TransactionQuantity should trigger check procedure.",
					WhsDocketLine
					.UpdateWhere(receiveLine.PK)
					.Set(l => l.WE_TransactionQuantity, 60m).AsSQL(), receiveLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_WE_StockOnHand()
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
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_ClientOrderedUnits = 50m, WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				// update receive line SOH - should call check procedure
				AssertCheckProcedureRanForTheInventoryLines(connection, "Update of WE_StockOnHand should trigger check procedure.",
					WhsDocketLine
					.UpdateWhere(receiveLine.PK)
					.Set(l => l.WE_StockOnHand, 40m).AsSQL(), receiveLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_WE_DocketLineStatus()
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

				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "CAN", "R1") { WD_CanceledTimeUtc = DateTime.Now, WD_GS_NKCanceledBy = "C" }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK)
				{
					WE_ClientOrderedUnits = 50m,
					WE_StockOnHand = 0m,
					WE_OriginalInventoryStatus = "PUT",
					WE_CurrentInventoryStatus = "PUT",
					WE_DocketLineStatus = "CAN",
				}.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_CheckDocketLineStatusAndDateForDocketLine, WhsDocketLineSchema.Constants.TableName, connection))
				{
					AssertCheckProcedureRanForTheInventoryLines(connection, "Update of WE_DocketLineStatus should trigger check procedure.",
						WhsDocketLine.UpdateWhere(receiveLine.PK).Set(l => l.WE_DocketLineStatus, "PFU").AsSQL(),
						receiveLine);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_WE_WE_ParentDocketLine()
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
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 15m }.AppendInsertAndReturnObject(sql);
				var receiveLine_HCC1 = new WhsDocketLine(receive, part.PK, 20m, locationA1.PK)
				{
					WE_StockOnHand = 20m,
					WE_WE_ParentDocketLine = receiveLine,
					WE_IsOriginalInventory = false,
					WE_WE_OriginalDocketLineForRating = receiveLine
				}.AppendInsertAndReturnObject(sql);
				var receiveLine_HCC2 = new WhsDocketLine(receive, part.PK, 15m, locationA1.PK)
				{
					WE_StockOnHand = 15m,
					WE_WE_ParentDocketLine = receiveLine,
					WE_IsOriginalInventory = false,
					WE_WE_OriginalDocketLineForRating = receiveLine
				}.AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, connection))
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
				{
					ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());
				}

				AssertCheckProcedureRanForTheInventoryLines(connection, "Update of WE_WE_ParentDocketLine should trigger check procedure for old and new Line but not the changing line.",
					WhsDocketLine
					.UpdateWhere(receiveLine_HCC2.PK)
					.Set(l => l.WE_WE_ParentDocketLine, receiveLine_HCC1).AsSQL(), receiveLine, receiveLine_HCC1);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_WE_WE_ParentDocketLineToEmpty()
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
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 15m }.AppendInsertAndReturnObject(sql);
				var receiveLine_HCC1 = new WhsDocketLine(receive, part.PK, 20m, locationA1.PK)
				{
					WE_StockOnHand = 20m,
					WE_WE_ParentDocketLine = receiveLine,
					WE_IsOriginalInventory = false,
					WE_WE_OriginalDocketLineForRating = receiveLine
				}.AppendInsertAndReturnObject(sql);
				var receiveLine_HCC2 = new WhsDocketLine(receive, part.PK, 15m, locationA1.PK)
				{
					WE_StockOnHand = 15m,
					WE_WE_ParentDocketLine = receiveLine,
					WE_IsOriginalInventory = false,
					WE_WE_OriginalDocketLineForRating = receiveLine
				}.AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, connection))
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
				{
					ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());
				}

				AssertCheckProcedureRanForTheInventoryLines(connection, "Update of WE_WE_ParentDocketLine should trigger check procedure for old parent Line but not the changing line.",
					WhsDocketLine
					.UpdateWhere(receiveLine_HCC2.PK)
					.Set(l => l.WE_WE_ParentDocketLine, null).Set(l => l.WE_IsOriginalInventory, true).AsSQL(), receiveLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_WE_TransactionQuantityOnChildLine()
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
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 15m }.AppendInsertAndReturnObject(sql);
				var receiveLine_HCC1 = new WhsDocketLine(receive, part.PK, 20m, locationA1.PK)
				{
					WE_StockOnHand = 20m,
					WE_WE_ParentDocketLine = receiveLine,
					WE_IsOriginalInventory = false,
					WE_WE_OriginalDocketLineForRating = receiveLine
				}.AppendInsertAndReturnObject(sql);
				var receiveLine_HCC2 = new WhsDocketLine(receive, part.PK, 15m, locationA1.PK)
				{
					WE_StockOnHand = 15m,
					WE_WE_ParentDocketLine = receiveLine,
					WE_IsOriginalInventory = false,
					WE_WE_OriginalDocketLineForRating = receiveLine
				}.AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, connection))
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
				{
					ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());
				}

				AssertCheckProcedureRanForTheInventoryLines(connection, "Update of WE_TransactionQuantity should trigger check procedure for parent Line.",
					WhsDocketLine
					.UpdateWhere(receiveLine_HCC2.PK)
					.Set(l => l.WE_TransactionQuantity, 10m).Set(l => l.WE_StockOnHand, 10m).AsSQL(), receiveLine, receiveLine_HCC2);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_WE_IsOriginalInventory()
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
				var receiveLine_HCC = new WhsDocketLine(receive, part.PK, 20m, locationA1.PK) { WE_StockOnHand = 20m }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				AssertCheckProcedureRanForTheInventoryLines(connection, "Update of WE_IsOriginalInventory should trigger check procedure for Parent Inventory but not for the changing line.",
					WhsDocketLine
					.UpdateWhere(receiveLine_HCC.PK)
					.Set(l => l.WE_WE_ParentDocketLine, receiveLine)
					.Set(l => l.WE_IsOriginalInventory, false)
					.Set(l => l.WE_WE_OriginalDocketLineForRating, receiveLine).AsSQL(), receiveLine);
			}
		}

		#endregion

		[UseSnapshotProtection]
		public void TestTrigger_Update_AdjustmentOut()
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
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "A1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, -50m, locationA1.PK).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, connection))
				{
					ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());
				}

				// update adjustment out - should not call check procedure
				AssertNoExceptionThrown("Adjustment Out should not trigger SOH trigger.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced,
					$"EXEC dbo.SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect}'\r\n" + 
					WhsDocketLine
						.UpdateWhere(adjustmentLine.PK)
						.Set(l => l.WE_TransactionQuantity, -49).AsSQL()));
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_AdjustmentInNonFinalised()
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
				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, 50m, locationA1.PK).AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				// update unfinalised adjustment line - should not call check procedure
				AssertNoExceptionThrown("Unfinalised Adjustment should not trigger SOH trigger.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced,
					WhsDocketLine
						.UpdateWhere(adjustmentLine.PK)
						.Set(l => l.WE_TransactionQuantity, 40).AsSQL()));
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_AdjustmentIn()
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
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "A1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());

				// update adjustment line SOH - should call check procedure
				AssertCheckProcedureRanForTheInventoryLines(connection, "Update of Adjustment In should trigger check procedure.",
					WhsDocketLine
					.UpdateWhere(adjustmentLine.PK)
					.Set(l => l.WE_StockOnHand, 40m).AsSQL(), adjustmentLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_EnteredTransferLine()
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
				var transferLine = new WhsDocketLine(transfer, part.PK, 50m, locationA1.PK)
				{
					WE_WL_TransferFrom = locationA1.PK,
					WE_DocketLineStatus = "ENT",
				}.AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, connection))
				{
					ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());
				}

				// update entered transfer line - should not call check procedure
				AssertNoExceptionThrown("Entered Transfer Line should not trigger SOH trigger.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(connection, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced,
					$"EXEC dbo.SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect}'\r\n" +
					WhsDocketLine
						.UpdateWhere(transferLine.PK)
						.Set(l => l.WE_TransactionQuantity, 40).AsSQL()));
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_PickedTransferLine()
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
				var transferLine = new WhsDocketLine(transfer, part.PK, 50m, locationA1.PK)
				{
					WE_StockOnHand = 50m,
					WE_WL_TransferFrom = locationA1.PK,
					WE_DocketLineStatus = "HFT",
					WE_CurrentInventoryStatus = "INT",
					WE_OriginalInventoryStatus = "INT",
				}.AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, connection))
				{
					ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());
				}

				// update picked transfer line SOH - should call check procedure
				AssertCheckProcedureRanForTheInventoryLines(connection, "Update of Picked transfer Line should trigger check procedure.",
					WhsDocketLine
					.UpdateWhere(transferLine.PK)
					.Set(l => l.WE_StockOnHand, 40m).AsSQL(), transferLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_FinalisedTransferLine()
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
				var transferLine = new WhsDocketLine(transfer, part.PK, 50m, locationA1.PK)
				{
					WE_StockOnHand = 50m,
					WE_WL_TransferFrom = locationA1.PK,
					WE_DocketLineStatus = "FIN",
					WE_PutawayTime = today,
					WE_FinalisedDate = today,
					WE_GS_NKPutawayBy = "P",
				}.AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, connection))
				{
					ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends());
				}

				var statementToRun = new SqlQueryBuilder($"EXEC dbo.SuspendTrigger '{TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert}'");
				statementToRun.Append(WhsDocketLine.UpdateWhere(transferLine.PK).Set(l => l.WE_StockOnHand, 40m).AsSQL());

				// update finalised transfer line SOH - should call check procedure
				AssertCheckProcedureRanForTheInventoryLines(connection, "Update of Finalised transfer Line should trigger check procedure.",
					WhsDocketLine
					.UpdateWhere(transferLine.PK)
					.Set(l => l.WE_StockOnHand, 40m).AsSQL(), transferLine);
			}
		}

		#region TestTrigger_InterWhsTransfer_MasterSource_FailsIfUpdatingStockOnHandToZero

		[UseSnapshotProtection]
		public void TestTrigger_InterWhsTransfer_MasterSource_FailsIfUpdatingStockOnHandToZero()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(connection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(connection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(connection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(connection);
				var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
				var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
				var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
				var today = DateTime.Today;

				// receive stock
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 40m }.AppendInsertAndReturnObject(sql);

				// transfer stock
				var transfer_Master = new WhsDocket(client.PK, whs.PK, "TFR", "IWS", "ENT", "TR1").AppendInsertAndReturnObject(sql);
				var transferLine_Master = new WhsDocketLine(transfer_Master, part.PK, 10m, location2.PK)
				{
					WE_StockOnHand = 0m,
					WE_WL_TransferFrom = locationA1.PK,
					WE_DocketLineStatus = "FIN",
					WE_PutawayTime = today,
					WE_FinalisedDate = today,
					WE_GS_NKPutawayBy = "P",
				}.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, transferLine_Master, 10m) { WZ_PickedDateTime = today, WZ_GS_NKAssignedTo = "P" }.AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
				{
					AssertNoExceptionThrown("Inter-Whs Source Transfer should have 0 SOH.", () => ExecuteSqlInTransaction(connection, sql.ToStringWithNewLineBetweenAppends()));
				}

				AssertExceptionThrown("IWS Transfer Lines should not have Stock on Hand.",
					typeof(SqlException),
					"Attempt to set Stock on IWS Transfer.",
					() => ExecuteSqlInTransaction(connection, WhsDocketLine.UpdateWhere(transferLine_Master.PK).Set(l => l.WE_StockOnHand, 10m).AsSQL()),
					assertStartsWith: true);
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
