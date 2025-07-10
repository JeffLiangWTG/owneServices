using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CommodityCodeWrapperTest : Customs.Business.Testing.DataProviderTestCase<CommodityCodeWrapper>
	{
		public void TestHarmonizedSystemSubHeadingCode()
		{
			AssertEquals("HarmonizedSystemSubHeadingCode should be mapped to tariff first 6 digits.", "112233", Provider.HarmonizedSystemSubHeadingCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			AssertEquals("CombinedNomenclatureCode should be mapped to tariff 7th and 8th digits.", "44", Provider.CombinedNomenclatureCode);
		}

		protected override CommodityCodeWrapper GetProvider()
		{
			ZString harmonisedTariff = "1122334455";
			return CommodityCodeWrapper.New(harmonisedTariff);
		}
	}
}
