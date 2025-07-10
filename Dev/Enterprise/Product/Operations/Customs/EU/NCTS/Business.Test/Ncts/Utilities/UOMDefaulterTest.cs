using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class UOMDefaulterTest : TestCaseWithFactory
	{
		public void TestDefaultUOMIfApplicableDeparture()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Latvia;

			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var esexcTariffType = helper.CreateTariffType(countryCode, "ESEXC");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			CreateNewFormula(helper, esexcTariffType.PK, "0A7", "0.2*VFD", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "0A0", "0.5*[LPA]", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			Factory.Save();

			AssertDefaultUOMIfApplicable("Departure Good Item", goodsItemDeparture);
		}

		public void TestDefaultUOMIfApplicableArrival()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Latvia;

			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var esexcTariffType = helper.CreateTariffType(countryCode, "ESEXC");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			CreateNewFormula(helper, esexcTariffType.PK, "0A7", "0.2*VFD", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, esexcTariffType.PK, "0A0", "0.5*[LPA]", rateType.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			Factory.Save();

			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertDefaultUOMIfApplicable("Arrival Good Item", goodsItemArrival);
			}
		}

		void AssertDefaultUOMIfApplicable(ZString assertionText, NctsCommonCargoDesc goodsItem)
		{
			CombineAssertions(assertionText, () =>
			{
				goodsItem.BY_HarmonisedTariff = "11112222";
				goodsItem.BY_CustomsSecondUnitQty = "LPA";
				if (goodsItem is NctsDepartureCargoDesc departureGoodItem)
				{
					AssertEquals("PreReq: First Unit filled", "KGM", departureGoodItem.CustomsFirstUnitQtyKilograms);
				}

				UOMDefaulter.DefaultUOMIfApplicable(GetRateViewForTest(ZString.Empty, goodsItem), goodsItem);
				AssertEquals("BY_CustomsThirdUnitQty: When no RateView, nothing change", ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When no RateView, nothing change", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				UOMDefaulter.DefaultUOMIfApplicable(GetRateViewForTest("0A7", goodsItem), goodsItem);
				AssertEquals("BY_CustomsThirdUnitQty: When no Unit in Rate Formula, no unit set", ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When no Unit in Rate Formula, no unit set", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				UOMDefaulter.DefaultUOMIfApplicable(GetRateViewForTest("0A0", goodsItem), goodsItem);
				AssertEquals("BY_CustomsThirdUnitQty: When unit is set in First or Second Qty, no unit set", ZString.Empty, goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When unit is set in First or Second Qty, no unit set", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_CustomsSecondUnitQty = "AAA";
				UOMDefaulter.DefaultUOMIfApplicable(GetRateViewForTest("0A0", goodsItem), goodsItem);
				AssertEquals("BY_CustomsThirdUnitQty: When there is unit in First and Second Qty, unit is set in Third", "LPA", goodsItem.BY_CustomsThirdUnitQty);
				AssertEquals("BY_CustomsFourthUnitQty: When Third is empty, no unit set in Fourth", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_CustomsThirdUnitQty = "AAA";
				UOMDefaulter.DefaultUOMIfApplicable(GetRateViewForTest("0A0", goodsItem), goodsItem);
				AssertEquals("BY_CustomsFourthUnitQty: When unit is set in First, Second Qty or Third, unit is set on Fourth", "LPA", goodsItem.BY_CustomsFourthUnitQty);

				goodsItem.BY_CustomsThirdUnitQty = "ASVX";
				goodsItem.BY_CustomsFourthUnitQty = ZString.Empty;
				UOMDefaulter.DefaultUOMIfApplicable(GetRateViewForTest("0A0", goodsItem), goodsItem);
				AssertEquals("BY_CustomsFourthUnitQty: If a related unit is set in First, Second Qty, Third or Fourth, no unit set", ZString.Empty, goodsItem.BY_CustomsFourthUnitQty);
			});
		}

		void CreateNewFormula(Universal.Testing.UniversalReferenceTestDataHelper helper, ZGuid tariffTypePK, string rateCode, string formula, ZGuid rateTypePK, ZGuid impTariffTypePK, ZString tariffCode)
		{
			var tariffExcise = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Latvia, tariffTypePK, rateCode, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
			var rate = helper.LoadOrCreateNewCusRateCode(Factory, rateCode, rateTypePK);
			helper.CreateRate(tariffExcise, rate.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: formula);

			helper.CreateTariffRelationship(tariffExcise.PK, impTariffTypePK, tariffCode);
		}

		RateView GetRateViewForTest(ZString exciseCode, NctsCommonCargoDesc goodsItem) => goodsItem.UniversalTariff?.ChildTariffs.Where(x => x.ZZH_RelatedTariffCode == exciseCode).Select(x => x.RelatedTariffFrom).FirstOrDefault(x => x.ZZ1_ZZI_TariffTypeCode == "ESEXC")?.Rates.FirstOrDefault();

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItemDeparture = header.Bills.AddNew().GoodsItems.AddNew();

			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			goodsItemArrival = arrivalHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			goodsItemArrival.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;

			Factory.Save();
		}

		NctsDepartureCargoDesc goodsItemDeparture;
		NctsArrivalCargoDesc goodsItemArrival;
	}
}
