using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class PartyDocAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyDocAddressProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyDocAddressProvider.NewOrNull(null));
		}

		public void TestNew_IsValidAddress()
		{
			jobDocAddress.E2_OA_Address = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("IsValidAddress", false, jobDocAddress.IsValidAddress);
				AssertNull("Provider is null", PartyDocAddressProvider.NewOrNull(jobDocAddress));
			});
		}

		public void TestAdditionalAddressInformation()
		{
			CombineAssertions(() =>
			{
				jobDocAddress.Organisation.OH_FullName = "FANTIANWA";
				AssertEquals("E2_AddressOverride is false", "FANTIANWA", dataProvider.AdditionalAddressInformation);

				jobDocAddress.E2_AddressOverride = true;
				jobDocAddress.E2_CompanyName = "KOUSHUIWA";
				AssertEquals("E2_AddressOverride is true", "KOUSHUIWA", dataProvider.AdditionalAddressInformation);
			});
		}

		public void TestAddress()
		{
			AssertEquals("Address1", dataProvider.Address);
		}

		public void TestAddress2()
		{
			AssertEquals("Address2", dataProvider.Address2);
		}

		public void TestCity()
		{
			AssertEquals("City", dataProvider.City);
		}

		public void TestPostcode()
		{
			AssertEquals("2730018", dataProvider.Postcode);
		}

		public void TestCountry()
		{
			AssertEquals("US", dataProvider.Country);
		}

		public void TestProperties_E2_AddressOverride()
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_AdditionalAddressInformation = "Additional2";
			jobDocAddress.E2_Address1 = "New Address1";
			jobDocAddress.E2_Address2 = "New Address2";
			jobDocAddress.E2_City = "City2";
			jobDocAddress.E2_Postcode = "1234567";
			jobDocAddress.E2_RN_NKCountryCode = "AU";
			jobDocAddress.E2_State = "NSW";
			jobDocAddress.E2_CompanyName = "TEST";

			dataProvider = PartyDocAddressProvider.NewOrNull(jobDocAddress);
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalAddressInformation", "TEST", dataProvider.AdditionalAddressInformation);
				AssertEquals("Address", "New Address1", dataProvider.Address);
				AssertEquals("Address2", "New Address2", dataProvider.Address2);
				AssertEquals("City", "City2", dataProvider.City);
				AssertEquals("Postcode", "1234567", dataProvider.Postcode);
				AssertEquals("Country", "AU", dataProvider.Country);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.Address1 = "Address1";
			orgAddress.Address2 = "Address2";
			orgAddress.City = "City";
			orgAddress.Postcode = "2730018";
			orgAddress.OA_RN_NKCountryCode = "US";
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional";

			jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = orgAddress.PK;
			dataProvider = PartyDocAddressProvider.NewOrNull(jobDocAddress);
		}
		JobDocAddress jobDocAddress;
		IPartyDocAddress dataProvider;

		protected override PartyDocAddressProvider GetProvider() => (PartyDocAddressProvider)dataProvider;
	}
}
