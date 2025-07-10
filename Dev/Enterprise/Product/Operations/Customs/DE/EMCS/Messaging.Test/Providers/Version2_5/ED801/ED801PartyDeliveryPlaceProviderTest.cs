using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	public class ED801PartyDeliveryPlaceProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED801PartyDeliveryPlaceProvider.NewOrNull(null));
		}

		public void TestTraderid()
		{
			deliveryPlaceTrader.Traderid = "DEDPT001";
			AssertEquals("DEDPT001", eD801PartyDeliveryPlaceProvider.TraderId);
		}

		public void TestTraderName()
		{
			deliveryPlaceTrader.TraderName = "Delivery Place 01";
			AssertEquals("Delivery Place 01", eD801PartyDeliveryPlaceProvider.Name);
		}

		public void TestAddress()
		{
			deliveryPlaceTrader.StreetName = "Street A";
			deliveryPlaceTrader.StreetNumber = "9";
			AssertEquals("Street A 9", eD801PartyDeliveryPlaceProvider.Address);
		}

		public void TestCity()
		{
			deliveryPlaceTrader.City = "Berlin";
			AssertEquals("Berlin", eD801PartyDeliveryPlaceProvider.City);
		}

		public void TestPostcode()
		{
			deliveryPlaceTrader.Postcode = "10115";
			AssertEquals("10115", eD801PartyDeliveryPlaceProvider.Postcode);
		}

		public void TestCountry()
		{
			deliveryPlaceTrader.Traderid = "DEDPT001";
			AssertEquals("DE", eD801PartyDeliveryPlaceProvider.Country);
		}

		public void TestLanguage()
		{
			deliveryPlaceTrader.NadLng = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, eD801PartyDeliveryPlaceProvider.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			deliveryPlaceTrader = new ED801EBodyEadContainerDeliveryPlaceTrader();
			eD801PartyDeliveryPlaceProvider = ED801PartyDeliveryPlaceProvider.NewOrNull(deliveryPlaceTrader);
		}
		ED801EBodyEadContainerDeliveryPlaceTrader deliveryPlaceTrader;
		IEMCSPartyDeliveryPlace eD801PartyDeliveryPlaceProvider;
	}
}
