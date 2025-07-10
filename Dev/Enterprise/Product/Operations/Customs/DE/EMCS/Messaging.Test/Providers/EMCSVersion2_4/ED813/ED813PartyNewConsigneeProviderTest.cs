using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	class ED813PartyConsigneeProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED813PartyNewConsigneeProvider.NewOrNull(null));
		}

		public void TestEoriNumber()
		{
			AssertEquals(string.Empty, eD813PartyNewConsigneeProvider.EoriNumber);
		}

		public void TestTraderId()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyNewConsigneeProvider.TraderId);

				newConsigneeTrader.Traderid = "DECET001";
				AssertEquals("DECET001", eD813PartyNewConsigneeProvider.TraderId);
			});
		}

		public void TestTraderName()
		{
			newConsigneeTrader.TraderName = "Consignee01";
			AssertEquals("Consignee01", eD813PartyNewConsigneeProvider.Name);
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				newConsigneeTrader.StreetName = "Street A";
				AssertEquals("StreeNumber not specified, No exception", "Street A ", eD813PartyNewConsigneeProvider.Address);

				newConsigneeTrader.StreetNumber = "9";
				AssertEquals("Street A 9", eD813PartyNewConsigneeProvider.Address);
			});
		}

		public void TestCity()
		{
			newConsigneeTrader.City = "Berlin";
			AssertEquals("Berlin", eD813PartyNewConsigneeProvider.City);
		}

		public void TestPostcode()
		{
			newConsigneeTrader.Postcode = "10115";
			AssertEquals("10115", eD813PartyNewConsigneeProvider.Postcode);
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, eD813PartyNewConsigneeProvider.Country);

				newConsigneeTrader.Traderid = "DECET001";
				AssertEquals("DE", eD813PartyNewConsigneeProvider.Country);
			});
		}

		public void TestLanguage()
		{
			newConsigneeTrader.NadLng = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, eD813PartyNewConsigneeProvider.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			newConsigneeTrader = new ED813EBodyChangeOfDestinationDestinationChangedNewConsigneeTrader();
			eD813PartyNewConsigneeProvider = ED813PartyNewConsigneeProvider.NewOrNull(newConsigneeTrader);
		}
		ED813EBodyChangeOfDestinationDestinationChangedNewConsigneeTrader newConsigneeTrader;
		IEMCSPartyConsignee eD813PartyNewConsigneeProvider;
	}
}
