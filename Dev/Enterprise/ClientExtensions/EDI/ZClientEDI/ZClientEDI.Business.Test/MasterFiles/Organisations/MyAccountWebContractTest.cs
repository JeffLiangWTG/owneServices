using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(MyAccountWebContract))]
	public abstract class MyAccountWebContractTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWebContractContent()
		{
			NotificationEmailTemplate contractTemplate = new NotificationEmailTemplate();
			contractTemplate.EmailBody = "Sample web contract (*ClientName*)";
			GetTermsAndConditionsRegistryItem().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contractTemplate);

			if (ShouldTestNotificationEmail)
			{
				NotificationEmailTemplate emailTemplate = new NotificationEmailTemplate(typeof(DocMyAccountWebContract), "Web Contract For CargoWise My Account Has Been Signed By (*ContactName*) From (*ClientName*)", "(*WebContractContent*)");
				GetNotificationEmailTemplateRegistryItem().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailTemplate);
			}

			MyAccountWebContract helper = GetMyAccountWebContractForTest(Contact);
			AssertEquals("Sample web contract DDD Testing Org Melbourne", helper.WebContractContent);
		}

		public void TestNeedSignWebContract()
		{
			NotificationEmailTemplate contractTemplate = new NotificationEmailTemplate();
			contractTemplate.EmailSubject = "Version 1.0.0";
			contractTemplate.EmailBody = "Sample web contract v1";
			GetTermsAndConditionsRegistryItem().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contractTemplate);

			MyAccountWebContract helper = GetMyAccountWebContractForTest(Contact);
			AssertEquals("Pre-condition: web contract is not signed", true, helper.NeedSignWebContract);

			helper.SignWebContract("215.33.10.104");
			AssertEquals("Web contract has been signed", false, helper.NeedSignWebContract);

			contractTemplate.EmailSubject = "Version 1.0.2";
			contractTemplate.EmailBody = "Sample web contract v2";
			GetTermsAndConditionsRegistryItem().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contractTemplate);

			helper = GetMyAccountWebContractForTest(Contact);
			AssertEquals("Web contract content has been changed and need to sign again", true, helper.NeedSignWebContract);
		}

		public void TestNeedSignWebContract_NotAffectClientSignedWithoutVersion()
		{
			NotificationEmailTemplate contractTemplate = new NotificationEmailTemplate();
			contractTemplate.EmailSubject = "";
			contractTemplate.EmailBody = "Sample web contract no version number";
			GetTermsAndConditionsRegistryItem().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contractTemplate);

			MyAccountWebContract helper = GetMyAccountWebContractForTest(Contact);
			helper.SignWebContract("215.33.10.104");
			AssertEquals("Web contract has been signed", false, helper.NeedSignWebContract);

			contractTemplate.EmailSubject = "";
			contractTemplate.EmailBody = "Sample web contract still has no version number";
			GetTermsAndConditionsRegistryItem().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contractTemplate);

			helper = GetMyAccountWebContractForTest(Contact);
			AssertEquals("No need to sign again since no version number", false, helper.NeedSignWebContract);

			contractTemplate.EmailSubject = "Ver 1.0";
			contractTemplate.EmailBody = "Sample web contract has version number";
			GetTermsAndConditionsRegistryItem().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contractTemplate);

			helper = GetMyAccountWebContractForTest(Contact);
			AssertEquals("Need to sign again now", true, helper.NeedSignWebContract);
		}

		public void TestSignWebContract()
		{
			NotificationEmailTemplate contractTemplate = new NotificationEmailTemplate();
			contractTemplate.EmailSubject = "Version 1.0.0";
			contractTemplate.EmailBody = "Sample web contract";
			GetTermsAndConditionsRegistryItem().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contractTemplate);

			if (ShouldTestNotificationEmail)
			{
				NotificationEmailTemplate emailTemplate = new NotificationEmailTemplate(typeof(DocMyAccountWebContract), "Web Contract For CargoWise My Account Has Been Signed By (*ContactName*) From (*ClientName*)", "(*WebContractContent*)");
				GetNotificationEmailTemplateRegistryItem().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailTemplate);
				SetEmailSender("CargoWise Testing", "tester@cargowise.com");

				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals(0, ((IDocManagerSupport)Organisation).DocManagerInfo.Documents.Count);
			}

			MyAccountWebContract helper = GetMyAccountWebContractForTest(Contact);
			AssertEquals(true, helper.NeedSignWebContract);

			helper.SignWebContract("215.33.10.104");

			if (ShouldTestNotificationEmail)
			{
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("CargoWise Testing", email.FromDisplayName);
				AssertEquals("tester@cargowise.com", email.FromAddress);
				AssertEquals("samuel.wang@cargowise.com", email.Recipients[0].Email);
				AssertEquals("Web Contract For CargoWise My Account Has Been Signed By Samuel From DDD Testing Org Melbourne", email.Subject);
				AssertContains("Sample web contract", email.Body);

				AssertEquals(2, ((IDocManagerSupport)helper.ContractSignatory).DocManagerInfo.Files.Count);
				StorageFile file1 = ((IDocManagerSupport)helper.ContractSignatory).DocManagerInfo.Files[0] as StorageFile;
				AssertEquals("Web Contract Signed (Version 1.0.0) Notification Email.txt", file1.Name);
				StorageFile file2 = ((IDocManagerSupport)helper.ContractSignatory).DocManagerInfo.Files[1] as StorageFile;
				AssertEquals("Web Contract Signed (Version 1.0.0).html", file2.Name);
			}

			AssertEquals(true, helper.HasWebContractSigned);
			AssertEquals(false, helper.NeedSignWebContract);
			AssertEquals(1, helper.ContractSignatory.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Web Contract Signed (Version 1.0.0): Samuel[samuel.wang@cargowise.com]")).Length);
			AssertEquals(1, helper.ContractSignatory.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Web Contract Signed (Version 1.0.0): From 215.33.10.104")).Length);

			StmALog log = helper.ContractSignatory.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Web Contract Signed (Version 1.0.0): Samuel[samuel.wang@cargowise.com]"))[0];
			AssertEquals(Events.ClickThroughAgreementExecuted.Code, log.SL_SE_NKEvent);
			log = helper.ContractSignatory.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Web Contract Signed (Version 1.0.0): From 215.33.10.104"))[0];
			AssertEquals(Events.ClickThroughAgreementExecuted.Code, log.SL_SE_NKEvent);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgContact contact = Factory.New<OrgContact>();
			return GetMyAccountWebContractForTest(contact);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Organisation = Factory.NewWithValidTestData<OrgHeader>();
			Organisation.OH_Code = "DDDTSTMEL";
			Organisation.OH_FullName = "DDD Testing Org Melbourne";
			Contact = Organisation.Contacts.AddNew();
			Contact.OC_ContactName = "Samuel";
			Contact.OC_Email = "samuel.wang@cargowise.com";

			Factory.Save();
		}

		OrgHeader Organisation;
		OrgContact Contact;

		#endregion

		protected abstract MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact);
		protected abstract TermsAndConditionsRegistryItem GetTermsAndConditionsRegistryItem();
		protected abstract NotificationEmailTemplateRegistryItem GetNotificationEmailTemplateRegistryItem();
		protected abstract void SetEmailSender(string name, string address);
		protected virtual bool ShouldTestNotificationEmail => true;
	}
}
