using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class EALAESDeclarantWrapperWithContactPersonTest : WrapperHelperTest<EALAESDeclarantWrapperWithContactPerson>
	{
		public void TestGetNewAESCommonDeclarantWrapper()
		{
			CombineAssertions(() =>
			{
				orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = OrgHeaderData.Code;

				CusExitHeader exitHeader = null;
				AssertNull("Header null", EALAESDeclarantWrapperWithContactPerson.New(exitHeader));
				exitHeader = Factory.New<CusExitHeader>();
				exitHeader.CXH_OA_Carrier = ZGuid.Empty;
				AssertNull("Header no CarrierAddress null", EALAESDeclarantWrapperWithContactPerson.New(exitHeader));
				var address = Factory.New<OrgAddress>();
				exitHeader.CXH_OA_Carrier = address.PK;
				AssertNull("Header CarrierAddress no Header null", EALAESDeclarantWrapperWithContactPerson.New(exitHeader));
				address.OA_OH = Factory.New<OrgHeader>().PK;
				AssertNotNull("Header not null", EALAESDeclarantWrapperWithContactPerson.New(exitHeader));
			});
		}

		public void TestContactPerson()
		{
			var contactPerson = wrapper.ContactPerson;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ContactPerson", contactPerson);
				AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);

				AssertEquals("ContactPerson Email is taken from the correct address, not the main one", "mail@other.com", contactPerson.Email);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.New<OrgHeader>();

			var orgAddressMain = orgHeader.MainAddress;
			orgAddressMain.OA_Email = "mail@mail.com";

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Email = "mail@other.com";

			exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_OA_Carrier = orgAddress.PK;
			wrapper = EALAESDeclarantWrapperWithContactPerson.New(exitHeader);
		}
		OrgHeader orgHeader;
		CusExitHeader exitHeader;
		EALAESDeclarantWrapperWithContactPerson wrapper;

		protected override EALAESDeclarantWrapperWithContactPerson GetProvider() => wrapper;
	}
}
