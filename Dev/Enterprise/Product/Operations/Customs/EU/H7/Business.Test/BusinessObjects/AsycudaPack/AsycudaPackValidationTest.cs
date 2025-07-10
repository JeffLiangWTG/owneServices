using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPA_PackQty()
		{
			var pack = Factory.New<AsycudaPack>();

			pack.APA_PackQty = 0;
			pack.Validation.ValidateAPA_PackQty();
			AssertHasMessageError(pack.APA_PackQtyInfo, "Quantity (on Pack) must be greater than 0.");

			pack.APA_PackQty = -1;
			pack.Validation.ValidateAPA_PackQty();
			AssertHasError(pack.APA_PackQtyInfo, "Quantity (on Pack) must be greater than 0.");

			pack.APA_PackQty = 12;
			pack.Validation.ValidateAPA_PackQty();
			AssertNoErrors("no message error as it is greater than 0", pack.APA_PackQtyInfo);
		}

		public void TestCheckAPA_PackUQ()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "pack type");

			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes,
				"ABC",
				"desc",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var pack = Factory.New<AsycudaPack>();
			ValidationTestHelper.AssertInvalidCodeMessageError(pack.APA_PackUQInfo, "XXX", "ABC");
		}

		public void TestCheckAPA_GoodsDescription()
		{
			var pack = Factory.New<AsycudaPack>();

			pack.APA_GoodsDescription = string.Empty;
			AssertNoMessageErrors(pack.APA_GoodsDescriptionInfo);

			pack.APA_GoodsDescription = "NormalDescription";
			AssertNoMessageErrors(pack.APA_GoodsDescriptionInfo);

			pack.APA_GoodsDescription = "%#中文字符[!";
			AssertHasMessageError(pack.APA_GoodsDescriptionInfo, "Goods Description can only be alphanumeric.");

			pack.APA_GoodsDescription = new ZString('a', 513);
			AssertHasMessageError(pack.APA_GoodsDescriptionInfo, "Goods Description has exceeded the max length of 512.");
		}
	}
}
