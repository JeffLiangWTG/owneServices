using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

sealed class NctsCommonGoodsItemsIntegratorTest : TestCaseWithFactory
{
	public void TestCopyFromCommonGoodsItem() => CombineAssertions(() =>
	{
		var goodsItem1 = SetUpGoodItem(false);
		var goodsItem2 = SetUpGoodItem(true);

		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var bill = nctsHeader.Bills.AddNew();
		var target1 = bill.GoodsItems.AddNew();
		var target2 = bill.GoodsItems.AddNew();

		var commonGoodsItemsIntegrator = new NctsCommonGoodsItemsIntegrator(nctsHeader);
		commonGoodsItemsIntegrator.CopyFromCommonGoodsItem(goodsItem1, target1);
		commonGoodsItemsIntegrator.CopyFromCommonGoodsItem(goodsItem2, target2);

		AssertGoodItemDetails(target1);
		AssertGoodItemDetails(target2);

		AssertPackage(target1.Packages[0], "BG", 123, "M-1234");
		AssertPackage(target1.Packages[1], "BX", 234, "M-2345");
		AssertPackageWithVehicle(target2.Packages[0], "FR", 1, ZString.Empty, "vin1", "peugeot", "308");
		AssertPackageWithVehicle(target2.Packages[1], "FR", 1, ZString.Empty, "vin2", "opel", "corsa");

		AssertEquals("IsVehicles target1", false, target1.IsVehicles);
		AssertEquals("IsVehicles target2", true, target2.IsVehicles);
	});

	void AssertPackageWithVehicle(NctsPackage package, ZString packageType, ZLong packageCount, ZString marksAndNumbers, ZString vin, ZString brandName, ZString modelName)
	{
		AssertPackage(package, packageType, packageCount, marksAndNumbers);
		AssertEquals("VehicleIdentificationNumber", vin, package.B5_PackageID);
		AssertEquals("BrandName", brandName, package.B5_Brand);
		AssertEquals("ModelName", modelName, package.B5_Model);
	}

	void AssertPackage(NctsPackage package, ZString packageType, ZLong packageCount, ZString marksAndNumbers)
	{
		AssertEquals("PackageType", packageType, package.B5_UnitType);
		AssertEquals("PackageCount", packageCount, package.B5_UnitCount);
		AssertEquals("MarksAndNumbers", marksAndNumbers, package.B5_MarksAndNumbers);
	}

	void AssertGoodItemDetails(NctsDepartureCargoDesc target)
	{
		AssertEquals("GoodsDescription", "description", target.BY_Description);
		AssertEquals("GrossMass", 12000m, target.BY_GrossWeight);
		AssertEquals("NetMass", 10m, target.BY_NetWeight);
		AssertEquals("commodity code", "1202.30.00 01", target.BY_FormattedHarmonisedTariff);
		AssertEquals("GrossMassUnit", "G", target.BY_GrossWeightUnit);
		AssertEquals("NetMassUnit", "KG", target.BY_NetWeightUnit);
		AssertEquals("DestinationCountry", "US", target.BY_RN_NKCountryOfDestination);
		AssertEquals("DispatchCountry", "FR", target.BY_RN_NKCountryOfDispatch);
		AssertEquals("Value", 8m, target.BY_MonetaryValue);
		AssertEquals("SupplementaryQuantity", 1.1m, target.BY_CustomsSecondQuantity);
		AssertEquals("SupplementaryUnitQuantity", "NAR", target.BY_CustomsSecondUnitQty);
		AssertEquals("Number of packages", 2, target.Packages.Count);
	}

	CommonGoodsItem SetUpGoodItem(ZBool hasVehicle)
	{
		return new CommonGoodsItem()
		{
			GoodsDescription = "description",
			GrossMass = 12000m,
			NetMass = 10m,
			GrossMassUnit = "G",
			NetMassUnit = "KG",
			CommodityCode = "1202.30.00 01",
			Value = 8m,
			JobReference = "jobreference",
			DispatchCountry = "FR",
			DestinationCountry = "US",
			SupplementaryQuantity = 1.1m,
			SupplementaryQuantityUnit = "NAR",
			Packages = hasVehicle ? SetUpPackagesWithVehicle() : SetUpPackages()
		};
	}

	CommonPackage[] SetUpPackagesWithVehicle() => new[]
			{
				SetUpPackage("JPB-1", "FR", 1, "", "vin1", "peugeot", "308"),
				SetUpPackage("JPB-2", "FR", 1, "", "vin2", "opel", "corsa")
			};

	CommonPackage[] SetUpPackages() => new[]
			{
				SetUpPackage("JPB-3", "BG", 123, "M-1234"),
				SetUpPackage("JPB-4", "BX", 234, "M-2345")
			};

	CommonPackage SetUpPackage(ZString referenceNumber, ZString packageType, ZInt packageCount, ZString marksAndNumbers, string vin = "", string brandName = "", string modelName = "")
	{
		return new CommonPackage
		{
			BillOrReferenceNumber = referenceNumber,
			PackageType = packageType,
			PackageCount = packageCount,
			MarksAndNumbers = marksAndNumbers,
			VehicleIdentificationNumber = vin,
			BrandName = brandName,
			ModelName = modelName
		};
	}
}
