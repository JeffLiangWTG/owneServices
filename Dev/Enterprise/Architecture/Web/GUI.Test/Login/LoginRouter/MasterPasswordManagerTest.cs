using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[TestedType(typeof(MasterPasswordManager))]
	sealed class MasterPasswordManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoginContacts()
		{
			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			activeContact.SetHashedPassword("1234");
			Factory.Save();

			var inactiveContact = Factory.NewWithValidTestData<OrgContact>();
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_WebAccessEnabled = true;
			inactiveContact.OC_PER = activeContact.OC_PER;
			inactiveContact.SetHashedPassword("abcd");

			var webAccessDisabledContact = Factory.NewWithValidTestData<OrgContact>();
			webAccessDisabledContact.OC_WebAccessEnabled = false;
			webAccessDisabledContact.OC_PER = activeContact.OC_PER;
			webAccessDisabledContact.SetHashedPassword("0000");

			var contactWithNoPassword = Factory.NewWithValidTestData<OrgContact>();
			contactWithNoPassword.OC_WebAccessEnabled = true;
			contactWithNoPassword.OC_PER = activeContact.OC_PER;

			var unrelatedContact = Factory.NewWithValidTestData<OrgContact>();
			unrelatedContact.OC_WebAccessEnabled = true;
			unrelatedContact.SetHashedPassword("8888");

			Factory.Save();

			Assert("Precondition OC_IsActive", activeContact.OC_IsActive);
			Assert("Precondition OC_IsActive", !inactiveContact.OC_IsActive);
			Assert("Precondition OC_IsActive", webAccessDisabledContact.OC_IsActive);
			Assert("Precondition OC_IsActive", unrelatedContact.OC_IsActive);
			Assert("Precondition OC_IsActive", unrelatedContact.OC_IsActive);

			Assert("Precondition OC_WebAccessEnabled", activeContact.OC_WebAccessEnabled);
			Assert("Precondition OC_WebAccessEnabled", inactiveContact.OC_WebAccessEnabled);
			Assert("Precondition OC_WebAccessEnabled", !webAccessDisabledContact.OC_WebAccessEnabled);
			Assert("Precondition OC_WebAccessEnabled", contactWithNoPassword.OC_WebAccessEnabled);
			Assert("Precondition OC_WebAccessEnabled", unrelatedContact.OC_WebAccessEnabled);

			Assert("Precondition HasPassword", activeContact.HasPassword);
			Assert("Precondition HasPassword", inactiveContact.HasPassword);
			Assert("Precondition HasPassword", webAccessDisabledContact.HasPassword);
			Assert("Precondition HasPassword", !contactWithNoPassword.HasPassword);
			Assert("Precondition HasPassword", unrelatedContact.HasPassword);

			var manager = new MasterPasswordManager(activeContact.Person);
			AssertEquals("Only active related web enabled contacts are valid for login", 2, manager.LoginContacts.Count);
			AssertEquals("Active contact", true, manager.LoginContacts.Contains(activeContact));
			AssertEquals("Active contact with no password", true, manager.LoginContacts.Contains(contactWithNoPassword));
		}

		public void TestPasswordHoldingContacts()
		{
			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			activeContact.SetHashedPassword("1234");
			Factory.Save();

			var inactiveContact = Factory.NewWithValidTestData<OrgContact>();
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_WebAccessEnabled = true;
			inactiveContact.OC_PER = activeContact.OC_PER;
			inactiveContact.SetHashedPassword("abcd");

			var webAccessDisabledContact = Factory.NewWithValidTestData<OrgContact>();
			webAccessDisabledContact.OC_WebAccessEnabled = false;
			webAccessDisabledContact.OC_PER = activeContact.OC_PER;
			webAccessDisabledContact.SetHashedPassword("0000");

			var contactWithNoPassword = Factory.NewWithValidTestData<OrgContact>();
			contactWithNoPassword.OC_WebAccessEnabled = true;
			contactWithNoPassword.OC_PER = activeContact.OC_PER;

			var unrelatedContact = Factory.NewWithValidTestData<OrgContact>();
			unrelatedContact.OC_WebAccessEnabled = true;
			unrelatedContact.SetHashedPassword("8888");

			Factory.Save();

			Assert("Precondition OC_IsActive", activeContact.OC_IsActive);
			Assert("Precondition OC_IsActive", !inactiveContact.OC_IsActive);
			Assert("Precondition OC_IsActive", webAccessDisabledContact.OC_IsActive);
			Assert("Precondition OC_IsActive", unrelatedContact.OC_IsActive);
			Assert("Precondition OC_IsActive", unrelatedContact.OC_IsActive);

			Assert("Precondition OC_WebAccessEnabled", activeContact.OC_WebAccessEnabled);
			Assert("Precondition OC_WebAccessEnabled", inactiveContact.OC_WebAccessEnabled);
			Assert("Precondition OC_WebAccessEnabled", !webAccessDisabledContact.OC_WebAccessEnabled);
			Assert("Precondition OC_WebAccessEnabled", contactWithNoPassword.OC_WebAccessEnabled);
			Assert("Precondition OC_WebAccessEnabled", unrelatedContact.OC_WebAccessEnabled);

			Assert("Precondition HasPassword", activeContact.HasPassword);
			Assert("Precondition HasPassword", inactiveContact.HasPassword);
			Assert("Precondition HasPassword", webAccessDisabledContact.HasPassword);
			Assert("Precondition HasPassword", !contactWithNoPassword.HasPassword);
			Assert("Precondition HasPassword", unrelatedContact.HasPassword);

			var manager = new MasterPasswordManager(activeContact.Person);
			AssertEquals("Only active related web enabled contacts with passwords are valid for login", 1, manager.PasswordHoldingContacts.Count);
			AssertEquals("Active contact", activeContact.PK, manager.PasswordHoldingContacts.First().PK);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			Factory.Save();
			return new MasterPasswordManager(activeContact.Person);
		}
	}
}
