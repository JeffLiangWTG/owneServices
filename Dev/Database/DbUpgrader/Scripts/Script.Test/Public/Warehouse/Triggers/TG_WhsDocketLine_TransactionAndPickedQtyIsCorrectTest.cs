using System;
using System.Linq;
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
	[TestedType(typeof(TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect))]
	class TG_WhsDocketLine_TransactionAndPickedQtyIsCorrectTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsDocketLine_TransactionAndPickedQtyIsCorrectTest : TestCase
	{
		const string TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert = "TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert";
		const string TG_WhsPickLine_TransactionAndPickedQtyIsCorrect = "TG_WhsPickLine_TransactionAndPickedQtyIsCorrect ";

		#region TestTrigger_Update

		[UseSnapshotProtection]
		public void TestTrigger_Update_WE_TransactionQuantity()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
				var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				// receive stock
				var today = DateTime.Now;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				// transfer stock
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "TR1").AppendInsertAndReturnObject(sql);
				var transferLine = new WhsDocketLine(transfer, part.PK, 10m, locationA2.PK) { WE_WL_TransferFrom = locationA1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, transferLine, 10m).AppendInsertAndReturnObject(sql);

				// defer triggers to save data into DB
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName, mainConnection))
				{
					ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				}

				// update line comment - should not effect trigger
				var updateSQL = WhsDocketLine.UpdateWhere(transferLine.PK).Set(dl => dl.WE_LineComment, "bla").AsSQL();
				// mocked procedure will always throw exception if run
				AssertNoExceptionThrown("Update of not related field should NOT trigger the trigger.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect, updateSQL, commitChanges: true));

				// update transfer line qty - should call check procedure
				AssertCheckProcedureRanForTheTransactionLines(
					"Update of WE_TransactionQuantity should trigger check procedure.",
					mainConnection,
					WhsDocketLine
					.UpdateWhere(transferLine.PK)
					.Set(l => l.WE_TransactionQuantity, 7m).AsSQL(),
					transferLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_AdjustmentLines()
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

				var today = DateTime.Now;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);
				var adjustmentOutLine = new WhsDocketLine(adjustment, part.PK, -25m, locationA1.PK).AppendInsertAndReturnObject(sql);
				var adjustmentInLine = new WhsDocketLine(adjustment, part.PK, 25m, locationA1.PK).AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, adjustmentOutLine, 25m).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				{
					ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				}

				var updateStatement1 = WhsDocketLine.UpdateWhere(adjustmentInLine.PK).Set(l => l.WE_TransactionQuantity, 26m).AsSQL();
				TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect, updateStatement1);

				var updateStatement2 = WhsDocketLine.UpdateWhere(adjustmentOutLine.PK).Set(l => l.WE_TransactionQuantity, -24m).AsSQL();
				AssertCheckProcedureRanForTheTransactionLines("If trigger is not suspended then check procedure should be run.", mainConnection, updateStatement2, adjustmentOutLine);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_ReceiveLines()
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

				var today = DateTime.Now;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var updateStatement = WhsDocketLine.UpdateWhere(receiveLine.PK).Set(l => l.WE_TransactionQuantity, 49m).Set(l => l.WE_StockOnHand, 49m).AsSQL();
				TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect, updateStatement);
				Assert(true); // If test gets to this point, the test has passed
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_OrderLines()
		{
			AssertTrigger_PickableDocketLines("ORD");
		}

		[UseSnapshotProtection]
		public void TestTrigger_WorkOrderLines()
		{
			AssertTrigger_PickableDocketLines("WOR");
		}

		[UseSnapshotProtection]
		public void TestTrigger_DynamicWorkOrderLines()
		{
			AssertTrigger_PickableDocketLines("DWO");
		}

		void AssertTrigger_PickableDocketLines(string docketType)
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

				var today = DateTime.Now;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				var subType = docketType == "ORD" ? "ORD" : "DIS";
				var pick = new WhsPick(whs, "P1", "PIS", docketType).AppendInsertAndReturnObject(sql);
				var pickableDocket = new WhsDocket(client.PK, whs.PK, docketType, subType, "ATP", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var pickableDocketLine = new WhsDocketLine(pickableDocket, part.PK, 25m).AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, pickableDocketLine, 25m).AppendInsertAndReturnObject(sql);
				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var updateStatement = WhsDocketLine.UpdateWhere(pickableDocketLine.PK).Set(l => l.WE_TransactionQuantity, 24m).AsSQL();
				AssertCheckProcedureRanForTheTransactionLines("If trigger is not suspended then check procedure should be run.", mainConnection, updateStatement, pickableDocketLine);
			}
		}

		#endregion

		#region AssertCheckProcedureRanForTheTransactionLines

		void AssertCheckProcedureRanForTheTransactionLines(string errorMessage, DbConnection connection, string sqlToRun, params WhsDocketLine[] expectedDocketLinesInTheProcedure)
		{
			var actualPKsInTheProcedure = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(connection, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect, sqlToRun, expectedDocketLinesInTheProcedure);
			if (actualPKsInTheProcedure == null)
			{
				Fail(errorMessage);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(errorMessage, expectedDocketLinesInTheProcedure.Select(l => l.PK), actualPKsInTheProcedure);
			}
		}

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		#endregion
	}
}
