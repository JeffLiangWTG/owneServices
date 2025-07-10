using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	class ED801PartyPlaceOfDispatchProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED801PartyPlaceOfDispatchProvider.NewOrNull(null));
		}

		public void TestReferenceOfTaxWarehouse()
		{
			placeOfDispatchTrader.ReferenceOfTaxWarehouse = "DERTW001";
			AssertEquals("DERTW001", eD801PartyPlaceOfDispatchProvider.ReferenceOfTaxWarehouse);
		}

		public void TestTraderName()
		{
			placeOfDispatchTrader.TraderName = "Place Of Dispatch 01";
			AssertEquals("Place Of Dispatch 01", eD801PartyPlaceOfDispatchProvider.Name);
		}

		public void TestAddress()
		{
			placeOfDispatchTrader.StreetName = "Street A";
			placeOfDispatchTrader.StreetNumber = "9";
			AssertEquals("Street A 9", eD801PartyPlaceOfDispatchProvider.Address);
		}

		public void TestCity()
		{
			placeOfDispatchTrader.City = "Berlin";
			AssertEquals("Berlin", eD801PartyPlaceOfDispatchProvider.City);
		}

		public void TestPostcode()
		{
			placeOfDispatchTrader.Postcode = "10115";
			AssertEquals("10115", eD801PartyPlaceOfDispatchProvider.Postcode);
		}

		public void TestCountry()
		{
			AssertEquals(string.Empty, eD801PartyPlaceOfDispatchProvider.Country);
			placeOfDispatchTrader.ReferenceOfTaxWarehouse = "DERTW001";
			AssertEquals("DE", eD801PartyPlaceOfDispatchProvider.Country);
		}

		public void TestLanguage()
		{
			placeOfDispatchTrader.NadLng = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, eD801PartyPlaceOfDispatchProvider.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			placeOfDispatchTrader = new ED801DBodyEadContainerPlaceOfDispatchTrader();
			eD801PartyPlaceOfDispatchProvider = ED801PartyPlaceOfDispatchProvider.NewOrNull(placeOfDispatchTrader);
		}
		ED801DBodyEadContainerPlaceOfDispatchTrader placeOfDispatchTrader;
		IEMCSPartyPlaceOfDispatch eD801PartyPlaceOfDispatchProvider;
	}
}

