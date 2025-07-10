using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	public class ED801PartyTransportArrangerProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED801PartyTransportArrangerProvider.NewOrNull(null));
		}

		public void TestVatNumber()
		{
			transportArrangerTrader.VatNumber = "DETRT001";
			AssertEquals("TRT001", eD801PartyTransportArrangerProvider.VatNumber);
		}

		public void TestTraderName()
		{
			transportArrangerTrader.TraderName = "Transport Arranger 01";
			AssertEquals("Transport Arranger 01", eD801PartyTransportArrangerProvider.Name);
		}

		public void TestAddress()
		{
			transportArrangerTrader.StreetName = "Street A";
			transportArrangerTrader.StreetNumber = "9";
			AssertEquals("Street A 9", eD801PartyTransportArrangerProvider.Address);
		}

		public void TestCity()
		{
			transportArrangerTrader.City = "Berlin";
			AssertEquals("Berlin", eD801PartyTransportArrangerProvider.City);
		}

		public void TestPostcode()
		{
			transportArrangerTrader.Postcode = "10115";
			AssertEquals("10115", eD801PartyTransportArrangerProvider.Postcode);
		}

		public void TestCountry()
		{
			transportArrangerTrader.VatNumber = "DETRT001";
			AssertEquals("DE", eD801PartyTransportArrangerProvider.Country);
		}

		public void TestLanguage()
		{
			transportArrangerTrader.NadLng = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, eD801PartyTransportArrangerProvider.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			transportArrangerTrader = new ED801EBodyEadContainerTransportArrangerTrader();
			eD801PartyTransportArrangerProvider = ED801PartyTransportArrangerProvider.NewOrNull(transportArrangerTrader);
		}
		ED801EBodyEadContainerTransportArrangerTrader transportArrangerTrader;
		IEMCSPartyTransporter eD801PartyTransportArrangerProvider;
	}
}
