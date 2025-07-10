using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.Testing;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;
using Constants = CargoWise.EventReference.Constants;
using CriticalityConstants = Enterprise.Core.Constants.CustomerService.CriticalityCodes;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using LoggerForTest = Enterprise.EConversation.Testing.LoggerForTest;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	public class SupportRequestProcessorTest : TestCaseWithFactory
	{
		public void TestProcessCriticality_CriticalIncidents_ShouldSendNotification() => AssertCriticalSupportRequestEmail(true);

		public void TestProcessCriticalityForWebRequest_CriticalIncidents_ShouldSendNotification() => AssertCriticalSupportRequestEmail(true, true);

		public void TestProcessCriticality_NonCriticalIncidents_ShouldNotSendNotification() => AssertCriticalSupportRequestEmail();

		public void TestProcessCriticalityForWebRequest_NonCriticalIncidents_ShouldNotSendNotification() => AssertCriticalSupportRequestEmail(false, true);

		public void TestGroupWithValidAndInvalidEmailsSendsEmail()
		{
			string xmlData = SetupRequest();

			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "";
			group.Staff.Add(staff);
			GlbStaff staff2 = group.Staff.AddNew();
			staff2.GS_EmailAddress = "test@test.com";
			staff2.GS_Code = "ZAC";
			Factory.Save();

			EDIDataRegistry.Instance.CustomerServiceHighCriticalityIncidentGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			SupportRequestProcessor processor = new SupportRequestProcessor();
			AssertNull(processor.Incident);
			processor.Process(xmlData);
			AssertNotNull(processor.Incident);

			AssertEquals("It should create a High Criticality email as the group has no user with a valid email", 2, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef email = Env.OutgoingMailManager.EmailsCreated[1];
			AssertEquals("Customer Service", email.FromDisplayName);
			AssertEquals("support@wisetechglobal.com", email.FromAddress);
			AssertEquals($"[{processor.Incident.IM_Product}] A CR1 incident raised by client Some Organisation", email.Subject);
			AssertContains("Zarn Bou raised a CR1 incident (ENT) for client: Some Organisation", email.Body);
			AssertContains("My Incident Summary", email.Body);
			AssertContains("My Incident Details\r\nLine two.", email.Body);
			AssertContains("<a href=\"edient:Command=ShowEditForm&ControllerID=SupportIncident&BusinessEntityPK=", email.Body);

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestGroupWithNoEmailSendsNoEmail()
		{
			string xmlData = SetupRequest();

			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			//This line below means no email will be sent
			staff.GS_Code = "ZAC";
			staff.GS_EmailAddress = "";
			Factory.Save();
			EDIDataRegistry.Instance.CustomerServiceHighCriticalityIncidentGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			SupportRequestProcessor processor = new SupportRequestProcessor();
			AssertNull(processor.Incident);
			processor.Process(xmlData);
			AssertNotNull(processor.Incident);

			AssertEquals("It shouldn't create a High Criticality email as the group has no user with a valid email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert(Env.OutgoingMailManager.EmailsCreated.Any(x => x.Subject == "Customer Service Incident Raised - CS00000001")); //make sure it's the correct email
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestGroupWithNoUserSendsNoEmail()
		{
			string xmlData = SetupRequest();

			//Group with no user
			GlbGroup group = Factory.New<GlbGroup>();
			Factory.Save();
			EDIDataRegistry.Instance.CustomerServiceHighCriticalityIncidentGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			SupportRequestProcessor processor = new SupportRequestProcessor();
			AssertNull(processor.Incident);
			processor.Process(xmlData);
			AssertNotNull(processor.Incident);

			AssertEquals("It shouldn't create a High Criticality email as the group has no user with a valid email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert(Env.OutgoingMailManager.EmailsCreated.Any(x => x.Subject == "Customer Service Incident Raised - CS00000001")); //make sure it's the correct email
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestNoGroupSendsNoEmail()
		{
			string xmlData = SetupRequest();

			SupportRequestProcessor processor = new SupportRequestProcessor();
			AssertNull(processor.Incident);
			processor.Process(xmlData);
			AssertNotNull(processor.Incident);

			AssertEquals("It shouldn't create a High Criticality email as the group has no user with a valid email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert(Env.OutgoingMailManager.EmailsCreated.Any(x => x.Subject == "Customer Service Incident Raised - CS00000001")); //make sure it's the correct email
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestProcessFromEmail()
		{
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident Summary with 'tag' <b>!";
			request.IncidentDetails = "My Incident Details\r\nLine two.";
			request.ReportingStaffMemberName = "Zubin Appoo";
			request.ClientReferenceNumber = "CL111111";
			request.ApprovingUser = "Zubin Appoo";

			string emailSubject = "Notification of Incident - (*Summary*)";
			string emailBody = @"Hey can you please wait for a couple of years since (*CurrentDate*), while we <b>start</b> analysing your incident. (*Summary*)";
			var templatePairCollection = new CodeDescriptionIncidentEmailTemplatePairCollection();
			var template = templatePairCollection.AddNew();
			template.Code = "AAA";
			template.Description = (NoResString)"AAA Desc";
			template.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = emailSubject;
			template.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = emailBody;
			template.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = emailSubject;
			template.EmailTemplates.ERequestV2EmailTemplate.EmailBody = emailBody;
			EDIDataRegistry.Instance.SelfLoggedIncidentNotificationEmailTemplateRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templatePairCollection);

			Xsd.OrgContact contact1 = request.Staff.AddNew();
			contact1.Name = "Zubin Appoo";
			contact1.EmailAddress = "zubin@test.com";

			Xsd.OrgContact contact2 = request.Staff.AddNew();
			contact2.Name = "Rakhsh";

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));

			string xmlData = "";
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, request);
				xmlData = writer.ToString();
			}

			SupportRequestProcessor processor = new SupportRequestProcessor();
			AssertNull(processor.Incident);
			processor.Process(xmlData);
			AssertNotNull(processor.Incident);

			AssertEquals(Licence.Database.PK, processor.Incident.IM_LD);
			AssertEquals(Licence.ClientCompany.PK, processor.Incident.IM_LCC);
			AssertEquals("My Incident Summary with 'tag' <b>!", processor.Incident.IM_Description);
			AssertEquals("My Incident Details\r\nLine two.", request.IncidentDetails);
			AssertEquals(true, processor.Incident.IsInDatabase);

			var orgReloaded = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			AssertEquals(4, orgReloaded.Contacts.Count);
			AssertEquals(true, orgReloaded.Contacts[0].IsInDatabase);
			AssertEquals(true, orgReloaded.Contacts[1].IsInDatabase);

			OrgContact[] found = (OrgContact[])orgReloaded.Contacts.Find(new ZQuery(OrgContactSchema.OC_ContactName, request.ApprovingUser));
			AssertEquals(1, found.Length);

			AssertEquals(found[0].PK, processor.Incident.IM_OC_Contact);

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);

			// Customer Notification Email
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("support@wisetechglobal.com", email.FromAddress);
			AssertEquals("zubin@test.com", email.Recipients[0].Email);
			AssertEquals("Notification of Incident - My Incident Summary with 'tag' <b>!", email.Subject);
			AssertContains("Email.Body", "Hey can you please wait for a couple of years since " + ZDateTime.Today.ToShortDateString() + ", while we <b>start</b> analysing your incident.", email.Body);
			AssertContains("Email.Body", "My Incident Summary with &#39;tag&#39; &lt;b&gt;!", email.Body);

			// Customer System Notification Email
			EmailDef emailSys = Env.OutgoingMailManager.EmailsCreated[1];
			AssertEquals(EDIDataRegistry.Instance.IncidentFromEmailAddress.Value, emailSys.FromAddress);
			AssertEquals("test@test.com", emailSys.Recipients[0].Email);
			AssertEquals("Customer Service Incident Raised - Your Ref: " + request.ClientReferenceNumber, emailSys.Subject);
			AssertEquals(1, emailSys.Attachments.Count);
			AssertEquals("Incident Details.xml", emailSys.Attachments[0].DisplayName);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			SupportIncident previousIncident = processor.Incident;
			processor.Process(xmlData);
			AssertEquals(previousIncident.PK, processor.Incident.PK);
		}

		public void TestProcessFromEHub()
		{
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.IncidentDetails = "My Incident Details\r\nLine two.";
			request.ReportingStaffMemberName = "Zubin Appoo";
			request.ClientReferenceNumber = "CL111111";
			request.ApprovingUser = "Zubin Appoo";

			string emailSubject = "Notification of Incident";
			string emailBody = @"Hey can you please wait for a couple of years since (*CurrentDate*), while we start analysing your incident.";
			var templatePairCollection = new CodeDescriptionIncidentEmailTemplatePairCollection();
			var template = templatePairCollection.AddNew();
			template.Code = "AAA";
			template.Description = (NoResString)"AAA Desc";
			template.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = emailSubject;
			template.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = emailBody;
			template.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = emailSubject;
			template.EmailTemplates.ERequestV2EmailTemplate.EmailBody = emailBody;
			EDIDataRegistry.Instance.SelfLoggedIncidentNotificationEmailTemplateRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templatePairCollection);

			Xsd.OrgContact contact1 = request.Staff.AddNew();
			contact1.Name = "Zubin Appoo";
			contact1.EmailAddress = "zubin@test.com";

			Xsd.OrgContact contact2 = request.Staff.AddNew();
			contact2.Name = "Rakhsh";

			SupportRequestProcessor processor = new SupportRequestProcessor();
			AssertNull(processor.Incident);
			processor.Process(request, true);
			AssertNotNull(processor.Incident);

			AssertEquals(Licence.Database.PK, processor.Incident.IM_LD);
			AssertEquals(Licence.ClientCompany.PK, processor.Incident.IM_LCC);
			AssertEquals("My Incident", processor.Incident.IM_Description);
			AssertEquals("My Incident Details\r\nLine two.", request.IncidentDetails);
			AssertEquals(true, processor.Incident.IsInDatabase);

			var orgReloaded = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			AssertEquals(4, orgReloaded.Contacts.Count);
			AssertEquals(true, orgReloaded.Contacts[0].IsInDatabase);
			AssertEquals(true, orgReloaded.Contacts[1].IsInDatabase);

			OrgContact[] found = (OrgContact[])orgReloaded.Contacts.Find(new ZQuery(OrgContactSchema.OC_ContactName, request.ApprovingUser));
			AssertEquals(1, found.Length);

			AssertEquals(found[0].PK, processor.Incident.IM_OC_Contact);

			// Customer System Notification Message
			EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("interchange count", 1, interchanges.Length);
			AssertEquals(SystemMessage.ApplicationCode, interchanges[0].EI_ApplicationCode);
			AssertContains("EI_BodyText", "<CustomerServiceResponse", interchanges[0].EI_BodyText);

			SupportIncident previousIncident = processor.Incident;
			processor.Process(request, true);
			AssertEquals(previousIncident.PK, processor.Incident.PK);

			EDIDataRegistry.Instance.SelfLoggedIncidentNotificationEmailTemplateRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionIncidentEmailTemplatePairCollection());
			request.ClientReferenceNumber = "CL2222222";
			processor = new SupportRequestProcessor();
			processor.Process(request, true);
			AssertNotNull(processor.Incident);
			var factory2 = new BusinessObjectFactory();
			var loadedIncident2 = factory2.Load<SupportIncident>(processor.Incident.PK);
			AssertEquals(true, loadedIncident2.EConversation.GetTimeOrderedMessages().Any(msg => msg.Body == "No incident created notification email sent because no template has been setup"));
		}

		public void TestProcessFromEHub_ProductsIgnoreForceSendResponseUsingEHubElseEmail()
		{
			var productList = new CodeDescriptionPairList();
			productList.AddPair("DTX", "DTX");
			EDIDataRegistry.Instance.IncidentProductsToIgnoreForceResponseSentViaEHub.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productList);

			var templatePairCollection = new CodeDescriptionIncidentEmailTemplatePairCollection();
			EDIDataRegistry.Instance.SelfLoggedIncidentNotificationEmailTemplateRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templatePairCollection);

			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.Product = "DTX";
			request.IncidentSummary = "DTX incident";
			request.IncidentDetails = "New incident";
			request.ReportingStaffMemberName = "Sam";
			request.ClientReferenceNumber = "MX821-L938";
			request.ApprovingUser = "Sam";

			Xsd.OrgContact contact = request.Staff.AddNew();
			contact.Name = "Sam";
			contact.EmailAddress = "sam@test.com";

			SupportRequestProcessor processor = new SupportRequestProcessor();
			AssertNull(processor.Incident);
			processor.Process(request, true);
			AssertNotNull(processor.Incident);
		}

		public void TestProcessFromEHub_IgnoreUpdateRequest()
		{
			var request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Update;
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.Product = "DTX";
			request.IncidentSummary = "DTX incident";
			request.IncidentDetails = "New incident";
			request.ReportingStaffMemberName = "Sam";
			request.ClientReferenceNumber = "MX821-L938";
			request.ApprovingUser = "Sam";

			var contact = request.Staff.AddNew();
			contact.Name = "Sam";
			contact.EmailAddress = "sam@test.com";

			var processor = new SupportRequestProcessor();
			processor.Process(request, true);
			AssertNull(processor.Incident);
		}

		public void TestProcess_CreateFeatureRequest()
		{
			GlbStaff.CurrentUser.GS_FullName = "Jenny Nguyen";

			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "AUT Feature Accepted";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "CLI";
			task11.P9_Description = "Auto Closed Feature Request";
			task11.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task11.P9_CompletedTimeUtc = ZDateTime.Now;

			var collection = EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.Value;
			var templatePair = collection.AddNew();
			templatePair.Code = SupportIncidentLookups.DispositionList.Constants.FeatureAccepted;
			templatePair.Description = (NoResString)"Auto Closed Feature Request";
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Auto Closed Feature";
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Feature is closed automatically";
			templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = "Auto Closed Feature";
			templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = "Feature is closed automatically";
			EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.IncidentDetails = "My Incident Details";
			request.ReportingStaffMemberName = "Samuel";
			request.ClientReferenceNumber = "SR00012341";
			request.ApprovingUser = "Samuel Wang";
			request.Criticality = "CR6";
			request.Action = SupportIncidentLookups.LegacyActions.Add;

			Xsd.OrgContact contact = request.Staff.AddNew();
			contact.Name = "Samuel Wang";
			contact.EmailAddress = "sam@test.com.au";

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));

			string xmlData = "";
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, request);
				xmlData = writer.ToString();
			}

			SupportRequestProcessor processor = new SupportRequestProcessor();
			processor.Process(xmlData);
			AssertNotNull(processor.Incident);
			AssertEquals(true, processor.Incident.IsInDatabase);
			AssertEquals(Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest, processor.Incident.IM_Priority);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, processor.Incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, processor.Incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, processor.Incident.IM_ResolutionCode);
			AssertEquals(ZString.Empty, processor.Incident.IM_GS_NKCustServiceContact);
			AssertEquals(1, processor.Incident.WorkflowItems.Count);
			var task = processor.Incident.WorkflowItems[0];
			AssertEquals("Auto Closed Feature Request", task.P9_Description);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("CLI", task.P9_Type);

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			CombineAssertions(() =>
			{
				var email = Env.OutgoingMailManager.EmailsCreated[1];
				var emailSys = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email (reponse message via email) subject", "Customer Service Incident Raised - Your Ref: SR00012341", email.Subject);
				AssertEquals("Email (reponse message via email) body", "", email.Body);

				AssertEquals("Email from display name", SupportIncident.SupportDisplayName, emailSys.FromDisplayName);
				AssertEquals("Email subject", "Auto Closed Feature", emailSys.Subject);
				AssertContains("Email body", "Feature is closed automatically", emailSys.Body);
				AssertContains("Email signature", SupportIncident.SupportDisplayName, emailSys.Body);
				AssertContains("Email signature", SupportIncident.SupportEmailAddress, emailSys.Body);
			});

			EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionIncidentEmailTemplatePairCollection());
			request.ClientReferenceNumber = "SR00012342";
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, request);
				xmlData = writer.ToString();
			}
			processor = new SupportRequestProcessor();
			processor.Process(xmlData);
			AssertNotNull(processor.Incident);
			AssertEquals(3, Env.OutgoingMailManager.EmailsCreated.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Email 3 (reponse message via email) subject", "Customer Service Incident Raised - Your Ref: SR00012342", Env.OutgoingMailManager.EmailsCreated[2].Subject);
				AssertEquals("Email 3 (reponse message via email) body", "", Env.OutgoingMailManager.EmailsCreated[2].Body);
			});

			var factory2 = new BusinessObjectFactory();
			var loadedIncident2 = factory2.Load<SupportIncident>(processor.Incident.PK);
			AssertEquals(true, loadedIncident2.EConversation.GetTimeOrderedMessages().Any(msg => msg.Body == "No incident closed as feature request notification email sent because no template has been setup"));
		}

		[TestDate(2014, 6, 30)]
		public void TestProcess_CreateQuoteRequest()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "DER Estimate";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "EST";
			task11.P9_Description = "Provide Development Estimate";
			task11.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task11.P9_EstDuration = TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(0, 10, 0));
			task11.P9_EstimateVariationFactor = 3M;

			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.IncidentDetails = "My Incident Details";
			request.ReportingStaffMemberName = "Samuel";
			request.ClientReferenceNumber = "SR00012341";
			request.ApprovingUser = "Samuel Wang";
			request.Criticality = "CR7";
			request.Action = SupportIncidentLookups.LegacyActions.Add;

			Xsd.OrgContact contact = request.Staff.AddNew();
			contact.Name = "Samuel Wang";
			contact.EmailAddress = "sam@test.com.au";

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));

			string xmlData = "";
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, request);
				xmlData = writer.ToString();
			}

			SupportRequestProcessor processor = new SupportRequestProcessor();
			processor.Process(xmlData);
			AssertNotNull(processor.Incident);
			AssertEquals(true, processor.Incident.IsInDatabase);
			AssertEquals(Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest, processor.Incident.IM_Priority);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, processor.Incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, processor.Incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate, processor.Incident.IM_ResolutionCode);
			AssertEquals(ZString.Empty, processor.Incident.IM_GS_NKCustServiceContact);
			AssertEquals(TestDateAttribute.Date, processor.Incident.Estimate.CIE_QuoteRequestedUTC);
			AssertEquals(processor.Incident.IM_OA_BranchAddress, processor.Incident.FeatureRequestClientAddressPK);
			AssertEquals(processor.Incident.IM_OH_Client, processor.Incident.FeatureRequestClientPK);
			AssertEquals(1, processor.Incident.WorkflowItems.Count);
			var task = processor.Incident.WorkflowItems[0];
			AssertEquals("Provide Development Estimate", task.P9_Description);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("EST", task.P9_Type);
			AssertEquals(10, task.P9_Sequence);
			AssertEquals(TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(0, 10, 0)), task.P9_EstDuration);
			AssertEquals(3M, task.P9_EstimateVariationFactor);
			AssertEquals("Estimate", task.ProcessHeader.FH_CompletionStatement);

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Email 1 subject", "Customer Service Incident Raised - Your Ref: SR00012341", Env.OutgoingMailManager.EmailsCreated[1].Subject);
			AssertEquals("Email 2 subject", "Customer Service Incident Raised - CS00000001", Env.OutgoingMailManager.EmailsCreated[0].Subject);
		}

		public void TestProcess_CreateServiceRequest()
		{
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.IncidentDetails = "My Incident Details";
			request.ReportingStaffMemberName = "Samuel";
			request.ClientReferenceNumber = "SR00012341";
			request.ApprovingUser = "Samuel Wang";
			request.Criticality = "CR9";
			request.Action = SupportIncidentLookups.LegacyActions.Add;

			Xsd.OrgContact contact = request.Staff.AddNew();
			contact.Name = "Samuel Wang";
			contact.EmailAddress = "sam@test.com.au";

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));

			string xmlData = "";
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, request);
				xmlData = writer.ToString();
			}

			SupportRequestProcessor processor = new SupportRequestProcessor();
			processor.Process(xmlData);
			AssertNotNull(processor.Incident);
			AssertEquals(true, processor.Incident.IsInDatabase);
			AssertEquals(Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest, processor.Incident.IM_Priority);
			AssertEquals(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, processor.Incident.IM_Category);
		}

		public void TestProcess_NotAddIncidentKeyLog()
		{
			string key = ZGuid.NewZGuid().ToString();

			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.PK = key;
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.IncidentDetails = "My Incident Details";
			request.ReportingStaffMemberName = "Samuel";
			request.ClientReferenceNumber = "SR00012341";
			request.ApprovingUser = "Samuel Wang";
			request.Criticality = "CR5";

			Xsd.OrgContact contact = request.Staff.AddNew();
			contact.Name = "Samuel Wang";
			contact.EmailAddress = "sam@test.com.au";

			SupportRequestProcessor processor = new SupportRequestProcessor();
			AssertNull(processor.Incident);
			processor.Process(request, true);
			AssertNotNull(processor.Incident);

			AssertEquals(0, processor.Incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Incident Key " + key)).Length);
		}

		[TestDate(2008, 1, 1)]
		public void TestProcess_SameClientReference()
		{
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.ReportingStaffMemberName = "Zubin Appoo";
			request.ClientReferenceNumber = "CL111111";
			request.ApprovingUser = "Zubin Appoo";

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));

			string xmlData = "";
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, request);
				xmlData = writer.ToString();
			}

			LoggerForTest logger = new LoggerForTest();
			SupportRequestProcessor processor = new SupportRequestProcessor(logger);
			AssertNull(processor.Incident);
			processor.Process(xmlData);
			AssertNotNull(processor.Incident);
			AssertEquals(true, processor.Incident.IsInDatabase);

			TestDateAttribute.Date = new DateTime(2008, 3, 30);
			processor.Incident = null;
			logger.Logs.Clear();
			processor.Process(xmlData);
			AssertNull("Incident has already been raised so not raised again", processor.Incident);

			AssertEquals(1, logger.Logs.Count);
			AssertEquals(new LogForTest(LogType.Information,
				"Request: CL111111, Incident: , Licence: ZUBRAKLOL, Approving User: \"Zubin Appoo\" <>, Status: Incident already exists. Nothing to do.",
				null), logger.Logs[0]);

			TestDateAttribute.Date = new DateTime(2008, 4, 2);
			processor.Incident = null;
			logger.Logs.Clear();
			processor.Process(xmlData);
			AssertNotNull("previous incident too old, new incident raised", processor.Incident);
			AssertEquals(true, processor.Incident.IsInDatabase);
		}

		public void TestProcess_BlankClientReference()
		{
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.ReportingStaffMemberName = "Zubin Appoo";
			request.ApprovingUser = "Zubin Appoo";

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));

			string xmlData = "";
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, request);
				xmlData = writer.ToString();
			}

			LoggerForTest logger = new LoggerForTest();
			SupportRequestProcessor processor = new SupportRequestProcessor(logger);
			AssertNull(processor.Incident);
			processor.Process(xmlData);
			AssertNotNull(processor.Incident);
			SupportIncident incident = processor.Incident;

			processor.Incident = null;
			logger.Logs.Clear();
			processor.Process(xmlData);
			AssertNotNull("Blank client ref so a new incident is always created", processor.Incident);
			AssertNotEquals("New incident created", incident.PK, processor.Incident.PK);

			AssertEquals(1, logger.Logs.Count);
			AssertEquals(new LogForTest(LogType.Information,
				"Request: , Incident: " + processor.Incident.IM_IncidentNumber + ", Licence: ZUBRAKLOL, Approving User: \"Zubin Appoo\" <>, Status: Incident created.",
				null), logger.Logs[0]);
		}

		public void TestContactNameWithTrailingNonbreakingSpace()
		{
			OrgContact contact = Org.Contacts.AddNew();
			contact.OC_ContactName = "bad name";
			contact.OC_Email = "wrongemail@test.com";

			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.ReportingStaffMemberName = "Bob";
			request.ApprovingUser = "bad name\u00A0";
			request.ApprovingUserEmail = "rightemail@test.com";
			var badContact = new Xsd.OrgContact();
			badContact.Name = "bad name\u00A0";
			badContact.EmailAddress = "rightemail@test.com";
			request.Staff.Add(badContact);

			Factory.Save();

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));

			string xmlData = "";
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, request);
				xmlData = writer.ToString();
			}

			SupportRequestProcessor processor = new SupportRequestProcessor();
			AssertNull(processor.Incident);
			processor.Process(xmlData);
			var orgReloaded = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			AssertEquals("new contact created since emails don't match", 6, orgReloaded.Contacts.Count);
			var contacts = (OrgContact[])orgReloaded.Contacts.Find(new ZQuery(OrgContactSchema.OC_Email, "rightemail@test.com"));
			AssertEquals(1, contacts.Length);
			AssertEquals("name is trimmed and unique", "bad name (1)", contacts[0].OC_ContactName);
		}

		public void TestChangeModuleIfIsLegacy()
		{
			CodeDescriptionPairList areas = new CodeDescriptionPairList();
			areas.AddPair("ARC", "Architecture");
			areas.AddPair("INT", "International Logistics");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew(Cr8ModuleList.Codes.CarbonEnvironmentalCompliance, Cr8ModuleList.Descriptions.CarbonEnvironmentalCompliance, "ARC", true);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew(Cr9ModuleList.Codes.AccountingDataTakeOn, Cr9ModuleList.Descriptions.AccountingDataTakeOn, "INT", true);
			product.ModuleMappings.AddNew(Cr9ModuleList.Codes.ConsultingAssistanceOnRating, Cr9ModuleList.Descriptions.ConsultingAssistanceOnRating, "INT", true);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var legacyMenuSectionMappings = new LegacyModuleMappingCollection(ModuleListType.MenuSection);
			legacyMenuSectionMappings.AddNew("OLD", "Old Menu Section", "CR8", Cr8ModuleList.Codes.CarbonEnvironmentalCompliance, "");
			EDIDataRegistry.Instance.LegacyMenuSectionMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, legacyMenuSectionMappings);

			var legacyCr8ModuleMappings = new LegacyModuleMappingCollection(ModuleListType.Cr8);
			legacyCr8ModuleMappings.AddNew("OLD", "Old CR8 Module", "CR9", Cr9ModuleList.Codes.AccountingDataTakeOn, "AU");
			EDIDataRegistry.Instance.LegacyCr8ModuleMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, legacyCr8ModuleMappings);

			var legacyCr9ModuleMappings = new LegacyModuleMappingCollection(ModuleListType.Cr9);
			legacyCr9ModuleMappings.AddNew("OLD", "Old CR9 Module", "", Cr9ModuleList.Codes.ConsultingAssistanceOnRating, "US");
			EDIDataRegistry.Instance.LegacyCr9ModuleMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, legacyCr9ModuleMappings);

			var serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));

			using (var writer = new StringWriter())
			{
				var requestForENT = new Xsd.CustomerServiceRequest();
				requestForENT.Product = "ENT";
				requestForENT.Criticality = "CR4";
				requestForENT.Module = "OLD";
				serializer.Serialize(writer, requestForENT);
				var xmlData = writer.ToString();

				var processor = new SupportRequestProcessor();
				processor.Process(xmlData);
				CombineAssertions(() =>
				{
					AssertEquals("ProductArea", "ARC", processor.Incident.ProductArea);
					AssertEquals("IM_Priority", "CR8", processor.Incident.IM_Priority);
					AssertEquals("IM_Module", Cr8ModuleList.Codes.CarbonEnvironmentalCompliance, processor.Incident.IM_Module);
					AssertEquals("IM_RN_NKCountry", "", processor.Incident.IM_RN_NKCountry);
				});
			}

			using (var writer = new StringWriter())
			{
				var requestForHUB = new Xsd.CustomerServiceRequest();
				requestForHUB.Product = "HUB";
				requestForHUB.Criticality = "CR4";
				requestForHUB.Module = "OLD";
				serializer.Serialize(writer, requestForHUB);
				var xmlData = writer.ToString();

				var processor = new SupportRequestProcessor();
				processor.Process(xmlData);
				CombineAssertions(() =>
				{
					AssertEquals("ProductArea", "", processor.Incident.ProductArea);
					AssertEquals("IM_Priority", "CR8", processor.Incident.IM_Priority);
					AssertEquals("IM_Module", Cr8ModuleList.Codes.CarbonEnvironmentalCompliance, processor.Incident.IM_Module);
					AssertEquals("IM_RN_NKCountry", "", processor.Incident.IM_RN_NKCountry);
				});
			}

			using (var writer = new StringWriter())
			{
				var requestForSPH = new Xsd.CustomerServiceRequest();
				requestForSPH.Product = "SPH";
				requestForSPH.Criticality = "CR4";
				requestForSPH.Module = "OLD";
				serializer.Serialize(writer, requestForSPH);
				var xmlData = writer.ToString();

				var processor = new SupportRequestProcessor();
				processor.Process(xmlData);
				CombineAssertions(() =>
				{
					AssertEquals("ProductArea", "", processor.Incident.ProductArea);
					AssertEquals("IM_Priority", "CR4", processor.Incident.IM_Priority);
					AssertEquals("IM_Module", "OLD", processor.Incident.IM_Module);
					AssertEquals("IM_RN_NKCountry", "", processor.Incident.IM_RN_NKCountry);
				});
			}

			using (var writer = new StringWriter())
			{
				var requestForENT = new Xsd.CustomerServiceRequest();
				requestForENT.Product = "ENT";
				requestForENT.Criticality = "CR9";
				requestForENT.Module = "OLD";
				serializer.Serialize(writer, requestForENT);
				var xmlData = writer.ToString();

				var processor = new SupportRequestProcessor();
				processor.Process(xmlData);
				CombineAssertions(() =>
				{
					AssertEquals("ProductArea", "INT", processor.Incident.ProductArea);
					AssertEquals("IM_Priority", "CR9", processor.Incident.IM_Priority);
					AssertEquals("IM_Module", Cr9ModuleList.Codes.ConsultingAssistanceOnRating, processor.Incident.IM_Module);
					AssertEquals("IM_RN_NKCountry", "US", processor.Incident.IM_RN_NKCountry);
				});
			}

			using (var writer = new StringWriter())
			{
				var requestForHUB = new Xsd.CustomerServiceRequest();
				requestForHUB.Product = "HUB";
				requestForHUB.Criticality = "CR8";
				requestForHUB.Module = "OLD";
				serializer.Serialize(writer, requestForHUB);
				var xmlData = writer.ToString();

				var processor = new SupportRequestProcessor();
				processor.Process(xmlData);
				CombineAssertions(() =>
				{
					AssertEquals("ProductArea", "", processor.Incident.ProductArea);
					AssertEquals("IM_Priority", "CR9", processor.Incident.IM_Priority);
					AssertEquals("IM_Module", Cr9ModuleList.Codes.AccountingDataTakeOn, processor.Incident.IM_Module);
					AssertEquals("IM_RN_NKCountry", "AU", processor.Incident.IM_RN_NKCountry);
				});
			}

			using (var writer = new StringWriter())
			{
				var requestForSPH = new Xsd.CustomerServiceRequest();
				requestForSPH.Product = "SPH";
				requestForSPH.Criticality = "CR8";
				requestForSPH.Module = "OLD";
				serializer.Serialize(writer, requestForSPH);
				var xmlData = writer.ToString();

				var processor = new SupportRequestProcessor();
				processor.Process(xmlData);
				CombineAssertions(() =>
				{
					AssertEquals("ProductArea", "", processor.Incident.ProductArea);
					AssertEquals("IM_Priority", "CR8", processor.Incident.IM_Priority);
					AssertEquals("IM_Module", "OLD", processor.Incident.IM_Module);
					AssertEquals("IM_RN_NKCountry", "", processor.Incident.IM_RN_NKCountry);
				});
			}

			using (var writer = new StringWriter())
			{
				var requestForENT = new Xsd.CustomerServiceRequest();
				requestForENT.Product = "ENT";
				requestForENT.Criticality = "CR9";
				requestForENT.Module = "OLD";
				serializer.Serialize(writer, requestForENT);
				var xmlData = writer.ToString();

				var processor = new SupportRequestProcessor();
				processor.Process(xmlData);
				CombineAssertions(() =>
				{
					AssertEquals("ProductArea", "INT", processor.Incident.ProductArea);
					AssertEquals("IM_Priority", "CR9", processor.Incident.IM_Priority);
					AssertEquals("IM_Module", Cr9ModuleList.Codes.ConsultingAssistanceOnRating, processor.Incident.IM_Module);
					AssertEquals("IM_RN_NKCountry", "US", processor.Incident.IM_RN_NKCountry);
				});
			}

			using (var writer = new StringWriter())
			{
				var requestForHUB = new Xsd.CustomerServiceRequest();
				requestForHUB.Product = "HUB";
				requestForHUB.Criticality = "CR9";
				requestForHUB.Module = "OLD";
				serializer.Serialize(writer, requestForHUB);
				var xmlData = writer.ToString();

				var processor = new SupportRequestProcessor();
				processor.Process(xmlData);
				CombineAssertions(() =>
				{
					AssertEquals("ProductArea", "", processor.Incident.ProductArea);
					AssertEquals("IM_Priority", "CR9", processor.Incident.IM_Priority);
					AssertEquals("IM_Module", Cr9ModuleList.Codes.ConsultingAssistanceOnRating, processor.Incident.IM_Module);
					AssertEquals("IM_RN_NKCountry", "US", processor.Incident.IM_RN_NKCountry);
				});
			}

			using (var writer = new StringWriter())
			{
				var requestForSPH = new Xsd.CustomerServiceRequest();
				requestForSPH.Product = "SPH";
				requestForSPH.Criticality = "CR9";
				requestForSPH.Module = "OLD";
				serializer.Serialize(writer, requestForSPH);
				var xmlData = writer.ToString();

				var processor = new SupportRequestProcessor();
				processor.Process(xmlData);
				CombineAssertions(() =>
				{
					AssertEquals("ProductArea", "", processor.Incident.ProductArea);
					AssertEquals("IM_Priority", "CR9", processor.Incident.IM_Priority);
					AssertEquals("IM_Module", "OLD", processor.Incident.IM_Module);
					AssertEquals("IM_RN_NKCountry", "", processor.Incident.IM_RN_NKCountry);
				});
			}
		}

		public virtual void TestHandleConcurrencyException()
		{
			string referenceNumber = "TestReferenceNumber";

			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident";
			request.IncidentDetails = "My Incident Details\r\nLine two.";
			request.ReportingStaffMemberName = "Zubin Appoo";
			request.ClientReferenceNumber = referenceNumber;
			request.ApprovingUser = "Zubin Appoo";

			string emailSubject = "Notification of Incident";
			string emailBody = @"Body.";
			var templatePairCollection = new CodeDescriptionIncidentEmailTemplatePairCollection();
			var template = templatePairCollection.AddNew();
			template.Code = "AAA";
			template.Description = (NoResString)"AAA Desc";
			template.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = emailSubject;
			template.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = emailBody;
			template.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = emailSubject;
			template.EmailTemplates.ERequestV2EmailTemplate.EmailBody = emailBody;
			EDIDataRegistry.Instance.SelfLoggedIncidentNotificationEmailTemplateRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templatePairCollection);

			Xsd.OrgContact contact1 = request.Staff.AddNew();
			contact1.Name = "Zubin Appoo";
			contact1.EmailAddress = "zubin@test.com";

			Xsd.OrgContact contact2 = request.Staff.AddNew();
			contact2.Name = "Rakhsh";

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));

			string xmlData = "";
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, request);
				xmlData = writer.ToString();
			}

			DummySupportRequestProcessorWithConcurrencyException testProcessor = new DummySupportRequestProcessorWithConcurrencyException();

			ZQuery query = new ZQuery();
			query.AddToFilter(IncidentMainSchema.IM_ClientIncidentReference, referenceNumber);
			SupportIncident[] incidents = Factory.Load<SupportIncident>(query);
			AssertEquals(0, incidents.Length);

			testProcessor.Process(xmlData);

			AssertEquals("Retried a few times", 3, testProcessor.SupportRequestProcessorCalled);
			incidents = Factory.Load<SupportIncident>(query);
			AssertEquals("Incident is saved.", 1, incidents.Length);
		}

		class DummySupportRequestProcessorWithConcurrencyException : SupportRequestProcessor
		{
			protected override void SaveIncident(BusinessObjectFactory factory)
			{
				supportRequestProcessorCalled++;

				if (supportRequestProcessorCalled < 3)
				{
					string newPhoneNUmber = "012345" + supportRequestProcessorCalled.ToString();
					string sql = string.Format(CultureInfo.CurrentCulture, "update dbo.OrgContact set OC_Phone = '{0}' where OC_ContactName = 'Zubin Appoo'", newPhoneNUmber);

					DbConnection connection = Db.Connection;
					using (DbCommand command = connection.Command(sql))
					{
						command.ExecuteNonQuery();
					}
				}

				factory.Save();
			}

			public int SupportRequestProcessorCalled => supportRequestProcessorCalled;

			protected int supportRequestProcessorCalled;
		}

		public void TestProcessNewWebRequest()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "DB2");
			var reportingContact = lic1.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request1 = Factory.New<IncidentRequest>();
			request1.INC_OC_ReportedBy = reportingContact.PK;
			request1.INC_Criticality = "CR5";
			request1.INC_Summary = "stuff happened";
			request1.INC_Details = "how stuff happen\nline two\r\nline three?";
			request1.INC_OC_ApprovedBy = reportingContact.PK;
			request1.INC_Type = "ENT";
			request1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request1.INC_SubType = "REF";
			request1.INC_Area = "Organisation";
			request1.INC_ProductLicence = "ENTCOMDB1";
			request1.INC_RN_NKCountry = "NZ";
			request1.INC_Language = "";

			var request2 = Factory.New<IncidentRequest>();
			request2.INC_OC_ReportedBy = reportingContact.PK;
			request2.INC_Criticality = "CR4";
			request2.INC_Summary = "stuff happened again";
			request2.INC_Details = "how stuff keep happening?";
			request2.INC_OC_ApprovedBy = reportingContact.PK;
			request2.INC_Type = "ENT";
			request2.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request2.INC_SubType = "RCB";
			request2.INC_Area = "AREnquiry";
			request2.INC_ProductLicence = "ENTCOMDB2";

			var requestNotSubmitted = Factory.New<IncidentRequest>();
			requestNotSubmitted.INC_OC_ReportedBy = reportingContact.PK;
			requestNotSubmitted.INC_Criticality = "CR3";
			requestNotSubmitted.INC_Summary = "will stuff happen";
			requestNotSubmitted.INC_Details = "could stuff keep happening?";
			requestNotSubmitted.INC_OC_ApprovedBy = reportingContact.PK;
			requestNotSubmitted.INC_Type = "ENT";
			requestNotSubmitted.INC_Status = SupportIncidentLookups.LegacyStatusCodes.New;
			requestNotSubmitted.INC_SubType = "RCB";
			requestNotSubmitted.INC_Area = "AREnquiry";
			requestNotSubmitted.INC_ProductLicence = "ENTCOMDB2";

			Factory.Save();

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			processor.ProcessAllNewWebRequests();
			processor.ProcessAllNewWebRequests();

			var supportIncident1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request1.PK));
			AssertNotNull("incident 1 created", supportIncident1);
			AssertEquals("CR5", supportIncident1.IM_Priority);
			AssertEquals(lic1.Company.LC_OH, supportIncident1.IM_OH_Client);
			AssertEquals(reportingContact.PK, supportIncident1.IM_OC_Contact);
			AssertEquals(lic1.LA_LD, supportIncident1.IM_LD);
			AssertEquals(lic1.ClientCompany.PK, supportIncident1.IM_LCC);
			AssertEquals("REF", supportIncident1.IM_Module);
			AssertEquals("Organisation", supportIncident1.IM_SourceModuleId);
			AssertEquals("how stuff happen\r\nline two\r\nline three?", supportIncident1.DetailNoteText);
			AssertEquals("NZ", supportIncident1.IM_RN_NKCountry);
			AssertEquals("EN", supportIncident1.IM_Language);

			var supportIncident2 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request2.PK));
			AssertNotNull("incident 2 created", supportIncident1);
			AssertEquals("CR4", supportIncident2.IM_Priority);
			AssertEquals(lic1.Company.LC_OH, supportIncident2.IM_OH_Client);
			AssertEquals(reportingContact.PK, supportIncident2.IM_OC_Contact);
			AssertEquals(lic2.LA_LD, supportIncident2.IM_LD);
			AssertEquals(lic2.ClientCompany.PK, supportIncident2.IM_LCC);
			AssertEquals("RCB", supportIncident2.IM_Module);
			AssertEquals("AREnquiry", supportIncident2.IM_SourceModuleId);

			AssertNull("not yet submitted request has no incident", Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, requestNotSubmitted.PK)));
		}

		public void TestProcessNewWebRequest_ReportedByBlank()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");

			var request1 = Factory.New<IncidentRequest>();
			request1.INC_Criticality = "CR5";
			request1.INC_Summary = "stuff happened";
			request1.INC_Details = "how stuff happen\nline two\r\nline three?";
			request1.INC_Type = "ENT";
			request1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request1.INC_SubType = "REF";
			request1.INC_Area = "Organisation";
			request1.INC_ProductLicence = "ENTCOMDB1";

			Factory.Save();

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			processor.ProcessAllNewWebRequests();

			var supportIncident1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request1.PK));
			request1.Reload();
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.Closed, request1.INC_Status);
			AssertNull("request without contact has no incident", supportIncident1);
			AssertEquals("CS00000001 processing new", logger.Logs[0].message);
			AssertEquals("CS00000001 Reported By is blank. Closing and ignoring since this should be impossible.", logger.Logs[1].message);
		}

		public void TestProcessNewWebRequest_ProductLicenceOrgNotEqualReportingOrg()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "CO2");
			lic2.ClientCompany.LCC_RN_NKCountryCode = "NZ";
			var reportingContact = lic1.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request1 = Factory.New<IncidentRequest>();
			request1.INC_OC_ReportedBy = reportingContact.PK;
			request1.INC_Criticality = "CR5";
			request1.INC_Summary = "stuff happened";
			request1.INC_Details = "how stuff happen?";
			request1.INC_OC_ApprovedBy = reportingContact.PK;
			request1.INC_Type = "ENT";
			request1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request1.INC_SubType = "REF";
			request1.INC_Area = "Organisation";
			request1.INC_ProductLicence = "ENTCO2DB1";

			Factory.Save();

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			processor.ProcessAllNewWebRequests();

			var supportIncident1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request1.PK));
			AssertEquals("CR5", supportIncident1.IM_Priority);
			AssertEquals("Client is reporting org, not licence org", lic1.Company.LC_OH, supportIncident1.IM_OH_Client);
			AssertEquals(reportingContact.PK, supportIncident1.IM_OC_Contact);
			AssertEquals(lic2.LA_LD, supportIncident1.IM_LD);
			AssertEquals(lic2.ClientCompany.PK, supportIncident1.IM_LCC);
			AssertEquals("NZ", supportIncident1.IM_RN_NKCountry);
			AssertEquals("REF", supportIncident1.IM_Module);
			AssertEquals("Organisation", supportIncident1.IM_SourceModuleId);
			AssertEquals("how stuff happen?", supportIncident1.DetailNoteText);
		}

		public void TestProcessNewWebRequest_FollowUpERequest()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "DB2");
			var reportingContact = lic1.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var oldRequest = Factory.New<IncidentRequest>();
			oldRequest.INC_IncidentNumber = "CS01191903";
			oldRequest.INC_OC_ReportedBy = reportingContact.PK;
			oldRequest.INC_Criticality = "CR4";
			oldRequest.INC_Summary = "stuff happened again";
			oldRequest.INC_Details = "how stuff keep happening?";
			oldRequest.INC_OC_ApprovedBy = reportingContact.PK;
			oldRequest.INC_Type = "ENT";
			oldRequest.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			oldRequest.INC_SubType = "RCB";
			oldRequest.INC_Area = "AREnquiry";
			oldRequest.INC_ProductLicence = "ENTCOMDB2";

			Factory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			processor.ProcessAllNewWebRequests();

			var oldSupportIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, oldRequest.PK));
			AssertNotNull("Precondition", oldSupportIncident);

			var freshRequest = Factory.New<IncidentRequest>();
			freshRequest.INC_OC_ReportedBy = reportingContact.PK;
			freshRequest.INC_Criticality = "CR5";
			freshRequest.INC_Summary = "stuff happened";
			freshRequest.INC_Details = "how stuff happen\nline two\r\nline three?";
			freshRequest.INC_OC_ApprovedBy = reportingContact.PK;
			freshRequest.INC_Type = "ENT";
			freshRequest.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			freshRequest.INC_SubType = "REF";
			freshRequest.INC_Area = "Organisation";
			freshRequest.INC_ProductLicence = "ENTCOMDB1";
			freshRequest.INC_RN_NKCountry = "NZ";

			var followUpRequest = Factory.New<IncidentRequest>();
			followUpRequest.INC_OC_ReportedBy = reportingContact.PK;
			followUpRequest.INC_Criticality = "CR4";
			followUpRequest.INC_Summary = "stuff happened again";
			followUpRequest.INC_Details = FormattableString.Invariant($"This is a follow-up eRequest of {oldRequest.INC_IncidentNumber}\r\n\r\nhow stuff keep happening?");
			followUpRequest.INC_OC_ApprovedBy = reportingContact.PK;
			followUpRequest.INC_Type = "ENT";
			followUpRequest.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			followUpRequest.INC_SubType = "RCB";
			followUpRequest.INC_Area = "AREnquiry";
			followUpRequest.INC_ProductLicence = "ENTCOMDB2";
			followUpRequest.Logs.AddNew(AutoEvents.MiscellaneousEvent, FormattableString.Invariant($"|DES=Follow up incident created|OLD={oldRequest.INC_IncidentNumber}|ORG=DEMORGSYD|EML=alex@contact.com"));

			var requestNotSubmitted = Factory.New<IncidentRequest>();
			requestNotSubmitted.INC_OC_ReportedBy = reportingContact.PK;
			requestNotSubmitted.INC_Criticality = "CR3";
			requestNotSubmitted.INC_Summary = "will stuff happen";
			requestNotSubmitted.INC_Details = "could stuff keep happening?";
			requestNotSubmitted.INC_OC_ApprovedBy = reportingContact.PK;
			requestNotSubmitted.INC_Type = "ENT";
			requestNotSubmitted.INC_Status = SupportIncidentLookups.LegacyStatusCodes.New;
			requestNotSubmitted.INC_SubType = "RCB";
			requestNotSubmitted.INC_Area = "AREnquiry";
			requestNotSubmitted.INC_ProductLicence = "ENTCOMDB2";

			Factory.Save();

			processor.ProcessAllNewWebRequests();
			processor.ProcessAllNewWebRequests();

			var supportIncident1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, freshRequest.PK));
			AssertNotNull("Precondition: incident 1 created", supportIncident1);
			AssertEquals("Should not create any follow up logs", false, supportIncident1.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode && x.SL_Reference.Contains("Follow up incident created")));

			var supportIncident2 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, followUpRequest.PK));
			AssertNotNull("incident 2 created", supportIncident2);
			AssertEquals("Precondition: Should copy details as usual", "CR4", supportIncident2.IM_Priority);
			AssertEquals("Precondition: Should copy details as usual", lic1.Company.LC_OH, supportIncident2.IM_OH_Client);
			AssertEquals("Precondition: Should copy details as usual", reportingContact.PK, supportIncident2.IM_OC_Contact);
			AssertEquals("Precondition: Should copy details as usual", lic2.LA_LD, supportIncident2.IM_LD);
			AssertEquals("Precondition: Should copy details as usual", lic2.ClientCompany.PK, supportIncident2.IM_LCC);
			AssertEquals("Precondition: Should copy details as usual", "RCB", supportIncident2.IM_Module);
			AssertEquals("Precondition: Should copy details as usual", "AREnquiry", supportIncident2.IM_SourceModuleId);
			var followUpLog1 = oldSupportIncident.Logs.MostRecentLogByEventTime(AutoEvents.MiscellaneousEvent);
			var followUpLog2 = supportIncident2.Logs.MostRecentLogByEventTime(AutoEvents.MiscellaneousEvent);

			AssertNotNull("Should create a log on the old incident", followUpLog1);
			AssertNotNull("Should create a log on the follow up incident", followUpLog2);

			var followUpIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, followUpRequest.PK));
			AssertNotNull(followUpIncident);
			AssertEquals(followUpIncident.RelatedItems.Count, 1);
			AssertCollectionContains("The old incident should be attached to the follow up incident as a related item", oldSupportIncident, followUpIncident.RelatedItems);

			var followUpLog1Parameters = followUpLog1.Parameters;
			var followUpLog2Parameters = followUpLog2.Parameters;

			var pair1 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Description, "Follow up incident created");
			var pair2 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.New, supportIncident2.Number);

			Assert(followUpLog1Parameters.Contains(pair1));
			Assert(followUpLog1Parameters.Contains(pair2));

			var pair3 = new KeyValuePair<string, string>(EventReferenceParameters.Codes.Old, oldSupportIncident.Number);

			Assert(followUpLog2Parameters.Contains(pair1));
			Assert(followUpLog2Parameters.Contains(pair3));

			AssertNull("not yet submitted request has no incident", Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, requestNotSubmitted.PK)));
		}

		public void TestProcessCriticalityCR6ForWebRequest_EmailSignature()
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

			var internalRequest = Factory.New<IncidentRequest>();
			internalRequest.INC_OC_ReportedBy = contact.PK;
			internalRequest.INC_Criticality = "CR6";
			internalRequest.INC_Summary = "feature request";
			internalRequest.INC_Details = "feature request";
			internalRequest.INC_OC_ApprovedBy = contact.PK;
			internalRequest.INC_Type = "ENT";
			internalRequest.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			internalRequest.INC_SubType = "REF";
			internalRequest.INC_Area = "Organisation";
			internalRequest.INC_ProductLicence = "ENTCOMDB1";

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INC";
			template.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;

			var header = template.ProcessHeaders.AddNew();
			header.FH_CompletionStatement = "AUT Feature Accepted";

			var task = template.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_Sequence = 10;
			task.P9_Type = "CLI";
			task.P9_Description = "Auto Closed Feature Request";
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task.P9_CompletedTimeUtc = ZDateTime.Now;

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllNewWebRequests();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			var supportIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, internalRequest.PK));
			AssertEquals($"Incident: {supportIncident.IM_IncidentNumber} has been Closed with reason: Feature Request Accepted.", email.Subject);
			AssertContains("Email Signature should use default user name", $"</p>\r\n                  <p>\r\n                      <b>\r\n                          {SupportIncident.SupportDisplayName}", email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestProcessNewWebRequest_InternalStaff()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "joe@test.org";
			staff1.GS_FullName = "Joe Staff";
			staff1.GS_Code = "JOE";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "bob@test.org";
			staff2.GS_FullName = "Bob Staff";
			staff2.GS_Code = "BOB";

			var internalLic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			internalLic.Database.LicEnterprise.LE_IsInternal = true;
			var externalLic = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "DB2");
			var contact1 = internalLic.Company.Header.Contacts.AddNew();
			contact1.OC_ContactName = staff1.GS_FullName;
			contact1.OC_Email = staff1.GS_EmailAddress;
			var contact2 = internalLic.Company.Header.Contacts.AddNew();
			contact2.OC_ContactName = staff2.GS_FullName;
			contact2.OC_Email = staff2.GS_EmailAddress;

			var reportingContact3 = externalLic.Company.Header.Contacts.AddNew();
			reportingContact3.OC_ContactName = staff1.GS_FullName;
			reportingContact3.OC_Email = staff1.GS_EmailAddress;

			var internalRequest1 = Factory.New<IncidentRequest>();
			internalRequest1.INC_OC_ReportedBy = contact1.PK;
			internalRequest1.INC_Criticality = "CR5";
			internalRequest1.INC_Summary = "stuff happened";
			internalRequest1.INC_Details = "how stuff happen?";
			internalRequest1.INC_OC_ApprovedBy = contact1.PK;
			internalRequest1.INC_Type = "ENT";
			internalRequest1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			internalRequest1.INC_SubType = "REF";
			internalRequest1.INC_Area = "Organisation";
			internalRequest1.INC_ProductLicence = "ENTCOMDB1";

			var internalRequest2 = Factory.New<IncidentRequest>();
			internalRequest2.INC_OC_ReportedBy = contact2.PK;
			internalRequest2.INC_Criticality = "CR5";
			internalRequest2.INC_Summary = "stuff happened";
			internalRequest2.INC_Details = "how stuff happen?";
			internalRequest2.INC_OC_ApprovedBy = contact1.PK;
			internalRequest2.INC_Type = "ENT";
			internalRequest2.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			internalRequest2.INC_SubType = "REF";
			internalRequest2.INC_Area = "Organisation";
			internalRequest2.INC_ProductLicence = "ENTCOMDB1";

			var internalRequest3 = Factory.New<IncidentRequest>();
			internalRequest3.INC_OC_ReportedBy = contact2.PK;
			internalRequest3.INC_Criticality = "CR5";
			internalRequest3.INC_Summary = "stuff happened";
			internalRequest3.INC_Details = "how stuff happen?";
			internalRequest3.INC_OC_ApprovedBy = contact1.PK;
			internalRequest3.INC_Type = "WTA";
			internalRequest3.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			internalRequest3.INC_SubType = "REF";
			internalRequest3.INC_Area = "Organisation";

			var externalRequest = Factory.New<IncidentRequest>();
			externalRequest.INC_OC_ReportedBy = reportingContact3.PK;
			externalRequest.INC_Criticality = "CR4";
			externalRequest.INC_Summary = "stuff happened again";
			externalRequest.INC_Details = "how stuff keep happening?";
			externalRequest.INC_OC_ApprovedBy = reportingContact3.PK;
			externalRequest.INC_Type = "ENT";
			externalRequest.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			externalRequest.INC_SubType = "RCB";
			externalRequest.INC_Area = "AREnquiry";
			externalRequest.INC_ProductLicence = "EN2CO2DB2";

			Factory.Save();

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			processor.ProcessAllNewWebRequests();
			processor.ProcessAllNewWebRequests();
			{
				var supportIncident1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, internalRequest1.PK));
				AssertEquals(contact1.PK, supportIncident1.IM_OC_Contact);
				AssertEquals(internalLic.ClientCompany.PK, supportIncident1.IM_LCC);
				AssertEquals("JOE", supportIncident1.IM_SystemCreateUser);
				AssertEquals("JOE", supportIncident1.IM_SystemLastEditUser);
			}

			{
				var supportIncident2 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, internalRequest2.PK));
				AssertEquals(contact2.PK, supportIncident2.IM_OC_Contact);
				AssertEquals(internalLic.ClientCompany.PK, supportIncident2.IM_LCC);
				AssertEquals("BOB", supportIncident2.IM_SystemCreateUser);
				AssertEquals("BOB", supportIncident2.IM_SystemLastEditUser);
			}

			{
				var supportIncident3 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, internalRequest3.PK));
				AssertEquals(contact2.PK, supportIncident3.IM_OC_Contact);
				AssertEquals(ZGuid.Empty, supportIncident3.IM_LCC);
				AssertEquals(ZGuid.Empty, supportIncident3.IM_LD);
				AssertEquals("BOB", supportIncident3.IM_SystemCreateUser);
				AssertEquals("BOB", supportIncident3.IM_SystemLastEditUser);
			}

			{
				var supportIncident4 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, externalRequest.PK));
				AssertEquals(reportingContact3.PK, supportIncident4.IM_OC_Contact);
				AssertEquals(externalLic.ClientCompany.PK, supportIncident4.IM_LCC);
				AssertEquals(Env.CurrentUser.Initials, supportIncident4.IM_SystemCreateUser);
				AssertEquals(Env.CurrentUser.Initials, supportIncident4.IM_SystemLastEditUser);
			}
		}

		public void TestProcessNewWebRequest_RetryAfterConcurrencyError()
		{
			Factory.RefreshEnabled = false;
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request = Factory.New<IncidentRequest>();
			request.INC_OC_ReportedBy = reportingContact.PK;
			request.INC_Criticality = "CR5";
			request.INC_Details = "how stuff happen?";
			request.INC_OC_ApprovedBy = reportingContact.PK;
			request.INC_Summary = "stuff happened";
			request.INC_Type = "ENT";
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request.INC_RN_NKCountry = "AU";

			Factory.Save();

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			bool needConcurrentEdit = true;
			processor.SavingFactoryForTest += (savingFactory) =>
			{
				if (needConcurrentEdit)
				{
					request.INC_Summary = "This will cause a concurrency error";
					Factory.Save();
					needConcurrentEdit = false;
				}
			};
			processor.ProcessAllNewWebRequests();

			Assert("SavingFactoryForTest was called", !needConcurrentEdit);
			request.Reload();
			AssertNotEquals("Status was changed", SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent, request.INC_Status);
			AssertEquals("This will cause a concurrency error", request.INC_Summary);
			var supportIncident1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request.PK));
			AssertNotNull(supportIncident1);
		}

		public void TestProcessNewWebRequest_DatabaseDefault()
		{
			var licBOR = BillingTestHelper.CreateBorderWiseLicence(Factory, "ENT", "COM", "BOR");
			var licCW = BillingTestHelper.CreateAnotherDatabase(licBOR, "PRD", false);
			var clientCompany1 = BillingTestHelper.CreateClientCompany(licCW.Database, "CO1");
			var clientCompany2 = BillingTestHelper.CreateClientCompany(licCW.Database, "CO2");
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			clientCompany2.LCC_RN_NKCountryCode = "US";
			licCW.Database.LD_LicenceType = DatabaseTypes.Codes.Training;
			var reportingContact1 = licCW.Company.Header.Contacts.AddNew();
			reportingContact1.OC_ContactName = "Joe";
			reportingContact1.OC_Email = "joe@test.org";

			var orgWithNoLicence = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgWithNoLicence.OH_RL_NKClosestPort = "AUSYD";
			var reportingContact2 = orgWithNoLicence.Contacts.AddNew();
			reportingContact2.OC_ContactName = "Bob";
			reportingContact2.OC_Email = "bob@bobs.org";

			var requestCW1 = Factory.New<IncidentRequest>();
			requestCW1.INC_OC_ReportedBy = reportingContact1.PK;
			requestCW1.INC_Criticality = "CR5";
			requestCW1.INC_Details = "how stuff happen in US?";
			requestCW1.INC_OC_ApprovedBy = reportingContact1.PK;
			requestCW1.INC_Summary = "CW1 US stuff happened";
			requestCW1.INC_Type = ProductTypes.Codes.Enterprise;
			requestCW1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			requestCW1.INC_RN_NKCountry = "AU";

			var requestCW2 = Factory.New<IncidentRequest>();
			requestCW2.INC_OC_ReportedBy = reportingContact1.PK;
			requestCW2.INC_Criticality = "CR5";
			requestCW2.INC_Details = "how stuff happen in AU?";
			requestCW2.INC_OC_ApprovedBy = reportingContact1.PK;
			requestCW2.INC_Summary = "CW1 AU stuff happened";
			requestCW2.INC_Type = ProductTypes.Codes.Enterprise;
			requestCW2.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			requestCW2.INC_RN_NKCountry = "US";

			var requestBOR = Factory.New<IncidentRequest>();
			requestBOR.INC_OC_ReportedBy = reportingContact1.PK;
			requestBOR.INC_Criticality = "CR5";
			requestBOR.INC_Details = "why is it so?";
			requestBOR.INC_OC_ApprovedBy = reportingContact1.PK;
			requestBOR.INC_Summary = "BorderWise stuff happened";
			requestBOR.INC_Type = ProductTypes.Codes.BorderWise;
			requestBOR.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			requestBOR.INC_RN_NKCountry = "NZ";

			var requestNoLicence = Factory.New<IncidentRequest>();
			requestNoLicence.INC_OC_ReportedBy = reportingContact2.PK;
			requestNoLicence.INC_Criticality = "CR5";
			requestNoLicence.INC_Details = "why is it so?";
			requestNoLicence.INC_OC_ApprovedBy = reportingContact2.PK;
			requestNoLicence.INC_Summary = "Generic stuff happened";
			requestNoLicence.INC_Type = ProductTypes.Codes.Enterprise;
			requestNoLicence.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			requestNoLicence.INC_RN_NKCountry = "AU";

			Factory.Save();

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			processor.ProcessAllNewWebRequests();

			var supportIncidentCW1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, requestCW1.PK));
			requestCW1.Reload();
			var supportIncidentCW2 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, requestCW2.PK));
			requestCW2.Reload();
			var supportIncidentBOR = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, requestBOR.PK));
			requestBOR.Reload();
			var supportIncidentNoLicence = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, requestNoLicence.PK));
			requestNoLicence.Reload();

			AssertEquals(licCW.LA_LD, supportIncidentCW1.IM_LD);
			AssertEquals(ProductTypes.Codes.Enterprise, supportIncidentCW1.IM_Product);
			Assert(supportIncidentCW1.IM_LCC.IsEmpty);

			AssertEquals(licCW.LA_LD, supportIncidentCW2.IM_LD);
			AssertEquals(ProductTypes.Codes.Enterprise, supportIncidentCW2.IM_Product);
			Assert(supportIncidentCW2.IM_LCC.IsEmpty);

			Assert("no need to set BorderWise DB even if there is one", supportIncidentBOR.IM_LD.IsEmpty);
			AssertEquals("BOR", supportIncidentBOR.IM_Product);
			Assert(supportIncidentBOR.IM_LCC.IsEmpty);

			Assert(supportIncidentNoLicence.IM_LD.IsEmpty);
			AssertEquals(ProductTypes.Codes.Enterprise, supportIncidentNoLicence.IM_Product);
			Assert(supportIncidentNoLicence.IM_LCC.IsEmpty);
		}

		public void TestProcessNewWebRequest_NonEnterpriseFamilyDatabase()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			var db = licence.Database;
			db.LD_Product = "CSP";
			db.LicEnterprise.LE_EnterpriseCode = "";
			db.LD_DatabaseNumber = 2130;

			var contact = licence.Company.Header.Contacts.AddNew();
			contact.OC_ContactName = "Tester";
			contact.OC_Email = "tester@test.org";

			var request = Factory.New<IncidentRequest>();
			request.INC_OC_ReportedBy = contact.PK;
			request.INC_Criticality = "CR5";
			request.INC_Summary = "stuff happened";
			request.INC_Details = "how stuff happen\nline two\r\nline three?";
			request.INC_Type = "CSP";
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request.INC_SubType = "REF";
			request.INC_Area = "Organisation";
			request.INC_ProductLicence = db.EnterpriseID + "." + db.DatabaseId;

			Factory.Save();

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			processor.ProcessAllNewWebRequests();

			var supportIncident1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request.PK));
			AssertNotNull(supportIncident1);
			AssertEquals("Database is set", db.PK, supportIncident1.IM_LD);
		}

		public void TestProcessNewWebRequestShouldLogJobConversationMutexException()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "DB2");
			var reportingContact = lic1.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request1 = Factory.New<IncidentRequest>();
			request1.INC_OC_ReportedBy = reportingContact.PK;
			request1.INC_Criticality = "CR5";
			request1.INC_Summary = "stuff happened";
			request1.INC_Details = "how stuff happen\nline two\r\nline three?";
			request1.INC_OC_ApprovedBy = reportingContact.PK;
			request1.INC_Type = "ENT";
			request1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request1.INC_SubType = "REF";
			request1.INC_Area = "Organisation";
			request1.INC_ProductLicence = "ENTCOMDB1";
			request1.INC_RN_NKCountry = "NZ";
			request1.INC_Language = "";
			request1.INC_ServiceType = "ST1";

			var request2 = Factory.New<IncidentRequest>();
			request2.INC_OC_ReportedBy = reportingContact.PK;
			request2.INC_Criticality = "CR4";
			request2.INC_Summary = "stuff happened again";
			request2.INC_Details = "how stuff keep happening?";
			request2.INC_OC_ApprovedBy = reportingContact.PK;
			request2.INC_Type = "ENT";
			request2.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request2.INC_SubType = "RCB";
			request2.INC_Area = "AREnquiry";
			request2.INC_ProductLicence = "ENTCOMDB2";
			request2.INC_ServiceType = "ST2";
			Factory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			SqlApplicationLock lock1 = null;
			SqlApplicationLock lock2 = null;
			try
			{
				using (var conn2 = Db.NewExtraConnectionToMainDb())
				{
					AssertEquals("Precondition: Should successfully obtain lock", true, conn2.TryGetLock("CreatingNewConversation" + request1.PK, out lock1));
					Assert(lock1.IsHoldingLock());

					AssertEquals("Precondition: Should not be able to obtain lock", false, Db.Connection.TryGetLock("CreatingNewConversation" + request1.PK, out lock2));
					AssertNull(lock2);
					AssertNoExceptionThrown("Mutex exception could be caught and logged", () => processor.ProcessAllNewWebRequests());
					Assert("Should have logged the failure", logger.Logs.Select(l => l.message).Contains("The incident could not be processed as its Job Conversation is already being processed in another instance"));
				}
			}
			finally
			{
				lock1?.Dispose();
				lock2?.Dispose();
			}

			var supportIncident1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request1.PK));
			AssertNull("incident 1 created", supportIncident1);

			var supportIncident2 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request2.PK));
			AssertNotNull("incident 2 created", supportIncident2);
			AssertEquals("CR4", supportIncident2.IM_Priority);
			AssertEquals(lic1.Company.LC_OH, supportIncident2.IM_OH_Client);
			AssertEquals(reportingContact.PK, supportIncident2.IM_OC_Contact);
			AssertEquals(lic2.LA_LD, supportIncident2.IM_LD);
			AssertEquals(lic2.ClientCompany.PK, supportIncident2.IM_LCC);
			AssertEquals("RCB", supportIncident2.IM_Module);
			AssertEquals("AREnquiry", supportIncident2.IM_SourceModuleId);
			AssertEquals("ST2", supportIncident2.IM_ServiceType);

			processor.ProcessAllNewWebRequests();
			supportIncident1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request1.PK));
			AssertNotNull("incident 1 created", supportIncident1);
			AssertEquals("CR5", supportIncident1.IM_Priority);
			AssertEquals(lic1.Company.LC_OH, supportIncident1.IM_OH_Client);
			AssertEquals(reportingContact.PK, supportIncident1.IM_OC_Contact);
			AssertEquals(lic1.LA_LD, supportIncident1.IM_LD);
			AssertEquals(lic1.ClientCompany.PK, supportIncident1.IM_LCC);
			AssertEquals("REF", supportIncident1.IM_Module);
			AssertEquals("Organisation", supportIncident1.IM_SourceModuleId);
			AssertEquals("how stuff happen\r\nline two\r\nline three?", supportIncident1.DetailNoteText);
			AssertEquals("NZ", supportIncident1.IM_RN_NKCountry);
			AssertEquals("EN", supportIncident1.IM_Language);
			AssertEquals("ST1", supportIncident1.IM_ServiceType);
		}

		public void TestProcessNewEdocs_YesIncident()
		{
			int mailCount1 = Factory.GetDatabaseCount(typeof(MailItem));
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request = Factory.New<IncidentRequest>();
			request.INC_OC_ReportedBy = reportingContact.PK;
			request.INC_Criticality = "CR5";
			request.INC_Details = "how stuff happen?";
			request.INC_OC_ApprovedBy = reportingContact.PK;
			request.INC_Summary = "stuff happened";
			request.INC_Type = "ENT";
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;

			var incident = Factory.New<SupportIncident>();
			SupportRequestProcessor.PopulateIncidentFromRequest(incident, request);
			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;

			var eDoc1 = (StorageDocsBase)request.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc"), "stuff.txt", "COR");
			request.DocManagerInfo.AddLogsForNewDocument(request, eDoc1);
			var eDoc2 = (StorageDocsBase)request.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc2"), "stuff2.txt", "COR");
			request.DocManagerInfo.AddLogsForNewDocument(request, eDoc2);

			BusinessObjectFactory.SaveTogether(Factory, request.DocManagerInfo.MasterFactory);

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			var factoryForProcesssing = new BusinessObjectFactory() { RefreshEnabled = false };
			processor.ProcessNewEdocs(factoryForProcesssing, request.PK, new ZGuid[] { eDoc1.PK, eDoc2.PK });
			factoryForProcesssing.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			request = factory2.Load<IncidentRequest>(request.PK);
			var convo = JobConversation.GetOrCreate(request);
			var msg0 = convo.Messages[0];
			var msg1 = convo.Messages[1];
			var msg2 = convo.Messages[2];
			AssertEquals("Joe (AAAAAA) has been added to the conversation.", msg0.Body);
			AssertEquals("Attached to eDocs: stuff.txt", msg1.Body);
			AssertEquals("Attached to eDocs: stuff2.txt", msg2.Body);
			AssertEquals(3, convo.Messages.Count);

			AssertEquals("staff not available", 2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessNewEdocs_YesIncident_NoSaving()
		{
			int mailCount1 = Factory.GetDatabaseCount(typeof(MailItem));
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request = Factory.New<IncidentRequest>();
			request.INC_OC_ReportedBy = reportingContact.PK;
			request.INC_Criticality = "CR5";
			request.INC_Details = "how stuff happen?";
			request.INC_OC_ApprovedBy = reportingContact.PK;
			request.INC_Summary = "stuff happened";
			request.INC_Type = "ENT";
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;

			var incident = Factory.New<SupportIncident>();
			SupportRequestProcessor.PopulateIncidentFromRequest(incident, request);
			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;

			var eDoc1 = (StorageDocsBase)request.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc"), "stuff.txt", "COR");
			request.DocManagerInfo.AddLogsForNewDocument(request, eDoc1);
			var eDoc2 = (StorageDocsBase)request.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc2"), "stuff2.txt", "COR");
			request.DocManagerInfo.AddLogsForNewDocument(request, eDoc2);

			BusinessObjectFactory.SaveTogether(Factory, request.DocManagerInfo.MasterFactory);

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			var factoryForProcesssing = new BusinessObjectFactory() { RefreshEnabled = false };
			processor.ProcessNewEdocs(factoryForProcesssing, request.PK, new ZGuid[] { eDoc1.PK, eDoc2.PK });

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			request = factory2.Load<IncidentRequest>(request.PK);
			var convo = JobConversation.GetOrCreate(request);

			AssertEquals(1, convo.Messages.Count);
			AssertEquals("staff not available", 2, Env.OutgoingMailManager.EmailsCreated.Count);

			int mailCount2 = Factory.GetDatabaseCount(typeof(MailItem));
			AssertEquals(mailCount1, mailCount2);
		}

		public void TestProcessNewEdocs_NoIncident()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request = Factory.New<IncidentRequest>();
			request.INC_OC_ReportedBy = reportingContact.PK;
			request.INC_Criticality = "CR5";
			request.INC_Details = "how stuff happen?";
			request.INC_OC_ApprovedBy = reportingContact.PK;
			request.INC_Summary = "stuff happened";
			request.INC_Type = "ENT";
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;

			var eDoc1 = (StorageDocsBase)request.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc"), "stuff.txt", "COR");
			request.DocManagerInfo.AddLogsForNewDocument(request, eDoc1);
			var eDoc2 = (StorageDocsBase)request.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc2"), "stuff2.txt", "COR");
			request.DocManagerInfo.AddLogsForNewDocument(request, eDoc2);

			BusinessObjectFactory.SaveTogether(Factory, request.DocManagerInfo.MasterFactory);

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			var factoryForProcesssing = new BusinessObjectFactory() { RefreshEnabled = false };
			processor.ProcessNewEdocs(factoryForProcesssing, request.PK, new ZGuid[] { eDoc1.PK, eDoc2.PK });
			factoryForProcesssing.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			request = factory2.Load<IncidentRequest>(request.PK);
			var convo = JobConversation.GetOrCreate(request);
			var msg1 = convo.Messages[0];
			var msg2 = convo.Messages[1];
			AssertEquals("Attached to eDocs: stuff.txt", msg1.Body);
			AssertEquals("Attached to eDocs: stuff2.txt", msg2.Body);
			AssertEquals(2, convo.Messages.Count);

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessAllNewWebMessages()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request = Factory.New<IncidentRequest>();
			request.INC_OC_ReportedBy = reportingContact.PK;
			request.INC_Criticality = "CR5";
			request.INC_Details = "how stuff happen?";
			request.INC_OC_ApprovedBy = reportingContact.PK;
			request.INC_Summary = "stuff happened";
			request.INC_Type = "ENT";
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;

			var incident = Factory.New<SupportIncident>();
			SupportRequestProcessor.PopulateIncidentFromRequest(incident, request);
			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
			var convo = incident.EConversation.JobConversationForTest;

			var contactParticpant = convo.Participants.AddNew();
			contactParticpant.JCP_ParticipantTableCode = reportingContact.TablePrefix;
			contactParticpant.JCP_ParticipantID = reportingContact.PK;
			var staffParticpant = convo.Participants.AddNew();
			staffParticpant.JCP_ParticipantTableCode = GlbStaffSchema.Constants.Prefix;
			staffParticpant.JCP_ParticipantID = Env.CurrentUserPK;

			var legacyMsg1 = convo.Messages.AddNew(contactParticpant, "legacyMsg1", false);
			legacyMsg1.JCM_IsLocal = false;
			var legacy1 = Factory.New<EdiLegacyConversationMessage>();
			legacy1.ELC_JCM_Message = legacyMsg1.PK;

			var webMsg1 = convo.Messages.AddNew(contactParticpant, "webMsg1", false);
			webMsg1.JCM_IsLocal = false;

			var staffMsg1 = convo.Messages.AddNew(staffParticpant, "staffMsg1", false);
			staffMsg1.JCM_IsLocal = false;

			Factory.Save();

			AssertEquals("count of new remote messages in queue", 2, (int)Db.Connection.ExecuteScalar("select count(*) from dbo.EdiIncidentConversationMessageQueue"));

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllNewWebMessages();

			AssertEquals("no messages left in queue", 0, (int)Db.Connection.ExecuteScalar("select count(*) from dbo.EdiIncidentConversationMessageQueue"));

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("webMsg1", email.Body);
		}

		public void TestShouldTriggerSubscriberUpdateEmail_WhenProcessAllNewWebMessages()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request = Factory.New<IncidentRequest>();
			request.INC_OC_ReportedBy = reportingContact.PK;
			request.INC_Criticality = "CR5";
			request.INC_Details = "how stuff happen?";
			request.INC_OC_ApprovedBy = reportingContact.PK;
			request.INC_Summary = "stuff happened";
			request.INC_Type = "ENT";
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;

			var incident = Factory.New<SupportIncident>();
			SupportRequestProcessor.PopulateIncidentFromRequest(incident, request);

			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
			var convo = incident.EConversation.JobConversationForTest;

			var contactParticpant = convo.Participants.AddNew();
			contactParticpant.JCP_ParticipantTableCode = reportingContact.TablePrefix;
			contactParticpant.JCP_ParticipantID = reportingContact.PK;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact@test.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Future Industries";
			org.OH_RL_NKClosestPort = "AUSYD";
			var orgAddress = org.Addresses.MainAddress;
			orgAddress.OA_Email = "org@test.com";
			orgAddress.OA_Address1 = "address 1";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "123";
			staff.GS_EmailAddress = "123@123.com";

			incident.EConversation.Conversation.Participants.AddNewParticipant(org);
			incident.EConversation.Conversation.Participants.AddNewParticipant(staff);
			incident.EConversation.Conversation.Participants.AddNewParticipant(contact);

			var staffParticpant = convo.Participants.AddNew();
			staffParticpant.JCP_ParticipantTableCode = GlbStaffSchema.Constants.Prefix;
			staffParticpant.JCP_ParticipantID = Env.CurrentUserPK;

			var participantEML2 = convo.Participants.AddNew();
			participantEML2.EmailAddress = "emailParticipant@os.com";

			var webMsg1 = convo.Messages.AddNew(contactParticpant, "webMsg1", false);
			webMsg1.JCM_IsLocal = false;

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllNewWebMessages();

			AssertEquals("no messages left in queue", 0, (int)Db.Connection.ExecuteScalar("select count(*) from dbo.EdiIncidentConversationMessageQueue"));

			AssertEquals(4, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("webMsg1", email.Body);

			var email1 = Env.OutgoingMailManager.EmailsCreated[1];
			AssertContains("New Messages in", email1.Subject);
			AssertEquals("Should only contain staff recipients", 1, email1.Recipients.Count);
			AssertEquals("Should only contain staff recipients", "123@123.com", email1.Recipients[0].Email);

			var email2 = Env.OutgoingMailManager.EmailsCreated[2];
			var email3 = Env.OutgoingMailManager.EmailsCreated[3];
			AssertContains("New Messages in", email2.Subject);
			AssertContains("New Messages in", email3.Subject);

			if (email2.Recipients.Count == 2)
			{
				AssertEquals("Should only contain contact recipients", true, email2.Recipients.Cast<RecipientDef>().Any(x => x.Email == "contact@test.com"));
				AssertEquals("Should only contain contact recipients", true, email2.Recipients.Cast<RecipientDef>().Any(x => x.Email == "org@test.com"));
				AssertEquals("Should only contain email recipients", true, email3.Recipients.Cast<RecipientDef>().Any(x => x.Email == "emailParticipant@os.com"));
			}
			else
			{
				AssertEquals("Should only contain contact recipients", true, email3.Recipients.Cast<RecipientDef>().Any(x => x.Email == "contact@test.com"));
				AssertEquals("Should only contain contact recipients", true, email3.Recipients.Cast<RecipientDef>().Any(x => x.Email == "org@test.com"));
				AssertEquals("Should only contain email recipients", true, email2.Recipients.Cast<RecipientDef>().Any(x => x.Email == "emailParticipant@os.com"));
			}
		}

		public void TestProcessAllNewWebMessages_UpdateLastEditTime()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request = Factory.New<IncidentRequest>();
			request.INC_OC_ReportedBy = reportingContact.PK;
			request.INC_Criticality = "CR5";
			request.INC_Details = "how stuff happen?";
			request.INC_OC_ApprovedBy = reportingContact.PK;
			request.INC_Summary = "stuff happened";
			request.INC_Type = "ENT";
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;

			var incident = Factory.New<SupportIncident>();
			SupportRequestProcessor.PopulateIncidentFromRequest(incident, request);
			incident.IM_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			var incidentImSystemLastEditTimeUtc = incident.IM_SystemLastEditTimeUtc;
			Factory.Save();
			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
			var convo = incident.EConversation.JobConversationForTest;

			var contactParticpant = convo.Participants.GetWithoutAdd(reportingContact);
			var staffParticpant = convo.Participants.AddNew();
			staffParticpant.JCP_ParticipantTableCode = GlbStaffSchema.Constants.Prefix;
			staffParticpant.JCP_ParticipantID = Env.CurrentUserPK;

			var legacyMsg1 = convo.Messages.AddNew(contactParticpant, "legacyMsg1", false);
			legacyMsg1.JCM_IsLocal = false;
			var legacy1 = Factory.New<EdiLegacyConversationMessage>();
			legacy1.ELC_JCM_Message = legacyMsg1.PK;
			Factory.Save();
			var webMsg1 = convo.Messages.AddNew(contactParticpant, "webMsg1", false);
			webMsg1.JCM_IsLocal = false;
			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllNewWebMessages();
			AssertEquals("incident's IM_SystemLastEditTimeUtc should be updated ", true, incidentImSystemLastEditTimeUtc.CompareTo(incident.IM_SystemLastEditTimeUtc) < 0);
		}

		public void TestProcessAllNewWebMessages_RetryAfterConcurrencyError()
		{
			Factory.RefreshEnabled = false;
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request = Factory.New<IncidentRequest>();
			request.INC_OC_ReportedBy = reportingContact.PK;
			request.INC_Criticality = "CR5";
			request.INC_Details = "how stuff happen?";
			request.INC_OC_ApprovedBy = reportingContact.PK;
			request.INC_Summary = "stuff happened";
			request.INC_Type = "ENT";
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;

			var incident = Factory.New<SupportIncident>();
			SupportRequestProcessor.PopulateIncidentFromRequest(incident, request);
			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
			var convo = incident.EConversation.JobConversationForTest;

			var contactParticpant = convo.Participants.AddNew();
			contactParticpant.JCP_ParticipantTableCode = reportingContact.TablePrefix;
			contactParticpant.JCP_ParticipantID = reportingContact.PK;
			var staffParticpant = convo.Participants.AddNew();
			staffParticpant.JCP_ParticipantTableCode = GlbStaffSchema.Constants.Prefix;
			staffParticpant.JCP_ParticipantID = Env.CurrentUserPK;

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);

			var webMsg1 = convo.Messages.AddNew(contactParticpant, "webMsg1", false);
			webMsg1.JCM_IsLocal = false;

			Factory.Save();

			AssertEquals("PRE", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, request.INC_Status);

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			bool needConcurrentEdit = true;
			processor.SavingFactoryForTest += (savingFactory) =>
			{
				if (needConcurrentEdit)
				{
					request.INC_Summary = "This will cause a concurrency error";
					Factory.Save();
					needConcurrentEdit = false;
				}
			};
			processor.ProcessAllNewWebMessages();

			Assert("SavingFactoryForTest was called", !needConcurrentEdit);
			request.Reload();
			AssertNotEquals("Status was changed", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, request.INC_Status);
			AssertEquals("This will cause a concurrency error", request.INC_Summary);
		}

		public void TestProcessAllNewWebMessages_EnsureOpenTask()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var incident1 = CreateTestWebIncident(reportingContact);
			var incident2 = CreateTestWebIncident(reportingContact);
			var incident3 = CreateTestWebIncident(reportingContact);
			var incident4 = CreateTestWebIncident(reportingContact);
			var incident5 = CreateTestWebIncident(reportingContact);
			incident1.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "Resolved");
			incident2.CloseAsAcceptedFeatureRequest("foo");
			incident3.Escalate(SupportIncidentCategoriesList.Codes.Defect, "this is a defect");
			incident3.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoDefectFound, "");
			incident4.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			incident5.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			var task = incident5.WorkflowItems.AddNew();
			task.P9_Sequence = 1;
			task.P9_Type = "INV";
			task.P9_Description = "investigation";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			AssertEquals(SupportIncidentLookups.Status.Closed, incident1.IM_Status);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident2.IM_Status);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident3.IM_Status);

			Factory.Save();

			AddMessageFromContact(incident1, "msg1");
			AddMessageFromContact(incident2, "msg2");
			AddMessageFromContact(incident3, "msg3");
			AddMessageFromContact(incident4, "msg4");
			AddMessageFromContact(incident5, "msg5");

			Factory.Save();

			var blankRegistryValue = new IncidentClosureDispositionCollection(true, 3, 4);

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, blankRegistryValue))
			{
				var logger = new LoggerForTest();
				var processor = new SupportRequestProcessor(logger);
				processor.ProcessAllNewWebMessages();

				incident1.Reload();
				incident2.Reload();
				incident3.Reload();
				incident4.Reload();
				incident5.Reload();

				AssertNotEquals(SupportIncidentLookups.Status.Closed, incident1.IM_Status);
				AssertNotEquals(SupportIncidentLookups.Status.Closed, incident2.IM_Status);
				AssertNotEquals(SupportIncidentLookups.Status.Closed, incident3.IM_Status);
				AssertNotEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident4.IM_ResolutionCode);
				AssertNotEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident5.IM_ResolutionCode);
			}
		}

		public void TestProcessAllNewWebMessages_WhenResolved_AddingNewMessageShouldReopenIncident()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, isResolution: ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var incident4 = CreateTestWebIncident(reportingContact);
				incident4.CloseIncident(resolvedCode, "");

				Factory.Save();

				AddMessageFromContact(incident4, "msg4");

				Factory.Save();

				var blankRegistryValue = new IncidentClosureDispositionCollection(true, 3, 4);

				var logger = new LoggerForTest();
				var processor = new SupportRequestProcessor(logger);
				AssertEquals("Precondition: Should be resolved before processing web messages",
					SupportIncidentLookups.DispositionList.Constants.Closed.Resolved,
					incident4.IM_ResolutionCode);
				processor.ProcessAllNewWebMessages();

				incident4.Reload();

				AssertNotEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved,
					incident4.IM_ResolutionCode);
				AssertEquals(IncidentMainLookups.Status.Working, incident4.IM_Status);
			}
		}

		public void TestProcessAllNewWebMessages_WhenResolvedAndClosed_AddingNewMessageShouldReopenIncident()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var incident5 = CreateTestWebIncident(reportingContact);
			incident5.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");

			Factory.Save();

			AddMessageFromContact(incident5, "msg5");

			Factory.Save();

			var blankRegistryValue = new IncidentClosureDispositionCollection(true, 3, 4);

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, blankRegistryValue))
			{
				var logger = new LoggerForTest();
				var processor = new SupportRequestProcessor(logger);
				AssertEquals("Precondition: Should be resolved and closed before processing web messages",
					SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed,
					incident5.IM_ResolutionCode);
				processor.ProcessAllNewWebMessages();

				incident5.Reload();

				AssertNotEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident5.IM_ResolutionCode);
				AssertEquals(IncidentMainLookups.Status.Working, incident5.IM_Status);
			}
		}

		public void TestProcessAllNewWebMessagesShouldNotTriggerREQEventForGroupControlledIncident()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var incident1 = CreateTestWebIncident(reportingContact);
			var incident2 = CreateTestWebIncident(reportingContact);

			var controlStage = "ZZZ";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(controlStage, "desc", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, controlIncidents: true);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_IncidentGroupNumber = "ING000001";
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = controlStage;
			var link = Factory.New<IncidentManagementLink>();
			link.INL_IM_Incident = incident1.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;
			Factory.Save();

			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INC";
			template.P0_OH_Client = templateOrg.PK;
			var templateProcessHeader1 = template.ProcessHeaders.AddNew();
			templateProcessHeader1.FH_CompletionStatement = "REQ";
			var templateTask11 = template.WorkflowItems.AddNew();
			templateTask11.P9_FH_ProcessHeader = templateProcessHeader1.PK;
			templateTask11.P9_Sequence = 5;
			templateTask11.P9_Type = "INV";
			templateTask11.P9_Description = "Resume investigation";
			templateTask11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			AddMessageFromContact(incident1, "msg1");
			AddMessageFromContact(incident2, "msg2");
			incident1.IM_Status = "CLS";
			incident2.IM_Status = "CLS";
			var task2 = incident2.WorkflowItems.AddNew();
			task2.P9_Sequence = 1;
			task2.P9_Type = "INV";
			task2.P9_Description = "investigation";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllNewWebMessages();

			incident1.Reload();
			incident2.Reload();
			var hasREQEvent1 = incident1.Logs.GetAllLogs().Cast<StmALog>().Any(l =>
															l.SL_Reference.Contains("Event - REQ") &&
															l.Event.SE_Code == Events.StatusChangeCode);
			var hasREQEvent2 = incident2.Logs.GetAllLogs().Cast<StmALog>().Any(l =>
															l.SL_Reference.Contains("Event - REQ") &&
															l.Event.SE_Code == Events.StatusChangeCode);

			AssertEquals("incident1 shoule be GroupControlled", true, incident1.IsGroupControlled);
			AssertEquals("incident2 shoule not be GroupControlled", false, incident2.IsGroupControlled);

			AssertEquals("GroupControlled incident should not trigger REQ event", false, hasREQEvent1);
			AssertEquals("incident2 should trigger REQ event", true, hasREQEvent2);
		}

		public void TestProcessAllNewWebMessages_EnsureOpenTask_ResolvedOrClosed()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var incident = CreateTestWebIncident(reportingContact);
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";

			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "Closed");
				Factory.Save();

				AssertEquals("Precondition: incident should be closed as Closed", SupportIncidentLookups.Status.Closed, incident.IM_Status);
				AssertEquals("Precondition: incident should be closed as SelfResolved", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("Precondition: incident should be closed as SelfResolved", SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, incident.IM_ClosureResolution);

				var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
				var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
				var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
				criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
				productResolutionAndClosureBehaviour.Code = incident.IM_Product;

				var logger = new LoggerForTest();
				var processor = new SupportRequestProcessor(logger);

				productResolutionAndClosureBehaviour.ClosedReopenRule = "NEV";
				using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
				{
					AddMessageFromContact(incident, "new msg1");
					Factory.Save();

					processor.ProcessAllNewWebMessages();
					incident.Reload();
					AssertEquals("incident should not be re-opened because the ClosedReopenRule is Never", SupportIncidentLookups.Status.Closed, incident.IM_Status);
				}

				productResolutionAndClosureBehaviour.ClosedReopenRule = "ALW";
				using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
				{
					AddMessageFromContact(incident, "new msg2");
					Factory.Save();

					processor.ProcessAllNewWebMessages();
					incident.Reload();
					incident.Request.Reload();
					AssertNotEquals("incident should be re-opened because the ClosedReopenRule is Allow", SupportIncidentLookups.Status.Closed, incident.IM_Status);
					AssertNotEquals("incident request status should not be closed (CLS)", SupportIncidentLookups.Status.Closed, incident.Request.INC_Status);
				}

				incident.CloseIncident(resolvedCode, "Resolved");
				Factory.Save();

				AssertEquals("incident request status should be set to resolved (SLV)", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, incident.Request.INC_Status);
				AssertEquals("Precondition: incident should be closed as Resolved", SupportIncidentLookups.Status.Closed, incident.IM_Status);
				AssertEquals("Precondition: incident should be closed as Resolved", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, incident.IM_ResolutionCode);
				AssertEquals("Precondition: incident should be closed as Resolved", resolvedCode, incident.IM_ClosureResolution);

				productResolutionAndClosureBehaviour.ClosedReopenRule = "NEV";
				using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
				{
					AddMessageFromContact(incident, "new msg3");
					Factory.Save();

					processor.ProcessAllNewWebMessages();
					incident.Reload();
					AssertNotEquals("incident should be re-opened because the ResolutionCode is Resolved", SupportIncidentLookups.Status.Closed, incident.IM_Status);
				}
			}
		}

		public void TestProcessAllNewWebMessages_ShouldAddMessageReceivedEventToIncidentManagementGroup()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = licence.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var incidentGroup = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var incident1 = CreateTestWebIncident(reportingContact);
			var incident2 = CreateTestWebIncident(reportingContact);

			var incidentLink = Factory.NewWithValidTestData<IncidentManagementLink>();
			incidentLink.INL_ING_Group = incidentGroup.PK;
			incidentLink.INL_IM_Incident = incident1.PK;

			Factory.Save();

			AddMessageFromContact(incident1, "msg1");
			AddMessageFromContact(incident2, "msg2");

			Factory.Save();

			AssertEquals("Precondition", 0, incidentGroup.Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == AutoEvents.MessageReceivedCode).Count());
			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllNewWebMessages();

			var incidentGroupReloaded = new BusinessObjectFactory().Load<IncidentManagementGroup>(incidentGroup.PK);
			var messageReceivedLogs = incidentGroupReloaded.Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == AutoEvents.MessageReceivedCode).ToArray();
			AssertEquals("Should have added a single MRR event", 1, messageReceivedLogs.Length);
			var messageReceivedLog = messageReceivedLogs.First();
			var freeTextBits = new List<string>();
			var paramBits = new List<(string Key, string Value)>();
			EventLogReferenceBuilder.New().ParseReference(messageReceivedLog.SL_Reference, (text) => freeTextBits.Add(text), (key, value) => paramBits.Add((key, value)));
			AssertEquals(incident1.IM_IncidentNumber, paramBits.First(x => x.Key == Constants.EventReferenceParameters.Codes.ReferenceNumber).Value);
		}

		public void TestProcessAllWebStatusUpdates()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var incident1 = CreateTestWebIncident(reportingContact);
			var incident2 = CreateTestWebIncident(reportingContact);
			var incident3 = CreateTestWebIncident(reportingContact);
			incident1.CloseAsAcceptedFeatureRequest("foo");
			incident3.CloseAsAcceptedFeatureRequest("bar");

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var webFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			webFactory.Load<IncidentRequest>(incident1.IM_INC_Request)
				.INC_Status = SupportIncidentLookups.LegacyStatusCodes.DevelopmentEstimateRequested;
			webFactory.Load<IncidentRequest>(incident2.IM_INC_Request)
				.INC_Status = SupportIncidentLookups.LegacyStatusCodes.Closed;
			webFactory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			incident1 = factory2.Load<SupportIncident>(incident1.PK);
			incident2 = factory2.Load<SupportIncident>(incident2.PK);
			incident3 = factory2.Load<SupportIncident>(incident3.PK);
			AssertNotEquals("PRE", incident1.IM_RequestStatus, incident1.Request.INC_Status);
			AssertNotEquals("PRE", incident2.IM_RequestStatus, incident2.Request.INC_Status);

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllWebStatusUpdates();

			factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident1 = factory2.Load<SupportIncident>(incident1.PK);
			var loadedIncident2 = factory2.Load<SupportIncident>(incident2.PK);
			var loadedIncident3 = factory2.Load<SupportIncident>(incident3.PK);

			AssertEquals("processed status matches", loadedIncident1.IM_RequestStatus, loadedIncident1.Request.INC_Status);
			AssertEquals("processed status matches", loadedIncident2.IM_RequestStatus, loadedIncident2.Request.INC_Status);
			AssertEquals("processed status matches", loadedIncident3.IM_RequestStatus, loadedIncident3.Request.INC_Status);
			CombineAssertions(() =>
			{
				AssertEquals("CS00000001 processing status change", logger.Logs[0].message);
				AssertEquals("CS00000002 processing status change", logger.Logs[1].message);
				AssertEquals("CS00000002 customer status change discarded CLS, set to original CSV", logger.Logs[2].message);
				AssertEquals(3, logger.Logs.Count);
			});
		}

		public void TestProcessAllWebStatusUpdates_RequestEstimate()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;

			var header11 = template1.ProcessHeaders.AddNew();
			header11.FH_CompletionStatement = "DER Investigate";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header11.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "INV";
			task11.P9_Description = "Investigate";
			task11.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task11.P9_EstDuration = TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(0, 10, 0));
			task11.P9_EstimateVariationFactor = 3M;

			var header12 = template1.ProcessHeaders.AddNew();
			header12.FH_CompletionStatement = "DER Estimate Request";

			var task12 = template1.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header12.PK;
			task12.P9_Sequence = 10;
			task12.P9_Type = "EST";
			task12.P9_Description = "Provide Development Estimate";
			task12.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task12.P9_EstDuration = TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(0, 10, 0));

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "INC";
			template2.P0_SubType1 = ProductTypes.Codes.Enterprise;
			template2.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header21 = template2.ProcessHeaders.AddNew();
			header21.FH_CompletionStatement = "Investigate";

			var task21 = template2.WorkflowItems.AddNew();
			task21.P9_FH_ProcessHeader = header21.PK;
			task21.P9_Sequence = 10;
			task21.P9_Type = "CLI";
			task21.P9_Description = "Auto Closed Feature Request";
			task21.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var header22 = template2.ProcessHeaders.AddNew();
			header22.FH_CompletionStatement = "Review";

			var task22 = template2.WorkflowItems.AddNew();
			task22.P9_FH_ProcessHeader = header22.PK;
			task22.P9_Sequence = 10;
			task22.P9_Type = "INV";
			task22.P9_Description = "Review";
			task22.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task22.P9_EstDuration = TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(0, 10, 0));
			task22.P9_EstimateVariationFactor = 3M;

			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.CreateDependencyLink(template1, header11, header12);
			bmTestHelper.CreateDependencyLink(template2, header21, header22);

			Factory.Save();

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";
			var incident = CreateTestWebIncident(reportingContact);
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			Factory.Save();

			AssertEquals(2, incident.WorkflowItems.Count);

			var webFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var request = webFactory.Load<IncidentRequest>(incident.IM_INC_Request);
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.DevelopmentEstimateRequested;
			request.INC_Criticality = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			webFactory.Save();

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllWebStatusUpdates();

			incident.Reload();
			AssertEquals(Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest, incident.IM_Priority);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate, incident.IM_ResolutionCode);

			incident.WorkflowItems.Load();
			AssertEquals(4, incident.WorkflowItems.Count);
		}

		public void TestProcessERequestDocumentQueue()
		{
			var request = Factory.NewWithValidTestData<IncidentRequest>();
			request.INC_IncidentNumber = "INC1024";
			request.INC_ReferenceID = ZGuid.NewZGuid();

			var queue1 = Factory.NewWithValidTestData<EdiERequestDocumentQueue>();
			queue1.EDQ_INC_ReferenceID = request.INC_ReferenceID;
			queue1.EDQ_FileName = "att1.bin";
			queue1.EDQ_Data = new byte[] { 1, 2, 3 };
			queue1.EDQ_IsPublished = true;

			var queue2 = Factory.NewWithValidTestData<EdiERequestDocumentQueue>();
			queue2.EDQ_INC_ReferenceID = ZGuid.NewZGuid();
			queue2.EDQ_FileName = "att2.bin";
			queue2.EDQ_Data = new byte[] { 4, 5, 6 };

			var queue3 = Factory.NewWithValidTestData<EdiERequestDocumentQueue>();
			queue3.EDQ_INC_ReferenceID = request.INC_ReferenceID;
			queue3.EDQ_FileName = "att3.bin";
			queue3.EDQ_Data = new byte[] { 7, 8, 9 };

			var queue1_EDQ_IsPublished = queue1.EDQ_IsPublished;
			var queue3_EDQ_IsPublished = queue3.EDQ_IsPublished;

			Factory.Save();

			TestConnection.ExecuteNonQuery($"UPDATE dbo.EdiERequestDocumentQueue SET EDQ_SystemCreateTimeUtc = '2018-1-1' WHERE EDQ_PK = '{queue2.PK}';");

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessERequestDocumentQueue();

			var factory = new BusinessObjectFactory();
			var requestInNewFactory = factory.Load<IncidentRequest>(request.PK);
			AssertEquals(2, requestInNewFactory.DocManagerInfo.AllEDocs.Count);

			var att1 = requestInNewFactory.DocManagerInfo.AllEDocs.OfType<IeDocBase>().Single(x => x.FileName == "att1.bin");
			AssertEquals(queue1_EDQ_IsPublished, att1.IsPublished);
			AssertEquals(new byte[] { 1, 2, 3 }, att1.GetImageDataReader().ConvertToByteArrayAndCloseStream());

			var att3 = requestInNewFactory.DocManagerInfo.AllEDocs.OfType<IeDocBase>().Single(x => x.FileName == "att3.bin");
			AssertEquals(queue3_EDQ_IsPublished, att3.IsPublished);
			AssertEquals(new byte[] { 7, 8, 9 }, att3.GetImageDataReader().ConvertToByteArrayAndCloseStream());

			AssertEquals(0, TestConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.EdiERequestDocumentQueue WHERE EDQ_PK IN ('{queue1.PK}', '{queue2.PK}');"));
		}

		public void TestProcessNewWebRequestWithWorkflowTemplate()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			var reportingContact = lic1.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request1 = Factory.New<IncidentRequest>();
			request1.INC_OC_ReportedBy = reportingContact.PK;
			request1.INC_Criticality = "CR6";
			request1.INC_Summary = "new stuff";
			request1.INC_Details = "need new stuff";
			request1.INC_OC_ApprovedBy = reportingContact.PK;
			request1.INC_Type = "ENT";
			request1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request1.INC_SubType = "REF";
			request1.INC_Area = "Organisation";
			request1.INC_ProductLicence = "ENTCOMDB1";
			request1.INC_RN_NKCountry = "NZ";
			request1.INC_Language = "";
			Factory.Save();

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "AUT Feature Accepted";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "CLI";
			task11.P9_Description = "Auto Closed Feature Request";
			task11.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task11.P9_CompletedTimeUtc = ZDateTime.Now;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "INC";
			template2.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header2 = template2.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "INC Trigger";
			var trigger = template2.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test Trigger";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AccreditationAttemptCommencedCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "abc@email.com";
			action.PQ_EmailText = "Test email";
			Factory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllNewWebRequests();

			var supportIncident1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request1.PK));
			var trigger2 = supportIncident1.WorkflowItems.Triggers.Single() as ProcessTask;
			AssertEquals("Test Trigger", trigger2.P9_Description);
		}

		public void TestProcessNewWebRequest_JustUpdateClientCompanyWithLicenceCode()
		{
			var licBOR = BillingTestHelper.CreateBorderWiseLicence(Factory, "ENT", "COM", "BOR");
			var licCW = BillingTestHelper.CreateAnotherDatabase(licBOR, "PRD", false);
			var clientCompany1 = BillingTestHelper.CreateClientCompany(licCW.Database, "CO1");
			var clientCompany2 = BillingTestHelper.CreateClientCompany(licBOR.Database, "CO2");
			var clientCompany3 = BillingTestHelper.CreateClientCompany(licCW.Database, "CO3");
			var clientCompany4 = BillingTestHelper.CreateClientCompany(licBOR.Database, "CO4");
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			clientCompany2.LCC_RN_NKCountryCode = "AU";
			clientCompany3.LCC_RN_NKCountryCode = "US";
			clientCompany4.LCC_RN_NKCountryCode = "US";
			licCW.Database.LD_LicenceType = DatabaseTypes.Codes.Training;
			var reportingContact1 = licCW.Company.Header.Contacts.AddNew();
			reportingContact1.OC_ContactName = "Joe";
			reportingContact1.OC_Email = "joe@test.org";

			var requestCW1 = Factory.New<IncidentRequest>();
			requestCW1.INC_OC_ReportedBy = reportingContact1.PK;
			requestCW1.INC_Criticality = "CR5";
			requestCW1.INC_Details = "Details au";
			requestCW1.INC_OC_ApprovedBy = reportingContact1.PK;
			requestCW1.INC_Summary = "Summary au";
			requestCW1.INC_Type = ProductTypes.Codes.Enterprise;
			requestCW1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			requestCW1.INC_RN_NKCountry = "AU";

			var requestCW2 = Factory.New<IncidentRequest>();
			requestCW2.INC_OC_ReportedBy = reportingContact1.PK;
			requestCW2.INC_Criticality = "CR5";
			requestCW2.INC_Details = "Details us";
			requestCW2.INC_OC_ApprovedBy = reportingContact1.PK;
			requestCW2.INC_Summary = "Summary us";
			requestCW2.INC_Type = ProductTypes.Codes.Enterprise;
			requestCW2.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			requestCW2.INC_RN_NKCountry = "US";
			requestCW2.INC_ProductLicence = "ENTCO4BOR";

			Factory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);

			processor.ProcessAllNewWebRequests();

			var supportIncidentCW1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, requestCW1.PK));
			var supportIncidentCW2 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, requestCW2.PK));

			AssertEquals("Should be empty when INC_ProductLicence isn't set", "", supportIncidentCW1.ClientCompanyCode);
			AssertEquals("Should be empty when INC_ProductLicence isn't set", ZGuid.Empty, supportIncidentCW1.IM_LCC);
			AssertEquals("Should match the company when INC_ProductLicence is set", "CO4", supportIncidentCW2.ClientCompanyCode);
			AssertEquals("Should match the company when INC_ProductLicence is set", clientCompany4.PK, supportIncidentCW2.IM_LCC);
		}

		public void TestProcessAllNewWebMessages_SetLongDetailNoteText()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request = Factory.New<IncidentRequest>();
			request.INC_OC_ReportedBy = reportingContact.PK;
			request.INC_Criticality = "CR5";
			request.INC_Details = "how stuff happen?";
			request.INC_OC_ApprovedBy = reportingContact.PK;
			request.INC_Summary = "stuff happened";
			request.INC_Type = "ENT";
			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;

			var incident = Factory.New<SupportIncident>();
			request.INC_Details = "---START---" + new string('\n', incident.DetailNoteText_MaxLength - 20) + "---END---";
			SupportRequestProcessor.PopulateIncidentFromRequest(incident, request);
			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
			var convo = incident.EConversation.JobConversationForTest;

			var contactParticpant = convo.Participants.AddNew();
			contactParticpant.JCP_ParticipantTableCode = reportingContact.TablePrefix;
			contactParticpant.JCP_ParticipantID = reportingContact.PK;
			var staffParticpant = convo.Participants.AddNew();
			staffParticpant.JCP_ParticipantTableCode = GlbStaffSchema.Constants.Prefix;
			staffParticpant.JCP_ParticipantID = Env.CurrentUserPK;

			var legacyMsg1 = convo.Messages.AddNew(contactParticpant, "legacyMsg1", false);
			legacyMsg1.JCM_IsLocal = false;
			var legacy1 = Factory.New<EdiLegacyConversationMessage>();
			legacy1.ELC_JCM_Message = legacyMsg1.PK;

			var webMsg1 = convo.Messages.AddNew(contactParticpant, "webMsg1", false);
			webMsg1.JCM_IsLocal = false;

			var staffMsg1 = convo.Messages.AddNew(staffParticpant, "staffMsg1", false);
			staffMsg1.JCM_IsLocal = false;

			Factory.Save();

			AssertEquals("count of new remote messages in queue", 2, (int)Db.Connection.ExecuteScalar("select count(*) from dbo.EdiIncidentConversationMessageQueue"));

			LoggerForTest logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllNewWebMessages();

			AssertEquals("no messages left in queue", 0, (int)Db.Connection.ExecuteScalar("select count(*) from dbo.EdiIncidentConversationMessageQueue"));

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("webMsg1", email.Body);

			AssertEquals(incident.DetailNoteText_MaxLength, incident.DetailNoteText.Length);
			AssertEquals(true, incident.DetailNoteText.StartsWith("---START---"));
			AssertEquals(true, incident.DetailNoteText.EndsWith("Continued on the Notes (Extra Incident Detail)."));
			AssertNotNull(incident.Notes.GetAllNotes().OfType<StmNote>().Single(x => x.ST_Description.StartsWith("Extra Incident Detail") && x.ST_NoteText.EndsWith("---END---")));
		}

		SupportIncident CreateTestWebIncident(OrgContact reportingContact)
		{
			var request1 = Factory.New<IncidentRequest>();
			request1.INC_OC_ReportedBy = reportingContact.PK;
			request1.INC_Criticality = "CR5";
			request1.INC_Details = "how stuff happen?";
			request1.INC_OC_ApprovedBy = reportingContact.PK;
			request1.INC_Summary = "stuff happened";
			request1.INC_Type = "ENT";
			request1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request1.INC_SystemCreateUser = User.WebUserCode;

			var incident = Factory.New<SupportIncident>();
			SupportRequestProcessor.PopulateIncidentFromRequest(incident, request1);
			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
			return incident;
		}

		void AddMessageFromContact(SupportIncident incident, string msg)
		{
			var convo = incident.EConversation.JobConversationForTest;
			var reportingContact = incident.Contact;

			var contactParticpant = convo.Participants.FirstOrDefault(x => (x.Parent as OrgContact) == reportingContact);
			if (contactParticpant == null)
			{
				contactParticpant = convo.Participants.AddNew();
				contactParticpant.JCP_ParticipantTableCode = reportingContact.TablePrefix;
				contactParticpant.JCP_ParticipantID = reportingContact.PK;
			}

			var webMsg1 = convo.Messages.AddNew(contactParticpant, msg, false);
			webMsg1.JCM_IsLocal = false;
		}

		public void TestProcessAllNewWebMessages_AutomaticAutoReply()
		{
			var code1 = "OZZ";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var config1 = registryValue[0].IncidentGroupStatusConfigurations.AddNew(code1, "num1", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			config1.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var incident1 = Factory.New<SupportIncident>();
			incident1.IM_IncidentNumber = "IM00000011";
			incident1.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;

			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = "OZZ";
			group.ING_Priority = CriticalityConstants.CR3_SingleFunctionNoWorkAround;
			group.ING_IsAutoReply = true;
			group.NowStage.ControlIncidents = true;
			var groupMessages = group.IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>(); // need call the collection to create default messages
			var autoReplay = groupMessages.First(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
			autoReplay.IGM_Message = "Test auto reply message";
			autoReplay.IGM_IsPublished = true;

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_ING_Group = group.PK;
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_IsGroupControlled = true;

			var convo1 = incident1.EConversation.JobConversationForTest;
			var contactParticpant1 = convo1.Participants.AddNew();
			contactParticpant1.JCP_ParticipantTableCode = reportingContact.TablePrefix;
			contactParticpant1.JCP_ParticipantID = reportingContact.PK;
			var staffParticpant = convo1.Participants.AddNew();
			staffParticpant.JCP_ParticipantTableCode = GlbStaffSchema.Constants.Prefix;
			staffParticpant.JCP_ParticipantID = Env.CurrentUserPK;
			var webMsg1 = convo1.Messages.AddNew(contactParticpant1, "webMsg1", false);
			webMsg1.JCM_IsLocal = false;
			var staffMsg1 = convo1.Messages.AddNew(staffParticpant, "staffMsg1", false);
			staffMsg1.JCM_IsLocal = false;

			var convo2 = incident2.EConversation.JobConversationForTest;
			var contactParticpant2 = convo2.Participants.AddNew();
			contactParticpant2.JCP_ParticipantTableCode = reportingContact.TablePrefix;
			contactParticpant2.JCP_ParticipantID = reportingContact.PK;
			var webMsg2 = convo2.Messages.AddNew(contactParticpant2, "webMsg2", false);
			webMsg2.JCM_IsLocal = false;

			Factory.Save();

			AssertEquals("Pre-condition", true, incident1.IsGroupControlled);
			AssertEquals("Should not contain auto-reply message", false, convo1.Messages.Any(m => m.Body.Contains("Test auto reply message")));
			AssertEquals("Should not contain auto-reply message", false, convo2.Messages.Any(m => m.Body.Contains("Test auto reply message")));

			var processor = new SupportRequestProcessor(new LoggerForTest());
			processor.ProcessAllNewWebMessages();

			var newFactory = new BusinessObjectFactory();
			var loadedIncident1 = newFactory.Load<SupportIncident>(incident1.PK);
			var loadedIncident2 = newFactory.Load<SupportIncident>(incident2.PK);

			AssertEquals("Should contain auto-reply message", true, loadedIncident1.EConversation.JobConversationForTest.Messages.Any(m => m.Body.Contains("Test auto reply message")));
			AssertEquals("Should not contain auto-reply message because it isn't attached to a group", false, loadedIncident2.EConversation.JobConversationForTest.Messages.Any(m => m.Body.Contains("Test auto reply message")));
		}

		public void TestProcessAllWebStatusUpdates_CriticalityChangeEConversationAndEventLog()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var incident1 = CreateTestWebIncident(reportingContact);
			incident1.IM_Priority = "CR6";
			var incident2 = CreateTestWebIncident(reportingContact);
			var incident3 = CreateTestWebIncident(reportingContact);
			incident3.IM_Priority = "CR5";
			incident1.CloseAsAcceptedFeatureRequest("foo");
			incident2.CloseAsAcceptedFeatureRequest("bar");

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var webFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncidentRequest1 = webFactory.Load<IncidentRequest>(incident1.IM_INC_Request);
			loadedIncidentRequest1.INC_Criticality = "CR7";
			loadedIncidentRequest1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.DevelopmentEstimateRequested;
			var loadedIncidentRequest3 = webFactory.Load<IncidentRequest>(incident3.IM_INC_Request);
			loadedIncidentRequest3.INC_Criticality = "CR7";
			loadedIncidentRequest3.INC_Status = SupportIncidentLookups.LegacyStatusCodes.DevelopmentEstimateRequested;
			webFactory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllWebStatusUpdates();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident1 = factory2.Load<SupportIncident>(incident1.PK);
			var loadedIncident2 = factory2.Load<SupportIncident>(incident2.PK);
			var loadedIncident3 = factory2.Load<SupportIncident>(incident3.PK);

			AssertEquals("Should have criticality change message",
				true,
				loadedIncident1.EConversation.GetTimeOrderedMessages().Any(m => m.Body.Contains("Criticality changed from CR6 to CR7")));

			AssertEquals("Should not have criticality change message",
				false,
				loadedIncident2.EConversation.GetTimeOrderedMessages().Any(m => m.Body.Contains("Criticality changed from")));

			AssertEquals("Should have criticality change message",
				true,
				loadedIncident3.EConversation.GetTimeOrderedMessages().Any(m => m.Body.Contains("Criticality changed from CR5 to CR7")));
		}

		public void TestProcessAllWebStatusUpdates_RequestDevelopmentEstimate_ShouldHaveUpdateEmail()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var incident1 = CreateTestWebIncident(reportingContact);
			incident1.IM_Priority = "CR6";
			incident1.CloseAsAcceptedFeatureRequest("foo");

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var webFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncidentRequest1 = webFactory.Load<IncidentRequest>(incident1.IM_INC_Request);
			loadedIncidentRequest1.INC_Criticality = "CR7";
			loadedIncidentRequest1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.DevelopmentEstimateRequested;
			webFactory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllWebStatusUpdates();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident1 = factory2.Load<SupportIncident>(incident1.PK);

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[1];
			AssertEquals("Customer Service", email.FromDisplayName);
			AssertEquals("support@wisetechglobal.com", email.FromAddress);
			AssertContains("Criticality has been updated from CR6 (Feature Request) to CR7 (Estimate / Quote Request)", email.Body);
		}

		public void TestProcessAllWebStatusUpdates_DeemedResolved()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "COM");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "SRS");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "ABC");

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "archie";
			reportingContact.OC_Email = "archie@test.org";

			var incident = CreateTestWebIncident(reportingContact);
			incident.IM_Category = "DEF";
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			Factory.Save();

			var webFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncidentRequest = webFactory.Load<IncidentRequest>(incident.IM_INC_Request);

			loadedIncidentRequest.INC_Status = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			var reference = "This eRequest has been deemed resolved";
			loadedIncidentRequest.Logs.AddNew(AutoEvents.MiscellaneousEvent, reference);
			webFactory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllWebStatusUpdates();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident = factory.Load<SupportIncident>(incident.PK);
			AssertEquals(loadedIncident.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed);
		}

		public void TestProcessAllWebStatusUpdates_DeemedResolved_ShouldAddStuEventLog()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, isResolution: ZBool.True));
			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
				var reportingContact = lic.Company.Header.Contacts.AddNew();
				reportingContact.OC_ContactName = "archie";
				reportingContact.OC_Email = "archie@test.org";

				var incident = CreateTestWebIncident(reportingContact);
				incident.IM_Priority = "CR4";
				Factory.Save();

				incident.CloseIncident("ZZZ", "test");
				Factory.Save();

				AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, incident.IM_ResolutionCode);

				var webFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var loadedIncidentRequest = webFactory.Load<IncidentRequest>(incident.IM_INC_Request);

				loadedIncidentRequest.INC_Status = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
				var reference = "This eRequest has been deemed resolved";
				loadedIncidentRequest.Logs.AddNew(AutoEvents.MiscellaneousEvent, reference);
				webFactory.Save();

				var logger = new LoggerForTest();
				var processor = new SupportRequestProcessor(logger);
				processor.ProcessAllWebStatusUpdates();

				var factory = new BusinessObjectFactory() { RefreshEnabled = false };
				var loadedIncident = factory.Load<SupportIncident>(incident.PK);
				var loadedIncidentLogs = loadedIncident.Logs.GetAllLogs().Cast<StmALog>();
				AssertEquals(1, loadedIncidentLogs.Count(x => x.SL_SE_NKEvent == "STU" && x.SL_Reference.Contains("|NEW=CLS|OLD=SLV")));
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedIncident.IM_ResolutionCode);
			}
		}

		[TestDate(2023, 01, 01)]
		public void TestProcessAllWebStatusUpdates_AwaitingClientResponseDeemedResolved()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "COM");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "SRS");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "ABC");

			var srsNode = tree.Find("DEF", "CR4", "ENT", "SRS");
			Assert("Precondition: ", srsNode.Bool);

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "archie";
			reportingContact.OC_Email = "archie@test.org";

			var incident = CreateTestWebIncident(reportingContact);
			incident.IM_Category = "DEF";
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty, true);
			Factory.Save();

			TestDateAttribute.AddDays(1);

			var webFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncidentRequest = webFactory.Load<IncidentRequest>(incident.IM_INC_Request);

			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.ClosedAwaitingResponse, loadedIncidentRequest.INC_Status);
			loadedIncidentRequest.INC_Status = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			var reference = "This eRequest has been deemed resolved";
			loadedIncidentRequest.Logs.AddNew(AutoEvents.MiscellaneousEvent, reference);
			webFactory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllWebStatusUpdates();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident = factory.Load<SupportIncident>(incident.PK);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedIncident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, loadedIncident.IM_ClosureResolution);
			AssertEquals("Resolve should set resolved time which should match the IWR event (which is added when we close as awaiting client response)", new ZDateTime(2023, 01, 01), loadedIncident.IM_ResolveTimeUtc);

			var resolvedEvent = loadedIncident.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_EventTime).FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);
			AssertNotNull("Should have an event", resolvedEvent);
			AssertEquals("Event time should match the IWR event (which is added when we close as awaiting client response)", new ZDateTime(2023, 01, 01), resolvedEvent.SL_EventTimeUtc);
			AssertEquals(true, loadedIncident.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_EventTime).Any(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode));
		}

		[TestDate(2023, 01, 01)]
		public void TestProcessAllWebStatusUpdates_AwaitingClientResponseDeemedResolved_SRSNotValid()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "COM");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "SRS");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "ABC");

			var inactiveSRSNode = tree.Find("DEF", "CR4", "ENT", "SRS");
			inactiveSRSNode.Bool = false;

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "archie";
			reportingContact.OC_Email = "archie@test.org";

			var incident = CreateTestWebIncident(reportingContact);
			incident.IM_Category = "DEF";
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty, true);
			Factory.Save();

			TestDateAttribute.AddDays(1);

			var webFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncidentRequest = webFactory.Load<IncidentRequest>(incident.IM_INC_Request);

			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.ClosedAwaitingResponse, loadedIncidentRequest.INC_Status);
			loadedIncidentRequest.INC_Status = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			var reference = "This eRequest has been deemed resolved";
			loadedIncidentRequest.Logs.AddNew(AutoEvents.MiscellaneousEvent, reference);
			webFactory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllWebStatusUpdates();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident = factory.Load<SupportIncident>(incident.PK);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedIncident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, loadedIncident.IM_ClosureResolution);
			AssertEquals("Resolve should set resolved time which should match the IWR event (which is added when we close as awaiting client response)", new ZDateTime(2023, 01, 01), loadedIncident.IM_ResolveTimeUtc);

			var resolvedEvent = loadedIncident.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_EventTime).FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);
			AssertNotNull("Should have an event", resolvedEvent);
			AssertEquals("Event time should match the IWR event (which is added when we close as awaiting client response)", new ZDateTime(2023, 01, 01), resolvedEvent.SL_EventTimeUtc);
			AssertEquals(true, loadedIncident.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_EventTime).Any(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode));
		}

		public void TestProcessAllNewWebMessages_SendClosureEmail()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "COM");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "SRS");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR4", "ENT", "ABC");

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "archie";
			reportingContact.OC_Email = "archie@test.org";

			var incident = CreateTestWebIncident(reportingContact);
			incident.IM_Category = "DEF";
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			incident.IM_IncidentNumber = "CS99000011";
			var incident2 = CreateTestWebIncident(reportingContact);
			incident2.IM_Category = "DEF";
			incident2.IM_Priority = "CR4";
			incident2.IM_Product = "ENT";
			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.CsStageDataFix;
			incident2.IM_IncidentNumber = "CS99000022";
			Factory.Save();

			var webFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var reference = "This eRequest has been deemed resolved";

			var loadedIncidentRequest = webFactory.Load<IncidentRequest>(incident.IM_INC_Request);
			loadedIncidentRequest.INC_Status = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			loadedIncidentRequest.Logs.AddNew(AutoEvents.MiscellaneousEvent, reference);

			var loadedIncidentRequest2 = webFactory.Load<IncidentRequest>(incident2.IM_INC_Request);
			loadedIncidentRequest2.INC_Status = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			loadedIncidentRequest2.Logs.AddNew(AutoEvents.MiscellaneousEvent, reference);

			webFactory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllWebStatusUpdates();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Customer Service", email.FromDisplayName);
			AssertEquals("support@wisetechglobal.com", email.FromAddress);
			Assert(email.Subject.Contains("CS99000011"));
		}

		public void TestProcessAllNewWebMessages_SendIncidentRaisedEmailWhenSubmit()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var orgHeader = lic.Company.Header;
			orgHeader.MainAddress.OA_Email = "orgHeader@test.org";
			var reportingContact = orgHeader.Contacts.AddNew();
			reportingContact.OC_ContactName = "Joe";
			reportingContact.OC_Email = "joe@test.org";

			var request1 = Factory.New<IncidentRequest>();
			request1.INC_IncidentNumber = "CS01191909";
			request1.INC_OC_ReportedBy = reportingContact.PK;
			request1.INC_Criticality = "CR5";
			request1.INC_Details = "how stuff happen?";
			request1.INC_OC_ApprovedBy = reportingContact.PK;
			request1.INC_Summary = "stuff happened";
			request1.INC_Type = "ENT";
			request1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
			request1.INC_SystemCreateUser = User.WebUserCode;

			var jobConversation = Factory.New<JobConversation>();
			jobConversation.JCC_ParentTableCode = "INC";
			jobConversation.JCC_ParentID = request1.PK;

			var jobConversationParticipant = Factory.New<JobConversationParticipant>();
			jobConversationParticipant.JCP_JCC_Conversation = jobConversation.PK;
			jobConversationParticipant.JCP_IsSubscribed = true;
			jobConversationParticipant.JCP_EmailAddress = "archie@test.com";

			var jobConversationOrgParticipant = Factory.New<JobConversationParticipant>();
			jobConversationOrgParticipant.JCP_JCC_Conversation = jobConversation.PK;
			jobConversationOrgParticipant.JCP_IsSubscribed = true;
			jobConversationOrgParticipant.JCP_ParticipantTableCode = "OH";
			jobConversationOrgParticipant.JCP_ParticipantID = orgHeader.PK;

			Env.OutgoingMailManager.EmailsCreated.Clear();
			Factory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllNewWebRequests();

			var supportIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request1.PK));
			AssertNotNull(supportIncident);
			Assert(supportIncident.EConversation.ExistingConversation.Participants.Any(x => x.EmailAddress == "archie@test.com"));
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var newIncidentRaisedEmail = Env.OutgoingMailManager.EmailsCreated[0];
			Assert(newIncidentRaisedEmail.Subject.Contains("Customer Service Incident Raised"));
			AssertEquals("Recipients count should be 1", 1, newIncidentRaisedEmail.Recipients.Count);
			AssertEquals("Recipients should be joe@test.org", "joe@test.org", newIncidentRaisedEmail.Recipients[0]);
			AssertEquals("CCRecipients count should be 1", 1, newIncidentRaisedEmail.CCRecipients.Count);
			AssertEquals("Recipients should be orgHeader@test.org", "orgHeader@test.org", newIncidentRaisedEmail.CCRecipients[0]);
		}

		public void TestProcessAllWebStatusUpdatesShouldAddSystemMessageWhenConfirmResolvedFromERequestPortal()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "archie";
			reportingContact.OC_Email = "archie@test.org";

			var incident = CreateTestWebIncident(reportingContact);
			incident.IM_Category = "DEF";
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			incident.IM_IncidentNumber = "CS99000011";

			Factory.Save();

			var reference = "This eRequest has been deemed resolved - eRequest Management Portal";

			var loadedIncidentRequest = Factory.Load<IncidentRequest>(incident.IM_INC_Request);
			loadedIncidentRequest.INC_Status = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			loadedIncidentRequest.Logs.AddNew(AutoEvents.MiscellaneousEvent, reference);

			Factory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllWebStatusUpdates();

			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident = factory.Load<SupportIncident>(incident.PK);

			Assert("The system message should be post", loadedIncident.EConversation.Conversation.Messages.Any(x => x.Body.Equals("This eRequest was closed via the confirm resolved action button on the eRequest Management Portal") && x.JCM_IsSystem));
		}

		public void TestProcessAllWebStatusUpdatesShouldAddSystemMessageWhenConfirmResolvedFromEMail()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var reportingContact = lic.Company.Header.Contacts.AddNew();
			reportingContact.OC_ContactName = "archie";
			reportingContact.OC_Email = "archie@test.org";

			var incident = CreateTestWebIncident(reportingContact);
			incident.IM_Category = "DEF";
			incident.IM_Priority = "CR4";
			incident.IM_Product = "ENT";
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			incident.IM_IncidentNumber = "CS99000011";

			Factory.Save();

			var reference = "This eRequest has been deemed resolved - Email";

			var loadedIncidentRequest = Factory.Load<IncidentRequest>(incident.IM_INC_Request);
			loadedIncidentRequest.INC_Status = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			loadedIncidentRequest.Logs.AddNew(AutoEvents.MiscellaneousEvent, reference);

			Factory.Save();

			var logger = new LoggerForTest();
			var processor = new SupportRequestProcessor(logger);
			processor.ProcessAllWebStatusUpdates();

			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident = factory.Load<SupportIncident>(incident.PK);

			Assert("The system message should be post", loadedIncident.EConversation.Conversation.Messages.Any(x => x.Body.Equals("This eRequest was closed via the confirm resolved action button on email") && x.JCM_IsSystem));
		}

		#region Implementation

		string SetupRequest()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.Action = SupportIncidentLookups.LegacyActions.Add;
			request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
			request.IncidentSummary = "My Incident Summary";
			request.IncidentDetails = "My Incident Details\r\nLine two.";
			request.ReportingStaffMemberName = "Zarn Bou";
			request.ApprovingUser = "Zarn Bou";
			request.Criticality = "CR1";

			Xsd.OrgContact contact1 = request.Staff.AddNew();
			contact1.Name = "Zarn Bou";
			contact1.EmailAddress = "zarn@test.com";

			Xsd.OrgContact contact2 = request.Staff.AddNew();
			contact2.Name = "Someone";

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));

			string xmlData = "";
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, request);
				xmlData = writer.ToString();
			}
			return xmlData;
		}

		void AssertCriticalSupportRequestEmail(bool isCritical = false, bool isForWebRequest = false)
		{
			Type t = typeof(CriticalityConstants);
			FieldInfo[] fileds = t.GetFields();
			List<string> criticalityConstants = new List<string>();
			foreach (var fieldInfo in fileds)
			{
				var codeValue = Convert.ToString(fieldInfo.GetRawConstantValue());
				if (isCritical)
				{
					if (!(codeValue == CriticalityConstants.CR1_SystemDown || codeValue == CriticalityConstants.CR2_ModuleDown || codeValue == CriticalityConstants.CR3_SingleFunctionNoWorkAround))
					{
						continue;
					}
				}
				else
				{
					if (codeValue == CriticalityConstants.CR1_SystemDown || codeValue == CriticalityConstants.CR2_ModuleDown || codeValue == CriticalityConstants.CR3_SingleFunctionNoWorkAround)
					{
						continue;
					}
				}

				criticalityConstants.Add(codeValue);
			}

			AssertCriticalSupportRequestEmail(criticalityConstants, isForWebRequest, isCritical);
		}

		void AssertCriticalSupportRequestEmail(IEnumerable<string> criticalityConstants, bool isForWebRequest = false, bool isCritical = false)
		{
			InitializationCriticalSupportRequestEmail(isForWebRequest);

			if (!criticalityConstants.IsNullOrEmpty())
			{
				foreach (var code in criticalityConstants)
				{
					AssertCriticalSupportRequestEmail(code, isForWebRequest, isCritical);
				}
			}
		}

		OrgContact reportingContact;

		void InitializationCriticalSupportRequestEmail(bool isForWebRequest = false)
		{
			if (isForWebRequest)
			{
				var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
				lic1.Company.Header.OH_FullName = "Some Organisation";
				reportingContact = lic1.Company.Header.Contacts.AddNew();
				reportingContact.OC_ContactName = "Zarn Bou";
				reportingContact.OC_Email = "joe@test.org";

				var group = Factory.New<GlbGroup>();
				var staff = group.Staff.AddNew();
				staff.GS_EmailAddress = "test@test.com";
				staff.GS_Code = "ZAC";
				Factory.Save();

				EDIDataRegistry.Instance.CustomerServiceHighCriticalityIncidentGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			}
			else
			{
				GlbGroup group = Factory.New<GlbGroup>();
				GlbStaff staff = group.Staff.AddNew();
				staff.GS_EmailAddress = "test@test.com";
				staff.GS_Code = "ZAC";
				Factory.Save();
				EDIDataRegistry.Instance.CustomerServiceHighCriticalityIncidentGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			}
		}

		void AssertPrepareCriticalSupportRequestEmail(string criticality, bool isForWebRequest = false)
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			if (isForWebRequest)
			{
				var request1 = Factory.New<IncidentRequest>();
				request1.INC_OC_ReportedBy = reportingContact.PK;
				request1.INC_Criticality = criticality;
				request1.INC_Summary = "My Incident Summary";
				request1.INC_Details = "My Incident Details\r\nLine two.";
				request1.INC_OC_ApprovedBy = reportingContact.PK;
				request1.INC_Type = "ENT";
				request1.INC_Status = SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent;
				request1.INC_SubType = "REF";
				request1.INC_Area = "Organisation";
				request1.INC_ProductLicence = "ENTCOMDB1";
				request1.INC_Type = ProductTypes.Codes.CargoWiseOne;

				Factory.Save();

				LoggerForTest logger = new LoggerForTest();
				var processor = new SupportRequestProcessor(logger);

				processor.ProcessAllNewWebRequests();

				var supportIncident1 = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request1.PK));
				AssertEquals(criticality, supportIncident1.IM_Priority);
			}
			else
			{
				Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
				request.Action = SupportIncidentLookups.LegacyActions.Add;
				request.LicenceCode = Licence.Database.LicEnterprise.LE_EnterpriseCode + Licence.Company.LC_CompanyCode + Licence.Database.LD_ServerCode;
				request.IncidentSummary = "My Incident Summary";
				request.IncidentDetails = "My Incident Details\r\nLine two.";
				request.ReportingStaffMemberName = "Zarn Bou";
				request.ApprovingUser = "Zarn Bou";
				request.Criticality = criticality;
				request.Product = ProductTypes.Codes.CargoWiseOne;

				Factory.Save();

				Xsd.OrgContact contact1 = request.Staff.AddNew();
				contact1.Name = "Zarn Bou";
				contact1.EmailAddress = "zarn@test.com";

				Xsd.OrgContact contact2 = request.Staff.AddNew();
				contact2.Name = "Someone";

				ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));

				string xmlData = "";
				using (StringWriter writer = new StringWriter())
				{
					serializer.Serialize(writer, request);
					xmlData = writer.ToString();
				}

				SupportRequestProcessor processor = new SupportRequestProcessor();
				AssertNull(processor.Incident);
				processor.Process(xmlData);
				AssertNotNull(processor.Incident);
			}
		}

		void AssertCriticalSupportRequestEmail(string criticality, bool isForWebRequest = false, bool isCritical = false)
		{
			AssertPrepareCriticalSupportRequestEmail(criticality, isForWebRequest);

			if (isCritical)
			{
				AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);

				var email = Env.OutgoingMailManager.EmailsCreated[1];
				AssertEquals("Customer Service", email.FromDisplayName);
				AssertEquals("support@wisetechglobal.com", email.FromAddress);
				AssertEquals("[" + ProductTypes.Codes.CargoWiseOne + "] A " + criticality + " incident raised by client Some Organisation", email.Subject);
				AssertContains("Zarn Bou raised a " + criticality + " incident (" + ProductTypes.Codes.CargoWiseOne + ") for client: Some Organisation", email.Body);
				AssertContains("My Incident Summary", email.Body);
				AssertContains("My Incident Details\r\nLine two.", email.Body);
				AssertContains("<a href=\"edient:Command=ShowEditForm&ControllerID=SupportIncident&BusinessEntityPK=", email.Body);

				var productDescription = IncidentDetailsLookupsHelper.ProductList.GetDescriptionFromCode(ProductTypes.Codes.CargoWiseOne);
				AssertContains("Product : " + productDescription, email.Body);
			}
			else
			{
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		OrgHeader Org
		{
			get
			{
				if (fOrg == null)
				{
					fOrg = Factory.New<OrgHeader>();
					fOrg.OH_FullName = "Some Organisation";
					fOrg.OH_Code = "MYCLIENT";

					OrgContact contact = fOrg.Contacts.AddNew();
					contact.OC_ContactName = "Zarn Bou";
					contact.IsCustomerServiceContact = true;

					OrgContact contact2 = fOrg.Contacts.AddNew();
					contact2.OC_ContactName = "Zubin Appoo";
					contact2.IsCustomerServiceContact = true;

					OrgContact contact3 = fOrg.Contacts.AddNew();
					contact3.OC_ContactName = "John Smith";

					OrgContact contact4 = fOrg.Contacts.AddNew();
					contact4.OC_ContactName = "Samuel Wang";
					contact4.IsCustomerServiceContact = true;
				}
				return fOrg;
			}
		}

		OrgHeader fOrg;

		LicenceHeader Licence
		{
			get
			{
				if (licence == null)
				{
					LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
					enterprise.LE_EnterpriseCode = "ZUB";
					enterprise.LE_OH = Org.PK;

					LicenceCompany company = Factory.New<LicenceCompany>();
					company.LC_CompanyCode = "RAK";
					company.LC_OH = Org.PK;
					company.LC_LE = enterprise.PK;

					LicenceDatabase database = Factory.New<LicenceDatabase>();
					database.LD_ServerCode = "LOL";
					database.LD_LE = enterprise.PK;
					database.LD_PublicEmailAddressForUpdate = "someone@somewhere.com";
					database.LD_PublicEmailAddressForUpdate = "test@test.com";

					licence = Factory.New<LicenceHeader>();
					licence.LA_LC = company.PK;
					licence.LA_LD = database.PK;

					var clientCompany = Factory.New<ClientCompany>();
					clientCompany.LCC_Code = company.LC_CompanyCode;
					clientCompany.LCC_OH = Org.PK;
					clientCompany.LCC_LD = database.PK;

					Factory.Save();
				}

				return licence;
			}
		}

		LicenceHeader licence;

		protected override void SetUp()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();

			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "INC");

			base.SetUp();
		}

		#endregion

		// Uncommment this to manually process a support request attachment
		//public void TestBadEmail()
		//{
		//    string path = @"c:\work\Incident Details.zip";
		//    byte [] data = File.ReadAllBytes(path);
		//    System.Text.StringBuilder sb = new System.Text.StringBuilder();
		//    new AttachmentTextAppender().Append("Incident Details.zip", data, sb);
		//    string xml = sb.ToString();
		//    var p = new SupportRequestProcessor();
		//    p.Process(xml);
		//}
	}
}
