using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE801PartyPlaceOfDispatchProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyPlaceOfDispatchProvider.NewOrNull(null));
		}

		public void TestReferenceOfTaxWarehouse()
		{
			placeOfDispatchTrader.ReferenceOfTaxWarehouse = "GBRTW001";
			AssertEquals("GBRTW001", ie801PartyPlaceOfDispatchProvider.ReferenceOfTaxWarehouse);
		}

		public void TestTraderName()
		{
			placeOfDispatchTrader.TraderName = "Place Of Dispatch 01";
			AssertEquals("Place Of Dispatch 01", ie801PartyPlaceOfDispatchProvider.Name);
		}

		public void TestAddress()
		{
			placeOfDispatchTrader.StreetName = "Street A";
			placeOfDispatchTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyPlaceOfDispatchProvider.Address);
		}

		public void TestCity()
		{
			placeOfDispatchTrader.City = "London";
			AssertEquals("London", ie801PartyPlaceOfDispatchProvider.City);
		}

		public void TestPostcode()
		{
			placeOfDispatchTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyPlaceOfDispatchProvider.Postcode);
		}

		public void TestCountry()
		{
			AssertEquals(string.Empty, ie801PartyPlaceOfDispatchProvider.Country);
			placeOfDispatchTrader.ReferenceOfTaxWarehouse = "GBRTW001";
			AssertEquals("GB", ie801PartyPlaceOfDispatchProvider.Country);
		}

		public void TestLanguage()
		{
			placeOfDispatchTrader.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, ie801PartyPlaceOfDispatchProvider.Language);
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
