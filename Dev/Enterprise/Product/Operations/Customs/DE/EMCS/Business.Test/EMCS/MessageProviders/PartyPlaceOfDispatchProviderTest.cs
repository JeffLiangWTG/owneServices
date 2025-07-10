using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	sealed class PartyPlaceOfDispatchProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyPlaceOfDispatchProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyPlaceOfDispatchProvider.NewOrNull(null));
			AssertNull(PartyPlaceOfDispatchProvider.NewOrNull(Factory.New<JobDocAddress>()));
			AssertNotNull(PartyPlaceOfDispatchProvider.NewOrNull(Factory.NewWithValidTestData<JobDocAddress>()));
		}

		public void TestReferenceOfTaxWarehouse()
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNum = "TID123";
			jobDocAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
			var placeOfDispatchTraderProvider = PartyPlaceOfDispatchProvider.NewOrNull(jobDocAddress);
			AssertEquals("TraderId", "TID123", placeOfDispatchTraderProvider.ReferenceOfTaxWarehouse);
		}

		public void TestLanguage()
		{
			jobDocAddress.E2_AddressOverride = true;
			var placeOfDispatchTraderProvider = PartyPlaceOfDispatchProvider.NewOrNull(jobDocAddress);
			AssertEquals("Language", "en", placeOfDispatchTraderProvider.Language);
		}

		public void TestName()
		{
			jobDocAddress.E2_AddressOverride = true;
			var placeOfDispatchTraderProvider = PartyPlaceOfDispatchProvider.NewOrNull(jobDocAddress);
			AssertEquals("Name", "TEST OVERRIDE COMPANY", placeOfDispatchTraderProvider.Name);
		}

		public void TestAddress()
		{
			jobDocAddress.E2_AddressOverride = true;
			var placeOfDispatchTraderProvider = PartyPlaceOfDispatchProvider.NewOrNull(jobDocAddress);
			AssertEquals("Address", "23 CROWN ST", placeOfDispatchTraderProvider.Address);
		}

		public void TestCity()
		{
			jobDocAddress.E2_AddressOverride = true;
			var placeOfDispatchTraderProvider = PartyPlaceOfDispatchProvider.NewOrNull(jobDocAddress);
			AssertEquals("City", "MELBOURNE", placeOfDispatchTraderProvider.City);
		}

		public void TestPostcode()
		{
			jobDocAddress.E2_AddressOverride = true;
			var placeOfDispatchTraderProvider = PartyPlaceOfDispatchProvider.NewOrNull(jobDocAddress);
			AssertEquals("Postcode", "3000", placeOfDispatchTraderProvider.Postcode);
		}

		public void TestCountry()
		{
			jobDocAddress.E2_AddressOverride = true;
			var placeOfDispatchTraderProvider = PartyPlaceOfDispatchProvider.NewOrNull(jobDocAddress);
			AssertEquals("Country", string.Empty, placeOfDispatchTraderProvider.Country);
		}

		public void TestPlaceOfDispatch_OrgAddress()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "JOB ADDRESS TRADER NAME";

			var address = organisation.MainAddress;
			address.OA_Language = Core.SharedConstants.Languages.Tamil;
			address.OA_City = "DARWIN";
			address.OA_Address1 = "12 MITCHELL ST";
			address.OA_PostCode = "0800";

			var orgAddress = Factory.New<JobDocAddress>();
			orgAddress.E2_OA_Address = address.PK;

			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "TID999", Core.Constants.CountryCodes.Germany);
			orgAddress.Address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "TID777", Core.Constants.CountryCodes.Greece);

			var placeOfDispatchTrader = PartyPlaceOfDispatchProvider.NewOrNull(orgAddress);
			CombineAssertions(() =>
			{
				AssertEquals("TraderId", "TID777", placeOfDispatchTrader.ReferenceOfTaxWarehouse);
				AssertEquals("Language", string.Empty, placeOfDispatchTrader.Language);
				AssertEquals("Address", string.Empty, placeOfDispatchTrader.Address);
				AssertEquals("City", string.Empty, placeOfDispatchTrader.City);
				AssertEquals("Name", string.Empty, placeOfDispatchTrader.Name);
				AssertEquals("Postcode", string.Empty, placeOfDispatchTrader.Postcode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_City = "MELBOURNE";
			jobDocAddress.E2_Address1 = "23 CROWN ST";
			jobDocAddress.E2_Postcode = "3000";
			jobDocAddress.E2_CompanyName = "TEST OVERRIDE COMPANY";
		}
		JobDocAddress jobDocAddress;

		protected override PartyPlaceOfDispatchProvider GetProvider() => PartyPlaceOfDispatchProvider.NewOrNull(jobDocAddress);
	}
}
