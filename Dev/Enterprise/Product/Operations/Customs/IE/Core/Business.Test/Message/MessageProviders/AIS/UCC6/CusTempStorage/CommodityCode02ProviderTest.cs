using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class CommodityCode02ProviderTest : DataProviderTestCase<CommodityCode02Provider>
	{
		protected override CommodityCode02Provider GetProvider() => CommodityCode02Provider.New("headingCode", "code1");

		public void TestHarmonizedSystemSubHeadingCode()
		{
			AssertEquals("headingCode", "headingCode", GetProvider().HarmonizedSystemSubHeadingCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			AssertEquals("nomenclatureCode", "code1", GetProvider().CombinedNomenclatureCode);
		}
	}
}
