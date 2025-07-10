using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE813;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
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

				message.Traderid = "DECET001";
				AssertEquals("DECET001", Provider.TraderId);
			});
		}

		public void TestName()
		{
			message.TraderName = "Consignee01";
			AssertEquals("Consignee01", Provider.Name);
		}

		public void TestStreetAndNumber()
		{
			CombineAssertions(() =>
			{
				message.StreetName = "Street A";
				AssertEquals("StreeNumber not specified, No exception", "Street A ", Provider.StreetAndNumber);

				message.StreetNumber = "9";
				AssertEquals("Street A 9", Provider.StreetAndNumber);
			});
		}

		public void TestCity()
		{
			message.City = "Berlin";
			AssertEquals("Berlin", Provider.City);
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

				message.Traderid = "DECET001";
				AssertEquals("DE", Provider.Country);
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
