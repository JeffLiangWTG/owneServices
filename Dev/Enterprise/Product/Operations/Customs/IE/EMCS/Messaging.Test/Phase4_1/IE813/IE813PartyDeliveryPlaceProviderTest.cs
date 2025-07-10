using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE813;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
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

		public void TestStreetAndNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", " ", Provider.StreetAndNumber);

				message.StreetName = "Street A";
				message.StreetNumber = "9";
				AssertEquals("Street A 9", Provider.StreetAndNumber);
			});
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.City);

				message.City = "Berlin";
				AssertEquals("Berlin", Provider.City);
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

				message.Traderid = "DEDPT001";
				AssertEquals("DE", Provider.Country);
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
