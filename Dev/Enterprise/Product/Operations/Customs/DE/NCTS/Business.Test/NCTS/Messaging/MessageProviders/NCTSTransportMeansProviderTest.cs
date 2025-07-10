namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSTransportMeansProvider>
	{
		public void TestTypeOfIdentification()
		{
			AssertEquals("1", Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("DE1234567", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			AssertEquals(Core.Constants.CountryCodes.Germany, Provider.Nationality);
		}

		protected override NCTSTransportMeansProvider GetProvider() => new NCTSTransportMeansProvider("1", "DE1234567", Core.Constants.CountryCodes.Germany);
	}
}
