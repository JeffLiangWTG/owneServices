using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class GoodsItemDutyCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");

			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var esexcTariffType = helper.CreateTariffType(countryCode, "ESEXC");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, esexcTariffType.PK, "0A7", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
			var rate = helper.LoadOrCreateNewCusRateCode(Factory, "0A7", rateType.PK);
			helper.CreateRate(tariffExcise, rate.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");

			helper.CreateTariffRelationship(tariffExcise.PK, expTariffType.PK, tariff.ZZ1_TariffCode);

			Factory.Save();

			goodsItem.BY_HarmonisedTariff = "11112222";
			goodsItem.ExciseCode = "0A7";
			goodsItem.PVPValue = 30;
			var result = new GoodsItemDutyCalculator(goodsItem).Calculate(goodsItem.CurrentExciseRate).ResultAmount;
			AssertEquals("Calculate had Spanish CountrySpecificValueList", 6m, result);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			Factory.Save();
		}

		NctsDepartureCargoDesc goodsItem;
	}
}
