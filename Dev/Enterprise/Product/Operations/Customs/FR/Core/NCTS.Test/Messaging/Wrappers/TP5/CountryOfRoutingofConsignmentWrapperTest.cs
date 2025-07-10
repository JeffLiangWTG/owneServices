namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CountryOfRoutingofConsignmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<CountryOfRoutingofConsignmentWrapper>
	{
		public void TestCountry()
		{
			AssertEquals("Country should equal to value passed to wrapper.", "YT", Provider.Country);
		}

		protected override CountryOfRoutingofConsignmentWrapper GetProvider()
		{
			return CountryOfRoutingofConsignmentWrapper.New("YT");
		}
	}
}
