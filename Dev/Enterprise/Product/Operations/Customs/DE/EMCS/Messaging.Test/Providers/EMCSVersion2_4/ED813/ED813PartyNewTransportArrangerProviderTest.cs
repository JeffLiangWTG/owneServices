using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	class ED813PartyNewTransportArrangerProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED813PartyNewTransportArrangerProvider.NewOrNull(null));
		}

		public void TestVatNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, eD813PartyNewTransportArrangerProvider.VatNumber);

				newTransportArrangerTrader.VatNumber = "DETRT001";
				AssertEquals("TRT001", eD813PartyNewTransportArrangerProvider.VatNumber);
			});
		}

		public void TestTraderName()
		{
			newTransportArrangerTrader.TraderName = "Transport Arranger 01";
			AssertEquals("Transport Arranger 01", eD813PartyNewTransportArrangerProvider.Name);
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				newTransportArrangerTrader.StreetName = "Street A";
				AssertEquals("StreetNumber not specified, No exception", "Street A ", eD813PartyNewTransportArrangerProvider.Address);

				newTransportArrangerTrader.StreetNumber = "9";
				AssertEquals("Street A 9", eD813PartyNewTransportArrangerProvider.Address);
			});
		}

		public void TestCity()
		{
			newTransportArrangerTrader.City = "Berlin";
			AssertEquals("Berlin", eD813PartyNewTransportArrangerProvider.City);
		}

		public void TestPostcode()
		{
			newTransportArrangerTrader.Postcode = "10115";
			AssertEquals("10115", eD813PartyNewTransportArrangerProvider.Postcode);
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, eD813PartyNewTransportArrangerProvider.Country);

				newTransportArrangerTrader.VatNumber = "DETRT001";
				AssertEquals("DE", eD813PartyNewTransportArrangerProvider.Country);
			});
		}

		public void TestLanguage()
		{
			newTransportArrangerTrader.NadLng = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, eD813PartyNewTransportArrangerProvider.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			newTransportArrangerTrader = new ED813EBodyChangeOfDestinationNewTransportArrangerTrader();
			eD813PartyNewTransportArrangerProvider = ED813PartyNewTransportArrangerProvider.NewOrNull(newTransportArrangerTrader);
		}
		ED813EBodyChangeOfDestinationNewTransportArrangerTrader newTransportArrangerTrader;
		IEMCSPartyTransporter eD813PartyNewTransportArrangerProvider;
	}
}
