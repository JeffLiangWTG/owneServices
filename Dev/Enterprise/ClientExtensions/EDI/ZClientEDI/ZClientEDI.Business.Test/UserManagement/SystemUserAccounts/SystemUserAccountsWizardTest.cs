using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	[TestedType(typeof(SystemUserAccountsWizard))]
	public class SystemUserAccountsWizardTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEdiCustomerUserAccountCollection()
		{
			var userAccount1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount1.EUA_Email = "email1@wisetech.com";
			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount2.EUA_Email = "wisetech@wisetech.com";
			Factory.Save();

			var systemUserAccounts = GetNewBusinessObject() as SystemUserAccountsWizard;
			systemUserAccounts.EdiCustomerUserAccountCollection.Load();
			Assert(systemUserAccounts.EdiCustomerUserAccountCollection.Cast<EdiCustomerUserAccount>().Any(user => user.EUA_Email == "email1@wisetech.com"));
			Assert(systemUserAccounts.EdiCustomerUserAccountCollection.Cast<EdiCustomerUserAccount>().Any(user => user.EUA_Email == "wisetech@wisetech.com"));
		}

		public void TestOrgContactCollection()
		{
			var lic1 = Factory.NewWithValidTestData<EDIOrgContact>();
			lic1.OC_ContactName = "name1";
			lic1.OC_JobCategory = "NGO";
			var lic2 = Factory.NewWithValidTestData<EDIOrgContact>();
			lic2.OC_ContactName = "testName1";
			lic2.OC_JobCategory = "NGO";
			Factory.Save();

			var systemUserAccounts = GetNewBusinessObject() as SystemUserAccountsWizard;
			systemUserAccounts.OrgContactCollection.Load(new ZQuery(OrgContactSchema.OC_JobCategory, "NGO"));
			Assert(systemUserAccounts.OrgContactCollection.Any(contact => ((EDIOrgContact)contact).OC_ContactName == "name1"));
			Assert(systemUserAccounts.OrgContactCollection.Any(contact => ((EDIOrgContact)contact).OC_ContactName == "testName1"));
		}

		public void TestUserAccountLinkedToContact()
		{
			var orgContact1 = Factory.NewWithValidTestData<EDIOrgContact>();
			var orgContact2 = Factory.NewWithValidTestData<EDIOrgContact>();

			var userAccount1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount1.EUA_Email = "email1@wisetech.com";
			userAccount1.EUA_OC_WebAccessContact = orgContact1.PK;
			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount2.EUA_Email = "wisetech@wisetech.com";
			userAccount2.EUA_OC_WebAccessContact = orgContact1.PK;
			var userAccount3 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount3.EUA_Email = "cargowise@wisetech.com";
			userAccount3.EUA_OC_WebAccessContact = orgContact2.PK;
			Factory.Save();

			var ediCustomerUserAccountCollection = (GetNewBusinessObject() as SystemUserAccountsWizard).UserAccountLinkedToContact(orgContact1);
			ediCustomerUserAccountCollection.Load();
			Assert(ediCustomerUserAccountCollection.Cast<EdiCustomerUserAccount>().Any(userAccount => userAccount.EUA_Email == "email1@wisetech.com"));
			Assert(ediCustomerUserAccountCollection.Cast<EdiCustomerUserAccount>().Any(userAccount => userAccount.EUA_Email == "wisetech@wisetech.com"));
			Assert(!ediCustomerUserAccountCollection.Cast<EdiCustomerUserAccount>().Any(userAccount => userAccount.EUA_Email == "cargowise@wisetech.com"));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SystemUserAccountsWizard(Factory);
		}

		#endregion
	}
}
