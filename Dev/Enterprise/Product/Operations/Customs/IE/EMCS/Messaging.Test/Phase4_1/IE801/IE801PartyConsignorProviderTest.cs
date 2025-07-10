using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE801PartyConsignorProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyConsignorProvider.NeworNull(null));
		}

		public void TestTraderExciseNumber()
		{
			consignorTrader.TraderExciseNumber = "IETEN001";
			AssertEquals("IETEN001", ie801PartyConsignorProvider.TraderExciseNumber);
		}

		public void TestTraderName()
		{
			consignorTrader.TraderName = "Consignor01";
			AssertEquals("Consignor01", ie801PartyConsignorProvider.Name);
		}

		public void TestStreetAndNumber()
		{
			consignorTrader.StreetName = "Street A";
			consignorTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyConsignorProvider.StreetAndNumber);
		}

		public void TestCity()
		{
			consignorTrader.City = "Dublin";
			AssertEquals("Dublin", ie801PartyConsignorProvider.City);
		}

		public void TestPostcode()
		{
			consignorTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyConsignorProvider.Postcode);
		}

		public void TestCountry()
		{
			consignorTrader.TraderExciseNumber = "IETEN001";
			AssertEquals("IE", ie801PartyConsignorProvider.Country);
		}

		public void TestLanguage()
		{
			consignorTrader.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, ie801PartyConsignorProvider.Language);
		}

		public void TestLanguageNull()
		{
			consignorTrader.Language = null;
			AssertNoExceptionThrown("Language null.", () => _ = consignorTrader.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consignorTrader = new ConsignorTraderType();
			ie801PartyConsignorProvider = IE801PartyConsignorProvider.NeworNull(consignorTrader);
		}
		ConsignorTraderType consignorTrader;
		IEMCSPartyConsignor ie801PartyConsignorProvider;
	}
}
