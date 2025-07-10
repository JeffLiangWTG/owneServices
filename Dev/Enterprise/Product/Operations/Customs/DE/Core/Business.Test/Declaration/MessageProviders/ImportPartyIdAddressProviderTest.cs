using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportPartyIdAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportPartyIdAddressProvider>
	{
		public void TestConstructor_OrgAddress()
		{
			CombineAssertions(() =>
			{
				AssertNull("Null", ImportPartyIdAddressProvider.NewOrNull((OrgAddress)null));
				AssertNull("Null", ImportPartyIdAddressProvider.NewOrNull((OrgAddress)null, false));
				AssertNotNull("Not Null", ImportPartyIdAddressProvider.NewOrNull(orgAddress));
				AssertNotNull("Not Null", ImportPartyIdAddressProvider.NewOrNull(orgAddress, false));
				AssertType<TruncatedImportPartyIdAddressProvider>(ImportPartyIdAddressProvider.NewOrNull(orgAddress));
				AssertType<ImportPartyIdAddressProvider>(ImportPartyIdAddressProvider.NewOrNull(orgAddress, false));
			});
		}

		public void TestConstructor_JobDocAddress()
		{
			CombineAssertions(() =>
			{
				AssertNull("Null", ImportPartyIdAddressProvider.NewOrNull((JobDocAddress)null));
				AssertNull("Null", ImportPartyIdAddressProvider.NewOrNull((JobDocAddress)null, false));
				var validJobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
				AssertNotNull("JobDocAddress", ImportPartyIdAddressProvider.NewOrNull(validJobDocAddress));
				AssertNotNull("JobDocAddress", ImportPartyIdAddressProvider.NewOrNull(validJobDocAddress, false));
				var invalidJobDocAddress = Factory.New<JobDocAddress>();
				AssertNull("Invalid JobDocAddress", ImportPartyIdAddressProvider.NewOrNull(invalidJobDocAddress));
				AssertNull("Invalid JobDocAddress", ImportPartyIdAddressProvider.NewOrNull(invalidJobDocAddress, false));
				AssertType<TruncatedImportPartyIdAddressProvider>(ImportPartyIdAddressProvider.NewOrNull(validJobDocAddress));
				AssertType<ImportPartyIdAddressProvider>(ImportPartyIdAddressProvider.NewOrNull(validJobDocAddress, false));
			});
		}

		public void TestName()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;

			CombineAssertions(() =>
			{
				AssertNull("Empty", Provider.Name);

				orgHeader.OH_FullName = "SampleFreight";
				AssertEquals("FullName", "SampleFreight", Provider.Name);

				orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional Information";
				AssertEquals("Both", "SampleFreight Additional Information", Provider.Name);

				orgHeader.OH_FullName = ZString.Empty;
				AssertEquals("AdditionalAddressInformation", "Additional Information", Provider.Name);
			});
		}

		public void TestDistrict()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.District);

				orgAddress.OA_Address2 = "Finthen";
				AssertEquals("Not Empty", "Finthen", Provider.District);
			});
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", string.Empty, Provider.Address);

				orgAddress.OA_Address1 = "Poststraße 1";
				AssertEquals("Not Empty", "Poststraße 1", Provider.Address);
			});
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", string.Empty, Provider.City);

				orgAddress.OA_City = "Mainz";
				AssertEquals("Not Empty", "Mainz", Provider.City);
			});
		}

		public void TestPostcode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", string.Empty, Provider.Postcode);

				orgAddress.OA_PostCode = "55126";
				AssertEquals("Not Empty", "55126", Provider.Postcode);
			});
		}

		public void TestCountry()
		{
			orgAddress.OA_RN_NKCountryCode = "FR";
			AssertEquals("FR", Provider.Country);
		}

		protected override ImportPartyIdAddressProvider GetProvider() => (ImportPartyIdAddressProvider)ImportPartyIdAddressProvider.NewOrNull(orgAddress, false);

		protected override void SetUp()
		{
			base.SetUp();
			orgAddress = Factory.New<OrgAddress>();
		}
		OrgAddress orgAddress;
	}
}
