using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocContactPasswordInstructionEmail))]
	public class DocContactPasswordInstructionEmailTest : DocumentWrapperTestCase
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
			var wrapper1 = DocContactPasswordInstructionEmail.New(contact, Factory);
			AssertEquals(GlbCompany.CurrentCompany.CompanyName, wrapper1.CurrentCompanyName);

			contact.CompanyPKForEmailTemplate = company.PK.ToGuid();
			var wrapper2 = DocContactPasswordInstructionEmail.New(contact, Factory);
			AssertEquals("Company For Email Template", wrapper2.CurrentCompanyName);

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			using (Env.SetTemporaryUserContext(new NullCompanyUserContext(Env.CurrentUserContext)))
			{
				var wrapper3 = DocContactPasswordInstructionEmail.New(contact2, Factory);
				AssertEquals(ZString.Empty, wrapper3.CurrentCompanyName);
			}
		}

		class NullCompanyUserContext : UserContext, IUserContext
		{
			public NullCompanyUserContext(IUserContext currentUserContext)
			{
				this.currentUserContext = currentUserContext;
			}
			readonly IUserContext currentUserContext;

			IBranch IUserContext.Branch => currentUserContext.Branch;
			IDepartment IUserContext.Department => currentUserContext.Department;
			IUser IUserContext.User => currentUserContext.User;
			ICompany IUserContext.Company => null;
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod() => DocContactPasswordInstructionEmail.New(Source, Factory);

		DocContactPasswordInstructionEmail Wrapper
		{
			get { return (DocContactPasswordInstructionEmail)base.Wrappers[0]; }
		}

		public override DocumentWrapper[] GetDocumentWrappers() => new DocumentWrapper[] { DocContactPasswordInstructionEmail.New(Source, Factory) };

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
