using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ExportDeclarantPartyIdWrapperTest : WrapperHelperTest<ExportDeclarantPartyIdWrapper>
	{
		public void TestNew()
		{
			AssertNull("OrgHeader null", ExportDeclarantPartyIdWrapper.New(null, ZString.Empty, ZString.Empty, ZBool.False));
		}

		public void TestPartyQualifier()
		{
			CombineAssertions(() =>
			{
				wrapper = ExportDeclarantPartyIdWrapper.New(orgHeader, "4", OrgHeaderData.Email, ZBool.False);
				AssertEquals("Expected filled PartyQualifier", "4", wrapper.PartyQualifier);

				wrapper = ExportDeclarantPartyIdWrapper.New(orgHeader, "SEL", OrgHeaderData.Email, ZBool.False);
				AssertEquals("Expected filled mapped PartyQualifier SEL to 1", "1", wrapper.PartyQualifier);

				wrapper = ExportDeclarantPartyIdWrapper.New(orgHeader, "DIR", OrgHeaderData.Email, ZBool.False);
				AssertEquals("Expected filled mapped PartyQualifier DIR to 2", "2", wrapper.PartyQualifier);

				wrapper = ExportDeclarantPartyIdWrapper.New(orgHeader, "IND", OrgHeaderData.Email, ZBool.False);
				AssertEquals("Expected filled mapped PartyQualifier IND to 3", "3", wrapper.PartyQualifier);
			});
		}

		public void TestEmailAddress()
		{
			AssertEquals("Expected filled EmailAddress", OrgHeaderData.Email, wrapper.EmailAddress);
		}

		public void TestNameCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty NameCode", ZString.Empty, wrapper.NameCode);

				wrapper = ExportDeclarantPartyIdWrapper.New(orgHeader, OrgHeaderData.DeclarantType, OrgHeaderData.Email, ZBool.True);
				AssertEquals("Expected filled NameCode", "O", wrapper.NameCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = OrgHeaderData.Code;

			wrapper = ExportDeclarantPartyIdWrapper.New(orgHeader, OrgHeaderData.DeclarantType, OrgHeaderData.Email, ZBool.False);
		}

		OrgHeader orgHeader;
		ExportDeclarantPartyIdWrapper wrapper;

		protected override ExportDeclarantPartyIdWrapper GetProvider() => wrapper;
	}
}
