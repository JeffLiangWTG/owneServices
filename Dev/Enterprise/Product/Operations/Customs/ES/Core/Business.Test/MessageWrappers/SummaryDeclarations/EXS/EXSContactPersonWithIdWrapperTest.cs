using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class EXSContactPersonWithIdWrapperTest : WrapperHelperTest<EXSContactPersonWithIdWrapper>
	{
		public void TestGetNewEXSContactPersonWithIdWrapper()
		{
			CombineAssertions(() =>
			{
				OrgHeader org = null;
				AssertNull("OrgHeader null", EXSContactPersonWithIdWrapper.New(org));
				org = Factory.New<OrgHeader>();
				AssertNotNull("OrgHeader not null", EXSContactPersonWithIdWrapper.New(org));
			});
		}

		public void TestPhone()
		{
			orgHeader.MainAddress.OA_Phone = "1234";
			AssertEquals("1234", wrapper.Phone);
		}

		public void TestEmailAddress()
		{
			orgHeader.MainAddress.OA_Email = "aaaa@bb.com";
			AssertEquals("aaaa@bb.com", wrapper.EmailAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.New<OrgHeader>();
			wrapper = EXSContactPersonWithIdWrapper.New(orgHeader);
		}
		OrgHeader orgHeader;
		EXSContactPersonWithIdWrapper wrapper;

		protected override EXSContactPersonWithIdWrapper GetProvider() => wrapper;
	}
}
