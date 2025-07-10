using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	sealed class PartyTransporterProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyTransporterProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyTransporterProvider.NewOrNull(null));
			AssertNull(PartyTransporterProvider.NewOrNull(Factory.New<JobDocAddress>()));
			AssertNotNull(PartyTransporterProvider.NewOrNull(Factory.NewWithValidTestData<JobDocAddress>()));
		}

		public void TestVatNumber()
		{
			oraganisation.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA123", Core.Constants.CountryCodes.France);

			var partyTransporter = PartyTransporterProvider.NewOrNull(orgAddress);
			AssertEquals("VatNumber", "TVA123", partyTransporter.VatNumber);
		}

		public void TestVatNumber_Override()
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNum = "TVA123";
			jobDocAddress.E2_GovRegNumType = OrgCusCode.FranceCodeTypes.TVA;
			jobDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var partyTransporter = PartyTransporterProvider.NewOrNull(jobDocAddress);
			AssertEquals("VatNumber", "TVA123", partyTransporter.VatNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_City = "MELBOURNE";
			jobDocAddress.E2_Address1 = "23 CROWN ST";
			jobDocAddress.E2_Postcode = "3000";
			jobDocAddress.E2_CompanyName = "TEST OVERRIDE COMPANY";

			oraganisation = Factory.NewWithValidTestData<OrgHeader>();
			oraganisation.OH_FullName = "JOB ADDRESS TRADER NAME";

			var address = oraganisation.MainAddress;
			address.OA_Language = Core.SharedConstants.Languages.Tamil;
			address.OA_City = "DARWIN";
			address.OA_Address1 = "12 MITCHELL ST";
			address.OA_PostCode = "0800";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			orgAddress = Factory.New<JobDocAddress>();
			orgAddress.E2_OA_Address = address.PK;
		}
		JobDocAddress jobDocAddress;
		JobDocAddress orgAddress;
		OrgHeader oraganisation;

		protected override PartyTransporterProvider GetProvider() => PartyTransporterProvider.NewOrNull(jobDocAddress);
	}
}
