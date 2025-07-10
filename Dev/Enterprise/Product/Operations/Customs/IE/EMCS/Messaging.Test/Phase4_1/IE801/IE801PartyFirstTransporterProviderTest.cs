using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE801PartyFirstTransporterProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyFirstTransporterProvider.NewOrNull(null));
		}

		public void TestVatNumber()
		{
			firstTransporterTrader.VatNumber = "IEFTT001";
			AssertEquals("FTT001", ie801PartyFirstTransporterProvider.VatNumber);
		}

		public void TestTraderName()
		{
			firstTransporterTrader.TraderName = "Delivery Place 01";
			AssertEquals("Delivery Place 01", ie801PartyFirstTransporterProvider.Name);
		}

		public void TestStreetAndNumber()
		{
			firstTransporterTrader.StreetName = "Street A";
			firstTransporterTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyFirstTransporterProvider.StreetAndNumber);
		}

		public void TestCity()
		{
			firstTransporterTrader.City = "Dublin";
			AssertEquals("Dublin", ie801PartyFirstTransporterProvider.City);
		}

		public void TestPostcode()
		{
			firstTransporterTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyFirstTransporterProvider.Postcode);
		}

		public void TestCountry()
		{
			AssertEquals(string.Empty, ie801PartyFirstTransporterProvider.Country);
			firstTransporterTrader.VatNumber = "IEFTT001";
			AssertEquals("IE", ie801PartyFirstTransporterProvider.Country);
		}

		public void TestLanguage()
		{
			firstTransporterTrader.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, ie801PartyFirstTransporterProvider.Language);
		}

		public void TestLanguageNull()
		{
			firstTransporterTrader.Language = null;
			AssertNoExceptionThrown("Language null.", () => _ = firstTransporterTrader.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			firstTransporterTrader = new FirstTransporterTraderType();
			ie801PartyFirstTransporterProvider = IE801PartyFirstTransporterProvider.NewOrNull(firstTransporterTrader);
		}
		FirstTransporterTraderType firstTransporterTrader;
		IEMCSPartyTransporter ie801PartyFirstTransporterProvider;
	}
}
