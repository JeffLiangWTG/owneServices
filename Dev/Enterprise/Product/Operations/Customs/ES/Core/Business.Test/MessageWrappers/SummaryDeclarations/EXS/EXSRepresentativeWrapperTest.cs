using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class EXSRepresentativeWrapperTest : WrapperHelperTest<EXSRepresentativeWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("JobDeclaration null", () => EXSRepresentativeWrapper.New(null, orgAddress));

				var declaration = Factory.New<JobDeclaration>();
				AssertNull("OrgAddress null", EXSRepresentativeWrapper.New(declaration, null));

				var address = Factory.New<OrgAddress>();
				AssertNull("OrgHeader null", EXSRepresentativeWrapper.New(declaration, address));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", EXSRepresentativeWrapper.New(declaration, address));
			});
		}

		public void TestDirectRepresentation()
		{
			declaration.JE_DeclarantType = "4";
			AssertEquals("Type declarant 4", string.Empty, wrapper.DirectRepresentation);
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			AssertEquals("Type declarant 2", ESRepresentationTypeList.Codes._2Direct, wrapper.DirectRepresentation);
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
			AssertEquals("Type declarant 3", ESRepresentationTypeList.Codes._3Indirect, wrapper.DirectRepresentation);
		}

		public void TestPhone()
		{
			orgHeader.MainAddress.OA_Phone = "1234";
			orgAddress.OA_Phone = "5678";

			CombineAssertions(() =>
			{
				AssertEquals("Phone is taken from OrgAddress if not empty", "5678", wrapper.Phone);

				orgAddress.OA_Phone = ZString.Empty;
				wrapper = EXSRepresentativeWrapper.New(declaration, orgAddress);
				AssertEquals("Phone is taken from MainAddress if OrgAddress Phone is empty", "1234", wrapper.Phone);
			});
		}

		public void TestEmailAddress()
		{
			orgHeader.MainAddress.OA_Email = "aaaa@bb.com";
			orgAddress.OA_Email = "cccc@dd.com";

			CombineAssertions(() =>
			{
				AssertEquals("Email is taken from OrgAddress if not empty", "cccc@dd.com", wrapper.EmailAddress);

				orgAddress.OA_Email = ZString.Empty;
				wrapper = EXSRepresentativeWrapper.New(declaration, orgAddress);
				AssertEquals("Email is taken from MainAddress if OrgAddress Email is empty", "aaaa@bb.com", wrapper.EmailAddress);

				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress("eeee@ff.com"))
				{
					orgHeader.MainAddress.OA_Email = ZString.Empty;
					wrapper = EXSRepresentativeWrapper.New(declaration, orgAddress);
					AssertEquals("Email is taken from Misc/Declaration Email if OrgAddress and MainAddress Email are empty", "eeee@ff.com", wrapper.EmailAddress);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			orgHeader = Factory.New<OrgHeader>();

			var orgAddressMain = orgHeader.MainAddress;
			orgAddressMain.OA_Address1 = "Address Main";

			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address Other";

			wrapper = EXSRepresentativeWrapper.New(declaration, orgAddress);
		}
		JobDeclaration declaration;

		OrgAddress orgAddress;
		OrgHeader orgHeader;
		EXSRepresentativeWrapper wrapper;

		protected override EXSRepresentativeWrapper GetProvider() => wrapper;
	}
}
