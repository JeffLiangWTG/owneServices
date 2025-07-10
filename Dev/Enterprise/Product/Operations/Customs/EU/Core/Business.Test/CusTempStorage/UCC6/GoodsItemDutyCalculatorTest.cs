using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	class GoodsItemDutyCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;

			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var esexcTariffType = helper.CreateTariffType(countryCode, "ESEXC");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, esexcTariffType.PK, "0A7", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
			var rate = helper.LoadOrCreateNewCusRateCode(Factory, "0A7", rateType.PK);
			var rateView = helper.CreateRate(tariffExcise, rate.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");

			helper.CreateTariffRelationship(tariffExcise.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			Factory.Save();

			goodsItem.API_Tariff = "11112222";
			goodsItem.API_GoodsValue = 30;
			goodsItem.API_RN_NKGoodsOrigin = countryCode;
			goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;

			var result = new GoodsItemDutyCalculator(goodsItem).Calculate(rateView).ResultAmount;
			AssertEquals("Calculate had Spanish CountrySpecificValueList", 6m, result);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = "ES";
			bill = header.Bills.AddNew();
			goodsItem = SetUpPackedItemForTest();

			Factory.Save();
		}

		TemporaryStorageBill bill;
		TemporaryStoragePackedItemForTest goodsItem;

		TemporaryStoragePackedItemForTest SetUpPackedItemForTest()
		{
			var item = Factory.New<TemporaryStoragePackedItemForTest>();
			bill.PackedItems.Add(item);
			return item;
		}
	}
}
