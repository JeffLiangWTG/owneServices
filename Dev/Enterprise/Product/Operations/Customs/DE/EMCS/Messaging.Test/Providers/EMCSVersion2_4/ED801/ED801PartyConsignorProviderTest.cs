using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	public class ED801PartyConsignorProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED801PartyConsignorProvider.NeworNull(null));
		}

		public void TestTraderExciseNumber()
		{
			consignorTrader.TraderExciseNumber = "DETEN001";
			AssertEquals("DETEN001", eD801PartyConsignorProvider.TraderExciseNumber);
		}

		public void TestTraderName()
		{
			consignorTrader.TraderName = "Consignor01";
			AssertEquals("Consignor01", eD801PartyConsignorProvider.Name);
		}

		public void TestAddress()
		{
			consignorTrader.StreetName = "Street A";
			consignorTrader.StreetNumber = "9";
			AssertEquals("Street A 9", eD801PartyConsignorProvider.Address);
		}

		public void TestCity()
		{
			consignorTrader.City = "Berlin";
			AssertEquals("Berlin", eD801PartyConsignorProvider.City);
		}

		public void TestPostcode()
		{
			consignorTrader.Postcode = "10115";
			AssertEquals("10115", eD801PartyConsignorProvider.Postcode);
		}

		public void TestCountry()
		{
			consignorTrader.TraderExciseNumber = "DETEN001";
			AssertEquals("DE", eD801PartyConsignorProvider.Country);
		}

		public void TestLanguage()
		{
			consignorTrader.NadLng = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, eD801PartyConsignorProvider.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consignorTrader = new ED801DBodyEadContainerConsignorTrader();
			eD801PartyConsignorProvider = ED801PartyConsignorProvider.NeworNull(consignorTrader);
		}
		ED801DBodyEadContainerConsignorTrader consignorTrader;
		IEMCSPartyConsignor eD801PartyConsignorProvider;
	}
}
