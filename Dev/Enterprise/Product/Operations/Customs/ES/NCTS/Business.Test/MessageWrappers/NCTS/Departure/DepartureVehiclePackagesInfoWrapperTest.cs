using System;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class DepartureVehiclePackagesInfoWrapperTest : Customs.Business.Testing.DataProviderTestCase<DepartureVehiclePackagesInfoWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null Movement Detail", () => new DepartureVehiclePackagesInfoWrapper(null));
		}

		public void TestPackages()
		{
			CombineAssertions(() =>
			{
				var packages = wrapper.Packages;
				AssertEquals("Expected empty Packages list", 0, packages.Count);
				AssertSame("Cached", packages, wrapper.Packages);
			});
		}

		public void TestPackagesFilledWhenIsVehicles()
		{
			CombineAssertions(() =>
			{
				var vehicle = goodsItem.Packages.AddNew();
				vehicle.B5_PackageID = "VINCODE";
				AssertEquals("Expected filled Packages list when IsVehicles is true", 1, wrapper.Packages.Count);

				goodsItem.IsVehicles = false;
				wrapper = new DepartureVehiclePackagesInfoWrapper(goodsItem);
				AssertEquals("Expected empty Packages list when IsVehicles is false", 0, wrapper.Packages.Count);
			});
		}

		public void TestPackagesWithVin()
		{
			var vehicle = goodsItem.Packages.AddNew();
			vehicle.B5_PackageID = "VINCODE";
			AssertEquals(1, wrapper.Packages.Count);
		}

		public void TestPackagesWithBrand()
		{
			var vehicle = goodsItem.Packages.AddNew();
			vehicle.B5_Brand = "Brand";
			AssertEquals(1, wrapper.Packages.Count);
		}

		public void TestPackagesWithModel()
		{
			var vehicle = goodsItem.Packages.AddNew();
			vehicle.B5_Model = "Model";
			AssertEquals(1, wrapper.Packages.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem.IsVehicles = true;
			wrapper = new DepartureVehiclePackagesInfoWrapper(goodsItem);
		}
		NctsDepartureCargoDesc goodsItem;
		DepartureVehiclePackagesInfoWrapper wrapper;

		protected override DepartureVehiclePackagesInfoWrapper GetProvider() => wrapper;
	}
}
