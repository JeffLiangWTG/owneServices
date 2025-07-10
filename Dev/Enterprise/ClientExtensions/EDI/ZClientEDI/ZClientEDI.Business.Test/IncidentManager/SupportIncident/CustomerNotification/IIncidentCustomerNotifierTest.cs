using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Test;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;
using static Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLookups;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class LocalChangeIncidentCustomerNotifierTest : TestCaseWithFactory
	{
		public void TestEConversationIsAppendedToClientEmail()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";

			var task = incident.WorkflowItems.AddNew();
			task.P9_Description = "Task 1";
			task.P9_Type = "UDF";
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			incident.AddStaffMessageToCustomer("Hello, I want to send this message");

			var email = SupportIncidentEmail.New(incident);
			email.Subject = "Queued client email subject";
			email.Body = "This is the queued client email body";
			email.XAutoResponseSuppressHeaderValue = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;

			var sender = new EmailOnlyCustomerNotificationSender();
			var notifier = new LocalChangeNotifierForTest(incident, sender);
			notifier.StandardEmailForTest = email;
			notifier.SendChanges();
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Queued client email subject", sentEmail.Subject);
			AssertContains("This is the queued client email body", sentEmail.Body);
			AssertContains("Hello, I want to send this message", sentEmail.Body);
			AssertNotNull(sentEmail.Headers.TryGetValue(SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress, out var xAutoSuppressHead));
			AssertEquals(SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, xAutoSuppressHead);
		}

		public void TestShouldSendWorkItemCompletedNotification()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "IM12345";
			incident.IM_Description = "Test Incident";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = IncidentMainLookups.Status.Open;
			Factory.Save();
			var sender = new SimpleEmailSenderForTest();
			Assert("No client and contact", !new LocalSystemChangeIncidentCustomerNotifier(incident, sender).ShouldSendWorkItemCompletedNotification);

			OrgHeader client = Factory.New<OrgHeader>();
			OrgContact incidentContact = client.Contacts.AddNew();
			incidentContact.OC_ContactName = "IT Support";
			incidentContact.OC_Email = "itsupport@someclient.com";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = incidentContact.PK;

			Assert("Not waiting for upgrade", !new LocalSystemChangeIncidentCustomerNotifier(incident, sender).ShouldSendWorkItemCompletedNotification);

			var workItem1 = incident.RelatedWorkItems.AddNew();
			var workItemTask1 = workItem1.WorkflowItems.AddNew();
			workItemTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			Assert(new LocalSystemChangeIncidentCustomerNotifier(incident, sender).ShouldSendWorkItemCompletedNotification);

			workItem1.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Assert(!new LocalSystemChangeIncidentCustomerNotifier(incident, sender).ShouldSendWorkItemCompletedNotification);

			var workItem2 = incident.RelatedWorkItems.AddNew();
			var workItemTask2 = workItem2.WorkflowItems.AddNew();
			workItemTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			Assert(new LocalSystemChangeIncidentCustomerNotifier(incident, sender).ShouldSendWorkItemCompletedNotification);

			incident.IM_Source = SupportIncidentLookups.SourceListConstants.IssueManagerReported;
			Assert("No need to create email if created from Issue", !new LocalSystemChangeIncidentCustomerNotifier(incident, sender).ShouldSendWorkItemCompletedNotification);

			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			incident.AddStaffMessageToCustomer("Development Work Completed email sent to contact");
			Assert("Email has previously been sent", !new LocalSystemChangeIncidentCustomerNotifier(incident, sender).ShouldSendWorkItemCompletedNotification);
		}

		public void TestShouldNotSendWorkItemCompletedNotificationWhenInStayIndependentState()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "IM12345";
			incident.IM_Description = "Test Incident";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = IncidentMainLookups.Status.Open;
			Factory.Save();

			var client = Factory.New<OrgHeader>();
			var incidentContact = client.Contacts.AddNew();
			incidentContact.OC_ContactName = "IT Support";
			incidentContact.OC_Email = "itsupport@someclient.com";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = incidentContact.PK;

			var workItem1 = incident.RelatedWorkItems.AddNew();
			var workItemTask1 = workItem1.WorkflowItems.AddNew();
			workItemTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;

			var sender = new SimpleEmailSenderForTest();
			Assert(!incident.ShouldStayIndependent);
			Assert(new LocalSystemChangeIncidentCustomerNotifier(incident, sender).ShouldSendWorkItemCompletedNotification);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.ContentDevelopment;
			Assert(incident.ShouldStayIndependent);
			Assert(!new LocalSystemChangeIncidentCustomerNotifier(incident, sender).ShouldSendWorkItemCompletedNotification);
		}

		#region Work Item Completed Email

		public void TestGetWorkItemCompletedEmail()
		{
			var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var templatePair1 = collection.AddNew();
			templatePair1.Product = "ENT";
			templatePair1.Code = SupportIncidentCategoriesList.Codes.Defect;
			templatePair1.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Incident (*IncidentNumber*) resolved: (*Summary*) - NeedUpgrade:false";
			templatePair1.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = "Incident (*IncidentNumber*) resolved: (*Summary*) - NeedUpgrade:false";
			templatePair1.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "rectify this problem has now been completed. Test Incident with &#39;tag&#39; &lt;b&gt;!";
			templatePair1.EmailTemplates.ERequestV2EmailTemplate.EmailBody = "rectify this problem has now been completed. Test Incident with &#39;tag&#39; &lt;b&gt;!";
			var templatePair2 = collection.AddNew();
			templatePair2.Code = SupportIncidentCategoriesList.Codes.Defect;
			templatePair2.Product = "ENT";
			templatePair2.NeedUpgrade = true;
			templatePair2.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Incident (*IncidentNumber*) resolved: (*Summary*) - NeedUpgrade:true";
			templatePair2.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = "Incident (*IncidentNumber*) resolved: (*Summary*) - NeedUpgrade:true";

			EDIDataRegistry.Instance.CustomerServiceIncidentWorkItemCompletedEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "IM12345";
			incident.IM_Description = "Test Incident with 'tag' <b>!";
			incident.IM_Category = "";
			incident.IM_Product = "ENT";

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact incidentContact = client.Contacts.AddNew();
			incidentContact.OC_ContactName = "IT Support";
			incidentContact.OC_Email = "itsupport@someclient.com";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = incidentContact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			AssertEquals(false, incident.NeedUpgrade);
			Factory.Save();

			var sender = new SimpleEmailSenderForTest();
			SupportIncidentEmail email = new LocalSystemChangeIncidentCustomerNotifier(incident, sender).CreateWorkItemCompletedNotificationOrHandleFailure();
			AssertNotNull(email);
			AssertEquals(incidentContact, email.Contact);

#pragma warning disable CS0618 // Obsolete set by constructor
			AssertEquals(client, email.Client);
#pragma warning restore CS0618 // Obsolete set by constructor			
			AssertEquals("Incident IM12345 resolved: Test Incident with 'tag' <b>! - NeedUpgrade:false", email.Subject);
			AssertEquals(SupportIncident.SupportDisplayName, email.FromDisplayName);
			AssertEquals(SupportIncident.SupportEmailAddress, email.FromEmailAddress);
			AssertEquals("Development Work Completed email sent to contact", email.PublicLogComment);
			AssertContains("rectify this problem has now been completed.", email.Body);
			AssertContains("Test Incident with &#39;tag&#39; &lt;b&gt;!", email.Body);
			AssertHeader(email);
		}

		public void TestWorkItemCompletedEmailAutomaticallySent_Defect()
		{
			AssertWorkItemCompletedEmailAutomaticallySentByStage(SupportIncidentCategoriesList.Codes.Defect, "Development Work Completed email sent to contact");
		}

		public void TestWorkItemCompletedEmailAutomaticallySent_FeatureRequest()
		{
			AssertWorkItemCompletedEmailAutomaticallySentByStage(SupportIncidentCategoriesList.Codes.FeatureRequest, "Development Work Completed email sent to contact");
		}

		public void TestWorkItemCompletedEmailAutomaticallySent_ContentDevelopment()
		{
			var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var templatePair = collection.AddNew();
			templatePair.Code = SupportIncidentCategoriesList.Codes.ContentDevelopment;
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Incident (*IncidentNumber*) resolved: (*Summary*)";
			templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = "Incident (*IncidentNumber*) resolved: (*Summary*)";
			EDIDataRegistry.Instance.CustomerServiceIncidentWorkItemCompletedEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertWorkItemCompletedEmailAutomaticallySentByStage(SupportIncidentCategoriesList.Codes.ContentDevelopment, "Content Development Work Completed email sent to contact");
		}

		void AssertWorkItemCompletedEmailAutomaticallySentByStage(ZString escalationStage, ZString logText)
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Test User";
			staff.GS_EmailAddress = "test@edi.com.au";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "blah blah lola lola";
			org.OH_RL_NKClosestPort = "AUSYD";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Email = "contact@edi.com.au";

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "IM0123";
			incident.IM_Product = "ENT";
			incident.IM_Description = "Test Incident";
			incident.Escalate(escalationStage, "");

			incident.IM_GS_NKAssignedToCurrent = staff.GS_Code;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.Assigned;

			var workItem = incident.RelatedWorkItems.AddNew();

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var workItemTask = workItem.WorkflowItems.AddNew();
			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			Factory.Save();

			AssertEquals("No contact nominated", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.Assigned;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			Factory.Save();

			var messageList = incident.EConversation.GetTimeOrderedMessages();
		
			if (escalationStage == SupportIncidentCategoriesList.Codes.ContentDevelopment)
			{
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals(false, messageList.Any(msg => msg.Body == logText));
			}
			else
			{
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("contact@edi.com.au", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
				AssertEquals("Incident IM0123 resolved: Test Incident", Env.OutgoingMailManager.EmailsCreated[0].Subject);
				AssertEquals(true, messageList.Any(msg => msg.Body == logText));
			}
		}

		public void TestWorkItemCompletedEmailAutomaticallySent_FromWorkItem()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "IM12345";
			incident.IM_Description = "Test Incident";
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact incidentContact = client.Contacts.AddNew();
			incidentContact.OC_ContactName = "IT Support";
			incidentContact.OC_Email = "itsupport@someclient.com";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = incidentContact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = IncidentMainLookups.Status.Open;
			NewWorkItem workItem = incident.RelatedWorkItems.AddNew();
			ProcessTask workItemTask = workItem.WorkflowItems.AddNew();
			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			workItemTask.P9_GS_NKAssignedStaffMember = "C";
			Factory.Save();
			AssertEquals("Precondition", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("itsupport@someclient.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			AssertEquals("Incident IM12345 resolved: Test Incident", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Development Work Completed email sent to contact"));
		}

		public void TestWorkItemCompletedEmailNotSent_FromWorkItem_WhenInStayIndependentState()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "IM12345";
			incident.IM_Description = "Test Incident";
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var incidentContact = client.Contacts.AddNew();
			incidentContact.OC_ContactName = "IT Support";
			incidentContact.OC_Email = "itsupport@someclient.com";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = incidentContact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = IncidentMainLookups.Status.Open;
			var workItem = incident.RelatedWorkItems.AddNew();
			var workItemTask = workItem.WorkflowItems.AddNew();
			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			workItemTask.P9_GS_NKAssignedStaffMember = "C";
			Factory.Save();
			AssertEquals("Precondition", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.ContentDevelopment;
			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			Assert(incident.ShouldStayIndependent);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(false, messageList.Any(msg => msg.Body == "Development Work Completed email sent to contact"));
		}

		public void TestWorkItemCompletedEmailAutomaticallySent_OnlySendOnce()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "IM12345";
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact incidentContact = client.Contacts.AddNew();
			incidentContact.OC_ContactName = "IT Support";
			incidentContact.OC_Email = "itsupport@someclient.com";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = incidentContact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = IncidentMainLookups.Status.Open;

			var workItem = incident.RelatedWorkItems.AddNew();
			var workItemTask = workItem.WorkflowItems.AddNew();

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_Status = IncidentMainLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			Factory.Save();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Development Work Completed email sent to contact"));

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			incident.IM_Status = IncidentMainLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			Factory.Save();
			AssertEquals("Email was sent previously", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestWorkItemCompletedEmailAutomaticallySent_CannotSendToClient()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "MEH";
			staff.GS_EmailAddress = "meh@cargowise.com";

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "IM12345";
			incident.IM_GS_NKAssignedToCurrent = staff.GS_Code;
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact incidentContact = client.Contacts.AddNew();
			incidentContact.OC_ContactName = "IT Support";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = incidentContact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = IncidentMainLookups.Status.Open;

			var workItem = incident.RelatedWorkItems.AddNew();
			var workItemTask = workItem.WorkflowItems.AddNew();
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_Status = IncidentMainLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			Factory.Save();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Development Work Completed email for IM12345 could not be sent to the client", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals("Please notify the client manually.", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEquals("meh@cargowise.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);

			EDIDataRegistry.Instance.CustomerServiceIncidentWorkItemCompletedEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionIncidentEmailTemplatePairCollection());
			incidentContact.OC_Email = "contact@org.com";
			incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = incidentContact.PK;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			workItem = incident.RelatedWorkItems.AddNew();
			workItemTask = workItem.WorkflowItems.AddNew();
			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_Status = IncidentMainLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Completed;
			Factory.Save();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Where(e => !e.Body.Contains("Work completed, no upgrade will be sent")).Count());

			var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			AssertEquals(true, loadedIncident.EConversation.GetTimeOrderedMessages().Any(msg => msg.Body == "No work item completed notification email sent because no template has been setup"));
		}

		public void TestWorkItemCompletedEmailAutomaticallySent_WhenInternalIncidentWithNoRelatedWorkItems()
		{
			var licence = Factory.NewWithValidTestData<LicenceHeader>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var company = Factory.NewWithValidTestData<LicenceCompany>();
			licence.LA_LC = company.PK;
			company.LC_LE = enterprise.PK;
			Factory.Save();

			var settings = new InternalIncidentLicenceSettings();
			var key = new LicenceEnterpriseKey();
			key.LE_PK = enterprise.PK;
			settings.LicenceEnterpriseKeys.Add(key);
			settings.EdiProd_LicencePK = licence.PK;
			settings.UAT_ALP_LicencePK = licence.PK;
			settings.UAT_DPR_LicencePK = licence.PK;
			settings.UAT_GPC_LicencePK = licence.PK;
			settings.UAT_GPR_LicencePK = licence.PK;
			settings.UAT_STD_LicencePK = licence.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var featureRequest = Factory.New<SupportIncident>();
			featureRequest.SetupForInternalReportedIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, "", licence);

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var contact = client.Contacts.AddNew();
			contact.OC_ContactName = "IT Support";
			contact.OC_Email = "itsupport@someclient.com";
			featureRequest.IM_OH_Client = client.PK;
			featureRequest.IM_OC_Contact = contact.PK;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			featureRequest.IM_Status = IncidentMainLookups.Status.Closed;
			featureRequest.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Completed;
			Factory.Save();

			AssertEquals("There should be no email because no work item attached", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#endregion

		public void TestIncidentNotificationEmail()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Fullname with a 'tag' <b>?!";
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Email = "zubin.appoo@edi.com.au";

			var incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Description = "Help me";
			incident.DetailNoteText = "Don't know what i'm doing";
			AssertEquals("Precondition: No email should be sent yet.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			GlbStaff.CurrentUser.GS_FullName = "V. Klichko";
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();

			string expectedSubject = "Notification of Incident: " + incident.IM_IncidentNumber + " - Help me";

			var emails = sender.DequeueClientEmails();
			AssertEquals("Email should have been sent.", 1, emails.Length);

			var email = emails[0];
			AssertNotNull("Email should not be null.", email);
			AssertEquals("Email.FromDisplayName", SupportIncident.SupportDisplayName, email.FromDisplayName);
			AssertEquals("Email.FromEmailAddress", SupportIncident.SupportEmailAddress, email.FromEmailAddress);
			AssertEquals("Email.ToEmailAddress", "zubin.appoo@edi.com.au", email.ToEmailAddress);
			AssertEquals("Email.Subject", expectedSubject, email.Subject);
			Assert(!email.UseCurrentUsersNameAndTitle);
			Assert(!email.UseCurrentUsersEmailAddress);
			AssertHeader(email);

			//Using default registry notification email body template
			AssertContains("Email.Body", "We have received your enquiry regarding the following problem and will contact you soon.", email.Body);
			AssertContains("Email.Body", "Don&#39;t know what i&#39;m doing", email.Body);
			AssertContains("Email.Body", "Your Incident Number is: " + incident.IM_IncidentNumber, email.Body);
			AssertContains("Email.Body", "We are also confirming your contact details are:", email.Body);
			AssertContains("Email.Body", "Name: Fullname with a &#39;tag&#39; &lt;b&gt;?!", email.Body);
			AssertContains("Email.Body", "Phone: ", email.Body);
			AssertContains("Email.Body", "If your contact details are incorrect, or if you require further clarification regarding this issue please let us know by directly replying to this email.", email.Body);
			AssertContains("Notification email sent to contact", email.PublicLogComment);

			Factory.Save();
			AssertEquals("No more emails.", 0, sender.Emails.Count);

			//Custom defined email body template
			string emailBody = @"Hey (*ClientName*)-(*ContactName*) can you please wait for a couple of years since (*CurrentDate*),
				while we start analysing your incident - (*Summary*) : (*DetailedDescription*).";
			var templatePairCollection = new CodeDescriptionIncidentEmailTemplatePairCollection();
			var template = templatePairCollection.AddNew();
			template.Code = "AAA";
			template.Description = (NoResString)"AAA Desc";
			template.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "";
			template.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = emailBody;
			template.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = "";
			template.EmailTemplates.ERequestV2EmailTemplate.EmailBody = emailBody;
			EDIDataRegistry.Instance.CustomerServiceNotificationEmailTemplateRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templatePairCollection);
			SupportIncident incident2 = Factory.New<SupportIncident>();
			incident2.IM_OH_Client = org.PK;
			incident2.IM_OC_Contact = contact.PK;
			incident2.IM_Description = "test summary";
			incident2.DetailNoteText = "test detailed description";
			sender = new DummySenderForTest();
			incident2.CustomerNotifier = new StaffChangeNotifierForTest(incident2, sender);
			Factory.Save();
			SupportIncidentEmail email2 = sender.Emails[0];
			AssertContains("Email.Body", "Hey Fullname with a &#39;tag&#39; &lt;b&gt;?!-Zubin Appoo can you please wait for a couple of years since " + ZDateTime.Today.ToShortDateString() + ",", email2.Body);
			AssertContains("Email.Body", "while we start analysing your incident - test summary : test detailed description.", email2.Body);
		}

		public void TestIncidentResolvedEmailNotification()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var incident = Factory.New<SupportIncident>();
				incident.IM_Category = "SUP";

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "contact@org.com";
				incident.IM_OH_Client = org.PK;
				incident.IM_OC_Contact = contact.PK;

				Factory.Save();
				var sender = new DummySenderForTest();
				incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
				sender.Emails.Clear();

				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
				Factory.Save();
				AssertEquals("Closed in support but the disposition is in exclusion list", 0, sender.Emails.Count);

				incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
				Factory.Save();

				incident.CloseIncident(resolvedCode, "Comment");
				AssertEquals("Disposition should now be set to resolved", DispositionList.Constants.Closed.Resolved, incident.IM_ResolutionCode);

				Factory.Save();
				var clientEmails = sender.Emails.ToArray();
				sender.Emails.Clear();
				AssertEquals("Client email should have been queued.", 1, clientEmails.Length);

				var clientEmail = clientEmails[0];
				AssertContains("clientEmail.Subject", "has been Resolved with reason: ZZZ Description.", clientEmail.Subject);
				AssertContains("clientEmail.Body", "has been Resolved with a disposition of Resolved", clientEmail.Body);
				AssertContains("Support Incident Resolved with reason: ZZZ Description email sent to contact", clientEmail.PublicLogComment);
				AssertHeader(clientEmail);

				var footerHyperlinks = clientEmail.GetFooterHyperlink();
				var token = Factory.Load<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.IncidentEmailActionLink)).Single();
				AssertEquals(incident.PK, token.SAT_ParentId);
				AssertEquals(IncidentMainSchema.Constants.Prefix, token.SAT_ParentTableCode);
				AssertEquals(1, token.SAT_RemainingUseCount);
				AssertEquals(false, token.SAT_IsPermanentToken);

				AssertContains(SupportIncidentEmailBodyGeneralControls.ConfirmResolvedButtonID, footerHyperlinks);
				AssertContains(token.SAT_Token, footerHyperlinks);
			}
		}

		public void TestIncidentClosedEmailNotification()
		{
			#region Setup

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "blah blah lola lola";
			org.OH_RL_NKClosestPort = "AUSYD";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Email = "zubin.appoo@edi.com.au";

			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "MYGROUP";
			group.GG_Desc = "Some Group iN the WOrld";
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Maaaah baah haaah Lola";
			staff.GS_EmailAddress = "NewUser@NewDomain.com";

			GlbGroupLink link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;
			link.GK_MembershipType = "MGR";

			GlbGroup group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "MYGROU2";
			group2.GG_Desc = "Some Other Group";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "LOLA";
			staff2.GS_EmailAddress = "lola@zappoo.com";

			GlbGroupLink link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = group2.PK;
			link2.GK_GS = staff2.PK;
			link2.GK_MembershipType = "MGR";

			GlbGroup group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Code = "MYGROU3";
			group3.GG_Desc = "Some third group";
			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_FullName = "ZIPPY";
			staff3.GS_EmailAddress = "zip@yahoo.com";

			GlbGroupLink link3 = Factory.New<GlbGroupLink>();
			link3.GK_GG = group3.PK;
			link3.GK_GS = staff3.PK;
			link3.GK_MembershipType = "MGR";

			Factory.Save();

			EDIDataRegistry.Instance.IncidentInstallationsGroupENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group2.PK.ToGuid());

			var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));

			var templatePair = collection.AddNew();
			templatePair.Code = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Self Resolved Email Subject";
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Self Resolved Email Body";

			templatePair = collection.AddNew();
			templatePair.Code = SupportIncidentLookups.DispositionList.Constants.Closed.DuplicateIncident;
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Duplicate Incident Email Subject";
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Duplicate Incident Email Body";

			templatePair = collection.AddNew();
			templatePair.Code = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Other Incident Email Subject";
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Other Incident Email Body";

			EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			AssertEquals("Precondition: No emails sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			#endregion

			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);

			Factory.Save();
			AssertEquals("Precondition: Only 1 email - the notification email will have been sent.", 1, sender.Emails.Count);
			sender.Emails.Clear();

			// Self Resolved
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Status = "CLS";
			incident.IM_ResolutionCode = "SRS";
			incident.IM_IncidentNumber = "CS111222";
			incident.IM_Priority = "CR2";
			incident.IM_Description = "Get in my belly";
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			GlbStaff.CurrentUser.GS_FullName = "Sergey Gordok";
			Factory.Save();

			SupportIncidentEmail[] clientEmails = sender.Emails.ToArray();
			sender.Emails.Clear();
			AssertEquals("Client email should have been queued.", 1, clientEmails.Length);

			SupportIncidentEmail clientEmail = clientEmails[0];
			AssertNotNull("clientEmail should not be null.", clientEmail);
			AssertEquals("clientEmail.FromDisplayName", SupportIncident.SupportDisplayName, clientEmail.FromDisplayName);
			AssertEquals("clientEmail.FromEmailAddress", SupportIncident.SupportEmailAddress, clientEmail.FromEmailAddress);
			AssertEquals("clientEmail.ToEmailAddress", "zubin.appoo@edi.com.au", clientEmail.ToEmailAddress);
			AssertEquals("clientEmail.Subject", "Self Resolved Email Subject", clientEmail.Subject);
			AssertEquals("clientEmail.Body", "Self Resolved Email Body", clientEmail.Body);
			AssertContains("Support Incident Closed with reason: Self Resolved email sent to contact", clientEmail.PublicLogComment);
			AssertHeader(clientEmail);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Factory.Save();
			AssertEquals("No more emails.", 0, sender.Emails.Count);
			AssertEquals("No more emails.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			// Duplicate Incident
			incident.IM_Status = "OPN";
			incident.IM_ResolutionCode = "ADD";
			Factory.Save();

			incident.IM_Status = "CLS";
			incident.IM_ResolutionCode = "DUP";
			Factory.Save();

			clientEmails = sender.Emails.ToArray();
			AssertEquals("Client email should have been queued.", 1, clientEmails.Length);
			AssertEquals("Staff assignment notification email should have been sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			clientEmail = clientEmails[0];
			AssertNotNull("clientEmail should not be null.", clientEmail);
			AssertEquals("clientEmail.FromDisplayName", SupportIncident.SupportDisplayName, clientEmail.FromDisplayName);
			AssertEquals("clientEmail.FromEmailAddress", SupportIncident.SupportEmailAddress, clientEmail.FromEmailAddress);
			AssertEquals("clientEmail.ToEmailAddress", "zubin.appoo@edi.com.au", clientEmail.ToEmailAddress);
			AssertEquals("clientEmail.Subject", "Duplicate Incident Email Subject", clientEmail.Subject);
			AssertContains("clientEmail.Body", "Duplicate Incident Email Body", clientEmail.Body);
			AssertContains("Support Incident Closed with reason: Duplicate Incident email sent to contact", clientEmail.PublicLogComment);
			AssertHeader(clientEmail);

			// No Current Support Contract
			incident.IM_Status = "OPN";
			incident.IM_ResolutionCode = "ADD";
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			sender.Emails.Clear();
			incident.IM_Status = "CLS";
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.NoSupportContract;

			GlbStaff.CurrentUser.GS_EmailAddress = "current@edi.com.au";
			Factory.Save();
			AssertEquals("Email should NOT be sent for this disposition.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Email should NOT be sent for this disposition.", 0, sender.Emails.Count);

			//ClosedAsCustomerServiceRequest
			incident.IM_Status = "OPN";
			incident.IM_ResolutionCode = "ADD";
			Factory.Save();

			sender.Emails.Clear();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
			incident.IM_Priority = "CR9";
			incident.IM_Status = "CLS";
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			Factory.Save();

			clientEmail = sender.Emails.Single();
			AssertEquals("clientEmail.FromDisplayName", SupportIncident.SupportDisplayName, clientEmail.FromDisplayName);
			AssertEquals("clientEmail.FromEmailAddress", SupportIncident.SupportEmailAddress, clientEmail.FromEmailAddress);
			AssertEquals("clientEmail.ToEmailAddress", "zubin.appoo@edi.com.au", clientEmail.ToEmailAddress);
			AssertEquals("clientEmail.Subject", "Other Incident Email Subject", clientEmail.Subject);
			AssertContains("clientEmail.Body", "Other Incident Email Body", clientEmail.Body);
			AssertHeader(clientEmail);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			incident.IM_Status = "OPN";
			incident.IM_ResolutionCode = "ADD";
			incident.IM_Priority = "CR1";
			Factory.Save();

			sender.Emails.Clear();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
			incident.IM_Priority = "CR9";
			incident.IM_Status = "CLS";
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			Factory.Save();

			AssertEquals(true, sender.Emails.Any());
			AssertEquals("Email for Criticality update", "Notification of Incident Criticality Changed: CS111222 - Get in my belly", sender.Emails[0].Subject);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestIncidentClosedEmailNotification_ExcludingDisposition()
		{
			var templates = EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.Value;
			var templatePair = templates.AddNew();
			templatePair.Code = SupportIncidentLookups.DispositionList.Constants.FeatureAccepted;
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Auto Closed Feature Request";
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "This incident is auto closed.";
			EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templates);

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = client.Contacts.AddNew();
			contact.OC_ContactName = "IT Support";
			contact.OC_Email = "itsupport@test.com";

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();

			sender.Emails.Clear();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();
			AssertEquals("Closed in support but the disposition is in exclusion list", 0, sender.Emails.Count);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			Factory.Save();
			AssertEquals("Closed in support and the disposition is not in exclusion list", 1, sender.DequeueClientEmails().Length);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			Factory.Save();
			AssertEquals("Closed as completed", 1, sender.DequeueClientEmails().Length);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, "");
			Factory.Save();
			AssertEquals("Closed as auto closed feature request", 1, sender.DequeueClientEmails().Length);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			var task = incident.WorkflowItems.AddNew();
			task.P9_Sequence = 1;
			task.P9_Type = "INV";
			task.P9_Description = "investigation";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("Closed but the disposition is in exclusion list", 0, sender.Emails.Count);
		}

		public void TestIncidentClosedEmailNotification_ForComplianceRequest()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = client.Contacts.AddNew();
			contact.OC_ContactName = "IT Support";
			contact.OC_Email = "itsupport@test.com";

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();

			sender.Emails.Clear();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();
			AssertEquals("Closed in support but the disposition is in exclusion list", 0, sender.Emails.Count);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			Factory.Save();
			AssertEquals("Closed in support and the disposition is not in exclusion list", 1, sender.DequeueClientEmails().Length);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			Factory.Save();
			AssertEquals("Closed as completed", 1, sender.DequeueClientEmails().Length);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			incident.Escalate(SupportIncidentCategoriesList.Codes.ComplianceRequirement, "");
			Factory.Save();
			sender.Emails.Clear();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, "");
			Factory.Save();
			AssertEquals(incident.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AssertEquals("UPO should not trigger notification", 0, sender.DequeueClientEmails().Length);
		}

		public void TestIncidentClosedEmailNotification_ForContentDevelopment()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = client.Contacts.AddNew();
			contact.OC_ContactName = "IT Support";
			contact.OC_Email = "itsupport@test.com";

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.ContentDevelopment;
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();

			sender.Emails.Clear();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();
			AssertEquals("Closed in support but the disposition is in exclusion list", 0, sender.Emails.Count);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			Factory.Save();
			AssertEquals("Closed in support and the disposition is not in exclusion list", 1, sender.DequeueClientEmails().Length);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			Factory.Save();
			AssertEquals("Closed as completed", 1, sender.DequeueClientEmails().Length);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			incident.Escalate(SupportIncidentCategoriesList.Codes.ContentDevelopment, "");
			Factory.Save();
			sender.Emails.Clear();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, "");
			Factory.Save();
			AssertEquals(incident.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AssertEquals("UPO should not trigger notification", 0, sender.DequeueClientEmails().Length);
		}

		public void TestIncidentClosedEmailNotification_ForDefect()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = client.Contacts.AddNew();
			contact.OC_ContactName = "IT Support";
			contact.OC_Email = "itsupport@test.com";

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();

			sender.Emails.Clear();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();
			AssertEquals("Closed in support but the disposition is in exclusion list", 0, sender.Emails.Count);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			Factory.Save();
			AssertEquals("Closed in support and the disposition is not in exclusion list", 1, sender.DequeueClientEmails().Length);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			Factory.Save();
			AssertEquals("Closed as completed", 1, sender.DequeueClientEmails().Length);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();
			sender.Emails.Clear();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, "");
			Factory.Save();
			AssertEquals(incident.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AssertEquals("UPO should not trigger notification", 0, sender.DequeueClientEmails().Length);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			sender.Emails.Clear();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed, "");
			Factory.Save();
			AssertEquals(incident.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed);
			AssertEquals("UDO should not trigger notification", 0, sender.DequeueClientEmails().Length);
		}

		public void TestIncidentClosedEmailNotification_ForFeatureAccepted()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "joe@test.org";
			staff.GS_FullName = "Joe Staff";
			staff.GS_Code = "JOE";
			staff.GS_Title = "Product";

			var internalLic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			internalLic.Database.LicEnterprise.LE_IsInternal = true;

			var contact = internalLic.Company.Header.Contacts.AddNew();
			contact.OC_ContactName = staff.GS_FullName;
			contact.OC_Email = staff.GS_EmailAddress;

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = internalLic.Company.Header.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var sender = new WebRequestNotificationSender();
			incident.CustomerNotifier = new CustomerChangeIncidentCustomerNotifier(incident, sender);
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var userContext = new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK, null, true, incident.Factory);
			using (Env.SetTemporaryUserContext(userContext))
			{
				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, "");
				Factory.Save();
			}

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("Email Signature should use default user name", $"</p>\r\n                  <p>\r\n                      <b>\r\n                          {SupportIncident.SupportDisplayName}", email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestIncidentClosedEmailNotification_ForAlreadyClosedIncident()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var contact = client.Contacts.AddNew();
			contact.OC_ContactName = "IT Support";
			contact.OC_Email = "itsupport@test.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();

			sender.Emails.Clear();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			Factory.Save();
			AssertEquals("Precondition", true, incident.IsClosedDisposition(incident.IM_ResolutionCode));
			AssertEquals("Precondition: Closed in support and the disposition is not in exclusion list", 1, sender.DequeueClientEmails().Length);

			sender.Emails.Clear();

			var task1 = incident.WorkflowItems.Tasks.AddNew();
			task1.P9_Sequence = 10;
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			AssertEquals("Precondition", true, incident.IsClosedDisposition(incident.IM_ResolutionCode));
			AssertEquals("Precondition: Adding a new task should not trigger notification", 0, sender.DequeueClientEmails().Length);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals("Precondition", true, incident.IsClosedDisposition(incident.IM_ResolutionCode));
			AssertEquals("Closing the task should not trigger notification since it was already resolved/closed", 0, sender.DequeueClientEmails().Length);
		}

		public void TestIncidentClosedEmailNotification_ShouldSendIfClosingDispositionButAddingTask()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var contact = client.Contacts.AddNew();
			contact.OC_ContactName = "IT Support";
			contact.OC_Email = "itsupport@test.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();

			sender.Emails.Clear();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			var task1 = incident.WorkflowItems.Tasks.AddNew();
			task1.P9_Sequence = 10;
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals("Precondition: Disposition should be closed", true, incident.IsClosedDisposition(incident.IM_ResolutionCode));
			AssertNotEquals("Precondition: Status should not be closed", SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Precondition: Notification should be sent since disposition has been updated to closed", 1, sender.DequeueClientEmails().Length);

			sender.Emails.Clear();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals("Precondition", true, incident.IsClosedDisposition(incident.IM_ResolutionCode));
			AssertEquals("Closing the task should not trigger notification since it was already closed", 0, sender.DequeueClientEmails().Length);
		}

		public void TestIncidentClosedEmail_ResolutionCommentAppearsOnlyOnce()
		{
			var templates = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var defaultTemplate = templates.AddNew();
			defaultTemplate.Code = "DFT";
			defaultTemplate.Description = (NoResString)"Default Close Notification Email Template";
			defaultTemplate.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Closed Feature Request";
			defaultTemplate.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Incident (*IncidentNumber*) - (*Summary*) - has been closed: (*ResolutionCommentText*).";
			defaultTemplate.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = defaultTemplate.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject;
			defaultTemplate.EmailTemplates.ERequestV2EmailTemplate.EmailBody = defaultTemplate.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody;
			EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templates);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "some comment from user");
			Factory.Save();

			var emails = sender.DequeueClientEmails();
			AssertEquals(1, emails.Length);
			var email = emails[0];
			AssertEquals(1, email.Body.Occurrences("some comment from user"));
			AssertEquals("Messages should be published even though an email contains them already", 2, sender.PublishedMessages.Count);
		}

		public void TestIncidentClosedEmail_ResolutionNoteAppearsOnlyOnce()
		{
			var templates = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var defaultTemplate = templates.AddNew();
			defaultTemplate.Code = "DFT";
			defaultTemplate.Description = (NoResString)"Default Close Notification Email Template";
			defaultTemplate.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Closed Feature Request";
			defaultTemplate.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Incident (*IncidentNumber*) - (*Summary*) - has been closed: (*ResolutionNoteText*).";
			defaultTemplate.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = defaultTemplate.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject;
			defaultTemplate.EmailTemplates.ERequestV2EmailTemplate.EmailBody = defaultTemplate.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody;
			EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templates);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "some comment from user");
			Factory.Save();

			var emails = sender.DequeueClientEmails();
			AssertEquals(1, emails.Length);
			var email = emails[0];
			AssertEquals(1, email.Body.Occurrences("some comment from user"));
			AssertEquals("Messages should be published even though an email contains them already", 2, sender.PublishedMessages.Count);
		}

		public void TestIncidentClosedEmail_WithReopenRuleNev()
		{
			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = "CR4";
			productResolutionAndClosureBehaviour.Code = "ENT";
			productResolutionAndClosureBehaviour.ClosedReopenRule = ResolutionAndClosureBehaviour.Constants.Code.NeverAllow;

			var tempalteCollection = EDIDataRegistry.Instance.CustomerServiceFinalClosureAutoReplyEmailTemplate.Value;
			var templatePair = tempalteCollection.FirstOrDefault() as CodeDescriptionIncidentEmailTemplatePair;
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Final Closure Auto Reply Email - LegacyAndERequestV1";
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Test Only EmailBody - LegacyAndERequestV1";
			templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = "Final Closure Auto Reply Email - ERequestV2EmailTemplate";
			templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = "Test Only EmailBody - ERequestV2EmailTemplate";

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			using (EDIDataRegistry.Instance.CustomerServiceFinalClosureAutoReplyEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempalteCollection))
			{
				var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
				incident.IM_Product = ProductTypes.Codes.Enterprise;
				incident.IM_Module = "AAA";
				incident.IM_Priority = "CR4";
				incident.ProductArea = "ARC";
				incident.IM_SourceModuleId = "SourceModule2";
				Factory.Save();

				Env.OutgoingMailManager.EmailsCreated.Clear();

				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
				var sender = new WebRequestNotificationSender();
				incident.CustomerNotifier = new CustomerChangeIncidentCustomerNotifier(incident, sender);
				Factory.Save();

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Final Closure Auto Reply Email - LegacyAndERequestV1", email.Subject);
			}
		}

		public void TestIncidentClosedEmail_WithReopenRuleAlw()
		{
			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = "CR4";
			productResolutionAndClosureBehaviour.Code = "ENT";
			productResolutionAndClosureBehaviour.ClosedReopenRule = ResolutionAndClosureBehaviour.Constants.Code.AlwaysAllow;

			var tempalteCollection = EDIDataRegistry.Instance.CustomerServiceFinalClosureAutoReplyEmailTemplate.Value;
			var templatePair = tempalteCollection.FirstOrDefault() as CodeDescriptionIncidentEmailTemplatePair;
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Final Closure Auto Reply Email - LegacyAndERequestV1";
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Test Only EmailBody - LegacyAndERequestV1";
			templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = "Final Closure Auto Reply Email - ERequestV2EmailTemplate";
			templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = "Test Only EmailBody - ERequestV2EmailTemplate";

			var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var templatePair2 = collection.AddNew();
			templatePair2.Code = "DFT";
			templatePair2.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Default Close Subject";
			templatePair2.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Default Close Body";
			EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			using (EDIDataRegistry.Instance.CustomerServiceFinalClosureAutoReplyEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempalteCollection))
			using (EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
				incident.IM_Product = ProductTypes.Codes.Enterprise;
				incident.IM_Module = "AAA";
				incident.IM_Priority = "CR4";
				incident.ProductArea = "ARC";
				incident.IM_SourceModuleId = "SourceModule2";
				Factory.Save();

				Env.OutgoingMailManager.EmailsCreated.Clear();

				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
				var sender = new WebRequestNotificationSender();
				incident.CustomerNotifier = new CustomerChangeIncidentCustomerNotifier(incident, sender);
				Factory.Save();

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Default Close Subject", email.Subject);
			}
		}

		public void TestNoEmailsWhenCreatedFromIssue()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			org.LicenceEnterpriseCode = "ORG";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact";
			contact.OC_Email = "contact@company.com";

			LicenceDatabase db = org.LicCompany.LicDatabases.AddNew();
			db.LD_OC_ContractInstallerOrInternalTechContact = contact.PK;

			ClientCompany clientCompany = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany.LCC_LD = db.PK;
			clientCompany.LCC_Code = org.LicCompany.LC_CompanyCode;

			Factory.Save();

			LicenceHeader licence = org.LicCompany.GetHeader(db);
			EdiHelpErrorLog issue = Factory.New<EdiHelpErrorLog>();
			NewWorkItem workItem = issue.RelatedWorkItems.AddNew();
			workItem.FillWithValidTestData();

			HelpErrorLogOccurrence occurrence = issue.Occurrences.AddNew();
			occurrence.HO_LD = db.PK;
			occurrence.HO_LCC = clientCompany.PK;

			var incident = issue.CreateIncidents(false)[0];
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();
			AssertEquals("RelatedIncidents.Count", 1, issue.RelatedIncidents.Count);

			AssertEquals("There should not be any emails.", 0, sender.DequeueClientEmails().Length);

			workItem.WorkflowItems.AddNew().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("There should not be any emails.", 0, sender.DequeueClientEmails().Length);
		}

		public void TestNoIncidentClosedEmail_WhenAllRelatedWorkItemsAreCancelled()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = client.Contacts.AddNew();
			contact.OC_Email = "contact@cargowise.com";
			contact.FillWithValidTestData();

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "MEH";
			staff.GS_EmailAddress = "meh@cargowise.com";

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "CS00001130";
			incident.IM_Description = "I'm an incident";
			incident.IM_GS_NKAssignedToCurrent = staff.GS_Code;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.IM_Status = SupportIncidentLookups.Status.Working;

			NewWorkItem workItem1 = incident.RelatedWorkItems.AddNew();
			workItem1.FillWithValidTestData();
			ProcessTask workItem1Task1 = workItem1.WorkflowItems.AddNew();
			ProcessTask workItem1Task2 = workItem1.WorkflowItems.AddNew();

			AssertEquals("Pre-condition", SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Pre-condition", SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			workItem1.Cancel();
			AssertEquals("Should still close defect.", SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Disposition as closed internal", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals("Disposition as closed internal", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, incident.IM_ClosureResolution);
			var sender = new DummySenderForTest();
			var notifier = new StaffChangeNotifierForTest(incident, sender);
			Assert("Should not create email", !notifier.ShouldSendWorkItemCompletedNotification);
			incident.CustomerNotifier = notifier;
			Factory.Save();
			Assert("Incident Closed notification email should not be sent for CANCELLED disposition", !Env.OutgoingMailManager.EmailsCreated.Exists(e => e.Subject.Contains("Incident: CS00001130 has been resolved")));
		}

		public void TestDefectWorkItemClosedNotificationEmail()
		{
			#region Test Data

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Defect Manager";
			staff.GS_EmailAddress = "defman@test.com";

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact incidentContact = client.Contacts.AddNew();
			incidentContact.OC_ContactName = "IT Support";
			incidentContact.OC_Email = "it@test.com";

			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.SetupForNewCreatedDefect();
			incident1.IM_IncidentNumber = "CS00005155";
			incident1.IM_Product = ProductTypes.Codes.Enterprise;
			incident1.IM_OH_Client = client.PK;
			incident1.IM_OC_Contact = incidentContact.PK;
			incident1.IM_GS_NKAssignedToCurrent = staff.GS_Code;

			NewWorkItem workItem1 = incident1.RelatedWorkItems.AddNew();
			workItem1.FillWithValidTestData();
			workItem1.WKI_Priority = ReleaseRings.Codes.STD;
			ProcessTask workItem1Task1 = workItem1.WorkflowItems.AddNew();
			workItem1Task1.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			ProcessTask workItem1Task2 = workItem1.WorkflowItems.AddNew();
			workItem1Task2.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.STD).CheckinTask;

			var sender = new DummySenderForTest();
			incident1.CustomerNotifier = new LocalChangeNotifierForTest(incident1, sender);
			Factory.Save();

			#endregion

			sender.Emails.Clear();
			workItem1Task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			workItem1Task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			((IWorkItemRelatedItem)incident1).OnRelatedWorkItemClosed(workItem1);

			Factory.Save();
			AssertEquals("Should create notification email", 1, sender.Emails.Count);
			var email = sender.DequeueClientEmails()[0];
			Assert("Incident Closed notification email should be sent", email.Subject.Contains("Incident CS00005155 resolved"));
			AssertHeader(email);

			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.SetupForNewCreatedDefect();
			incident3.IM_IncidentNumber = "CS00003089";
			incident3.IM_Product = ProductTypes.Codes.Enterprise;
			incident3.IM_OH_Client = client.PK;
			incident3.IM_OC_Contact = incidentContact.PK;
			incident3.IM_GS_NKAssignedToCurrent = staff.GS_Code;

			var sender3 = new DummySenderForTest();
			incident3.CustomerNotifier = new LocalChangeNotifierForTest(incident3, sender3);

			Factory.Save();

			sender.Emails.Clear();
			incident3.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, "");
			Factory.Save();
			AssertEquals("Should not create notification email", 0, sender.Emails.Count);
		}

		public void TestWorkItemCompletedEmailIsNotSentWhenLoadingRelatedWorkItems()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Status = SupportIncidentLookups.Status.Working;

			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "bob@builders.com";
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			NewWorkItem workItem = incident.RelatedWorkItems.AddNew();
			workItem.WorkflowItems.AddNew().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new LocalChangeNotifierForTest(incident, sender);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident = newFactory.Load<SupportIncident>(incident.PK);
			NewWorkItemRelatedCollectionView relatedWorkItems = loadedIncident.RelatedWorkItems; // Invoke the RelatedWorkItems getter;
			AssertNull(loadedIncident.CurrentCustomerNotiferForTest);
			AssertEquals("No new emails should be created.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSendWorkItemCreatedEmailToClient_NonSupport()
		{
			GlbStaff.CurrentUser.GS_FullName = "Ted Burhan";
			GlbStaff.CurrentUser.GS_EmailAddress = "ted.burhan@cargowise.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Email = "contact@edi.com.au";

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;

			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var emails = sender.DequeueClientEmails();
			AssertEquals(1, emails.Length);
			var email = emails[0];
			AssertEquals("Incident: " + incident.IM_IncidentNumber + " has been scheduled for development work.", email.Subject);
			AssertEquals(SupportIncident.SupportDisplayName, email.FromDisplayName);
			AssertEquals(SupportIncident.SupportEmailAddress, email.FromEmailAddress);
			Assert(!email.UseCurrentUsersNameAndTitle);
			Assert(!email.UseCurrentUsersEmailAddress);
			AssertEquals("contact@edi.com.au", email.ToEmailAddress);
			AssertContains("We will be in further contact when this work has been completed.", email.Body);
			AssertContains("Development Work Scheduled email sent to contact", email.PublicLogComment);
			AssertHeader(email);

			Factory.Save();
			AssertEquals("No more emails sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("No more emails sent", 0, sender.Emails.Count);
		}

		public void TestSendWorkItemCreatedEmailToClient_Support()
		{
			GlbStaff.CurrentUser.GS_FullName = "Ted Burhan";
			GlbStaff.CurrentUser.GS_EmailAddress = "ted.burhan@cargowise.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Email = "contact@edi.com.au";

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;

			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();
			var email = sender.DequeueClientEmails().FirstOrDefault(x => x.Subject == "Incident: " + incident.IM_IncidentNumber + " has been scheduled for development work.");
			AssertNull(email);
		}

		public void TestSendWorkItemCreatedEmailToClient_Template()
		{
			GlbStaff.CurrentUser.GS_FullName = "Ted Burhan";
			GlbStaff.CurrentUser.GS_EmailAddress = "ted.burhan@cargowise.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Email = "contact@edi.com.au";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Description = "Summary Test";
			incident.IM_Details = Encoding.UTF8.GetBytes("Test Details");
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;

			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var emails = sender.DequeueClientEmails();
			AssertEquals(1, emails.Length);
			var email = emails[0];
			AssertEquals("Incident: " + incident.IM_IncidentNumber + " has been scheduled for development work.", email.Subject);
			AssertEquals(SupportIncident.SupportDisplayName, email.FromDisplayName);
			AssertEquals(SupportIncident.SupportEmailAddress, email.FromEmailAddress);
			Assert(!email.UseCurrentUsersNameAndTitle);
			Assert(!email.UseCurrentUsersEmailAddress);
			AssertEquals("contact@edi.com.au", email.ToEmailAddress);

			AssertContains("Development Work Scheduled email sent to contact", email.PublicLogComment);

			var expectedBody = @"We wish to advise you that an urgent fix is underway to rectify the problem you were experiencing. We will be in further contact when this work has been completed.

If any details are incorrect or there are any concerns please let us know by directly replying to this email.

<b>Incident Details CS00000001 (Summary Test)</b>

<i></i>";

			AssertContains(expectedBody, email.Body);
			AssertHeader(email);

			Factory.Save();
			AssertEquals("No more emails sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("No more emails sent", 0, sender.Emails.Count);
		}

		public void TestEscalateIncident()
		{
			SupportIncident incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertNull(incident.AssignedToCurrent);
			sender.DequeueClientEmails();

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();
			var clientEmails = sender.DequeueClientEmails();
			AssertEquals(0, clientEmails.Length);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			incident.AssignToStaff(staff, "It is for you");
			Factory.Save();
			clientEmails = sender.DequeueClientEmails();
			AssertEquals(0, clientEmails.Length);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			Factory.Save();
			clientEmails = sender.DequeueClientEmails();
			AssertEquals(0, clientEmails.Length);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
		}

		public void TestSendQueuedEmails()
		{
			var template = EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.Value;
			template.EmailSubject = "Awaiting Email";
			EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			SupportIncident incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var sender = new EmailOnlyCustomerNotificationSender();
			var notifier = new StaffChangeNotifierForTest(incident, sender);
			incident.CustomerNotifier = notifier;

			incident.AddStaffMessageToCustomer("this message should be added to standard email");

			var standardEmail = SupportIncidentEmail.New(incident);
			standardEmail.Subject = "standard subject";
			standardEmail.Body = "standard body";

			var userEmail = SupportIncidentEmail.New(incident);
			userEmail.Subject = "user subject";
			userEmail.Body = "user body";
			userEmail.UserCanEdit = true;
			userEmail.InternalLogComment = "this will cause the incident to be modified and included in save";
			notifier.SendEmailOnIncidentSave(userEmail);

			notifier.StandardEmailForTest = standardEmail;
			notifier.CanPreviewForTest = true;
			Factory.Save();
			var emailsAfterSave = Env.OutgoingMailManager.EmailsCreated.ToArray();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			notifier.StandardEmailForTest = null;
			notifier.SendQueuedEmails();
			var emailsQueued = Env.OutgoingMailManager.EmailsCreated.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("emailsAfterSave", 2, emailsAfterSave.Length);
				AssertEquals("emailsQueued", 1, emailsQueued.Length);
			});

			CombineAssertions(() =>
			{
				Assert("Standard email subject", emailsAfterSave.Any(x => x.Subject == "standard subject"));
				Assert("New Message should be published", emailsAfterSave.Any(x => x.Subject == "Awaiting Email"));
				AssertEquals("email2.Subject", "user subject", emailsQueued[0].Subject);
				AssertContains("email1.Body has message", "this message should be added to standard email", emailsAfterSave.First(x => x.Subject == "standard subject").Body);
			});
		}

		public void TestCreateIncidentFollowUpERequestEmail()
		{
			var templates = EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.Value;
			var templateNEV = templates.Cast<CodeDescriptionIncidentEmailTemplatePair>().First(t => t.Code == "NEV");
			templateNEV.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Unable to reopen Incident Email";
			templateNEV.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "This incident cannot be reopened.";
			var templateDFT = templates.Cast<CodeDescriptionIncidentEmailTemplatePair>().First(t => t.Code == "DFT");
			templateDFT.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Close Incident Email";
			templateDFT.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Incident was closed.";

			using (EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, templates))
			{
				var collection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
				var templateEnt = collection.AddNew();
				templateEnt.ParentID = collection[0].PK;
				templateEnt.ClosedReopenRule = ResolutionAndClosureBehaviour.Constants.Code.NeverAllow;
				templateEnt.Code = ProductTypes.Codes.Enterprise;

				using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
				{
					var client = Factory.NewWithValidTestData<OrgHeader>();
					var contact = client.Contacts.AddNew();
					contact.OC_ContactName = "IT Support";
					contact.OC_Email = "itsupport@test.com";

					var incidentSph = Factory.NewWithValidTestData<SupportIncident>();
					incidentSph.IM_Product = ProductTypes.Codes.Sapphire;
					incidentSph.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
					incidentSph.IM_OH_Client = client.PK;
					incidentSph.IM_OC_Contact = contact.PK;

					var incidentEnt = Factory.NewWithValidTestData<SupportIncident>();
					incidentEnt.IM_Product = ProductTypes.Codes.Enterprise;
					incidentEnt.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
					incidentEnt.IM_OH_Client = client.PK;
					incidentEnt.IM_OC_Contact = contact.PK;
					incidentEnt.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Completed;

					var senderSph = new DummySenderForTest();
					incidentSph.CustomerNotifier = new StaffChangeNotifierForTest(incidentSph, senderSph);
					var senderEnt = new DummySenderForTest();
					incidentEnt.CustomerNotifier = new StaffChangeNotifierForTest(incidentEnt, senderEnt);
					Factory.Save();

					senderSph.Emails.Clear();
					senderEnt.Emails.Clear();

					incidentEnt.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
					Factory.Save();
					AssertEquals("Should load the reopen closed template", 1, senderEnt.Emails.Count);
					AssertEquals("Unable to reopen Incident Email", senderEnt.Emails[0].Subject);
					AssertEquals("This incident cannot be reopened.", senderEnt.Emails[0].Body);
					AssertHeader(senderEnt.Emails[0]);

					incidentSph.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
					Factory.Save();
					AssertEquals("Should load the default close template", 1, senderSph.Emails.Count);
					AssertEquals("Close Incident Email", senderSph.Emails[0].Subject);
					AssertEquals("Incident was closed.", senderSph.Emails[0].Body);
					AssertHeader(senderSph.Emails[0]);
				}
			}
		}

		public void TestCreateIncidentFollowUpERequestEmail_AddCreateFollowUpERequestButton()
		{
			var rootUrl = "https://localhost";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";

			var task = incident.WorkflowItems.AddNew();
			task.P9_Description = "Task 1";
			task.P9_Type = "UDF";
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			incident.AddStaffMessageToCustomer("Hello, I want to send this message");

			var (email, _) = SupportIncidentEmailQuickBuilder.CreateIncidentClosedEmail(incident, false, incident.IM_ResolutionCode, false, shouldUseFollowUpTemplate: true);

			email.Subject = "Queued client email subject";
			email.Body = "This is the queued client email body";

			var sender = new EmailOnlyCustomerNotificationSender();
			var notifier = new LocalChangeNotifierForTest(incident, sender);
			notifier.StandardEmailForTest = email;
			notifier.SendChanges();
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Queued client email subject", sentEmail.Subject);
			AssertContains("This is the queued client email body", sentEmail.Body);
			AssertContains("Hello, I want to send this message", sentEmail.Body);
			AssertContains(SupportIncidentEmailBodyGeneralControls.GetCreateFollowUpERequestButton(incident), sentEmail.Body);
			var expectedUrl = $"{rootUrl}/goto/FollowUpERequest?incidentNumber={incident.Request.INC_IncidentNumber}";
			AssertContains(expectedUrl, sentEmail.Body);
			AssertNotNull(sentEmail.Headers.TryGetValue(SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress, out var xAutoSuppressHead));
			AssertEquals(SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, xAutoSuppressHead);
		}

		public void TestSendCriticalityChangedEmailShouldNotSendEmptyNotificationEmail()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			Factory.Save();

			var emailParticipant = incident.EConversation.ExistingConversation.Participants.AddNew();
			emailParticipant.JCP_IsSubscribed = true;
			emailParticipant.JCP_EmailAddress = "archie@test.com";
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR5_Training;
			var sender = new EmailOnlyCustomerNotificationSender();
			var notifier = new StaffChangeNotifierForTest(incident, sender);
			notifier.SendChanges();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var criticalityChangedEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("Notification of Incident Criticality Changed", criticalityChangedEmail.Subject);
		}

		public void TestSendWorkItemCreatedEmailShouldNotSendEmptyNotificationEmail()
		{
			GlbStaff.CurrentUser.GS_FullName = "Archie Test";
			GlbStaff.CurrentUser.GS_EmailAddress = "archie.test@cw1.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "mario";
			contact.OC_Email = "mario@edi.com.au";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Description = "Summary Test";
			incident.IM_Details = Encoding.UTF8.GetBytes("Test Details");
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;
			Factory.Save();

			var emailParticipant = incident.EConversation.ExistingConversation.Participants.AddNew();
			emailParticipant.JCP_IsSubscribed = true;
			emailParticipant.JCP_EmailAddress = "archie@test.com";
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			var sender = new SimpleEmailSenderForTest();
			var notifier = new StaffChangeNotifierForTest(incident, sender);
			notifier.SendChanges();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var workItemCreatedEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("has been scheduled for development work", workItemCreatedEmail.Subject);
		}

		public void TestSendWorkItemCompletedEmailShouldNotSendEmptyNotificationEmail()
		{
			GlbStaff.CurrentUser.GS_FullName = "Archie Test";
			GlbStaff.CurrentUser.GS_EmailAddress = "archie.test@cw1.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "mario";
			contact.OC_Email = "mario@edi.com.au";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Description = "Summary Test";
			incident.IM_Details = Encoding.UTF8.GetBytes("Test Details");
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;

			var workItem = incident.RelatedWorkItems.AddNew();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			var emailParticipant = incident.EConversation.ExistingConversation.Participants.AddNew();
			emailParticipant.JCP_IsSubscribed = true;
			emailParticipant.JCP_EmailAddress = "archie@test.com";
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Completed;
			var sender = new SimpleEmailSenderForTest();
			var notifier = new StaffChangeNotifierForTest(incident, sender);
			notifier.SendChanges();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var workItemCompletedEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains($"Incident {incident.IM_IncidentNumber} resolved: {incident.IM_Description}", workItemCompletedEmail.Subject);
		}

		public void TestCriticalityEmailPopupOnlyOnSave()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var sender = new DummySenderForTest();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);
			var contact = Factory.NewWithValidTestData<OrgContact>();
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			Factory.Save();

			incident.IM_Priority = "CR3";
			AssertEquals("Should only have an email for incident creation", sender.Emails.Count, 1);
			AssertContains("Notification of Incident", sender.Emails[0].Subject);
			incident.IM_Priority = "CR2";
			Factory.Save();

			AssertEquals("Should have an email for incident creation and criticality change", 2, sender.Emails.Count);
			AssertContains("Notification of Incident Criticality Changed", sender.Emails[1].Subject);
			AssertContains("Criticality changed from CR4 to CR2", sender.Emails[1].Body);
		}

		void AssertHeader(SupportIncidentEmail email)
		{
			AssertEquals("XAutoResponseSuppressHeader" , SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, email.XAutoResponseSuppressHeaderValue);
		}

		protected override void SetUp()
		{
			ReleaseBuildContentForLegacyTest.Enable();
			base.SetUp();
		}

		class SimpleEmailSenderForTest : IIncidentCustomerNotificationSender
		{
			public void AddHyperlinks(SupportIncidentEmail email)
			{
			}

			public void SendChanges(SupportIncident incident, SupportIncidentEmail email)
			{
				if (email != null)
				{
					email.SendEmail();
				}
			}

			public void SendEmail(SupportIncidentEmail email)
			{
				email.SendEmail();
			}
		}

		class DummySenderForTest : IIncidentCustomerNotificationSender
		{
			public SupportIncidentEmail[] DequeueClientEmails()
			{
				var result = Emails.ToArray();
				Emails.Clear();
				return result;
			}

			public List<SupportIncidentEmail> Emails = new List<SupportIncidentEmail>();
			public List<JobConversationMessage> PublishedMessages = new List<JobConversationMessage>();

			public void SendChanges(SupportIncident incident, SupportIncidentEmail email)
			{
				if (email != null)
				{
					Emails.Add(email);
				}

				PublishedMessages.AddRange(incident.EConversation.GetNewLocalPublishedMessages());
			}

			public void AddHyperlinks(SupportIncidentEmail email)
			{
			}

			public void SendEmail(SupportIncidentEmail email)
			{
				Emails.Add(email);
			}
		}
	}

	internal class LocalChangeNotifierForTest : LocalChangeIncidentCustomerNotifier
	{
		public LocalChangeNotifierForTest(SupportIncident incident, IIncidentCustomerNotificationSender sender) : base(incident, sender) { }
		public SupportIncidentEmail StandardEmailForTest { get; set; }
		protected override SupportIncidentEmail CreateStandardEmailIfApplicable() => StandardEmailForTest ?? base.CreateStandardEmailIfApplicable();
	}

	internal class StaffChangeNotifierForTest : StaffChangeIncidentCustomerNotifier
	{
		//
		public StaffChangeNotifierForTest(SupportIncident incident, IIncidentCustomerNotificationSender sender) : base(incident, sender) { }

		public SupportIncidentEmail StandardEmailForTest { get; set; }
		public bool CanPreviewForTest { get; set; }

		protected override SupportIncidentEmail CreateStandardEmailIfApplicable() => StandardEmailForTest ?? base.CreateStandardEmailIfApplicable();
		protected override bool CanPreview => CanPreviewForTest;
	}
}
