using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseOrderWrapperForManifest))]
	sealed class WarehouseOrderWrapperForManifestTest : WarehouseOrderWrapperTest
	{
		#region TestPackages

		protected override void TestPackagesCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			// So we have
			// PackageJob
			//     1x Container
			//         2x Part1 (PackableItem)
			//         2x Box
			//         4x Crate
			//             3x Part1 (PackableItem)
			//             9x Part2 (PackableItem)
			//     5x Pallet
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			// Level 1
			var container = packageJob.Packages.AddNew(Constants.PkgUnit.Container, 1);
			var pallet = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, 5);

			// Level 2
			var box = container.Packages.AddNew(Constants.PkgUnit.Box, 2);
			var crate = container.Packages.AddNew(Constants.PkgUnit.Crate, 4);
			var packedItem1 = container.Pack_ForTesting(orderLine1.ReleaseLines[0], 2);

			// Level 3
			var packedItem2 = crate.Pack_ForTesting(orderLine1.ReleaseLines[0], 3);
			var packedItem3 = crate.Pack_ForTesting(orderLine2.ReleaseLines[0], 5);
			var packedItem4 = crate.Pack_ForTesting(orderLine2.ReleaseLines[0], 4);
			AssertEquals("PackedItems should be same - Divots should be grouped together.", packedItem3, packedItem4);

			// To avoid intermittent test failures.
			packageJob.Packages.ApplySort(PkgPackageSchema.KP_F3_NKPackType.Name, ListSortDirection.Ascending);
			packageJob.Packages[0].Packages.ApplySort(PkgPackageSchema.KP_F3_NKPackType.Name, ListSortDirection.Ascending);
			packageJob.Packages[0].Packages[1].PackedItemDivots.ApplySort(PkgPackageItemDivotSchema.Constants.KI_PackedQty, ListSortDirection.Ascending);

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var packagesWrapper = orderWrapper.Packages;
			AssertEquals(5, packagesWrapper.Count);
			AssertDisplayOrderAndIndent(packagesWrapper[0], "001", "", container, packedItem1);
			AssertDisplayOrderAndIndent(packagesWrapper[1], "002", new string(' ', 6), box, null);
			AssertDisplayOrderAndIndent(packagesWrapper[2], "003", new string(' ', 6), crate, packedItem2);
			AssertDisplayOrderAndIndent(packagesWrapper[3], "003", new string(' ', 6), crate, packedItem3);
			AssertDisplayOrderAndIndent(packagesWrapper[4], "004", "", pallet, null);
		}

		public void TestLoadedPackages_MultipleProductsInPackage()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part1 = Helper.CreateProduct(org, "P1");
			var part2 = Helper.CreateProduct(org, "P2");
			var part3 = Helper.CreateProduct(org, "P3");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org, whs, "R1");
			Helper.CreateWhsReceiveLine(receive, part1, 30m, whs.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, part2, 10m, whs.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, part3, 20m, whs.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(org, whs, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, part1, 30m);
			var orderline2 = Helper.CreateWhsOrderLine(order, part2, 10m);
			var orderline3 = Helper.CreateWhsOrderLine(order, part3, 20m);

			Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var package = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG3", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			package.Pack_ForTesting(orderline1.ReleaseLines[0], 30);
			package.Pack_ForTesting(orderline2.ReleaseLines[0], 10);
			package.Pack_ForTesting(orderline3.ReleaseLines[0], 20);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package, load, isLoaded: true);
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;

			var loadedPackages = orderWrapper.LoadedPackages;
			CombineAssertions(() =>
			{
				AssertEquals("LoadedPackages count correct", 1, loadedPackages.Count);
				AssertEquals("Package - RefNumber", "PKG3", loadedPackages[0].RefNumber);
				AssertEquals("Package - PackedItemCount", 60, loadedPackages[0].PackedItemCount);
				AssertEquals("Package - Volume", 5000m, loadedPackages[0].Volume.Value);
				AssertEquals("Package - Weight", 125m, loadedPackages[0].Weight.Value);
			});
		}

		public void TestLoadedPackages_MultipleProductsInPackage_InternalPackage()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part1 = Helper.CreateProduct(org, "P1");
			var part2 = Helper.CreateProduct(org, "P2");
			var part3 = Helper.CreateProduct(org, "P3");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org, whs, "R1");
			Helper.CreateWhsReceiveLine(receive, part1, 30m, whs.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, part2, 10m, whs.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, part3, 20m, whs.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(org, whs, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, part1, 30m);
			var orderline2 = Helper.CreateWhsOrderLine(order, part2, 10m);
			var orderline3 = Helper.CreateWhsOrderLine(order, part3, 20m);

			Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var package = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG3", 1, "UNT", 5000m, Volume.CubicCentimeters, 5m, Weight.Kilograms);
			package.Pack_ForTesting(orderline1.ReleaseLines[0], 30);
			package.Pack_ForTesting(orderline2.ReleaseLines[0], 10);

			var box = package.Packages.AddNew(PkgUnit.Box);
			box.KP_PackageID = "FTS";
			box.Pack_ForTesting(orderline3.ReleaseLines[0], 20);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package, load, isLoaded: true);
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;

			var loadedPackages = orderWrapper.LoadedPackages;
			CombineAssertions(() =>
			{
				AssertEquals("LoadedPackages count correct", 1, loadedPackages.Count);
				AssertEquals("Package - RefNumber", "PKG3", loadedPackages[0].RefNumber);
				AssertEquals("Package - PackedItemCount", 60, loadedPackages[0].PackedItemCount);
				AssertEquals("Package - Volume", 5000m, loadedPackages[0].Volume.Value);
				AssertEquals("Package - Weight", 125m, loadedPackages[0].Weight.Value);
			});
		}

		public void TestLoadedPackages_MultipleProductsInPackage_ConsolidatedHU()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part1 = Helper.CreateProduct(org, "P1");
			var part2 = Helper.CreateProduct(org, "P2");
			var part3 = Helper.CreateProduct(org, "P3");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org, whs, "R1");
			Helper.CreateWhsReceiveLine(receive, part1, 30m, whs.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, part2, 10m, whs.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, part3, 20m, whs.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(org, whs, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, part1, 30m);
			var orderline2 = Helper.CreateWhsOrderLine(order, part2, 10m);
			var orderline3 = Helper.CreateWhsOrderLine(order, part3, 20m);

			Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var package = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG3", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			package.Pack_ForTesting(orderline1.ReleaseLines[0], 30);
			package.Pack_ForTesting(orderline2.ReleaseLines[0], 10);
			package.Pack_ForTesting(orderline3.ReleaseLines[0], 20);

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(whs.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package, load, isLoaded: true);
			PreparePkgPackagePivot(handlingUnitPackage, load, isLoaded: true);
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;

			var loadedPackages = orderWrapper.LoadedPackages;
			CombineAssertions(() =>
			{
				AssertEquals("LoadedPackages count correct", 1, loadedPackages.Count);
				AssertEquals("Package - RefNumber", "PKG3", loadedPackages[0].RefNumber);
				AssertEquals("Package - PackedItemCount", 60, loadedPackages[0].PackedItemCount);
				AssertEquals("Package - Volume", 5000m, loadedPackages[0].Volume.Value);
				AssertEquals("Package - Weight", 125m, loadedPackages[0].Weight.Value);
			});
		}

		public void TestLoadedPackages_MultipleProductsInPackage_LoadAndConsolidatedHUs()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part1 = Helper.CreateProduct(org, "P1");
			var part2 = Helper.CreateProduct(org, "P2");
			var part3 = Helper.CreateProduct(org, "P3");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org, whs, "R1");
			Helper.CreateWhsReceiveLine(receive, part1, 30m, whs.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, part2, 10m, whs.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, part3, 20m, whs.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(org, whs, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, part1, 30m);
			var orderline2 = Helper.CreateWhsOrderLine(order, part2, 10m);
			var orderline3 = Helper.CreateWhsOrderLine(order, part3, 20m);

			Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var package = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG3", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			package.Pack_ForTesting(orderline1.ReleaseLines[0], 30);
			package.Pack_ForTesting(orderline2.ReleaseLines[0], 10);
			package.Pack_ForTesting(orderline3.ReleaseLines[0], 20);

			var loadHandlingUnit = packingHelper.CreatePkgHandlingUnit(whs.WW_GB_RelatedCompanyBranch, "3PL");
			var loadHandlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(loadHandlingUnit);
			var loadHandlingUnitPackage = packingHelper.CreatePackage(loadHandlingUnitPackageJob, "HU2", 1, PkgUnit.Package);

			var consolidatedHandlingUnit = packingHelper.CreatePkgHandlingUnit(whs.WW_GB_RelatedCompanyBranch, "3PL");
			var consolidatedHandlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(consolidatedHandlingUnit);
			var consolidatedHandlingUnitPackage = packingHelper.CreatePackage(consolidatedHandlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(consolidatedHandlingUnitPackage, package, loadHandlingUnitPackage);
			consolidatedHandlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;

			packingHelper.PackHandlingUnit(loadHandlingUnitPackage, consolidatedHandlingUnitPackage, loadHandlingUnitPackage);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package, load, isLoaded: true);
			PreparePkgPackagePivot(consolidatedHandlingUnitPackage, load, isLoaded: true);
			PreparePkgPackagePivot(loadHandlingUnitPackage, load, isLoaded: true);
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;

			var loadedPackages = orderWrapper.LoadedPackages;
			CombineAssertions(() =>
			{
				AssertEquals("LoadedPackages count correct", 1, loadedPackages.Count);
				AssertEquals("Package - RefNumber", "PKG3", loadedPackages[0].RefNumber);
				AssertEquals("Package - PackedItemCount", 60, loadedPackages[0].PackedItemCount);
				AssertEquals("Package - Volume", 5000m, loadedPackages[0].Volume.Value);
				AssertEquals("Package - Weight", 125m, loadedPackages[0].Weight.Value);
			});
		}

		public void TestLoadedPackagesTotals_NoLoadedPackages()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 70m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);

			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "ER3", 1, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "W2", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: false);
			PreparePkgPackagePivot(package2, load, isLoaded: false);

			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport.CommonLoadVolumeUQ = Constants.Volume.Litre;

			AssertEquals("Order - TotalPackagesCount", 2, orderWrapper.Packages.Count);
			AssertEquals("Order - TotalLoadedPackagesCount", 0, orderWrapper.LoadedPackages.Count);
			AssertEquals("Order - TotalLoadedPackages", "0", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "0", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 0m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 0m, orderWrapper.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_MixedLoad()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 20m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order1, part, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(org, whs, "O2", part, 5m);
			Helper.CreateWhsOrderLine(order2, part, 5m);

			order1.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order1.WD_TotalCubicUnit = Constants.Volume.Litre;

			order2.WD_TotalWeightUnit = Constants.Weight.Grams;
			order2.WD_TotalCubicUnit = Constants.Volume.CubicCentimeters;

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var packingHelper = new PackingTestHelper(Factory);

			var pick1Lines = pick1.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order1.PackageJob.PK, "PKG1", 1, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order1.PackageJob.PK, "PKG2", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pick1Lines[0]);
			packingHelper.CreatePackageDivot(package2, pick1Lines[1]);

			var pick2Lines = pick2.GetAllPickLines().ToArray();
			var package3 = (PkgPackage)packingHelper.CreatePackage(order2.PackageJob.PK, "PKG3", 1, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package4 = (PkgPackage)packingHelper.CreatePackage(order2.PackageJob.PK, "PKG4", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package3, pick2Lines[0]);
			packingHelper.CreatePackageDivot(package4, pick2Lines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: false);

			PreparePkgPackagePivot(package3, load, isLoaded: false);
			PreparePkgPackagePivot(package4, load, isLoaded: true);

			Factory.Save();

			var orderWrapper1 = new WarehouseOrderWrapperForManifest(order1, Factory);
			((ILoadingSupport)orderWrapper1).LoadPK = load.PK;
			var loadingSupport1 = (ILoadingSupport)orderWrapper1;
			loadingSupport1.LoadPK = load.PK;
			loadingSupport1.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport1.CommonLoadVolumeUQ = Constants.Volume.Litre;

			AssertEquals("Order1 - TotalLoadedPackages", "1", orderWrapper1.TotalLoadedPackages.Value);
			AssertEquals("Order1 - TotalLoadedUnits", "5", orderWrapper1.TotalLoadedUnits.Value);
			AssertEquals("Order1 - TotalLoadedWeight", 5m, orderWrapper1.TotalLoadedWeight.Value);
			AssertEquals("Order1 - TotalLoadedVolume", 5m, orderWrapper1.TotalLoadedVolume.Value);

			var orderWrapper2 = new WarehouseOrderWrapperForManifest(order2, Factory);
			var loadingSupport2 = (ILoadingSupport)orderWrapper2;
			loadingSupport2.LoadPK = load.PK;
			loadingSupport2.CommonLoadWeightUQ = Constants.Weight.Grams;
			loadingSupport2.CommonLoadVolumeUQ = Constants.Volume.CubicCentimeters;

			AssertEquals("Order2 - TotalLoadedPackages", "1", orderWrapper2.TotalLoadedPackages.Value);
			AssertEquals("Order2 - TotalLoadedUnits", "5", orderWrapper2.TotalLoadedUnits.Value);
			AssertEquals("Order2 - TotalLoadedWeight", 5000m, orderWrapper2.TotalLoadedWeight.Value);
			AssertEquals("Order2 - TotalLoadedVolume", 5000m, orderWrapper2.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_LoadedPackages()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 70m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);
			order.WD_TotalWeightUnit = Constants.Weight.Grams;
			order.WD_TotalCubicUnit = Constants.Volume.CubicMetres;

			var pick = Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG1", 1, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG2", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport.CommonLoadVolumeUQ = Constants.Volume.Litre;

			AssertEquals("Order - TotalLoadedPackages", "2", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "10", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 10m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedWeight", Constants.Weight.Kilograms, orderWrapper.TotalLoadedWeight.Unit.Code);
			AssertEquals("Order - TotalLoadedVolume", 10m, orderWrapper.TotalLoadedVolume.Value);
			AssertEquals("Order - TotalLoadedVolume", Constants.Volume.Litre, orderWrapper.TotalLoadedVolume.Unit.Code);
		}

		public void TestLoadedPackagesTotals_InvalidUQs()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 20m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(org, whs, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, part, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, part, 5m);
			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG1", 1, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			package1.Pack_ForTesting(orderLine1.ReleaseLines[0], 5);

			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG2", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			package2.Pack_ForTesting(orderLine2.ReleaseLines[0], 5);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);

			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadWeightUQ = "AB";
			loadingSupport.CommonLoadVolumeUQ = "CD";

			AssertEquals("Order - TotalLoadedPackages", "2", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "10", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 0m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 0m, orderWrapper.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_MultipleDivots()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 20m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(org, whs, "O1");
			Helper.CreateWhsOrderLine(order, part, 3m);
			Helper.CreateWhsOrderLine(order, part, 5m);
			Helper.CreateWhsOrderLine(order, part, 7m);
			Helper.CreateWhsOrderLine(order, part, 5m);

			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG1", 1, "UNT", 3m, Constants.Volume.Litre, 3000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG2", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			var package3 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG3", 1, "UNT", 7m, Constants.Volume.Litre, 7000m, Constants.Weight.Grams);
			var package4 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG4", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);
			packingHelper.CreatePackageDivot(package3, pickLines[2]);
			packingHelper.CreatePackageDivot(package4, pickLines[3]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);
			PreparePkgPackagePivot(package3, load, isLoaded: true);
			PreparePkgPackagePivot(package4, load, isLoaded: true);

			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport.CommonLoadVolumeUQ = Constants.Volume.Litre;
			AssertEquals("Order - TotalLoadedPackages", "4", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "20", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 20m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 20m, orderWrapper.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_LoadedPackagesOnMultipleLoads()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 70m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);
			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG1", 1, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG2", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			var package3 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG3", 1, "UNT", 1m, Constants.Volume.Litre, 1m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);
			packingHelper.CreatePackageDivot(package3, pickLines[2]);

			var load1 = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load1, isLoaded: true);
			PreparePkgPackagePivot(package2, load1, isLoaded: true);
			var load2 = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package3, load2, isLoaded: true);
			Factory.Save();

			var orderWrapper1 = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport1 = (ILoadingSupport)orderWrapper1;
			loadingSupport1.LoadPK = load1.PK;
			loadingSupport1.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport1.CommonLoadVolumeUQ = Constants.Volume.Litre;

			AssertEquals("Order - TotalLoadedPackages", "2", orderWrapper1.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "10", orderWrapper1.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 10m, orderWrapper1.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 10m, orderWrapper1.TotalLoadedVolume.Value);

			var orderWrapper2 = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport2 = (ILoadingSupport)orderWrapper2;
			loadingSupport2.LoadPK = load2.PK;
			loadingSupport2.CommonLoadWeightUQ = Constants.Weight.Kilograms;
			loadingSupport2.CommonLoadVolumeUQ = Constants.Volume.Litre;
			AssertEquals("Order - TotalLoadedPackages", "1", orderWrapper2.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "5", orderWrapper2.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 1m, orderWrapper2.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 1m, orderWrapper2.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_MultipleProductsInPackage_InternalPackage()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part1 = Helper.CreateProduct(org, "P1");
			var part2 = Helper.CreateProduct(org, "P2");
			var part3 = Helper.CreateProduct(org, "P3");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org, whs, "R1");
			Helper.CreateWhsReceiveLine(receive, part1, 30m, whs.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, part2, 10m, whs.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, part3, 20m, whs.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(org, whs, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, part1, 30m);
			var orderline2 = Helper.CreateWhsOrderLine(order, part2, 10m);
			var orderline3 = Helper.CreateWhsOrderLine(order, part3, 20m);

			Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var package = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PKG3", 1, "UNT", 5000m, Volume.CubicCentimeters, 5m, Weight.Kilograms);
			package.Pack_ForTesting(orderline1.ReleaseLines[0], 15);

			var box = package.Packages.AddNew(PkgUnit.Box);
			box.KP_PackageID = "FTS";
			box.Pack_ForTesting(orderline3.ReleaseLines[0], 20);

			var bag = box.Packages.AddNew(PkgUnit.Bag);
			bag.KP_PackageID = "bag";
			bag.Pack_ForTesting(orderline1.ReleaseLines[0], 10);
			bag.Pack_ForTesting(orderline2.ReleaseLines[0], 5);

			var casePkg = package.Packages.AddNew(PkgUnit.Case);
			casePkg.KP_PackageID = "casePkg";
			casePkg.Pack_ForTesting(orderline2.ReleaseLines[0], 5);
			casePkg.Pack_ForTesting(orderline1.ReleaseLines[0], 5);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package, load, isLoaded: true);
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadWeightUQ = Weight.Kilograms;
			loadingSupport.CommonLoadVolumeUQ = Volume.CubicMetres;

			CombineAssertions(() =>
			{
				AssertEquals("Order - TotalLoadedPackages", "1", orderWrapper.TotalLoadedPackages.Value);
				AssertEquals("Order - TotalLoadedUnits", "60", orderWrapper.TotalLoadedUnits.Value);
				AssertEquals("Order - TotalLoadedWeight", 125m, orderWrapper.TotalLoadedWeight.Value);
				AssertEquals("Order - TotalLoadedVolume", 0.005m, orderWrapper.TotalLoadedVolume.Value);
			});
		}

		public void TestLoadedPackagesTotals_LoadedPackages_NoLoadPK()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 70m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);
			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "", 5, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);
			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			AssertEquals("Order - TotalLoadedPackages", "0", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "0", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 0m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 0m, orderWrapper.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_NoCommonUnit_NoWeightUQ()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 20m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);

			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "ID3", 1, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "ID45", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);

			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadVolumeUQ = Constants.Volume.Litre;

			AssertEquals("Order - TotalLoadedPackages", "2", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "10", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 0m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 10m, orderWrapper.TotalLoadedVolume.Value);
		}

		public void TestLoadedPackagesTotals_NoCommonUnit_NoVolumeUQ()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("W1", "A", 4, 4);
			var part = Helper.CreateProduct(org, "P1");

			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 20m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "O1", part, 5m);
			Helper.CreateWhsOrderLine(order, part, 5m);

			order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;

			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);

			var pickLines = pick.GetAllPickLines().ToArray();
			var package1 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PK1", 1, "UNT", 5m, Constants.Volume.Litre, 5000m, Constants.Weight.Grams);
			var package2 = (PkgPackage)packingHelper.CreatePackage(order.PackageJob.PK, "PK3", 1, "UNT", 5000m, Constants.Volume.CubicCentimeters, 5m, Constants.Weight.Kilograms);
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);

			var load = Helper.CreateWhsLoad(org, whs.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			PreparePkgPackagePivot(package1, load, isLoaded: true);
			PreparePkgPackagePivot(package2, load, isLoaded: true);

			Factory.Save();

			var orderWrapper = new WarehouseOrderWrapperForManifest(order, Factory);
			var loadingSupport = (ILoadingSupport)orderWrapper;
			loadingSupport.LoadPK = load.PK;
			loadingSupport.CommonLoadWeightUQ = Constants.Weight.Kilograms;

			AssertEquals("Order - TotalLoadedPackages", "2", orderWrapper.TotalLoadedPackages.Value);
			AssertEquals("Order - TotalLoadedUnits", "10", orderWrapper.TotalLoadedUnits.Value);
			AssertEquals("Order - TotalLoadedWeight", 10m, orderWrapper.TotalLoadedWeight.Value);
			AssertEquals("Order - TotalLoadedVolume", 0m, orderWrapper.TotalLoadedVolume.Value);
		}

		void AssertDisplayOrderAndIndent(PackageWrapper packageWrapper, ZString expectedDisplayOrder, ZString expectedIndent, PkgPackage expectedPackage, params PkgPackageItemDivotsWrapper[] expectedPackedItems)
		{
			string message = string.Format("Package wrapper ({0}) {1}", packageWrapper.Packages.ValueAndUnitCode, packageWrapper.RefNumber);
			AssertEquals(message + " has a wrong display order.", expectedDisplayOrder, packageWrapper.DisplayOrder);
			AssertEquals(message + " has a wrong indent.", expectedIndent, packageWrapper.Indent);
			AssertEquals(message + " wraps wrong package.", expectedPackage.PK, packageWrapper.WrappedObjectPK);
			if (expectedPackedItems == null || expectedPackedItems.Length == 0)
			{
				AssertNull("No PackableItemParent should be wrapped.", packageWrapper.PackedItem.WrappedObject);
				AssertEquals("No Packed Items should be wrapped.", 0, packageWrapper.PackedItem.PackedItems.Count());
			}
			else
			{
				var packableItemParent = expectedPackedItems.First().PackableItemParent;
				AssertEquals(message + " wraps wrong packed item.", packableItemParent, packageWrapper.PackedItem.WrappedObject);
				AssertContainsExactElementsInAnyOrder(message + " wraps wrong packed item.", expectedPackedItems, packageWrapper.PackedItem.PackedItems);
			}
		}

		void PreparePkgPackagePivot(PkgPackage package, WhsLoad load, bool isLoaded)
		{
			var pivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			if (isLoaded)
			{
				pivot.WLP_LoadedTime = ZDateTimeOffset.Now;
				pivot.WLP_GS_NKLoadingUser = "E";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return string.Format(@"
CarrierAccount :  is null
CarrierServiceLevel : 
Client : 
ClientRequestedBillToParty :  is null
CODAmount : 
CODType : 
ConfirmationInstructions : 
Consignee : 
ConsigneeAddress : 
Consignor : 
CubicSent : 
CustomerReference : 
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress : 
DropMode : 
DropOffAddress : 
FinalisedDate : 
Forwarder : 
FulfillRule : 
GoodsBillToAddress : 
HandlingInstructions : 
IncoTerm : 
Insurance : 
JobClient : 
PackagesSent : 
PickingInstructions : 
PickMethod : 
PickNo : 
PickNumberReference : 
PickOption : AUT - Auto Pick
PickUpAddress : 
PrimaryBarcode : 
Registry : (No Default Field Value Available on Registry)
RequiredDate : 
SalesChannel :  is null
SecondaryReference : 
ServiceLevel : 
SOPCarrierServiceLevel : 
SOPOrderNumber : 
SOPRequiredDate : 
SOPSpecialInstructions : 
SOPStagingAreaName : 
SOPTransportCompany : 
SplitNumber : 
StagingAreaName : 
StagingLocationString : 
Status : New Entry (Unsaved)
Supplier : 
SupplierBuyerLink : (No Default Field Value Available on SupplierBuyerLink)
SupplierDocAddress : 
TotalExtendedLinePrice : 
TotalLoadedPackages : 0
TotalLoadedUnits : 0
TotalLoadedVolume : 
TotalLoadedWeight : 
TotalOuterPackagesVolume : 
TotalOuterPackagesWeight : 
TransportationUnit :  is null
TransportBillToAddress : 
TransportCoAddress : 
TransportCompany : 
TransportReference : 
VehicleReference : 
Warehouse : 
WarehouseName : 
WeightSent : 
WhoCreated : {0}
WhoFinalised :
", Env.CurrentUser.Initials);
			}
		}

		#endregion

		#region Implementation

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehouseOrderWrapperForManifest((WhsOrder)bizO, Factory);
		}

		#endregion
	}
}
