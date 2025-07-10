using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class ERequestEHubCustomerNotificationSenderTest : TestCaseWithFactory
	{
		public void TestMacroLinkIsPrependedToClientEmail()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithERequestV2();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			Factory.Save();

			var email = SupportIncidentEmail.New(incident);
			email.Subject = "client email subject";
			email.Body = "This is the client email body";

			var sender = new ERequestEHubCustomerNotificationSender();
			sender.AddHyperlinks(email);
			AssertEquals("client email subject", email.Subject);
			AssertContains("<a href='(*ServiceRequestLink*)'>Incident number CS00000001 / SR00001002</a>", email.Body);
			AssertContains("This is the client email body", email.Body);
		}

		public void TestMacroLinkIsPrependedToClientEmail_LegacyClient()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLegacyClient();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			Factory.Save();

			var email = SupportIncidentEmail.New(incident);
			email.Subject = "client email subject";
			email.Body = "This is the client email body";

			var sender = new ERequestEHubCustomerNotificationSender();
			sender.AddHyperlinks(email);
			AssertEquals("client email subject", email.Subject);
			AssertContains("This is the client email body", email.Body);
			AssertNotContains("Should not include macro link for legacy clients", "(*ServiceRequestLink*)", email.Body);
		}

		public void TestSend()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			var db = org.LicCompany.LicDatabases.AddNew();
			db.LD_ServerCode = "PRD";
			db.LD_PublicEmailAddressForUpdate = "test@test.com";
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = db.PK;
			clientCompany.LCC_Code = org.LicCompany.LC_CompanyCode;
			Factory.Save();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "IM12345";
			incident.IM_Description = "Test Incident";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = IncidentMainLookups.Status.Open;
			incident.IM_LCC = clientCompany.PK;

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.ClientReferenceNumber = "CL111";

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse));
			byte[] content;
			using (MemoryStream xmlStream = new MemoryStream())
			{
				serializer.Serialize(xmlStream, response);
				content = xmlStream.ToArray();
			}

			var sender = new ERequestEHubCustomerNotificationSenderForTest();
			sender.Send_Exposed(incident, response);

			AssertEquals("CreateSecureCalls", 1, DebugOnlyOutgoingSystemMessage.CreateSecureCalls);
			AssertEquals(incident.ClientCompany.LicenceCode, DebugOnlyOutgoingSystemMessage.RecipientId);
			AssertEquals("MessageName", SystemMessageList.Descriptions.CustomerServiceResponse, DebugOnlyOutgoingSystemMessage.MessageName);
			AssertArrayEqualsByElements("MessageStream", content, DebugOnlyOutgoingSystemMessage.MessageStream);

			DebugOnlyOutgoingSystemMessage.Initialize();
		}

		#region Send

		public void TestIsBiDirectionResponseSupported()
		{
			var bidirectionalIncident = new SupportIncidentTestHelper(Factory).CreateIncidentWithBidirectionalUpdateClient();

			bidirectionalIncident.IM_ClientIncidentReference = ZString.Empty;
			bidirectionalIncident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			AssertEquals(true, ERequestEHubCustomerNotificationSender.IsBiDirectionResponseSupported(bidirectionalIncident));

			bidirectionalIncident.IM_ClientIncidentReference = "XXX";
			bidirectionalIncident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			AssertEquals(true, ERequestEHubCustomerNotificationSender.IsBiDirectionResponseSupported(bidirectionalIncident));

			bidirectionalIncident.IM_ClientIncidentReference = ZString.Empty;
			bidirectionalIncident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			AssertEquals(true, ERequestEHubCustomerNotificationSender.IsBiDirectionResponseSupported(bidirectionalIncident));

			bidirectionalIncident.IM_ClientIncidentReference = "XXX";
			bidirectionalIncident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			AssertEquals(true, ERequestEHubCustomerNotificationSender.IsBiDirectionResponseSupported(bidirectionalIncident));

			var legacyIncident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLegacyClient();

			legacyIncident.IM_ClientIncidentReference = ZString.Empty;
			legacyIncident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			AssertEquals(false, ERequestEHubCustomerNotificationSender.IsBiDirectionResponseSupported(legacyIncident));

			legacyIncident.IM_ClientIncidentReference = "XXX";
			legacyIncident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			AssertEquals(false, ERequestEHubCustomerNotificationSender.IsBiDirectionResponseSupported(legacyIncident));

			legacyIncident.IM_ClientIncidentReference = ZString.Empty;
			legacyIncident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			AssertEquals(false, ERequestEHubCustomerNotificationSender.IsBiDirectionResponseSupported(legacyIncident));

			legacyIncident.IM_ClientIncidentReference = "XXX";
			legacyIncident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			AssertEquals(false, ERequestEHubCustomerNotificationSender.IsBiDirectionResponseSupported(legacyIncident));
		}

		public void TestCreateSenderIfSupported()
		{
			ReleaseBuilds.Business.ReleaseBuild build = Factory.New<ReleaseBuilds.Business.ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);

			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();

			var db = org.LicCompany.LicDatabases.AddNew();
			db.LD_ServerCode = "PRD";

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = org.LicCompany.LC_CompanyCode;
			clientCompany.LCC_LD = db.PK;

			Factory.Save();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "IM12345";
			incident.IM_Description = "Test Incident";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = IncidentMainLookups.Status.Open;
			incident.IM_LD = db.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_ClientIncidentReference = "CR0001";

			var sender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident);
			AssertEquals("sender when client version unknown", nameof(ERequestEmailCustomerNotificationSender), sender.GetType().Name);

			db.LD_HL_CurrentRunningVersion = build.PK;
			sender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident);
			AssertEquals("sender when client version too old", nameof(ERequestEmailCustomerNotificationSender), sender.GetType().Name);

			build.HL_Release = LicenceDatabase.FirstReleaseSystemMessageCSR;
			sender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident);
			AssertEquals("sender when client version recent", nameof(ERequestEHubCustomerNotificationSender), sender.GetType().Name);
		}

		[TestDate(2013, 10, 17, 14, 0, 0)]
		[TestUtcOffset(0, 0, 0)]
		public void TestCalculateEHubMessageSentTimeInUtc()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			staff.GS_FullName = "Samuel";

			var oldIncidentFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var oldIncident = oldIncidentFactory.NewWithValidTestData<SupportIncidentForTest>();
			SetupIncidentForCustomerSystemNotificationTest(oldIncident);
			oldIncidentFactory.Save();
			oldIncident.EConversation.JobConversationForTest.Messages.DeleteAll();

			oldIncident.SetLogTextForTest(
@"15/07/2006 22:51:48 ~BP - Old incident log will be converted to eConversation.
----------------------------------------------------------------------------------------------------------------------
22/06/2005 4:45:12 PM G - This convert should not cause exception when calculating latest eConversation sent time.
----------------------------------------------------------------------------------------------------------------------
");
			oldIncidentFactory.Save();

			var incident = Factory.Load<SupportIncidentForTest>(oldIncident.PK);
			AssertEquals(2, incident.EConversation.JobConversationForTest.Messages.Count);
			AssertEquals("Old incident log will be converted to eConversation.", incident.EConversation.GetTimeOrderedMessages()[0].Body);
			AssertEquals("This convert should not cause exception when calculating latest eConversation sent time.", incident.EConversation.GetTimeOrderedMessages()[1].Body);

			incident.EConversation.AddMessageFromCurrentUser("first", true, false);
			var sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);
			Factory.Save();

			incident.IM_Priority = "CR1";
			Factory.Save();
			AssertEquals("2013-10-17T14:00:00.0000000Z", sender.LastSentResponse.SentTimeUtc);

			TestDateAttribute.Date = new DateTime(2013, 10, 17, 14, 0, 5);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				incident.EConversation.AddMessageFromCurrentUser("Test 1", isInternal: false, isSystem: false, kind: SupportIncidentEConversation.LocalMessageKind.ForCustomer);
				var msg = incident.EConversation.LastAddedMessageForTest;
				msg.JCM_PostedTimeUtc = new ZDateTime(2013, 10, 17, 14, 0, 4);
				Factory.Save();
			}
			AssertEquals("since the incident autolog is disabled,should be conversation last added msg time", "2013-10-17T14:00:04.0000000Z", sender.LastSentResponse.SentTimeUtc);

			TestDateAttribute.Date = new DateTime(2013, 10, 17, 14, 0, 6);
			var file = (StorageFile)incident.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1, 1 }, "file.pdf", "MSC");
			file.SC_IsPublished = true;

			using (var mem = new MemoryStream())
			{
				(new System.Drawing.Bitmap(1, 1)).Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
				var document = (StorageDocs)incident.DocManagerInfo.AddFileOrDocument(mem.ToArray(), "screenshot.jpg", "MSC");
				document.SC_IsPublished = false;
			}

			EventHandler eDocsHandler = (s, e) =>
			{
				TestDateAttribute.Date = new DateTime(2013, 10, 17, 14, 0, 10);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				var log = file.Logs.AddNew(ZArchitecture.Business.Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				log.Factory.Save();
				TestDateAttribute.Date = new DateTime(2013, 10, 17, 14, 0, 6);
			};
			incident.BeforeSendEHubMessage += eDocsHandler;
			incident.IM_Priority = "CR2";
			Factory.Save();
			AssertEquals("Should be the most recent log time of eDocs", "2013-10-17T14:00:10.0000000Z", sender.LastSentResponse.SentTimeUtc);
			incident.BeforeSendEHubMessage -= eDocsHandler;
		}

		public void TestCalculateEHubMessageSentTimeInUtc_FilenameDuplicate()
		{
			SupportIncidentForTest incident = Factory.NewWithValidTestData<SupportIncidentForTest>();
			SetupIncidentForCustomerSystemNotificationTest(incident);

			Xsd.CustomerServiceResponseAttachmentCollection attachments = new Xsd.CustomerServiceResponseAttachmentCollection();
			Xsd.CustomerServiceResponseAttachment a = new Xsd.CustomerServiceResponseAttachment();
			a.FileName = "duplicate.txt";
			attachments.Add(a);
			a = new Xsd.CustomerServiceResponseAttachment();
			a.FileName = "duplicate.txt";
			attachments.Add(a);

			AssertNoExceptionThrown("should not throw any exception", delegate
			{ ERequestEHubCustomerNotificationSender.CalculateEHubMessageSentTimeInUtc(incident, null, attachments); });
		}

		#endregion

		#region Customer System Notification Message

		public void TestSendCustomerSystemNotification_ShouldUpdateStatusCorrectly()
		{
			#region Setup

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_FullName = "blah blah lola lola";
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Email = "zubin.appoo@edi.com.au";

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			ReleaseBuilds.Business.ReleaseBuild someBuild = Factory.New<ReleaseBuilds.Business.ReleaseBuild>();
			someBuild.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = someBuild.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Description = "Help me";
			incident.DetailNoteText = "Don't know what i'm doing";
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_ClientIncidentReference = "CL111111";
			var sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);
			Factory.Save();

			#endregion

			byte[] pDFbody = new byte[] { 1, 1, 1, 1, 1 };

			AssertEquals("Precondition", SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("Precondition", SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertCustomerServiceResponseStatus(sender.LastSentResponse, "Precondition", SupportIncidentLookups.LegacyStatusCodes.SupportTeam);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			StorageFile pdfFile1 = (StorageFile)((IDocManagerSupport)incident).DocManagerInfo.AddFileOrDocument(pDFbody, "pdf1.pdf", "MSC");
			pdfFile1.SC_IsPublished = true;
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertCustomerServiceResponseStatus(sender.LastSentResponse, "Should still use Support stage", SupportIncidentLookups.LegacyStatusCodes.SupportTeam);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertCustomerServiceResponseStatus(sender.LastSentResponse, "Should use Defect stage", SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			StorageFile pdfFile2 = (StorageFile)((IDocManagerSupport)incident).DocManagerInfo.AddFileOrDocument(pDFbody, "pdf2.pdf", "MSC");
			pdfFile2.SC_IsPublished = true;
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertCustomerServiceResponseStatus(sender.LastSentResponse, "Should still use Defect stage", SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertCustomerServiceResponseStatus(sender.LastSentResponse, "Should use FeatureRequest stage", SupportIncidentLookups.LegacyStatusCodes.PendingFeatureResult);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			StorageFile pdfFile3 = (StorageFile)((IDocManagerSupport)incident).DocManagerInfo.AddFileOrDocument(pDFbody, "pdf3.pdf", "MSC");
			pdfFile3.SC_IsPublished = true;
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertCustomerServiceResponseStatus(sender.LastSentResponse, "Should still use FeatureRequest stage", SupportIncidentLookups.LegacyStatusCodes.PendingFeatureResult);
		}

		[ExpectNoExceptions]
		public void TestSendCustomerSystemNotification_NoLicenceHeader()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "blah blah lola lola";
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Email = "zubin.appoo@edi.com.au";

			var incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Description = "Help me";
			incident.DetailNoteText = "Don't know what i'm doing";
			incident.IM_ClientIncidentReference = "CL111111";
			Factory.Save();
		}

		public void TestSendCustomerSystemNotification_NoneWhenEscalateBackToSupport()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_FullName = "Test Org";
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "samuel@test.info";

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			ReleaseBuilds.Business.ReleaseBuild releaseBuild = Factory.New<ReleaseBuilds.Business.ReleaseBuild>();
			releaseBuild.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SYD";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = releaseBuild.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "AAA";
			clientCompany.LCC_LD = database.PK;

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Description = "Test Incident";
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_ClientIncidentReference = "SR0023492";
			var sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);
			Factory.Save();

			AssertEquals("One message is sent for incident creation", 1, sender.Responses.Count);

			sender.Responses.Clear();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(1, sender.Responses.Count);
			AssertCustomerServiceResponseStatus(sender.LastSentResponse, "Should use Defect stage", SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam);

			sender.Responses.Clear();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(1, sender.Responses.Count);
			AssertCustomerServiceResponseStatus(sender.LastSentResponse, "Should not be closed but use Support stage", SupportIncidentLookups.LegacyStatusCodes.SupportTeam);

			sender.Responses.Clear();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingReferredToLearningMaterials, "");
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(1, sender.Responses.Count);
			AssertCustomerServiceResponseStatus(sender.LastSentResponse, "Should be closed", SupportIncidentLookups.LegacyStatusCodes.Closed);
		}

		public void TestSendCustomerSystemNotification_EConversationUpdate()
		{
			SupportIncident incident = SetupIncidentForCustomerSystemNotificationTest();
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_ClientIncidentReference = "SR0023492";
			var sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);
			Factory.Save();

			incident.AddSystemMessageToCustomer("This log should be sent to client system");
			incident.AddSystemMessageToCustomer("Another log should be sent");
			Factory.Save();

			AssertNotNull(sender.LastSentResponse);
			AssertEquals("This log should be sent to client system", sender.LastSentResponse.IncidentConversationUpdate[0].Body);
			AssertEquals("Another log should be sent", sender.LastSentResponse.IncidentConversationUpdate[1].Body);

			sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);
			Factory.Save();
			AssertNull("Log update should be cleared so no out bound message", sender.LastSentResponse);
		}

		public void TestSendCustomerSystemNotification_EmailUpdate()
		{
			SupportIncident incident = SetupIncidentForCustomerSystemNotificationTest();
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_IncidentNumber = "CS00000001";

			var sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);

			Factory.Save();

			incident.IM_ClientIncidentReference = "SR000018293";
			incident.IM_Module = "SAL";
			incident.IM_Priority = "CR5";
			incident.IM_Description = "Test Incident (Changed)";
			incident.DetailNoteText = "Incident details (Changed)";
			sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);

			Factory.Save();

			AssertEquals("The incident updated email should be in response message", 1, sender.LastSentResponse.IncidentEmails.Count);
			AssertEquals("Email header and footer should be attached", 2, sender.LastSentResponse.IncidentEmails[0].Attachments.Count);
			AssertEquals("Customer Service Incident CS00000001 Has Been Updated", sender.LastSentResponse.IncidentEmails[0].Subject);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			Factory.Save();

			incident.IM_Module = "BRK";
			incident.IM_Priority = "CR6";
			sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);
			Factory.Save();
			AssertEquals("Update email should still be included when incident is closed", 1, sender.LastSentResponse.IncidentEmails.Count);

			incident.Contact.OC_Email = "sUPPort@wisetechglobal.com";
			incident.IM_Priority = "CR5";
			sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);
			Factory.Save();
			AssertEquals("Contact email in response should be empty", "", sender.LastSentResponse.ContactEmailAddress);
			AssertEquals("Exclude email in response should be support email address", "support@wisetechglobal.com", sender.LastSentResponse.SystemReservedEmailAddresses);
			AssertEquals("Email recipient should not contain support email address", false, sender.LastSentResponse.IncidentEmails[0].ToEmailAddress.Contains("sUPPort@cargowise.com"));
		}

		public void TestSendCustomerSystemNotification_ClientCreatedCompany()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			ReleaseBuilds.Business.ReleaseBuild build = Factory.New<ReleaseBuilds.Business.ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
			build.HL_Release = LicenceDatabase.FirstReleaseSystemMessageCSR;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_LicenceType = DatabaseTypes.Codes.Production;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "SYD";
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_ClientIncidentReference = "S00004938";
			Factory.Save();

			var sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();
			AssertEquals(1, sender.Responses.Count);
			AssertCustomerServiceResponseStatus(sender.LastSentResponse, "Should use Defect stage", SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam);
		}

		void AssertCustomerServiceResponseStatus(Xsd.CustomerServiceResponse response, string errorMessage, string expectedCustomerSystemStatusCode)
		{
			AssertEquals(errorMessage, expectedCustomerSystemStatusCode, response.Status);
		}

		#endregion

		#region Customer System Notification Message - New Incident Created in ediProd Replicates in Client System

		public void TestSendCustomerSystemNotification_NewIncidentReplicated()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("SAA", "Super Module", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			SupportIncident incident = SetupIncidentForCustomerSystemNotificationTest();
			incident.IM_Module = "SAA";
			incident.IM_Priority = "CR8";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_ClientIncidentReference = "";
			AssertEquals("Precondition", "CR5", incident.PriorityDisplayedOnClientSide);

			var sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);

			Factory.Save();

			AssertNotNull(sender.LastSentResponse);
			AssertEquals(incident.IM_IncidentNumber, sender.LastSentResponse.IncidentNumber);
			AssertEquals("", sender.LastSentResponse.ClientReferenceNumber);
			AssertEquals("DDDCOMSRV", sender.LastSentResponse.LicenceCode);
			AssertEquals("CR5", sender.LastSentResponse.Criticality);
			AssertEquals("SAA", sender.LastSentResponse.Module);
			AssertEquals("Super Module", sender.LastSentResponse.ModuleDescription);
			AssertEquals("Test Incident", sender.LastSentResponse.IncidentSummary);
			AssertEquals("Incident details", sender.LastSentResponse.IncidentDetails);

			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_OH_Client = incident.IM_OH_Client;
			incident2.IM_OC_Contact = incident.IM_OC_Contact;
			incident2.IM_LD = incident.IM_LD;
			incident2.IM_LCC = incident.IM_LCC;
			incident2.IM_Module = "SAA";
			incident2.IM_Priority = "CR7";
			incident2.IM_Description = "Test Incident";
			incident2.DetailNoteText = "Incident details";
			incident2.IM_ClientIncidentReference = "";

			var actualSender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident2);
			AssertType<ERequestEHubCustomerNotificationSender>(actualSender);

			sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident2.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident2, sender);

			Factory.Save();
			AssertNotNull(sender.LastSentResponse);
			AssertEquals(incident2.IM_IncidentNumber, sender.LastSentResponse.IncidentNumber);
		}

		public void TestCreateSenderIfSupported_CargoWiseInternalIncidentNotReplicated()
		{
			#region Setup

			ReleaseBuilds.Business.ReleaseBuild build = Factory.New<ReleaseBuilds.Business.ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.FirstReleaseCSBiDirectionMessage);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise1 = Factory.New<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = "EDI";
			enterprise1.LE_OH = org1.PK;

			var database1 = Factory.New<LicenceDatabase>();
			database1.LD_ServerCode = "SRV";
			database1.LD_LE = enterprise1.PK;
			database1.LD_HL_CurrentRunningVersion = build.PK;

			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_Code = "COM";
			clientCompany1.LCC_LD = database1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise2 = Factory.New<LicenceEnterprise>();
			enterprise2.LE_EnterpriseCode = "EEE";
			enterprise2.LE_OH = org2.PK;

			var database2 = Factory.New<LicenceDatabase>();
			database2.LD_ServerCode = "ADD";
			database2.LD_LE = enterprise2.PK;
			database2.LD_HL_CurrentRunningVersion = build.PK;

			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_Code = "COM";
			clientCompany2.LCC_LD = database2.PK;

			Factory.Save();

			#endregion

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org1.MainAddress.PK;
			incident.IM_LD = database1.PK;
			incident.IM_LCC = clientCompany1.PK;
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test EDI Incident";
			incident.DetailNoteText = "EDI Incident details";
			incident.IM_ClientIncidentReference = "";

			var sender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident);
			AssertNull("Licence enterprise code is EDI", sender);

			incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org2.MainAddress.PK;
			incident.IM_LD = database2.PK;
			incident.IM_LCC = clientCompany2.PK;
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test NON EDI Incident";
			incident.DetailNoteText = "NON EDI Incident details";
			incident.IM_ClientIncidentReference = "";

			sender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident);
			AssertNotNull("Licence enterprise code is NOT EDI", sender);
		}

		public void TestCreateSenderIfSupported_NonProductionDBNotReplicated()
		{
			#region Setup

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test CW";
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "sam@test.com.au";

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			ReleaseBuilds.Business.ReleaseBuild build = Factory.New<ReleaseBuilds.Business.ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.FirstReleaseCSBiDirectionMessage);

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			database.LD_LicenceType = DatabaseTypes.Codes.Test;

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			#endregion

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_ClientIncidentReference = "";

			var sender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident);
			AssertNull("Licence database type is Testing", sender);

			database.LD_LicenceType = DatabaseTypes.Codes.Training;
			Factory.Save();
			incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_ClientIncidentReference = "";

			sender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident);
			AssertNull("Licence database type is Training", sender);

			database.LD_LicenceType = DatabaseTypes.Codes.Production;
			Factory.Save();
			incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_ClientIncidentReference = "";

			sender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident);
			AssertNotNull("Licence database type is Production", sender);
		}

		public void TestCreateSenderIfSupported_IncidentCreatedBeforeV2NotReplicatedAfterUpgrade()
		{
			#region Setup

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test CW";
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "sam@test.com.au";

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			ReleaseBuilds.Business.ReleaseBuild build = Factory.New<ReleaseBuilds.Business.ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			database.LD_LicenceType = DatabaseTypes.Codes.Production;

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			#endregion

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_ClientIncidentReference = "";

			var sender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident);
			AssertNull("Reported on version does not support bi-direction message", sender);

			ReleaseBuilds.Business.ReleaseBuild build2 = Factory.New<ReleaseBuilds.Business.ReleaseBuild>();
			build2.VersionNumber = new VersionNumber(LicenceDatabase.FirstReleaseCSBiDirectionMessage);
			database.LD_HL_CurrentRunningVersion = build2.PK;
			Factory.Save();

			incident.IM_Priority = "CR3";
			sender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident);
			AssertNull("Should not send any message for old incidents even after upgrade", sender);

			incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_ClientIncidentReference = "";
			sender = ERequestCustomerNotificationSender.CreateSenderIfSupported(incident);
			AssertNotNull("New incident after upgrade should send message", sender);
		}

		public void TestWorkItemCompletedEmailAutomaticallySent_V2()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithERequestV2();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Test User";
			staff.GS_EmailAddress = "test@edi.com.au";

			incident.IM_IncidentNumber = "IM0123";
			incident.IM_Product = "ENT";
			incident.IM_Description = "Test Incident";
			incident.Contact.OC_Email = "contact@edi.com.au";
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.IM_GS_NKAssignedToCurrent = staff.GS_Code;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.Assigned;

			var workItem = incident.RelatedWorkItems.AddNew();
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);

			Factory.Save();

			var workItemTask = workItem.WorkflowItems.AddNew();
			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;

			var sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);

			Factory.Save();

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Development Work Completed email sent to contact"));

			AssertEquals(2, sender.Sends);

			var emailMessage = sender.Responses[0].Response;
			AssertEquals(SupportIncidentLookups.LegacyActions.Email, emailMessage.Action);
			AssertEquals(1, emailMessage.IncidentEmails.Count);
			AssertEquals("contact@edi.com.au", emailMessage.IncidentEmails[0].ToEmailAddress);
			AssertEquals("Incident IM0123 resolved: Test Incident", emailMessage.IncidentEmails[0].Subject);

			var updateMessage = sender.Responses[1].Response;
			AssertEquals(SupportIncidentLookups.LegacyActions.Update, updateMessage.Action);
			AssertArrayEqualsByElements(updateMessage.IncidentConversationUpdate.Cast<Xsd.ConversationMessage>().Select(m => m.Body).ToArray(), new ZString[]
			{
				"Closed As Completed",
				"Development Work Completed email sent to contact"
			});
		}

		public void TestSendCustomerSystemNotification()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithERequestV2();
			incident.IM_Description = "Help me";
			incident.DetailNoteText = "Don't know what i'm doing";
			var sender = new ERequestEHubCustomerNotificationSenderForTest();
			incident.CustomerNotifier = new LocalSystemChangeIncidentCustomerNotifier(incident, sender);
			Factory.Save();

			AssertEquals("response sent", 1, sender.Sends);
			sender.Responses.Clear();

			Factory.Save();
			AssertEquals("No changes, so no more emails", 0, sender.Sends);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();
			AssertEquals("One message should be sent because stage is development", 1, sender.Sends);
			sender.Responses.Clear();

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			AssertEquals("One message should be sent because reopened", 1, sender.Sends);
			sender.Responses.Clear();

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			Factory.Save();
			AssertEquals("One message should be sent because stage is feature request", 1, sender.Sends);
			sender.Responses.Clear();

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			AssertEquals("One message should be sent because", 1, sender.Sends);
			sender.Responses.Clear();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Other, "");
			Factory.Save();
			AssertEquals("One message should be sent because closed in support", 1, sender.Sends);
			sender.Responses.Clear();

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			Factory.Save();
			AssertEquals("One message should be sent because reopened and escalated as feature", 1, sender.Sends);
			sender.Responses.Clear();

			incident.IM_Module = "AAA";
			Factory.Save();
			AssertEquals("One message should be sent because module changed", 1, sender.Sends);
			sender.Responses.Clear();

			incident.IM_Priority = "CR3";
			Factory.Save();
			AssertEquals("One message should be sent because criticality changed", 1, sender.Sends);
			sender.Responses.Clear();

			incident.IM_Description = "Help me again";
			Factory.Save();
			AssertEquals("One message should be sent because description changed", 1, sender.Sends);
			sender.Responses.Clear();

			incident.DetailNoteText = "I know what i'm doing";
			Factory.Save();
			AssertEquals("One message should be sent because detail text changed", 1, sender.Sends);
			sender.Responses.Clear();

			incident.AddStaffMessageToCustomer("This is an update to eConversation");
			Factory.Save();
			AssertEquals("One message should be sent because eConversation updated", 1, sender.Sends);
			sender.Responses.Clear();
		}

		SupportIncident SetupIncidentForCustomerSystemNotificationTest()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			SetupIncidentForCustomerSystemNotificationTest(incident);
			return incident;
		}

		static void SetupIncidentForCustomerSystemNotificationTest(SupportIncident incident)
		{
			#region Setup

			var factory = incident.Factory;
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Org";
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "sam@test.com.au";

			LicenceEnterprise enterprise = factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;
			factory.Save();

			ReleaseBuilds.Business.ReleaseBuild build = factory.New<ReleaseBuilds.Business.ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.FirstReleaseCSBiDirectionMessage);

			LicenceDatabase database = factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			database.LD_LicenceType = DatabaseTypes.Codes.Production;

			ClientCompany clientCompany = factory.New<ClientCompany>();
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_Code = "COM";

			factory.Save();

			#endregion

			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
		}

		#endregion

		#region Implementation

		class ERequestEHubCustomerNotificationSenderForTest : ERequestEHubCustomerNotificationSender
		{
			public readonly IList<SentResponse> Responses = new List<SentResponse>();
			public int Sends => Responses.Count;
			public Xsd.CustomerServiceResponse LastSentResponse
			{
				get { return (Responses.Count > 0) ? Responses.Last().Response : null; }
			}

			protected override void Send(SupportIncident incident, Xsd.CustomerServiceResponse response)
			{
				Responses.Add(new SentResponse(incident, response));
			}

			internal void Send_Exposed(SupportIncident incident, Xsd.CustomerServiceResponse response)
			{
				base.Send(incident, response);
			}

			protected override IOutgoingSystemMessage GetSender()
			{
				return new DebugOnlyOutgoingSystemMessage();
			}

			public struct SentResponse
			{
				public readonly SupportIncident Incident;
				public readonly Xsd.CustomerServiceResponse Response;

				public SentResponse(SupportIncident incident, Xsd.CustomerServiceResponse response)
				{
					Incident = incident;
					Response = response;
				}
			}
		}

		class SupportIncidentForTest : SupportIncident
		{
			public SupportIncidentForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnSaveSucceeded()
			{
				if (BeforeSendEHubMessage != null)
				{
					BeforeSendEHubMessage(this, EventArgs.Empty);
				}

				base.OnSaveSucceeded();
			}

			protected override void LogProductChange()
			{
			}

			public event EventHandler BeforeSendEHubMessage;
		}

		#endregion
	}
}
