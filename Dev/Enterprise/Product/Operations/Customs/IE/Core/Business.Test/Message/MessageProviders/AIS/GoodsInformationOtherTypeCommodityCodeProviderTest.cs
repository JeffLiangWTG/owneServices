using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class GoodsInformationOtherTypeCommodityCodeProviderTest : DataProviderTestCase<GoodsInformationOtherTypeCommodityCodeProvider>
	{
		public void TestIGoodsInformationOtherTypeCommodityCode()
		{
			Assert("Should implement IGoodsInformationOtherTypeCommodityCode", Provider is IGoodsInformationOtherTypeCommodityCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			tariff = "8703231100";
			AssertEquals("87032311", Provider.CombinedNomenclatureCode);
		}

		public void TestTaricCode()
		{
			tariff = "8703231100";
			AssertEquals("00", Provider.TaricCode);
		}

		protected override GoodsInformationOtherTypeCommodityCodeProvider GetProvider() => new GoodsInformationOtherTypeCommodityCodeProvider(tariff);

		ZString tariff;
	}
}
