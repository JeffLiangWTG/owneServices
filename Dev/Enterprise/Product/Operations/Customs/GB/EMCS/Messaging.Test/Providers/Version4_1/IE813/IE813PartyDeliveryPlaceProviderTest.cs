using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE813PartyDeliveryPlaceProviderTest : Business.Testing.DataProviderTestCase<IE813PartyDeliveryPlaceProvider>
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE813PartyDeliveryPlaceProvider.NewOrNull(null));
		}

		public void TestTraderId()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.TraderId);

				message.Traderid = "DEDPT001";
				AssertEquals("DEDPT001", Provider.TraderId);
			});
		}

		public void TestName()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.Name);

				message.TraderName = "Delivery Place 01";
				AssertEquals("Delivery Place 01", Provider.Name);
			});
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", " ", Provider.Address);

				message.StreetName = "Street A";
				message.StreetNumber = "9";
				AssertEquals("Street A 9", Provider.Address);
			});
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.City);

				message.City = "London";
				AssertEquals("London", Provider.City);
			});
		}

		public void TestPostcode()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.Postcode);

				message.Postcode = "10115";
				AssertEquals("10115", Provider.Postcode);
			});
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, Provider.Country);

				message.Traderid = "GBDPT001";
				AssertEquals("GB", Provider.Country);
			});
		}

		public void TestLanguage()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.Language);

				message.Language = Core.Constants.CountryCodes.France;
				AssertEquals(Core.Constants.CountryCodes.France, Provider.Language);
			});
		}

		protected override IE813PartyDeliveryPlaceProvider GetProvider() => IE813PartyDeliveryPlaceProvider.NewOrNull(message);

		protected override void SetUp()
		{
			base.SetUp();
			message = new DeliveryPlaceTraderType();
		}
		DeliveryPlaceTraderType message;
	}
}
