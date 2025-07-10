using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	sealed class PartyAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyAddressProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyAddressProvider.NewOrNull(null));
			AssertNull(PartyAddressProvider.NewOrNull(Factory.New<JobDocAddress>()));
			AssertNotNull(PartyAddressProvider.NewOrNull(Factory.NewWithValidTestData<JobDocAddress>()));
		}

		public void TestAddress()
		{
			var jobDocAddress = CreateValidJobDocAddress();
			var partyAddressProvider = (IEMCSPartyAddress)PartyAddressProvider.NewOrNull(jobDocAddress);
			AssertEquals("Street Name", "12 MITCHELL STPART2", partyAddressProvider.Address);
		}

		public void TestLanguage()
		{
			var jobDocAddress = CreateValidJobDocAddress();
			var partyAddressProvider = (IEMCSPartyAddress)PartyAddressProvider.NewOrNull(jobDocAddress);
			AssertEquals("Language", Core.SharedConstants.Languages.French.ToLower(), partyAddressProvider.Language);
		}

		public void TestName()
		{
			var jobDocAddress = CreateValidJobDocAddress();
			var partyAddressProvider = (IEMCSPartyAddress)PartyAddressProvider.NewOrNull(jobDocAddress);
			AssertEquals("Trader Name", "JOB ADDRESS TRADER NAME", partyAddressProvider.Name);
		}

		public void TestCity()
		{
			var jobDocAddress = CreateValidJobDocAddress();
			var partyAddressProvider = (IEMCSPartyAddress)PartyAddressProvider.NewOrNull(jobDocAddress);
			AssertEquals("City", "DARWIN", partyAddressProvider.City);
		}

		public void TestPostcode()
		{
			var jobDocAddress = CreateValidJobDocAddress();
			var partyAddressProvider = (IEMCSPartyAddress)PartyAddressProvider.NewOrNull(jobDocAddress);
			AssertEquals("Post Code", "0800", partyAddressProvider.Postcode);
		}

		public void TestCountry()
		{
			var jobDocAddress = CreateValidJobDocAddress();
			var partyAddressProvider = (IEMCSPartyAddress)PartyAddressProvider.NewOrNull(jobDocAddress);
			AssertEquals(string.Empty, partyAddressProvider.Country);
		}

		public void TestAddressOverride()
		{
			var orgHeader = GlbBranch.CurrentBranch.Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Language = Core.SharedConstants.Languages.French;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgHeader.PK;
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_City = "MELBOURNE";
			jobDocAddress.E2_Address1 = "23 CROWN ST";
			jobDocAddress.E2_Address2 = "PART2OV";
			jobDocAddress.E2_Postcode = "3000";
			jobDocAddress.E2_CompanyName = "TEST OVERRIDE COMPANY";

			var partyAddressWrapper = (IEMCSPartyAddress)PartyAddressProvider.NewOrNull(jobDocAddress);

			CombineAssertions(() =>
			{
				AssertEquals("Language", Core.SharedConstants.Languages.French.ToLower(), partyAddressWrapper.Language);
				AssertEquals("City", "MELBOURNE", partyAddressWrapper.City);
				AssertEquals("Street Name", "23 CROWN STPART2OV", partyAddressWrapper.Address);
				AssertEquals("Post Code", "3000", partyAddressWrapper.Postcode);
				AssertEquals("Trader Name", "TEST OVERRIDE COMPANY", partyAddressWrapper.Name);
			});
		}

		protected override PartyAddressProvider GetProvider()
		{
			var jobDocAddress = CreateValidJobDocAddress();
			return PartyAddressProvider.NewOrNull(jobDocAddress);
		}

		JobDocAddress CreateValidJobDocAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "JOB ADDRESS TRADER NAME";
			org.OH_Language = Core.SharedConstants.Languages.French;
			var address = org.MainAddress;
			address.OA_Language = Core.SharedConstants.Languages.Tamil;
			address.OA_City = "DARWIN";
			address.OA_Address1 = "12 MITCHELL ST";
			address.OA_Address2 = "PART2";
			address.OA_PostCode = "0800";
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_OA_Address = address.PK;
			return jobDocAddress;
		}
	}
}
