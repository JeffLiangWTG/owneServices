using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Common;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Common
{
	[TestedType(typeof(WhsPickWithDDLAnalysis))]
	class WhsPickWithDDLAnalysisTest : DbCreateScriptTest
	{
		#region TestView_EmptyPick

		public void TestView_EmptyPick()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL").First();
			var otherLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass != "DDL").First();

			// create 2 warehouses with dock door locations
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var ddlAssignment1 = new WhsDockDoorAssignment(locationA1).AppendInsertAndReturnObject(sql);

			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationB1 = new WhsLocation(row2.PK, area2.PK, area2.PK, ddlLocationType.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationB2 = new WhsLocation(row2.PK, area2.PK, area2.PK, otherLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var ddlAssignment2 = new WhsDockDoorAssignment(locationB2).AppendInsertAndReturnObject(sql);

			// correct DDLs
			var pick1 = new WhsPick(whs1, "P1", "NEW") { WP_WL_DockDoor = locationA1 }.AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs1, "P2", "NEW") { WP_WL_DockDoor = locationA2 }.AppendInsertAndReturnObject(sql);
			var pick3 = new WhsPick(whs2, "P3", "NEW") { WP_WL_DockDoor = locationB1 }.AppendInsertAndReturnObject(sql);
			var pick4 = new WhsPick(null, "P4", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
			var pick5 = new WhsPick(whs1, "P5", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
			var pick6A = new WhsPick(whs1, "P6A", "NEW")
			{
				WP_WL_DockDoor = null,
				WP_WDA_DockDoorAssignment = ddlAssignment1
			}.AppendInsertAndReturnObject(sql);

			// incorrect DDLs
			var pick6 = new WhsPick(null, "P6", "NEW") { WP_WL_DockDoor = locationA1 }.AppendInsertAndReturnObject(sql);
			var pick7 = new WhsPick(whs2, "P7", "NEW") { WP_WL_DockDoor = locationA1 }.AppendInsertAndReturnObject(sql);
			var pick8 = new WhsPick(whs2, "P8", "NEW") { WP_WL_DockDoor = locationB2 }.AppendInsertAndReturnObject(sql);
			var pick9 = new WhsPick(whs2, "P9", "NEW")
			{
				WP_WL_DockDoor = null,
				WP_WDA_DockDoorAssignment = ddlAssignment1
			}.AppendInsertAndReturnObject(sql);
			var pick10 = new WhsPick(whs2, "P10", "NEW")
			{
				WP_WL_DockDoor = null,
				WP_WDA_DockDoorAssignment = ddlAssignment2
			}.AppendInsertAndReturnObject(sql);

			// Save data to DB
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			CombineAssertions(() =>
			{
				AssertEquals("Warehouse1 has multiple DDLs. Pick with DDL of A-1 should be valid.", true, IsDockDoorLocationValid(pick1));
				AssertEquals("Warehouse1 has multiple DDLs. Pick with DDL of A-2 should be valid.", true, IsDockDoorLocationValid(pick2));
				AssertEquals("Warehouse2 has a single DDL (excluding any auto-created DDLs). Pick with DDL of B-1 should be valid.", true, IsDockDoorLocationValid(pick3));
			});
			AssertEquals("Warehouse and DDL can be empty for picks with no orders attached.", true, IsDockDoorLocationValid(pick4));
			AssertEquals("When warehouse is entered, DDL can remain empty if no orders attached.", true, IsDockDoorLocationValid(pick5));
			AssertEquals("When warehouse is entered, Pick with DDL Assignment of A-1 should be valid.", true, IsDockDoorLocationValid(pick5));

			AssertEquals("DDL cannot be entered without also entering Warehouse.", false, IsDockDoorLocationValid(pick6));
			AssertEquals("Pick's warehouse must match warehouse of DDL.", false, IsDockDoorLocationValid(pick7));
			AssertEquals("Specified location should be a DDL type.", false, IsDockDoorLocationValid(pick8));
			AssertEquals("Pick's warehouse must match warehouse of DDL Assignment.", false, IsDockDoorLocationValid(pick9));
			AssertEquals("Specified DDL Assignment location should be a DDL type.", false, IsDockDoorLocationValid(pick10));
		}

		#endregion

		#region TestView_LocationWithIncorrectStatus

		public void TestView_LocationWithIncorrectStatus_PickDDL()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "DOCK").InsertAndReturnObject(TestConnection);

			var ddlLocationType = warehouse.WW_DefaultOutboundDockDoor.LoadAs<WhsLocation>().WL_WLT_LocationType;
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);

			int index = 0;

			foreach (var locationStatus in new[] { "DAM", "HEL", "VOI" })
			{
				index++;
				var row = new WhsRow(warehouse, "DOCK" + index).InsertAndReturnObject(TestConnection);
				var location = new WhsLocation(row.PK, area.PK, area.PK, ddlLocationType) { WL_LocationStatus = locationStatus }.InsertAndReturnObject(TestConnection);
				var pick = new WhsPick(warehouse, "P" + index, "PIC") { WP_WL_DockDoor = location }.InsertAndReturnObject(TestConnection);
				new WhsDocket(client.PK, warehouse.PK, "ORD", "ORD", "PIC", "0" + index) { WD_WP = pick.PK }.Insert(TestConnection);
				AssertEquals($"Specified Location with status of {locationStatus} should *not* be considered a valid Dock Door Location.", false, IsDockDoorLocationValid(pick));
			}

			var dockRow = new WhsRow(warehouse, "DOCK").InsertAndReturnObject(TestConnection);
			var normalLocation = new WhsLocation(dockRow.PK, area.PK, area.PK, ddlLocationType) { WL_LocationStatus = "NOR" }.InsertAndReturnObject(TestConnection);
			var goodPick = new WhsPick(warehouse, "GOODPICK", "PIC") { WP_WL_DockDoor = normalLocation }.InsertAndReturnObject(TestConnection);
			new WhsDocket(client.PK, warehouse.PK, "ORD", "ORD", "PIC", "GOODORDER") { WD_WP = goodPick.PK }.Insert(TestConnection);
			AssertEquals("Specified location has correct Location Status.", true, IsDockDoorLocationValid(goodPick));
		}

		public void TestView_LocationWithIncorrectStatus_AssignmentDDL()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var area = new WhsArea(warehouse.PK, "DOCK").InsertAndReturnObject(TestConnection);

			var ddlLocationType = warehouse.WW_DefaultOutboundDockDoor.LoadAs<WhsLocation>().WL_WLT_LocationType;
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);

			int index = 0;
			foreach (var locationStatus in new[] { "DAM", "HEL", "VOI" })
			{
				index++;
				var row = new WhsRow(warehouse, "DOCK" + index).InsertAndReturnObject(TestConnection);
				var location = new WhsLocation(row.PK, area.PK, area.PK, ddlLocationType) { WL_LocationStatus = locationStatus }.InsertAndReturnObject(TestConnection);
				var ddlAssignment = new WhsDockDoorAssignment(location).InsertAndReturnObject(TestConnection);
				var pick = new WhsPick(warehouse, "P" + index, "PIC") { WP_WL_DockDoor = null, WP_WDA_DockDoorAssignment = ddlAssignment }.InsertAndReturnObject(TestConnection);
				new WhsDocket(client.PK, warehouse.PK, "ORD", "ORD", "PIC", "0" + index) { WD_WP = pick.PK }.Insert(TestConnection);
				AssertEquals($"Specified Location with status of {locationStatus} should *not* be considered a valid Dock Door Location.", false, IsDockDoorLocationValid(pick));
			}

			var dockRow = new WhsRow(warehouse, "DOCK").InsertAndReturnObject(TestConnection);
			var normalLocation = new WhsLocation(dockRow.PK, area.PK, area.PK, ddlLocationType) { WL_LocationStatus = "NOR" }.InsertAndReturnObject(TestConnection);
			var assignment = new WhsDockDoorAssignment(normalLocation).InsertAndReturnObject(TestConnection);
			var goodPickDDL = new WhsPick(warehouse, "GOODPICK", "PIC") { WP_WL_DockDoor = null, WP_WDA_DockDoorAssignment = assignment }.InsertAndReturnObject(TestConnection);
			new WhsDocket(client.PK, warehouse.PK, "ORD", "ORD", "PIC", "GOODORDER") { WD_WP = goodPickDDL.PK }.Insert(TestConnection);
			AssertEquals("Specified location has correct Location Status.", true, IsDockDoorLocationValid(goodPickDDL));
		}

		#endregion

		#region TestView_PickWithOrder

		public void TestView_PickWithOrder_PickDDL()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var branch3 = new GlbBranch("BR3").InsertAndReturnObject(TestConnection);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL").First();
			var otherLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass != "DDL").First();

			// create warehouse with dock door location
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row1.PK, area1.PK, area1.PK, otherLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			// create inactive warehouse
			var inactiveWhs = new WhsWarehouse("WH2", branch2.PK) { WW_IsActive = false }.WithDockDoor(TestConnection);

			// create virtual warehouse
			var virtualWhs = new WhsWarehouse("WH3", branch3.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);

			// Pick with correct DDL
			var pick1 = new WhsPick(whs1, "P00000001", "NEW") { WP_WL_DockDoor = locationA1 }.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "W00000001") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order1, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// Pick with not DDL location
			var pick2 = new WhsPick(whs1, "P00000002", "NEW") { WP_WL_DockDoor = locationA2 }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "W00000002") { WD_WP = pick2.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order2, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// Pick with no DDL location entered
			var pick3 = new WhsPick(whs1, "P00000003", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
			var order3 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "W00000003") { WD_WP = pick3.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order3, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// Pick for non-active Warehouses do not require DDL.
			var pick4 = new WhsPick(inactiveWhs, "P00000004", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
			var order4 = new WhsDocket(client.PK, inactiveWhs.PK, "ORD", "ORD", "PIC", "W00000004") { WD_WP = pick4.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order4, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// Pick for virtual Warehouses do not require DDL.
			var pick5 = new WhsPick(virtualWhs, "P00000005", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
			var order5 = new WhsDocket(client.PK, virtualWhs.PK, "ORD", "ORD", "PIC", "W00000005") { WD_WP = pick5.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order5, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// insert data into DB
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertEquals("Warehouse1 has a single DDL (excluding any auto-created DDLs). Pick with DDL of A-1 should be valid.", true, IsDockDoorLocationValid(pick1));
			AssertEquals("Specified location should be a DDL type.", false, IsDockDoorLocationValid(pick2));

			AssertEquals("DDL is mandatory for picks with orders attached.", false, IsDockDoorLocationValid(pick3));
			AssertEquals("Pick for non-active Warehouses do not require DDL.", true, IsDockDoorLocationValid(pick4)); // we only allow this because the original transformation that auto-created DDLs did not create them for inactive or virtual warehouses
			AssertEquals("Pick for virtual Warehouses do not require DDL.", true, IsDockDoorLocationValid(pick5)); // we only allow this because the original transformation that auto-created DDLs did not create them for inactive or virtual warehouses
		}

		public void TestView_PickWithOrder_Assignment()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var branch3 = new GlbBranch("BR3").InsertAndReturnObject(TestConnection);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL").First();
			var otherLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass != "DDL").First();

			// create warehouse with dock door location
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var assignmentA1 = new WhsDockDoorAssignment(locationA1).AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row1.PK, area1.PK, area1.PK, otherLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var assignmentA2 = new WhsDockDoorAssignment(locationA2).AppendInsertAndReturnObject(sql);

			// create inactive warehouse
			var inactiveWhs = new WhsWarehouse("WH2", branch2.PK) { WW_IsActive = false }.WithDockDoor(TestConnection);

			// create virtual warehouse
			var virtualWhs = new WhsWarehouse("WH3", branch3.PK) { WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);

			// Pick with correct DDL
			var pick1 = new WhsPick(whs1, "P00000001", "NEW") { WP_WL_DockDoor = null, WP_WDA_DockDoorAssignment = assignmentA1 }.AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "W00000001") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order1, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// Pick with not DDL location
			var pick2 = new WhsPick(whs1, "P00000002", "NEW") { WP_WL_DockDoor = null, WP_WDA_DockDoorAssignment = assignmentA2 }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "W00000002") { WD_WP = pick2.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order2, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// Pick with no DDL location entered
			var pick3 = new WhsPick(whs1, "P00000003", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
			var order3 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "W00000003") { WD_WP = pick3.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order3, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// Pick for non-active Warehouses do not require DDL.
			var pick4 = new WhsPick(inactiveWhs, "P00000004", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
			var order4 = new WhsDocket(client.PK, inactiveWhs.PK, "ORD", "ORD", "PIC", "W00000004") { WD_WP = pick4.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order4, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// Pick for virtual Warehouses do not require DDL.
			var pick5 = new WhsPick(virtualWhs, "P00000005", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
			var order5 = new WhsDocket(client.PK, virtualWhs.PK, "ORD", "ORD", "PIC", "W00000005") { WD_WP = pick5.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(order5, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// insert data into DB
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertEquals("Warehouse1 has a single DDL (excluding any auto-created DDLs). Pick with Assigned DDL of A-1 should be valid.", true, IsDockDoorLocationValid(pick1));
			AssertEquals("Specified location should be a DDL type.", false, IsDockDoorLocationValid(pick2));

			AssertEquals("DDL is mandatory for picks with orders attached.", false, IsDockDoorLocationValid(pick3));
			AssertEquals("Pick for non-active Warehouses do not require DDL.", true, IsDockDoorLocationValid(pick4)); // we only allow this because the original transformation that auto-created DDLs did not create them for inactive or virtual warehouses
			AssertEquals("Pick for virtual Warehouses do not require DDL.", true, IsDockDoorLocationValid(pick5)); // we only allow this because the original transformation that auto-created DDLs did not create them for inactive or virtual warehouses
		}

		#endregion

		#region TestView_PickWithWorkOrder

		public void TestView_PickWithWorkOrder()
		{
			TestView_PickWithComponentOrder("WOR", isAssignment: false);
		}

		public void TestView_PickWithDynamicWorkOrder()
		{
			TestView_PickWithComponentOrder("DWO", isAssignment: false);
		}

		void TestView_PickWithComponentOrder(string docketType, bool isAssignment)
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL").First();

			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			// DDL is not used by Work Order
			var pick1 = new WhsPick(whs1, "P1", "NEW") { WP_WL_DockDoor = null }.AppendInsertAndReturnObject(sql);
			var workOrder1 = new WhsDocket(client.PK, whs1.PK, docketType, "ASS", "PIC", "W00000001") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(workOrder1, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// Save data to DB
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertEquals("Picks with Work Orders should not have DDL entered.", true, IsDockDoorLocationValid(pick1));
		}

		public void TestView_Invalid_PickWithWorkOrder_PickDDL()
		{
			TestView_Invalid_PickWithComponentOrder("WOR", isAssignment: false);
		}

		public void TestView_Invalid_PickWithDynamicWorkOrder_PickDDL()
		{
			TestView_Invalid_PickWithComponentOrder("DWO", isAssignment: false);
		}

		public void TestView_Invalid_PickWithWorkOrder_Assignment()
		{
			TestView_Invalid_PickWithComponentOrder("WOR", isAssignment: true);
		}

		public void TestView_Invalid_PickWithDynamicWorkOrder_Assignment()
		{
			TestView_Invalid_PickWithComponentOrder("DWO", isAssignment: true);
		}

		void TestView_Invalid_PickWithComponentOrder(string docketType, bool isAssignment)
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL").First();

			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK) { WL_LocationStatus = "DAM" }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			// DDL is not used by Work Order
			var pick1 = new WhsPick(whs1, "P1", "NEW")
			{
				WP_WL_DockDoor = isAssignment ? null : locationA1,
				WP_WDA_DockDoorAssignment = isAssignment ? new WhsDockDoorAssignment(locationA1).AppendInsertAndReturnObject(sql) : null,
			}.AppendInsertAndReturnObject(sql);
			var workOrder1 = new WhsDocket(client.PK, whs1.PK, docketType, "ASS", "PIC", "W00000001") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);
			new WhsDocketLine(workOrder1, product.PK, 10m).AppendInsertAndReturnObject(sql);

			// Save data to DB
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertEquals("Picks with Work Orders will also consider invalid DockDoors.", false, IsDockDoorLocationValid(pick1));
		}

		#endregion

		#region Implementation

		bool IsDockDoorLocationValid(WhsPick pick)
		{
			var sql = $"select count(*) from dbo.WhsPickWithDDLAnalysis where PickPK = '{pick.PK}'";
			var count = (int)TestConnection.ExecuteScalar(sql);
			return count == 0;
		}

		protected override void SetUp()
		{
			// drop triggers so we can insert incorrect data for view to find
			TestConnection.ExecuteNonQuery(@"IF EXISTS (SELECT null from sys.triggers where name = 'TG_WhsPick_DockDoorLocationIsCorrect')
	DROP TRIGGER TG_WhsPick_DockDoorLocationIsCorrect");

			TestConnection.ExecuteNonQuery(@"IF EXISTS (SELECT null from sys.triggers where name = 'TG_WhsOrder_EnsureDDLIsEnteredOnPick')
	DROP TRIGGER TG_WhsOrder_EnsureDDLIsEnteredOnPick");

			TestConnection.ExecuteNonQuery(@"IF EXISTS (SELECT null from sys.triggers where name = 'TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick')
	DROP TRIGGER TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick");

			base.SetUp();
		}

		#endregion
	}
}
