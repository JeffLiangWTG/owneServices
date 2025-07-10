using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE801PartyConsignorProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyConsignorProvider.NeworNull(null));
		}

		public void TestTraderExciseNumber()
		{
			consignorTrader.TraderExciseNumber = "GBTEN001";
			AssertEquals("GBTEN001", ie801PartyConsignorProvider.TraderExciseNumber);
		}

		public void TestTraderName()
		{
			consignorTrader.TraderName = "Consignor01";
			AssertEquals("Consignor01", ie801PartyConsignorProvider.Name);
		}

		public void TestAddress()
		{
			consignorTrader.StreetName = "Street A";
			consignorTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyConsignorProvider.Address);
		}

		public void TestCity()
		{
			consignorTrader.City = "London";
			AssertEquals("London", ie801PartyConsignorProvider.City);
		}

		public void TestPostcode()
		{
			consignorTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyConsignorProvider.Postcode);
		}

		public void TestCountry()
		{
			consignorTrader.TraderExciseNumber = "GBTEN001";
			AssertEquals("GB", ie801PartyConsignorProvider.Country);
		}

		public void TestLanguage()
		{
			consignorTrader.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, ie801PartyConsignorProvider.Language);
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
