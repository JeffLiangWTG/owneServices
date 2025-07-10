using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EdiContactSendEmailSetResetPasswordTest : TestCaseWithFactory
	{
		public void TestSendPasswordInstructionsShouldClearAccountStatus()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_IsActive = true;
			contact.SetHashedPassword("hello");

			var account1 = CreateAccount(contact.PK, ContactRelationshipStatusList.Codes.AccountReactivated);
			var account2 = CreateAccount(contact.PK, ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked);
			var account3 = CreateAccount(contact.PK, ContactRelationshipStatusList.Codes.EmailChanged);
			var account4 = CreateAccount(contact.PK, ContactRelationshipStatusList.Codes.ProductDeactivation);
			var account5 = CreateAccount(contact.PK, ContactRelationshipStatusList.Codes.DistinctEmailRequired);
			var account6 = CreateAccount(contact.PK, ContactRelationshipStatusList.Codes.SelfDeactivation);
			var account7 = CreateAccount(contact.PK, ContactRelationshipStatusList.Codes.DissolvedContactWithPassword);

			Factory.Save();
			contact.Person.SetHashedPassword("isthereanybodythere");
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var instructionSender = new EdiContactSendEmailSetResetPassword();
			instructionSender.SendPasswordInstructions(contact);

			account1.Reload();
			account2.Reload();
			account3.Reload();
			account4.Reload();
			account5.Reload();
			account6.Reload();
			account7.Reload();

			AssertEquals("", account1.EUA_ContactRelationshipStatus);
			AssertEquals("", account2.EUA_ContactRelationshipStatus);
			AssertEquals("", account3.EUA_ContactRelationshipStatus);
			AssertEquals("", account4.EUA_ContactRelationshipStatus);
			AssertEquals(true, account1.EUA_IsContactRelationshipActive);
			AssertEquals(true, account2.EUA_IsContactRelationshipActive);
			AssertEquals(true, account3.EUA_IsContactRelationshipActive);
			AssertEquals(true, account4.EUA_IsContactRelationshipActive);
			AssertEquals("", account5.EUA_ContactRelationshipStatus);
			AssertEquals("", account6.EUA_ContactRelationshipStatus);
			AssertEquals("", account7.EUA_ContactRelationshipStatus);
		}

		EdiCustomerUserAccount CreateAccount(ZGuid contactPk, string status)
		{
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contactPk;
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_ContactRelationshipStatus = status;

			return userAccount;
		}

		public void TestSendPasswordInstructionsShouldClearPersonPasswordIfExists()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_IsActive = true;
			contact.SetHashedPassword("hello");
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			Factory.Save();
			contact.Person.SetHashedPassword("isthereanybodythere");
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var instructionSender = new EdiContactSendEmailSetResetPassword();
			instructionSender.SendPasswordInstructions(contact);

			AssertEquals("Password should be cleared", false, contact.HasPassword);
			AssertEquals("Password should be cleared", false, contact.Person.HasPassword);
		}

		public void TestSendPasswordInstructionsShouldClearContactPasswordIfExists()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_IsActive = true;
			contact.SetHashedPassword("hello");
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var instructionSender = new EdiContactSendEmailSetResetPassword();
			instructionSender.SendPasswordInstructions(contact);

			AssertEquals("Password should be cleared", false, contact.HasPassword);
			AssertEquals("Should never have had a person password", false, contact.Person.HasPassword);
		}

		public void TestSendPasswordInstructionsShouldSendSetMasterPasswordEmail()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_IsActive = true;
			contact.OC_PER = person.PK;
			contact.Person.SetHashedPassword("hello");
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var instructionSender = new EdiContactSendEmailSetResetPassword();
			instructionSender.SendPasswordInstructions(contact);

			var sentEmails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals(1, sentEmails.Count);
			AssertEquals("Precondition: Password should be cleared", false, contact.Person.HasPassword);
			AssertEquals("Precondition: Account contact relationship should be reactivated", true, userAccount.EUA_IsContactRelationshipActive);
			AssertEquals("Precondition: Account contact relationship should be reactivated", string.Empty, userAccount.EUA_ContactRelationshipStatus);
			AssertEquals("Should send to master password page", true, sentEmails[0].Body.Contains("/Admin/SetMasterPassword.aspx?SetKey="));

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			instructionSender = new EdiContactSendEmailSetResetPassword();
			instructionSender.SendPasswordInstructions(contact);
			sentEmails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals(1, sentEmails.Count);
			AssertEquals("Should send to master password page", true, sentEmails[0].Body.Contains("/Admin/SetMasterPassword.aspx?SetKey="));
		}

		public void TestSendPasswordInstructionsShouldFailIfCanSendResetPasswordEmailIsFalse()
		{
			var enterpriseCode = "DDD";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "ABC", "SYD");
			var database = licence.Database;
			var org = database.WebAccessOrg;
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			contact.OC_ContactName = "User 1";
			contact.OC_Email = "u1@cw1.com";
			Factory.Save();

			AssertEquals("Reset should be allowed when no user accounts", true, contact.CanSendResetPasswordEmail);

			var sdaUserAccount = Factory.New<EdiCustomerUserAccount>();
			sdaUserAccount.EUA_LD = database.PK;
			sdaUserAccount.EUA_UserID = "U01";
			sdaUserAccount.EUA_FullName = "User 1";
			sdaUserAccount.EUA_Email = "u1@cw1.com";
			sdaUserAccount.EUA_IsContactRelationshipActive = false;
			sdaUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.SelfDeactivation;
			sdaUserAccount.EUA_OC_WebAccessContact = contact.PK;
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var linkedContact = otherOrg.Contacts.AddNew();
			linkedContact.OC_ContactName = "User 1";
			linkedContact.OC_Email = "u1@cw1.com";
			linkedContact.OC_PER = contact.OC_PER;
			Factory.Save();

			AssertEquals("Precondition: Reset should not be allowed if the contact has no active user accounts and has an SDA", false, contact.CanSendResetPasswordEmail);

			var instructionSender = new EdiContactSendEmailSetResetPassword();
			AssertEquals("Should fail to send password instruction", false, instructionSender.SendPasswordInstructions(contact));
			AssertEquals("This contact is linked to a user account which has been deactivated by the user. Password reset cannot occur from this organization until the user account is reactivated.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
