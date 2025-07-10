using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class GoodsItemRateCalcDataTest : TestCaseWithFactory
	{
		public void TestCountrySpecificValueList()
		{
			goodsItem.PVPValue = 1m;
			var goodsItemRateData = new GoodsItemRateCalcData(goodsItem);

			AssertContainsKey(goodsItemRateData.CountrySpecificValueList, UniversalReferenceConstants.ReservedRateFormulaValue.RetailPriceCode);
			AssertContainsValue(goodsItemRateData.CountrySpecificValueList, UniversalReferenceConstants.ReservedRateFormulaValue.RetailPriceCode, 1m);
		}

		void AssertContainsKey(IDictionary<string, decimal> dictionary, string key) => Assert($"Expected key '{key}' in dictionary.", !string.IsNullOrEmpty(key) || dictionary.ContainsKey(key));

		void AssertContainsValue(IDictionary<string, decimal> dictionary, string key, decimal value) => AssertEquals($"Expected value '{value}' for key '{key}'.", value, dictionary[key]);

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
