using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsPackagePhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB5_PackageID()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = false;
				var package = goodsItem.Packages.AddNew();
				package.Validation.ValidateB5_PackageID();
				AssertNoMessageErrors("When IsVehicles is false no validation is shown", package.B5_PackageIDInfo);

				goodsItem.IsVehicles = true;
				package = goodsItem.Packages.AddNew();
				package.Validation.ValidateB5_PackageID();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(package.B5_PackageIDInfo);

				var arrivalPackage = GetArrivalPackage(Common.CusInBondApplicationCodeList.Codes.NCTS5);
				arrivalPackage.Validation.ValidateB5_PackageID();
				AssertNoMessageErrors("When Arrival no FR validation is not shown", arrivalPackage.B5_PackageIDInfo);

				arrivalPackage.B5_UnitType = "FR";
				arrivalPackage.Validation.ValidateB5_PackageID();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(arrivalPackage.B5_PackageIDInfo);
			});
		}

		public void TestCheckB5_Brand()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = false;
				var package = goodsItem.Packages.AddNew();
				package.Validation.ValidateB5_Brand();
				AssertNoMessageErrors("When IsVehicles is false no validation is shown", package.B5_BrandInfo);

				goodsItem.IsVehicles = true;
				package = goodsItem.Packages.AddNew();
				package.Validation.ValidateB5_Brand();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(package.B5_BrandInfo);

				var arrivalPackage = GetArrivalPackage(Common.CusInBondApplicationCodeList.Codes.NCTS5);
				arrivalPackage.Validation.ValidateB5_Brand();
				AssertNoMessageErrors("When Arrival no FR validation is not shown", arrivalPackage.B5_BrandInfo);

				arrivalPackage.B5_UnitType = "FR";
				arrivalPackage.Validation.ValidateB5_Brand();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(arrivalPackage.B5_BrandInfo);
			});
		}

		public void TestCheckB5_Model()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = false;
				var package = goodsItem.Packages.AddNew();
				package.Validation.ValidateB5_Model();
				AssertNoMessageErrors("When IsVehicles is false no validation is shown", package.B5_ModelInfo);

				goodsItem.IsVehicles = true;
				package = goodsItem.Packages.AddNew();
				package.Validation.ValidateB5_Model();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(package.B5_ModelInfo);

				var arrivalPackage = GetArrivalPackage(Common.CusInBondApplicationCodeList.Codes.NCTS5);
				arrivalPackage.Validation.ValidateB5_Model();
				AssertNoMessageErrors("When Arrival no FR validation is not shown", arrivalPackage.B5_ModelInfo);

				arrivalPackage.B5_UnitType = "FR";
				arrivalPackage.Validation.ValidateB5_Model();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(arrivalPackage.B5_ModelInfo);
			});
		}

		public void TestCheckUnitCountBeZero()
		{
			var nctsDeparture = Factory.NewDepartureNctsHeader();
			nctsDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bills = nctsDeparture.Bills.AddNew();
			var goodsItemCollection = bills.GoodsItems;
			var package1 = PackageTestHelper.GetNewDeparturePackageInNewItem(goodsItemCollection, 1, "CT", 0, "M1");
			var package2 = PackageTestHelper.GetNewDeparturePackageInNewItem(goodsItemCollection, 1, "CT", 5, "M1");

			package1.Validation.ValidateB5_UnitCount();
			package2.Validation.ValidateB5_UnitCount();

			AssertNoNotifications(package1.B5_UnitCountInfo);
			AssertNoNotifications(package2.B5_UnitCountInfo);
		}

		public void TestCheckB5_UnitType_Departure()
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
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				var arrivalGoodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
				var nctsPackage = arrivalGoodsItem.Packages.AddNew();
				nctsPackage.Validation.ValidateAll();
				AssertNoMessageErrorContaining("When Arrival no validation is shown", nctsPackage.B5_UnitTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckB5_UnitType()
		{
			var packType = Factory.SetupStandardPackCusCode();
			var package = goodsItem.Packages.AddNew();
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			package.Validation.ValidateAll();
			ValidationTestHelper.AssertInvalidCodeMessageError(package.B5_UnitTypeInfo, "~Z", packType);
		}

		public void TestCheckB5_TypeOfDifference()
		{
			var package = GetArrivalPackage(Common.CusInBondApplicationCodeList.Codes.NCTS5);
			var typeOfDifferenceInfo = package.B5_TypeOfDifferenceInfo;
			CombineAssertions(() =>
			{
				package.B5_TypeOfDifference = "N/A";
				AssertHasErrorContaining("Invalid Code", typeOfDifferenceInfo, ListValidation.InvalidCodeError);

				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				AssertNoErrorContaining("Valid Code", typeOfDifferenceInfo, ListValidation.InvalidCodeError);
			});
		}

		NctsPackage GetArrivalPackage(string applicationCode)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = applicationCode;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			var arrivalGoodItem = bill.ArrivalGoodsItems.AddNew();
			return arrivalGoodItem.Packages.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			goodsItem = bill.GoodsItems.AddNew();
		}
		NctsHeader header;
		NctsDepartureCargoDesc goodsItem;
	}
}
