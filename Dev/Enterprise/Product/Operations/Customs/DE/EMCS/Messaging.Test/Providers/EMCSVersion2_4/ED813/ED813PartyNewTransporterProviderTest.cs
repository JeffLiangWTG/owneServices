using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	class ED813PartyNewTransporterProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED813PartyNewTransporterProvider.NewOrNull(null));
		}

		public void TestVatNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, eD813PartyNewTransporterProvider.VatNumber);

				newTransporterTrader.VatNumber = "DEFTT001";
				AssertEquals("FTT001", eD813PartyNewTransporterProvider.VatNumber);
			});
		}

		public void TestTraderName()
		{
			newTransporterTrader.TraderName = "Delivery Place 01";
			AssertEquals("Delivery Place 01", eD813PartyNewTransporterProvider.Name);
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				newTransporterTrader.StreetName = "Street A";
				AssertEquals("StreetNumber not specified, No exception", "Street A ", eD813PartyNewTransporterProvider.Address);

				newTransporterTrader.StreetNumber = "9";
				AssertEquals("Street A 9", eD813PartyNewTransporterProvider.Address);
			});
		}

		public void TestCity()
		{
			newTransporterTrader.City = "Berlin";
			AssertEquals("Berlin", eD813PartyNewTransporterProvider.City);
		}

		public void TestPostcode()
		{
			newTransporterTrader.Postcode = "10115";
			AssertEquals("10115", eD813PartyNewTransporterProvider.Postcode);
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, eD813PartyNewTransporterProvider.Country);

				newTransporterTrader.VatNumber = "DEFTT001";
				AssertEquals("DE", eD813PartyNewTransporterProvider.Country);
			});
		}

		public void TestLanguage()
		{
			newTransporterTrader.NadLng = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, eD813PartyNewTransporterProvider.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			newTransporterTrader = new ED813EBodyChangeOfDestinationNewTransporterTrader();
			eD813PartyNewTransporterProvider = ED813PartyNewTransporterProvider.NewOrNull(newTransporterTrader);
		}
		ED813EBodyChangeOfDestinationNewTransporterTrader newTransporterTrader;
		IEMCSPartyTransporter eD813PartyNewTransporterProvider;
	}
}

