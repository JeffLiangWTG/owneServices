using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class CommodityCodeWrapperTest : Customs.Business.Testing.DataProviderTestCase<CommodityCodeWrapper>
	{
		public void TestCombinedNomenclatureCode()
		{
			AssertEquals("Wrapper CombinedNomenclatureCode should equal 7th and 8th digits of item API_Tariff.", "78", Provider.CombinedNomenclatureCode);
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			AssertEquals("Wrapper HarmonizedSystemSubHeadingCode should equal 6 first digits of item API_Tariff.", "234516", Provider.HarmonizedSystemSubHeadingCode);
		}

		protected override CommodityCodeWrapper GetProvider()
		{
			var item = Factory.New<AsycudaPackedItem>();
			item.API_Tariff = "2345167890";
			return CommodityCodeWrapper.New(item);
		}
	}
}
