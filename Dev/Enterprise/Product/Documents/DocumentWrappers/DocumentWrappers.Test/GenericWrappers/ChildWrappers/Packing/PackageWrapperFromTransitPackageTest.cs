using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageWrapperFromTransitPackage))]
	sealed class PackageWrapperFromTransitPackageTest : PackageWrapperFromPkgPackageTest
	{
		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (PackageWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.OutturnPackages.Value", 0m, wrapperEmpty.OutturnedPackages.Value);
			AssertEquals("wrapperEmpty.OutturnPackages.Unit.Code", "", wrapperEmpty.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.OutturnWeight.Value", 0m, wrapperEmpty.OutturnedWeight.Value);
			AssertEquals("wrapperEmpty.OutturnWeight.Unit.Code", "KG", wrapperEmpty.OutturnedWeight.Unit.Code);
			AssertEquals("wrapperEmpty.OutturnVolume.Value", 0m, wrapperEmpty.OutturnedVolume.Value);
			AssertEquals("wrapperEmpty.OutturnVolume.Unit.Code", "M3", wrapperEmpty.OutturnedVolume.Unit.Code);
		}

		#endregion

		#region TestWrapperMappingFull

		[TestDate(2014, 2, 6)]
		public override void TestWrapperMappingFull()
		{
			var transithelper = new WhsTransitTestHelper(Factory);
			var warehouse = transithelper.CreateWarehouse("TTT", true);
			Factory.Save();

			var rcn = transithelper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = transithelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var packageState = transithelper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "P1", "ARV", receiveUnit: rtu, volume: 10, volumeUQ: Constants.Volume.CubicCentimeters, weight: 15, weightUQ: Constants.Weight.Tonnes);

			var wrapperFull = new PackageWrapperFromTransitPackage(packageState.Package, Factory);
			AssertEquals("wrapperFull.OutturnPackages.Value", 1m, wrapperFull.OutturnedPackages.Value);
			AssertEquals("wrapperFull.OutturnPackages.Unit.Code", Constants.PkgUnit.Pallet, wrapperFull.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperFull.OutturnVolume.Value", 10m, wrapperFull.OutturnedVolume.Value);
			AssertEquals("wrapperFull.OutturnVolume.Unit.Code", "CC", wrapperFull.OutturnedVolume.Unit.Code);
			AssertEquals("wrapperFull.OutturnWeight.Value", 15m, wrapperFull.OutturnedWeight.Value);
			AssertEquals("wrapperFull.OutturnWeight.Unit.Code", "T", wrapperFull.OutturnedWeight.Unit.Code);
		}

		#endregion

		#region TestConstructor_NullPkgPackage

		public void TestConstructor_NullPkgPackage()
		{
			var wrapper1 = new PackageWrapperFromTransitPackage(null, Factory);
			AssertNoExceptionThrown(() =>
			{
				AssertEquals("outturnedPackages.Value should be 0", 0m, wrapper1.OutturnedPackages.Value);
			});

			var wrapper2 = new PackageWrapperFromTransitPackage(Factory.GetNull<PkgPackage>(), Factory);
			AssertNoExceptionThrown(() =>
			{
				AssertEquals("outturnedPackages.Value should be 0", 0m, wrapper2.OutturnedPackages.Value);
			});
		}

		#endregion

		#region TestGetHandlingUnit

		public void TestGetHandlingUnit()
		{
			var package = Factory.New<PkgPackage>();
			var hu = Factory.New<PkgPackage>();
			var divot = Factory.New<PkgPackageHandlingUnitDivot>();
			divot.KPD_KP_Package = package.PK;
			divot.KPD_KP_HandlingUnit = hu.PK;

			var wrapper = new PackageWrapperFromTransitPackage(package, Factory);
			AssertEquals("Package should have handling unit", hu.PK, wrapper.HandlingUnit.Identifier);

			divot.KPD_UnpackedTime = new ZDateTimeOffset(2021, 11, 1);
			var newHu = Factory.New<PkgPackage>();
			var newDivot = Factory.New<PkgPackageHandlingUnitDivot>();
			newDivot.KPD_KP_Package = package.PK;
			newDivot.KPD_KP_HandlingUnit = newHu.PK;

			var wrapper2 = new PackageWrapperFromTransitPackage(package, Factory);
			AssertEquals("Package should have handling unit unpacked", newHu.PK, wrapper2.HandlingUnit.Identifier);
		}

		#endregion

		#region TestGetToplevelHandlingUnit

		public void TestGetToplevelHandlingUnit()
		{
			var package = Factory.New<PkgPackage>();
			var hu = Factory.New<PkgPackage>();
			package.KP_KP_TopHandlingUnitPackage = hu.PK;

			var wrapper = new PackageWrapperFromTransitPackage(package, Factory);
			AssertEquals("Package should have top handling unit", hu.PK, wrapper.TopLevelHandlingUnit.Identifier);
		}

		#endregion

		#region TestGetHandlingUnit_Nullable

		public void TestGetHandlingUnit_Nullable()
		{
			var package = Factory.New<PkgPackage>();
			var wrapper = new PackageWrapperFromTransitPackage(package, Factory);
			AssertNoExceptionThrown(() =>
			{
				AssertEquals("HandlingUnit.PackageState.Arrived should be empty", ZDateTime.Empty, wrapper.HandlingUnit.PackageState.Arrived);
				AssertEquals("TopLevelHandlingUnit.PackageState.Arrived should be empty", ZDateTime.Empty, wrapper.TopLevelHandlingUnit.PackageState.Arrived);
			});
		}

		#endregion

		#region TestPackedPackages

		public void TestPackedPackagesForHandlingUnitPackage()
		{
			var warehouse = Helper.CreateWarehouse("TTT");
			Factory.Save();

			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);

			var hut1_p1 = Helper.CreatePackageState(rcn, 1, "PKG", "HUT1_P1", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var hut1_hu1_p1 = Helper.CreatePackageState(rcn, 1, "PKG", "HUT1_HU1_P1", TransitWarehouseStatuses.Codes.Committed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);

			Helper.DisableTopLevelHUFKForTest(TestConnection);

			var huJobTop1 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackageTop1 = Helper.CreateHandlingUnitPackage("HUT1", huJobTop1, rtu, dll: loadList);

			var huJob1 = Helper.CreatePackageHandlingUnit();
			var hut1_hu1 = Helper.CreateHandlingUnitPackage("HUT1_HU1", huJob1, rtu, dll: loadList);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageTop1, hut1_hu1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageTop1, hut1_p1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);
			Helper.PackPackageIntoHandlingUnit(hut1_hu1, hut1_hu1_p1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);

			Factory.Save();

			var hu1Wrapper = new PackageWrapperFromTransitPackage(handlingUnitPackageTop1.Package, Factory);

			AssertNotNull(hu1Wrapper.PackedPackages);
			AssertEquals(3, hu1Wrapper.PackedPackages.Count);
			AssertPackageWrapper(hu1Wrapper.PackedPackages.OfType<PackageWrapper>().FirstOrDefault(o => o.RefNumber == "HUT1_HU1"), 0, "HUT1_HU1");
			AssertPackageWrapper(hu1Wrapper.PackedPackages.OfType<PackageWrapper>().FirstOrDefault(o => o.RefNumber == "HUT1_HU1_P1"), 1, "HUT1_HU1_P1");
			AssertPackageWrapper(hu1Wrapper.PackedPackages.OfType<PackageWrapper>().FirstOrDefault(o => o.RefNumber == "HUT1_P1"), 0, "HUT1_P1");
		}

		public void TestPackedPackagesForNonTrackedPackage()
		{
			var functionHelper = new WhsTestHelperFunctions(Factory);
			var warehouse = Helper.CreateWarehouse("TTT");
			Factory.Save();

			var location = Helper.CreateLocation(warehouse, "LOC");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(rcn);
			Factory.Save();

			var package1 = functionHelper.CreatePackage("KEG", "P1", packageJob1.Packages);
			functionHelper.CreateWhsItemPackageState(TransitWarehouseStatuses.Codes.Arrived, location, rtu, package1);

			var package2 = functionHelper.CreatePackage("KEG", "", package1.Packages, "NonTrackableInner1");
			functionHelper.CreateWhsItemPackageState(TransitWarehouseStatuses.Codes.Arrived, location, rtu, package2);

			var package3 = functionHelper.CreatePackage("KEG", "", package2.Packages, "NonTrackableInner2");
			functionHelper.CreateWhsItemPackageState(TransitWarehouseStatuses.Codes.Arrived, location, rtu, package3);

			Factory.Save();

			var packageLoad = Factory.Load<PkgPackage>(package1.PK);

			var hu1Wrapper = new PackageWrapperFromTransitPackage(packageLoad, Factory);

			AssertNotNull(hu1Wrapper.PackedPackages);
			AssertEquals(2, hu1Wrapper.PackedPackages.Count);
			AssertPackageWrapper(hu1Wrapper.PackedPackages.Cast<PackageWrapper>().Single(p => p.Description == "NonTrackableInner1"), 0, "");
			AssertPackageWrapper(hu1Wrapper.PackedPackages.Cast<PackageWrapper>().Single(p => p.Description == "NonTrackableInner2"), 1, "");
		}

		public void TestPackedPackagesForHUAndNonTranckedPackage()
		{
			var functionHelper = new WhsTestHelperFunctions(Factory);
			var warehouse = Helper.CreateWarehouse("TTT");
			Factory.Save();

			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);

			var hut1_p1 = Helper.CreatePackageState(rcn, 1, "PKG", "HUT1_P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var hut1_p1_p1 = functionHelper.CreatePackage("PKG", "", hut1_p1.Package.Packages);
			hut1_p1_p1.KP_GoodsDescription = "NonTrackableInner";
			functionHelper.CreateWhsItemPackageState(TransitWarehouseStatuses.Codes.Arrived, outBoundLocation, rtu, hut1_p1_p1);

			var hut1_hu1_p1 = Helper.CreatePackageState(rcn, 1, "PKG", "HUT1_HU1_P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);

			Helper.DisableTopLevelHUFKForTest(TestConnection);

			var huJobTop1 = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackageTop1 = Helper.CreateHandlingUnitPackage("HUT1", huJobTop1, rtu, dll: loadList);

			var huJob1 = Helper.CreatePackageHandlingUnit();
			var hut1_hu1 = Helper.CreateHandlingUnitPackage("HUT1_HU1", huJob1, rtu, dll: loadList);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageTop1, hut1_hu1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageTop1, hut1_p1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);
			Helper.PackPackageIntoHandlingUnit(hut1_hu1, hut1_hu1_p1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);

			Factory.Save();

			var hu1Wrapper = new PackageWrapperFromTransitPackage(handlingUnitPackageTop1.Package, Factory);

			AssertNotNull(hu1Wrapper.PackedPackages);
			AssertEquals(4, hu1Wrapper.PackedPackages.Count);
			AssertPackageWrapper(hu1Wrapper.PackedPackages.OfType<PackageWrapper>().FirstOrDefault(o => o.RefNumber == "HUT1_HU1"), 0, "HUT1_HU1");
			AssertPackageWrapper(hu1Wrapper.PackedPackages.OfType<PackageWrapper>().FirstOrDefault(o => o.RefNumber == "HUT1_HU1_P1"), 1, "HUT1_HU1_P1");
			AssertPackageWrapper(hu1Wrapper.PackedPackages.OfType<PackageWrapper>().FirstOrDefault(o => o.RefNumber == "HUT1_P1"), 0, "HUT1_P1");
			AssertPackageWrapper(hu1Wrapper.PackedPackages.OfType<PackageWrapper>().FirstOrDefault(o => o.Description == "NonTrackableInner"), 1, "");
		}

		void AssertPackageWrapper(PackageWrapper packageWrapper, int level, string expectedPackageId)
		{
			AssertNotNull(packageWrapper);
			var expectedIndent = new string(' ', level * 6);
			var package = (PkgPackage)packageWrapper.WrappedObject;

			AssertEquals(expectedIndent, packageWrapper.Indent);
			AssertEquals(expectedPackageId, package.KP_PackageID);
		}

		#endregion

		#region TestFirstLevelPackedPackages

		public void TestGetFirstLevelPackedPackages()
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			Factory.Save();

			var inBoundLocation = Helper.CreateLocation(warehouse, "INB");
			var outBoundLocation = Helper.CreateLocation(warehouse, "OBU");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);

			var overpack = helper.CreateOverpackPackage("OVP001", rcn, receiveUnit: null, rcn: rcn);
			var firstLevelInner = helper.CreateOverpackPackage("OVP002", rcn, receiveUnit: null, rcn: rcn);
			var inner = helper.CreatePackageState(rcn, 10, "BOX", null, TransitWarehouseStatuses.Codes.Arrived, rtu);

			helper.PackPackageIntoHandlingUnit(overpack, firstLevelInner, ZDateTimeOffset.Now, "XXX", overpack);
			helper.PackPackageIntoHandlingUnit(firstLevelInner, inner, ZDateTimeOffset.Now, "XXX", overpack);
			Factory.Save();

			var outerWrapper = new PackageWrapperFromTransitPackage(overpack.Package, Factory);

			AssertNotNull(outerWrapper.FirstLevelPackedPackages);
			AssertEquals(1, outerWrapper.FirstLevelPackedPackages.Count);
			AssertPackageWrapper(outerWrapper.FirstLevelPackedPackages.OfType<PackageWrapper>().FirstOrDefault(o => o.RefNumber == "OVP002"), 0, "OVP002");
		}

		#endregion

		#region TestOutterCount

		public void TestOutterCount()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateTRWWarehouse();
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var job = PkgPackageJob.LoadOrCreatePackageJob(rcn);
			var package = job.Packages.AddNew("BOX", "P1");
			job.Packages.AddNew("BOX", "P2");
			job.Packages.AddNew("BOX", "P3");
			Factory.Save();

			var wrapper = new PackageWrapperFromTransitPackage(package, Factory);
			AssertEquals((ZShort)3, wrapper.OutterPackagesCount);

			wrapper.SetOutterPackagesCount(10);
			AssertEquals((ZShort)10, wrapper.OutterPackagesCount);
		}

		#endregion

		#region TestInnerPackagesCount

		public void TestInnerPackagesCount()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateTRWWarehouse();
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState1 = helper.CreatePackageState(rcn, 10, "BOX", null, TransitWarehouseStatuses.Codes.Arrived, rtu);
			var overpack = helper.CreateOverpackPackage("OVP001", rcn, receiveUnit: null, rcn: rcn);
			helper.PackPackageIntoHandlingUnit(overpack, packageState1, ZDateTimeOffset.Now, "XXX", overpack);
			Factory.Save();

			var wrapper = new PackageWrapperFromTransitPackage(overpack.Package, Factory);
			AssertEquals((ZShort)10, wrapper.InnerPackagesCount);
		}

		#endregion

		#region TestIsOuterPackage

		public void TestIsOuterPackage()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateTRWWarehouse();
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState1 = helper.CreatePackageState(rcn, 10, "BOX", null, TransitWarehouseStatuses.Codes.Arrived, rtu);
			var overpack = helper.CreateOverpackPackage("OVP001", rcn, receiveUnit: null, rcn: rcn);
			helper.PackPackageIntoHandlingUnit(overpack, packageState1, ZDateTimeOffset.Now, "XXX", overpack);

			var packageState2 = helper.CreatePackageState(rcn, 10, "BOX", null, TransitWarehouseStatuses.Codes.Arrived, rtu);
			var branch = helper.CreateGlbBranch("BRN");
			var handlingUnitPackage = helper.PackingHelper.CreatePkgHandlingUnit(branch.PK, "TWH");
			var handlingUnit = helper.CreateHandlingUnitPackage("HU", handlingUnitPackage, rtu);
			helper.PackPackageIntoHandlingUnit(handlingUnit, packageState2, ZDateTimeOffset.Now, "XXX", handlingUnit);
			Factory.Save();

			var wrapperOVP = new PackageWrapperFromTransitPackage(overpack.Package, Factory);
			AssertEquals((ZBool)true, wrapperOVP.IsOuterPackage);

			var wrapperPackageState1 = new PackageWrapperFromTransitPackage(packageState1.Package, Factory);
			AssertEquals((ZBool)false, wrapperPackageState1.IsOuterPackage);

			var wrapperHU = new PackageWrapperFromTransitPackage(handlingUnit.Package, Factory);
			AssertEquals((ZBool)false, wrapperHU.IsOuterPackage);

			var wrapperPackageState2 = new PackageWrapperFromTransitPackage(packageState2.Package, Factory);
			AssertEquals((ZBool)true, wrapperPackageState2.IsOuterPackage);
		}

		#endregion

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageWrapperFromTransitPackage(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var pallet = Factory.New<PkgPackage>();
			pallet.KP_PackageQty = 1;
			pallet.KP_F3_NKPackType = Constants.PkgUnit.Pallet;

			return new PackageWrapperFromTransitPackage(pallet, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Commodity : 
CommonCurrency : 
Container : 
DamagedPackages : 
DamagedReason : 
Dimensions : 
FumigatedPackages : 
HandlingUnit : 1 CNT
HeatTreatedPackages : 
ISPMPalletPackages : 
MostRecentAudit :  is null
NonStackablePackages : 
Origin : 
OutturnedPackages : 
OutturnedVolume : 
OutturnedWeight : 
PackageOrderReference :  is null
Packages : 1 PLT
PackageState : (No Default Field Value Available on PackageState)
PackedItem : (No Default Field Value Available on PackedItemEmpty)
Parent :  is null
PillagedPackages : 
Registry : (No Default Field Value Available on Registry)
TopLevelHandlingUnit : 1 CNT
TopLoadOnlyPackages : 
UOMType : 
Volume : 
Weight :
";
			}
		}

		#endregion

		#region Helper

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
