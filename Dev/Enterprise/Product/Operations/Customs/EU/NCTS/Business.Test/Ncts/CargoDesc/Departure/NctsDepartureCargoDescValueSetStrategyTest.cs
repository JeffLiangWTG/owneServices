using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsDepartureCargoDescValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestValueSet()
		{
			SetUpTariff(Core.Constants.CountryCodes.Latvia);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.Guarantees.AddNew();
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_ZZF_NKTaxType = ZString.Empty;
			goodsItem.BY_MonetaryValue = 1_000m;

			AssertEquals(344m, guarantee.PW_BondAmount);

			goodsItem.BY_MonetaryValue = 500m;
			AssertEquals(172m, guarantee.PW_BondAmount);

			goodsItem.BY_HarmonisedTariff = "0403909900";
			AssertEquals(304m, guarantee.PW_BondAmount);

			goodsItem.BY_RN_NKCountryOfOrigin = "FR";
			AssertEquals(100m, guarantee.PW_BondAmount);

			goodsItem.BY_ZZF_NKTaxType = "RID";
			AssertEquals(25m, guarantee.PW_BondAmount);
		}

		void SetUpTariff(string countryCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "test country", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			Factory.Save();

			var tariff1 = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, NCTSTestHelper.TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff2 = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "0403909900", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(Factory, "A00", rateType1.PK);
			var reference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var reference2 = helper.CreatePreferenceForCountryAndGrouping("200", "200", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

			var rateType2 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "SEC");
			var rateCode2 = helper.CreateCusRateCode(Factory, "SEC", rateType2.PK);

			var rate11 = helper.CreateRefCusRate(tariff1.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", reference1.PK);
			var rate12 = helper.CreateRefCusRate(tariff1.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference2.PK);
			var rate13 = helper.CreateRefCusRate(tariff1.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference1.PK);

			var rate21 = helper.CreateRefCusRate(tariff2.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.34", reference1.PK);
			var rate22 = helper.CreateRefCusRate(tariff2.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference2.PK);
			var rate23 = helper.CreateRefCusRate(tariff2.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference1.PK);

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var t1 = helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate11.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate12.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate13.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateCusApplicability(rate21.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate22.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate23.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTaxOrFee("RID", 0.05m, countryCode);
			helper.CreateTaxOrFee("ORD", 0.2m, countryCode);
			helper.CreateNewOrGetExistingVATApplicability(tariff1, countryCode, "RID");
			helper.CreateNewOrGetExistingVATApplicability(tariff1, countryCode, "ORD");

			helper.CreateNewOrGetExistingVATApplicability(tariff2, countryCode, "RID");
			helper.CreateNewOrGetExistingVATApplicability(tariff2, countryCode, "ORD");

			Factory.Save();
		}
	}
}
