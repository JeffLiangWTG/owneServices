using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))]
	class WhsCheckTransactionAndPickQtyIsCorrect_ForInsertTest : DbCreateScriptTest
	{
		const string ExpectedErrorMessageFromCheckProcedure = "TriggerLikelyConcurrencyError: Transaction and Pick quantity is not correct.";
		const string TG_WhsPickLine_TransactionAndPickedQtyIsCorrect = "TG_WhsPickLine_TransactionAndPickedQtyIsCorrect";
		const string TG_WhsPickLine_StockOnHandIsBalanced = "TG_WhsPickLine_StockOnHandIsBalanced";
		const string TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert = "TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert";
		const string TG_WhsDocketLine_StockOnHandIsBalanced_Insert = "TG_WhsDocketLine_StockOnHandIsBalanced_Insert";

		#region TestCheckProcedure_InternalTransfer

		#region TestCheckProcedure_InternalTransfer_UnderCommit

		public void TestCheckProcedure_InternalTransfer_UnderCommit_NoPickLines()
		{
			TestCheckProcedure_InternalTransferCore_10Units("Check procedure should not allow to under-commit transfer line.", shouldFail: true, pickLineQuantities: Array.Empty<decimal>());
		}

		public void TestCheckProcedure_InternalTransfer_UnderCommit()
		{
			TestCheckProcedure_InternalTransferCore_10Units("Check procedure should not allow to under-commit transfer line.", shouldFail: true, pickLineQuantities: new decimal[] { 5m });
		}

		public void TestCheckProcedure_InternalTransfer_UnderCommit_MultiplePickLines()
		{
			TestCheckProcedure_InternalTransferCore_10Units("Check procedure should not allow to under-commit transfer line.", shouldFail: true, pickLineQuantities: new decimal[] { 3m, 4m });
		}

		#endregion

		#region TestCheckProcedure_InternalTransfer_CorrectCommit

		public void TestCheckProcedure_InternalTransfer_CorrectCommit()
		{
			TestCheckProcedure_InternalTransferCore_10Units("Check procedure should not fail correct transfer line.", shouldFail: false, pickLineQuantities: new decimal[] { 10m });
		}

		public void TestCheckProcedure_InternalTransfer_CorrectCommit_MultiplePickLines()
		{
			TestCheckProcedure_InternalTransferCore_10Units("Check procedure should not fail correct transfer line.", shouldFail: false, pickLineQuantities: new decimal[] { 2m, 3m, 5m });
		}

		#endregion

		#region TestCheckProcedure_InternalTransfer_OverCommit

		public void TestCheckProcedure_InternalTransfer_OverCommit()
		{
			TestCheckProcedure_InternalTransferCore_10Units("Check procedure should not allow to over-commit transfer lines.", shouldFail: true, pickLineQuantities: new decimal[] { 15m });
		}

		public void TestCheckProcedure_InternalTransfer_OverCommit_MultiplePickLines()
		{
			TestCheckProcedure_InternalTransferCore_10Units("Check procedure should not allow to over-commit transfer lines.", shouldFail: true, pickLineQuantities: new decimal[] { 6m, 8m });
		}

		#endregion

		#region TestCheckProcedure_InternalTransferCore_10Units

		void TestCheckProcedure_InternalTransferCore_10Units(string errorMessage, bool shouldFail, params decimal[] pickLineQuantities)
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 2);
			var sql = data.Sql;

			// receive stock
			var receive = new WhsDocket(data.ClientPK, data.WhsPK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.ProductPK, 50m, data.LocationPKs[0]) { WE_StockOnHand = 50m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer = new WhsDocket(data.ClientPK, data.WhsPK, "TFR", "TFR", "ENT", "TR1").AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, data.ProductPK, 10m, data.LocationPKs[1]) { WE_WL_TransferFrom = data.LocationPKs[0], WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);
			foreach (var qty in pickLineQuantities)
			{
				new WhsPickLine(receiveLine, transferLine, qty).AppendInsertAndReturnObject(sql);
			}

			SaveBadDataToDB(sql);

			// run check procedure
			if (shouldFail)
			{
				AssertCheckProcedure_Fails(errorMessage, transferLine.PK);
			}
			else
			{
				AssertCheckProcedure_Pass(errorMessage, transferLine.PK);
			}
		}

		#endregion

		#region TestCheckProcedure_UnrelatedLines

		public void TestCheckProcedure_UnrelatedLines()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 2);
			var sql = data.Sql;

			// receive stock
			var receive = new WhsDocket(data.ClientPK, data.WhsPK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.ProductPK, 50m, data.LocationPKs[0]) { WE_StockOnHand = 40m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer = new WhsDocket(data.ClientPK, data.WhsPK, "TFR", "TFR", "ENT", "TR1").AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, data.ProductPK, 10m, data.LocationPKs[1])
			{
				WE_WL_TransferFrom = data.LocationPKs[0],
				WE_StockOnHand = 5m,
				WE_GS_NKPutawayBy = "E",
				WE_PutawayTime = data.Today,
				WE_DocketLineStatus = "FIN",
				WE_FinalisedDate = data.Today
			}.AppendInsertAndReturnObject(sql);
			var transferLine_StatusChanged = new WhsDocketLine(transfer, data.ProductPK, 5m, data.LocationPKs[1])
			{
				WE_WL_TransferFrom = data.LocationPKs[0],
				WE_StockOnHand = 5m,
				WE_GS_NKPutawayBy = "E",
				WE_PutawayTime = null,
				WE_DocketLineStatus = "FIN",
				WE_FinalisedDate = data.Today,
				WE_IsOriginalInventory = false,
				WE_WE_ParentDocketLine = transferLine
			}.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(sql);

			SaveBadDataToDB(sql);

			AssertCheckProcedure_Pass("Receive lines should no be checked, therefore no error generated.", receiveLine.PK);
			AssertCheckProcedure_Pass("Status changed lines should no be checked, therefore no error generated.", transferLine_StatusChanged.PK);
			AssertCheckProcedure_Pass("Check procedure should work if keys passed are not from WhsDocketLine.", data.WhsPK);
		}

		#endregion

		#endregion

		#region TestCheckProcedure_InterWhs_MasterSource

		#region TestCheckProcedure_InterWhs_MasterSource_UnderCommit

		public void TestCheckProcedure_InterWhs_MasterSource_UnderCommit_NoPickLines()
		{
			TestCheckProcedure_InterWhs_MasterSourceCore_10Units("Check procedure should not allow to under-commit transfer line.", shouldFail: true, pickLineQuantities: Array.Empty<decimal>());
		}

		public void TestCheckProcedure_InterWhs_MasterSource_UnderCommit()
		{
			TestCheckProcedure_InterWhs_MasterSourceCore_10Units("Check procedure should not allow to under-commit transfer line.", shouldFail: true, pickLineQuantities: new decimal[] { 5m });
		}

		public void TestCheckProcedure_InterWhs_MasterSource_UnderCommit_MultiplePickLines()
		{
			TestCheckProcedure_InterWhs_MasterSourceCore_10Units("Check procedure should not allow to under-commit transfer line.", shouldFail: true, pickLineQuantities: new decimal[] { 3m, 4m });
		}

		#endregion

		#region TestCheckProcedure_InterWhs_MasterSource_CorrectCommit

		public void TestCheckProcedure_InterWhs_MasterSource_CorrectCommit()
		{
			TestCheckProcedure_InterWhs_MasterSourceCore_10Units("Check procedure should not fail correct transfer line.", shouldFail: false, pickLineQuantities: new decimal[] { 10m });
		}

		public void TestCheckProcedure_InterWhs_MasterSource_CorrectCommit_MultiplePickLines()
		{
			TestCheckProcedure_InterWhs_MasterSourceCore_10Units("Check procedure should not fail correct transfer line.", shouldFail: false, pickLineQuantities: new decimal[] { 2m, 3m, 5m });
		}

		#endregion

		#region TestCheckProcedure_InterWhs_MasterSource_OverCommit

		public void TestCheckProcedure_InterWhs_MasterSource_OverCommit()
		{
			TestCheckProcedure_InterWhs_MasterSourceCore_10Units("Check procedure should not allow to over-commit transfer lines.", shouldFail: true, pickLineQuantities: new decimal[] { 15m });
		}

		public void TestCheckProcedure_InterWhs_MasterSource_OverCommit_MultiplePickLines()
		{
			TestCheckProcedure_InterWhs_MasterSourceCore_10Units("Check procedure should not allow to over-commit transfer lines.", shouldFail: true, pickLineQuantities: new decimal[] { 6m, 8m });
		}

		#endregion

		#region TestCheckProcedure_InterWhs_MasterSourceCore_10Units

		void TestCheckProcedure_InterWhs_MasterSourceCore_10Units(string errorMessage, bool shouldFail, params decimal[] pickLineQuantities)
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.ClientPK, data.WhsPK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.ProductPK, 50m, data.LocationPKs[0]) { WE_StockOnHand = 50m - pickLineQuantities.Sum(qty => qty), WE_DocketLineStatus = "FIN", WE_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer_Master = new WhsDocket(data.ClientPK, data.WhsPK, "TFR", "IWS", "FIN", "TR1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Master = new WhsDocketLine(transfer_Master, data.ProductPK, 10m, location2.PK)
			{
				WE_WL_TransferFrom = data.LocationPKs[0],
				WE_GS_NKPutawayBy = "E",
				WE_PutawayTime = data.Today,
				WE_DocketLineStatus = "FIN",
				WE_FinalisedDate = data.Today
			}.AppendInsertAndReturnObject(sql);
			foreach (var qty in pickLineQuantities)
			{
				new WhsPickLine(receiveLine, transferLine_Master, qty) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(sql);
			}

			var transfer_Child = new WhsDocket(data.ClientPK, whs2.PK, "TFR", "IWD", "FIN", "TR2") { WD_FinalisedDate = data.Today, WD_WD_ParentDocket = transfer_Master }.AppendInsertAndReturnObject(sql);
			var transferLine_Child = new WhsDocketLine(transfer_Child, data.ProductPK, 10m, location2.PK)
			{
				WE_WL_TransferFrom = data.LocationPKs[0],
				WE_StockOnHand = 10m,
				WE_GS_NKPutawayBy = "E",
				WE_PutawayTime = data.Today,
				WE_DocketLineStatus = "FIN",
				WE_FinalisedDate = data.Today,
				WE_WE_ParentDocketLine = transferLine_Master
			}.AppendInsertAndReturnObject(sql);

			SaveBadDataToDB(sql);

			// run check procedure
			AssertCheckProcedure_Pass("Child transfer lines should not be checked, therefore no error generated.", transferLine_Child.PK);
			if (shouldFail)
			{
				AssertCheckProcedure_Fails(errorMessage, transferLine_Master.PK);
			}
			else
			{
				AssertCheckProcedure_Pass(errorMessage, transferLine_Master.PK);
			}
		}

		#endregion

		#endregion

		#region TestCheckProcedure_InterWhs_MasterDest

		#region TestCheckProcedure_InterWhs_MasterDest_UnderCommit

		public void TestCheckProcedure_InterWhs_MasterDest_UnderCommit_NoPickLines()
		{
			TestCheckProcedure_InterWhs_MasterDestCore_10Units("Check procedure should not allow to under-commit transfer line.", shouldFail: true, pickLineQuantities: Array.Empty<decimal>());
		}

		public void TestCheckProcedure_InterWhs_MasterDest_UnderCommit()
		{
			TestCheckProcedure_InterWhs_MasterDestCore_10Units("Check procedure should not allow to under-commit transfer line.", shouldFail: true, pickLineQuantities: new decimal[] { 5m });
		}

		public void TestCheckProcedure_InterWhs_MasterDest_UnderCommit_MultiplePickLines()
		{
			TestCheckProcedure_InterWhs_MasterDestCore_10Units("Check procedure should not allow to under-commit transfer line.", shouldFail: true, pickLineQuantities: new decimal[] { 3m, 4m });
		}

		#endregion

		#region TestCheckProcedure_InterWhs_MasterDest_CorrectCommit

		public void TestCheckProcedure_InterWhs_MasterDest_CorrectCommit()
		{
			TestCheckProcedure_InterWhs_MasterDestCore_10Units("Check procedure should not fail correct transfer line.", shouldFail: false, pickLineQuantities: new decimal[] { 10m });
		}

		public void TestCheckProcedure_InterWhs_MasterDest_CorrectCommit_MultiplePickLines()
		{
			TestCheckProcedure_InterWhs_MasterDestCore_10Units("Check procedure should not fail correct transfer line.", shouldFail: false, pickLineQuantities: new decimal[] { 2m, 3m, 5m });
		}

		#endregion

		#region TestCheckProcedure_InterWhs_MasterDest_OverCommit

		public void TestCheckProcedure_InterWhs_MasterDest_OverCommit()
		{
			TestCheckProcedure_InterWhs_MasterDestCore_10Units("Check procedure should not allow to over-commit transfer lines.", shouldFail: true, pickLineQuantities: new decimal[] { 15m });
		}

		public void TestCheckProcedure_InterWhs_MasterDest_OverCommit_MultiplePickLines()
		{
			TestCheckProcedure_InterWhs_MasterDestCore_10Units("Check procedure should not allow to over-commit transfer lines.", shouldFail: true, pickLineQuantities: new decimal[] { 6m, 8m });
		}

		#endregion

		#region TestCheckProcedure_InterWhs_MasterDestCore

		void TestCheckProcedure_InterWhs_MasterDestCore_10Units(string errorMessage, bool shouldFail, params decimal[] pickLineQuantities)
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.ClientPK, data.WhsPK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.ProductPK, 50m, data.LocationPKs[0]) { WE_StockOnHand = 50m - pickLineQuantities.Sum(qty => qty), WE_DocketLineStatus = "FIN", WE_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer_Master = new WhsDocket(data.ClientPK, whs2.PK, "TFR", "IWD", "FIN", "TR1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Master = new WhsDocketLine(transfer_Master, data.ProductPK, 10m, location2.PK)
			{
				WE_WL_TransferFrom = data.LocationPKs[0],
				WE_StockOnHand = 10m,
				WE_GS_NKPutawayBy = "E",
				WE_PutawayTime = data.Today,
				WE_DocketLineStatus = "FIN",
				WE_FinalisedDate = data.Today
			}.AppendInsertAndReturnObject(sql);
			foreach (var qty in pickLineQuantities)
			{
				new WhsPickLine(receiveLine, transferLine_Master, qty) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(sql);
			}

			var transfer_Child = new WhsDocket(data.ClientPK, data.WhsPK, "TFR", "IWS", "FIN", "TR2") { WD_FinalisedDate = data.Today, WD_WD_ParentDocket = transfer_Master }.AppendInsertAndReturnObject(sql);
			var transferLine_Child = new WhsDocketLine(transfer_Child, data.ProductPK, 10m, location2.PK)
			{
				WE_WL_TransferFrom = data.LocationPKs[0],
				WE_GS_NKPutawayBy = "E",
				WE_PutawayTime = data.Today,
				WE_DocketLineStatus = "FIN",
				WE_FinalisedDate = data.Today,
				WE_WE_ParentDocketLine = transferLine_Master
			}.AppendInsertAndReturnObject(sql);

			SaveBadDataToDB(sql);

			// run check procedure
			AssertCheckProcedure_Pass("Child transfer lines should not be checked, therefore no error generated.", transferLine_Child.PK);
			if (shouldFail)
			{
				AssertCheckProcedure_Fails(errorMessage, transferLine_Master.PK);
			}
			else
			{
				AssertCheckProcedure_Pass(errorMessage, transferLine_Master.PK);
			}
		}

		#endregion

		#endregion

		#region TestCheckProcedure_AdjustmentOut 

		#region TestCheckProcedure_AdjustmentOut_IgnoresAdjustmentIn

		public void TestCheckProcedure_AdjustmentOut_IgnoresAdjustmentIn()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;

			// adjustment in
			var adjustment = new WhsDocket(data.ClientPK, data.WhsPK, "ADJ", "NEA", "FIN", "A1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var adjustmentLine = new WhsDocketLine(adjustment, data.ProductPK, 20m, data.LocationPKs[0]) { WE_StockOnHand = 20m }.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			// run check procedure
			AssertCheckProcedure_Pass("Check procedure should ignore adjustment in lines.", adjustmentLine.PK);
		}

		#endregion

		#region TestCheckProcedure_AdjustmentOut_Under

		public void TestCheckProcedure_AdjustmentOut_UnderPicked_NoPickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "", picked: Array.Empty<decimal>(), shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_UnderPicked_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "", picked: new[] { 15m }, shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_UnderPicked_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "", picked: new[] { 5m, 8m, 2m }, shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_UnderCommit_NoPickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "", picked: Array.Empty<decimal>(), shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_UnderCommit_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "", picked: new[] { 15m }, shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_UnderCommit_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "", picked: new[] { 5m, 7m, 4m }, shouldFail: true);
		}

		#endregion

		#region TestCheckProcedure_AdjustmentOut_Over

		public void TestCheckProcedure_AdjustmentOut_OverPicked_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "", picked: new[] { 25m }, shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_OverPicked_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "", picked: new[] { 10m, 15m }, shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_OverCommitted_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "", picked: new[] { 28m }, shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_OverCommitted_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "", picked: new[] { 12m, 17m }, shouldFail: true);
		}

		#endregion

		#region TestCheckProcedure_AdjustmentOut_Correct

		public void TestCheckProcedure_AdjustmentOut_Correct_Finalised_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "", picked: new[] { 20m }, shouldFail: false);
		}

		public void TestCheckProcedure_AdjustmentOut_Correct_Finalised_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "", picked: new[] { 5m, 15m }, shouldFail: false);
		}

		public void TestCheckProcedure_AdjustmentOut_Correct_Unfinalised_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "", picked: new[] { 20m }, shouldFail: false);
		}

		public void TestCheckProcedure_AdjustmentOut_Correct_Unfinalised_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "", picked: new[] { 10m, 10m }, shouldFail: false);
		}

		#endregion

		#region TestCheckProcedure_AdjustmentOut_Stocktake

		#region TestCheckProcedure_AdjustmentOut_Stocktake_Under

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Under_Finalised_NoPickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "STOCKTAKE", picked: Array.Empty<decimal>(), shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Under_Finalised_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "STOCKTAKE", picked: new[] { 10m }, shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Under_Finalised_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "STOCKTAKE", picked: new[] { 10m, 5m }, shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Under_Unfinalised_NoPickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "STOCKTAKE", picked: Array.Empty<decimal>(), shouldFail: false);
		}

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Under_Unfinalised_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "STOCKTAKE", picked: new[] { 3m }, shouldFail: false);
		}

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Under_Unfinalised_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "STOCKTAKE", picked: new[] { 1m, 5m }, shouldFail: false);
		}

		#endregion

		#region TestCheckProcedure_AdjustmentOut_Stocktake_Over

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Over_Finalised_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "STOCKTAKE", picked: new[] { 30m }, shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Over_Finalised_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "STOCKTAKE", picked: new[] { 10m, 15m }, shouldFail: true);
		}

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Over_Unfinalised_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "STOCKTAKE", picked: new[] { 30m }, shouldFail: false);
		}

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Over_Unfinalised_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "STOCKTAKE", picked: new[] { 10m, 15m }, shouldFail: false);
		}

		#endregion

		#region TestCheckProcedure_AdjustmentOut_Stocktake_Correct

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Correct_Finalised_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "STOCKTAKE", picked: new[] { 20m }, shouldFail: false);
		}

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Correct_Finalised_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "FIN", externalRef: "STOCKTAKE", picked: new[] { 5m, 15m }, shouldFail: false);
		}

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Correct_Unfinalised_SinglePickLine()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "STOCKTAKE", picked: new[] { 20m }, shouldFail: false);
		}

		public void TestCheckProcedure_AdjustmentOut_Stocktake_Correct_Unfinalised_MultiplePickLines()
		{
			TestCheckProcedure_Receive50AndAdjustOut20Units_Core(adjustmentStatus: "ENT", externalRef: "STOCKTAKE", picked: new[] { 10m, 10m }, shouldFail: false);
		}

		#endregion

		#endregion

		#region TestCheckProcedure_ReceiveAndAdjustmentLine20Units_Core

		void TestCheckProcedure_Receive50AndAdjustOut20Units_Core(string adjustmentStatus, string externalRef, decimal[] picked, bool shouldFail)
		{
			var data = new SimpleDataEnvironment(1);
			var sql = data.Sql;

			// receive stock
			var receive = new WhsDocket(data.ClientPK, data.WhsPK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.ProductPK, 50m, data.LocationPKs[0]) { WE_StockOnHand = adjustmentStatus == "FIN" ? 50m - picked.Sum() : 50m }.AppendInsertAndReturnObject(sql);

			// adjustment out
			var finalisedDate = adjustmentStatus == "FIN" ? data.Today : (DateTime?)null;
			var adjustment = new WhsDocket(data.ClientPK, data.WhsPK, "ADJ", "NEA", adjustmentStatus, "A1", $"{externalRef} 123456") { WD_FinalisedDate = finalisedDate }.AppendInsertAndReturnObject(sql);
			var adjustmentLine = new WhsDocketLine(adjustment, data.ProductPK, -20, data.LocationPKs[0]).AppendInsertAndReturnObject(sql);

			// picklines
			foreach (var qty in picked)
			{
				new WhsPickLine(receiveLine, adjustmentLine, qty) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = finalisedDate }.AppendInsertAndReturnObject(sql);
			}

			SaveBadDataToDB(sql);

			// run check procedure
			if (shouldFail)
			{
				AssertCheckProcedure_Fails($"Expected trigger to fail for stocktake adjustment lines with status = '{adjustmentStatus}'", adjustmentLine.PK);
			}
			else
			{
				AssertCheckProcedure_Pass($"Unexpected trigger failure for stocktake adjustment lines with status = '{adjustmentStatus}'", adjustmentLine.PK);
			}
		}

		#endregion

		#endregion

		#region TestCheckProcedure_OrdersAndWorkOrders

		#region TestCheckProcedure_OrdersAndWorkOrders_ExpectIgnoresUnpicked

		public void TestCheckProcedure_OrdersAndWorkOrders_ExpectIgnoresUnpicked_Order()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;

			// Create unpicked order
			var order = new WhsDocket(data.ClientPK, data.WhsPK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, data.ProductPK, 30m).AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			// run check procedure
			AssertCheckProcedure_Pass("Check procedure should ignore unpicked order lines.", orderLine.PK);
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_ExpectIgnoresUnpicked_WorkOrder()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;

			// Create unpicked work order
			var workOrder = new WhsDocket(data.ClientPK, data.WhsPK, "WOR", "ASS", "ENT", "WO1").AppendInsertAndReturnObject(sql);
			var workOrderMainLine = new WhsDocketLine(workOrder, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
			var workOrderComponentLine = new WhsDocketLine(workOrder, data.ComponentPK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			// run check procedure
			AssertCheckProcedure_Pass("Check procedure should ignore upicked work order lines.", workOrderComponentLine.PK);
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_ExpectIgnoresUnpicked_DynamicWorkOrder()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;

			// Create unpicked work order
			var workOrder = new WhsDocket(data.ClientPK, data.WhsPK, "DWO", "ASS", "ENT", "WO1").AppendInsertAndReturnObject(sql);
			var workOrderMainLine = new WhsDocketLine(workOrder, data.ProductPK, 10m).AppendInsertAndReturnObject(sql);
			var workOrderComponentLine = new WhsDocketLine(workOrder, data.ComponentPK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			// run check procedure
			AssertCheckProcedure_Pass("Check procedure should ignore upicked work order lines.", workOrderComponentLine.PK);
		}

		#endregion

		#region TestCheckProcedure_OrdersAndWorkOrders_Under

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderPicked_Order_SinglePickLine()
		{
			TestCheckProcedure_OrderCore_30UnitsPicked("Check procedure should not fail underpicked order line.", shouldFail: false, pickLineQuantities: new decimal[] { 10m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderPicked_Order_MultiplePickLines()
		{
			TestCheckProcedure_OrderCore_30UnitsPicked("Check procedure should not fail underpicked order line.", shouldFail: false, pickLineQuantities: new decimal[] { 10m, 5m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderReserved_Order_SinglePickLine()
		{
			TestCheckProcedure_OrderCore_30UnitsReserved("Check procedure should not fail underpicked order line.", shouldFail: false, pickLineQuantities: new decimal[] { 10m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderReserved_Order_MultiplePickLines()
		{
			TestCheckProcedure_OrderCore_30UnitsReserved("Check procedure should not fail underpicked order line.", shouldFail: false, pickLineQuantities: new decimal[] { 10m, 5m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderPicked_WorkOrder_SinglePickLine()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsPicked("Check procedure should not fail underpicked work order line.", "WOR", shouldFail: false, pickLineQuantities: new decimal[] { 10m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderPicked_WorkOrder_MultiplePickLines()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsPicked("Check procedure should not fail underpicked work order line.", "WOR", shouldFail: false, pickLineQuantities: new decimal[] { 10m, 5m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderPicked_DynamicWorkOrder_SinglePickLine()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsPicked("Check procedure should not fail underpicked work order line.", "DWO", shouldFail: false, pickLineQuantities: new decimal[] { 10m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderPicked_DynamicWorkOrder_MultiplePickLines()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsPicked("Check procedure should not fail underpicked work order line.", "DWO", shouldFail: false, pickLineQuantities: new decimal[] { 10m, 5m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderReserved_WorkOrder_SinglePickLine()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsReserved("Check procedure should not fail underpicked work order line.", "WOR", shouldFail: false, pickLineQuantities: new decimal[] { 10m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderReserved_WorkOrder_MultiplePickLines()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsReserved("Check procedure should not fail underpicked work order line.", "WOR", shouldFail: false, pickLineQuantities: new decimal[] { 10m, 5m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderReserved_DynamicWorkOrder_SinglePickLine()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsReserved("Check procedure should not fail underpicked work order line.", "DWO", shouldFail: false, pickLineQuantities: new decimal[] { 10m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_UnderReserved_DynamicWorkOrder_MultiplePickLines()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsReserved("Check procedure should not fail underpicked work order line.", "DWO", shouldFail: false, pickLineQuantities: new decimal[] { 10m, 5m });
		}

		#endregion

		#region TestCheckProcedure_OrdersAndWorkOrders_Correct

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_Finalised_Order_SinglePickLine()
		{
			TestCheckProcedure_OrderCore_30UnitsPicked("Check procedure should not fail correct order line.", shouldFail: false, pickLineQuantities: new decimal[] { 30m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_Finalised_Order_MultiplePickLines()
		{
			TestCheckProcedure_OrderCore_30UnitsPicked("Check procedure should not fail correct order line.", shouldFail: false, pickLineQuantities: new decimal[] { 10m, 20m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_Reserved_Order_SinglePickLine()
		{
			TestCheckProcedure_OrderCore_30UnitsReserved("Check procedure should not fail correct order line.", shouldFail: false, pickLineQuantities: new decimal[] { 30m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_Reserved_Order_MultiplePickLines()
		{
			TestCheckProcedure_OrderCore_30UnitsReserved("Check procedure should not fail correct order line.", shouldFail: false, pickLineQuantities: new decimal[] { 10m, 20m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_WorkOrder_Finalised_Order_SinglePickLine()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsPicked("Check procedure should not fail correct work order line.", "WOR", shouldFail: false, pickLineQuantities: new decimal[] { 30m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_Finalised_WorkOrder_MultiplePickLines()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsPicked("Check procedure should not fail correct work order line.", "WOR", shouldFail: false, pickLineQuantities: new decimal[] { 10m, 20m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_DynamicWorkOrder_Finalised_Order_SinglePickLine()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsPicked("Check procedure should not fail correct work order line.", "DWO", shouldFail: false, pickLineQuantities: new decimal[] { 30m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_Finalised_DynamicWorkOrder_MultiplePickLines()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsPicked("Check procedure should not fail correct work order line.", "DWO", shouldFail: false, pickLineQuantities: new decimal[] { 10m, 20m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_Reserved_WorkOrder_SinglePickLine()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsReserved("Check procedure should not fail correct work order line.", "WOR", shouldFail: false, pickLineQuantities: new decimal[] { 30m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_Reserved_WorkOrder_MultiplePickLines()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsReserved("Check procedure should not fail correct work order line.", "WOR", shouldFail: false, pickLineQuantities: new decimal[] { 20m, 10m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_Reserved_DynamicWorkOrder_SinglePickLine()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsReserved("Check procedure should not fail correct work order line.", "DWO", shouldFail: false, pickLineQuantities: new decimal[] { 30m });
		}

		public void TestCheckProcedure_OrdersAndWorkOrders_Correct_Reserved_DynamicWorkOrder_MultiplePickLines()
		{
			TestCheckProcedure_WorkOrderCore_30UnitsReserved("Check procedure should not fail correct work order line.", "DWO", shouldFail: false, pickLineQuantities: new decimal[] { 20m, 10m });
		}

		#endregion

		#region TestCheckProcedure_OrderCore_30Units

		void TestCheckProcedure_OrderCore_30UnitsPicked(string errorMessage, bool shouldFail, params decimal[] pickLineQuantities)
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;

			// receive stock
			var stockOnHand = 100 - pickLineQuantities.Sum(d => d);
			var receive = new WhsDocket(data.ClientPK, data.WhsPK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.ProductPK, 100m, data.LocationPKs[0]) { WE_StockOnHand = stockOnHand }.AppendInsertAndReturnObject(sql);

			// order
			var pick = new WhsPick(data.Whs, "P1", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(data.ClientPK, data.WhsPK, "ORD", "ORD", "DEP", "O1") { WD_WP = pick.PK, WD_FinalisedDate = data.Today, WD_GS_NKFinalizedBy = "E" }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, data.ProductPK, 30m) { WE_FinalisedDate = data.Today, WE_DocketLineStatus = "DEP" }.AppendInsertAndReturnObject(sql);

			// picklines
			foreach (var qty in pickLineQuantities)
			{
				new WhsPickLine(receiveLine, orderLine, qty) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(sql);
			}

			SaveBadDataToDB(sql);

			// run check procedure
			if (shouldFail)
			{
				AssertCheckProcedure_Fails(errorMessage, orderLine.PK);
			}
			else
			{
				AssertCheckProcedure_Pass(errorMessage, orderLine.PK);
			}
		}

		void TestCheckProcedure_OrderCore_30UnitsReserved(string errorMessage, bool shouldFail, params decimal[] pickLineQuantities)
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;

			// receive stock
			var receive = new WhsDocket(data.ClientPK, data.WhsPK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveList = new List<WhsDocketLine>();
			foreach (var qty in pickLineQuantities)
			{
				receiveList.Add(new WhsDocketLine(receive, data.ProductPK, 100m, data.LocationPKs[0]) { WE_StockOnHand = 100m }.AppendInsertAndReturnObject(sql));
			}

			// order
			var order = new WhsDocket(data.ClientPK, data.WhsPK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, data.ProductPK, 30m).AppendInsertAndReturnObject(sql);

			// picklines
			for (var index = 0; index < pickLineQuantities.Length; index++)
			{
				new WhsPickLine(receiveList[index], orderLine, pickLineQuantities[index]) { WZ_OriginalReservedQty = pickLineQuantities[index] }.AppendInsertAndReturnObject(sql);
			}

			SaveBadDataToDB(sql);

			// run check procedure
			if (shouldFail)
			{
				AssertCheckProcedure_Fails(errorMessage, orderLine.PK);
			}
			else
			{
				AssertCheckProcedure_Pass(errorMessage, orderLine.PK);
			}
		}

		#endregion

		#region TestCheckProcedure_WorkOrderCore_30Units

		void TestCheckProcedure_WorkOrderCore_30UnitsPicked(string errorMessage, string docketType, bool shouldFail, params decimal[] pickLineQuantities)
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;

			// receive stock
			var stockOnHand = 100 - pickLineQuantities.Sum(d => d);
			var receive = new WhsDocket(data.ClientPK, data.WhsPK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.ComponentPK, 100m, data.LocationPKs[0]) { WE_StockOnHand = stockOnHand }.AppendInsertAndReturnObject(sql);

			// work order
			var pick = new WhsPick(data.Whs, "P1", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var workOrder = new WhsDocket(data.ClientPK, data.WhsPK, docketType, "ASS", "FIN", "WO1") { WD_WP = pick.PK, WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var workOrderMainLine = new WhsDocketLine(workOrder, data.ProductPK, 30m).AppendInsertAndReturnObject(sql);
			var workOrderComponentLine = new WhsDocketLine(workOrder, data.ComponentPK, 30m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);

			// picklines
			foreach (var qty in pickLineQuantities)
			{
				new WhsPickLine(receiveLine, workOrderComponentLine, qty) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(sql);
			}

			SaveBadDataToDB(sql);

			// run check procedure
			if (shouldFail)
			{
				AssertCheckProcedure_Fails(errorMessage, workOrderComponentLine.PK);
			}
			else
			{
				AssertCheckProcedure_Pass(errorMessage, workOrderComponentLine.PK);
			}
		}

		void TestCheckProcedure_WorkOrderCore_30UnitsReserved(string errorMessage, string docketType, bool shouldFail, params decimal[] pickLineQuantities)
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;

			// receive stock
			var receive = new WhsDocket(data.ClientPK, data.WhsPK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveList = new List<WhsDocketLine>();
			foreach (var qty in pickLineQuantities)
			{
				receiveList.Add(new WhsDocketLine(receive, data.ComponentPK, 100m, data.LocationPKs[0]) { WE_StockOnHand = 100m }.AppendInsertAndReturnObject(sql));
			}

			// work order
			var workOrder = new WhsDocket(data.ClientPK, data.WhsPK, docketType, "ASS", "ENT", "WO1").AppendInsertAndReturnObject(sql);
			var workOrderMainLine = new WhsDocketLine(workOrder, data.ProductPK, 30m).AppendInsertAndReturnObject(sql);
			var workOrderComponentLine = new WhsDocketLine(workOrder, data.ComponentPK, 30m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);

			// picklines
			var index = 0;
			foreach (WhsDocketLine receiveLine in receiveList)
			{
				new WhsPickLine(receiveLine, workOrderComponentLine, pickLineQuantities[index]) { WZ_OriginalReservedQty = pickLineQuantities[index] }.AppendInsertAndReturnObject(sql);
				index++;
			}

			SaveBadDataToDB(sql);

			// run check procedure
			if (shouldFail)
			{
				AssertCheckProcedure_Fails(errorMessage, workOrderComponentLine.PK);
			}
			else
			{
				AssertCheckProcedure_Pass(errorMessage, workOrderComponentLine.PK);
			}
		}

		#endregion

		#endregion

		#region SaveBadDataToDB

		static void SaveBadDataToDB(SqlQueryBuilder sql)
		{
			// defer triggers to run in the end
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			// suspend triggers to save bad data into DB
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion

		#region Asserts

		void AssertCheckProcedure_Pass(string errorDescription, Guid docketLinePK)
		{
			var sql = GetCheckProcedureSql(docketLinePK);
			AssertNoExceptionThrown(errorDescription, () => Db.Connection.ExecuteNonQuery(sql));
		}

		void AssertCheckProcedure_Fails(string errorDescription, Guid docketLinePK)
		{
			var sql = GetCheckProcedureSql(docketLinePK);
			AssertExceptionThrown(errorDescription, typeof(SqlException), ExpectedErrorMessageFromCheckProcedure, () => Db.Connection.ExecuteNonQuery(sql), assertStartsWith: true);
		}

		string GetCheckProcedureSql(Guid docketLinePK)
		{
			return $@"
DECLARE @LinePKs dbo.TVP_uniqueidentifier;
INSERT INTO @LinePKs
SELECT 
	WE_PK 
FROM 
	dbo.WhsDocketLine
where
	WE_PK = '{docketLinePK}'

EXEC {TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert} @LinePKs;";
		}

		#endregion

		#region SimpleDataEnvironment

		class SimpleDataEnvironment
		{
			public SimpleDataEnvironment(int numberOfLocations)
			{
				Sql = new SqlQueryBuilder();
				Whs = new WhsWarehouse("WH1").WithDockDoor(Db.Connection);
				WhsPK = Whs.PK;

				var area = new WhsArea(Whs.PK, "AREA1").AppendInsertAndReturnObject(Sql);
				var row = new WhsRow(Whs, "A") { WR_Columns = (short)numberOfLocations }.AppendInsertAndReturnObject(Sql);
				LocationPKs = new Guid[numberOfLocations];
				for (short i = 1; i <= numberOfLocations; i++)
				{
					var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = i }.AppendInsertAndReturnObject(Sql);
					LocationPKs[i - 1] = location.PK;
				}

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(Sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(Sql);
				var component = new OrgSupplierPart("P2").AppendInsertAndReturnObject(Sql);
				new OrgPartBOM(product, component) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(Sql);
				ClientPK = client.PK;
				ProductPK = product.PK;
				ComponentPK = component.PK;
				Today = DateTime.Today;
			}

			public readonly SqlQueryBuilder Sql;
			public readonly Guid WhsPK;
			public readonly WhsWarehouse Whs;
			public readonly Guid[] LocationPKs;
			public readonly Guid ClientPK;
			public readonly Guid ProductPK;
			public readonly Guid ComponentPK;
			public readonly DateTime Today;
		}

		#endregion
	}
}
