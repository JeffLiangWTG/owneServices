using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class ContactPersonWrapperTest : DataProviderTestCase<ContactPersonWrapper>
	{
		public void TestEMailAddress()
		{
			AssertEquals("Should return EMailAddress address of CusAgent in case both PhoneNumber and EMailAddress are present.", "emailAddress", Provider.EMailAddress);

			var declaration = Factory.New<JobDeclaration>();
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "CUT";
			staff.GS_FullName = "name";
			staff.GS_LoginName = "loginName";
			declaration.Branch.GB_Email = "branchEmail";
			Factory.Save();
			declaration.JE_GS_NKCusAgent = "CUT";

			var provider = ContactPersonWrapper.New(declaration.CusAgent, declaration.Branch);
			AssertEquals("Should return EMailAddress of Branch in case both PhoneNumber and EMailAddress are not present for CusAgent.", "branchEmail", provider.EMailAddress);
		}

		public void TestName()
		{
			AssertEquals("Should return name of CusAgent.", "Name", Provider.Name);
		}

		public void TestPhoneNumber()
		{
			AssertEquals("Should return PhoneNumber of CusAgent in case both PhoneNumber and EMailAddress are present in CusAgent.", "staffPhone", Provider.PhoneNumber);

			var declaration = Factory.New<JobDeclaration>();
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "CUT";
			staff.GS_FullName = "name";
			staff.GS_LoginName = "loginName";
			declaration.Branch.GB_Email = "branchEmail";
			declaration.Branch.GB_Phone_Formatted = "branchPhone";
			Factory.Save();
			declaration.JE_GS_NKCusAgent = "CUT";

			var provider = ContactPersonWrapper.New(declaration.CusAgent, declaration.Branch);
			AssertEquals("Should return PhoneNumber of Branch in case both PhoneNumber and EmailAddress are not present for CusAgent.", "branchPhone", provider.PhoneNumber);
		}

		protected override ContactPersonWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "CUS";
			staff.GS_FullName = "Name";
			staff.GS_WorkPhone = "staffPhone";
			var email = staff.EmailAddresses.AddNew();
			email.GSE_EmailAddress = "emailAddress";
			email.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;
			Factory.Save();

			declaration.JE_GS_NKCusAgent = "CUS";

			return ContactPersonWrapper.New(declaration.CusAgent, declaration.Branch);
		}
	}
}
