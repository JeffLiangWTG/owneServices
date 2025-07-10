using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus;
using Enterprise.Client.EDI.UserManagement.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Organisation.Business.Test
{
	[TestedType(typeof(EdiAccountVerificationStatusCollection))]
	sealed class EdiAccountVerificationStatusCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EdiAccountVerificationStatusCollection>
	{
		public void TestUserAccounts()
		{
			var contact = CreateEdiOrgContactWithVerificationRequired(new[]
			{
				ContactRelationshipStatusList.Codes.AccountReactivated,
				ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked,
				ContactRelationshipStatusList.Codes.EmailChanged,
				ContactRelationshipStatusList.Codes.ProductDeactivation,
				ContactRelationshipStatusList.Codes.DistinctEmailRequired,
				ContactRelationshipStatusList.Codes.SelfDeactivation,
				ContactRelationshipStatusList.Codes.DissolvedContactWithPassword
			});

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_ContactRelationshipStatus = "";
			userAccount1.EUA_IsContactRelationshipActive = false;
			userAccount1.EUA_OC_WebAccessContact = contact.PK;
			userAccount1.EUA_UserID = "AAA";

			Factory.Save();
			var collection = new EdiAccountVerificationStatusCollection(Factory, contact);

			AssertEquals(8, collection.UserAccounts.Count);
			AssertContainsExactElementsInAnyOrder(new string[]
			{
				ContactRelationshipStatusList.Codes.AccountReactivated,
				ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked,
				ContactRelationshipStatusList.Codes.EmailChanged,
				ContactRelationshipStatusList.Codes.ProductDeactivation,
				ContactRelationshipStatusList.Codes.DistinctEmailRequired,
				ContactRelationshipStatusList.Codes.SelfDeactivation,
				ContactRelationshipStatusList.Codes.DissolvedContactWithPassword,
				""
			}, collection.UserAccounts.Select(a => a.EUA_ContactRelationshipStatus));
		}

		public void TestFetchCollectionBasedOnContactWithVerificationRequired()
		{
			var contact = CreateEdiOrgContactWithVerificationRequired();
			Factory.Save();
			var collection = new EdiAccountVerificationStatusCollection(Factory, contact);

			Assert(collection.RelationshipPromptRequired);
		}

		public void TestShouldLoadProductDeactivation()
		{
			var contact = CreateEdiOrgContactWithVerificationRequired(new[] { ContactRelationshipStatusList.Codes.ProductDeactivation });
			Factory.Save();
			var collection = new EdiAccountVerificationStatusCollection(Factory, contact);

			AssertEquals(1, collection.Count);
		}

		public void TestReload()
		{
			var contact = CreateEdiOrgContactWithVerificationRequired(new[] { ContactRelationshipStatusList.Codes.ProductDeactivation });
			Factory.Save();
			var collection = new EdiAccountVerificationStatusCollection(Factory, contact);

			AssertEquals(1, collection.Count);

			var ediCustomerUserAccount = Factory.New<EdiCustomerUserAccount>();

			ediCustomerUserAccount.EUA_LD = database.PK;
			ediCustomerUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked;
			ediCustomerUserAccount.EUA_IsContactRelationshipActive = false;
			ediCustomerUserAccount.EUA_OC_WebAccessContact = contact.PK;
			ediCustomerUserAccount.EUA_UserID = ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked;
			Factory.Save();

			AssertEquals("Precondition", 1, collection.Count);

			collection.Reload();
			AssertEquals("Reload should repopulate with new user account", 2, collection.Count);
		}

		protected override EdiAccountVerificationStatusCollection GetCollectionToTest()
		{
			return new EdiAccountVerificationStatusCollection(Factory, Factory.New<EDIOrgContact>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new EdiAccountVerificationStatus(Factory);

		LicenceDatabase database;

		EDIOrgContact CreateEdiOrgContactWithVerificationRequired(string[] statuses = null)
		{
			var orgHeader = Factory.New<EDIOrgHeader>();
			orgHeader.OH_FullName = "Test organization INC";
			orgHeader.OH_Code = "TOINC";

			var contact = Factory.New<EDIOrgContact>();
			contact.OC_OH = orgHeader.PK;

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			if (statuses == null)
			{
				statuses = new string[] { ContactRelationshipStatusList.Codes.EmailChanged };
			}

			database = licence.Database;

			foreach (var status in statuses)
			{
				var ediCustomerUserAccount = Factory.New<EdiCustomerUserAccount>();

				ediCustomerUserAccount.EUA_LD = database.PK;
				ediCustomerUserAccount.EUA_ContactRelationshipStatus = status;
				ediCustomerUserAccount.EUA_OC_WebAccessContact = contact.PK;
				ediCustomerUserAccount.EUA_UserID = status;
			}

			return contact;
		}
	}
}
