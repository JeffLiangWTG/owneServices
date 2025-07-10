using System.Collections.Specialized;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI
{
	[TestedType(typeof(PasswordResetHelper))]
	public class PasswordResetHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected string CompanyName => Enterprise.Environment.Env.Registry.MailboxDisplayName;

		public void TestOrgheadersShouldNotContainsDeactivatedOrgHeaders()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = orgHeader1.Contacts.AddNew();
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "testuser@cargowise.com";
			contact1.OC_ContactName = "User A";
			orgHeader1.Contacts.Add(contact1);
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = orgHeader2.Contacts.AddNew();
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = "testuser@cargowise.com";
			contact2.OC_ContactName = "User B";
			orgHeader2.Contacts.Add(contact2);
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = orgHeader3.Contacts.AddNew();
			contact3.OC_IsActive = true;
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_Email = "testuser@cargowise.com";
			contact3.OC_ContactName = "User C";
			orgHeader3.Contacts.Add(contact3);
			Factory.Save();

			var passwordResetHelper1 = new PasswordResetHelper(Factory, "testuser@cargowise.com");
			AssertEquals(3, passwordResetHelper1.OrgHeaders.Count);

			orgHeader3.OH_IsActive = false;
			Factory.Save();

			var passwordResetHelper2 = new PasswordResetHelper(Factory, "testuser@cargowise.com");
			AssertEquals(2, passwordResetHelper2.OrgHeaders.Count);
		}

		public void TestOrgHeadersWhenOrgCodeInConstructor()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "ALBANIA";
			var contact1 = orgHeader1.Contacts.AddNew();
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "testuser@cargowise.com";
			contact1.OC_ContactName = "User A";
			orgHeader1.Contacts.Add(contact1);
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "MONTENE";
			var contact2 = orgHeader2.Contacts.AddNew();
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = "testuser@cargowise.com";
			contact2.OC_ContactName = "User B";
			orgHeader2.Contacts.Add(contact2);
			Factory.Save();

			var passwordResetHelper1 = new PasswordResetHelper(Factory, "testuser@cargowise.com", orgHeader1.OH_Code);
			AssertEquals("Should only show orgheader for passed in code", 1, passwordResetHelper1.OrgHeaders.Count);
			AssertEquals("Should only show orgheader for passed in code", orgHeader1.PK, passwordResetHelper1.OrgHeaders[0].PK);
		}

		public void TestEmailAddress()
		{
			TestPasswordReset.EmailAddress = ZString.Empty;
			AssertEquals("Email address should have an error", 1, TestPasswordReset.EmailAddressInfo.GetErrors().Count());
			AssertEquals("Email address should have an error", "Email address is required", TestPasswordReset.EmailAddressInfo.GetErrors().GetFirstMessage());

			TestPasswordReset.EmailAddress = "Abc";
			AssertEquals("Email address should have NO errors", 0, TestPasswordReset.EmailAddressInfo.GetErrors().Count());

			TestPasswordReset.EmailAddress = TestOrgContactWithoutWebAccess.OC_Email;
			AssertEquals("Email address should have NO errors", 0, TestPasswordReset.EmailAddressInfo.GetErrors().Count());
			AssertEquals(TestOrgContactWithoutWebAccess.PK, TestPasswordReset.defaultOrgContact.PK);

			TestPasswordReset.EmailAddress = TestOrgContactWithWebAccess.OC_Email;
			AssertEquals("Email address should have NO errors", 0, TestPasswordReset.EmailAddressInfo.GetErrors().Count());
			AssertEquals(TestOrgContactWithWebAccess.PK, TestPasswordReset.defaultOrgContact.PK);
		}

		public void TestRunPreSaveValidation()
		{
			AssertEquals(0, TestPasswordReset.EmailAddressInfo.GetErrors().Count());

			TestPasswordReset.RunPreSaveValidation();

			AssertEquals(1, TestPasswordReset.EmailAddressInfo.GetErrors().Count());
		}

		public void TestEmailAddressInfo()
		{
			AssertEquals(OrgContactSchema.OC_Email.MaxLength, TestPasswordReset.EmailAddressInfo.MaxLength);
		}

		public void TestDefaultContact()
		{
			CreateOrgContact("Test User A", false, false);
			CreateOrgContact("Test User B", false, false);

			TestPasswordReset.EmailAddress = "testuser@cargowise.com";
			AssertEquals("Test User A", TestPasswordReset.defaultOrgContact.Name);

			CreateOrgContact("Test User C", false, true);
			CreateOrgContact("Test User D", false, true);
			TestPasswordReset.EmailAddress = ZString.Empty;
			TestPasswordReset.EmailAddress = "testuser@cargowise.com";
			AssertEquals("Test User C", TestPasswordReset.defaultOrgContact.Name);

			CreateOrgContact("Test User E", true, false);
			CreateOrgContact("Test User F", true, false);
			TestPasswordReset.EmailAddress = ZString.Empty;
			TestPasswordReset.EmailAddress = "testuser@cargowise.com";
			AssertEquals("Test User E", TestPasswordReset.defaultOrgContact.Name);

			CreateOrgContact("Test User G", true, true);
			TestPasswordReset.EmailAddress = ZString.Empty;
			TestPasswordReset.EmailAddress = "testuser@cargowise.com";
			AssertEquals("Test User G", TestPasswordReset.defaultOrgContact.Name);
		}

		public void TestDefaultContactShouldPrioritiseContactsWithPersonPassword()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "testuser@cargowise.com";
			contact1.OC_ContactName = "User A";
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = "testuser@cargowise.com";
			contact2.OC_ContactName = "User B";
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_IsActive = true;
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_Email = "testuser@cargowise.com";
			contact3.OC_ContactName = "User C";
			Factory.Save();
			contact2.Person.SetHashedPassword("password");
			TestPasswordReset.EmailAddress = "testuser@cargowise.com";
			AssertEquals("contact 2's person has a password so should be prioritised.", contact2.PK, TestPasswordReset.defaultOrgContact.PK);
		}

		public void TestOrgHeaders()
		{
			var orgContact = TestOrgContactWithWebAccess;
			Factory.Save();

			var passwordResetHelper = new PasswordResetHelper(Factory, "testweb@cargowise.com");
			AssertEquals(1, passwordResetHelper.OrgHeaders.Count);

			passwordResetHelper = new PasswordResetHelper(Factory, "invalidUser@cargowise.com");
			AssertEquals(0, passwordResetHelper.OrgHeaders.Count);

			passwordResetHelper = new PasswordResetHelper(Factory);
			AssertEquals(0, passwordResetHelper.OrgHeaders.Count);
		}

		#region Implementation

		protected void AssertEmailDetails(StringCollection expectedRecipients, string expectedEmailSubject, params string[] partsOfBody)
		{
			AssertEquals("Email count should be one", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Email should not be null", email);

			AssertEquals("Email number of recipients", expectedRecipients.Count, email.Recipients.Count);
			foreach (var recipient in expectedRecipients)
			{
				var found = false;
				for (var i = 0; i < email.Recipients.Count; i++)
				{
					if (email.Recipients[i].Equals(recipient))
					{
						found = true;
						break;
					}
				}
				Assert(string.Format("Recipient {0} not found", recipient), found);
			}

			AssertEquals("Email Subject", expectedEmailSubject, email.Subject);

			foreach (var bodyPart in partsOfBody)
			{
				AssertContains(bodyPart, email.Body);
			}

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		protected GlbStaff CreateStaff(string emailAddress)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			staff.GS_EmailAddress = emailAddress;
			return staff;
		}

		void CreateOrgContact(string contactName, bool isActive, bool isWebAccessEnabled)
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_IsActive = isActive;
			contact.OC_WebAccessEnabled = isWebAccessEnabled;
			contact.OC_Email = "testuser@cargowise.com";
			contact.OC_ContactName = contactName;
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PasswordResetHelper(Factory);
		}

		protected PasswordResetHelper TestPasswordReset;

		protected OrgContact TestOrgContactWithWebAccess
		{
			get
			{
				if (fTestOrgContactWithWebAccess == null)
				{
					/*OrgHeader Org = Factory.NewWithValidTestData<OrgHeader>();
					fTestOrgContactWithWebAccess = Org.Contacts.AddNew();*/
					fTestOrgContactWithWebAccess = Factory.NewWithValidTestData<OrgContact>();
					fTestOrgContactWithWebAccess.OC_Email = "testweb@cargowise.com";
					fTestOrgContactWithWebAccess.OC_WebAccessEnabled = true;
					fTestOrgContactWithWebAccess.OC_IsActive = true;
					fTestOrgContactWithWebAccess.OC_OH = TestOrgHeader.PK;
					fTestOrgContactWithWebAccess.SetHashedPassword("12345");
				}
				return fTestOrgContactWithWebAccess;
			}
		}
		OrgContact fTestOrgContactWithWebAccess;

		protected OrgContact TestOrgContactWithoutWebAccess
		{
			get
			{
				if (fTestOrgContactWithoutWebAccess == null)
				{
					fTestOrgContactWithoutWebAccess = Factory.NewWithValidTestData<OrgContact>();
					fTestOrgContactWithoutWebAccess.OC_Email = "test@cargowise.com";
				}
				return fTestOrgContactWithoutWebAccess;
			}
		}
		OrgContact fTestOrgContactWithoutWebAccess;

		public OrgHeader TestOrgHeader
		{
			get
			{
				if (fOrgHeader == null)
				{
					fOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
					fOrgHeader.OH_Code = "SYDCOMSYD";
				}

				return fOrgHeader;
			}
		}

		OrgHeader fOrgHeader;

		protected override void SetUp()
		{
			base.SetUp();
			TestPasswordReset = (PasswordResetHelper)GetNewBusinessObject();
		}

		protected DocumentParser Parser
		{
			get
			{
				if (parser == null)
				{
					parser = DocumentParser.New(ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactPasswordInstructionEmail>(), Factory);
				}
				return parser;
			}
		}
		DocumentParser parser;

		#endregion
	}
}
