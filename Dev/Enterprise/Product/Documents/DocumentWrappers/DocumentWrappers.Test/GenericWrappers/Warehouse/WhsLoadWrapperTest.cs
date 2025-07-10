using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WhsLoadWrapper))]
	public class WhsLoadWrapperTest : WarehouseJobGenericWrapperTest
	{
		protected override void TestDockDoorCore()
		{
			var whs = Helper.CreateWarehouse("W1");
			var load = Factory.New<WhsLoad>();
			load.WLO_WL_PlannedDockDoor = whs.WW_DefaultInboundDockDoor;

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals(whs.DefaultInboundDockDoorLocation.WLV_LocationString, loadWrapper.DockDoor);
		}

		protected override void TestWarehouseCore()
		{
			var whs = Helper.CreateWarehouse("W1");
			var load = Factory.New<WhsLoad>();
			load.WLO_WL_PlannedDockDoor = whs.WW_DefaultInboundDockDoor;

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("W1", loadWrapper.Warehouse.Code);
		}

		protected override void TestSealCore()
		{
			var load = Factory.New<WhsLoad>();
			load.WLO_SealNumber = "SEAL-1";

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("SEAL-1", loadWrapper.Seal);
		}

		protected override void TestDispatchDriverNameCore()
		{
			var load = Factory.New<WhsLoad>();
			load.WLO_DriversName = "Driver";

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("Driver", loadWrapper.DispatchDriverName);
		}

		protected override void TestTransportationUnitCore()
		{
			var whs = Helper.CreateWarehouse("W1");
			var transportationUnit = Helper.CreateEquipment("Equipment", 10m, Core.Constants.Weight.Kilograms, 10m, Core.Constants.Volume.CubicMetres);
			var load = Factory.New<WhsLoad>();
			load.WLO_RQ_TransportationUnit = transportationUnit.PK;

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("Equipment", loadWrapper.TransportationUnit.Registration);
		}

		protected override void TestOrdersCore()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O2", part, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O3", part, 10m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O4", part, 10m);

			var pick = Helper.CreatePickNew(order1, order2, order3, order4);

			var packingHelper = new PackingTestHelper(Factory);

			var package1 = packingHelper.CreatePackage(order1.PackageJob, "PKG1", 1, "UNT");
			var package2 = packingHelper.CreatePackage(order2.PackageJob, "PKG2", 1, "UNT");
			var package3 = packingHelper.CreatePackage(order3.PackageJob, "PKG3", 1, "UNT");
			var package4 = packingHelper.CreatePackage(order3.PackageJob, "PKG4", 1, "UNT");
			var package5 = packingHelper.CreatePackage(order4.PackageJob, "PKG5", 1, "UNT");
			var package6 = packingHelper.CreatePackage(order4.PackageJob, "PKG6", 1, "UNT");

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot1.WLP_GS_NKLoadingUser = "E";
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			pivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot2.WLP_GS_NKLoadingUser = "E";
			var pivot3 = Helper.CreateLoadPkgPackagePivot(package3.PK, load);
			pivot3.WLP_LoadedTime = ZDateTimeOffset.Empty;
			var pivot4 = Helper.CreateLoadPkgPackagePivot(package4.PK, load);
			pivot4.WLP_LoadedTime = ZDateTimeOffset.Empty;
			var pivot5 = Helper.CreateLoadPkgPackagePivot(package5.PK, load);
			pivot5.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot5.WLP_GS_NKLoadingUser = "E";
			var pivot6 = Helper.CreateLoadPkgPackagePivot(package6.PK, load);
			pivot6.WLP_LoadedTime = ZDateTimeOffset.Empty;

			Factory.Save();

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("Orders with atleast 1 loaded Package should be returned", 3, loadWrapper.Orders.Count);

			var orderWrappers = loadWrapper.Orders.Cast<WarehouseOrderWrapper>();
			var orderWrapper1 = orderWrappers.FirstOrDefault(ow => ow.JobNumber == order1.WD_DocketID);
			var orderWrapper2 = orderWrappers.FirstOrDefault(ow => ow.JobNumber == order2.WD_DocketID);
			var orderWrapper4 = orderWrappers.FirstOrDefault(ow => ow.JobNumber == order4.WD_DocketID);
			AssertNotNull("Order1 Wrapper", orderWrapper1);
			AssertEquals(nameof(orderWrapper1.TotalLoadedPackages), "1", orderWrapper1.TotalLoadedPackages.Value);
			AssertEquals("Loaded Packages Count", 1, orderWrapper1.LoadedPackages.Count);
			AssertEquals("All Packages Count", 1, orderWrapper1.Packages.Count);
			AssertEquals("PackageId", package1.KP_PackageID, orderWrapper1.LoadedPackages[0].RefNumber);

			AssertNotNull("Order2 Wrapper", orderWrapper2);
			AssertEquals(nameof(orderWrapper2.TotalLoadedPackages), "1", orderWrapper2.TotalLoadedPackages.Value);
			AssertEquals("Loaded Packages Count", 1, orderWrapper2.LoadedPackages.Count);
			AssertEquals("All Packages Count", 1, orderWrapper2.Packages.Count);
			AssertEquals("PackageId", package2.KP_PackageID, orderWrapper2.LoadedPackages[0].RefNumber);

			AssertNotNull("Order4 Wrapper", orderWrapper4);
			AssertEquals(nameof(orderWrapper4.TotalLoadedPackages), "1", orderWrapper4.TotalLoadedPackages.Value);
			AssertEquals("Loaded Packages Count", 1, orderWrapper4.LoadedPackages.Count);
			AssertEquals("All Packages Count", 2, orderWrapper4.Packages.Count);
			AssertEquals("PackageId", package5.KP_PackageID, orderWrapper4.LoadedPackages[0].RefNumber);
		}

		public void TestOrders_ILoadingSupport()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			order1.WD_TotalWeightUnit = "KG";
			order1.WD_TotalCubicUnit = "M3";
			var order2 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O2", part, 5m);
			order2.WD_TotalWeightUnit = "KG";
			order2.WD_TotalCubicUnit = "M3";
			var pick = Helper.CreatePickNew(order1, order2);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 5, "UNT");
			var package2 = packingHelper.CreatePackage(order2.PackageJob, 5, "UNT");

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot1.WLP_GS_NKLoadingUser = "E";
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			pivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot2.WLP_GS_NKLoadingUser = "E";
			Factory.Save();

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("Orders with atleast 1 loaded Package should be returned", 2, loadWrapper.Orders.Count);
			var loadingSupports = loadWrapper.Orders.Cast<ILoadingSupport>();
			AssertEquals("Order wrappers are ILoadingSupports", 2, loadingSupports.Count());
			Assert("Order wrappers have load's PK as LoadPK value.", loadingSupports.All(l => l.LoadPK == load.PK));
			Assert("Order wrappers have load's PK as LoadPK value.", loadingSupports.All(l => l.CommonLoadVolumeUQ == "M3"));
			Assert("Order wrappers have load's PK as LoadPK value.", loadingSupports.All(l => l.CommonLoadWeightUQ == "KG"));
		}

		public void TestOrders_ILoadingSupport_MultipleWeightUQ()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			order1.WD_TotalWeightUnit = "KG";
			var order2 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O2", part, 5m);
			order2.WD_TotalWeightUnit = "KG";
			var order3 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O3", part, 5m);
			order3.WD_TotalWeightUnit = "G";
			var pick = Helper.CreatePickNew(order1, order2, order3);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 5, "UNT");
			var package2 = packingHelper.CreatePackage(order2.PackageJob, 5, "UNT");
			var package3 = packingHelper.CreatePackage(order3.PackageJob, 5, "UNT");

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot1.WLP_GS_NKLoadingUser = "E";
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			pivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot2.WLP_GS_NKLoadingUser = "E";
			var pivot3 = Helper.CreateLoadPkgPackagePivot(package3.PK, load);
			pivot3.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot3.WLP_GS_NKLoadingUser = "E";
			Factory.Save();

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			var loadingSupports = loadWrapper.Orders.Cast<ILoadingSupport>();
			Assert("Order wrappers have a common weight UQ.", loadingSupports.All(l => l.CommonLoadWeightUQ == "KG"));
		}

		public void TestOrders_ILoadingSupport_MultipleVolumeUQ()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			order1.WD_TotalCubicUnit = "M3";
			var order2 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O2", part, 5m);
			order2.WD_TotalCubicUnit = "M3";
			var order3 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O3", part, 5m);
			order3.WD_TotalCubicUnit = "L";
			var pick = Helper.CreatePickNew(order1, order2, order3);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 5, "UNT");
			var package2 = packingHelper.CreatePackage(order2.PackageJob, 5, "UNT");
			var package3 = packingHelper.CreatePackage(order3.PackageJob, 5, "UNT");

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot1.WLP_GS_NKLoadingUser = "E";
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			pivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot2.WLP_GS_NKLoadingUser = "E";
			var pivot3 = Helper.CreateLoadPkgPackagePivot(package3.PK, load);
			pivot3.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot3.WLP_GS_NKLoadingUser = "E";
			Factory.Save();

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			var loadingSupports = loadWrapper.Orders.Cast<ILoadingSupport>();
			Assert("Order wrappers have a common volume UQ.", loadingSupports.All(l => l.CommonLoadVolumeUQ == "M3"));
		}

		protected override void TestDeliveryRouteCore()
		{
			var list = DataRegistry.Instance.DeliveryRoutesList;
			var newList = new CodeDescriptionPairList();
			newList.AddPair("TST", "TST Desc");
			DataRegistry.Instance.DeliveryRoutesList = newList;

			TestDeliveryRouteCore("TST Desc");
			DataRegistry.Instance.DeliveryRoutesList = list;
		}

		public void TestDeliveryRoute_NoDescription()
		{
			TestDeliveryRouteCore("TST");
		}

		void TestDeliveryRouteCore(string expectedDeliveryRoute)
		{
			var org = Helper.CreateClient("C1");
			var consignee = Helper.CreateClient("C2");
			consignee.MainAddress.OA_DeliveryRoute = "TST";
			consignee.MainAddress.OA_DeliveryRouteSequence = 1;
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			order.ConsigneePK = consignee.PK;
			Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);
			var package = packingHelper.CreatePackage(order.PackageJob, 5, "UNT");

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			pivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot.WLP_GS_NKLoadingUser = "E";
			Factory.Save();

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("loadWrapper.DeliveryRoute", expectedDeliveryRoute, loadWrapper.DeliveryRoute);
		}

		public void TestDeliveryRoute_Empty()
		{
			var org = Helper.CreateClient("C1");
			var consignee = Helper.CreateClient("C2");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			order.ConsigneePK = consignee.PK;
			Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);
			var package = packingHelper.CreatePackage(order.PackageJob, 5, "UNT");

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			pivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot.WLP_GS_NKLoadingUser = "E";
			Factory.Save();

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("loadWrapper.DeliveryRoute", string.Empty, loadWrapper.DeliveryRoute);
		}

		public void TestDeliveryRoute_MultipleConsignees() => TestDeliveryRoute_MultipleConsignees(sameDeliveryRoute: false);

		public void TestDeliveryRoute_MultipleConsignees_SameDeliveryRoute() => TestDeliveryRoute_MultipleConsignees(sameDeliveryRoute: true);

		void TestDeliveryRoute_MultipleConsignees(bool sameDeliveryRoute)
		{
			var org = Helper.CreateClient("C1");
			var consignee1 = Helper.CreateClient("C2");
			consignee1.MainAddress.OA_DeliveryRoute = "TS1";
			consignee1.MainAddress.OA_DeliveryRouteSequence = 1;

			var consignee2 = Helper.CreateClient("C3");
			consignee2.MainAddress.OA_DeliveryRoute = sameDeliveryRoute ? "TS1" : "TS2";
			consignee2.MainAddress.OA_DeliveryRouteSequence = 2;

			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			order1.ConsigneePK = consignee1.PK;
			var order2 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O2", part, 5m);
			order2.ConsigneePK = consignee2.PK;
			Helper.CreatePickNew(order1, order2);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 5, "UNT");
			var package2 = packingHelper.CreatePackage(order2.PackageJob, 5, "UNT");

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot1.WLP_GS_NKLoadingUser = "E";
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			pivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot2.WLP_GS_NKLoadingUser = "E";
			Factory.Save();

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("loadWrapper.DeliveryRoute", sameDeliveryRoute ? "TS1" : "Many", loadWrapper.DeliveryRoute);
		}

		public void TestDeliveryRoute_MultipleConsignees_OverrideAddress() => TestDeliveryRoute_MultipleConsignees_OverrideAddress(bothEmpty: false);
		public void TestDeliveryRoute_MultipleConsignees_OverrideAddress_BothEmpty() => TestDeliveryRoute_MultipleConsignees_OverrideAddress(bothEmpty: true);

		void TestDeliveryRoute_MultipleConsignees_OverrideAddress(bool bothEmpty)
		{
			var org = Helper.CreateClient("C1");
			var consignee1 = Helper.CreateClient("C2");
			consignee1.MainAddress.OA_DeliveryRoute = bothEmpty ? "" : "TS1";
			consignee1.MainAddress.OA_DeliveryRouteSequence = bothEmpty ? ZShort.Zero : new ZShort(1);

			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			order1.ConsigneePK = consignee1.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O2", part, 5m);
			order2.ConsigneeDocAddress.E2_AddressOverride = true;
			order2.ConsigneeDocAddress.E2_CompanyName = "ABCD";
			order2.ConsigneeDocAddress.E2_Address1 = "123 Fake Street";
			order2.ConsigneeDocAddress.E2_City = "SYDNEY";
			Helper.CreatePickNew(order1, order2);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 5, "UNT");
			var package2 = packingHelper.CreatePackage(order2.PackageJob, 5, "UNT");

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot1.WLP_GS_NKLoadingUser = "E";
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			pivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot2.WLP_GS_NKLoadingUser = "E";
			Factory.Save();

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("loadWrapper.DeliveryRoute", bothEmpty ? "" : "Many", loadWrapper.DeliveryRoute);
		}

		public void TestDeliveryRoute_MultipleConsignees_SameAddress()
		{
			var org = Helper.CreateClient("C1");
			var consignee1 = Helper.CreateClient("C2");
			consignee1.MainAddress.OA_DeliveryRoute = "TS1";
			consignee1.MainAddress.OA_DeliveryRouteSequence = 1;
			var consignee2 = Helper.CreateClient("C3");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			order1.ConsigneePK = consignee1.PK;
			var order2 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O2", part, 5m);
			order2.ConsigneePK = consignee2.PK;
			order2.ConsigneeAddressPK = consignee1.MainAddress.PK;
			Helper.CreatePickNew(order1, order2);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 5, "UNT");
			var package2 = packingHelper.CreatePackage(order2.PackageJob, 5, "UNT");

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot1.WLP_GS_NKLoadingUser = "E";
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			pivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot2.WLP_GS_NKLoadingUser = "E";
			Factory.Save();

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("loadWrapper.DeliveryRoute", "TS1", loadWrapper.DeliveryRoute);
		}

		public void TestDeliveryRoute_MultipleOrders_SingleConsignee()
		{
			var org = Helper.CreateClient("C1");
			var consignee = Helper.CreateClient("C2");
			consignee.MainAddress.OA_DeliveryRoute = "TST";
			consignee.MainAddress.OA_DeliveryRouteSequence = 1;
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			order1.ConsigneePK = consignee.PK;
			var order2 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O2", part, 5m);
			order2.ConsigneePK = consignee.PK;
			Helper.CreatePickNew(order1, order2);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 5, "UNT");
			var package2 = packingHelper.CreatePackage(order2.PackageJob, 5, "UNT");

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot1.WLP_GS_NKLoadingUser = "E";
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			pivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot2.WLP_GS_NKLoadingUser = "E";
			Factory.Save();

			var loadWrapper = GetNewWarehouseJobGenericWrapper(load);
			AssertEquals("loadWrapper.DeliveryRoute", "TST", loadWrapper.DeliveryRoute);
		}

		#region Implementation

		protected override void SetWhsBusinessObjectWarehouse(BusinessObject bizO, ZGuid warehousePK)
		{
			var whs = Factory.LoadTop1<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.PK, warehousePK));
			((WhsLoad)bizO).WLO_WL_PlannedDockDoor = whs.WW_DefaultOutboundDockDoor;
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WhsLoadWrapper((WhsLoad)bizO, Factory);
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WhsLoadWrapper((WhsLoad)bizO, factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var load = Factory.NewWithValidTestData<WhsLoad>();
			return new WhsLoadWrapper(load, Factory);
		}

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return Factory.New<WhsLoad>();
		}

		protected override BusinessObject GetNewWhsBusinessObjectInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsLoad>();
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CarrierAccount :  is null
CarrierServiceLevel :  is null
Client :  is null
ClientRequestedBillToParty :  is null
CODAmount :  is null
CODType :  is null
ConfirmationInstructions : 
Consignee :  is null
ConsigneeAddress :  is null
Consignor :  is null
CubicSent :  is null
CustomerReference : 
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress :  is null
DropMode :  is null
DropOffAddress :  is null
FinalisedDate : 
Forwarder :  is null
FulfillRule :  is null
GoodsBillToAddress :  is null
HandlingInstructions : 
IncoTerm :  is null
Insurance :  is null
JobClient :  is null
PackagesSent :  is null
PickingInstructions : 
PickMethod : 
PickNo : 
PickNumberReference : 
PickOption :  is null
PickUpAddress :  is null
PrimaryBarcode : 
Registry : (No Default Field Value Available on Registry)
RequiredDate : 
SalesChannel :  is null
SecondaryReference : 
ServiceLevel :  is null
SOPCarrierServiceLevel : 
SOPOrderNumber : 
SOPRequiredDate : 
SOPSpecialInstructions : 
SOPStagingAreaName : 
SOPTransportCompany : 
SplitNumber : 
StagingAreaName : 
StagingLocationString : 
Status : 
Supplier :  is null
SupplierBuyerLink :  is null
SupplierDocAddress :  is null
TotalExtendedLinePrice :  is null
TotalLoadedPackages : 
TotalLoadedUnits : 
TotalLoadedVolume : 
TotalLoadedWeight : 
TotalOuterPackagesVolume : 
TotalOuterPackagesWeight : 
TransportationUnit :  is null
TransportBillToAddress :  is null
TransportCoAddress :  is null
TransportCompany :  is null
TransportReference : 
VehicleReference : 
Warehouse :  is null
WarehouseName : 
WeightSent :  is null
WhoCreated : 
WhoFinalised :
";
			}
		}

		#endregion
	}
}
