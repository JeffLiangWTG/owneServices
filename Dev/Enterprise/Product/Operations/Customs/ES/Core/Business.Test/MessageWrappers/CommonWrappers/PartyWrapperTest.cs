using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class PartyWrapperTest : WrapperHelperTest<PartyWrapper>
	{
		public void TestGetNewPartyWrapper()
		{
			CombineAssertions(() =>
			{
				OrgHeader org = null;
				AssertNull("OrgHeader null", PartyWrapper.New(org));
				org = Factory.New<OrgHeader>();
				AssertNotNull("OrgHeader not null", PartyWrapper.New(org));

				OrgAddress address = null;
				AssertNull("OrgAddress null", PartyWrapper.New(address));
				address = Factory.New<OrgAddress>();
				AssertNull("OrgAddress with null OrgHeader null", PartyWrapper.New(address));

				JobDocAddress docAddress = null;
				AssertNull("JobDocAddress null", PartyWrapper.New(docAddress));
				docAddress = Factory.New<JobDocAddress>();
				AssertNull("JobDocAddress with no org address null", PartyWrapper.New(docAddress));
				docAddress.E2_OA_Address = address.PK;
				AssertNull("JobDocAddress with org address but no OrgHeader null", PartyWrapper.New(docAddress));
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", PartyWrapper.New(address));
				AssertNotNull("JobDocAddress not nullnull", PartyWrapper.New(docAddress));
			});
		}

		public void TestAddress()
		{
			orgAddress.OA_Address1 = OrgHeaderData.Address;
			AssertEquals("Expected filled Address", OrgHeaderData.Address, wrapper.Address);
		}

		public void TestCity()
		{
			orgAddress.OA_City = OrgHeaderData.City;
			AssertEquals("Expected filled City", OrgHeaderData.City, wrapper.City);
		}

		public void TestPostCode()
		{
			orgAddress.OA_PostCode = OrgHeaderData.PostCode;
			AssertEquals("Expected filled PostCode", OrgHeaderData.PostCode, wrapper.PostCode);
		}

		public void TestCountry()
		{
			orgAddress.OA_RN_NKCountryCode = OrgHeaderData.Country;
			AssertEquals("Expected filled Country", OrgHeaderData.Country, wrapper.Country);
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress = orgHeader.Addresses.AddNew();
			wrapper = PartyWrapper.New(orgAddress);
		}
		OrgHeader orgHeader;
		OrgAddress orgAddress;
		PartyWrapper wrapper;

		protected override PartyWrapper GetProvider() => wrapper;
	}
}
