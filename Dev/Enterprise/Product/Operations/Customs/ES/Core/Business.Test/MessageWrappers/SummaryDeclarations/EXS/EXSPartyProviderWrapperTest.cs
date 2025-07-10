using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class EXSPartyProviderWrapperTest : WrapperHelperTest<EXSPartyProviderWrapper>
	{
		public void TestGetNewEXSPartyProviderWrapper()
		{
			CombineAssertions(() =>
			{
				OrgHeader org = null;
				AssertNull("OrgHeader null", EXSPartyProviderWrapper.New(org));
				org = Factory.New<OrgHeader>();
				AssertNotNull("OrgHeader not null", EXSPartyProviderWrapper.New(org));

				OrgAddress address = null;
				AssertNull("OrgAddress null", EXSPartyProviderWrapper.New(address, ZString.Empty));
				address = Factory.New<OrgAddress>();
				AssertNull("OrgAddress with null OrgHeader null", EXSPartyProviderWrapper.New(address, ZString.Empty));
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", EXSPartyProviderWrapper.New(address, ZString.Empty));
			});
		}

		public void TestEmailAddress_FromAddress()
		{
			CombineAssertions(() =>
			{
				orgHeader.MainAddress.OA_Email = "aaaa@bb.com";
				orgAddress.OA_Email = "cccc@dd.com";
				wrapper = EXSPartyProviderWrapper.New(orgHeader, orgAddress);
				AssertEquals("when orgAddres is not null then that address's email", "cccc@dd.com", wrapper.EmailAddress);

				wrapper = EXSPartyProviderWrapper.New(orgHeader, null);
				AssertEquals("when orgAddres is null then MainAddres's email", "aaaa@bb.com", wrapper.EmailAddress);
			});
		}

		public void TestEmailAddress_FromConstructor()
		{
			wrapper = EXSPartyProviderWrapper.New(orgHeader.MainAddress, "mail@mail.com");
			AssertEquals("mail@mail.com", wrapper.EmailAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.New<OrgHeader>();

			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			wrapper = EXSPartyProviderWrapper.New(orgHeader, orgAddress);
		}
		OrgHeader orgHeader;
		OrgAddress orgAddress;
		EXSPartyProviderWrapper wrapper;

		protected override EXSPartyProviderWrapper GetProvider() => wrapper;
	}
}
