using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE801PartyTransportArrangerProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyTransportArrangerProvider.NewOrNull(null));
		}

		public void TestVatNumber()
		{
			transportArrangerTrader.VatNumber = "IETRT001";
			AssertEquals("TRT001", ie801PartyTransportArrangerProvider.VatNumber);
		}

		public void TestTraderName()
		{
			transportArrangerTrader.TraderName = "Transport Arranger 01";
			AssertEquals("Transport Arranger 01", ie801PartyTransportArrangerProvider.Name);
		}

		public void TestStreetAndNumber()
		{
			transportArrangerTrader.StreetName = "Street A";
			transportArrangerTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyTransportArrangerProvider.StreetAndNumber);
		}

		public void TestCity()
		{
			transportArrangerTrader.City = "Dublin";
			AssertEquals("Dublin", ie801PartyTransportArrangerProvider.City);
		}

		public void TestPostcode()
		{
			transportArrangerTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyTransportArrangerProvider.Postcode);
		}

		public void TestCountry()
		{
			transportArrangerTrader.VatNumber = "IETRT001";
			AssertEquals("IE", ie801PartyTransportArrangerProvider.Country);
		}

		public void TestLanguage()
		{
			transportArrangerTrader.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, ie801PartyTransportArrangerProvider.Language);
		}

		public void TestLanguageNull()
		{
			transportArrangerTrader.Language = null;
			AssertNoExceptionThrown("Language null.", () => _ = transportArrangerTrader.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			transportArrangerTrader = new TransportArrangerTraderType();
			ie801PartyTransportArrangerProvider = IE801PartyTransportArrangerProvider.NewOrNull(transportArrangerTrader);
		}
		TransportArrangerTraderType transportArrangerTrader;
		IEMCSPartyTransporter ie801PartyTransportArrangerProvider;
	}
}
