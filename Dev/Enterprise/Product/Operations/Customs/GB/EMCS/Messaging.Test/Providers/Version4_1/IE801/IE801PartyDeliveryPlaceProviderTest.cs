using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE801PartyDeliveryPlaceProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyDeliveryPlaceProvider.NewOrNull(null));
		}

		public void TestTraderid()
		{
			deliveryPlaceTrader.Traderid = "GBDPT001";
			AssertEquals("GBDPT001", ie801PartyDeliveryPlaceProvider.TraderId);
		}

		public void TestTraderName()
		{
			deliveryPlaceTrader.TraderName = "Delivery Place 01";
			AssertEquals("Delivery Place 01", ie801PartyDeliveryPlaceProvider.Name);
		}

		public void TestAddress()
		{
			deliveryPlaceTrader.StreetName = "Street A";
			deliveryPlaceTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyDeliveryPlaceProvider.Address);
		}

		public void TestCity()
		{
			deliveryPlaceTrader.City = "London";
			AssertEquals("London", ie801PartyDeliveryPlaceProvider.City);
		}

		public void TestPostcode()
		{
			deliveryPlaceTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyDeliveryPlaceProvider.Postcode);
		}

		public void TestCountry()
		{
			deliveryPlaceTrader.Traderid = "GBDPT001";
			AssertEquals("GB", ie801PartyDeliveryPlaceProvider.Country);
		}

		public void TestLanguage()
		{
			deliveryPlaceTrader.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, ie801PartyDeliveryPlaceProvider.Language);
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
