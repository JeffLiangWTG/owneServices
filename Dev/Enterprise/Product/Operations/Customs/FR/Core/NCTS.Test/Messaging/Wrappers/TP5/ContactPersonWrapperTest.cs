using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class ContactPersonWrapperTest : Customs.Business.Testing.DataProviderTestCase<ContactPersonWrapper>
	{
		public void TestEmailAddress()
		{
			AssertEquals("EmailAddress should be equal to OC_Email", "a@a.com", Provider.EmailAddress);
		}

		public void TestName()
		{
			AssertEquals("Name should be equal to OC_ContactName", "Full Name", Provider.Name);
		}

		public void TestPhoneNumber()
		{
			AssertEquals("PhoneNumber should be equal to OC_Phone_Formatted", "+33 1 01 01 01 01", Provider.PhoneNumber);
		}

		protected override ContactPersonWrapper GetProvider()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_Email = "a@a.com";
			contact.OC_ContactName = "Full Name";
			contact.OC_Phone_Formatted = "+33 1 01 01 01 01";
			return ContactPersonWrapper.New(contact);
		}
	}
}
