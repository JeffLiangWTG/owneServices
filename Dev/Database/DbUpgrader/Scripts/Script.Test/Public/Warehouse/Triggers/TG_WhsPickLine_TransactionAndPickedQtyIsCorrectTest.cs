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
	[TestedType(typeof(TG_WhsPickLine_TransactionAndPickedQtyIsCorrect))]
	class TG_WhsPickLine_TransactionAndPickedQtyIsCorrectTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsPickLine_TransactionAndPickedQtyIsCorrectTest : TestCase
	{
		const string TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert = "TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert";
		const string TG_WhsPickLine_TransactionAndPickedQtyIsCorrect = "TG_WhsPickLine_TransactionAndPickedQtyIsCorrect";

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
				var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
				var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				// receive stock
				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				// transfer stock
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "TR1").AppendInsertAndReturnObject(sql);
				var transferLine = new WhsDocketLine(transfer, part.PK, 10m, locationA2.PK) { WE_WL_TransferFrom = locationA1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);

				mainConnection.BeginTransaction();
				// suspend trigger to save bad data into DB (no pick line)
				using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, mainConnection))
				{
					mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				}
				mainConnection.CommitTransaction();

				var pickLine = new WhsPickLine(receiveLine, transferLine, 10m);
				if (isDeferred)
				{
					// mocked procedure will always throw exception if run
					AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.", () =>
					{
						var sqlToRun = $"SuspendTrigger '{TG_WhsPickLine_TransactionAndPickedQtyIsCorrect}'" + pickLine.GetInsertStatement();
						TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect, sqlToRun);
					});
				}
				else
				{
					AssertCheckProcedureRanForTheTransactionLines("If trigger is not suspended then check procedure should be run.", mainConnection, pickLine.GetInsertStatement(), transferLine);
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
				var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
				var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				// receive stock
				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				// transfer stock
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "TR1").AppendInsertAndReturnObject(sql);
				var transferLine = new WhsDocketLine(transfer, part.PK, 10m, locationA2.PK) { WE_WL_TransferFrom = locationA1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);

				// suspend trigger to save bad data into DB (no pick line)
				using (mainConnection.BeginTransactionWithManager())
				{
					using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, mainConnection))
					{
						mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
						mainConnection.CommitTransaction();
					}
				}

				var pickLine = new WhsPickLine(receiveLine, transferLine, 10m);
				AssertCheckProcedureRanForTheTransactionLines("Check procedure should be triggered when inserting new pick lines", mainConnection, pickLine.GetInsertStatement(), transferLine);
			}
		}

		#endregion

		#region TestTrigger_Update

		[UseSnapshotProtection]
		public void TestTrigger_Update_WZ_Units()
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
				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				// transfer stock
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "TR1").AppendInsertAndReturnObject(sql);
				var transferLine = new WhsDocketLine(transfer, part.PK, 10m, locationA2.PK) { WE_WL_TransferFrom = locationA1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, transferLine, 10m).AppendInsertAndReturnObject(sql);

				// defer triggers to save data into DB
				using (mainConnection.BeginTransactionWithManager())
				{
					using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
					{
						mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					}

					// update Assigned User - should not effect trigger
					var updateSQL = WhsPickLine.UpdateWhere(pickLine.PK).Set(pl => pl.WZ_GS_NKAssignedTo, "~BP").AsSQL();
					// mocked procedure will always throw exception if run
					AssertNoExceptionThrown("Update of not related field should NOT trigger the trigger.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect, updateSQL));
					mainConnection.CommitTransaction();

					// update WZ_Units - should call check procedure
					AssertCheckProcedureRanForTheTransactionLines(
						"Should run check procedure for the transaction of updated Pick Line.",
						mainConnection,
						WhsPickLine.UpdateWhere(pickLine.PK).Set(pl => pl.WZ_Units, 15m).AsSQL(),
						transferLine);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_WZ_WE_TransactionLine()
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
				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				// transfer stock
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "TR1").AppendInsertAndReturnObject(sql);
				var transferLine1 = new WhsDocketLine(transfer, part.PK, 10m, locationA2.PK) { WE_WL_TransferFrom = locationA1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);
				var transferLine2 = new WhsDocketLine(transfer, part.PK, 10m, locationA2.PK) { WE_WL_TransferFrom = locationA1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, transferLine1, 10m).AppendInsertAndReturnObject(sql);

				// suspend triggers to save bad data into DB (one transfer line have no associated pick lines).
				using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, mainConnection))
				using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName, mainConnection))
				using (mainConnection.BeginTransactionWithManager())
				{
					mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					mainConnection.CommitTransaction();
				}

				// fix balance of transferLine2 by under-committing tranfserLine1 is incorrect
				AssertCheckProcedureRanForTheTransactionLines(
					"Should run check procedure for both old and new transaction line of the updated pick line.",
					mainConnection,
					WhsPickLine.UpdateWhere(pickLine.PK).Set(pl => pl.WZ_WE_TransactionLine.FK, transferLine2.PK).AsSQL(),
					transferLine1, transferLine2);
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
				var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
				var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
				var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				// receive stock
				var today = DateTime.Today;
				var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				// transfer stock
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "TR1").AppendInsertAndReturnObject(sql);
				var transferLine = new WhsDocketLine(transfer, part.PK, 10m, locationA2.PK) { WE_WL_TransferFrom = locationA1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, transferLine, 10m).AppendInsertAndReturnObject(sql);

				// defer triggers to save data into DB
				using (mainConnection.BeginTransactionWithManager())
				{
					using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
					{
						mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
						mainConnection.CommitTransaction();
					}
				}

				AssertCheckProcedureRanForTheTransactionLines(
					"Should run check procedure for the transaction of deleted Pick Line.",
					mainConnection,
					$"delete dbo.WhsPickLine where WZ_PK = '{pickLine.PK}'",
					transferLine);
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
		#endregion
	}
}
