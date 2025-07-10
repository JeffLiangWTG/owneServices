using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class CommodityDetailDataTest : TestCase
	{
		public void TestConstructor()
		{
			CommodityDetailData commodity = new CommodityDetailData("Blah1", "Blah2", Core.Constants.CountryCodes.Australia, 1032.2m);
			AssertEquals("Goods Description", "Blah1", commodity.GoodsDescription);
			AssertEquals("Tariff Number", "Blah2", commodity.TariffNumber);
			AssertEquals("Country Of Origin", Core.Constants.CountryCodes.Australia, commodity.CountryOfOrigin);
			AssertEquals("Item Price", 1032.2m, commodity.ItemPrice);
		}

		public void TestWeight()
		{
			CommodityDetailData commodity = new CommodityDetailData("", "", "", 0m);
			AssertEquals("Weight", ZDecimal.Zero, commodity.Weight);
		}

		public void TestCPC()
		{
			CommodityDetailData commodity = new CommodityDetailData("", "", "", 0m);
			AssertEquals("CPC", ZString.Empty, commodity.CPC);
		}
	}
}
