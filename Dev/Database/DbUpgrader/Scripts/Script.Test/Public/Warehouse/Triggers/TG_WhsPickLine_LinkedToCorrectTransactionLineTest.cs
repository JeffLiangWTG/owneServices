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
	[TestedType(typeof(TG_WhsPickLine_LinkedToCorrectTransactionLine))]
	class TG_WhsPickLine_LinkedToCorrectTransactionLineTest : DBCreateTriggerScriptTest
	{
		#region TestTrigger_Insert

		#region TestTrigger_Insert_ReceiveLine

		[UseSnapshotProtection]
		public void TestTrigger_Insert_ReceiveLine()
		{
			TestTriggerFailsCore("Receive line should not be a transaction line of pick lines.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				// insert incorrect pick line
				new WhsPickLine(receiveLine, receiveLine, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = DateTime.Today }.AppendInsertAndReturnObject(sql);
			});
		}

		#endregion

		#region TestTrigger_Insert_AdjustmentLine

		[UseSnapshotProtection]
		public void TestTrigger_Insert_AdjustmentInLine()
		{
			TestTriggerFailsCore("Adjustment In line should not be a transaction line of pick lines.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var today = DateTime.Today;
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "AD1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, 10m, locations[0].PK).AppendInsertAndReturnObject(sql);

				// insert incorrect pick line
				new WhsPickLine(receiveLine, adjustmentLine, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);
			});
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_AdjustmentOutLine()
		{
			TestTriggerPassCore("Adjustment Out line should have pick lines.", 40m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var today = DateTime.Today;
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "FIN", "AD1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var adjustmentLine = new WhsDocketLine(adjustment, part.PK, -10m, locations[0].PK).AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, adjustmentLine, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);
			});
		}

		#endregion

		#region TestTrigger_Insert_InternalTransferLine

		[UseSnapshotProtection]
		public void TestTrigger_Insert_InternalTransferLine()
		{
			TestTriggerPassCore("Internal transfer line should have pick lines.", 40m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var today = DateTime.Today;
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "TFR1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var transferLine = new WhsDocketLine(transfer, part.PK, 10m, locations[1].PK) { WE_WL_TransferFrom = locations[0].PK, WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, transferLine, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);
			});
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_InternalTransferLine_HoldCodeChange()
		{
			TestTriggerFailsCore("Hold Code Change line should not be a transaction line of pick lines.", 40m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var today = DateTime.Today;

				// add correct transfer line
				var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "TFRX") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var transferLine = new WhsDocketLine(transfer, part.PK, 10m, locations[1].PK) { WE_WL_TransferFrom = locations[0].PK, WE_StockOnHand = 6m }.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, transferLine, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);

				// add hold code change transfer line with incorrect pick line. 
				var transferLine_HoldCodeChange = new WhsDocketLine(transfer, part.PK, 4m, locations[1].PK) { WE_PutawayTime = null, WE_WE_ParentDocketLine = transferLine, WE_IsOriginalInventory = false, WE_WHC_NKCurrentInventoryHeldCode = "YES", WE_WHC_NKOriginalInventoryHeldCode = "YES", WE_CurrentInventoryStatus = "HEL", WE_OriginalInventoryStatus = "HEL", WE_WL_TransferFrom = locations[0].PK, WE_StockOnHand = 4m, WE_GS_NKPutawayBy = "E", WE_AdjustmentArrivalDate = today, WE_F3_NKPackType = "UNT" }.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, transferLine_HoldCodeChange, 4m) { WZ_PickedDateTime = today, WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);
			});
		}

		#endregion

		#region TestTrigger_Insert_InterWhsSourceMaster

		[UseSnapshotProtection]
		public void TestTrigger_Insert_InterWhsSourceMaster_LinkedToMasterLine()
		{
			TestTriggerPassCore("For inter-whs transfers pick lines should always be linked to master transfer lines.", 40m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(Db.Connection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(Db.Connection);
				var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
				var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
				var locationB1 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);
				var today = DateTime.Today;

				var transfer_Master = new WhsDocket(client.PK, whs.PK, "TFR", "IWS", "FIN", "TR1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var transferLine_Master = new WhsDocketLine(transfer_Master, part.PK, 10m, locationB1.PK) { WE_WL_TransferFrom = locations[0].PK, WE_GS_NKPutawayBy = "AA" }.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, transferLine_Master, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);

				var transfer_Child = new WhsDocket(client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR2") { WD_FinalisedDate = today, WD_WD_ParentDocket = transfer_Master }.AppendInsertAndReturnObject(sql);
				var transferLine_Child = new WhsDocketLine(transfer_Child, part.PK, 10m, locationB1.PK) { WE_WL_TransferFrom = locations[0].PK, WE_GS_NKPutawayBy = "AA", WE_WE_ParentDocketLine = transferLine_Master, WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);
			});
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_InterWhsSourceMaster_LinkedToChildLine()
		{
			TestTriggerFailsCore("For inter-whs transfers pick lines should always be linked to master transfer lines.", 40m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(Db.Connection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(Db.Connection);
				var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
				var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
				var locationB1 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);
				var today = DateTime.Today;

				var transfer_Master = new WhsDocket(client.PK, whs.PK, "TFR", "IWS", "FIN", "TR1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var transferLine_Master = new WhsDocketLine(transfer_Master, part.PK, 10m, locationB1.PK) { WE_WL_TransferFrom = locations[0].PK, WE_GS_NKPutawayBy = "AA" }.AppendInsertAndReturnObject(sql);

				var transfer_Child = new WhsDocket(client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR2") { WD_FinalisedDate = today, WD_WD_ParentDocket = transfer_Master }.AppendInsertAndReturnObject(sql);
				var transferLine_Child = new WhsDocketLine(transfer_Child, part.PK, 10m, locationB1.PK) { WE_WL_TransferFrom = locations[0].PK, WE_GS_NKPutawayBy = "AA", WE_WE_ParentDocketLine = transferLine_Master }.AppendInsertAndReturnObject(sql);

				// insert incorrect pick line
				new WhsPickLine(receiveLine, transferLine_Child, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);
			});
		}

		#endregion

		#region TestTrigger_Insert_InterWhsDestinationMaster

		[UseSnapshotProtection]
		public void TestTrigger_Insert_InterWhsDestinationMaster_LinkedToMasterLine()
		{
			TestTriggerPassCore("For inter-whs transfers pick lines should always be linked to master transfer lines.", 40m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(Db.Connection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(Db.Connection);
				var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
				var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
				var locationB1 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);
				var today = DateTime.Today;

				var transfer_Master = new WhsDocket(client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var transferLine_Master = new WhsDocketLine(transfer_Master, part.PK, 10m, locationB1.PK) { WE_WL_TransferFrom = locations[0].PK, WE_GS_NKPutawayBy = "AA", WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, transferLine_Master, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);

				var transfer_Child = new WhsDocket(client.PK, whs.PK, "TFR", "IWS", "FIN", "TR2") { WD_FinalisedDate = today, WD_WD_ParentDocket = transfer_Master }.AppendInsertAndReturnObject(sql);
				var transferLine_Child = new WhsDocketLine(transfer_Child, part.PK, 10m, locationB1.PK) { WE_WL_TransferFrom = locations[0].PK, WE_GS_NKPutawayBy = "AA", WE_WE_ParentDocketLine = transferLine_Master }.AppendInsertAndReturnObject(sql);
			});
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_InterWhsDestinationMaster_LinkedToChildLine()
		{
			TestTriggerFailsCore("For inter-whs transfers pick lines should always be linked to master transfer lines.", 40m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(Db.Connection);
				var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(Db.Connection);
				var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
				var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
				var locationB1 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);
				var today = DateTime.Today;

				var transfer_Master = new WhsDocket(client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var transferLine_Master = new WhsDocketLine(transfer_Master, part.PK, 10m, locationB1.PK) { WE_WL_TransferFrom = locations[0].PK, WE_GS_NKPutawayBy = "AA" }.AppendInsertAndReturnObject(sql);

				var transfer_Child = new WhsDocket(client.PK, whs.PK, "TFR", "IWS", "FIN", "TR2") { WD_FinalisedDate = today, WD_WD_ParentDocket = transfer_Master }.AppendInsertAndReturnObject(sql);
				var transferLine_Child = new WhsDocketLine(transfer_Child, part.PK, 10m, locationB1.PK) { WE_WL_TransferFrom = locations[0].PK, WE_GS_NKPutawayBy = "AA", WE_WE_ParentDocketLine = transferLine_Master }.AppendInsertAndReturnObject(sql);

				// insert incorrect pick line
				new WhsPickLine(receiveLine, transferLine_Child, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = today }.AppendInsertAndReturnObject(sql);
			});
		}

		#endregion

		#region TestTrigger_Insert_OrderLine

		[UseSnapshotProtection]
		public void TestTrigger_Insert_OrderLine()
		{
			TestTriggerFailsCore("Not Picked Order line can not have committing pick lines.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, part.PK, 10m).AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, orderLine, 10m).AppendInsertAndReturnObject(sql);
			});
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_OrderLine_Reserving()
		{
			TestTriggerPassCore("Order line can have reserving pick lines.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, part.PK, 10m).AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, orderLine, 10m) { WZ_OriginalReservedQty = 10m }.AppendInsertAndReturnObject(sql);
			});
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_OrderLine_Picked()
		{
			TestTriggerPassCore("Order line can have pick lines.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var pick = new WhsPick(whs, "P1", "PIC").AppendInsertAndReturnObject(sql);
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, part.PK, 10m).AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, orderLine, 10m).AppendInsertAndReturnObject(sql);
			});
		}

		#endregion

		#region TestTrigger_Insert_WorkOrder

		[UseSnapshotProtection]
		public void TestTrigger_Insert_WorkOrder()
		{
			TestTriggerFailsCore("Not picked work order lines should not have pick lines.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var part2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
				new OrgPartBOM(part, part2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine2 = new WhsDocketLine(receive2, part2.PK, 50m, locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				var workOrder = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ENT", "WO1").AppendInsertAndReturnObject(sql);
				var workOrderMainLine = new WhsDocketLine(workOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var workOrderComponentLine = new WhsDocketLine(workOrder, part2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine2, workOrderComponentLine, 10m).AppendInsertAndReturnObject(sql);
			});
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DynamicWorkOrder()
		{
			TestTriggerFailsCore("Not picked work order lines should not have pick lines.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var part2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
				new OrgPartBOM(part, part2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine2 = new WhsDocketLine(receive2, part2.PK, 50m, locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				var workOrder = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "ENT", "WO1").AppendInsertAndReturnObject(sql);
				var workOrderMainLine = new WhsDocketLine(workOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var workOrderComponentLine = new WhsDocketLine(workOrder, part2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine2, workOrderComponentLine, 10m).AppendInsertAndReturnObject(sql);
			});
		}

		#region TestTrigger_Insert_WorkOrder_Assemble

		[UseSnapshotProtection]
		public void TestTrigger_Insert_WorkOrder_Assemble_LinkedToTopLevelBOMLine()
		{
			TestTriggerFailsCore("Assemble work orders should not have any pick lines linked to main product line.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var part2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
				new OrgPartBOM(part, part2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var pick = new WhsPick(whs, "P1", "PIC").AppendInsertAndReturnObject(sql);
				var workOrder = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "PIC", "WO1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var workOrderMainLine = new WhsDocketLine(workOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var workOrderComponentLine = new WhsDocketLine(workOrder, part2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);

				// insert incorrect pick line
				new WhsPickLine(receiveLine, workOrderMainLine, 10m).AppendInsertAndReturnObject(sql);
			});
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DynamicWorkOrder_Assemble_LinkedToTopLevelBOMLine()
		{
			TestTriggerFailsCore("Assemble work orders should not have any pick lines linked to main product line.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var part2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
				new OrgPartBOM(part, part2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var pick = new WhsPick(whs, "P1", "PIC").AppendInsertAndReturnObject(sql);
				var workOrder = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "PIC", "WO1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var workOrderMainLine = new WhsDocketLine(workOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var workOrderComponentLine = new WhsDocketLine(workOrder, part2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);

				// insert incorrect pick line
				new WhsPickLine(receiveLine, workOrderMainLine, 10m).AppendInsertAndReturnObject(sql);
			});
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_WorkOrder_Assemble_LinkedToChildBOMLine()
		{
			TestTriggerPassCore("Assemble work orders should have pick lines linked to component product line.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var part2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
				new OrgPartBOM(part, part2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine2 = new WhsDocketLine(receive2, part2.PK, 50m, locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(whs, "P1", "PIC").AppendInsertAndReturnObject(sql);
				var workOrder = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "PIC", "WO1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var workOrderMainLine = new WhsDocketLine(workOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var workOrderComponentLine = new WhsDocketLine(workOrder, part2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine2, workOrderComponentLine, 10m).AppendInsertAndReturnObject(sql);
			});
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_DynamicWorkOrder_Assemble_LinkedToChildBOMLine()
		{
			TestTriggerPassCore("Assemble work orders should have pick lines linked to component product line.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var part2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
				new OrgPartBOM(part, part2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine2 = new WhsDocketLine(receive2, part2.PK, 50m, locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(whs, "P1", "PIC").AppendInsertAndReturnObject(sql);
				var workOrder = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "PIC", "WO1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var workOrderMainLine = new WhsDocketLine(workOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var workOrderComponentLine = new WhsDocketLine(workOrder, part2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine2, workOrderComponentLine, 10m).AppendInsertAndReturnObject(sql);
			});
		}

		#endregion

		#region TestTrigger_Insert_WorkOrder_Disassemble

		[UseSnapshotProtection]
		public void TestTrigger_Insert_WorkOrder_Disassemble_LinkedToTopLevelBOMLine()
		{
			TestTriggerPassCore("Disassemble work orders should have pick lines linked to main product line.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var part2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
				new OrgPartBOM(part, part2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(whs, "P1", "PIC").AppendInsertAndReturnObject(sql);
				var workOrder = new WhsDocket(client.PK, whs.PK, "WOR", "DIS", "PIC", "WO1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var workOrderMainLine = new WhsDocketLine(workOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var workOrderComponentLine = new WhsDocketLine(workOrder, part2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);
				new WhsPickLine(receiveLine, workOrderMainLine, 10m).AppendInsertAndReturnObject(sql);
			});
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_WorkOrder_Disassemble_LinkedToChildBOMLine()
		{
			TestTriggerFailsCore("Disassemble work orders should not have any pick lines linked to component product line.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var part2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
				new OrgPartBOM(part, part2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

				var today = DateTime.Today;
				var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
				var receiveLine2 = new WhsDocketLine(receive2, part2.PK, 50m, locations[0].PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(whs, "P1", "PIC").AppendInsertAndReturnObject(sql);
				var workOrder = new WhsDocket(client.PK, whs.PK, "WOR", "DIS", "PIC", "WO1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var workOrderMainLine = new WhsDocketLine(workOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var workOrderComponentLine = new WhsDocketLine(workOrder, part2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);

				// insert incorrect pick line
				new WhsPickLine(receiveLine2, workOrderComponentLine, 10m).AppendInsertAndReturnObject(sql);
			});
		}

		#endregion

		#endregion

		#endregion

		#region TestTrigger_Update_WZ_WE_TransactionLine

		[UseSnapshotProtection]
		public void TestTrigger_Update_WZ_WE_TransactionLine()
		{
			TestTriggerFailsCore("Updating WZ_WE_TransactionLine to point to incorrect transaction line should cause trigger to fail.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var part2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
				new OrgPartBOM(part, part2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

				var pick = new WhsPick(whs, "P1", "PIC").AppendInsertAndReturnObject(sql);
				var workOrder = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "PIC", "WO1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
				var workOrderMainLine = new WhsDocketLine(workOrder, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var workOrderComponentLine = new WhsDocketLine(workOrder, part2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);
				var pickLine = new WhsPickLine(receiveLine, workOrderComponentLine, 10m).AppendInsertAndReturnObject(sql);

				// update pick line incorrectly
				sql.Append(WhsPickLine
					.UpdateWhere(pickLine.PK)
					.Set(l => l.WZ_WE_TransactionLine, workOrderMainLine).AsSQL());
			});
		}

		#endregion

		#region TestTrigger_Update_WZ_OriginalReservedQty

		[UseSnapshotProtection]
		public void TestTrigger_Update_WZ_OriginalReservedQty()
		{
			TestTriggerFailsCore("Updating WZ_OriginalReservedQty to make pick line not reserved and linked to not picked order should cause trigger to fail.", 50m, (sql, whs, client, part, locations, receiveLine) =>
			{
				var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
				var orderLine = new WhsDocketLine(order, part.PK, 10m).AppendInsertAndReturnObject(sql);
				var reservedPickLine = new WhsPickLine(receiveLine, orderLine, 10m) { WZ_OriginalReservedQty = 10m }.AppendInsertAndReturnObject(sql);

				// update pick line incorrectly
				sql.Append(WhsPickLine
					.UpdateWhere(reservedPickLine.PK)
					.Set(l => l.WZ_OriginalReservedQty, 0m).AsSQL());
			});
		}

		#endregion

		#region TestTriggerCore

		void TestTriggerFailsCore(string errorMessage, decimal totalUnits, InsertDataDelegate addIncorrectDataSetup)
		{
			TestTriggerCore(errorMessage, totalUnits, addIncorrectDataSetup, shouldFail: true);
		}

		void TestTriggerPassCore(string errorMessage, decimal totalUnits, InsertDataDelegate addCorrectDataSetup)
		{
			TestTriggerCore(errorMessage, totalUnits, addCorrectDataSetup, shouldFail: false);
		}

		void TestTriggerCore(string errorMessage, decimal totalUnits, InsertDataDelegate testSpecificDataSetup, bool shouldFail)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(Db.Connection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			// receive stock
			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "RX") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, part.PK, 50m, locationA1.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			// add test specific data
			var sqlWithIncorrectPickLine = new SqlQueryBuilder();
			sqlWithIncorrectPickLine.Append($"update dbo.WhsDocketLine set WE_StockOnHand = {totalUnits}, WE_SystemLastEditTimeUtc = SYSUTCDATETIME(), WE_SystemLastEditUser = '~BP' where WE_PK = '{receiveLine.PK}';");

			testSpecificDataSetup(sqlWithIncorrectPickLine, whs, client, part, new[] { locationA1, locationA2 }, receiveLine);

			if (shouldFail)
			{
				AssertTriggerFails(errorMessage, sqlWithIncorrectPickLine.ToStringWithNewLineBetweenAppends());
			}
			else
			{
				// defer unrelated triggers
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.WZ_WE_TransactionLine, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName))
				{
					AssertNoExceptionThrown(errorMessage, () => Db.Connection.ExecuteNonQuery(sqlWithIncorrectPickLine.ToStringWithNewLineBetweenAppends()));
				}
			}
		}

		delegate void InsertDataDelegate(SqlQueryBuilder sql, WhsWarehouse whs, OrgHeader client, OrgSupplierPart partPK, WhsLocation[] locations, WhsDocketLine receiveLine);

		#region AssertTriggerFails

		static void AssertTriggerFails(string errorDescription, string sql)
		{
			// Suspend triggers to save bad data into DB for another trigger to fail. 
			// Cannot temporary suspend the triggers because transaction is killed by another failed trigger.
			TestWhsDataSetupHelper.SuspendTrigger_ForPreUpgradeTransformations(TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsDocketLineSchema.Constants.TableName);
			TestWhsDataSetupHelper.SuspendTrigger_ForPreUpgradeTransformations(TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName);
			TestWhsDataSetupHelper.SuspendTrigger_ForPreUpgradeTransformations(TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsDocketLineSchema.Constants.TableName);
			TestWhsDataSetupHelper.SuspendTrigger_ForPreUpgradeTransformations(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName);
			TestWhsDataSetupHelper.SuspendTrigger_ForPreUpgradeTransformations(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName);
			TestWhsDataSetupHelper.SuspendTrigger_ForPreUpgradeTransformations(TestWhsDataSetupHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName);

			try
			{
				var command = Db.Connection.Command(sql);
				command.CommandTimeout = 5; // seconds
				command.ExecuteNonQuery();

				Assert(errorDescription, false);
			}
			catch (SqlException ex)
			{
				Assert(errorDescription, ex.Message.Contains("Attempt to link Pick Line to incorrect Transaction Line."));
			}
		}
		#endregion

		#endregion
	}
}
