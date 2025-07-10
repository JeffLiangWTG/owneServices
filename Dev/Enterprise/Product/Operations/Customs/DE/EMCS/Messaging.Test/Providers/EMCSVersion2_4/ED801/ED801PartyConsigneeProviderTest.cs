using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	class ED801PartyConsigneeProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED801PartyConsigneeProvider.NewOrNull(null));
		}

		public void TestEoriNumber()
		{
			AssertEquals(string.Empty, eD801PartyConsigneeProvider.EoriNumber);
		}

		public void TestTraderId()
		{
			consigneeTrader.Traderid = "DECET001";
			AssertEquals("DECET001", eD801PartyConsigneeProvider.TraderId);
		}

		public void TestTraderName()
		{
			consigneeTrader.TraderName = "Consignee01";
			AssertEquals("Consignee01", eD801PartyConsigneeProvider.Name);
		}

		public void TestAddress()
		{
			consigneeTrader.StreetName = "Street A";
			consigneeTrader.StreetNumber = "9";
			AssertEquals("Street A 9", eD801PartyConsigneeProvider.Address);
		}

		public void TestCity()
		{
			consigneeTrader.City = "Berlin";
			AssertEquals("Berlin", eD801PartyConsigneeProvider.City);
		}

		public void TestPostcode()
		{
			consigneeTrader.Postcode = "10115";
			AssertEquals("10115", eD801PartyConsigneeProvider.Postcode);
		}

		public void TestCountry()
		{
			consigneeTrader.Traderid = "DECET001";
			AssertEquals("DE", eD801PartyConsigneeProvider.Country);
		}

		public void TestLanguage()
		{
			consigneeTrader.NadLng = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, eD801PartyConsigneeProvider.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consigneeTrader = new ED801DBodyEadContainerConsigneeTrader();
			eD801PartyConsigneeProvider = ED801PartyConsigneeProvider.NewOrNull(consigneeTrader);
		}
		ED801DBodyEadContainerConsigneeTrader consigneeTrader;
		IEMCSPartyConsignee eD801PartyConsigneeProvider;
	}
}
