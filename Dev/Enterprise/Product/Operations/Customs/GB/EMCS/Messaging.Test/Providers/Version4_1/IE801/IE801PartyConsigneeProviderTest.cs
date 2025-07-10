using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
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
			consigneeTrader.Traderid = "GBCET001";
			AssertEquals("GBCET001", ie801PartyConsigneeProvider.TraderId);
		}

		public void TestTraderName()
		{
			consigneeTrader.TraderName = "Consignee01";
			AssertEquals("Consignee01", ie801PartyConsigneeProvider.Name);
		}

		public void TestAddress()
		{
			consigneeTrader.StreetName = "Street A";
			consigneeTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyConsigneeProvider.Address);
		}

		public void TestCity()
		{
			consigneeTrader.City = "London";
			AssertEquals("London", ie801PartyConsigneeProvider.City);
		}

		public void TestPostcode()
		{
			consigneeTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyConsigneeProvider.Postcode);
		}

		public void TestCountry()
		{
			consigneeTrader.Traderid = "GBCET001";
			AssertEquals("GB", ie801PartyConsigneeProvider.Country);
		}

		public void TestLanguage()
		{
			consigneeTrader.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, ie801PartyConsigneeProvider.Language);
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
