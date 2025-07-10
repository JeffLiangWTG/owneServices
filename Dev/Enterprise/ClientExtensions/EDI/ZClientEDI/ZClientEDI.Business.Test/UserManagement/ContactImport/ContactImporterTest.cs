using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	class ContactImporterTest : TestCaseWithFactory
	{
		public void TestGetDuplicateContactFromOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "a@a.com";
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = false;
			var inactiveContact = org.Contacts.AddNew();
			inactiveContact.OC_ContactName = "User 1 [3]";
			inactiveContact.OC_Email = "a@a.com";
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_WebAccessEnabled = true;
			Factory.Save();

			var duplicateContact = ContactImporter.GetDuplicateContactFromOrg(org.PK, contact1.OC_Email, contact1, Factory);
			AssertEquals("Only active contacts should be found", null, duplicateContact);

			var nonWebAccessContact = org.Contacts.AddNew();
			nonWebAccessContact.OC_ContactName = "User 1 [2]";
			nonWebAccessContact.OC_Email = "a@a.com";
			nonWebAccessContact.OC_IsActive = true;
			nonWebAccessContact.OC_WebAccessEnabled = false;
			Factory.Save();

			duplicateContact = ContactImporter.GetDuplicateContactFromOrg(org.PK, contact1.OC_Email, contact1, Factory);
			AssertEquals("nonWebAccessContact should be found", nonWebAccessContact.PK, duplicateContact.PK);

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User 1 [1]";
			contact2.OC_Email = "a@a.com";
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			Factory.Save();

			duplicateContact = ContactImporter.GetDuplicateContactFromOrg(org.PK, contact1.OC_Email, contact1, Factory);
			AssertEquals("contact2 should be prioritised as it has web access", contact2.PK, duplicateContact.PK);
		}
	}
}
