using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsArrivalAndUnloadingCargoDesc))]
	class NctsArrivalAndUnloadingCargoDescTest : NctsCommonCargoDescAbstractTest<NctsHeader>
	{
		public void TestValidation()
		{
			AssertType<NctsArrivalAndUnloadingCargoDescValidation>(goodsItem.Validation);
		}

		public void TestLookups()
		{
			AssertType<NctsArrivalAndUnloadingCargoDescLookups>(goodsItem.Lookups);
		}

		public void TestUnloadedGoodsItemsReadOnly()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			goodsItem = header.UnloadingMovementHeader.GoodsItems.AddNew();
			AssertEquals(false, goodsItem.UnloadingNotesInfo.ReadOnly);
			goodsItem.HasDifferences = false;
			goodsItem.IsNew = false;
			AssertEquals(true, goodsItem.BY_DescriptionInfo.ReadOnly);
		}

		public void TestHasDifferencesReadOnly()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			goodsItem = header.UnloadingMovementHeader.GoodsItems.AddNew();

			goodsItem.IsNew = true;
			Assert("ReadOnly", goodsItem.HasDifferencesInfo.ReadOnly);

			goodsItem.IsNew = false;
			Assert("Not ReadOnly", !goodsItem.HasDifferencesInfo.ReadOnly);
		}

		public void TestIsMissingReadOnly()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			goodsItem = header.UnloadingMovementHeader.GoodsItems.AddNew();

			goodsItem.IsNew = true;
			Assert("ReadOnly", goodsItem.IsMissingInfo.ReadOnly);

			goodsItem.IsNew = false;
			Assert("Not ReadOnly", !goodsItem.IsMissingInfo.ReadOnly);
		}

		public void TestIsNewReadOnly()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			goodsItem = header.UnloadingMovementHeader.GoodsItems.AddNew();
			Assert("Always ReadOnly", goodsItem.IsNewInfo.ReadOnly);
		}

		public void TestUnloadedContainers()
		{
			AssertEquals(0, goodsItem.Containers.Count);
			NCTSTestHelper.AddContainerForTest(goodsItem, "CONTAINER1", "", "");
			NCTSTestHelper.AddContainerForTest(goodsItem, "CONTAINER2", "", "");
			AssertEquals(2, goodsItem.Containers.Count);
			AssertEquals("CONTAINER1", goodsItem.Containers[0].ContainerNumber);
			AssertEquals("CONTAINER2", goodsItem.Containers[1].ContainerNumber);
		}

		public void TestDelete_Containers()
		{
			goodsItem.Containers.AddNew();
			AssertEquals(1, goodsItem.Containers.Count);
			goodsItem.Delete();
			AssertEquals(0, goodsItem.Containers.Count);
		}

		public void TestRestoreUnloadedItemB5ParentIdNotNull()
		{
			NCTSTestHelper.SetupPackageForTest(goodsItem, "TestPackage1", "PX", 1);
			NCTSTestHelper.SetupPackageForTest(goodsItem, "TestPackage2", "PX", 2);

			var packages = goodsItem.Packages.Cast<NctsPackage>();
			AssertEquals(2, packages.Count());
			Assert("First B5_ParentID GUID exists", packages.FirstOrDefault().B5_PackageID.IsValid);
			Assert("Second B5_ParentID GUID exists", packages.LastOrDefault().B5_PackageID.IsValid);

			var newGoodItem = header.UnloadingMovementHeader.GoodsItems.AddNew();
			newGoodItem.IsMissing = true;
			AssertEquals(0, newGoodItem.Packages.Count);

			var unloadingPackage1 = newGoodItem.Packages.AddNew();
			var unloadingPackage2 = newGoodItem.Packages.AddNew();
			var unloadingPackage3 = newGoodItem.Packages.AddNew();
			Factory.Save();

			newGoodItem.IsMissing = false;
			Factory.Save();
			Assert(unloadingPackage1.IsDeleted);
			Assert(unloadingPackage2.IsDeleted);
			Assert(unloadingPackage3.IsDeleted);

			var newPackages = newGoodItem.Packages.Cast<NctsPackage>();
			AssertEquals(2, newPackages.Count());
			Assert(!newPackages.Contains(unloadingPackage1));
			Assert(!newPackages.Contains(unloadingPackage2));
			Assert(!newPackages.Contains(unloadingPackage3));
			Assert("First unloading B5_ParentID GUID exists", newPackages.FirstOrDefault().B5_ParentID.IsValid);
			Assert("Second unloading B5_ParentID GUID exists", newPackages.LastOrDefault().B5_ParentID.IsValid);
		}

		protected override ZString CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			return nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			goodsItem.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections, Array.Empty<PropertyDescriptor>());
			return goodsItem;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			goodsItem = header.ArrivalMovementHeader.GoodsItems.AddNew();
			Factory.Save();
		}
		NctsHeader header;
		NctsArrivalAndUnloadingCargoDesc goodsItem;
	}
}
