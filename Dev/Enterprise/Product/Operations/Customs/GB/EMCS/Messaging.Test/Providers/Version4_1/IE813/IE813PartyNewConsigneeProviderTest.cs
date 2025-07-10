using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE813PartyNewConsigneeProviderTest : Business.Testing.DataProviderTestCase<IE813PartyNewConsigneeProvider>
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE813PartyNewConsigneeProvider.NewOrNull(null));
		}

		public void TestEoriNumber()
		{
			AssertEquals(string.Empty, Provider.EoriNumber);
		}

		public void TestTraderId()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.TraderId);

				message.Traderid = "GBCET001";
				AssertEquals("GBCET001", Provider.TraderId);
			});
		}

		public void TestName()
		{
			message.TraderName = "Consignee01";
			AssertEquals("Consignee01", Provider.Name);
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				message.StreetName = "Street A";
				AssertEquals("StreeNumber not specified, No exception", "Street A ", Provider.Address);

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

				message.Traderid = "GBCET001";
				AssertEquals("GB", Provider.Country);
			});
		}

		public void TestLanguage()
		{
			message.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, Provider.Language);
		}

		protected override IE813PartyNewConsigneeProvider GetProvider() => IE813PartyNewConsigneeProvider.NewOrNull(message);

		protected override void SetUp()
		{
			base.SetUp();
			message = new NewConsigneeTraderType();
		}
		NewConsigneeTraderType message;
	}
}
