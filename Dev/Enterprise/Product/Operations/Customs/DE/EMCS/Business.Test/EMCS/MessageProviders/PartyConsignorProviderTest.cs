using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	sealed class PartyConsignorProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyConsignorProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyConsignorProvider.NewOrNull(null));
			AssertNull(PartyConsignorProvider.NewOrNull(Factory.New<JobDocAddress>()));
			AssertNotNull(PartyConsignorProvider.NewOrNull(Factory.NewWithValidTestData<JobDocAddress>()));
		}

		public void TestTraderExciseNumber()
		{
			oraganisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TEN999", Core.Constants.CountryCodes.Greece);

			var consignorTrader = PartyConsignorProvider.NewOrNull(orgAddress);
			AssertEquals("ExciseNo", "TEN999", consignorTrader.TraderExciseNumber);
		}

		public void TestTraderExciseNumber_Override()
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNum = "TEN123";
			jobDocAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;

			var consignorTrader = PartyConsignorProvider.NewOrNull(jobDocAddress);
			AssertEquals("ExciseNo", "TEN123", consignorTrader.TraderExciseNumber);
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

			orgAddress = Factory.New<JobDocAddress>();
			orgAddress.E2_OA_Address = address.PK;
		}
		JobDocAddress jobDocAddress;
		JobDocAddress orgAddress;
		OrgHeader oraganisation;

		protected override PartyConsignorProvider GetProvider() => PartyConsignorProvider.NewOrNull(jobDocAddress);
	}
}
