using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE801PartyTransportArrangerProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyTransportArrangerProvider.NewOrNull(null));
		}

		public void TestVatNumber()
		{
			transportArrangerTrader.VatNumber = "GBTRT001";
			AssertEquals("TRT001", ie801PartyTransportArrangerProvider.VatNumber);
		}

		public void TestTraderName()
		{
			transportArrangerTrader.TraderName = "Transport Arranger 01";
			AssertEquals("Transport Arranger 01", ie801PartyTransportArrangerProvider.Name);
		}

		public void TestAddress()
		{
			transportArrangerTrader.StreetName = "Street A";
			transportArrangerTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyTransportArrangerProvider.Address);
		}

		public void TestCity()
		{
			transportArrangerTrader.City = "London";
			AssertEquals("London", ie801PartyTransportArrangerProvider.City);
		}

		public void TestPostcode()
		{
			transportArrangerTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyTransportArrangerProvider.Postcode);
		}

		public void TestCountry()
		{
			transportArrangerTrader.VatNumber = "GBTRT001";
			AssertEquals("GB", ie801PartyTransportArrangerProvider.Country);
		}

		public void TestLanguage()
		{
			transportArrangerTrader.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, ie801PartyTransportArrangerProvider.Language);
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
