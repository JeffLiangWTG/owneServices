using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS.Testing;

class PackageWrapperTest : Customs.Business.Testing.DataProviderTestCase<PackageWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new PackageWrapper(null, false));
	}

	public void TestMarksAndNumbersOfPackages()
	{
		CombineAssertions(() =>
		{
			package.B5_MarksAndNumbers = "SOME PACKAGE MARKS AND NUMBERS";
			package.B5_PackageID = "VINCODE1";
			package.B5_Brand = "BRAND1";
			package.B5_Model = "MODEL1";

			AssertEquals("When isVehicles flag is false we get data from B5_MarksAndNumbers", "SOME PACKAGE MARKS AND NUMBERS", wrapper.MarksAndNumbersOfPackages);

			wrapper = new PackageWrapper(package, true);
			AssertEquals("When isVehicles flag is true we get data from B5_PackageID:B5_Brand:B5_Model (with all fields filled)", "VINCODE1:BRAND1:MODEL1", wrapper.MarksAndNumbersOfPackages);

			package.B5_PackageID = ZString.Empty;
			wrapper = new PackageWrapper(package, true);
			AssertEquals("When isVehicles flag is true we get data from B5_PackageID:B5_Brand:B5_Model (with B5_PackageID missing)", ":BRAND1:MODEL1", wrapper.MarksAndNumbersOfPackages);

			package.B5_PackageID = "VINCODE1";
			package.B5_Brand = ZString.Empty;
			wrapper = new PackageWrapper(package, true);
			AssertEquals("When isVehicles flag is true we get data from B5_PackageID:B5_Brand:B5_Model (with B5_PackageID missing)", "VINCODE1::MODEL1", wrapper.MarksAndNumbersOfPackages);

			package.B5_Brand = "BRAND1";
			package.B5_Model = ZString.Empty;
			wrapper = new PackageWrapper(package, true);
			AssertEquals("When isVehicles flag is true we get data from B5_PackageID:B5_Brand:B5_Model (with B5_PackageID missing)", "VINCODE1:BRAND1:", wrapper.MarksAndNumbersOfPackages);
		});
	}

	public void TestKindOfPackages()
	{
		CombineAssertions(() =>
		{
			package.B5_UnitType = "BG";
			AssertEquals("When isVehicles flag is false we get data from B5_UnitType", "BG", wrapper.KindOfPackages);

			wrapper = new PackageWrapper(package, true);
			AssertEquals("When isVehicles flag is true we get FR when B5_UnitType is not empty", "FR", wrapper.KindOfPackages);

			package.B5_UnitType = ZString.Empty;
			wrapper = new PackageWrapper(package, true);
			AssertEquals("When isVehicles flag is true we get FR when B5_UnitType is empty", "FR", wrapper.KindOfPackages);
		});
	}

	public void TestNumberOfPackages()
	{
		CombineAssertions(() =>
		{
			package.B5_UnitCount = 23;
			package.B5_UnitType = ZString.Empty;
			AssertEquals("When isVehicles flag is false we get data from B5_UnitCount", 23, wrapper.NumberOfPackages);

			wrapper = new PackageWrapper(package, true);
			AssertEquals("When isVehicles flag is true we get 1 when B5_UnitCount is not empty", 1, wrapper.NumberOfPackages);

			package.B5_UnitCount = 0;
			wrapper = new PackageWrapper(package, true);
			AssertEquals("When isVehicles flag is true we get 1 when B5_UnitCount is empty", 1, wrapper.NumberOfPackages);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var item = header.Bills.AddNew().GoodsItems.AddNew();
		package = Factory.New<NctsPackage>();
		package.B5_ParentID = item.PK;
		package.B5_ParentTableCode = item.TablePrefix;
		wrapper = new PackageWrapper(package, false);
	}
	PackageWrapper wrapper;
	NctsPackage package;

	protected override PackageWrapper GetProvider() => wrapper;
}
