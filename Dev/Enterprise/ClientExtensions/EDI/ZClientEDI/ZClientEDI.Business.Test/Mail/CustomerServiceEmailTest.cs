using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	[TestedType(typeof(CustomerServiceEmail))]
	sealed class CustomerServiceEmailTest : CustomerServiceEmailTestCase<CustomerServiceEmail>
	{
		enum eDocLoadType
		{
			Unloaded = 0,
			Loaded = 1,
			DelayedLoaded = 2
		}

		public void TestSendEmailAndPublishedAttachmentWitheDocUnloaded()
		{
			TestSendEmailAndPublishedAttachment(eDocLoadType.Unloaded);
		}

		public void TestSendEmailAndPublishedAttachmentWitheDocLoaded()
		{
			TestSendEmailAndPublishedAttachment(eDocLoadType.Loaded);
		}

		public void TestSendEmailAndPublishedAttachmentWitheDocUnLoadedThenLoaded()
		{
			TestSendEmailAndPublishedAttachment(eDocLoadType.DelayedLoaded);
		}

		public void TestFindEConversation()
		{
			// clear the (cached) assembly data in case a non-EDI AssemblyDataLookup has been loaded
			AssemblyDataLookup.ClearDataForTesting();

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_ReferenceType = "BPW";
			docType.RT_DocType = "COR";
			docType.RT_Desc = "Client Correspondence";
			docType.RT_IsPublished = ZBool.True;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Org";
			org.OH_RL_NKClosestPort = "AUSYD";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "sam@test.com.au";

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			var finder = new BusinessObjectEConversationAttacher();
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Description = "Help me";
			incident.DetailNoteText = "Don't know what i'm doing";
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_ClientIncidentReference = "CL111111";
			incident.IM_Category = "SUP";
			incident.OnLoaded(); // Initialise incident.OriginalStage to prevent sending extra email from SendAutomatedEmailToClientSystem

			incident.EConversation.AddMessageFromCurrentUser("Hola", true, false);
			Factory.Save();

			var result = finder.FindEConversation(null);
			var result2 = finder.FindEConversation(incident);

			AssertEquals(null, result);
			AssertEquals(2, result2.Messages.Count);
		}

		public void TestGetEmailTemplate()
		{
			var incident = Factory.New<SupportIncident>();
			var customerServiceEmail = new CustomerServiceEmailForTest(incident);
			var template = customerServiceEmail.GetEmailTemplate_Exposed();
			AssertContains("(*ContactSalutation*)", template);
			AssertContains("(*EmailBody*)", template);
			AssertContains("(*SignOffNameAndTitle*)", template);
			AssertContains("(*CurrentCompanyOrgProxyName*)", template);
			AssertContains("(*SignOffEmailAddress*)", template);
		}

		void TestSendEmailAndPublishedAttachment(eDocLoadType eDocLoad)
		{
			// clear the (cached) assembly data in case a non-EDI AssemblyDataLookup has been loaded
			AssemblyDataLookup.ClearDataForTesting();

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_ReferenceType = "BPW";
			docType.RT_DocType = "COR";
			docType.RT_Desc = "Client Correspondence";
			docType.RT_IsPublished = ZBool.True;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Org";
			org.OH_RL_NKClosestPort = "AUSYD";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "sam@test.com.au";

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Description = "Help me";
			incident.DetailNoteText = "Don't know what i'm doing";
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_ClientIncidentReference = "CL111111";
			incident.IM_Category = "SUP";
			incident.OnLoaded(); // Initialise incident.OriginalStage to prevent sending extra email from SendAutomatedEmailToClientSystem

			CustomerServiceEmail customerServiceEmail = new CustomerServiceEmail(incident);

			// Simulate loaded/Unloaded eDoc tab on Incident form
			BusinessObject bizObject = customerServiceEmail.BusinessObjectSendingEmail;
			DocManagerInfo docSupportInfo = ((IDocManagerSupport)bizObject).DocManagerInfo;
			DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			StorageMain main = docFactory.RetrieveExistingOrCreateStorageMainForPK(bizObject.PK, IncidentConstants.SupportIncidentDocManagerCode);
			switch (eDocLoad)
			{
				case eDocLoadType.Unloaded:
					docSupportInfo.UseBusinessEntityFactoryAsInternal = false;
					bizObject.UnRegisterEditableChildObject(main);
					break;
				case eDocLoadType.Loaded:
					docSupportInfo.UseBusinessEntityFactoryAsInternal = true;
					bizObject.RegisterEditableChildObject(main);
					break;
				case eDocLoadType.DelayedLoaded:
					// Different factory even UseBusinessEntityFactoryAsInternal=true
					docSupportInfo.UseBusinessEntityFactoryAsInternal = false;
					bizObject.RegisterEditableChildObject(main);
					IDocumentFactory tempMasterFactory = docSupportInfo.MasterFactory; // Initialise factory 
					docSupportInfo.UseBusinessEntityFactoryAsInternal = true;
					break;
			}

			customerServiceEmail.ToEmailAddress = "1@1.com";
			customerServiceEmail.Subject = "This is the subject.";
			customerServiceEmail.Body = "This is the body.";
			customerServiceEmail.FromEmailAddress = "Test@cargowise.com";
			customerServiceEmail.FromDisplayName = "Developer";
			customerServiceEmail.Contact = contact;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			customerServiceEmail.SendEmail();

			// Check that the  EDI-specific assembly data is there and that the correct value is given. 
			// If incorrect value, the second email will not be sent and there'll be a few cascading errors
			CombineAssertions(delegate
			{
				AssertEquals("Correct reference type is found for INC in EDI dictionary", "BPW",
					new ZString(AssemblyDataLookup.GetReferenceTypeFromDocManagerCode(IncidentConstants.SupportIncidentDocManagerCode)));

				var targetEmail = Env.OutgoingMailManager.EmailsCreated.Where(x => !x.Subject.Contains("New Messages in")).ToList();
				AssertEquals(2, targetEmail.Count);
				EmailDef email1 = targetEmail[0];
				AssertEquals("customerServiceEmail.FromDisplayName", "Developer", email1.FromDisplayName);
				AssertEquals("customerServiceEmail.FromAddress", "Test@cargowise.com", email1.FromAddress);
				AssertEquals("customerServiceEmail.Subject", "This is the subject.", email1.Subject);
				Assert("customerServiceEmail.Body", email1.Body.Contains("<b>Samuel,</b>"));

				IDocManagerSupport docManagerSupport = incident;
				AssertEquals(1, docManagerSupport.DocManagerInfo.Files.Count);
				AssertEquals("File name", "Incident Email.txt", ((StorageFile)(docManagerSupport.DocManagerInfo.Files[0])).Name);
				AssertEquals("File doc type", "COR", docManagerSupport.DocManagerInfo.Files[0].DocType);
				AssertEquals("File description", "Client Correspondence", docManagerSupport.DocManagerInfo.Files[0].Description);
				AssertEquals("The file is set as published", true, ((StorageFile)(docManagerSupport.DocManagerInfo.Files[0])).SC_IsPublished);

				EmailDef email2 = targetEmail[1];
				AssertEquals("customerServiceEmail.FromDisplayName", SupportIncident.SupportDisplayName, email2.FromDisplayName);
				AssertEquals("customerServiceEmail.FromAddress", EDIDataRegistry.Instance.IncidentFromEmailAddress.Value, email2.FromAddress);
				AssertEquals("customerServiceEmail.Subject", "Customer Service Incident Raised - Your Ref: CL111111", email2.Subject);
			});

			// clear the assembly data for other unit tests to reload it properly
			AssemblyDataLookup.ClearDataForTesting();
		}
	}

	[TestsSubclassesOf(typeof(CustomerServiceEmail))]
	abstract class CustomerServiceEmailTestCase<T> : EmailToContactBusinessObjectTestCase<T> where T : CustomerServiceEmail
	{
		#region Default Email Address / Display Name

		protected override string DefaultFromEmailAddress { get { return SupportIncident.SupportEmailAddress; } }
		protected override string DefaultFromDisplayName { get { return SupportIncident.SupportDisplayName; } }

		public override void TestSetupDefaultFromAddressCore()
		{
			var emailWithAttachment = GetEmailWithAttachment();
			Assert("UseCurrentUsersNameAndTitle should be false by default", !emailWithAttachment.UseCurrentUsersNameAndTitle);
			Assert("UseCurrentUsersEmailAddress should be false by default", !emailWithAttachment.UseCurrentUsersEmailAddress);
			AssertEquals("FromDisplayName", DefaultFromDisplayName, emailWithAttachment.FromDisplayName);
			AssertEquals("FromEmailAddress", DefaultFromEmailAddress, emailWithAttachment.FromEmailAddress);
			Assert("FromDisplayName readonly", !emailWithAttachment.FromDisplayNameInfo.ReadOnly);
			Assert("FromEmailAddress readonly", !emailWithAttachment.FromEmailAddressInfo.ReadOnly);
		}

		#endregion Default Email Address / Display Name

		public override void TestSaveAsNote()
		{
			AssertEquals("SaveAsNote", false, EmailContactObject.SaveAsNote);
		}

		public void TestClient()
		{
			EDIOrgHeader client = Factory.New<EDIOrgHeader>();
			EmailContactObject.Client = client;
			AssertEquals("Client", client, EmailContactObject.Client);
		}

		public void TestContact()
		{
			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Bob Gob";
			contact.OC_Email = "bob@gob.com";

			EmailContactObject.Contact = contact;
			AssertEquals("Contact", contact, EmailContactObject.Contact);
			AssertEquals("ToDisplayName", "Bob Gob", EmailContactObject.ToDisplayName);
			AssertEquals("ToEmailAddress", "bob@gob.com", EmailContactObject.ToEmailAddress);

			EmailContactObject.Contact = null;
			AssertNull("Contact", EmailContactObject.Contact);
			AssertEquals("ToDisplayName", "", EmailContactObject.ToDisplayName);
			AssertEquals("ToEmailAddress", "", EmailContactObject.ToEmailAddress);
		}

		protected override void AssertSentEmail()
		{
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("ContentType", EmailContentTypes.HTML, email.ContentType);
			AssertEquals("Body.Contains(EmailContactObject.HtmlStyleSheet)", true, email.Body.Contains(SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value));
			AssertEquals("Body.Contains(EmailContactObject.Body)", true, email.Body.Contains(EmailContactObject.Body));

			AssertCorrectImage(email, "edibanner.gif", SystemDataRegistry.Instance.HtmlEmailBannerImage);
			AssertCorrectImage(email, "edifooter.gif", SystemDataRegistry.Instance.HtmlEmailFooterImage);
		}

		void AssertCorrectImage(EmailDef email, string imageName, ImageRegistryItem registryItem)
		{
			AttachmentDef bannerAttachment = null;
			foreach (AttachmentDef attachment in email.Attachments)
			{
				if (attachment.DisplayName == imageName)
				{
					bannerAttachment = attachment;
					break;
				}
			}

			using (MemoryStream stream = new MemoryStream(bannerAttachment.Data))
			using (Image image = Image.FromStream(stream))
			{
				AssertImageEquals("Email Image Attachment value", registryItem.Value, image);
			}
		}
	}

	class CustomerServiceEmailForTest : CustomerServiceEmail
	{
		public CustomerServiceEmailForTest(BusinessObject businessObjectSendingEmail)
			: base(businessObjectSendingEmail)
		{
		}

		public string GetEmailTemplate_Exposed()
		{
			return GetEmailTemplate();
		}
	}
}
