using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.CodeDescriptionPairLists.Testing
{
	class NationalFeeTypeCodeListTest : TestCaseWithFactory
	{
		public void TestNationalFeeTypeCodeListExcluteParentDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", euGrouping);
			var nationalFeeTypeCode = "U165_Description";
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, UniversalReferenceConstants.RefCusRateCodes.U165, true, false, nationalFeeTypeCode);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.France, "HSN");
			Factory.Save();

			var oldVATCode = "N434";
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.France, tariffType.PK, "11111111", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "ZA1", startDate: ZDateTime.BrettsBirthday, endDate: ZDateTime.Today.AddDays(1), category: oldVATCode);
			Factory.Save();

			var list = new NationalFeeTypeCodeList(Factory, tariff, ZDateTime.Today);

			Assert("There should be FR code in NationalFeeTypeCodeList, and should not be EU code in NationalFeeTypeCodeList.", list.Count == 2);
			AssertEquals("There are FR VAT code in NationalFeeTypeCodeList.", list[0].Description, oldVATCode);
			AssertEquals("There are FR code in NationalFeeTypeCodeList.", list[1].Description, nationalFeeTypeCode);

			var tariffType2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.France, "HSM");
			Factory.Save();

			var newVATCode = "N435";
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.France, tariffType2.PK, "2222222", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingVATApplicability(tariff2, Core.Constants.CountryCodes.France, "ZA1", startDate: ZDateTime.BrettsBirthday, endDate: ZDateTime.Today.AddDays(1), category: newVATCode);
			Factory.Save();

			list = new NationalFeeTypeCodeList(Factory, tariff2, ZDateTime.Today);

			Assert("There should be FR code in NationalFeeTypeCodeList, and should not be EU code in NationalFeeTypeCodeList.", list.Count == 2);
			AssertEquals("The old VAT code should not exists in the NationalFeeTypeCodeList", list[0].Description, newVATCode);
			AssertEquals("There are FR code in NationalFeeTypeCodeList.", list[1].Description, nationalFeeTypeCode);
		}
	}
}
