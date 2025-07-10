using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsCommonCargoDescValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestValueSet()
		{
			SetUpTariff(Factory);

			AssertValueSet("Departure Good Item", goodsItemDeparture);
			AssertValueSet("Arrival Good Item", goodsItemArrival);

			void AssertValueSet(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(() =>
				{
					goodsItem.BY_MonetaryValue = 1_000.0m;
					goodsItem.BY_ZZF_NKTaxType = "RID";
					goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
					goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;

					AssertEquals("[PreReq] Duty is calculated", 100.0m, goodsItem.DutyAmount);

					goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
					AssertEquals(string.Format("{0}: Duty is calculated when BY_RN_NKCountryOfOrigin change", assertionText), ZDecimal.Zero, goodsItem.DutyAmount);

					goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;
					goodsItem.BY_HarmonisedTariff = "9111100000";
					AssertEquals(string.Format("{0}: Duty is calculated when BY_HarmonisedTariff change", assertionText), 200.0m, goodsItem.DutyAmount);

					goodsItem.BY_MonetaryValue = 100.0m;
					AssertEquals(string.Format("{0}: Duty is calculated when BY_MonetaryValue change", assertionText), 20.0m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsQuantity = 1.0m;
					goodsItem.BY_CustomsUnitQty = "NAR";
					AssertEquals(string.Format("{0}: Duty is calculated when BY_CustomsUnitQty change", assertionText), 20.5m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsQuantity = 2.0m;
					AssertEquals(string.Format("{0}: Duty is calculated when BY_CustomsQuantity change", assertionText), 21.0m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsQuantity = ZDecimal.Zero;
					goodsItem.BY_CustomsUnitQty = ZString.Empty;
					goodsItem.BY_CustomsSecondQuantity = 1.0m;
					goodsItem.BY_CustomsSecondUnitQty = "NAR";
					AssertEquals(string.Format("{0}: Duty is calculated when BY_CustomsSecondUnitQty change", assertionText), 20.5m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsSecondQuantity = 2.0m;
					AssertEquals(string.Format("{0}: Duty is calculated when BY_CustomsSecondQuantity change", assertionText), 21.0m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsSecondQuantity = ZDecimal.Zero;
					goodsItem.BY_CustomsSecondUnitQty = ZString.Empty;
					goodsItem.BY_CustomsThirdQuantity = 1.0m;
					goodsItem.BY_CustomsThirdUnitQty = "NAR";
					AssertEquals(string.Format("{0}: Duty is calculated when BY_CustomsSecondUnitQty change", assertionText), 20.5m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsThirdQuantity = 2.0m;
					AssertEquals(string.Format("{0}: Duty is calculated when BY_CustomsThirdQuantity change", assertionText), 21.0m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsThirdQuantity = ZDecimal.Zero;
					goodsItem.BY_CustomsThirdUnitQty = ZString.Empty;
					goodsItem.BY_CustomsFourthQuantity = 1.0m;
					goodsItem.BY_CustomsFourthUnitQty = "NAR";
					AssertEquals(string.Format("{0}: Duty is calculated when BY_CustomsSecondUnitQty change", assertionText), 20.5m, goodsItem.DutyAmount);

					goodsItem.BY_CustomsFourthQuantity = 2.0m;
					AssertEquals(string.Format("{0}: Duty is calculated when BY_CustomsFourthQuantity change", assertionText), 21.0m, goodsItem.DutyAmount);
				});
			}
		}

		void SetUpTariff(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia");

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.TariffTypes.Import, nomenclatureGroupType: Core.Constants.CountryCodes.Latvia, ensureDataGroupingExists: false);
			factory.Save();

			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Latvia, tariffType.PK, "9111100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Latvia, tariffType.PK, NCTSTestHelper.TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Latvia, Core.Constants.CountryCodes.Latvia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Latvia);

			var rateTypeDuties = helper.CreateCusRateType(Core.Constants.CountryCodes.Latvia, "DTY");
			var rateCodeDuties = helper.CreateCusRateCode(factory, "A00", rateTypeDuties.PK);
			var rateDuties1 = helper.CreateRefCusRate(tariff1.PK, rateCodeDuties.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0.500 * [NAR] + VFD * 0.2");
			var rateDuties2 = helper.CreateRefCusRate(tariff2.PK, rateCodeDuties.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.1");
			helper.CreateCusApplicability(rateDuties1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rateDuties2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTaxOrFee("RID", 0.2m, Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingVATApplicability(tariff1, Core.Constants.CountryCodes.Latvia, "RID");
			helper.CreateNewOrGetExistingVATApplicability(tariff2, Core.Constants.CountryCodes.Latvia, "RID");

			factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			var headerDeparture = Factory.New<NctsHeader>();
			headerDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			headerDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = headerDeparture.Bills.AddNew();
			goodsItemDeparture = bill.GoodsItems.AddNew();
			goodsItemArrival = NCTSTestHelper.GetArrivalCargoDescTest(Factory);

			Factory.Save();
		}
		NctsCommonCargoDesc goodsItemDeparture;
		NctsCommonCargoDesc goodsItemArrival;
	}
}
