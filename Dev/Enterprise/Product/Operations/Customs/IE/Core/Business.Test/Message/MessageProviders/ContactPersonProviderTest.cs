using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class ContactPersonProviderTest : DataProviderTestCase<ContactPersonProvider>
	{
		public void TestName()
		{
			Contact.OC_ContactName = "Contact Name";
			AssertEquals("Contact Name", Provider.Name);
		}

		public void TestPhoneNumber()
		{
			Contact.OC_Phone = "(01) 858 9843";
			AssertEquals("(01) 858 9843", Provider.PhoneNumber);
		}

		public void TestFax()
		{
			Contact.OC_Fax = "(01) 647 6843";
			AssertEquals("(01) 647 6843", Provider.Fax);
		}

		public void TestEmailAddress()
		{
			Contact.OC_Email = "user@test.ie";
			AssertEquals("user@test.ie", Provider.EmailAddress);
		}

		protected override ContactPersonProvider GetProvider() => ContactPersonProvider.New(Contact);

		OrgContact Contact => contact ?? (contact = Factory.New<OrgContact>());
		OrgContact contact;
	}
}
