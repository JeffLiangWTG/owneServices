using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE813PartyNewTransporterProviderTest : Business.Testing.DataProviderTestCase<IE813PartyNewTransporterProvider>
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE813PartyNewTransporterProvider.NewOrNull(null));
		}

		public void TestVatNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, Provider.VatNumber);

				message.VatNumber = "GBFTT001";
				AssertEquals("FTT001", Provider.VatNumber);
			});
		}

		public void TestName()
		{
			message.TraderName = "Delivery Place 01";
			AssertEquals("Delivery Place 01", Provider.Name);
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				message.StreetName = "Street A";
				AssertEquals("StreetNumber not specified, No exception", "Street A ", Provider.Address);

				message.StreetNumber = "9";
				AssertEquals("Street A 9", Provider.Address);
			});
		}

		public void TestCity()
		{
			message.City = "London";
			AssertEquals("London", Provider.City);
		}

		public void TestPostcode()
		{
			message.Postcode = "10115";
			AssertEquals("10115", Provider.Postcode);
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, Provider.Country);

				message.VatNumber = "GBFTT001";
				AssertEquals("GB", Provider.Country);
			});
		}

		public void TestLanguage()
		{
			message.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, Provider.Language);
		}

		protected override IE813PartyNewTransporterProvider GetProvider() => IE813PartyNewTransporterProvider.NewOrNull(message);

		protected override void SetUp()
		{
			base.SetUp();
			message = new NewTransporterTraderType();
		}
		NewTransporterTraderType message;
	}
}
