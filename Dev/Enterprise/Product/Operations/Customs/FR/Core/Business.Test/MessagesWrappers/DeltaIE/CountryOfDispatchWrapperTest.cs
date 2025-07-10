namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class CountryOfDispatchWrapperTest : Customs.Business.Testing.DataProviderTestCase<CountryOfDispatchWrapper>
	{
		protected override CountryOfDispatchWrapper GetProvider()
		{
			return CountryOfDispatchWrapper.New(Core.Constants.CountryCodes.France);
		}

		public void TestCountryOfDispatch()
		{
			AssertEquals("CountryOfDispatch should be equal to Core.Constants.CountryCodes.France entered as parameter of the wrapper.", Core.Constants.CountryCodes.France, Provider.CountryOfDispatch);
		}
	}
}
