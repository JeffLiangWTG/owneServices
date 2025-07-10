using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	internal class OrgContactDependentCollectionExtensionsTest : TestCaseWithFactory
	{
		public void TestFindOrCreateFromStaff()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Developer";
			contact.OC_Email = "dev@cargowise.com";
			contact.OC_Phone = "02-0000-0000";

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "Developer";
			staff1.GS_EmailAddress = "dev@cargowise.com";
			staff1.GS_WorkPhone = "02-0000-0000";

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "Samuel";
			staff2.GS_EmailAddress = "samuel.wang@cargowise.com";
			staff2.GS_WorkPhone = "02-xxxx-xxxx";

			OrgContact loadContact1 = org.Contacts.FindOrCreateFromStaff(staff1);
			AssertEquals("Developer", loadContact1.OC_ContactName);
			AssertEquals("dev@cargowise.com", loadContact1.OC_Email);
			AssertEquals("02-0000-0000", loadContact1.OC_Phone);

			OrgContact loadContact2 = org.Contacts.FindOrCreateFromStaff(staff2);
			AssertEquals("Samuel", loadContact2.OC_ContactName);
			AssertEquals("samuel.wang@cargowise.com", loadContact2.OC_Email);
			AssertEquals("02-xxxx-xxxx", loadContact2.OC_Phone);
		}

		public void TestFindOrCreateFromStaffDoesNotSelectInactiveContacts()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			OrgContact activeContact = org.Contacts.AddNew();
			activeContact.OC_ContactName = "ActiveContact";
			activeContact.OC_Email = "active@cargowise.com";
			activeContact.OC_Phone = "02-0000-0000";
			activeContact.OC_IsActive = true;

			OrgContact inactiveContact = org.Contacts.AddNew();
			inactiveContact.OC_ContactName = "InactiveContact";
			inactiveContact.OC_Email = "inactive@cargowise.com";
			inactiveContact.OC_Phone = "02-0000-0000";
			inactiveContact.OC_IsActive = false;

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "A member which corresponds to the active contact";
			staff1.GS_EmailAddress = "active@cargowise.com";
			staff1.GS_WorkPhone = "02-0000-0000";

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "A member which corresponds to the inactive contact";
			staff2.GS_EmailAddress = "inactive@cargowise.com";
			staff2.GS_WorkPhone = "02-xxxx-xxxx";

			OrgContact loadContact1 = org.Contacts.FindOrCreateFromStaff(staff1);
			AssertEquals("ActiveContact", loadContact1.OC_ContactName);
			AssertEquals("active@cargowise.com", loadContact1.OC_Email);
			AssertEquals("02-0000-0000", loadContact1.OC_Phone);

			OrgContact loadContact2 = org.Contacts.FindOrCreateFromStaff(staff2);
			AssertEquals("A member which corresponds to the inactive contact", loadContact2.OC_ContactName);
			AssertEquals("inactive@cargowise.com", loadContact2.OC_Email);
			AssertEquals("02-xxxx-xxxx", loadContact2.OC_Phone);
		}

		public void TestFindOrCreateFromStaff_UniqueContactName()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact existingContact = org.Contacts.AddNew();
			existingContact.OC_ContactName = "Mark";
			existingContact.OC_Email = "mark.one@cargowise.com";

			GlbStaff anotherMark = Factory.NewWithValidTestData<GlbStaff>();
			anotherMark.GS_FullName = "Mark";
			anotherMark.GS_EmailAddress = "mark.two@cargowise.com";
			OrgContact newContact = org.Contacts.FindOrCreateFromStaff(anotherMark);

			AssertNoExceptionThrown("The new contact should have a unique name within the collection", delegate
			{ Factory.Save(); });
			AssertEquals("Mark (1)", newContact.OC_ContactName);
		}
	}
}