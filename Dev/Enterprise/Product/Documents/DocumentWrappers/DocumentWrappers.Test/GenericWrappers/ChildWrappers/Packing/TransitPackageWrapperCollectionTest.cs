using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(TransitPackageWrapperCollection))]
	sealed class TransitPackageWrapperCollectionTest : PackageWrapperCollectionTest
	{
		#region TestTransitPackageWrapperCollection

		public void TestTransitPackageWrapperCollection()
		{
			var transitHelper = new WhsTransitTestHelper(Factory);
			var warehouse = Helper.CreateWarehouse("TTT");
			Factory.Save();

			var inBoundLocation = transitHelper.CreateLocation(warehouse, "INB");
			var outBoundLocation = transitHelper.CreateLocation(warehouse, "OBU");

			var rtu = transitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var rcn = transitHelper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var dcn = transitHelper.CreateDispatchConsignment("DTU", warehouse.PK);
			var loadList = transitHelper.CreateDispatchLoadList("DLL1", warehouse.PK, outBoundLocation, isReadyToStage: true);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(rcn);

			var hut1_p1 = transitHelper.CreatePackageState(rcn, 1, "PKG", "HUT1_P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);
			var hut1_p1_p1 = Helper.CreatePackage("PKG", "", hut1_p1.Package.Packages, "NonTrackableInner");
			Helper.CreateWhsItemPackageState(TransitWarehouseStatuses.Codes.Arrived, outBoundLocation, rtu, hut1_p1_p1);

			var hut1_hu1_p1 = transitHelper.CreatePackageState(rcn, 1, "PKG", "HUT1_HU1_P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: loadList);

			transitHelper.DisableTopLevelHUFKForTest(TestConnection);

			var huJobTop1 = transitHelper.CreatePackageHandlingUnit();
			var handlingUnitPackageTop1 = transitHelper.CreateHandlingUnitPackage("HUT1", huJobTop1, rtu, dll: loadList);

			var huJob1 = transitHelper.CreatePackageHandlingUnit();
			var hut1_hu1 = transitHelper.CreateHandlingUnitPackage("HUT1_HU1", huJob1, rtu, dll: loadList);
			transitHelper.PackPackageIntoHandlingUnit(handlingUnitPackageTop1, hut1_hu1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);
			transitHelper.PackPackageIntoHandlingUnit(handlingUnitPackageTop1, hut1_p1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);
			transitHelper.PackPackageIntoHandlingUnit(hut1_hu1, hut1_hu1_p1, ZDateTimeOffset.Now, "ABC", handlingUnitPackageTop1);

			Factory.Save();

			var collection = new TransitPackageWrapperCollection(handlingUnitPackageTop1.Package.HandlingUnitPackedPackages, PackageWrapperCollection.PackLevel.All, Factory, pkg => pkg.HandlingUnitPackedPackages.Union(pkg.Packages));

			AssertNotNull(collection);
			AssertEquals(4, collection.Count);
			AssertPackageWrapper(collection.OfType<PackageWrapper>().FirstOrDefault(o => o.RefNumber == "HUT1_HU1"), 0, "HUT1_HU1");
			AssertPackageWrapper(collection.OfType<PackageWrapper>().FirstOrDefault(o => o.RefNumber == "HUT1_HU1_P1"), 1, "HUT1_HU1_P1");
			AssertPackageWrapper(collection.OfType<PackageWrapper>().FirstOrDefault(o => o.RefNumber == "HUT1_P1"), 0, "HUT1_P1");
			AssertPackageWrapper(collection.OfType<PackageWrapper>().FirstOrDefault(o => o.Description == "NonTrackableInner"), 1, "");
		}

		void AssertPackageWrapper(PackageWrapper packageWrapper, int level, string expectedPackageId)
		{
			var expectedIndent = new string(' ', level * 6);
			var package = (PkgPackage)packageWrapper.WrappedObject;

			AssertEquals(expectedIndent, packageWrapper.Indent);
			AssertEquals(expectedPackageId, package.KP_PackageID);
		}

		#endregion

		protected override void LoadFromPkgPackagesCore(bool checkPackageID)
		{
			var warehouse = Helper.CreateWarehouse("TTT");
			Factory.Save();

			var rcn = TransitHelper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(rcn);

			Factory.Save();

			var package1 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "789", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "", packageJob.Packages);

			var collection = new PackageWrapperCollection(new[] { packageJob }, PackageWrapperCollection.PackLevel.All, PackageWrapperCollection.PackSelection.All, Factory, checkPackageID);
			AssertContainsExactElementsInAnyOrder("collection", checkPackageID ? new[] { package1, package3 } : new[] { package1, package2, package3, package4 }, collection.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		protected override PackageWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new TransitPackageWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new PackageWrapperFromPkgPackage(null, Factory);
		}

		WhsTransitTestHelper TransitHelper
		{
			get { return transitHelper ?? (transitHelper = new WhsTransitTestHelper(Factory)); }
		}
		WhsTransitTestHelper transitHelper;
	}
}
