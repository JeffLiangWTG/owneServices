using System;
using System.Linq;
using System.Threading;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(PopulateWhsPickDockDoorAssignment))]
	class PopulateWhsPickDockDoorAssignmentsTest : DataTransformationTestCase
	{
		#region Run and Assert Twice

		protected override void PrepareTestData()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();

			// Whs1
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "Area1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "Row1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs1.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "Me" }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 20m, location1.PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "AVL",
				WE_UnloadedTime = new DateTimeOffset(now),
				WE_GS_NKUnloadedBy = "Bob"
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs1.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 20m, whs1.WW_DefaultOutboundDockDoor.FK)
			{
				WE_StockOnHand = 20m,
				WE_WL_TransferFrom = location1.PK,
				WE_CurrentInventoryStatus = "INT",
				WE_OriginalInventoryStatus = "INT",
				WE_DocketLineStatus = "HFT",
			}.AppendInsertAndReturnObject(sql);

			new WhsPickLine(receiveLine, transferLine, 20m) { WZ_PickedDateTime = now, WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs1, "P1", "ENT") { WP_WL_DockDoor = whs1.WW_DefaultOutboundDockDoor }.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 10m).AppendInsertAndReturnObject(sql);
			var pickline1 = new WhsPickLine(transferLine, orderLine1, 10m)
			{
				WZ_GS_NKAssignedTo = "Bob",
				WZ_WE_OriginalPickedInventoryLine = receiveLine,
			}.AppendInsertAndReturnObject(sql);

			var pick2 = new WhsPick(whs1, "P2", "ENT") { WP_WL_DockDoor = whs1.WW_DefaultOutboundDockDoor }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "O2") { WD_WP = pick2.PK }.AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order2, product.PK, 10m).AppendInsertAndReturnObject(sql);
			var pickline2 = new WhsPickLine(transferLine, orderLine2, 10m)
			{
				WZ_GS_NKAssignedTo = "Bob",
				WZ_WE_OriginalPickedInventoryLine = receiveLine,
			}.AppendInsertAndReturnObject(sql);

			// Whs2
			var branch2 = new GlbBranch("BR2").AppendInsertAndReturnObject(sql);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(sql);
			var area2 = new WhsArea(whs2.PK, "Area2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs1, "Row2") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var pick3 = new WhsPick(whs2, "P3", "ENT") { WP_WL_DockDoor = whs2.WW_DefaultOutboundDockDoor }.AppendInsertAndReturnObject(sql);
			var order4 = new WhsDocket(client.PK, whs2.PK, "ORD", "ORD", "ATP", "O4") { WD_WP = pick3.PK }.AppendInsertAndReturnObject(sql);
			var orderLine4 = new WhsDocketLine(order4, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var pick4 = new WhsPick(whs2, "P4", "ENT") { WP_WL_DockDoor = whs2.WW_DefaultOutboundDockDoor }.AppendInsertAndReturnObject(sql);
			var order6 = new WhsDocket(client.PK, whs2.PK, "ORD", "ORD", "PIC", "O6") { WD_WP = pick4.PK }.AppendInsertAndReturnObject(sql);

			// Pick no order
			new WhsPick(whs2, "PR", "ENT") { WP_WL_DockDoor = whs2.WW_DefaultOutboundDockDoor }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("2 DDA created.", 2, WhsDockDoorAssignment.CountInDB(TestConnection));
			var whs1 = WhsWarehouse.ShallowLoadFromDB(TestConnection, w => w.WW_WarehouseCode == "WH1").Single();
			var dda1 =	WhsDockDoorAssignment.ShallowLoadFromDB(TestConnection, d => d.WDA_WL_AssignedDockDoor == whs1.WW_DefaultOutboundDockDoor.FK).Single();

			WhsDockDoorAssignment.AssertFromDB(TestConnection, dda1.PK)
				.ExpectEquals(nameof(WhsDockDoorAssignment.WDA_WL_AssignedDockDoor), a => a.WDA_WL_AssignedDockDoor.FK, whs1.WW_DefaultOutboundDockDoor.FK)
				.ExpectNotEquals(nameof(WhsDockDoorAssignment.WDA_FirstPutawayToDockDoorUtc), a => a.WDA_FirstPutawayToDockDoorUtc, null)
				.VerifyAll();

			var whs2 = WhsWarehouse.ShallowLoadFromDB(TestConnection, w => w.WW_WarehouseCode == "WH2").Single();
			var dda2 =	WhsDockDoorAssignment.ShallowLoadFromDB(TestConnection, d => d.WDA_WL_AssignedDockDoor == whs2.WW_DefaultOutboundDockDoor.FK).Single();

			WhsDockDoorAssignment.AssertFromDB(TestConnection, dda2.PK)
				.ExpectEquals(nameof(WhsDockDoorAssignment.WDA_WL_AssignedDockDoor), a => a.WDA_WL_AssignedDockDoor.FK, whs2.WW_DefaultOutboundDockDoor.FK)
				.ExpectNotEquals(nameof(WhsDockDoorAssignment.WDA_FirstPutawayToDockDoorUtc), a => a.WDA_FirstPutawayToDockDoorUtc, null)
				.VerifyAll();

			var picks = WhsPick.ShallowLoadFromDB(TestConnection);
			AssertEquals("Correct number of picks found", 5, picks.Length);

			WhsPick.AssertFromDB(TestConnection, picks.Single(p => p.WP_PickNo == "P1").PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs1)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, dda1)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, null)
				.VerifyAll();

			WhsPick.AssertFromDB(TestConnection, picks.Single(p => p.WP_PickNo == "P2").PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs1)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, dda1)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, null)
				.VerifyAll();

			WhsPick.AssertFromDB(TestConnection, picks.Single(p => p.WP_PickNo == "P3").PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs2)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, dda2)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, null)
				.VerifyAll();

			WhsPick.AssertFromDB(TestConnection, picks.Single(p => p.WP_PickNo == "P4").PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs2)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, dda2)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, null)
				.VerifyAll();

			WhsPick.AssertFromDB(TestConnection, picks.Single(p => p.WP_PickNo == "PR").PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs2)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, null)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor.FK, whs2.WW_DefaultOutboundDockDoor.FK)
				.VerifyAll();
		}

		#endregion

		#region TestTransform_MultipleDDLs

		public void TestTransform_MultipleDDLs()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var area1 = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "Row1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs, "P1", "ENT") { WP_WL_DockDoor = whs.WW_DefaultOutboundDockDoor }.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var pick2 = new WhsPick(whs, "P2", "ENT") { WP_WL_DockDoor = whs.WW_DefaultOutboundDockDoor }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ATP", "O2") { WD_WP = pick2.PK }.AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order2, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// DDL2
			var area2 = new WhsArea(whs.PK, "Area3").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs, "Row2") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var ddLType = new WhsLocationType("DOL") { WLT_LocationClass = "DDL", WLT_DefaultCycleCountGranularity = "" }.AppendInsertAndReturnObject(sql);
			var ddl2 = new WhsLocation(row2.PK, area2.PK, area2.PK, ddLType.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var pick3 = new WhsPick(whs, "P3", "ENT") { WP_WL_DockDoor = ddl2 }.AppendInsertAndReturnObject(sql);
			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ATP", "O3") { WD_WP = pick3.PK }.AppendInsertAndReturnObject(sql);
			var orderLine3 = new WhsDocketLine(order3, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var pick4 = new WhsPick(whs, "P4", "ENT") { WP_WL_DockDoor = ddl2 }.AppendInsertAndReturnObject(sql);
			var order4 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ATP", "O4") { WD_WP = pick4.PK }.AppendInsertAndReturnObject(sql);
			var orderLine4 = new WhsDocketLine(order4, product.PK, 10m).AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("2 DDA created.", 2, WhsDockDoorAssignment.CountInDB(TestConnection));

			var dda1 = WhsDockDoorAssignment.ShallowLoadFromDB(TestConnection, d => d.WDA_WL_AssignedDockDoor == whs.WW_DefaultOutboundDockDoor.FK).Single();
			WhsDockDoorAssignment.AssertFromDB(TestConnection, dda1.PK)
				.ExpectEquals(nameof(WhsDockDoorAssignment.WDA_WL_AssignedDockDoor), a => a.WDA_WL_AssignedDockDoor.FK, whs.WW_DefaultOutboundDockDoor.FK)
				.ExpectNotEquals(nameof(WhsDockDoorAssignment.WDA_FirstPutawayToDockDoorUtc), a => a.WDA_FirstPutawayToDockDoorUtc, null)
				.VerifyAll();

			WhsPick.AssertFromDB(TestConnection, pick1.PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, dda1)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, null)
				.VerifyAll();

			WhsPick.AssertFromDB(TestConnection, pick2.PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, dda1)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, null)
				.VerifyAll();

			var dda2 = WhsDockDoorAssignment.ShallowLoadFromDB(TestConnection, d => d.WDA_WL_AssignedDockDoor == ddl2.PK).Single();
			WhsDockDoorAssignment.AssertFromDB(TestConnection, dda2.PK)
				.ExpectEquals(nameof(WhsDockDoorAssignment.WDA_WL_AssignedDockDoor), a => a.WDA_WL_AssignedDockDoor.FK, ddl2.PK)
				.ExpectNotEquals(nameof(WhsDockDoorAssignment.WDA_FirstPutawayToDockDoorUtc), a => a.WDA_FirstPutawayToDockDoorUtc, null)
				.VerifyAll();

			WhsPick.AssertFromDB(TestConnection, pick3.PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, dda2)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, null)
				.VerifyAll();

			WhsPick.AssertFromDB(TestConnection, pick4.PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, dda2)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, null)
				.VerifyAll();
		}

		#endregion

		#region TestTransform_IgnoresFinalisedPicks

		public void TestTransform_IgnoresFinalisedPicks()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "Row1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "Me" }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 20m, location.PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "AVL",
				WE_UnloadedTime = new DateTimeOffset(now),
				WE_GS_NKUnloadedBy = "Bob"
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "Me" }.AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 20m, whs.WW_DefaultOutboundDockDoor.FK)
			{
				WE_FinalisedDate = now,
				WE_StockOnHand = 0m,
				WE_WL_TransferFrom = location.PK,
				WE_CurrentInventoryStatus = "AVL",
				WE_OriginalInventoryStatus = "STA",
				WE_DocketLineStatus = "FIN",
			}.AppendInsertAndReturnObject(sql);

			new WhsPickLine(receiveLine, transferLine, 20m) { WZ_PickedDateTime = now, WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs, "P1", "FIN")
			{
				WP_WL_DockDoor = whs.WW_DefaultOutboundDockDoor,
				WP_FinalizedDateUtc = now,
				WP_GS_NKFinalizedBy = "A",
			}.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1") { WD_WP = pick1.PK, WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 10m) { WE_DocketLineStatus = "DEP", WE_FinalisedDate = now }.AppendInsertAndReturnObject(sql);
			var pickline1 = new WhsPickLine(transferLine, orderLine1, 10m)
			{
				WZ_PickedDateTime = now,
				WZ_WE_OriginalPickedInventoryLine = receiveLine,
				WZ_GS_NKAssignedTo = "Bob",
			}.AppendInsertAndReturnObject(sql);

			var pick2 = new WhsPick(whs, "P2", "FIN") {
				WP_WL_DockDoor = whs.WW_DefaultOutboundDockDoor,
				WP_FinalizedDateUtc = now,
				WP_GS_NKFinalizedBy = "A",
			}.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O2") { WD_WP = pick2.PK, WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order2, product.PK, 10m) { WE_DocketLineStatus = "DEP", WE_FinalisedDate = now }.AppendInsertAndReturnObject(sql);
			var pickline2 = new WhsPickLine(transferLine, orderLine2, 10m)
			{
				WZ_PickedDateTime = now,
				WZ_WE_OriginalPickedInventoryLine = receiveLine,
				WZ_GS_NKAssignedTo = "Bob",
			}.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			AssertEquals("Precondition: no DDA exists.", 0, WhsDockDoorAssignment.CountInDB(TestConnection));

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("No DDA exists.", 0, WhsDockDoorAssignment.CountInDB(TestConnection));

			WhsPick.AssertFromDB(TestConnection, pick1.PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, null)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, whs.WW_DefaultOutboundDockDoor)
				.VerifyAll("Pick1 correct");

			WhsPick.AssertFromDB(TestConnection, pick2.PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, null)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, whs.WW_DefaultOutboundDockDoor)
				.VerifyAll("Pick2 correct");
		}

		#endregion

		#region TestTransform_IgnoresCancelledPicks

		public void TestTransform_IgnoresCancelledPicks()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(sql);

			var pick1 = new WhsPick(whs, "P1", "CAN") { WP_WL_DockDoor = whs.WW_DefaultOutboundDockDoor }.AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "P2", "CAN") { WP_WL_DockDoor = whs.WW_DefaultOutboundDockDoor }.AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			AssertEquals("Precondition: no DDA exists.", 0, WhsDockDoorAssignment.CountInDB(TestConnection));

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("No DDA exists.", 0, WhsDockDoorAssignment.CountInDB(TestConnection));

			WhsPick.AssertFromDB(TestConnection, pick1.PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, null)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, whs.WW_DefaultOutboundDockDoor)
				.VerifyAll("Pick1 correct");

			WhsPick.AssertFromDB(TestConnection, pick2.PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, null)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, whs.WW_DefaultOutboundDockDoor)
				.VerifyAll("Pick2 correct");
		}

		#endregion

		#region TestTransform_IgnoresWorkOrderPicks

		public void TestTransform_IgnoresWorkOrderPicks()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var area1 = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "Row1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs, "P1", "ENT") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", "WO1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var pick2 = new WhsPick(whs, "P2", "ENT") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "WOR", "DIS", "ATP", "WO2") { WD_WP = pick2.PK }.AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order2, product.PK, 10m).AppendInsertAndReturnObject(sql);

			SaveToDB(sql);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("0 DDA created.", 0, WhsDockDoorAssignment.CountInDB(TestConnection));

			WhsPick.AssertFromDB(TestConnection, pick1.PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, null)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, null)
				.VerifyAll();

			WhsPick.AssertFromDB(TestConnection, pick2.PK)
				.ExpectEquals(nameof(WhsPick.WP_WW_Whs), p => p.WP_WW_Whs, whs)
				.ExpectEquals(nameof(WhsPick.WP_WDA_DockDoorAssignment), p => p.WP_WDA_DockDoorAssignment, null)
				.ExpectEquals(nameof(WhsPick.WP_WL_DockDoor), p => p.WP_WL_DockDoor, null)
				.VerifyAll();
		}

		#endregion

		#region SaveToDB

		void SaveToDB(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_PreventOverCommitOfStockViaPickLine, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, "WhsCheckPickLinesAreNotOverCommitting_V3"))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotSchema.Constants.TableName, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick, WhsDockDoorAssignmentSchema.Constants.TableName, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger("TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick", WhsPickSchema.Constants.TableName, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger("TG_WhsOrder_EnsureDDLIsEnteredOnPick", WhsDocketSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateWhsPickDockDoorAssignment();
	}
}
