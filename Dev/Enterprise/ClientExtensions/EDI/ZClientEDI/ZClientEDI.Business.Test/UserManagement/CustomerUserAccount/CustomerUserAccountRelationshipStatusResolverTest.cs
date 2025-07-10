namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.MasterFiles.Business;

	public class CustomerUserAccountRelationshipStatusResolverTest : TestCaseWithFactory
	{
		public void TestMergeToSameContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User 1 [1]";

			var account1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var account2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			account1.EUA_OC_WebAccessContact = contact1.PK;
			account1.EUA_ContactRelationshipStatus = "DER";
			account1.EUA_IsContactRelationshipActive = false;
			account2.EUA_OC_WebAccessContact = contact2.PK;
			account2.EUA_ContactRelationshipStatus = "DER";
			account2.EUA_IsContactRelationshipActive = false;
			Factory.Save();

			account1.WebAccessContact.OC_ContactName = "User 1";
			account1.WebAccessContact.OC_Email = "a@a.com";
			account1.WebAccessContact.OC_IsActive = true;
			account1.WebAccessContact.OC_WebAccessEnabled = true;

			account2.WebAccessContact.OC_ContactName = "User 1 [1]";
			account2.WebAccessContact.OC_Email = "b@a.com";
			account2.WebAccessContact.OC_IsActive = true;
			account2.WebAccessContact.OC_WebAccessEnabled = true;
			Factory.Save();

			var sourceContactPk = account1.WebAccessContact.PK;
			var sourcePersonPk = account1.WebAccessContact.Person.PK;
			AssertNotEquals(account1.WebAccessContact.PK, account2.WebAccessContact.PK);
			AssertNotEquals(account1.WebAccessContact.Person.PK, account2.WebAccessContact.Person.PK);

			CustomerUserAccountRelationshipStatusResolver.MergeToSameContact(account1.PK, account2.PK);

			var factory = new BusinessObjectFactory();
			var sourceUserAccount = factory.Load<EdiCustomerUserAccount>(account1.PK);
			var targetUserAccount = factory.Load<EdiCustomerUserAccount>(account2.PK);

			AssertEquals(sourceContactPk, sourceUserAccount.WebAccessContact.PK);
			AssertEquals(sourcePersonPk, sourceUserAccount.WebAccessContact.Person.PK);
			AssertEquals(sourceUserAccount.WebAccessContact.PK, targetUserAccount.WebAccessContact.PK);
			AssertEquals(sourceUserAccount.WebAccessContact.Person.PK, targetUserAccount.WebAccessContact.Person.PK);
			AssertEquals("", sourceUserAccount.EUA_ContactRelationshipStatus);
			AssertEquals(true, sourceUserAccount.EUA_IsContactRelationshipActive);
			AssertEquals("", targetUserAccount.EUA_ContactRelationshipStatus);
			AssertEquals(true, targetUserAccount.EUA_IsContactRelationshipActive);
		}

		public void TestMergeToContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User 1 [1]";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "User 1 [2]";

			var account1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var account2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var account3 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			account1.EUA_OC_WebAccessContact = contact1.PK;
			account1.EUA_ContactRelationshipStatus = "DER";
			account1.EUA_IsContactRelationshipActive = false;
			account2.EUA_OC_WebAccessContact = contact2.PK;
			account2.EUA_Email = "a@a.com";
			account3.EUA_OC_WebAccessContact = contact3.PK;
			account3.EUA_ContactRelationshipStatus = "DER";
			account3.EUA_IsContactRelationshipActive = false;
			account3.EUA_Email = "a@a.com";
			Factory.Save();

			account1.WebAccessContact.OC_ContactName = "User 1";
			account1.WebAccessContact.OC_Email = "b@a.com";
			account1.WebAccessContact.OC_IsActive = true;
			account1.WebAccessContact.OC_WebAccessEnabled = false;

			account2.WebAccessContact.OC_ContactName = "User 1 [1]";
			account2.WebAccessContact.OC_Email = "a@a.com";
			account2.WebAccessContact.OC_IsActive = true;
			account2.WebAccessContact.OC_WebAccessEnabled = true;

			account3.WebAccessContact.OC_ContactName = "User 1 [2]";
			account3.WebAccessContact.OC_Email = "a@a.com";
			account3.WebAccessContact.OC_IsActive = true;
			account3.WebAccessContact.OC_WebAccessEnabled = false;
			Factory.Save();

			AssertNotEquals(account1.WebAccessContact.PK, account2.WebAccessContact.PK);
			AssertNotEquals(account2.WebAccessContact.PK, account3.WebAccessContact.Person.PK);
			AssertNotEquals(account1.WebAccessContact.Person.PK, account2.WebAccessContact.Person.PK);
			AssertNotEquals(account1.WebAccessContact.Person.PK, account3.WebAccessContact.Person.PK);

			CustomerUserAccountRelationshipStatusResolver.MergeToContact(account1, contact2);
			CustomerUserAccountRelationshipStatusResolver.MergeToContact(account3, contact2);

			account1.Reload();
			account2.Reload();
			account3.Reload();
			contact1.Reload();
			contact2.Reload();
			contact3.Reload();

			AssertEquals(contact2.PK, account1.EUA_OC_WebAccessContact);
			AssertEquals(contact2.PK, account2.EUA_OC_WebAccessContact);
			AssertEquals(contact2.PK, account3.EUA_OC_WebAccessContact);
			AssertEquals(contact1.OC_PER, contact2.OC_PER);
			AssertEquals(contact2.OC_PER, contact3.OC_PER);
			AssertEquals(false, contact1.OC_IsActive);
			AssertEquals(false, contact1.OC_WebAccessEnabled);
			AssertEquals(string.Empty, contact1.OC_Email);
			AssertEquals(false, contact3.OC_IsActive);
			AssertEquals(false, contact3.OC_WebAccessEnabled);
			AssertEquals(string.Empty, contact3.OC_Email);

			AssertEquals(true, contact2.OC_IsActive);
			AssertEquals(true, contact2.OC_WebAccessEnabled);
			AssertEquals("a@a.com", contact2.OC_Email);
			AssertEquals(true, account1.EUA_IsEmailOverridden);
			AssertEquals("a@a.com", account2.EUA_Email);
			AssertEquals(false, account3.EUA_IsEmailOverridden);
			AssertEquals(true, account1.EUA_IsContactRelationshipActive);
			AssertEquals(true, account2.EUA_IsContactRelationshipActive);
			AssertEquals(true, account3.EUA_IsContactRelationshipActive);
			AssertEquals(string.Empty, account1.EUA_ContactRelationshipStatus);
			AssertEquals(string.Empty, account2.EUA_ContactRelationshipStatus);
			AssertEquals(string.Empty, account3.EUA_ContactRelationshipStatus);
		}

		public void TestClearRelationshipStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";

			var account1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			account1.EUA_OC_WebAccessContact = contact1.PK;
			account1.EUA_ContactRelationshipStatus = "DER";
			account1.EUA_IsContactRelationshipActive = false;
			Factory.Save();

			CustomerUserAccountRelationshipStatusResolver.ClearRelationshipStatus(new[] { account1.PK });
			var factory = new BusinessObjectFactory();
			var sourceUserAccount = factory.Load<EdiCustomerUserAccount>(account1.PK);
			AssertEquals("", sourceUserAccount.EUA_ContactRelationshipStatus);
			AssertEquals(true, sourceUserAccount.EUA_IsContactRelationshipActive);
		}
	}
}
