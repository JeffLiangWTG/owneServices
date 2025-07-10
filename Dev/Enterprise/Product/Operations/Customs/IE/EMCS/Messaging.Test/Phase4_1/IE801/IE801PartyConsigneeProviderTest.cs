using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE801PartyConsigneeProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyConsigneeProvider.NewOrNull(null));
		}

		public void TestEoriNumber()
		{
			AssertEquals(string.Empty, ie801PartyConsigneeProvider.EoriNumber);
		}

		public void TestTraderId()
		{
			consigneeTrader.Traderid = "IECET001";
			AssertEquals("IECET001", ie801PartyConsigneeProvider.TraderId);
		}

		public void TestTraderName()
		{
			consigneeTrader.TraderName = "Consignee01";
			AssertEquals("Consignee01", ie801PartyConsigneeProvider.Name);
		}

		public void TestStreetAndNumber()
		{
			consigneeTrader.StreetName = "Street A";
			consigneeTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyConsigneeProvider.StreetAndNumber);
		}

		public void TestCity()
		{
			consigneeTrader.City = "Dublin";
			AssertEquals("Dublin", ie801PartyConsigneeProvider.City);
		}

		public void TestPostcode()
		{
			consigneeTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyConsigneeProvider.Postcode);
		}

		public void TestCountry()
		{
			consigneeTrader.Traderid = "IECET001";
			AssertEquals("IE", ie801PartyConsigneeProvider.Country);
		}

		public void TestLanguage()
		{
			consigneeTrader.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, ie801PartyConsigneeProvider.Language);
		}

		public void TestLanguageNull()
		{
			consigneeTrader.Language = null;
			AssertNoExceptionThrown("Language null.", () => _ = consigneeTrader.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consigneeTrader = new ConsigneeTraderType();
			ie801PartyConsigneeProvider = IE801PartyConsigneeProvider.NewOrNull(consigneeTrader);
		}
		ConsigneeTraderType consigneeTrader;
		IEMCSPartyConsignee ie801PartyConsigneeProvider;
	}
}
