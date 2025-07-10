using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsPackagePhase4ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB5_PackageID()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = false;
				var package = goodsItem.Packages.AddNew();
				package.Validation.ValidateAll();
				AssertNoMessageErrors("When IsVehicles is false no validation is shown", package.B5_PackageIDInfo);

				goodsItem.IsVehicles = true;
				package = goodsItem.Packages.AddNew();
				package.Validation.ValidateAll();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(package.B5_PackageIDInfo, "You have not entered a VIN");

				header.SetMovementType(NctsMovementType.Codes.Arrival);
				var arrivalGoodItem = header.MovementHeader.GoodsItems.AddNew();
				package = arrivalGoodItem.Packages.AddNew();
				package.Validation.ValidateAll();
				AssertNoMessageErrors("When Arrival no validation is shown", package.B5_PackageIDInfo);
			});
		}

		public void TestCheckB5_Brand()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = false;
				var package = goodsItem.Packages.AddNew();
				package.Validation.ValidateAll();
				AssertNoMessageErrors("When IsVehicles is false no validation is shown", package.B5_BrandInfo);

				goodsItem.IsVehicles = true;
				package = goodsItem.Packages.AddNew();
				package.Validation.ValidateAll();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(package.B5_BrandInfo, "You have not entered a Brand");

				header.SetMovementType(NctsMovementType.Codes.Arrival);
				var arrivalGoodItem = header.MovementHeader.GoodsItems.AddNew();
				package = arrivalGoodItem.Packages.AddNew();
				package.Validation.ValidateAll();
				AssertNoMessageErrors("When Arrival no validation is shown", package.B5_BrandInfo);
			});
		}

		public void TestCheckB5_Model()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = false;
				var package = goodsItem.Packages.AddNew();
				package.Validation.ValidateAll();
				AssertNoMessageErrors("When IsVehicles is false no validation is shown", package.B5_ModelInfo);

				goodsItem.IsVehicles = true;
				package = goodsItem.Packages.AddNew();
				package.Validation.ValidateAll();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(package.B5_ModelInfo, "You have not entered a Model");

				header.SetMovementType(NctsMovementType.Codes.Arrival);
				var arrivalGoodItem = header.MovementHeader.GoodsItems.AddNew();
				package = arrivalGoodItem.Packages.AddNew();
				package.Validation.ValidateAll();
				AssertNoMessageErrors("When Arrival no validation is shown", package.B5_ModelInfo);
			});
		}

		public void TestCheckUnitCountBeZero()
		{
			var nctsDeparture = Factory.NewDepartureNctsHeader();
			var goodsItemCollection = nctsDeparture.MovementHeader.GoodsItems;
			var package1 = PackageTestHelper.GetNewDeparturePackageInNewItem(goodsItemCollection, 1, "CT", 0, "M1");
			var package2 = PackageTestHelper.GetNewDeparturePackageInNewItem(goodsItemCollection, 1, "CT", 5, "M1");

			package1.Validation.ValidateB5_UnitCount();
			package2.Validation.ValidateB5_UnitCount();

			AssertNoNotifications(package1.B5_UnitCountInfo);
			AssertNoNotifications(package2.B5_UnitCountInfo);
		}

		public void TestCheckUnitCount()
		{
			var expectedMessageError = "Quantity can be zero only if 'Type' and 'Marks and Numbers' are the same as those of the other line item.";

			var nctsDeparture = Factory.NewDepartureNctsHeader();
			var goodsItemCollection = nctsDeparture.MovementHeader.GoodsItems;
			var goodsItemCollection1 = nctsDeparture.MovementHeader.GoodsItems;

			CombineAssertions(() =>
			{
				var package1 = PackageTestHelper.GetNewDeparturePackageInNewItem(goodsItemCollection, 1, "CT", 5, "M1");
				AssertNoMessageErrorContaining("Only one and UnitCount is not 0 (1).", package1.B5_UnitCountInfo, expectedMessageError);
				var package2 = PackageTestHelper.GetNewDeparturePackageInNewItem(goodsItemCollection, 1, "CT", 0, "M2");
				AssertHasMessageErrorContaining("Two, with 0 UnitCount and differents marks (2).", package2.B5_UnitCountInfo, expectedMessageError);
				var package3 = PackageTestHelper.GetNewDeparturePackageInNewItem(goodsItemCollection, 1, "CT", 0, "M1");
				AssertNoMessageErrorContaining("Three, with 0 UnitCount and the same that first (3).", package3.B5_UnitCountInfo, expectedMessageError);
				var package4 = PackageTestHelper.GetNewDeparturePackageInNewItem(goodsItemCollection, 1, "CT", 3, "M2");
				AssertNoMessageErrorContaining("Four, with not 0 UnitCount and the same that second (4).", package4.B5_UnitCountInfo, expectedMessageError);
				var package5 = PackageTestHelper.GetNewDeparturePackageInNewItem(goodsItemCollection, 1, "CT", 3, "DIFFGOODSITEM");
				var package6 = PackageTestHelper.GetNewDeparturePackageInNewItem(goodsItemCollection1, 2, "CT", 0, "DIFFGOODSITEM");
				AssertNoMessageErrorContaining("With 0 UnitCount in differents goods items.", package6.B5_UnitCountInfo, expectedMessageError);
			});
		}

		public void TestCheckB5_UnitType()
		{
			goodsItem.IsVehicles = true;
			var package = goodsItem.Packages.AddNew();
			package.Validation.ValidateAll();
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining("When IsVehicles is true no validation is shown", package.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);

				goodsItem.IsVehicles = false;
				package = goodsItem.Packages.AddNew();
				package.Validation.ValidateAll();
				AssertHasMessageErrorContaining("When IsVehicles is false validation is shown", package.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);

				package.B5_UnitType = "U";
				package.Validation.ValidateAll();
				AssertNoMessageErrorContaining("When IsVehicles is false and Unit Type is filled no validation shown", package.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);

				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
				var arrivalGoodsItem = arrivalMovementHeader.GoodsItems.AddNew();
				var nctsPackage = arrivalGoodsItem.Packages.AddNew();
				nctsPackage.Validation.ValidateAll();
				AssertNoMessageErrorContaining("When Arrival no validation is shown", nctsPackage.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = header.MovementHeader.GoodsItems.AddNew();
		}
		NctsHeader header;
		NctsDepartureCargoDesc goodsItem;
	}
}
