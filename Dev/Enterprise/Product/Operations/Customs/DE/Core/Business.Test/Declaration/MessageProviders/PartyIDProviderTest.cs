using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class PartyIDProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyIDProvider>
	{
		public void TestNew_OrgHeader()
		{
			CombineAssertions(() =>
			{
				AssertNull("No Organisation", PartyIDProvider.NewOrNull((OrgHeader)null));
				AssertNotNull("Organisation", PartyIDProvider.NewOrNull(Factory.NewWithValidTestData<OrgHeader>()));
			});
		}

		public void TestNew_OrgAddress()
		{
			CombineAssertions(() =>
			{
				AssertNull("No Address", PartyIDProvider.NewOrNull((OrgAddress)null));
				AssertNotNull("Address", PartyIDProvider.NewOrNull(Factory.NewWithValidTestData<OrgAddress>()));
			});
		}

		public void TestNew_JobDocAddress()
		{
			CombineAssertions(() =>
			{
				AssertNull("No JobDocAddress", PartyIDProvider.NewOrNull((JobDocAddress)null));
				AssertNotNull("JobDocAddress", PartyIDProvider.NewOrNull(Factory.NewWithValidTestData<JobDocAddress>()));
				AssertNull("Invalid JobDocAddress", PartyIDProvider.NewOrNull(Factory.New<JobDocAddress>()));
			});
		}

		public void TestEoriNumber_OrgHeader()
		{
			address.OA_Address1 = "address 1";
			TestHelper.CreateCL010CoutryList(Factory);
			var partyId = PartyIDProvider.NewOrNull(orgHeader);
			AssertEquals("EoriNumber", "GREOR1", partyId.EoriNumber);
		}

		public void TestEoriBranchSuffix_OrgHeader()
		{
			var partyId = PartyIDProvider.NewOrNull(orgHeader);
			AssertEquals("EoriBranchSuffix", null, partyId.EoriBranchSuffix);
		}

		public void TestEoriNumber_OrgAddress()
		{
			address.OA_Address1 = "address 1";
			TestHelper.CreateCL010CoutryList(Factory);
			var partyId = PartyIDProvider.NewOrNull(address);
			AssertEquals("EoriNumber", "GREOR1", partyId.EoriNumber);
		}

		public void TestEoriBranchSuffix_OrgAddress()
		{
			var partyId = PartyIDProvider.NewOrNull(address);
			AssertEquals("EoriBranchSuffix", "EBS1", partyId.EoriBranchSuffix);
		}

		public void TestEoriNumber_JobDocAddress()
		{
			address.OA_Address1 = "address 1";
			TestHelper.CreateCL010CoutryList(Factory);
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.OrganisationPK = orgHeader.PK;
			docAddress.E2_OA_Address = address.PK;
			var partyId = PartyIDProvider.NewOrNull(docAddress);
			AssertEquals("EoriNumber", "GREOR1", partyId.EoriNumber);
		}

		public void TestEoriBranchSuffix_JobDocAddress()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.OrganisationPK = orgHeader.PK;
			docAddress.E2_OA_Address = address.PK;
			var partyId = PartyIDProvider.NewOrNull(docAddress);
			AssertEquals("EoriBranchSuffix", "EBS1", partyId.EoriBranchSuffix);
		}

		public void TestTCUNumber()
		{
			address.OA_Address1 = "address 1";
			TestHelper.CreateCL010CoutryList(Factory);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "TCU1", Core.Constants.CountryCodes.Greece);
			var partyId = PartyIDProvider.NewOrNull(orgHeader);
			AssertEquals("GRTCU1", partyId.TCUNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Greece);
			address = orgHeader.Addresses.AddNew();
			address.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			var ebsCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS1", Core.Constants.CountryCodes.Germany);
			ebsCode.OK_OA_PremisesAddress = address.PK;
		}
		OrgHeader orgHeader;
		OrgAddress address;

		protected override PartyIDProvider GetProvider() => PartyIDProvider.NewOrNull(orgHeader);
	}
}
