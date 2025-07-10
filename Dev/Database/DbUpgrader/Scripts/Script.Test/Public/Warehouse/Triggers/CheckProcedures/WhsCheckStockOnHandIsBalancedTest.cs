using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(WhsCheckStockOnHandIsBalanced))]
	class WhsCheckStockOnHandIsBalancedTest : WhsCheckStockOnHandIsBalancedTestCase
	{
		protected override string ProcedureName => TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced;
	}

	abstract class WhsCheckStockOnHandIsBalancedTestCase : DbCreateScriptTest
	{
		const string ExpectedErrorMessageFromCheckProcedure = "TriggerLikelyConcurrencyError: Attempt to put Stock On Hand out of balance.";

		#region TestCheckProcedure_Receive_Cancelled

		public void TestCheckProcedure_Receive_Cancelled()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "CAN", "R1") { WD_GS_NKCanceledBy = "~BP", WD_CanceledTimeUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK)
			{
				WE_StockOnHand = 0m, // non-zero cases are prevented by Constraint_CancelledDocketLineStockOnHandIsZero
				WE_DocketLineStatus = "CAN",
				WE_OriginalInventoryStatus = "PUT",
				WE_CurrentInventoryStatus = "PUT",
				WE_UnloadedTime = DateTimeOffset.Now,
				WE_GS_NKUnloadedBy = "A"
			}.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Cancelled receive lines should have SOH = 0.", receiveLine);
		}

		#endregion

		#region TestCheckProcedure_Receive_Unloading

		public void TestCheckProcedure_Receive_Unloading()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(data.Sql);
			var receiveLine1 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK)
			{
				WE_StockOnHand = 50m,
				WE_OriginalInventoryStatus = "PUT",
				WE_CurrentInventoryStatus = "PUT",
				WE_UnloadedTime = DateTimeOffset.Now,
				WE_GS_NKUnloadedBy = "A"
			}.AppendInsertAndReturnObject(data.Sql);
			var receiveLine2 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK)
			{
				WE_StockOnHand = 50m,
				WE_OriginalInventoryStatus = "PUT",
				WE_CurrentInventoryStatus = "PUT",
				WE_UnloadedTime = DateTimeOffset.Now,
				WE_GS_NKUnloadedBy = "A"
			}.AppendInsertAndReturnObject(data.Sql);

			// reserve some stock on Receive Line 3, should change nothing.
			var order = new WhsDocket(data.Client.PK, data.Whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(data.Sql);
			var orderLine = new WhsDocketLine(order, data.Product.PK, 10m).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine2, orderLine, 10m) { WZ_OriginalReservedQty = 10m }.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Pre-unload and unloading receive lines should have Transaction Quantity = SOH.", receiveLine1);
			AssertCheckProcedure_Pass("Pre-unload and unloading receive lines should have Transaction Quantity = SOH.", receiveLine2);
		}

		#endregion

		#region TestCheckProcedure_Receive_PickedForUnload

		public void TestCheckProcedure_Receive_PickedForUnload()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 2);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(data.Sql);
			var receiveLine1 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK)
			{
				WE_StockOnHand = 0m, // SOH > 0 handled by WhsDocketLine.Constraint_PickedStockShouldHaveNoStockOnHand 
				WE_OriginalInventoryStatus = "REC",
				WE_CurrentInventoryStatus = "REC",
				WE_DocketLineStatus = "PFU",
				WE_UnloadedTime = DateTimeOffset.Now,
				WE_GS_NKUnloadedBy = "A"
			}.AppendInsertAndReturnObject(data.Sql);
			var receiveLine2 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "REC",
				WE_CurrentInventoryStatus = "REC",
				WE_DocketLineStatus = "PFU",
				WE_UnloadedTime = DateTimeOffset.Now,
				WE_GS_NKUnloadedBy = "A"
			}.AppendInsertAndReturnObject(data.Sql);

			var putawayTransfer = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "TFR", "HFT", "T1") { WD_IsPutawayTransfer = true }.AppendInsertAndReturnObject(data.Sql);
			var putawayTransferLine = new WhsDocketLine(putawayTransfer, data.Product.PK, 50m, data.Locations[1].PK)
			{
				WE_StockOnHand = 50m,
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_OriginalInventoryStatus = "PTA",
				WE_CurrentInventoryStatus = "PTA",
				WE_AdjustmentArrivalDate = data.Today
			}.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine1, putawayTransferLine, 50m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Picked for unload receive lines shoud have SOH = 0.", receiveLine1);
			AssertCheckProcedure_Fails("Picked for unload receive lines shoud have SOH = 0, but must have Putaway Transfer.", receiveLine2);
		}

		#endregion

		#region TestCheckProcedure_Receive_Finalised

		public void TestCheckProcedure_Receive_Finalised()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine1 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine2 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 40m }.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised receive lines should have Transaction Quantity = SOH + HCC + Picked Qty.", receiveLine1);
			AssertCheckProcedure_Fails("Finalised receive lines should have Transaction Quantity = SOH + HCC + Picked Qty.", receiveLine2);
		}

		public void TestCheckProcedure_Receive_Finalised_WithPickLines()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine1 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 35m }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine2 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 30m }.AppendInsertAndReturnObject(data.Sql);

			var pick = new WhsPick(data.Whs, "P1", "PIS").AppendInsertAndReturnObject(data.Sql);
			var order = new WhsDocket(data.Client.PK, data.Whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(data.Sql);
			var orderLine1 = new WhsDocketLine(order, data.Product.PK, 20m).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine1, orderLine1, 8m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine1, orderLine1, 7m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today.AddDays(-5) }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine1, orderLine1, 5m).AppendInsertAndReturnObject(data.Sql);

			var orderLine2 = new WhsDocketLine(order, data.Product.PK, 20m).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine2, orderLine2, 8m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine2, orderLine2, 7m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today.AddDays(-5) }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine2, orderLine2, 5m).AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised receive lines should have Transaction Quantity = SOH + HCC + Picked Qty.", receiveLine1);
			AssertCheckProcedure_Fails("Finalised receive lines should have Transaction Quantity = SOH + HCC + Picked Qty.", receiveLine2);
		}

		public void TestCheckProcedure_Receive_Finalised_WithHoldCodeChanges()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine1 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 15m }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine1_HCC1 = new WhsDocketLine(receive, data.Product.PK, 20m, data.Locations[0].PK)
			{
				WE_StockOnHand = 20m,
				WE_WE_ParentDocketLine = receiveLine1,
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = receiveLine1
			}.AppendInsertAndReturnObject(data.Sql);
			var receiveLine1_HCC2 = new WhsDocketLine(receive, data.Product.PK, 15m, data.Locations[0].PK)
			{
				WE_StockOnHand = 10m,
				WE_WE_ParentDocketLine = receiveLine1,
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = receiveLine1
			}.AppendInsertAndReturnObject(data.Sql);

			var receiveLine2 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 20m }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine2_HCC1 = new WhsDocketLine(receive, data.Product.PK, 20m, data.Locations[0].PK)
			{
				WE_StockOnHand = 20m,
				WE_WE_ParentDocketLine = receiveLine2,
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = receiveLine2
			}.AppendInsertAndReturnObject(data.Sql);
			var receiveLine2_HCC2 = new WhsDocketLine(receive, data.Product.PK, 15m, data.Locations[0].PK)
			{
				WE_StockOnHand = 10m,
				WE_WE_ParentDocketLine = receiveLine2,
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = receiveLine2
			}.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised receive lines should have Transaction Quantity = SOH + HCC + Picked Qty.", receiveLine1);
			AssertCheckProcedure_Pass("Finalised receive lines should have Transaction Quantity = SOH + HCC + Picked Qty.", receiveLine1_HCC1);
			AssertCheckProcedure_Fails("Finalised receive lines should have Transaction Quantity = SOH + HCC + Picked Qty.", receiveLine1_HCC2);

			AssertCheckProcedure_Fails("Finalised receive lines should have Transaction Quantity = SOH + HCC + Picked Qty.", receiveLine2);
			AssertCheckProcedure_Pass("Finalised receive lines should have Transaction Quantity = SOH + HCC + Picked Qty.", receiveLine2_HCC1);
			AssertCheckProcedure_Fails("Finalised receive lines should have Transaction Quantity = SOH + HCC + Picked Qty.", receiveLine2_HCC2);
		}

		#endregion

		#region TestCheckProcedure_Adjustment_NotFinalised

		public void TestCheckProcedure_Adjustment_NotFinalised()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(data.Sql);

			var adjustment = new WhsDocket(data.Client.PK, data.Whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(data.Sql);
			var adjustmentLine_In = new WhsDocketLine(adjustment, data.Product.PK, 20m, data.Locations[0].PK) { WE_StockOnHand = 0m }.AppendInsertAndReturnObject(data.Sql); // Constraint_WE_DocketLineStatus checks for non 0 SOH
			var adjustmentLine_Out = new WhsDocketLine(adjustment, data.Product.PK, -20m, data.Locations[0].PK).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, adjustmentLine_Out, 20m).AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Not Finalised Adjustment In lines should should have SOH = 0.", adjustmentLine_In);
			AssertCheckProcedure_Pass("Not Finalised Adjustment Out lines should should have SOH = 0.", adjustmentLine_Out);
		}

		#endregion

		#region TestCheckProcedure_Adjustment_Finalised

		public void TestCheckProcedure_Adjustment_Finalised()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 40m }.AppendInsertAndReturnObject(data.Sql);

			var adjustment = new WhsDocket(data.Client.PK, data.Whs.PK, "ADJ", "NEA", "FIN", "A1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var adjustmentLine_In1 = new WhsDocketLine(adjustment, data.Product.PK, 10m, data.Locations[0].PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(data.Sql);
			var adjustmentLine_In2 = new WhsDocketLine(adjustment, data.Product.PK, 10m, data.Locations[0].PK) { WE_StockOnHand = 0m }.AppendInsertAndReturnObject(data.Sql);
			var adjustmentLine_Out = new WhsDocketLine(adjustment, data.Product.PK, -10m, data.Locations[0].PK).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, adjustmentLine_Out, 10m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Adjustment In lines should have Transaction Quantity = SOH + HCC + Picked Qty.", adjustmentLine_In1);
			AssertCheckProcedure_Fails("Finalised Adjustment In lines should have Transaction Quantity = SOH + HCC + Picked Qty.", adjustmentLine_In2);
			AssertCheckProcedure_Pass("Finalised Adjustment Out lines should have SOH = 0.", adjustmentLine_Out);
		}

		public void TestCheckProcedure_Adjustment_Finalised_WithPickLines()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var adjustment = new WhsDocket(data.Client.PK, data.Whs.PK, "ADJ", "NEA", "FIN", "A1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var adjustmentLine1 = new WhsDocketLine(adjustment, data.Product.PK, 20m, data.Locations[0].PK) { WE_StockOnHand = 5m }.AppendInsertAndReturnObject(data.Sql);
			var adjustmentLine2 = new WhsDocketLine(adjustment, data.Product.PK, 20m, data.Locations[0].PK) { WE_StockOnHand = 7m }.AppendInsertAndReturnObject(data.Sql);

			var pick = new WhsPick(data.Whs, "P1", "PIS").AppendInsertAndReturnObject(data.Sql);
			var order = new WhsDocket(data.Client.PK, data.Whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(data.Sql);
			var orderLine1 = new WhsDocketLine(order, data.Product.PK, 20m).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(adjustmentLine1, orderLine1, 8m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(adjustmentLine1, orderLine1, 7m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today.AddDays(-5) }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(adjustmentLine1, orderLine1, 5m).AppendInsertAndReturnObject(data.Sql);

			var orderLine2 = new WhsDocketLine(order, data.Product.PK, 20m).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(adjustmentLine2, orderLine2, 8m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(adjustmentLine2, orderLine2, 7m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today.AddDays(-5) }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(adjustmentLine2, orderLine2, 5m).AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Adjustment In lines should have Transaction Quantity = SOH + HCC + Picked Qty.", adjustmentLine1);
			AssertCheckProcedure_Fails("Finalised Adjustment In lines should have Transaction Quantity = SOH + HCC + Picked Qty.", adjustmentLine2);
		}

		public void TestCheckProcedure_Adjustment_Finalised_WithHoldCodeChange()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var adjustment = new WhsDocket(data.Client.PK, data.Whs.PK, "ADJ", "NEA", "FIN", "A1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var adjustmentLine1 = new WhsDocketLine(adjustment, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 15m }.AppendInsertAndReturnObject(data.Sql);
			var adjustmentLine1_HCC1 = new WhsDocketLine(adjustment, data.Product.PK, 20m, data.Locations[0].PK)
			{
				WE_StockOnHand = 20m,
				WE_WE_ParentDocketLine = adjustmentLine1,
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = adjustmentLine1
			}.AppendInsertAndReturnObject(data.Sql);
			var adjustmentLine1_HCC2 = new WhsDocketLine(adjustment, data.Product.PK, 15m, data.Locations[0].PK)
			{
				WE_StockOnHand = 10m,
				WE_WE_ParentDocketLine = adjustmentLine1,
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = adjustmentLine1
			}.AppendInsertAndReturnObject(data.Sql);

			var adjustmentLine2 = new WhsDocketLine(adjustment, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(data.Sql);
			var adjustmentLine2_HCC1 = new WhsDocketLine(adjustment, data.Product.PK, 20m, data.Locations[0].PK)
			{
				WE_StockOnHand = 20m,
				WE_WE_ParentDocketLine = adjustmentLine2,
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = adjustmentLine2
			}.AppendInsertAndReturnObject(data.Sql);
			var adjustmentLine2_HCC2 = new WhsDocketLine(adjustment, data.Product.PK, 15m, data.Locations[0].PK)
			{
				WE_StockOnHand = 10m,
				WE_WE_ParentDocketLine = adjustmentLine2,
				WE_IsOriginalInventory = false,
				WE_WE_OriginalDocketLineForRating = adjustmentLine2
			}.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Adjustment In lines should have Transaction Quantity = SOH + HCC + Picked Qty.", adjustmentLine1);
			AssertCheckProcedure_Pass("Finalised Adjustment In lines should have Transaction Quantity = SOH + HCC + Picked Qty.", adjustmentLine1_HCC1);
			AssertCheckProcedure_Fails("Finalised Adjustment In lines should have Transaction Quantity = SOH + HCC + Picked Qty.", adjustmentLine1_HCC2);

			AssertCheckProcedure_Fails("Finalised Adjustment In lines should have Transaction Quantity = SOH + HCC + Picked Qty.", adjustmentLine2);
			AssertCheckProcedure_Pass("Finalised Adjustment In lines should have Transaction Quantity = SOH + HCC + Picked Qty.", adjustmentLine2_HCC1);
			AssertCheckProcedure_Fails("Finalised Adjustment In lines should have Transaction Quantity = SOH + HCC + Picked Qty.", adjustmentLine2_HCC2);
		}

		#endregion

		#region TestCheckProcedure_InternalTransfer_Entered

		public void TestCheckProcedure_InternalTransfer_Entered()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 2);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(data.Sql);

			var transfer = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(data.Sql);
			var transferLine = new WhsDocketLine(transfer, data.Product.PK, 20m, data.Locations[1].PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = data.Locations[0].PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(data.Sql); // Constraint_WE_DocketLineStatus checks for non 0 SOH
			new WhsPickLine(receiveLine, transferLine, 20m).AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Entered Transfer Lines should have SOH = 0.", transferLine);
		}

		#endregion

		#region TestCheckProcedure_InternalTransfer_InTransit

		public void TestCheckProcedure_InternalTransfer_InTransit()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 2);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(data.Sql);

			var transfer = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(data.Sql);
			var transferLine1 = new WhsDocketLine(transfer, data.Product.PK, 20m, data.Locations[1].PK)
			{
				WE_StockOnHand = 20m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_DocketLineStatus = "HFT",
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT"
			}.AppendInsertAndReturnObject(data.Sql);
			var transferLine2 = new WhsDocketLine(transfer, data.Product.PK, 20m, data.Locations[1].PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_DocketLineStatus = "HFT",
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT"
			}.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine1, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine2, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("In-transit Transfer Lines should have Transaction Quantity = SOH.", transferLine1);
			AssertCheckProcedure_Fails("In-transit Transfer Lines should have Transaction Quantity = SOH.", transferLine2);
		}

		#endregion

		#region TestCheckProcedure_InternalTransfer_Putaway

		public void TestCheckProcedure_InternalTransfer_Putaway()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 2);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(data.Sql);
			var receiveLine1 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "REC",
				WE_CurrentInventoryStatus = "REC",
				WE_DocketLineStatus = "PFU",
				WE_UnloadedTime = DateTimeOffset.Now,
				WE_GS_NKUnloadedBy = "A"
			}.AppendInsertAndReturnObject(data.Sql);
			var receiveLine2 = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "REC",
				WE_CurrentInventoryStatus = "REC",
				WE_DocketLineStatus = "PFU",
				WE_UnloadedTime = DateTimeOffset.Now,
				WE_GS_NKUnloadedBy = "A"
			}.AppendInsertAndReturnObject(data.Sql);

			var putawayTransfer = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "TFR", "HFT", "T1") { WD_IsPutawayTransfer = true }.AppendInsertAndReturnObject(data.Sql);
			var putawayTransferLine1 = new WhsDocketLine(putawayTransfer, data.Product.PK, 50m, data.Locations[1].PK)
			{
				WE_StockOnHand = 50m,
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_OriginalInventoryStatus = "PTA",
				WE_CurrentInventoryStatus = "PTA",
				WE_AdjustmentArrivalDate = data.Today
			}.AppendInsertAndReturnObject(data.Sql);
			var putawayTransferLine2 = new WhsDocketLine(putawayTransfer, data.Product.PK, 50m, data.Locations[1].PK)
			{
				WE_StockOnHand = 0m,
				WE_DocketLineStatus = "HFT",
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_OriginalInventoryStatus = "PTA",
				WE_CurrentInventoryStatus = "PTA",
				WE_AdjustmentArrivalDate = data.Today
			}.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine1, putawayTransferLine1, 50m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine2, putawayTransferLine2, 50m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Putaway Transfer Lines should have Transaction Quantity = SOH.", putawayTransferLine1);
			AssertCheckProcedure_Fails("Putaway Transfer Lines should have Transaction Quantity = SOH.", putawayTransferLine2);
		}

		#endregion

		#region TestCheckProcedure_InternalTransfer_Finalised

		public void TestCheckProcedure_InternalTransfer_Finalised()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 2);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(data.Sql);

			var transfer = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var transferLine1 = new WhsDocketLine(transfer, data.Product.PK, 20m, data.Locations[1].PK) { WE_StockOnHand = 20m, WE_WL_TransferFrom = data.Locations[0].PK }.AppendInsertAndReturnObject(data.Sql);
			var transferLine2 = new WhsDocketLine(transfer, data.Product.PK, 20m, data.Locations[1].PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = data.Locations[0].PK }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine1, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine2, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine1);
			AssertCheckProcedure_Fails("Finalised Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine2);
		}

		public void TestCheckProcedure_InternalTransfer_Finalised_WithPickLines()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 2);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(data.Sql);

			var transfer = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var transferLine1 = new WhsDocketLine(transfer, data.Product.PK, 20m, data.Locations[1].PK) { WE_StockOnHand = 5m, WE_WL_TransferFrom = data.Locations[0].PK }.AppendInsertAndReturnObject(data.Sql);
			var transferLine2 = new WhsDocketLine(transfer, data.Product.PK, 20m, data.Locations[1].PK) { WE_StockOnHand = 7m, WE_WL_TransferFrom = data.Locations[0].PK }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine1, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine2, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);

			var pick = new WhsPick(data.Whs, "P1", "PIS").AppendInsertAndReturnObject(data.Sql);
			var order = new WhsDocket(data.Client.PK, data.Whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(data.Sql);
			var orderLine1 = new WhsDocketLine(order, data.Product.PK, 20m).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine1, orderLine1, 8m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine1, orderLine1, 7m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today.AddDays(-5) }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine1, orderLine1, 5m).AppendInsertAndReturnObject(data.Sql);

			var orderLine2 = new WhsDocketLine(order, data.Product.PK, 20m).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine2, orderLine2, 8m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine2, orderLine2, 7m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today.AddDays(-5) }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine2, orderLine2, 5m).AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine1);
			AssertCheckProcedure_Fails("Finalised Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine2);
		}

		public void TestCheckProcedure_InternalTransfer_Finalised_WithHeldCodeChange()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 2);
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(data.Sql);

			var transfer = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(data.Sql);
			var transferLine1 = new WhsDocketLine(transfer, data.Product.PK, 20m, data.Locations[1].PK)
			{
				WE_StockOnHand = 5m,
				WE_WL_TransferFrom = data.Locations[0].PK
			}.AppendInsertAndReturnObject(data.Sql);
			var transferLine1_HCC1 = new WhsDocketLine(transfer, data.Product.PK, 10m, data.Locations[1].PK)
			{
				WE_StockOnHand = 10m,
				WE_WE_ParentDocketLine = transferLine1,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine1
			}.AppendInsertAndReturnObject(data.Sql);
			var transferLine1_HCC2 = new WhsDocketLine(transfer, data.Product.PK, 5m, data.Locations[1].PK)
			{
				WE_StockOnHand = 0m,
				WE_WE_ParentDocketLine = transferLine1,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine1
			}.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine1, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);

			var transferLine2 = new WhsDocketLine(transfer, data.Product.PK, 20m, data.Locations[1].PK)
			{
				WE_StockOnHand = 2m,
				WE_WL_TransferFrom = data.Locations[0].PK
			}.AppendInsertAndReturnObject(data.Sql);
			var transferLine2_HCC1 = new WhsDocketLine(transfer, data.Product.PK, 10m, data.Locations[1].PK)
			{
				WE_StockOnHand = 10m,
				WE_WE_ParentDocketLine = transferLine2,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine2
			}.AppendInsertAndReturnObject(data.Sql);
			var transferLine2_HCC2 = new WhsDocketLine(transfer, data.Product.PK, 5m, data.Locations[1].PK)
			{
				WE_StockOnHand = 0m,
				WE_WE_ParentDocketLine = transferLine2,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine2
			}.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine2, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine1);
			AssertCheckProcedure_Pass("Finalised Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine1_HCC1);
			AssertCheckProcedure_Fails("Finalised Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine1_HCC2);

			AssertCheckProcedure_Fails("Finalised Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine2);
			AssertCheckProcedure_Pass("Finalised Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine2_HCC1);
			AssertCheckProcedure_Fails("Finalised Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine2_HCC2);
		}

		#endregion

		#region TestCheckProcedure_InterWhsTransfer_MasterSource_Entered

		public void TestCheckProcedure_InterWhsTransfer_MasterSource_Entered()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer_Master = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "IWS", "ENT", "TR1").AppendInsertAndReturnObject(sql);
			var transferLine_Master = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_DocketLineStatus = "ENT"
			}.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine_Master, 10m).AppendInsertAndReturnObject(sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Entered Inter-Source Transfer Lines should have SOH = 0.", transferLine_Master);
		}

		#endregion

		#region TestCheckProcedure_InterWhsTransfer_MasterSource_InTransit

		public void TestCheckProcedure_InterWhsTransfer_MasterSource_InTransit()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 30m }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer_Master = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "IWS", "ENT", "TR1").AppendInsertAndReturnObject(sql);
			var transferLine_Master1 = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_DocketLineStatus = "HFT",
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT"
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Master2 = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_DocketLineStatus = "HFT",
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT"
			}.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine_Master1, 10m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine_Master2, 10m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);

			var transfer_Child = new WhsDocket(data.Client.PK, whs2.PK, "TFR", "IWD", "ENT", "TR2") { WD_WD_ParentDocket = transfer_Master }.AppendInsertAndReturnObject(sql);
			var transferLine_Child1 = new WhsDocketLine(transfer_Child, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master1,
				WE_DocketLineStatus = "HFT",
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT"
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Child2 = new WhsDocketLine(transfer_Child, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master2,
				WE_DocketLineStatus = "HFT",
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT"
			}.AppendInsertAndReturnObject(sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("In-transit Inter-Source Transfer Lines should have SOH = 0.", transferLine_Master1);
			AssertCheckProcedure_Pass("In-transit Inter-Source Transfer Lines should have SOH = 0, but this is done in the trigger not Stored Proc.", transferLine_Master2);
			AssertCheckProcedure_Pass("In-transit Inter-Destination Transfer Lines should have Transaction Quantity = SOH.", transferLine_Child1);
			AssertCheckProcedure_Fails("In-transit Inter-Destination Transfer Lines should have Transaction Quantity = SOH.", transferLine_Child2);
		}

		#endregion

		#region TestCheckProcedure_InterWhsTransfer_MasterSource_Finalised

		public void TestCheckProcedure_InterWhsTransfer_MasterSource_Finalised()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 30m }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer_Master = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "IWS", "FIN", "TR1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Master1 = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Master2 = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = data.Locations[0].PK
			}.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine_Master1, 10m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine_Master2, 10m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);

			var transfer_Child = new WhsDocket(data.Client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR2") { WD_WD_ParentDocket = transfer_Master, WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Child1 = new WhsDocketLine(transfer_Child, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master1
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Child2 = new WhsDocketLine(transfer_Child, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master2
			}.AppendInsertAndReturnObject(sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Inter-Source Transfer Lines should have SOH = 0.", transferLine_Master1);
			AssertCheckProcedure_Pass("Finalised Inter-Source Transfer Lines should have SOH = 0, but this is done in the trigger not Stored Proc.", transferLine_Master2);
			AssertCheckProcedure_Pass("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Child1);
			AssertCheckProcedure_Fails("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Child2);
		}

		public void TestCheckProcedure_InterWhsTransfer_MasterSource_Finalised_WithPickLines()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer_Master = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "IWS", "FIN", "TR1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Master1 = new WhsDocketLine(transfer_Master, data.Product.PK, 20m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = data.Locations[0].PK }.AppendInsertAndReturnObject(sql);
			var transferLine_Master2 = new WhsDocketLine(transfer_Master, data.Product.PK, 20m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = data.Locations[0].PK }.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine_Master1, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine_Master2, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);

			var transfer_Child = new WhsDocket(data.Client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR2") { WD_WD_ParentDocket = transfer_Master, WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Child1 = new WhsDocketLine(transfer_Child, data.Product.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 5m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master1
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Child2 = new WhsDocketLine(transfer_Child, data.Product.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 7m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master2
			}.AppendInsertAndReturnObject(sql);

			// pick some stock
			var pick = new WhsPick(whs2, "P1", "PIS").AppendInsertAndReturnObject(data.Sql);
			var order = new WhsDocket(data.Client.PK, whs2.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(data.Sql);
			var orderLine1 = new WhsDocketLine(order, data.Product.PK, 20m).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Child1, orderLine1, 8m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Child1, orderLine1, 7m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today.AddDays(-5) }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Child1, orderLine1, 5m).AppendInsertAndReturnObject(data.Sql);

			var orderLine2 = new WhsDocketLine(order, data.Product.PK, 20m).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Child2, orderLine2, 8m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Child2, orderLine2, 7m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today.AddDays(-5) }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Child2, orderLine2, 5m).AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Child1);
			AssertCheckProcedure_Fails("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Child2);
		}

		public void TestCheckProcedure_InterWhsTransfer_MasterSource_Finalised_WithHoldCodeChange()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer_Master = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "IWS", "FIN", "TR1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Master1 = new WhsDocketLine(transfer_Master, data.Product.PK, 20m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = data.Locations[0].PK }.AppendInsertAndReturnObject(sql);
			var transferLine_Master2 = new WhsDocketLine(transfer_Master, data.Product.PK, 20m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = data.Locations[0].PK }.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine_Master1, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine_Master2, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);

			var transfer_Child = new WhsDocket(data.Client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR2") { WD_WD_ParentDocket = transfer_Master, WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Child1 = new WhsDocketLine(transfer_Child, data.Product.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 5m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master1
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Child1_HCC1 = new WhsDocketLine(transfer_Child, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WE_ParentDocketLine = transferLine_Child1,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine_Child1
			}.AppendInsertAndReturnObject(data.Sql);
			var transferLine_Child1_HCC2 = new WhsDocketLine(transfer_Child, data.Product.PK, 5m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WE_ParentDocketLine = transferLine_Child1,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine_Child1
			}.AppendInsertAndReturnObject(data.Sql);

			var transferLine_Child2 = new WhsDocketLine(transfer_Child, data.Product.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master2
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Child2_HCC1 = new WhsDocketLine(transfer_Child, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WE_ParentDocketLine = transferLine_Child2,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine_Child2
			}.AppendInsertAndReturnObject(data.Sql);
			var transferLine_Child2_HCC2 = new WhsDocketLine(transfer_Child, data.Product.PK, 5m, location2.PK)
			{
				WE_StockOnHand = 2m,
				WE_WE_ParentDocketLine = transferLine_Child2,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine_Child2
			}.AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Child1);
			AssertCheckProcedure_Pass("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Child1_HCC1);
			AssertCheckProcedure_Fails("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Child1_HCC2);

			AssertCheckProcedure_Fails("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Child2);
			AssertCheckProcedure_Pass("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Child2_HCC1);
			AssertCheckProcedure_Fails("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Child2_HCC2);
		}

		#endregion

		#region TestCheckProcedure_InterWhsTransfer_MasterDest_Entered

		public void TestCheckProcedure_InterWhsTransfer_MasterDest_Entered()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer_Master = new WhsDocket(data.Client.PK, whs2.PK, "TFR", "IWD", "ENT", "TR1").AppendInsertAndReturnObject(sql);
			var transferLine_Master = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_DocketLineStatus = "ENT"
			}.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine_Master, 10m).AppendInsertAndReturnObject(sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Entered Inter-Destination Transfer Lines should have SOH = 0.", transferLine_Master);
		}

		#endregion

		#region TestCheckProcedure_InterWhsTransfer_MasterDest_InTransit

		public void TestCheckProcedure_InterWhsTransfer_MasterDest_InTransit()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 30m }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer_Master = new WhsDocket(data.Client.PK, whs2.PK, "TFR", "IWD", "ENT", "TR1").AppendInsertAndReturnObject(sql);
			var transferLine_Master1 = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_DocketLineStatus = "HFT",
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT"
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Master2 = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_DocketLineStatus = "HFT",
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT"
			}.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine_Master1, 10m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine_Master2, 10m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);

			var transfer_Child = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "IWS", "ENT", "TR2") { WD_WD_ParentDocket = transfer_Master }.AppendInsertAndReturnObject(sql);
			var transferLine_Child1 = new WhsDocketLine(transfer_Child, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master1,
				WE_DocketLineStatus = "HFT",
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT"
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Child2 = new WhsDocketLine(transfer_Child, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master2,
				WE_DocketLineStatus = "HFT",
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT"
			}.AppendInsertAndReturnObject(sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("In-transit Inter-Destination Transfer Lines should have Transaction Quantity = SOH.", transferLine_Master1);
			AssertCheckProcedure_Fails("In-transit Inter-Destination Transfer Lines should have Transaction Quantity = SOH.", transferLine_Master2);
			AssertCheckProcedure_Pass("In-transit Inter-Source Transfer Lines should have SOH = 0.", transferLine_Child1);
			AssertCheckProcedure_Pass("In-transit Inter-Source Transfer Lines should have SOH = 0, but this is done in the trigger not Stored Proc.", transferLine_Child2);
		}

		#endregion

		#region TestCheckProcedure_InterWhsTransfer_MasterDest_Finalised

		public void TestCheckProcedure_InterWhsTransfer_MasterDest_Finalised()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 30m }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer_Master = new WhsDocket(data.Client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Master1 = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = data.Locations[0].PK
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Master2 = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK
			}.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine_Master1, 10m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine_Master2, 10m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);

			var transfer_Child = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "IWS", "FIN", "TR2") { WD_WD_ParentDocket = transfer_Master, WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Child1 = new WhsDocketLine(transfer_Child, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master1
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Child2 = new WhsDocketLine(transfer_Child, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_WE_ParentDocketLine = transferLine_Master2
			}.AppendInsertAndReturnObject(sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Master1);
			AssertCheckProcedure_Fails("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Master2);
			AssertCheckProcedure_Pass("Finalised Inter-Source Transfer Lines should have SOH = 0.", transferLine_Child1);
			AssertCheckProcedure_Pass("Finalised Inter-Source Transfer Lines should have SOH = 0, but this is done in the trigger not Stored Proc.", transferLine_Child2);
		}

		public void TestCheckProcedure_InterWhsTransfer_MasterDest_Finalised_WithPickLines()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

			// transfer stock
			var transfer_Master = new WhsDocket(data.Client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Master1 = new WhsDocketLine(transfer_Master, data.Product.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 5m,
				WE_WL_TransferFrom = data.Locations[0].PK
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Master2 = new WhsDocketLine(transfer_Master, data.Product.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 7m,
				WE_WL_TransferFrom = data.Locations[0].PK
			}.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine_Master1, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine_Master2, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);

			var transfer_Child = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "IWS", "FIN", "TR2") { WD_WD_ParentDocket = transfer_Master, WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(transfer_Child, data.Product.PK, 20m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = data.Locations[0].PK, WE_WE_ParentDocketLine = transferLine_Master1 }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(transfer_Child, data.Product.PK, 20m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = data.Locations[0].PK, WE_WE_ParentDocketLine = transferLine_Master2 }.AppendInsertAndReturnObject(sql);

			// pick some stock
			var pick = new WhsPick(whs2, "P1", "PIS").AppendInsertAndReturnObject(data.Sql);
			var order = new WhsDocket(data.Client.PK, whs2.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(data.Sql);
			var orderLine1 = new WhsDocketLine(order, data.Product.PK, 20m).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Master1, orderLine1, 8m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Master1, orderLine1, 7m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today.AddDays(-5) }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Master1, orderLine1, 5m).AppendInsertAndReturnObject(data.Sql);

			var orderLine2 = new WhsDocketLine(order, data.Product.PK, 20m).AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Master2, orderLine2, 8m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Master2, orderLine2, 7m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = data.Today.AddDays(-5) }.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(transferLine_Master2, orderLine2, 5m).AppendInsertAndReturnObject(data.Sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Master1);
			AssertCheckProcedure_Fails("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Master2);
		}

		public void TestCheckProcedure_InterWhsTransfer_MasterDest_Finalised_WithHoldCodeChange()
		{
			var data = new SimpleDataEnvironment(numberOfLocations: 1);
			var sql = data.Sql;
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);

			// receive stock
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Product.PK, 50m, data.Locations[0].PK) { WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

			// create master transfer
			var transfer_Master = new WhsDocket(data.Client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR1") { WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			var transferLine_Master1 = new WhsDocketLine(transfer_Master, data.Product.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 5m,
				WE_WL_TransferFrom = data.Locations[0].PK
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Master1_HCC1 = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WE_ParentDocketLine = transferLine_Master1,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine_Master1
			}.AppendInsertAndReturnObject(data.Sql);
			var transferLine_Master1_HCC2 = new WhsDocketLine(transfer_Master, data.Product.PK, 5m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WE_ParentDocketLine = transferLine_Master1,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine_Master1
			}.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine_Master1, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);

			var transferLine_Master2 = new WhsDocketLine(transfer_Master, data.Product.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = data.Locations[0].PK
			}.AppendInsertAndReturnObject(sql);
			var transferLine_Master2_HCC1 = new WhsDocketLine(transfer_Master, data.Product.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WE_ParentDocketLine = transferLine_Master2,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine_Master2
			}.AppendInsertAndReturnObject(data.Sql);
			var transferLine_Master2_HCC2 = new WhsDocketLine(transfer_Master, data.Product.PK, 5m, location2.PK)
			{
				WE_StockOnHand = 2m,
				WE_WE_ParentDocketLine = transferLine_Master2,
				WE_WL_TransferFrom = data.Locations[0].PK,
				WE_IsOriginalInventory = false,
				WE_PutawayTime = null,
				WE_WE_OriginalDocketLineForRating = transferLine_Master2
			}.AppendInsertAndReturnObject(data.Sql);
			new WhsPickLine(receiveLine, transferLine_Master2, 20m) { WZ_PickedDateTime = data.Today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(data.Sql);

			// create child transfer
			var transfer_Child = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "IWS", "FIN", "TR2") { WD_WD_ParentDocket = transfer_Master, WD_FinalisedDate = data.Today }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(transfer_Child, data.Product.PK, 20m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = data.Locations[0].PK, WE_WE_ParentDocketLine = transferLine_Master1 }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(transfer_Child, data.Product.PK, 20m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = data.Locations[0].PK, WE_WE_ParentDocketLine = transferLine_Master2 }.AppendInsertAndReturnObject(sql);
			SaveBadData(data.Sql.ToStringWithNewLineBetweenAppends());

			AssertCheckProcedure_Pass("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Master1);
			AssertCheckProcedure_Pass("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Master1_HCC1);
			AssertCheckProcedure_Fails("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Master1_HCC2);

			AssertCheckProcedure_Fails("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Master2);
			AssertCheckProcedure_Pass("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Master2_HCC1);
			AssertCheckProcedure_Fails("Finalised Inter-Destination Transfer Lines should have Transaction Quantity = SOH + HCC + Picked Qty.", transferLine_Master2_HCC2);
		}

		#endregion

		#region SaveBadData

		void SaveBadData(string sqlToSave)
		{
			// suspend triggers to save bad data into DB
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsDocketLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			// defer triggers to run at the end
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(sqlToSave);
			}
		}

		#endregion

		#region Asserts

		void AssertCheckProcedure_Pass(string errorDescription, WhsDocketLine docketLine)
		{
			var sql = GetCheckProcedureSql(docketLine);
			AssertNoExceptionThrown(errorDescription, () => Db.Connection.ExecuteNonQuery(sql));
		}

		void AssertCheckProcedure_Fails(string errorDescription, WhsDocketLine docketLine)
		{
			var sql = GetCheckProcedureSql(docketLine);
			AssertExceptionThrown(errorDescription, typeof(SqlException), ExpectedErrorMessageFromCheckProcedure, () => Db.Connection.ExecuteNonQuery(sql), assertStartsWith: true);
		}

		string GetCheckProcedureSql(WhsDocketLine docketLine)
		{
			return $@"
DECLARE @LinePKs dbo.TVP_uniqueidentifier;
INSERT INTO @LinePKs
SELECT 
	WE_PK 
FROM 
	dbo.WhsDocketLine
where
	WE_PK = '{docketLine.PK}'

EXEC {ProcedureName} @LinePKs;";
		}

		protected abstract string ProcedureName { get; }

		#endregion

		#region SimpleDataEnvironment class

		class SimpleDataEnvironment
		{
			public SimpleDataEnvironment(int numberOfLocations)
			{
				Sql = new SqlQueryBuilder();
				Whs = new WhsWarehouse("WH1").WithDockDoor(Db.Connection);
				Locations = new WhsLocation[numberOfLocations];

				var area = new WhsArea(Whs.PK, "AREA1").AppendInsertAndReturnObject(Sql);
				var row = new WhsRow(Whs, "A") { WR_Columns = (short)numberOfLocations }.AppendInsertAndReturnObject(Sql);
				for (short i = 1; i <= numberOfLocations; i++)
				{
					Locations[i - 1] = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = i }.AppendInsertAndReturnObject(Sql);
				}

				Client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(Sql);
				Product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(Sql);
				Today = DateTime.Today;
			}

			public readonly SqlQueryBuilder Sql;
			public readonly WhsWarehouse Whs;
			public readonly WhsLocation[] Locations;
			public readonly OrgHeader Client;
			public readonly OrgSupplierPart Product;
			public readonly DateTime Today;
		}

		#endregion
	}
}
