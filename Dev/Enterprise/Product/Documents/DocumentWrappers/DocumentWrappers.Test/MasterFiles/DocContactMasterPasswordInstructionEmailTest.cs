using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocContactMasterPasswordInstructionEmail))]
	public class DocContactMasterPasswordInstructionEmailTest : DocumentWrapperTestCase
	{
		public void TestFields()
		{
			AssertEquals("Contact Name1", Wrapper.ContactName);
			AssertEquals(WrappedObject.Name, Wrapper.ContactName);

			AssertEquals("Mailbox Name1", Wrapper.MailboxDisplayName);

			WrappedObject.PasswordInstructionEmail.SendEmailWithPasswordInstructionUrl("http://www.cw1.com/?k=123", PasswordInstructionType.Set);
			AssertEquals("http://www.cw1.com/?k=123", Wrapper.PasswordInstructionUrl);
			AssertEquals(WrappedObject.PasswordInstructionEmail.PasswordInstructionUrl, Wrapper.PasswordInstructionUrl);
		}

		public void TestCurrentCompanyName()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Company For Email Template";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			var wrapper1 = DocContactMasterPasswordInstructionEmail.New(contact, Factory);
			AssertEquals(GlbCompany.CurrentCompany.CompanyName, wrapper1.CurrentCompanyName);

			contact.CompanyPKForEmailTemplate = company.PK.ToGuid();
			var wrapper2 = DocContactMasterPasswordInstructionEmail.New(contact, Factory);
			AssertEquals("Company For Email Template", wrapper2.CurrentCompanyName);
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod() => DocContactMasterPasswordInstructionEmail.New(Source, Factory);

		DocContactMasterPasswordInstructionEmail Wrapper
		{
			get { return (DocContactMasterPasswordInstructionEmail)base.Wrappers[0]; }
		}

		public override DocumentWrapper[] GetDocumentWrappers() => new DocumentWrapper[] { DocContactMasterPasswordInstructionEmail.New(Source, Factory) };

		IPasswordInstructionEmailSource WrappedObject => Wrapper.WrappedObject;

		protected override void SetUp()
		{
			Env.Registry.MailboxDisplayName = "Mailbox Name1";
			Source = Factory.NewWithValidTestData<OrgContact>();
			Source.OC_ContactName = "Contact Name1";
			base.SetUp();
		}

		OrgContact Source;
	}
}
