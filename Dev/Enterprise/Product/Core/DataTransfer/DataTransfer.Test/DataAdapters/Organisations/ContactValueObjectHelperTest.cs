using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	public class ContactValueObjectHelperTest : TestCaseWithFactory
	{
		#region Import

		public void TestFromContactReference()
		{
			Xsd.ContactReference reference = new Xsd.ContactReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "Test Org";
			reference.Organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "Test Org";
			Xsd.OrgContactCollection contactCollection = reference.Organisation.OrganisationDetails.Contacts;

			Xsd.OrgContact contact1 = contactCollection.AddNew();
			contact1.Name = "Contact1";
			contact1.Sequence = 4;
			Xsd.OrgContact contact2 = contactCollection.AddNew();
			contact2.Name = "Contact2";
			contact2.Sequence = 2;

			reference.ContactSequenceRef = 2;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var matchedContact = ContactHelper.FromContactReference(reference, context);
			AssertEquals("Should match the correct Contact", "Contact2", matchedContact.OC_ContactName);

			reference.ContactSequenceRef = 1;
			var unmatchedContact = ContactHelper.FromContactReference(reference, context);
			AssertEquals("Should match no Contact", null, unmatchedContact);

			reference.ContactSequenceRef = 2;
			var secondMatchedContact = ContactHelper.FromContactReference(reference, context);
			ZGuid secondMatchedContactPK = ContactHelper.FromContactReferenceGetContactPK(reference, context);
			string secondMatchedContactName = ContactHelper.FromContactReferenceGetContactName(reference, context);
			AssertEquals("Should match the same Contact as originally matched", matchedContact.PK, secondMatchedContact.PK);
			AssertEquals("Should match the same Contact as originally matched", matchedContact.PK, secondMatchedContactPK);
			AssertEquals("Should match the same Contact as originally matched", matchedContact.OC_ContactName, secondMatchedContactName);
		}

		public void TestImportFromValueObjectCollection()
		{
			#region Value Collection

			Xsd.OrgContactCollection contactsValueCollection = new Xsd.OrgContactCollection();
			Xsd.OrgContact contact1 = contactsValueCollection.AddNew();
			Xsd.OrgContact contact2 = contactsValueCollection.AddNew();
			Xsd.OrgContact contact3 = contactsValueCollection.AddNew();
			Xsd.OrgContact contact4 = contactsValueCollection.AddNew();
			Xsd.OrgContact contact5 = contactsValueCollection.AddNew();

			contact1.Name = "NewContact";

			contact2.Name = "ExistingContact";
			contact2.Birthday = new DateTime(1981, 2, 24);
			contact2.WebAccessEnable = true;

			contact3.Name = "ExistingContact2";
			contact3.WebAccessEnable = false;
			contact3.EmailAddress = "noone@lovesme.com";

			contact4.Name = "TestContact";
			contact4.EmailAddress = "test.contact@gogogo.com";

			var nameTooLong = new ZString('A', OrgContactSchema.OC_ContactName.MaxLength + 1);
			var nameTooLongTruncated = nameTooLong.SubstringSafe(0, OrgContactSchema.OC_ContactName.MaxLength);
			contact5.Name = nameTooLong;

			#endregion

			#region Org Contact List

			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact existingContact = organisation.Contacts.AddNew();
			existingContact.OC_ContactName = "ExistingContact";
			existingContact.OC_Birthday = new DateTime(1981, 5, 28);
			existingContact.OC_WebAccessEnabled = false;

			OrgContact existingContact2 = organisation.Contacts.AddNew();
			existingContact2.OC_ContactName = "ExistingContact2WithDifferentName";
			existingContact2.OC_Email = "noone@lovesme.com";
			existingContact2.OC_WebAccessEnabled = true;
			existingContact2.OC_SystemCreateUser = "~BP";

			OrgContact existingContact3 = organisation.Contacts.AddNew();
			existingContact3.OC_ContactName = "ExistingContact3";
			existingContact3.OC_WebAccessEnabled = true;

			OrgContact existingContact4 = organisation.Contacts.AddNew();
			existingContact4.OC_ContactName = nameTooLongTruncated;

			OrgContact oldSystemGeneratedContact = organisation.Contacts.AddNew();
			oldSystemGeneratedContact.OC_ContactName = "OldSystemGeneratedContact";
			oldSystemGeneratedContact.OC_SystemCreateUser = "~BP";

			OrgContact testContact1 = organisation.Contacts.AddNew();
			testContact1.OC_ContactName = "TestContact";
			testContact1.OC_Email = "test.contact@gogogo.com";
			testContact1.OC_SystemCreateUser = "~BP";

			OrgContact testContact2 = organisation.Contacts.AddNew();
			testContact2.OC_ContactName = "TestContact (1)";
			testContact2.OC_Email = "test.contact@gogogo.com";
			testContact2.OC_SystemCreateUser = "~BP";

			#endregion

			organisation.ShowSystemGeneratedContacts = false;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ContactHelper.ImportFromValueObjectCollection(contactsValueCollection, organisation, context);

			AssertEquals("Organisation.ShowSystemGeneratedContacts should not be changed", false, organisation.ShowSystemGeneratedContacts);

			organisation.ShowSystemGeneratedContacts = true;

			AssertEquals("6 existing contacts, 1 imported (and 1 inactive) contact should exist", 8, organisation.Contacts.Count);

			OrgContact foundContact1 = null;
			OrgContact foundContact2 = null;
			OrgContact foundContact3 = null;
			OrgContact foundContact4 = null;
			OrgContact foundContact5 = null;
			OrgContact foundContact6 = null;
			OrgContact foundContact7 = null;
			OrgContact foundContact8 = null;

			foreach (OrgContact contact in organisation.Contacts)
			{
				if (contact.OC_ContactName == "NewContact")
				{
					foundContact1 = contact;
				}
				else if (contact.OC_ContactName == "ExistingContact")
				{
					foundContact2 = contact;
				}
				else if (contact.OC_ContactName == "ExistingContact2WithDifferentName")
				{
					foundContact3 = contact;
				}
				else if (contact.OC_ContactName == "ExistingContact3")
				{
					foundContact4 = contact;
				}
				else if (contact.OC_ContactName == "OldSystemGeneratedContact")
				{
					foundContact5 = contact;
				}
				else if (contact.OC_ContactName == "TestContact")
				{
					foundContact6 = contact;
				}
				else if (contact.OC_ContactName == "TestContact (1)")
				{
					foundContact7 = contact;
				}
				else if (contact.OC_ContactName == nameTooLongTruncated)
				{
					AssertNull(foundContact8);
					foundContact8 = contact;
				}
			}

			AssertEquals("NewContact", foundContact1.OC_ContactName);

			AssertEquals("ExistingContact", foundContact2.OC_ContactName);
			AssertEquals("ExistingContact DOB should be updated", new DateTime(1981, 2, 24), foundContact2.OC_Birthday);
			AssertEquals("ExistingContact WEb Access should not be updated since no email", false, foundContact2.OC_WebAccessEnabled);

			AssertEquals("Name should never be updated", "ExistingContact2WithDifferentName", foundContact3.OC_ContactName);
			AssertEquals("ExistingContact2 Web Access should NOT be updated - should stay as true", true, foundContact3.OC_WebAccessEnabled);

			AssertEquals("ExistingContact3", foundContact4.OC_ContactName);

			Assert("Old system-generated contact should be made inactive", !foundContact5.OC_IsActive);

			Assert("Contact 6 should not be made inactive", foundContact6.OC_IsActive);
			Assert("Contact 7 should not be made inactive", foundContact7.OC_IsActive);

			AssertEquals(nameTooLongTruncated, foundContact8.OC_ContactName);
		}

		public void TestImportFromValueObjectCollectionWithMultipleSimilarContacts_ExistingContactHasEmailAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";
			contact1.OC_Email = "1";
			contact2.OC_ContactName = "Bob (1)";
			contact2.OC_Email = "2";

			Xsd.OrgContactCollection xsdContacts = new Xsd.OrgContactCollection();
			Xsd.OrgContact xsdContact = xsdContacts.AddNew();
			xsdContact.EmailAddress = "3";
			xsdContact.Name = "Bob";

			AssertEquals("Pre-condition", 2, org.Contacts.Count);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ContactHelper.ImportFromValueObjectCollection(xsdContacts, org, context);
			AssertEquals("Should not change", "1", contact1.OC_Email);
			AssertEquals("Should not change", "Bob", contact1.OC_ContactName);
			AssertEquals("Should not change", "2", contact2.OC_Email);
			AssertEquals("Should not change", "Bob (1)", contact2.OC_ContactName);
			AssertEquals("Should create a new contact for this Bob", 3, org.Contacts.Count);
			AssertEquals("3", org.Contacts[2].OC_Email);
			AssertEquals("Bob (2)", org.Contacts[2].OC_ContactName);
		}

		public void TestImportFromValueObjectCollectionWithMultipleSimilarContacts_ExistingContactHasNoEmailAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";

			Xsd.OrgContactCollection xsdContacts = new Xsd.OrgContactCollection();
			Xsd.OrgContact xsdContact = xsdContacts.AddNew();
			xsdContact.EmailAddress = "1";
			xsdContact.Name = "Bob";

			AssertEquals("Pre-condition", 1, org.Contacts.Count);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ContactHelper.ImportFromValueObjectCollection(xsdContacts, org, context);
			AssertEquals("Should not create a new contact", 1, org.Contacts.Count);
			AssertEquals("Should update the email address", "1", contact1.OC_Email);
			AssertEquals("Should not change", "Bob", contact1.OC_ContactName);
		}

		public void TestImportFromValueObjectCollection_ChangedEmailAndNameDiffersInWhitespace()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Carmen";
			contact1.OC_Email = "oldemail@test.com";

			Xsd.OrgContactCollection xsdContacts = new Xsd.OrgContactCollection();
			Xsd.OrgContact xsdContact = xsdContacts.AddNew();
			xsdContact.EmailAddress = "newemail@test.com\u00A0";
			xsdContact.Name = "Carmen\u00A0\u00A0";

			AssertEquals("Pre-condition", 1, org.Contacts.Count);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ContactHelper.ImportFromValueObjectCollection(xsdContacts, org, context);
			AssertEquals("Should create a new contact", 2, org.Contacts.Count);
			AssertEquals("Should trim the whitespace", "Carmen (1)", org.Contacts[1].OC_ContactName);
			AssertEquals("Should trim the whitespace", "newemail@test.com", org.Contacts[1].OC_Email);
		}

		public void TestImportFromValueObjectCollection_ChangedEmailAndNameReachesMaxLength()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Super Long Contact Name Reaches Max Length";
			if (contact.OC_ContactName.Length < OrgContactSchema.OC_ContactName.MaxLength)
			{
				int numberOfRepeatedChars = OrgContactSchema.OC_ContactName.MaxLength - contact.OC_ContactName.Length;
				contact.OC_ContactName = contact.OC_ContactName + new String('Z', numberOfRepeatedChars);
			}
			contact.OC_Email = "oldemail@test.com";

			Xsd.OrgContactCollection xsdContacts = new Xsd.OrgContactCollection();
			Xsd.OrgContact xsdContact = xsdContacts.AddNew();
			xsdContact.Name = contact.OC_ContactName;

			AssertEquals("Pre-condition", 1, org.Contacts.Count);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			for (int i = 1; i <= 100; i++)
			{
				xsdContact.EmailAddress = "newemail" + i + "@test.com";

				ContactHelper.ImportFromValueObjectCollection(xsdContacts, org, context);
				AssertEquals("Should create a new contact", i + 1, org.Contacts.Count);
				string expectedSuffix = " (" + i + ")";
				AssertEndsWith("Number suffix should be added", expectedSuffix, org.Contacts[i].OC_ContactName);
			}
		}

		public void TestImportFromValueObject_MatchActiveContactOnly()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact 1";
			contact1.OC_Email = "contact1@test.com";
			contact1.OC_IsActive = false;
			AssertEquals("Pre-condition", 1, org.Contacts.Count);

			Xsd.OrgContact xsdContact1 = new Xsd.OrgContact();
			xsdContact1.Name = "Contact 1 New";
			xsdContact1.EmailAddress = "contact1@test.com";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var returnedContact = ContactHelper.ImportFromValueObject(org, xsdContact1, context, false, false);
			AssertEquals("No contact has been added", 1, org.Contacts.Count);
			AssertEquals("Contact 1", returnedContact.OC_ContactName);
			AssertEquals("contact1@test.com", returnedContact.OC_Email);

			returnedContact = ContactHelper.ImportFromValueObject(org, xsdContact1, context, false, true);
			AssertEquals("New contact has been added", 2, org.Contacts.Count);
			AssertEquals("Contact 1 New", org.Contacts[1].OC_ContactName);
			AssertEquals("contact1@test.com", org.Contacts[1].OC_Email);
			AssertEquals("Contact 1 New", returnedContact.OC_ContactName);
			AssertEquals("contact1@test.com", returnedContact.OC_Email);
		}

		public void TestImportOneWithoutLoadingOtherContacts()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact 1";
			contact1.OC_Email = "contact1@test.com";
			Factory.Save();

			Xsd.OrgContact xsdContact = new Xsd.OrgContact();
			xsdContact.Name = "Contact 2";
			xsdContact.EmailAddress = "contact2@test.com";

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var orgInFactory2 = factory2.Load<OrgHeader>(org.PK);

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var returnedContact = ContactHelper.ImportOneWithoutLoadingOtherContacts(orgInFactory2, xsdContact, context, false, false);
			AssertEquals("Contact 2", returnedContact.OC_ContactName);
			AssertEquals("contact2@test.com", returnedContact.OC_Email);
			AssertEquals("contact1 not loaded into factory2", false, ((IBusinessObjectFactoryInternals)factory2).AllBusinessObjects.Any(x => (x as OrgContact)?.PK == contact1.PK));
		}

		public void TestImportOneWithoutLoadingOtherContacts_WebAccess()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contactSameEmail1 = org.Contacts.AddNew();
			contactSameEmail1.OC_ContactName = "Contact 1";
			contactSameEmail1.OC_Email = "shared.email@test.com";
			contactSameEmail1.OC_WebAccessEnabled = true;
			var contactSameEmail2 = org.Contacts.AddNew();
			contactSameEmail2.OC_ContactName = "Contact 2";
			contactSameEmail2.OC_Email = "shared.email@test.com";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Contact 3";
			contact3.OC_Email = "contact3@test.com";
			var contactNoEmail4 = org.Contacts.AddNew();
			contactNoEmail4.OC_ContactName = "Contact 4";
			Factory.Save();

			Xsd.OrgContact xsdContact = new Xsd.OrgContact();
			xsdContact.Name = "Contact 2";
			xsdContact.EmailAddress = "shared.email@test.com";
			xsdContact.WebAccessEnable = true;
			{
				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var orgInFactory2 = factory2.Load<OrgHeader>(org.PK);

				var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				var returnedContact = ContactHelper.ImportOneWithoutLoadingOtherContacts(orgInFactory2, xsdContact, context, false, false);
				factory2.Save();

				AssertEquals("Pick the contact with web access", contactSameEmail1.PK, returnedContact.PK);
				AssertEquals("shared.email@test.com", returnedContact.OC_Email);
				AssertEquals("Name is not updated", "Contact 1", returnedContact.OC_ContactName);
				var loadedContacts = ((IBusinessObjectFactoryInternals)factory2).AllBusinessObjects.Select(x => (x as OrgContact)).Where(x => x != null).ToList();
				AssertEquals("only contact 1 & 2 loaded into factory2", false, loadedContacts.Any(x => x.PK != contactSameEmail1.PK && x.PK != contactSameEmail2.PK));
			}

			// swap web access and repeat
			contactSameEmail1.OC_WebAccessEnabled = false;
			Factory.Save();
			contactSameEmail2.OC_WebAccessEnabled = true;
			Factory.Save();
			{
				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var orgInFactory2 = factory2.Load<OrgHeader>(org.PK);

				var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				var returnedContact = ContactHelper.ImportOneWithoutLoadingOtherContacts(orgInFactory2, xsdContact, context, false, false);
				factory2.Save();

				AssertEquals("Pick the contact with web access", contactSameEmail2.PK, returnedContact.PK);
				AssertEquals("Web access enabled", true, returnedContact.OC_WebAccessEnabled);
				AssertEquals("shared.email@test.com", returnedContact.OC_Email);
				AssertEquals("Name is not updated", "Contact 2", returnedContact.OC_ContactName);
				var loadedContacts = ((IBusinessObjectFactoryInternals)factory2).AllBusinessObjects.Select(x => (x as OrgContact)).Where(x => x != null).ToList();
				AssertEquals("only contact 1 & 2 loaded into factory2", false, loadedContacts.Any(x => x.PK != contactSameEmail1.PK && x.PK != contactSameEmail2.PK));
			}

			contactSameEmail1.OC_WebAccessEnabled = false;
			contactSameEmail2.OC_WebAccessEnabled = false;
			Factory.Save();
			{
				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var orgInFactory2 = factory2.Load<OrgHeader>(org.PK);

				var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				var returnedContact = ContactHelper.ImportOneWithoutLoadingOtherContacts(orgInFactory2, xsdContact, context, false, false);
				factory2.Save();

				AssertEquals("Pick the contact with same name", contactSameEmail2.PK, returnedContact.PK);
				AssertEquals("Web access enabled", true, returnedContact.OC_WebAccessEnabled);
				AssertEquals("shared.email@test.com", returnedContact.OC_Email);
				AssertEquals("Name is not updated", "Contact 2", returnedContact.OC_ContactName);
				var loadedContacts = ((IBusinessObjectFactoryInternals)factory2).AllBusinessObjects.Select(x => (x as OrgContact)).Where(x => x != null).ToList();
				AssertEquals("only contact 1 & 2 loaded into factory2", false, loadedContacts.Any(x => x.PK != contactSameEmail1.PK && x.PK != contactSameEmail2.PK));

				returnedContact.OC_WebAccessEnabled = false;
				factory2.Save();
			}

			xsdContact.Name = "Contact 1";
			{
				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var orgInFactory2 = factory2.Load<OrgHeader>(org.PK);

				var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				var returnedContact = ContactHelper.ImportOneWithoutLoadingOtherContacts(orgInFactory2, xsdContact, context, false, false);
				factory2.Save();

				AssertEquals("Pick the contact with same name", contactSameEmail1.PK, returnedContact.PK);
				AssertEquals("Web access enabled", true, returnedContact.OC_WebAccessEnabled);
				AssertEquals("shared.email@test.com", returnedContact.OC_Email);
				AssertEquals("Name is not updated", "Contact 1", returnedContact.OC_ContactName);
				var loadedContacts = ((IBusinessObjectFactoryInternals)factory2).AllBusinessObjects.Select(x => (x as OrgContact)).Where(x => x != null).ToList();
				AssertEquals("only contact 1 & 2 loaded into factory2", false, loadedContacts.Any(x => x.PK != contactSameEmail1.PK && x.PK != contactSameEmail2.PK));
			}

			xsdContact.Name = "Contact 4";
			xsdContact.EmailAddress = "";
			{
				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var orgInFactory2 = factory2.Load<OrgHeader>(org.PK);

				var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				var returnedContact = ContactHelper.ImportOneWithoutLoadingOtherContacts(orgInFactory2, xsdContact, context, false, false);
				factory2.Save();

				AssertEquals("Pick the contact with same name", contactNoEmail4.PK, returnedContact.PK);
				AssertEquals("Web access NOT enabled since no email", false, returnedContact.OC_WebAccessEnabled);
				AssertEquals("", returnedContact.OC_Email);
				AssertEquals("Name is not updated", "Contact 4", returnedContact.OC_ContactName);
				var loadedContacts = ((IBusinessObjectFactoryInternals)factory2).AllBusinessObjects.Select(x => (x as OrgContact)).Where(x => x != null).ToList();
				AssertEquals("only contact 4 loaded into factory2", false, loadedContacts.Any(x => x.PK != contactNoEmail4.PK));
			}
		}

		public void TestGetNormalizedPhone()
		{
			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Bob";
			contact.OC_OA_OrgAddress = address.PK;

			Xsd.OrgContactCollection xsdContacts = new Xsd.OrgContactCollection();
			Xsd.OrgContact xsdContact = xsdContacts.AddNew();
			xsdContact.EmailAddress = "1";
			xsdContact.Name = "Bob";
			xsdContact.Phone = "02 8001 2200";
			xsdContact.Mobile = "0426 829924";
			xsdContact.HomePhone = "+86 010 65281649";
			xsdContact.OtherPhone = "+86 156 0113 1981";
			xsdContact.Pager = "";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ContactHelper.ImportFromValueObjectCollection(xsdContacts, org, context);

			AssertEquals("+61280012200", contact.OC_Phone);
			AssertEquals("+61426829924", contact.OC_Mobile);
			AssertEquals("+861065281649", contact.OC_HomePhone);
			AssertEquals("+8615601131981", contact.OC_OtherPhone);
			AssertEquals("", contact.OC_Pager);
		}

		public void TestImport_ConvertInvalidToEmpty()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			Xsd.OrgContactCollection xsdContacts = new Xsd.OrgContactCollection();
			Xsd.OrgContact xsdContact1 = xsdContacts.AddNew();
			xsdContact1.EmailAddress = "3";
			xsdContact1.Name = "Bob";
			xsdContact1.Phone = "ABC";
			xsdContact1.Fax = "++123";
			xsdContact1.Mobile = "ZZZ";
			xsdContact1.HomePhone = "XXX";

			Xsd.OrgContact xsdContact2 = xsdContacts.AddNew();
			xsdContact2.EmailAddress = "4";
			xsdContact2.Name = "Jim";
			xsdContact2.Phone = "+61280012200";
			xsdContact2.Fax = "+861065281649";
			xsdContact2.Mobile = "+61426829924";
			xsdContact2.HomePhone = "+8615601131981";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ContactHelper.ImportFromValueObjectCollection(xsdContacts, org, context, true);
			OrgContact contact1 = org.Contacts[0];
			AssertEquals("Bob", contact1.OC_ContactName);
			AssertEquals("", contact1.OC_Phone);
			AssertEquals("", contact1.OC_Fax);
			AssertEquals("", contact1.OC_Mobile);
			AssertEquals("", contact1.OC_HomePhone);

			OrgContact contact2 = org.Contacts[1];
			AssertEquals("Jim", contact2.OC_ContactName);
			AssertEquals("+61280012200", contact2.OC_Phone);
			AssertEquals("+86 10 6528 1649", contact2.OC_Fax_Formatted);
			AssertEquals("+61426829924", contact2.OC_Mobile);
			AssertEquals("+86 156 0113 1981", contact2.OC_HomePhone_Formatted);
		}

		public void TestFindOneWithoutLoadingOtherContacts()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";
			contact1.OC_Email = "";
			contact2.OC_ContactName = "Bob (1)";
			contact2.OC_Email = "bob2@test.com";

			Factory.Save();

			var xsdContacts = new Xsd.OrgContactCollection();
			var xsdContact = xsdContacts.AddNew();
			xsdContact.Name = "Bob";
			xsdContact.EmailAddress = "bob2@test.com";

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var orgInAnotherFactory = factory2.Load<OrgHeader>(org.PK);

			var foundContact = ContactHelper.FindOneWithoutLoadingOtherContacts(org, xsdContact);
			AssertEquals(contact2, foundContact);
			AssertEquals("contact1 not loaded into factory2", false, ((IBusinessObjectFactoryInternals)factory2).AllBusinessObjects.Any(x => (x as OrgContact)?.PK == contact1.PK));

			xsdContact.EmailAddress = "";
			foundContact = ContactHelper.FindOneWithoutLoadingOtherContacts(org, xsdContact);
			AssertEquals(contact1, foundContact);

			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Bobby";
			contact3.OC_Email = "bob2@test.com";
			contact3.OC_WebAccessEnabled = true;
			xsdContact.EmailAddress = "bob2@test.com";
			xsdContact.WebAccessEnable = true;
			foundContact = ContactHelper.FindOneWithoutLoadingOtherContacts(org, xsdContact);
			AssertEquals(contact3, foundContact);

			xsdContact.WebAccessEnable = false;
			foundContact = ContactHelper.FindOneWithoutLoadingOtherContacts(org, xsdContact);
			AssertEquals(null, foundContact);
		}

		#endregion

		#region Export

		public void TestToContactReference()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "splaty";
			OrgContact contact1 = organisation.Contacts.AddNew();
			contact1.OC_ContactName = "Contact1";
			OrgContact contact2 = organisation.Contacts.AddNew();
			contact2.OC_ContactName = "Contact2";

			Xsd.ContactReference reference = ContactHelper.ToContactReference(contact2, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.OrgContact referredContact = null;
			foreach (Xsd.OrgContact contact in reference.Organisation.OrganisationDetails.Contacts)
			{
				if (contact.Sequence == reference.ContactSequenceRef)
				{
					referredContact = contact;
					break;
				}
			}
			AssertEquals("Should find the referenced Contact", "Contact2", referredContact.Name);
		}

		public void TestExportToValueObjectCollection()
		{
			ContactValueObjectHelper helper = new ContactValueObjectHelper("Error context");
			OrgHeader organisation = Factory.New<OrgHeader>();
			var contact1 = organisation.Contacts.AddNew();
			var contact2 = organisation.Contacts.AddNew();
			var contact3 = organisation.Contacts.AddNew();

			contact1.OC_ContactName = "Contact1";
			contact2.OC_ContactName = "Contact2";
			contact3.OC_ContactName = "";

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.OrgContactCollection contactValueCollection = new Xsd.OrgContactCollection();
			helper.ExportToValueObjectCollection(organisation.Contacts, contactValueCollection, new ValueObjectExportContext(notify));

			AssertEquals("There should be 2 contacts exported", 2, contactValueCollection.Count);
			AssertEquals("Sequence", 1, contactValueCollection[0].Sequence);
			AssertEquals("ContactName", "Contact1", contactValueCollection[0].Name);
			AssertEquals("Sequence", 2, contactValueCollection[1].Sequence);
			AssertEquals("ContactName", "Contact2", contactValueCollection[1].Name);
		}

		public void TestExportToValueObjectCollection_IncludeNonEnglish()
		{
			ContactValueObjectHelper helper = new ContactValueObjectHelper("Error context");
			OrgHeader organisation = Factory.New<OrgHeader>();

			OrgContact contact1 = organisation.Contacts.AddNew();
			contact1.OC_Language = Core.Constants.Languages.English;
			contact1.OC_ContactName = "English";

			OrgContact contact2 = organisation.Contacts.AddNew();
			contact2.OC_Language = "";
			contact2.OC_ContactName = "EnglishBecauseLanguageEmpty";

			OrgContact contact3 = organisation.Contacts.AddNew();
			contact3.OC_Language = Core.Constants.Languages.Malay;
			contact3.OC_ContactName = "Chinese";

			OrgContact contact4 = organisation.Contacts.AddNew();
			contact4.OC_Language = Core.Constants.Languages.ChineseSimplified;
			contact4.OC_ContactName = "Chinese";

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.OrgContactCollection contactValueCollection = new Xsd.OrgContactCollection();
			helper.ExportToValueObjectCollection(organisation.Contacts, contactValueCollection, new ValueObjectExportContext(notify));

			AssertEquals("Only the English contacts should be exported", 4, contactValueCollection.Count);
		}

		public void TestExportToValueObjectCollectionWithParentIDocAddress()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgContact contact1 = organisation.Contacts.AddNew();
			OrgContact contact2 = organisation.Contacts.AddNew();
			OrgContact contact3 = organisation.Contacts.AddNew();

			contact1.OC_ContactName = "Contact1";
			contact2.OC_ContactName = "Contact2";
			contact3.OC_ContactName = "Contact3";

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.OrgContactCollection contactValueCollection = new Xsd.OrgContactCollection();

			Enterprise.MasterFiles.Business.Testing.DummyWithDocAddress dummy = Factory.New<Enterprise.MasterFiles.Business.Testing.DummyWithDocAddress>();

			ContactHelper = new ContactValueObjectHelper("", dummy);
			JobDocAddress addr1 = JobDocAddress.New(dummy);
			addr1.ContactPK = contact2.PK;
			(dummy as IDocAddresses).DocAddresses.Add(addr1);
			ContactHelper.ExportToValueObjectCollection(organisation.Contacts, contactValueCollection, new ValueObjectExportContext(notify));

			AssertEquals("There should be 1 contact exported", 1, contactValueCollection.Count);
			AssertEquals("Sequence", 1, contactValueCollection[0].Sequence);
			AssertEquals("Correct contact", "Contact2", contactValueCollection[0].Name);
		}

		#endregion

		#region Implementation

		ContactValueObjectHelper ContactHelper = new ContactValueObjectHelper("");

		#endregion
	}
}
