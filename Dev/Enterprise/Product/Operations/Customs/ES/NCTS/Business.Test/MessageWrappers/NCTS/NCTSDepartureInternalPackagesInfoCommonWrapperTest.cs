using System;
using System.Linq;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsDepartureInternalPackagesInfoCommonWrapperTest : Customs.Business.Testing.DataProviderTestCase<NctsDepartureInternalPackagesInfoCommonWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("NctsMovementDetail", () => new NctsDepartureInternalPackagesInfoCommonWrapper(null));
		}

		public void TestPackagesIsVehicles()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = true;
				goodsItem.Packages.AddNew();
				goodsItem.Packages.AddNew();

				wrapper = new NctsDepartureInternalPackagesInfoCommonWrapper(goodsItem);
				var packages = wrapper.Packages;

				AssertEquals("Expected filled Packages with vehicles", 2, packages.Count);
				AssertSame("Cached Packages", wrapper.Packages, packages);
			});
		}

		public void TestPackagesIsNotVehicles()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = false;
				goodsItem.Packages.AddNew();
				goodsItem.Packages.AddNew();

				wrapper = new NctsDepartureInternalPackagesInfoCommonWrapper(goodsItem);
				var packages = wrapper.Packages;

				AssertEquals("Expected filled Packages with packages", 2, packages.Count);
				AssertSame("Cached Packages", wrapper.Packages, packages);
			});
		}

		public void TestPackages()
		{
			goodsItem.IsVehicles = false;
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_MarksAndNumbers = InternalPackage1.Marks;
			package1.B5_UnitType = InternalPackage1.Type;
			package1.B5_UnitCount = InternalPackage1.NumberOfPackages;
			var wrappedPackage = wrapper.Packages.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Tag", InternalPackage1.Marks, wrappedPackage.Tag);
				AssertEquals("ElementsType", InternalPackage1.Type, wrappedPackage.ElementsType);
				AssertEquals("NumberOfElements", InternalPackage1.NumberOfPackages, wrappedPackage.NumberOfElements);
			});
		}

		public void TestCreatedVehicles()
		{
			goodsItem.IsVehicles = true;
			var vehicle1 = goodsItem.Packages.AddNew();
			vehicle1.B5_PackageID = InternalVehicle1.Vin;
			var wrappedVehicle = wrapper.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Tag", InternalVehicle1.Vin, wrappedVehicle.Tag);
				AssertEquals("ElementsType", InternalVehicle1.Type, wrappedVehicle.ElementsType);
				AssertEquals("NumberOfElements", InternalVehicle1.NumberOfElements, wrappedVehicle.NumberOfElements);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			wrapper = new NctsDepartureInternalPackagesInfoCommonWrapper(goodsItem);
		}

		NctsDepartureCargoDesc goodsItem;
		NctsDepartureInternalPackagesInfoCommonWrapper wrapper;

		protected override NctsDepartureInternalPackagesInfoCommonWrapper GetProvider() => wrapper;
	}

	public struct InternalPackage1
	{
		public const string Type = "CT";
		public const string Marks = "marks";
		public const int NumberOfPackages = 5;
	}

	public struct InternalPackage2
	{
		public const string Type = "NE";
		public const string Marks = "marks2";
		public const int NumberOfElements = 0;
		public const int NumberOfPackages = 5;
		public const int NumberOfPieces = 4;
	}

	public struct InternalVehicle1
	{
		public const string Type = "FR";
		public const int NumberOfElements = 1;
		public const string Vin = "VINCODE1";
	}

	public struct InternalVehicle2
	{
		public const string Type = "FR";
		public const int NumberOfElements = 2;
		public const string Mark = "BASTIDORES";
		public const string Vin = "VINCODE";
	}
}
