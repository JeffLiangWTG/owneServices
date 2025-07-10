namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class PortProviderTest : Customs.Business.Testing.DataProviderTestCase<PortProvider>
	{
		protected override PortProvider GetProvider() => new PortProvider() { Country = "IE", Location = "DUB", UNLocode = "IEDUB100" };

		public void TestUNLocode()
		{
			AssertEquals("UNLocode", "IEDUB100", Provider.UNLocode);
		}

		public void TestCountry()
		{
			AssertEquals("Country", "IE", Provider.Country);
		}

		public void TestLocation()
		{
			AssertEquals("Location", "DUB", Provider.Location);
		}
	}
}
