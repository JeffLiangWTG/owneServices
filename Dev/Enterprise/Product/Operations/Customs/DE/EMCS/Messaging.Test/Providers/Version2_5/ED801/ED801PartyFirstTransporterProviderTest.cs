using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	class ED801PartyFirstTransporterProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED801PartyFirstTransporterProvider.NewOrNull(null));
		}

		public void TestVatNumber()
		{
			firstTransporterTrader.VatNumber = "DEFTT001";
			AssertEquals("FTT001", eD801PartyFirstTransporterProvider.VatNumber);
		}

		public void TestTraderName()
		{
			firstTransporterTrader.TraderName = "Delivery Place 01";
			AssertEquals("Delivery Place 01", eD801PartyFirstTransporterProvider.Name);
		}

		public void TestAddress()
		{
			firstTransporterTrader.StreetName = "Street A";
			firstTransporterTrader.StreetNumber = "9";
			AssertEquals("Street A 9", eD801PartyFirstTransporterProvider.Address);
		}

		public void TestCity()
		{
			firstTransporterTrader.City = "Berlin";
			AssertEquals("Berlin", eD801PartyFirstTransporterProvider.City);
		}

		public void TestPostcode()
		{
			firstTransporterTrader.Postcode = "10115";
			AssertEquals("10115", eD801PartyFirstTransporterProvider.Postcode);
		}

		public void TestCountry()
		{
			AssertEquals(string.Empty, eD801PartyFirstTransporterProvider.Country);
			firstTransporterTrader.VatNumber = "DEFTT001";
			AssertEquals("DE", eD801PartyFirstTransporterProvider.Country);
		}

		public void TestLanguage()
		{
			firstTransporterTrader.NadLng = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, eD801PartyFirstTransporterProvider.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			firstTransporterTrader = new ED801EBodyEadContainerFirstTransporterTrader();
			eD801PartyFirstTransporterProvider = ED801PartyFirstTransporterProvider.NewOrNull(firstTransporterTrader);
		}
		ED801EBodyEadContainerFirstTransporterTrader firstTransporterTrader;
		IEMCSPartyTransporter eD801PartyFirstTransporterProvider;
	}
}

