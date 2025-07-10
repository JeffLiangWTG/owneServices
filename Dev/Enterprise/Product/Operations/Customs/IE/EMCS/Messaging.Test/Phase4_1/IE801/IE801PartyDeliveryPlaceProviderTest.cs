using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE801PartyDeliveryPlaceProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyDeliveryPlaceProvider.NewOrNull(null));
		}

		public void TestTraderid()
		{
			deliveryPlaceTrader.Traderid = "IEDPT001";
			AssertEquals("IEDPT001", ie801PartyDeliveryPlaceProvider.TraderId);
		}

		public void TestTraderName()
		{
			deliveryPlaceTrader.TraderName = "Delivery Place 01";
			AssertEquals("Delivery Place 01", ie801PartyDeliveryPlaceProvider.Name);
		}

		public void TestStreetAndNumber()
		{
			deliveryPlaceTrader.StreetName = "Street A";
			deliveryPlaceTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyDeliveryPlaceProvider.StreetAndNumber);
		}

		public void TestCity()
		{
			deliveryPlaceTrader.City = "Dublin";
			AssertEquals("Dublin", ie801PartyDeliveryPlaceProvider.City);
		}

		public void TestPostcode()
		{
			deliveryPlaceTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyDeliveryPlaceProvider.Postcode);
		}

		public void TestCountry()
		{
			deliveryPlaceTrader.Traderid = "IEDPT001";
			AssertEquals("IE", ie801PartyDeliveryPlaceProvider.Country);
		}

		public void TestLanguage()
		{
			deliveryPlaceTrader.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, ie801PartyDeliveryPlaceProvider.Language);
		}

		public void TestLanguageNull()
		{
			deliveryPlaceTrader.Language = null;
			AssertNoExceptionThrown("Language null.", () => _ = deliveryPlaceTrader.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			deliveryPlaceTrader = new DeliveryPlaceTraderType();
			ie801PartyDeliveryPlaceProvider = IE801PartyDeliveryPlaceProvider.NewOrNull(deliveryPlaceTrader);
		}
		DeliveryPlaceTraderType deliveryPlaceTrader;
		IEMCSPartyDeliveryPlace ie801PartyDeliveryPlaceProvider;
	}
}
