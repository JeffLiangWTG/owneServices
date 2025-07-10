using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class PackageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When package is null",
			() => CreateNewWrapper(package: null));
	}

	public void TestPackageType()
	{
		package.B5_UnitType = "PKG";

		var wrapper = CreateNewWrapper(package);
		AssertEquals(nameof(IPackage.PackageType), "PKG", wrapper.PackageType);
	}

	public void TestNumberOfPacks()
	{
		package.B5_UnitCount = 99;
		var wrapper = CreateNewWrapper(package);
		AssertEquals(nameof(IPackage.NumberOfPacks), 99, wrapper.NumberOfPacks);

		package.B5_UnitCount = 99999999;
		wrapper = CreateNewWrapper(package);
		AssertEquals(nameof(IPackage.NumberOfPacks), 99999999, wrapper.NumberOfPacks);
	}

	public void TestNumberOfPacksReturnsInvalidValueWhenExceedMaximumAllowed()
	{
		package.B5_UnitCount = 100000000;

		var wrapper = CreateNewWrapper(package);
		AssertEquals(nameof(IPackage.NumberOfPacks), -1, wrapper.NumberOfPacks);
	}

	public void TestMarksAndNumbers()
	{
		package.B5_MarksAndNumbers = "MARKS AND NUM";

		var wrapper = CreateNewWrapper(package);
		AssertEquals(nameof(IPackage.MarksAndNumbers), "MARKS AND NUM", wrapper.MarksAndNumbers);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		package = (NctsPackage)goodsItem.Packages.AddNew();
	}

	NctsPackage package;

	IPackage CreateNewWrapper(NctsPackage package) => new PackageWrapper(package);
}
