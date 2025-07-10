using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	[TestedType(typeof(CustomerServiceEmailWrapper))]
	sealed class CustomerServiceEmailWrapperTestCase : CustomerServiceEmailBaseWrapperTestCase<CustomerServiceEmailWrapper>
	{
		protected override CustomerServiceEmailWrapper GetDocumentWrapper()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			CustomerServiceEmail email = new CustomerServiceEmail(incident);
			return CustomerServiceEmailWrapper.New(email, Factory);
		}
	}

	[TestedType(typeof(CustomerServiceEmailWrapper))]
	class CustomerServiceEmailWrapperTest : DocumentWrapperTestCase
	{
		public void TestHtmlStyleSheet()
		{
			AssertEquals("HtmlStyleSheet", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value, Wrapper.HtmlStyleSheet);
			Assert("HtmlStyleSheet should not be empty.", Wrapper.HtmlStyleSheet.Length > 0);
		}

		public void TestEmailBody()
		{
			Email.Body = "a\r\nb  c\td<e&f<b>ZZZ</b><i>ZZZ</i>&#39;";
			AssertEquals("EmailBody", "a<br />b  c&nbsp;&nbsp;&nbsp;&nbsp;d<e&f<b>ZZZ</b><i>ZZZ</i>&#39;", Wrapper.EmailBody);
		}

		public void TestContactAndClientName()
		{
			var clientLicence = Billing.Business.Test.BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "PRD", true);
			var client = clientLicence.Company.Header;
			client.OH_FullName = "DDD Company Australia";
			clientLicence.ClientCompany.LCC_OH = ZGuid.Empty;
			clientLicence.ClientCompany.LCC_Name = "DDD Company Inc.";

			var contact = client.Contacts.AddNew();
			contact.OC_ContactName = "Tester one (1)";

			var incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = client.PK;
			incident.IM_LD = clientLicence.Database.PK;
			incident.IM_LCC = clientLicence.ClientCompany.PK;

			var email = new CustomerServiceEmail(incident);
			var wrapper = CustomerServiceEmailWrapper.New(email, Factory);
			wrapper.SetClient(client);
			wrapper.SetContact(contact);
			AssertContains("ClientName", "DDD Company Inc.", wrapper.ContactAndClientName);
			AssertContains("ContactName", "Tester One", wrapper.ContactAndClientName);

			clientLicence.ClientCompany.LCC_Name = "";
			AssertContains("ClientName", "DDD Company Australia", wrapper.ContactAndClientName);

			var address = client.Addresses.AddNew();
			address.OA_Address1 = "1 Test Rd";
			address.OA_CompanyNameOverride = "DDD Company NZ";
			contact.OC_OA_OrgAddress = address.PK;
			AssertContains("ClientName", "DDD Company NZ", wrapper.ContactAndClientName);
		}

		public void TestSignOffNameAndTitle()
		{
			Email.UseCurrentUsersNameAndTitle = false;
			AssertEquals("SignOffNameAndTitle", SupportIncident.SupportDisplayName, Wrapper.SignOffNameAndTitle);

			GlbStaff fromStaff = Factory.New<GlbStaff>();
			fromStaff.GS_FullName = "Sergey Gordok";
			Email.FromDisplayName = fromStaff.GS_FullName;
			AssertEquals("SignOffNameAndTitle", "Sergey Gordok", Wrapper.SignOffNameAndTitle);

			fromStaff.GS_Title = "Sergey";
			AssertEquals("SignOffNameAndTitle", "Sergey Gordok<br />Sergey", Wrapper.SignOffNameAndTitle);
			GlbStaff fromStaff2 = Factory.New<GlbStaff>();
			fromStaff2.GS_FullName = "Sergey Gordok";
			AssertEquals("SignOffNameAndTitle", "Sergey Gordok", Wrapper.SignOffNameAndTitle);

			GlbStaff.CurrentUser.GS_FullName = "Bob Gob";
			GlbStaff.CurrentUser.GS_Title = "";
			Email.UseCurrentUsersNameAndTitle = true;
			AssertEquals("SignOffNameAndTitle", "Bob Gob", Wrapper.SignOffNameAndTitle);

			Email.OverrideSignOffNameAndTitleWithCurrentUsersNameAndTitle = true;
			Email.FromDisplayName = "Customer Service";
			GlbStaff.CurrentUser.GS_Title = "Peon";
			AssertEquals("SignOffNameAndTitle", "Bob Gob<br />Peon", Wrapper.SignOffNameAndTitle);

			Email.OverrideSignOffNameAndTitleWithCurrentUsersNameAndTitle = false;
			Email.UseCurrentUsersNameAndTitle = false;
			AssertEquals("SignOffNameAndTitle", SupportIncident.SupportDisplayName, Wrapper.SignOffNameAndTitle);

			GlbStaff fromStaff3 = Factory.New<GlbStaff>();
			fromStaff3.GS_FullName = "Jenny Nguyen";
			fromStaff3.GS_Title = "Associate Developer";

			Email.FromDisplayName = "Jenny Nguyen";
			AssertEquals("SignOffNameAndTitle", "Jenny Nguyen<br />Associate Developer", Wrapper.SignOffNameAndTitle);
		}

		public void TestSignOffEmailAddress()
		{
			Email.UseCurrentUsersEmailAddress = false;
			AssertEquals("SignOffEmailAddress should be defaulted to SupportEmailAddress", SupportIncident.SupportEmailAddress, Wrapper.SignOffEmailAddress);
			Email.FromEmailAddress = "sergey.gordok@cargowise.com";
			AssertEquals("SignOffEmailAddress should be set to entered FROM(Email Adress)", "sergey.gordok@cargowise.com", Wrapper.SignOffEmailAddress);

			GlbStaff.CurrentUser.GS_EmailAddress = "moo@cows.com";
			Email.UseCurrentUsersEmailAddress = true;
			AssertEquals("SignOffEmailAddress should be set to current user email", "moo@cows.com", Wrapper.SignOffEmailAddress);

			Email.UseCurrentUsersEmailAddress = false;
			AssertEquals("SignOffEmailAddress should be defaulted to SupportEmailAddress", SupportIncident.SupportEmailAddress, Wrapper.SignOffEmailAddress);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			CustomerServiceEmail email = new CustomerServiceEmail(incident);
			return new DocumentWrapper[] { CustomerServiceEmailWrapper.New(email, Factory) };
		}

		CustomerServiceEmailWrapper Wrapper
		{
			get { return (CustomerServiceEmailWrapper)base.Wrappers[0]; }
		}

		CustomerServiceEmail Email
		{
			get { return Wrapper.WrappedObject; }
		}
	}
}
