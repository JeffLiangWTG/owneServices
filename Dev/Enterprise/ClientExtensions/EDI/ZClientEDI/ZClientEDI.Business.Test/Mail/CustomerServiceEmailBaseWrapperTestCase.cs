using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	[TestedType(typeof(CustomerServiceEmailBaseWrapper))]
	abstract class CustomerServiceEmailBaseWrapperTestCase<T> : DocumentWrapperTestCase
		where T : CustomerServiceEmailBaseWrapper
	{
		[TestDate(2006, 12, 21)]
		public void TestCurrentDate()
		{
			AssertEquals("CurrentDate", ZDateTime.Now.ToShortDateString(), GetDocumentWrapper().CurrentDate);
		}

		public void TestContactAndClientName()
		{
			T wrapper = GetDocumentWrapper();
			wrapper.SetClient(null);
			wrapper.SetContact(null);
			AssertEquals("ContactAndClientName", "", wrapper.ContactAndClientName);

			EDIOrgHeader client = Factory.New<EDIOrgHeader>();
			client.OH_FullName = "Assorted Nuts";

			OrgContact contact = client.Contacts.AddNew();
			contact.OC_ContactName = "MILK o'tea";

			wrapper.SetClient(client);
			AssertEquals("ContactAndClientName", "<p><b>Assorted Nuts</b></p>", wrapper.ContactAndClientName);

			wrapper.SetContact(contact);
			AssertEquals("ContactAndClientName", "<p><b>Milk O'Tea<br />Assorted Nuts</b></p>", wrapper.ContactAndClientName);

			wrapper.SetClient(null);
			AssertEquals("ContactAndClientName", "<p><b>Milk O'Tea<br />Assorted Nuts</b></p>", wrapper.ContactAndClientName);
		}

		public void TestContactSalutation()
		{
			T wrapper = GetDocumentWrapper();
			wrapper.SetClient(null);
			wrapper.SetContact(null);
			AssertEquals("ContactSalutation", "Dear Client", wrapper.ContactSalutation);

			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "milk tea";

			wrapper.SetContact(contact);
			AssertEquals("ContactSalutation", "Milk Tea", wrapper.ContactSalutation);

			contact.OC_Salutation = "milk";
			AssertEquals("ContactSalutation", "Milk", wrapper.ContactSalutation);
		}

		public void TestCurrentCompanyOrgProxyName()
		{
			T wrapper = GetDocumentWrapper();
			AssertEquals("CurrentCompanyOrgProxyName", wrapper.CurrentCompany.Organisation.Name, wrapper.CurrentCompanyOrgProxyName);
			Assert("CurrentCompanyOrgProxyName should not be empty.", wrapper.CurrentCompanyOrgProxyName.Length > 0);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { GetDocumentWrapper() };
		}

		protected abstract T GetDocumentWrapper();
	}
}
