using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE801PartyPlaceOfDispatchProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyPlaceOfDispatchProvider.NewOrNull(null));
		}

		public void TestReferenceOfTaxWarehouse()
		{
			placeOfDispatchTrader.ReferenceOfTaxWarehouse = "IERTW001";
			AssertEquals("IERTW001", ie801PartyPlaceOfDispatchProvider.ReferenceOfTaxWarehouse);
		}

		public void TestTraderName()
		{
			placeOfDispatchTrader.TraderName = "Place Of Dispatch 01";
			AssertEquals("Place Of Dispatch 01", ie801PartyPlaceOfDispatchProvider.Name);
		}

		public void TestStreetAndNumber()
		{
			placeOfDispatchTrader.StreetName = "Street A";
			placeOfDispatchTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyPlaceOfDispatchProvider.StreetAndNumber);
		}

		public void TestCity()
		{
			placeOfDispatchTrader.City = "Dublin";
			AssertEquals("Dublin", ie801PartyPlaceOfDispatchProvider.City);
		}

		public void TestPostcode()
		{
			placeOfDispatchTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyPlaceOfDispatchProvider.Postcode);
		}

		public void TestCountry()
		{
			AssertEquals(string.Empty, ie801PartyPlaceOfDispatchProvider.Country);
			placeOfDispatchTrader.ReferenceOfTaxWarehouse = "IERTW001";
			AssertEquals("IE", ie801PartyPlaceOfDispatchProvider.Country);
		}

		public void TestLanguage()
		{
			placeOfDispatchTrader.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, ie801PartyPlaceOfDispatchProvider.Language);
		}

		public void TestLanguageNull()
		{
			placeOfDispatchTrader.Language = null;
			AssertNoExceptionThrown("Language null.", () => _ = placeOfDispatchTrader.Language);
		}

		protected override void SetUp()
		{
			base.SetUp();
			placeOfDispatchTrader = new PlaceOfDispatchTraderType();
			ie801PartyPlaceOfDispatchProvider = IE801PartyPlaceOfDispatchProvider.NewOrNull(placeOfDispatchTrader);
		}
		PlaceOfDispatchTraderType placeOfDispatchTrader;
		IEMCSPartyPlaceOfDispatch ie801PartyPlaceOfDispatchProvider;
	}
}
