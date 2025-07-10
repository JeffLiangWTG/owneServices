using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class PartyGuarantorProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyGuarantorProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyGuarantorProvider.NewOrNull(null));
			AssertNull(PartyGuarantorProvider.NewOrNull(Factory.New<JobDocAddress>()));
			AssertNotNull(PartyGuarantorProvider.NewOrNull(Factory.NewWithValidTestData<JobDocAddress>()));
		}

		public void TestTraderExciseNumber()
		{
			oraganisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TEN123", Core.Constants.CountryCodes.Greece);

			var guarantorTrader = PartyGuarantorProvider.NewOrNull(orgAddress);
			AssertEquals("TraderExciseNumber", "TEN123", guarantorTrader.TraderExciseNumber);
		}

		public void TestTraderExciseNumber_Override()
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNum = "TEN123";
			jobDocAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;

			var guarantorTrader = PartyGuarantorProvider.NewOrNull(jobDocAddress);
			AssertEquals("TraderExciseNumber", "TEN123", guarantorTrader.TraderExciseNumber);
		}

		public void TestVatNumber()
		{
			oraganisation.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA123", Core.Constants.CountryCodes.France);

			var guarantorTrader = PartyGuarantorProvider.NewOrNull(orgAddress);
			AssertEquals("VatNumber", "TVA123", guarantorTrader.VatNumber);
		}

		public void TestVatNumber_Override()
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNum = "TVA123";
			jobDocAddress.E2_GovRegNumType = OrgCusCode.FranceCodeTypes.TVA;
			jobDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var guarantorTrader = PartyGuarantorProvider.NewOrNull(jobDocAddress);
			AssertEquals("VatNumber", "TVA123", guarantorTrader.VatNumber);
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

		protected override PartyGuarantorProvider GetProvider() => PartyGuarantorProvider.NewOrNull(jobDocAddress);
	}
}
