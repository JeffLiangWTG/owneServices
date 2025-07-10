using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsCustomsTransitOfficeWrapperTest : Customs.Business.Testing.DataProviderTestCase<NctsCustomsTransitOfficeWrapper>
	{
		public void TestCustomsTransitOfficeState()
		{
			AssertEquals("Expected filled CustomsTransitOfficeState", Core.Constants.CountryCodes.Spain, wrapper.CustomsTransitOfficeState);
		}

		public void TestCustomsTransitOfficeCode()
		{
			AssertEquals("Expected filled CustomsTransitOfficeCode", "009999", wrapper.CustomsTransitOfficeCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new NctsCustomsTransitOfficeWrapper("ES009999");
		}
		NctsCustomsTransitOfficeWrapper wrapper;

		protected override NctsCustomsTransitOfficeWrapper GetProvider() => wrapper;
	}
}
