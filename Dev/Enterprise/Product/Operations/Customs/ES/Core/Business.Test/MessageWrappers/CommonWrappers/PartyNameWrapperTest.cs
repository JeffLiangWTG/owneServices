using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class PartyNameWrapperTest : WrapperHelperTest<PartyNameWrapper>
	{
		public void TestGetNewPartyNameWrapper()
		{
			CombineAssertions(() =>
			{
				AssertNull("Header null", PartyNameWrapper.New((OrgHeader)null));
				var org = Factory.New<OrgHeader>();
				AssertNotNull("Header not null", PartyNameWrapper.New(Factory.New<OrgHeader>()));

				AssertNull("OrgAddress null", PartyNameWrapper.New((OrgAddress)null));
				var address = Factory.New<OrgAddress>();
				AssertNull("OrgAddress with null OrgHeader null", PartyNameWrapper.New(address));

				AssertNull("JobDocAddress null", PartyNameWrapper.New((JobDocAddress)null));
				var docAddress = Factory.New<JobDocAddress>();
				AssertNull("JobDocAddress with no org address null", PartyNameWrapper.New(docAddress));
				docAddress.E2_OA_Address = address.PK;
				AssertNull("JobDocAddress with org address but no OrgHeader null", PartyNameWrapper.New(docAddress));
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", PartyNameWrapper.New(address));
				AssertNotNull("JobDocAddress not nullnull", PartyNameWrapper.New(docAddress));
			});
		}

		public void TestName()
		{
			orgHeader.OH_FullName = OrgHeaderData.Name;
			AssertEquals("Expected filled Name", OrgHeaderData.Name, wrapper.Name);
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			wrapper = PartyNameWrapper.New(orgHeader);
		}
		OrgHeader orgHeader;
		PartyNameWrapper wrapper;

		protected override PartyNameWrapper GetProvider() => wrapper;
	}
}
