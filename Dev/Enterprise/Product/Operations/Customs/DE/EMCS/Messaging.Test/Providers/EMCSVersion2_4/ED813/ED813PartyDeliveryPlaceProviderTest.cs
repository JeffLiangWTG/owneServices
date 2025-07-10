using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	class ED813PartyDeliveryPlaceProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED813PartyDeliveryPlaceProvider.NewOrNull(null));
		}

		public void TestTraderid()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyDeliveryPlaceProvider.TraderId);

				deliveryPlaceTrader.Traderid = "DEDPT001";
				AssertEquals("DEDPT001", eD813PartyDeliveryPlaceProvider.TraderId);
			});
		}

		public void TestTraderName()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyDeliveryPlaceProvider.Name);

				deliveryPlaceTrader.TraderName = "Delivery Place 01";
				AssertEquals("Delivery Place 01", eD813PartyDeliveryPlaceProvider.Name);
			});
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", " ", eD813PartyDeliveryPlaceProvider.Address);

				deliveryPlaceTrader.StreetName = "Street A";
				deliveryPlaceTrader.StreetNumber = "9";
				AssertEquals("Street A 9", eD813PartyDeliveryPlaceProvider.Address);
			});
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyDeliveryPlaceProvider.City);

				deliveryPlaceTrader.City = "Berlin";
				AssertEquals("Berlin", eD813PartyDeliveryPlaceProvider.City);
			});
		}

		public void TestPostcode()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyDeliveryPlaceProvider.Postcode);

				deliveryPlaceTrader.Postcode = "10115";
				AssertEquals("10115", eD813PartyDeliveryPlaceProvider.Postcode);
			});
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, eD813PartyDeliveryPlaceProvider.Country);

				deliveryPlaceTrader.Traderid = "DEDPT001";
				AssertEquals("DE", eD813PartyDeliveryPlaceProvider.Country);
			});
		}

		public void TestLanguage()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyDeliveryPlaceProvider.Language);

				deliveryPlaceTrader.NadLng = Core.Constants.CountryCodes.France;
				AssertEquals(Core.Constants.CountryCodes.France, eD813PartyDeliveryPlaceProvider.Language);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			deliveryPlaceTrader = new ED813EBodyChangeOfDestinationDestinationChangedDeliveryPlaceTrader();
			eD813PartyDeliveryPlaceProvider = ED813PartyDeliveryPlaceProvider.NewOrNull(deliveryPlaceTrader);
		}
		ED813EBodyChangeOfDestinationDestinationChangedDeliveryPlaceTrader deliveryPlaceTrader;
		IEMCSPartyDeliveryPlace eD813PartyDeliveryPlaceProvider;
	}
}
