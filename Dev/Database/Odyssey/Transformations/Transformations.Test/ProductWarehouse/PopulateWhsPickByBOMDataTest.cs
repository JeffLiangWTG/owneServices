using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.ProductWarehouse
{
	[TestedType(typeof(PopulateWhsPickByBOMData))]
	class PopulateWhsPickByBOMDataTest : DataTransformationTestCase
	{
		#region Basic Test

		protected override void PrepareTestData()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3").AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, kit.PK, 10m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "P1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_WP = pick.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "E",
			}.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order, kit.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, kit.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(transferLine1, orderLine1, 10m).AppendInsertAndReturnObject(sql);

			SaveToDB(sql);
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("No new Receive created.", 1, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			AssertEquals("No new Transfer Lines created.", 1, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
		}

		#endregion

		#region TestNoValidDataToProcess

		public void TestNoValidDataToProcess()
		{
			AssertEquals("Precondition", 0, WhsDocket.CountInDB(TestConnection));

			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run());
			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run());

			AssertEquals(0, WhsDocket.CountInDB(TestConnection));
		}

		#endregion

		#region TestBOMWithOnlyOrdersAndNoPick

		public void TestBOMWithOnlyOrdersAndNoPick()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, kit.PK, 10m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 10m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "NEW", "O1").AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run();
			GetNewTestTransformationInstance().Run();

			AssertEquals("No new Receive created.", 1, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			AssertEquals("No new Transfer Lines created.", 0, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
		}

		#endregion

		#region TestBOMWithOrdersAndPick_NoComponentLines

		public void TestBOMWithOrdersAndPick_NoComponentLines()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "P1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1")
			{
				WD_WP = pick.PK,
			}.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run();
			GetNewTestTransformationInstance().Run();

			AssertEquals("No new Receive created.", 0, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			AssertEquals("No new Transfer Lines created.", 0, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
		}

		#endregion

		#region TestBOMWithOrdersAndUnfinalisedPicks_NotPickedYet

		public void TestBOMWithOrdersAndUnfinalisedPicks_NotPickedYet()
		{
			TestBOMWithOrdersAndUnfinalisedPicks_NotPickedYetCore(TransformationSection.OfflinePostUpgrade);
		}

		public void TestBOMWithOrdersAndUnfinalisedPicks_NotPickedYet_ShouldNotBeTransformedInOnlinePhase()
		{
			TestBOMWithOrdersAndUnfinalisedPicks_NotPickedYetCore(TransformationSection.OnlinePostUpgrade);
		}

		void TestBOMWithOrdersAndUnfinalisedPicks_NotPickedYetCore(TransformationSection transformationSection)
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 100m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 100m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1")
			{
				WD_WP = pick.PK,
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, orderComponentLine1, 10m).AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, orderComponentLine2, 20m).AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(transformationSection, CancellationToken.None);
			GetNewTestTransformationInstance().Run(transformationSection, CancellationToken.None);

			if (transformationSection == TransformationSection.OfflinePostUpgrade)
			{
				AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
				var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
				newReceive.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
					.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
					.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
					.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
					.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
					.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate, null)
					.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "ENT")
					.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "")
					.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
					.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
					.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
					.VerifyAll();

				var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
				newReceiveLine.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
					.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "")
					.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "PND")
					.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "PND")
					.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
					.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
					.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 10m)
					.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, null)
					.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
					.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
					.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
					.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate, null)
					.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate, null)
					.VerifyAll();

				AssertEquals("New Pick Lines should be created.", 3, WhsPickLine.CountInDB(TestConnection));
				var newPickLine = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
				newPickLine.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(newPickLine.WZ_Units), l => l.WZ_Units, 10m)
					.ExpectEquals(nameof(newPickLine.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
					.ExpectEquals(nameof(newPickLine.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
					.ExpectEquals(nameof(newPickLine.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
					.VerifyAll();

				AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
				var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
				link1.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
					.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
					.VerifyAll();
				var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
				link2.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
					.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
					.VerifyAll();

				AssertEquals("No new Transfer Lines created.", 0, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			}
			else
			{
				AssertEquals("No new Receive created.", 1, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
				AssertEquals("No new Receive Lines created.", 2, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "INW"));
				AssertEquals("No new Transfer Lines created.", 0, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			}
		}

		#endregion

		#region TestBOMWithOrdersAndUnfinalisedPicks_PickedAndInTransit

		public void TestBOMWithOrdersAndUnfinalisedPicks_PickedAndInTransit()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 80m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = location1.PK,
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT",
				WE_DocketLineStatus = "HFT",
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 20m,
				WE_WL_TransferFrom = location1.PK,
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT",
				WE_DocketLineStatus = "HFT",
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 20m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1, 10m) { WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine2, 20m) { WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate, null)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "ENT")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, null)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate, null)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate, null)
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 5, WhsPickLine.CountInDB(TestConnection));
			var newPickLine = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(newPickLine.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.ExpectEquals(nameof(newPickLine.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			AssertEquals("No new Transfer Lines created.", 2, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine3.PK)
				.ExpectEquals(nameof(pickLine3.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine3.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine4.PK)
				.ExpectEquals(nameof(pickLine4.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine4.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndUnfinalisedPicks_OneComponentIsPickedAndOneIsNot

		public void TestBOMWithOrdersAndUnfinalisedPicks_OneComponentIsPickedAndOneIsNot()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 100m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = location1.PK,
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT",
				WE_DocketLineStatus = "HFT",
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, orderComponentLine2, 20m).AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1, 10m) { WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate, null)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "ENT")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, null)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate, null)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate, null)
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 4, WhsPickLine.CountInDB(TestConnection));
			var newPickLine = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(newPickLine.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.ExpectEquals(nameof(newPickLine.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			AssertEquals("No new Transfer Lines created.", 1, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine2.PK)
				.ExpectEquals(nameof(pickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine3.PK)
				.ExpectEquals(nameof(pickLine3.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine3.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndUnfinalisedPicks_OneComponentIsPartiallyPickedAndOneIsNotPicked

		public void TestBOMWithOrdersAndUnfinalisedPicks_OneComponentIsPartiallyPickedAndOneIsNotPicked()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 100m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = location1.PK,
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT",
				WE_DocketLineStatus = "HFT",
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, orderComponentLine1, 10m).AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, orderComponentLine2, 10m).AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(receiveLine2, transferLine2, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine2, 10m) { WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate, null)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "ENT")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, null)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate, null)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate, null)
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 5, WhsPickLine.CountInDB(TestConnection));
			var newPickLine = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(newPickLine.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.ExpectEquals(nameof(newPickLine.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			AssertEquals("No new Transfer Lines created.", 1, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine1.PK)
				.ExpectEquals(nameof(pickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine2.PK)
				.ExpectEquals(nameof(pickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine4.PK)
				.ExpectEquals(nameof(pickLine4.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine4.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndUnfinalisedPicks_OneComponentIsPartiallyPickedAndOneIsAllPicked

		public void TestBOMWithOrdersAndUnfinalisedPicks_OneComponentIsPartiallyPickedAndOneIsAllPicked()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = location1.PK,
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT",
				WE_DocketLineStatus = "HFT",
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = location1.PK,
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT",
				WE_DocketLineStatus = "HFT",
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(transferLine1, orderComponentLine1, 10m).AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(receiveLine2, orderComponentLine2, 10m).AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(receiveLine2, transferLine2, 10m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine5 = new WhsPickLine(transferLine2, orderComponentLine2, 10m) { WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate, null)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "ENT")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, null)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate, null)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate, null)
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 6, WhsPickLine.CountInDB(TestConnection));
			var newPickLine = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(newPickLine.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.ExpectEquals(nameof(newPickLine.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			AssertEquals("No new Transfer Lines created.", 2, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine2.PK)
				.ExpectEquals(nameof(pickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine3.PK)
				.ExpectEquals(nameof(pickLine3.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine3.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndUnfinalisedPicks_PickedAndInTransit_HasShortedComponentLine

		public void TestBOMWithOrdersAndUnfinalisedPicks_PickedAndInTransit_HasShortedComponentLine()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 10m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 20m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine3 = new WhsDocketLine(receive1, component2.PK, 2m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "HEL",
				WE_WHC_NKCurrentInventoryHeldCode = "SHORT",
				WE_WE_ParentDocketLine = receiveLine2,
				WE_WE_OriginalDocketLineForRating = receiveLine2,
				WE_IsOriginalInventory = false,
				WE_StockOnHand = 2m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = location1.PK,
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT",
				WE_DocketLineStatus = "HFT",
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 18m, location2.PK)
			{
				WE_StockOnHand = 18m,
				WE_WL_TransferFrom = location1.PK,
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT",
				WE_DocketLineStatus = "HFT",
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 18m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1, 10m) { WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine2, 18m) { WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate, null)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "ENT")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 9m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 9m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 9m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, null)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate, null)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate, null)
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 5, WhsPickLine.CountInDB(TestConnection));
			var newPickLine = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine.WZ_Units), l => l.WZ_Units, 9m)
				.ExpectEquals(nameof(newPickLine.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(newPickLine.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.ExpectEquals(nameof(newPickLine.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 9m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 18m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			AssertEquals("No new Transfer Lines created.", 2, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine3.PK)
				.ExpectEquals(nameof(pickLine3.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine3.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine4.PK)
				.ExpectEquals(nameof(pickLine4.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine4.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndUnfinalisedPicks_PickedAndInTransit_HasOneComponentAllShorted

		public void TestBOMWithOrdersAndUnfinalisedPicks_PickedAndInTransit_HasOneComponentAllShorted()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 10m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 10m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine3 = new WhsDocketLine(receive1, component2.PK, 10m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "HEL",
				WE_WHC_NKCurrentInventoryHeldCode = "SHORT",
				WE_WE_ParentDocketLine = receiveLine2,
				WE_WE_OriginalDocketLineForRating = receiveLine2,
				WE_IsOriginalInventory = false,
				WE_StockOnHand = 10m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "P1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 10m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = location1.PK,
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT",
				WE_DocketLineStatus = "HFT",
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(transferLine1, orderComponentLine1, 10m) { WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("No need to create new Receive.", 1, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			AssertEquals("No new Receive Lines created.", 3, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "INW"));
			AssertEquals("No new Pick Lines created.", 2, WhsPickLine.CountInDB(TestConnection));
			AssertEquals("No WhsBOMInventoryPivot created because one WIP_ComponentQuantity is ZERO now.", 0, WhsBOMInventoryPivot.CountInDB(TestConnection));
			AssertEquals("No new Transfer Lines created.", 1, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine2.PK)
				.ExpectEquals(nameof(pickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndUnfinalisedPicks_Staged

		[TestDate(2024, 3, 18, 5, 6, 7)]
		public void TestBOMWithOrdersAndUnfinalisedPicks_Staged()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 80m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_FinalisedDate = today.AddHours(3),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 20m,
				WE_FinalisedDate = today.AddHours(4),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 20m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1, 10m) { WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine2, 20m) { WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate, null)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "ENT")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "PFU")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "PUT")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "PUT")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, location1.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate, null)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.VerifyAll();

			AssertEquals("New Transfer Lines created.", 3, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			var newTransferLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_DocketLineType == "TFR" && l.WE_OP == kit.PK).Single();
			newTransferLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newTransferLine1.WE_WD), l => l.WE_WD.FK, transfer.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newTransferLine1.WE_WL_TransferFrom), l => l.WE_WL_TransferFrom, location1.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_StockOnHand), l => l.WE_StockOnHand, 10m)
				.ExpectEquals(nameof(newTransferLine1.WE_FinalisedDate), l => l.WE_FinalisedDate, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newTransferLine1.WE_PutawayTime), l => l.WE_PutawayTime.Value.DateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 6, WhsPickLine.CountInDB(TestConnection));
			var newPickLine1 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine1.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newTransferLine1.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(newPickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();
			var newPickLine2 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_InventoryLine == newReceiveLine.PK).Single();
			newPickLine2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine2.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "B")
				.ExpectEquals(nameof(newPickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, orderKitLine1)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine3.PK)
				.ExpectEquals(nameof(pickLine3.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "B")
				.ExpectEquals(nameof(pickLine3.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine2.WE_FinalisedDate)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine4.PK)
				.ExpectEquals(nameof(pickLine4.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "B")
				.ExpectEquals(nameof(pickLine4.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine2.WE_FinalisedDate)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndUnfinalisedPicks_OneComponentIsStagedAndOneIsNot

		[TestDate(2024, 3, 18, 5, 6, 7)]
		public void TestBOMWithOrdersAndUnfinalisedPicks_OneComponentIsStagedAndOneIsNot()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 80m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 10m,
				WE_WL_TransferFrom = location1.PK,
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT",
				WE_DocketLineStatus = "HFT",
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 20m,
				WE_FinalisedDate = today.AddHours(4),
				WE_PutawayTime = today.AddHours(4),
				WE_GS_NKPutawayBy = "E",
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 20m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1, 10m) { WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine2, 20m) { WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate, null)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "ENT")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, null)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate, null)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate, null)
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 5, WhsPickLine.CountInDB(TestConnection));
			var newPickLine = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(newPickLine.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.ExpectEquals(nameof(newPickLine.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			AssertEquals("No new Transfer Lines created.", 2, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine3.PK)
				.ExpectEquals(nameof(pickLine3.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine3.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();

			WhsPickLine.AssertFromDB(TestConnection, pickLine4.PK)
				.ExpectEquals(nameof(pickLine4.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(pickLine4.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndFinalisedPicks

		public void TestBOMWithOrdersAndFinalisedPicks()
		{
			TestBOMWithOrdersAndFinalisedPicksCore(TransformationSection.OnlinePostUpgrade);
		}

		public void TestBOMWithOrdersAndFinalisedPicks_ShouldNotBeTransformedInOfflinePhase()
		{
			TestBOMWithOrdersAndFinalisedPicksCore(TransformationSection.OfflinePostUpgrade);
		}

		void TestBOMWithOrdersAndFinalisedPicksCore(TransformationSection transformationSection)
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 80m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "FIN")
			{
				WP_FinalizedDateUtc = new DateTime(2024, 3, 19, 10, 0, 0),
				WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0),
				WP_GS_NKFinalizedBy = "A",
				WP_SystemLastEditUser = "XYZ",
			}.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_WP = pick.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "ORZ",
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(3),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(4),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 20m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1, 10m) { WZ_PickedDateTime = today.AddHours(3), WZ_GS_NKAssignedTo = "C", WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine2, 20m) { WZ_PickedDateTime = today.AddHours(4), WZ_GS_NKAssignedTo = "D", WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(transformationSection, CancellationToken.None);
			GetNewTestTransformationInstance().Run(transformationSection, CancellationToken.None);

			if (transformationSection == TransformationSection.OnlinePostUpgrade)
			{
				AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
				var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
				newReceive.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
					.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
					.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive.FK), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
					.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.UtcDateTime, pick.WP_SystemCreateTimeUtc)
					.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
					.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
					.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "FIN")
					.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "XYZ")
					.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
					.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
					.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
					.VerifyAll();

				var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
				newReceiveLine.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
					.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
					.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
					.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
					.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
					.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
					.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
					.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, location1.PK)
					.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
					.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
					.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
					.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
					.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
					.VerifyAll();

				AssertEquals("New Transfer Lines created.", 3, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
				var newTransferLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_DocketLineType == "TFR" && l.WE_OP == kit.PK).Single();
				newTransferLine1.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(newTransferLine1.WE_WD), l => l.WE_WD.FK, transfer.PK)
					.ExpectEquals(nameof(newTransferLine1.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
					.ExpectEquals(nameof(newTransferLine1.WE_WL_TransferFrom), l => l.WE_WL_TransferFrom, location1.PK)
					.ExpectEquals(nameof(newTransferLine1.WE_WL), l => l.WE_WL, location2.PK)
					.ExpectEquals(nameof(newTransferLine1.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
					.ExpectEquals(nameof(newTransferLine1.WE_FinalisedDate), l => l.WE_FinalisedDate, transferLine2.WE_FinalisedDate)
					.ExpectEquals(nameof(newTransferLine1.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
					.ExpectEquals(nameof(newTransferLine1.WE_PutawayTime), l => l.WE_PutawayTime.Value.DateTime, transferLine2.WE_FinalisedDate)
					.ExpectEquals(nameof(newTransferLine1.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
					.VerifyAll();

				AssertEquals("New Pick Lines should be created.", 6, WhsPickLine.CountInDB(TestConnection));
				var newPickLine1 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
				newPickLine1.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(newPickLine1.WZ_Units), l => l.WZ_Units, 10m)
					.ExpectEquals(nameof(newPickLine1.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newTransferLine1.PK)
					.ExpectEquals(nameof(newPickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XYZ")
					.ExpectEquals(nameof(newPickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick.WP_FinalizedDateUtc)
					.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine.FK, newReceiveLine.PK)
					.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
					.VerifyAll();
				var newPickLine2 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_InventoryLine == newReceiveLine.PK).Single();
				newPickLine2.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(newPickLine2.WZ_Units), l => l.WZ_Units, 10m)
					.ExpectEquals(nameof(newPickLine2.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
					.ExpectEquals(nameof(newPickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "B")
					.ExpectEquals(nameof(newPickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine2.WE_FinalisedDate)
					.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
					.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, orderKitLine1)
					.VerifyAll();

				AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
				var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
				link1.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
					.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
					.VerifyAll();
				var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
				link2.BuildAssertion(TestConnection)
					.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
					.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
					.VerifyAll();

				WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
					.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location1.PK)
					.VerifyAll();

				WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
					.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location1.PK)
					.VerifyAll();
			}
			else
			{
				AssertEquals("No new Receive created.", 1, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
				AssertEquals("No new Receive Lines created.", 2, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "INW"));
				AssertEquals("No new Transfer Lines created.", 2, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			}
		}

		#endregion

		#region TestBOMWithOrdersAndFinalisedPicks_UpdateToLocationOfComponentTransferLine

		public void TestBOMWithOrdersAndFinalisedPicks_UpdateToLocationOfComponentTransferLine()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var location3 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 3 }.AppendInsertAndReturnObject(sql);
			var location4 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 4 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location2.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 80m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "FIN")
			{
				WP_FinalizedDateUtc = new DateTime(2024, 3, 19, 10, 0, 0),
				WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0),
				WP_GS_NKFinalizedBy = "A",
				WP_SystemLastEditUser = "XYZ",
			}.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_WP = pick.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "ORZ",
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location3.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(3),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 20m, location4.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(4),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location2.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 20m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1, 10m) { WZ_PickedDateTime = today.AddHours(3), WZ_GS_NKAssignedTo = "C", WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine2, 20m) { WZ_PickedDateTime = today.AddHours(4), WZ_GS_NKAssignedTo = "D", WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive.FK), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.UtcDateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "FIN")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "XYZ")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, location2.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.VerifyAll();

			AssertEquals("New Transfer Lines created.", 3, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			var newTransferLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_DocketLineType == "TFR" && l.WE_OP == kit.PK).Single();
			newTransferLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newTransferLine1.WE_WD), l => l.WE_WD.FK, transfer.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newTransferLine1.WE_WL_TransferFrom), l => l.WE_WL_TransferFrom, location2.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_WL), l => l.WE_WL, location4.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newTransferLine1.WE_FinalisedDate), l => l.WE_FinalisedDate, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newTransferLine1.WE_PutawayTime), l => l.WE_PutawayTime.Value.DateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 6, WhsPickLine.CountInDB(TestConnection));
			var newPickLine1 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine1.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newTransferLine1.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XYZ")
				.ExpectEquals(nameof(newPickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();
			var newPickLine2 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_InventoryLine == newReceiveLine.PK).Single();
			newPickLine2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine2.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "B")
				.ExpectEquals(nameof(newPickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, orderKitLine1)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndFinalisedPicks_LastPickedLocation

		public void TestBOMWithOrdersAndFinalisedPicks_LastPickedLocation()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var location3 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 3 }.AppendInsertAndReturnObject(sql);
			var location4 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 4 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location2.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 80m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "FIN")
			{
				WP_FinalizedDateUtc = new DateTime(2024, 3, 19, 10, 0, 0),
				WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0),
				WP_GS_NKFinalizedBy = "A",
				WP_SystemLastEditUser = "XYZ",
			}.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_WP = pick.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "ORZ",
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location3.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(3),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 20m, location4.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(4),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location2.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 20m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1, 10m) { WZ_PickedDateTime = today.AddHours(3), WZ_GS_NKAssignedTo = "C", WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine2, 20m) { WZ_PickedDateTime = today.AddHours(4), WZ_GS_NKAssignedTo = "D", WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "FIN")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "XYZ")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, location2.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.VerifyAll();

			AssertEquals("New Transfer Lines created.", 3, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			var newTransferLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_DocketLineType == "TFR" && l.WE_OP == kit.PK).Single();
			newTransferLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newTransferLine1.WE_WD), l => l.WE_WD.FK, transfer.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newTransferLine1.WE_WL_TransferFrom), l => l.WE_WL_TransferFrom, location2.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_WL), l => l.WE_WL, location4.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newTransferLine1.WE_FinalisedDate), l => l.WE_FinalisedDate, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newTransferLine1.WE_PutawayTime), l => l.WE_PutawayTime.Value.DateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 6, WhsPickLine.CountInDB(TestConnection));
			var newPickLine1 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine1.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newTransferLine1.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XYZ")
				.ExpectEquals(nameof(newPickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();
			var newPickLine2 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_InventoryLine == newReceiveLine.PK).Single();
			newPickLine2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine2.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "B")
				.ExpectEquals(nameof(newPickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, orderKitLine1)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
			.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
			.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
			.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndFinalisedPicks_BOMUnitConversion

		public void TestBOMWithOrdersAndFinalisedPicks_BOMUnitConversion()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1PartUnit1 = new OrgPartUnit(component1, 3, "UNT", "CAS").AppendInsertAndReturnObject(sql);
			var component1PartUnit2 = new OrgPartUnit(component1, 4, "CAS", "PLT").AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2PartUnit1 = new OrgPartUnit(component2, 4, "UNT", "CAS").AppendInsertAndReturnObject(sql);
			var component2PartUnit2 = new OrgPartUnit(component2, 3, "CAS", "PLT").AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_IsValid = true, OE_F3_NKPackType = "PLT" }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 1, OE_IsValid = true, OE_F3_NKPackType = "PLT" }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 12m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 12m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "FIN")
			{
				WP_FinalizedDateUtc = new DateTime(2024, 3, 19, 10, 0, 0),
				WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0),
				WP_GS_NKFinalizedBy = "A",
				WP_SystemLastEditUser = "XYZ",
			}.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_WP = pick.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "ORZ",
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 1m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 12m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
				WE_F3_NKPackType = "PLT",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 12m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
				WE_F3_NKPackType = "PLT",
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 12m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(3),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 12m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(4),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 12m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 12m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1, 12m) { WZ_PickedDateTime = today.AddHours(3), WZ_GS_NKAssignedTo = "C", WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine2, 12m) { WZ_PickedDateTime = today.AddHours(4), WZ_GS_NKAssignedTo = "D", WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "FIN")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "XYZ")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 1m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 1m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, location1.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.VerifyAll();

			AssertEquals("New Transfer Lines created.", 3, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			var newTransferLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_DocketLineType == "TFR" && l.WE_OP == kit.PK).Single();
			newTransferLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newTransferLine1.WE_WD), l => l.WE_WD.FK, transfer.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 1m)
				.ExpectEquals(nameof(newTransferLine1.WE_WL_TransferFrom), l => l.WE_WL_TransferFrom, location1.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newTransferLine1.WE_FinalisedDate), l => l.WE_FinalisedDate, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newTransferLine1.WE_PutawayTime), l => l.WE_PutawayTime.Value.DateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 6, WhsPickLine.CountInDB(TestConnection));
			var newPickLine1 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine1.WZ_Units), l => l.WZ_Units, 1m)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newTransferLine1.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XYZ")
				.ExpectEquals(nameof(newPickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();
			var newPickLine2 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == newTransferLine1.PK).Single();
			newPickLine2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine2.WZ_Units), l => l.WZ_Units, 1m)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "B")
				.ExpectEquals(nameof(newPickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, orderKitLine1)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 12m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 12m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndFinalisedPicks_BOMUnitConversion_ByPackType

		public void TestBOMWithOrdersAndFinalisedPicks_BOMUnitConversion_ByPackType()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1PartUnit = new OrgPartUnit(component1, 3, "UNT", "CAS").AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_IsValid = true, OE_F3_NKPackType = "CAS" }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 4, OE_IsValid = true, OE_F3_NKPackType = "UNT" }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 10m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 6m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "FIN")
			{
				WP_FinalizedDateUtc = new DateTime(2024, 3, 19, 10, 0, 0),
				WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0),
				WP_SystemLastEditUser = "XYZ",
				WP_GS_NKFinalizedBy = "A",
			}.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_WP = pick.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "ORZ",
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 1m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 4m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
				WE_F3_NKPackType = "UNT"
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 4m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(3),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 4m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(transferLine1, orderComponentLine1, 4m) { WZ_PickedDateTime = today.AddHours(3), WZ_GS_NKAssignedTo = "C", WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "FIN")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "XYZ")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 1m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 1m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, location1.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.VerifyAll();

			AssertEquals("New Transfer Lines created.", 2, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			var newTransferLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_DocketLineType == "TFR" && l.WE_OP == kit.PK).Single();
			newTransferLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newTransferLine1.WE_WD), l => l.WE_WD.FK, transfer.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 1m)
				.ExpectEquals(nameof(newTransferLine1.WE_WL_TransferFrom), l => l.WE_WL_TransferFrom, location1.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newTransferLine1.WE_FinalisedDate), l => l.WE_FinalisedDate, transferLine1.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newTransferLine1.WE_PutawayTime), l => l.WE_PutawayTime.Value.DateTime, transferLine1.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 4, WhsPickLine.CountInDB(TestConnection));
			var newPickLine1 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine1.WZ_Units), l => l.WZ_Units, 1m)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newTransferLine1.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XYZ")
				.ExpectEquals(nameof(newPickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();
			var newPickLine2 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == newTransferLine1.PK).Single();
			newPickLine2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine2.WZ_Units), l => l.WZ_Units, 1m)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "A")
				.ExpectEquals(nameof(newPickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine1.WE_FinalisedDate)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, orderKitLine1)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 1, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 4m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndFinalisedPicks_FinalisedWithoutPicking

		public void TestBOMWithOrdersAndFinalisedPicks_FinalisedWithoutPicking()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 80m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "FIN")
			{
				WP_FinalizedDateUtc = new DateTime(2024, 3, 19, 10, 0, 0),
				WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0),
				WP_GS_NKFinalizedBy = "A",
				WP_SystemLastEditUser = "XYZ",
			}.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_WP = pick.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "ORZ",
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, orderComponentLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, orderComponentLine2, 20m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive.FK), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.UtcDateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "FIN")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "XYZ")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, location1.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.VerifyAll();

			AssertEquals("No Transfer Lines created.", 0, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));

			AssertEquals("New Pick Line should be created.", 3, WhsPickLine.CountInDB(TestConnection));
			var newPickLine1 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine1.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XYZ")
				.ExpectEquals(nameof(newPickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
		}

		#endregion

		#region TestBOMWithOrdersAndFinalisedPicks_FinalisedWithSomeLinesStaged

		public void TestBOMWithOrdersAndFinalisedPicks_FinalisedWithSomeLinesStaged()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 80m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "FIN")
			{
				WP_FinalizedDateUtc = new DateTime(2024, 3, 19, 10, 0, 0),
				WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0),
				WP_GS_NKFinalizedBy = "A",
				WP_SystemLastEditUser = "XYZ",
			}.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_WP = pick.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "ORZ",
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(3),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 8m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(4),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 8m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1, 10m) { WZ_PickedDateTime = today.AddHours(3), WZ_GS_NKAssignedTo = "C", WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine2, 8m) { WZ_PickedDateTime = today.AddHours(4), WZ_GS_NKAssignedTo = "D", WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);
			var pickLine5 = new WhsPickLine(receiveLine2, orderComponentLine2, 12m) { WZ_PickedDateTime = today.AddHours(5), WZ_GS_NKAssignedTo = "D" }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive.FK), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.UtcDateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "FIN")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "XYZ")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, location1.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.VerifyAll();

			AssertEquals("No new Transfer Lines created.", 2, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));

			AssertEquals("New Pick Line should be created.", 6, WhsPickLine.CountInDB(TestConnection));
			var newPickLine1 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine1.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XYZ")
				.ExpectEquals(nameof(newPickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
		}

		#endregion

		#region TestMultipleOrdersFromSameClientOnOnePick

		public void TestMultipleOrdersFromSameClientOnOnePick()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 87m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 74m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "FIN")
			{
				WP_FinalizedDateUtc = new DateTime(2024, 3, 19, 10, 0, 0),
				WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0),
				WP_GS_NKFinalizedBy = "A",
				WP_SystemLastEditUser = "XYZ",
			}.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "Y",
				WD_WP = pick.PK
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order1, kit.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1_1 = new WhsDocketLine(order1, component1.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1_2 = new WhsDocketLine(order1, component2.PK, 20m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1
			}.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O2")
			{
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "Y",
				WD_WP = pick.PK
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine2 = new WhsDocketLine(order2, kit.PK, 3m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine2_1 = new WhsDocketLine(order2, component1.PK, 3m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine2
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine2_2 = new WhsDocketLine(order2, component2.PK, 6m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine2
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(1),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(2),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);
			var transferLine3 = new WhsDocketLine(transfer, component1.PK, 3m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(3),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);
			var transferLine4 = new WhsDocketLine(transfer, component2.PK, 6m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(4),
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 20m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1_1, 10m) { WZ_PickedDateTime = today.AddHours(3), WZ_GS_NKAssignedTo = "C", WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine1_2, 20m) { WZ_PickedDateTime = today.AddHours(4), WZ_GS_NKAssignedTo = "D", WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);
			var pickLine5 = new WhsPickLine(receiveLine1, transferLine3, 3m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine6 = new WhsPickLine(receiveLine2, transferLine4, 6m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine7 = new WhsPickLine(transferLine3, orderComponentLine2_1, 3m) { WZ_PickedDateTime = today.AddHours(3), WZ_GS_NKAssignedTo = "C", WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine8 = new WhsPickLine(transferLine4, orderComponentLine2_2, 6m) { WZ_PickedDateTime = today.AddHours(4), WZ_GS_NKAssignedTo = "D", WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order1.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order1.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order1.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "FIN")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "XYZ")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLines = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK);
			var newReceiveLine1 = newReceiveLines.Single(l => l.WE_TransactionQuantity == 10m);
			newReceiveLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine1.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine1.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.ExpectEquals(nameof(newReceiveLine1.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine1.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine1.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine1.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine1.WE_WL), l => l.WE_WL, location1.PK)
				.ExpectEquals(nameof(newReceiveLine1.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine1.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine1.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine1.PK)
				.ExpectEquals(nameof(newReceiveLine1.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceiveLine1.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.VerifyAll();

			var newReceiveLine2 = newReceiveLines.Single(l => l.WE_TransactionQuantity == 3m);
			newReceiveLine2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine2.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine2.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.ExpectEquals(nameof(newReceiveLine2.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine2.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine2.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 3m)
				.ExpectEquals(nameof(newReceiveLine2.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine2.WE_WL), l => l.WE_WL, location1.PK)
				.ExpectEquals(nameof(newReceiveLine2.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine2.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine2.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine2.PK)
				.ExpectEquals(nameof(newReceiveLine2.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceiveLine2.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.VerifyAll();

			AssertEquals("New Transfer Lines created.", 6, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			var newTransferLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_DocketLineType == "TFR" && l.WE_OP == kit.PK && l.WE_TransactionQuantity == 10m).Single();
			newTransferLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newTransferLine1.WE_WD), l => l.WE_WD.FK, transfer.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_WL_TransferFrom), l => l.WE_WL_TransferFrom, location1.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newTransferLine1.WE_FinalisedDate), l => l.WE_FinalisedDate, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newTransferLine1.WE_PutawayTime), l => l.WE_PutawayTime.Value.DateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.VerifyAll();

			var newTransferLine2 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_DocketLineType == "TFR" && l.WE_OP == kit.PK && l.WE_TransactionQuantity == 3m).Single();
			newTransferLine2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newTransferLine2.WE_WD), l => l.WE_WD.FK, transfer.PK)
				.ExpectEquals(nameof(newTransferLine2.WE_WL_TransferFrom), l => l.WE_WL_TransferFrom, location1.PK)
				.ExpectEquals(nameof(newTransferLine2.WE_WL), l => l.WE_WL, location2.PK)
				.ExpectEquals(nameof(newTransferLine2.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newTransferLine2.WE_FinalisedDate), l => l.WE_FinalisedDate, transferLine4.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine2.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newTransferLine2.WE_PutawayTime), l => l.WE_PutawayTime.Value.DateTime, transferLine4.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine2.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 12, WhsPickLine.CountInDB(TestConnection));
			var newPickLine1 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine1.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newTransferLine1.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XYZ")
				.ExpectEquals(nameof(newPickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine.FK, newReceiveLine1.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();
			var newPickLine2 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_InventoryLine == newReceiveLine1.PK).Single();
			newPickLine2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine2.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine1.PK)
				.ExpectEquals(nameof(newPickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "B")
				.ExpectEquals(nameof(newPickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, orderKitLine1)
				.VerifyAll();

			var newPickLine3 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine2.PK).Single();
			newPickLine3.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine3.WZ_Units), l => l.WZ_Units, 3m)
				.ExpectEquals(nameof(newPickLine3.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newTransferLine2.PK)
				.ExpectEquals(nameof(newPickLine3.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XYZ")
				.ExpectEquals(nameof(newPickLine3.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newPickLine3.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine.FK, newReceiveLine2.PK)
				.ExpectEquals(nameof(newPickLine3.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();
			var newPickLine4 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_InventoryLine == newReceiveLine2.PK).Single();
			newPickLine4.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine4.WZ_Units), l => l.WZ_Units, 3m)
				.ExpectEquals(nameof(newPickLine4.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine2.PK)
				.ExpectEquals(nameof(newPickLine4.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "B")
				.ExpectEquals(nameof(newPickLine4.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine4.WE_FinalisedDate)
				.ExpectEquals(nameof(newPickLine4.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine4.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, orderKitLine2)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 4, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1_1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine1.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1_2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine1.PK)
				.VerifyAll();
			var link3 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2_1.PK).Single();
			link3.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link3.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 3m)
				.ExpectEquals(nameof(link3.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine2.PK)
				.VerifyAll();
			var link4 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2_2.PK).Single();
			link4.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link4.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 6m)
				.ExpectEquals(nameof(link4.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine2.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine3.PK)
				.ExpectEquals(nameof(transferLine3.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine4.PK)
				.ExpectEquals(nameof(transferLine4.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();
		}

		#endregion

		#region TestMultipleOrdersFromSameClientOnMultiplePicks

		public void TestMultipleOrdersFromSameClientOnMultiplePicks()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 87m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 74m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs, "PICK1", "FIN")
			{
				WP_FinalizedDateUtc = new DateTime(2024, 3, 19, 10, 0, 0),
				WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0),
				WP_GS_NKFinalizedBy = "A",
				WP_SystemLastEditUser = "XY1",
			}.AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "PICK2", "FIN")
			{
				WP_FinalizedDateUtc = new DateTime(2024, 3, 19, 11, 0, 0),
				WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0),
				WP_GS_NKFinalizedBy = "A",
				WP_SystemLastEditUser = "XY2",
			}.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "OR1",
				WD_WP = pick1.PK,
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order1, kit.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1_1 = new WhsDocketLine(order1, component1.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1_2 = new WhsDocketLine(order1, component2.PK, 20m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1
			}.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O2")
			{
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "OR2",
				WD_WP = pick2.PK
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine2 = new WhsDocketLine(order2, kit.PK, 3m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine2_1 = new WhsDocketLine(order2, component1.PK, 3m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine2
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine2_2 = new WhsDocketLine(order2, component2.PK, 6m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine2
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = location1.PK,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 20m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = location1.PK,
				WE_FinalisedDate = today.AddHours(1),
			}.AppendInsertAndReturnObject(sql);
			var transferLine3 = new WhsDocketLine(transfer, component1.PK, 3m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = location1.PK,
				WE_FinalisedDate = today.AddHours(2),
			}.AppendInsertAndReturnObject(sql);
			var transferLine4 = new WhsDocketLine(transfer, component2.PK, 6m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = location1.PK,
				WE_FinalisedDate = today.AddHours(3),
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 20m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1_1, 10m) { WZ_PickedDateTime = today.AddHours(3), WZ_GS_NKAssignedTo = "C", WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine1_2, 20m) { WZ_PickedDateTime = today.AddHours(4), WZ_GS_NKAssignedTo = "D", WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);
			var pickLine5 = new WhsPickLine(receiveLine1, transferLine3, 3m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine6 = new WhsPickLine(receiveLine2, transferLine4, 6m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine7 = new WhsPickLine(transferLine3, orderComponentLine2_1, 3m) { WZ_PickedDateTime = today.AddHours(3), WZ_GS_NKAssignedTo = "C", WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine8 = new WhsPickLine(transferLine4, orderComponentLine2_2, 6m) { WZ_PickedDateTime = today.AddHours(4), WZ_GS_NKAssignedTo = "D", WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 3, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive1 = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.WD_WP_ParentPickForReceive == pick1.PK).Single();
			newReceive1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive1.WD_OH_Client), r => r.WD_OH_Client, order1.WD_OH_Client)
				.ExpectEquals(nameof(newReceive1.WD_WW_Whs), r => r.WD_WW_Whs, order1.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive1.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick1.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive1.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick1.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive1.WD_FinalisedDate), r => r.WD_FinalisedDate.Value.DateTime, pick1.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceive1.WD_DocketStatus), r => r.WD_DocketStatus, "FIN")
				.ExpectEquals(nameof(newReceive1.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "XY1")
				.ExpectEquals(nameof(newReceive1.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive1.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive1.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceive2 = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.WD_WP_ParentPickForReceive == pick2.PK).Single();
			newReceive2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive2.WD_OH_Client), r => r.WD_OH_Client, order1.WD_OH_Client)
				.ExpectEquals(nameof(newReceive2.WD_WW_Whs), r => r.WD_WW_Whs, order1.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive2.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick2.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive2.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick2.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive2.WD_FinalisedDate), r => r.WD_FinalisedDate.Value.DateTime, pick2.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceive2.WD_DocketStatus), r => r.WD_DocketStatus, "FIN")
				.ExpectEquals(nameof(newReceive2.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "XY2")
				.ExpectEquals(nameof(newReceive2.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive2.WD_DocketID), r => r.WD_DocketID, "PICK2")
				.ExpectEquals(nameof(newReceive2.WD_ExternalReference), r => r.WD_ExternalReference, "PICK2")
				.VerifyAll();

			var newReceiveLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive1.PK).Single();
			newReceiveLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine1.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine1.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.ExpectEquals(nameof(newReceiveLine1.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine1.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine1.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine1.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine1.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine1.WE_WL), l => l.WE_WL, location1.PK)
				.ExpectEquals(nameof(newReceiveLine1.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine1.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine1.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine1.PK)
				.ExpectEquals(nameof(newReceiveLine1.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick1.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceiveLine1.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick1.WP_SystemCreateTimeUtc)
				.VerifyAll();

			var newReceiveLine2 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive2.PK).Single();
			newReceiveLine2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine2.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine2.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.ExpectEquals(nameof(newReceiveLine2.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine2.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine2.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 3m)
				.ExpectEquals(nameof(newReceiveLine2.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 3m)
				.ExpectEquals(nameof(newReceiveLine2.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine2.WE_WL), l => l.WE_WL, location1.PK)
				.ExpectEquals(nameof(newReceiveLine2.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine2.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine2.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine2.PK)
				.ExpectEquals(nameof(newReceiveLine2.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick2.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceiveLine2.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick2.WP_SystemCreateTimeUtc)
				.VerifyAll();

			AssertEquals("New Transfer Lines created.", 6, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			var newTransferLine1 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_DocketLineType == "TFR" && l.WE_OP == kit.PK && l.WE_TransactionQuantity == 10m).Single();
			newTransferLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newTransferLine1.WE_WD), l => l.WE_WD.FK, transfer.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_WL_TransferFrom), l => l.WE_WL_TransferFrom, location1.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.ExpectEquals(nameof(newTransferLine1.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newTransferLine1.WE_FinalisedDate), l => l.WE_FinalisedDate, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick1.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newTransferLine1.WE_PutawayTime), l => l.WE_PutawayTime.Value.DateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine1.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.VerifyAll();

			var newTransferLine2 = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_DocketLineType == "TFR" && l.WE_OP == kit.PK && l.WE_TransactionQuantity == 3m).Single();
			newTransferLine2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newTransferLine2.WE_WD), l => l.WE_WD.FK, transfer.PK)
				.ExpectEquals(nameof(newTransferLine2.WE_WL_TransferFrom), l => l.WE_WL_TransferFrom, location1.PK)
				.ExpectEquals(nameof(newTransferLine2.WE_WL), l => l.WE_WL, location2.PK)
				.ExpectEquals(nameof(newTransferLine2.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newTransferLine2.WE_FinalisedDate), l => l.WE_FinalisedDate, transferLine4.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine2.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick2.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newTransferLine2.WE_PutawayTime), l => l.WE_PutawayTime.Value.DateTime, transferLine4.WE_FinalisedDate)
				.ExpectEquals(nameof(newTransferLine2.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 12, WhsPickLine.CountInDB(TestConnection));
			var newPickLine1 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine1.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newTransferLine1.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XY1")
				.ExpectEquals(nameof(newPickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick1.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine.FK, newReceiveLine1.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();
			var newPickLine2 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_InventoryLine == newReceiveLine1.PK).Single();
			newPickLine2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine2.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine1.PK)
				.ExpectEquals(nameof(newPickLine2.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "B")
				.ExpectEquals(nameof(newPickLine2.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine2.WE_FinalisedDate)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine2.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, orderKitLine1)
				.VerifyAll();

			var newPickLine3 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine2.PK).Single();
			newPickLine3.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine3.WZ_Units), l => l.WZ_Units, 3m)
				.ExpectEquals(nameof(newPickLine3.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newTransferLine2.PK)
				.ExpectEquals(nameof(newPickLine3.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XY2")
				.ExpectEquals(nameof(newPickLine3.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick2.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newPickLine3.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine.FK, newReceiveLine2.PK)
				.ExpectEquals(nameof(newPickLine3.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();
			var newPickLine4 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_InventoryLine == newReceiveLine2.PK).Single();
			newPickLine4.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine4.WZ_Units), l => l.WZ_Units, 3m)
				.ExpectEquals(nameof(newPickLine4.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine2.PK)
				.ExpectEquals(nameof(newPickLine4.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "B")
				.ExpectEquals(nameof(newPickLine4.WZ_PickedDateTime), l => l.WZ_PickedDateTime, transferLine4.WE_FinalisedDate)
				.ExpectEquals(nameof(newPickLine4.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine4.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, orderKitLine2)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 4, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1_1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine1.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1_2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine1.PK)
				.VerifyAll();
			var link3 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2_1.PK).Single();
			link3.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link3.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 3m)
				.ExpectEquals(nameof(link3.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine2.PK)
				.VerifyAll();
			var link4 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2_2.PK).Single();
			link4.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link4.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 6m)
				.ExpectEquals(nameof(link4.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine2.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine3.PK)
				.ExpectEquals(nameof(transferLine3.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine4.PK)
				.ExpectEquals(nameof(transferLine4.WE_WL), l => l.WE_WL, location1.PK)
				.VerifyAll();
		}

		#endregion

		#region TestOrderHasSomeLinesStaged

		public void TestOrderHasSomeLinesStaged()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 85m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "NEW") { WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0) }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m) { WE_WE_ParentDocketLine = orderKitLine1 }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, component1.PK, 10m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(3),
				WE_PutawayTime = today.AddHours(4),
				WE_GS_NKPutawayBy = "E",
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, component2.PK, 15m, location2.PK)
			{
				WE_StockOnHand = 0m,
				WE_FinalisedDate = today.AddHours(3),
				WE_PutawayTime = today.AddHours(4),
				WE_GS_NKPutawayBy = "E",
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = location1.PK,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, transferLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, transferLine2, 15m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);
			var pickLine3 = new WhsPickLine(transferLine1, orderComponentLine1, 10m) { WZ_PickedDateTime = today.AddHours(3), WZ_GS_NKAssignedTo = "C", WZ_WE_OriginalPickedInventoryLine = receiveLine1 }.AppendInsertAndReturnObject(sql);
			var pickLine4 = new WhsPickLine(transferLine2, orderComponentLine2, 15m) { WZ_PickedDateTime = today.AddHours(4), WZ_GS_NKAssignedTo = "D", WZ_WE_OriginalPickedInventoryLine = receiveLine2 }.AppendInsertAndReturnObject(sql);
			var pickLine5 = new WhsPickLine(receiveLine2, orderComponentLine2, 5m).AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run();
			GetNewTestTransformationInstance().Run();

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate, null)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "ENT")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, null)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate, null)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate, null)
				.VerifyAll();

			AssertEquals("New Pick Lines should be created.", 6, WhsPickLine.CountInDB(TestConnection));
			var newPickLine = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "")
				.ExpectEquals(nameof(newPickLine.WZ_PickedDateTime), l => l.WZ_PickedDateTime, null)
				.ExpectEquals(nameof(newPickLine.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();

			AssertEquals("No new Transfer Lines created.", 2, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));

			WhsDocketLine.AssertFromDB(TestConnection, transferLine1.PK)
				.ExpectEquals(nameof(transferLine1.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, transferLine2.PK)
				.ExpectEquals(nameof(transferLine2.WE_WL), l => l.WE_WL, location2.PK)
				.VerifyAll();
		}

		#endregion

		#region TestOnlinePhase_Batching

		public void TestOnlinePhase_Batching_0Records()
		{
			TestOnlinePhase_BatchingCore(numberOfRecords: 0);
		}

		public void TestOnlinePhase_Batching_100Records()
		{
			TestOnlinePhase_BatchingCore(numberOfRecords: 100);
		}

		public void TestOnlinePhase_Batching_1000Records()
		{
			TestOnlinePhase_BatchingCore(numberOfRecords: 1000);
		}

		public void TestOnlinePhase_Batching_1001Records()
		{
			TestOnlinePhase_BatchingCore(numberOfRecords: 1001);
		}

		public void TestOnlinePhase_Batching_2001Records()
		{
			TestOnlinePhase_BatchingCore(numberOfRecords: 2001);
		}

		public void TestOnlinePhase_Batching_NoDataToProcessInSomeWeeks()
		{
			TestOnlinePhase_BatchingCore(numberOfRecords: 20, hourInterval: 400);
		}

		void TestOnlinePhase_BatchingCore(int numberOfRecords, int hourInterval = 1)
		{
			var date = new DateTime(2018, 1, 1);
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var receives = new WhsDocket[numberOfRecords];
			var receiveLines = new WhsDocketLine[numberOfRecords];
			var picks = new WhsPick[numberOfRecords];
			var orders = new WhsDocket[numberOfRecords];
			var orderKitLines = new WhsDocketLine[numberOfRecords];
			var orderComponentLines = new WhsDocketLine[numberOfRecords];
			var transfers = new WhsDocket[numberOfRecords];
			var transferLines = new WhsDocketLine[numberOfRecords];
			var pickLines1 = new WhsPickLine[numberOfRecords];
			var pickLines2 = new WhsPickLine[numberOfRecords];

			for (var i = 0; i < numberOfRecords; i++)
			{
				receives[i] = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"R{i}") { WD_FinalisedDate = date };
				receiveLines[i] = new WhsDocketLine(receives[i], component1.PK, 2m, location1.PK)
				{
					WE_OriginalInventoryStatus = "AVL",
					WE_CurrentInventoryStatus = "AVL",
					WE_StockOnHand = 0m,
					WE_FinalisedDate = date,
				};
				picks[i] = new WhsPick(whs, $"PICK{i}", "FIN")
				{
					WP_FinalizedDateUtc = date,
					WP_SystemCreateTimeUtc = date,
					WP_GS_NKFinalizedBy = "A",
					WP_SystemLastEditUser = "XYZ",
				};
				orders[i] = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", $"O{i}")
				{
					WD_WP = picks[i].PK,
					WD_FinalisedDate = date,
					WD_GS_NKFinalizedBy = "ORZ",
				};
				orderKitLines[i] = new WhsDocketLine(orders[i], kit.PK, 1m)
				{
					WE_FinalisedDate = date,
					WE_DocketLineStatus = "DEP",
				};
				orderComponentLines[i] = new WhsDocketLine(orders[i], component1.PK, 2m)
				{
					WE_FinalisedDate = date,
					WE_DocketLineStatus = "DEP",
					WE_WE_ParentDocketLine = orderKitLines[i],
				};
				transfers[i] = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", $"T{i}") { WD_FinalisedDate = date };
				transferLines[i] = new WhsDocketLine(transfers[i], component1.PK, 2m, location2.PK)
				{
					WE_StockOnHand = 0m,
					WE_FinalisedDate = date.AddHours(3),
					WE_DocketLineStatus = "FIN",
					WE_WL_TransferFrom = location1.PK,
				};
				pickLines1[i] = new WhsPickLine(receiveLines[i], transferLines[i], 2m) { WZ_PickedDateTime = date.AddHours(1), WZ_GS_NKAssignedTo = "A" };
				pickLines2[i] = new WhsPickLine(transferLines[i], orderComponentLines[i], 2m) { WZ_PickedDateTime = date.AddHours(1), WZ_GS_NKAssignedTo = "A", WZ_WE_OriginalPickedInventoryLine = receiveLines[i] };
				date = date.AddHours(hourInterval);
			}

			if (numberOfRecords > 0)
			{
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
				using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
				{
					TestConnection.ExecuteNonQuery(WhsDocket.GetBulkInsertStatement(receives));
					TestConnection.ExecuteNonQuery(WhsDocketLine.GetBulkInsertStatement(receiveLines));
					TestConnection.ExecuteNonQuery(WhsPick.GetBulkInsertStatement(picks));
					TestConnection.ExecuteNonQuery(WhsDocket.GetBulkInsertStatement(orders));
					TestConnection.ExecuteNonQuery(WhsDocketLine.GetBulkInsertStatement(orderKitLines));
					TestConnection.ExecuteNonQuery(WhsDocketLine.GetBulkInsertStatement(orderComponentLines));
					TestConnection.ExecuteNonQuery(WhsDocket.GetBulkInsertStatement(transfers));
					TestConnection.ExecuteNonQuery(WhsDocketLine.GetBulkInsertStatement(transferLines));
					TestConnection.ExecuteNonQuery(WhsPickLine.GetBulkInsertStatement(pickLines1));
					TestConnection.ExecuteNonQuery(WhsPickLine.GetBulkInsertStatement(pickLines2));
				}
			}

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receives should be created.", 2 * numberOfRecords, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			AssertEquals("New Receive Lines should be created.", 2 * numberOfRecords, WhsDocketLine.CountInDB(TestConnection, d => d.WE_DocketLineType == "INW"));
			AssertEquals("New Transfer Lines should be created.", 2 * numberOfRecords, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			AssertEquals("New Pick Lines should be created.", 4 * numberOfRecords, WhsPickLine.CountInDB(TestConnection));
			AssertEquals("New Links should be created.", numberOfRecords, WhsBOMInventoryPivot.CountInDB(TestConnection));
		}

		#endregion

		#region TestOnlinePhase_RespondToCancellationToken

		public void TestOnlinePhase_RespondToCancellationToken()
		{
			var numberOfRecords = 1234;
			var date = new DateTime(2018, 1, 1);
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var receives = new WhsDocket[numberOfRecords];
			var receiveLines = new WhsDocketLine[numberOfRecords];
			var picks = new WhsPick[numberOfRecords];
			var orders = new WhsDocket[numberOfRecords];
			var orderKitLines = new WhsDocketLine[numberOfRecords];
			var orderComponentLines = new WhsDocketLine[numberOfRecords];
			var transfers = new WhsDocket[numberOfRecords];
			var transferLines = new WhsDocketLine[numberOfRecords];
			var pickLines1 = new WhsPickLine[numberOfRecords];
			var pickLines2 = new WhsPickLine[numberOfRecords];

			for (var i = 0; i < numberOfRecords; i++)
			{
				receives[i] = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"R{i}") { WD_FinalisedDate = date };
				receiveLines[i] = new WhsDocketLine(receives[i], component1.PK, 2m, location1.PK)
				{
					WE_OriginalInventoryStatus = "AVL",
					WE_CurrentInventoryStatus = "AVL",
					WE_StockOnHand = 0m,
					WE_FinalisedDate = date,
				};
				picks[i] = new WhsPick(whs, $"PICK{i}", "FIN")
				{
					WP_FinalizedDateUtc = date,
					WP_SystemCreateTimeUtc = date,
					WP_GS_NKFinalizedBy = "A",
					WP_SystemLastEditUser = "XYZ",
				};
				orders[i] = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", $"O{i}")
				{
					WD_WP = picks[i].PK,
					WD_FinalisedDate = date,
					WD_GS_NKFinalizedBy = "ORZ",
				};
				orderKitLines[i] = new WhsDocketLine(orders[i], kit.PK, 1m)
				{
					WE_FinalisedDate = date,
					WE_DocketLineStatus = "DEP",
				};
				orderComponentLines[i] = new WhsDocketLine(orders[i], component1.PK, 2m)
				{
					WE_FinalisedDate = date,
					WE_DocketLineStatus = "DEP",
					WE_WE_ParentDocketLine = orderKitLines[i],
				};
				transfers[i] = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", $"T{i}") { WD_FinalisedDate = date };
				transferLines[i] = new WhsDocketLine(transfers[i], component1.PK, 2m, location2.PK)
				{
					WE_StockOnHand = 0m,
					WE_FinalisedDate = date.AddHours(3),
					WE_DocketLineStatus = "FIN",
					WE_WL_TransferFrom = location1.PK,
				};
				pickLines1[i] = new WhsPickLine(receiveLines[i], transferLines[i], 2m) { WZ_PickedDateTime = date.AddHours(1), WZ_GS_NKAssignedTo = "A" };
				pickLines2[i] = new WhsPickLine(transferLines[i], orderComponentLines[i], 2m) { WZ_PickedDateTime = date.AddHours(1), WZ_GS_NKAssignedTo = "A", WZ_WE_OriginalPickedInventoryLine = receiveLines[i] };
				date = date.AddHours(1);
			}

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(WhsDocket.GetBulkInsertStatement(receives));
				TestConnection.ExecuteNonQuery(WhsDocketLine.GetBulkInsertStatement(receiveLines));
				TestConnection.ExecuteNonQuery(WhsPick.GetBulkInsertStatement(picks));
				TestConnection.ExecuteNonQuery(WhsDocket.GetBulkInsertStatement(orders));
				TestConnection.ExecuteNonQuery(WhsDocketLine.GetBulkInsertStatement(orderKitLines));
				TestConnection.ExecuteNonQuery(WhsDocketLine.GetBulkInsertStatement(orderComponentLines));
				TestConnection.ExecuteNonQuery(WhsDocket.GetBulkInsertStatement(transfers));
				TestConnection.ExecuteNonQuery(WhsDocketLine.GetBulkInsertStatement(transferLines));
				TestConnection.ExecuteNonQuery(WhsPickLine.GetBulkInsertStatement(pickLines1));
				TestConnection.ExecuteNonQuery(WhsPickLine.GetBulkInsertStatement(pickLines2));
			}

			AssertEquals("Precondition: normal receives.", 1234, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			AssertEquals("Precondition: normal receive lines.", 1234, WhsDocketLine.CountInDB(TestConnection, d => d.WE_DocketLineType == "INW"));
			AssertEquals("Precondition: normal transfer lines.", 1234, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			AssertEquals("Precondition: normal pick lines.", 2468, WhsPickLine.CountInDB(TestConnection));

			AssertExceptionThrown<OperationCanceledException>(() => GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(true)));

			AssertEquals("First batch completed, processed data within 1 week, 7 * 24 = 168.", 168, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW" && d.WD_WP_ParentPickForReceive != null));
			AssertEquals("First batch completed, 168 newly created + 1234 normal receives.", 1402, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			AssertEquals("First batch completed.", 1402, WhsDocketLine.CountInDB(TestConnection, d => d.WE_DocketLineType == "INW"));
			AssertEquals("First batch completed.", 1402, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			AssertEquals("First batch completed, 168 *2 + 1234.", 2804, WhsPickLine.CountInDB(TestConnection));
			AssertEquals("First batch completed.", 168, WhsBOMInventoryPivot.CountInDB(TestConnection));

			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(false)));

			AssertEquals("Second batch completed.", 2468, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			AssertEquals("Second batch completed.", 2468, WhsDocketLine.CountInDB(TestConnection, d => d.WE_DocketLineType == "INW"));
			AssertEquals("Second batch completed.", 2468, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));
			AssertEquals("Second batch completed.", 4936, WhsPickLine.CountInDB(TestConnection));
			AssertEquals("Second batch completed.", 1234, WhsBOMInventoryPivot.CountInDB(TestConnection));
		}

		#endregion

		#region SaveToDB

		void SaveToDB(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion

		#region TestUserDescription

		public void TestUserDescription()
		{
			AssertEquals("Populate data for Pick By BOM refactor.", GetNewTestTransformationInstance().UserDescription);
		}

		#endregion

		#region TestPopulateWhsPickByBOMData_Index

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Populate data for Pick By BOM refactor._1] ON [dbo].[WhsPick] ([WP_PickStatus]) INCLUDE ([WP_FinalizedDateUtc], [WP_PickNo], [WP_PK], [WP_SystemCreateTimeUtc], [WP_SystemLastEditUser]) WHERE ([WP_PickStatus]<>'FIN' AND [WP_PickStatus]<>'CAN') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Populate data for Pick By BOM refactor._2] ON [dbo].[WhsPickLine] ([WZ_WE_TransactionLine]) INCLUDE ([WZ_GS_NKAssignedTo], [WZ_PickedDateTime], [WZ_Units], [WZ_WE_InventoryLine], [WZ_WE_OriginalPickedInventoryLine]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
#if NETFRAMEWORK
			"NONCLUSTERED INDEX [_WTG__Populate data for Pick By BOM refactor._3] ON [dbo].[WhsDocketLine] ([WE_PK]) INCLUDE ([WE_DocketLineStatus], [WE_FinalisedDate], [WE_WD], [WE_WL], [WE_WL_TransferFrom]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
#else
			"NONCLUSTERED INDEX [_WTG__Populate data for Pick By BOM refactor._3] ON [dbo].[WhsDocketLine] ([WE_PK]) INCLUDE ([WE_DocketLineStatus], [WE_FinalisedDate], [WE_WD], [WE_WL_TransferFrom], [WE_WL]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
#endif
		};

		#endregion

		#region Implementation

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new PopulateWhsPickByBOMData();
		}

		#endregion
	}

	[UseSnapshotProtection]
	class PopulateWhsPickByBOMDataTestNonTransactioned : TestCase
	{
		public void TestUpdateChunk_NoErrorAboutSuspendTriggersOutsideATransaction()
		{
			var today = DateTime.Today;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var kit = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component1 = new OrgSupplierPart("P2") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var component2 = new OrgSupplierPart("P3") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var bom1 = new OrgPartBOM(kit, component1) { OE_ComponentQty = 1, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var bom2 = new OrgPartBOM(kit, component2) { OE_ComponentQty = 2, OE_F3_NKPackType = "UNT", OE_IsValid = true }.AppendInsertAndReturnObject(sql);
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, component1.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 90m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive1, component2.PK, 100m, location1.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_StockOnHand = 80m,
				WE_FinalisedDate = today,
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "PICK1", "FIN")
			{
				WP_FinalizedDateUtc = new DateTime(2024, 3, 19, 10, 0, 0),
				WP_GS_NKFinalizedBy = "A",
				WP_SystemCreateTimeUtc = new DateTime(2024, 3, 18, 1, 2, 0),
				WP_SystemLastEditUser = "XYZ",
			}.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1")
			{
				WD_WP = pick.PK,
				WD_FinalisedDate = today,
				WD_GS_NKFinalizedBy = "ORZ",
			}.AppendInsertAndReturnObject(sql);
			var orderKitLine1 = new WhsDocketLine(order, kit.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine1 = new WhsDocketLine(order, component1.PK, 10m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);
			var orderComponentLine2 = new WhsDocketLine(order, component2.PK, 20m)
			{
				WE_FinalisedDate = today,
				WE_DocketLineStatus = "DEP",
				WE_WE_ParentDocketLine = orderKitLine1,
			}.AppendInsertAndReturnObject(sql);

			var pickLine1 = new WhsPickLine(receiveLine1, orderComponentLine1, 10m) { WZ_PickedDateTime = today.AddHours(1), WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, orderComponentLine2, 20m) { WZ_PickedDateTime = today.AddHours(2), WZ_GS_NKAssignedTo = "B" }.AppendInsertAndReturnObject(sql);

			using (var manager = Db.Connection.BeginTransactionWithManager())
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				manager.CommitTransaction();
			}

			var instance = new PopulateWhsPickByBOMData();
			instance.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			instance.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals("New Receive should be created.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_DocketType == "INW"));
			var newReceive = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketType == "INW" && d.PK != receive1.PK).Single();
			newReceive.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceive.WD_OH_Client), r => r.WD_OH_Client, order.WD_OH_Client)
				.ExpectEquals(nameof(newReceive.WD_WW_Whs), r => r.WD_WW_Whs, order.WD_WW_Whs)
				.ExpectEquals(nameof(newReceive.WD_WP_ParentPickForReceive.FK), r => r.WD_WP_ParentPickForReceive.FK, order.WD_WP)
				.ExpectEquals(nameof(newReceive.WD_BookingDate), r => r.WD_BookingDate.UtcDateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_ArrivalDate), r => r.WD_ArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.ExpectEquals(nameof(newReceive.WD_FinalisedDate), r => r.WD_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceive.WD_DocketStatus), r => r.WD_DocketStatus, "FIN")
				.ExpectEquals(nameof(newReceive.WD_GS_NKFinalizedBy), r => r.WD_GS_NKFinalizedBy, "XYZ")
				.ExpectEquals(nameof(newReceive.WD_DocketSubType), r => r.WD_DocketSubType, "REC")
				.ExpectEquals(nameof(newReceive.WD_DocketID), r => r.WD_DocketID, "PICK1")
				.ExpectEquals(nameof(newReceive.WD_ExternalReference), r => r.WD_ExternalReference, "PICK1")
				.VerifyAll();

			var newReceiveLine = WhsDocketLine.ShallowLoadFromDB(TestConnection, l => l.WE_WD == newReceive.PK).Single();
			newReceiveLine.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newReceiveLine.WE_OP), l => l.WE_OP, kit.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineStatus), l => l.WE_DocketLineStatus, "FIN")
				.ExpectEquals(nameof(newReceiveLine.WE_CurrentInventoryStatus), l => l.WE_CurrentInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_OriginalInventoryStatus), l => l.WE_OriginalInventoryStatus, "AVL")
				.ExpectEquals(nameof(newReceiveLine.WE_TransactionQuantity), l => l.WE_TransactionQuantity, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_ClientOrderedUnits), l => l.WE_ClientOrderedUnits, 10m)
				.ExpectEquals(nameof(newReceiveLine.WE_StockOnHand), l => l.WE_StockOnHand, 0m)
				.ExpectEquals(nameof(newReceiveLine.WE_WL), l => l.WE_WL, location1.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_F3_NKPackType), l => l.WE_F3_NKPackType, "UNT")
				.ExpectEquals(nameof(newReceiveLine.WE_DocketLineType), l => l.WE_DocketLineType, "INW")
				.ExpectEquals(nameof(newReceiveLine.WE_WE_OriginalDocketLineForRating), l => l.WE_WE_OriginalDocketLineForRating.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newReceiveLine.WE_FinalisedDate), l => l.WE_FinalisedDate.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newReceiveLine.WE_AdjustmentArrivalDate), l => l.WE_AdjustmentArrivalDate.Value.DateTime, pick.WP_SystemCreateTimeUtc)
				.VerifyAll();

			AssertEquals("No Transfer Lines created.", 0, WhsDocketLine.CountInDB(TestConnection, l => l.WE_DocketLineType == "TFR"));

			AssertEquals("New Pick Line should be created.", 3, WhsPickLine.CountInDB(TestConnection));
			var newPickLine1 = WhsPickLine.ShallowLoadFromDB(TestConnection, pl => pl.WZ_WE_TransactionLine == orderKitLine1.PK).Single();
			newPickLine1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(newPickLine1.WZ_Units), l => l.WZ_Units, 10m)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_InventoryLine), l => l.WZ_WE_InventoryLine.FK, newReceiveLine.PK)
				.ExpectEquals(nameof(newPickLine1.WZ_GS_NKAssignedTo), l => l.WZ_GS_NKAssignedTo, "XYZ")
				.ExpectEquals(nameof(newPickLine1.WZ_PickedDateTime), l => l.WZ_PickedDateTime.Value.DateTime, pick.WP_FinalizedDateUtc)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalPickedInventoryLine), l => l.WZ_WE_OriginalPickedInventoryLine, null)
				.ExpectEquals(nameof(newPickLine1.WZ_WE_OriginalOrderLine), l => l.WZ_WE_OriginalOrderLine, null)
				.VerifyAll();

			AssertEquals("New WhsBOMInventoryPivot should be created.", 2, WhsBOMInventoryPivot.CountInDB(TestConnection));
			var link1 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine1.PK).Single();
			link1.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link1.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 10m)
				.ExpectEquals(nameof(link1.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
			var link2 = WhsBOMInventoryPivot.ShallowLoadFromDB(TestConnection, l => l.WIP_WE_ComponentLine == orderComponentLine2.PK).Single();
			link2.BuildAssertion(TestConnection)
				.ExpectEquals(nameof(link2.WIP_ComponentQuantity), l => l.WIP_ComponentQuantity, 20m)
				.ExpectEquals(nameof(link2.WIP_WE_InventoryLine), l => l.WIP_WE_InventoryLine, newReceiveLine.PK)
				.VerifyAll();
		}

		DbConnection TestConnection => Db.Connection;
	}
}
