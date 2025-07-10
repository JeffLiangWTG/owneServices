using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business.Testing
{
	sealed class CustomerServiceResponseMessageActionTest : TestCaseWithFactory
	{
		public void TestProcessAndSave()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";
			Factory.Save();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.ClientReferenceNumber = incident.IA_ClientReference;
			response.IncidentNumber = "Number";
			response.Status = IncidentApprovalLookups.StatusCodes.ApprovedAndSent;
			response.Action = IncidentApprovalLookups.Actions.Update;

			CustomerServiceResponseMessageAction.ProcessAndSave(response);
			AssertEquals("IA_Status", IncidentApprovalLookups.StatusCodes.ApprovedAndSent, incident.IA_Status);
			AssertEquals("Should not has changes after saving", false, incident.IA_StatusInfo.HasChanges);
		}

		public void TestExecuteAction_IncidentNotFound()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";
			incident.IA_IncidentNumber = "CS00001900";
			incident.IA_ClientReference = "SR00003000";
			Factory.Save();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.ClientReferenceNumber = "SR00004000";
			response.IncidentNumber = "Number";
			response.Status = IncidentApprovalLookups.StatusCodes.ApprovedAndSent;
			response.Action = IncidentApprovalLookups.Actions.Update;

			EDIMessage message = CreateMessage(response);
			IMessageAction action = new CustomerServiceResponseMessageAction(new BusinessObjectFactoryProvider(Factory));

			NotificationBuffer notifications = new NotificationBuffer();
			List<ITransactionParticipant> participants;
			bool result = action.ExecuteAction(message, notifications, out participants);

			AssertEquals(false, result);
			AssertEquals(1, notifications.Events.Length);
			AssertEquals("Error", notifications.Events[0].Type.EnumValueName);
			AssertEquals("Incident Number/SR00004000 is not found.", notifications.Events[0].Message);
		}

		public void TestExecuteAction_StatusChange()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";
			Factory.Save();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.ClientReferenceNumber = incident.IA_ClientReference;
			response.IncidentNumber = "Number";
			response.Status = IncidentApprovalLookups.StatusCodes.ApprovedAndSent;
			response.Action = IncidentApprovalLookups.Actions.Update;

			EDIMessage message = CreateMessage(response);
			IMessageAction action = new CustomerServiceResponseMessageAction(new BusinessObjectFactoryProvider(Factory));

			INotifications notifications = null;
			List<ITransactionParticipant> participants;
			action.ExecuteAction(message, notifications, out participants);

			AssertEquals("IA_Status", IncidentApprovalLookups.StatusCodes.ApprovedAndSent, incident.IA_Status);
		}

		public void TestExecuteAction_DoNotClearFieldsIfV1Message()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_LicenceCode = "DDDCOMSYD";
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.SupportTeam;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";
			incident.IA_ClientReference = "SR00001001";
			incident.IA_IncidentNumber = "CS00003000";
			Factory.Save();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.ClientReferenceNumber = "SR00001001";
			response.IncidentNumber = "CS00003000";

			CustomerServiceResponseMessageAction.ProcessAndSave(response);

			IncidentApproval loadedIncident = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			AssertEquals("Old message should not clear Criticality", Constants.CustomerService.CriticalityCodes.CR5_Training, loadedIncident.IA_Criticality);
			AssertEquals("Old message should not clear Module", "COR", loadedIncident.IA_Module);
			AssertEquals("Old message should not clear Incident Summary", "Test Summary", loadedIncident.IA_IncidentSummary);
			AssertEquals("Old message should not clear Incident Details", "Test Details", loadedIncident.IA_IncidentDetails);
		}

		public void TestExecuteAction_NewIncidentFromResponse_Ignored()
		{
			string key = ZGuid.NewZGuid().ToString();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.PK = key;
			response.ClientReferenceNumber = "";
			response.IncidentNumber = "CS00001234";
			response.LicenceCode = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			response.Status = IncidentApprovalLookups.StatusCodes.SupportTeam;
			response.Criticality = "CR4";
			response.Module = ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing;
			response.ModuleDescription = "Super Module";
			response.IncidentSummary = "Test Incident";
			response.IncidentDetails = "Incident details";
			response.Action = IncidentApprovalLookups.Actions.Add;
			CustomerServiceResponseMessageAction.ProcessAndSave(response);

			IncidentApproval incident = Factory.LoadTop1<IncidentApproval>(new ZQuery(IncidentApprovalSchema.IA_IncidentNumber, "CS00001234"));
			AssertNull(incident);
		}

		public void TestExecuteAction_DoNotProcessOldMessageSentAfterSync()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_LicenceCode = "DDDCOMSYD";
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.SupportTeam;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";
			incident.IA_ClientReference = "SR00001001";
			incident.IA_IncidentNumber = "CS00003000";
			Factory.Save();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.LicenceCode = "DDDCOMSYD";
			response.ClientReferenceNumber = "SR00001001";
			response.IncidentNumber = "CS00003000";
			response.Status = IncidentApprovalLookups.StatusCodes.DevelopmentTeam;
			response.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			response.Action = IncidentApprovalLookups.Actions.Update;
			response.SentTimeUtc = new ZDateTime(2012, 6, 5, 14, 20, 0).ToString("o");

			incident.LastSyncTime = new ZDateTime(2012, 6, 5, 14, 20, 1);
			Factory.Save();

			CustomerServiceResponseMessageAction.ProcessAndSave(response);

			IncidentApproval loadedIncident = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			AssertEquals("Should not change", IncidentApprovalLookups.StatusCodes.SupportTeam, loadedIncident.IA_Status);
			AssertEquals("Should not change", Constants.CustomerService.CriticalityCodes.CR5_Training, loadedIncident.IA_Criticality);
		}

		#region Email

		public void TestEmail()
		{
			SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, IncidentApprovalLookups.EmailRecipientSettings.AllParties);

			GlbStaff reportedByStaff = Factory.NewWithValidTestData<GlbStaff>();
			reportedByStaff.GS_Code = "TST";
			reportedByStaff.GS_EmailAddress = "Tester@test.com.au";
			reportedByStaff.GS_LoginName = "tester";

			GlbStaff approvingStaff = Factory.NewWithValidTestData<GlbStaff>();
			approvingStaff.GS_Code = "SAM";
			approvingStaff.GS_EmailAddress = "Sam@test.com.au";
			approvingStaff.GS_LoginName = "sam";

			GlbStaff reportingStaff = Factory.NewWithValidTestData<GlbStaff>();
			reportingStaff.GS_Code = "AAA";
			reportingStaff.GS_EmailAddress = "aaa@test.com.au";
			reportingStaff.GS_LoginName = "aaa";

			Factory.Save();

			IncidentApproval incident;

			using (Env.SetTemporaryUserContext(reportedByStaff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				incident = Factory.New<IncidentApproval>();
				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
				incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
				incident.IA_Module = "COR";
				incident.IA_IncidentSummary = "Test Summary";
				incident.IA_IncidentDetails = "Test Details Is Long Enough";
				incident.IA_GS_NKReportingStaff = "AAA";
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(approvingStaff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.IncidentApprovalApprove.IsAllowed = true;
				incident.Approve();
			}

			AssertEquals("SAM", incident.IA_GS_NKApprovingStaff);
			AssertEquals("TST", incident.ReportedByStaffCode);
			AssertEquals("AAA", incident.IA_GS_NKReportingStaff);

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.ClientReferenceNumber = incident.IA_ClientReference;
			response.IncidentNumber = "CS00001234";
			response.Status = IncidentApprovalLookups.StatusCodes.SupportTeam;
			response.Action = IncidentApprovalLookups.Actions.Update;
			Xsd.CustomerServiceResponseEmail incidentEmail = response.IncidentEmails.AddNew();
			incidentEmail.Subject = "Test Email Subject";
			incidentEmail.Body = "Test Email Body";

			EDIMessage message = CreateMessage(response);
			IMessageAction action = new CustomerServiceResponseMessageAction(new BusinessObjectFactoryProvider(Factory));
			List<ITransactionParticipant> participants;
			action.ExecuteAction(message, null, out participants);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(3, email.Recipients.Count);
			AssertEquals("Sam@test.com.au", email.Recipients[0].Email);
			AssertEquals("Tester@test.com.au", email.Recipients[1].Email);
			AssertEquals("aaa@test.com.au", email.Recipients[2].Email);
			AssertEquals("Test Email Subject", email.Subject);
			AssertEquals("Test Email Body", email.Body);
		}

		public void TestNewIncidentEmail_IgnoredSinceIncidentNotCreated()
		{
			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.ClientReferenceNumber = "";
			response.IncidentNumber = "CS00001234";
			response.LicenceCode = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			response.Status = IncidentApprovalLookups.StatusCodes.SupportTeam;
			response.Criticality = "CR4";
			response.Module = Env.Licence.RelationshipCampaignManager.Name;
			response.ModuleDescription = "Super Module";
			response.IncidentSummary = "Test Incident";
			response.IncidentDetails = "Incident details";
			response.ContactEmailAddress = "Samuel.Wang@cargowise.com";
			response.Action = IncidentApprovalLookups.Actions.Add;

			Xsd.CustomerServiceResponseEmail incidentEmail = response.IncidentEmails.AddNew();
			incidentEmail.Subject = "Test Email Subject";
			incidentEmail.Body = "Test Email Body";

			CustomerServiceResponseMessageAction.ProcessAndSave(response);

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2012, 5, 18, 11, 30, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestEConversationUpdate()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_ClientReference = "SR00001412";
			incident.IA_IncidentNumber = "CS00001234";
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";
			incident.IA_LicenceCode = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			Factory.Save();

			incident.AddUserMessageToEConversation("Local Message 1");
			Factory.Save();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.ClientReferenceNumber = "SR00001412";
			response.IncidentNumber = "CS00001234";
			response.LicenceCode = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			response.Status = IncidentApprovalLookups.StatusCodes.SupportTeam;
			response.Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			response.Module = "COR";
			response.ModuleDescription = "Super Module";
			response.IncidentSummary = "Test Summary";
			response.IncidentDetails = "Test Details";
			response.Action = IncidentApprovalLookups.Actions.Update;
			Xsd.ConversationMessage message1 = response.IncidentConversationUpdate.AddNew();
			message1.SentTimeInUtc = new ZDateTime(2012, 5, 16, 8, 0, 0).ToString("o");
			message1.Body = "Remote Message 1";
			Xsd.ConversationMessage message2 = response.IncidentConversationUpdate.AddNew();
			message2.SentTimeInUtc = new ZDateTime(2012, 5, 13, 22, 0, 0).ToString("o");
			message2.Body = "Remote Message 2";

			CustomerServiceResponseMessageAction.ProcessAndSave(response);

			incident = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("Local Message 1", messageList[0].Body);
			AssertEquals("Remote Message 1", messageList[1].Body);
			AssertEquals("Remote Message 2", messageList[2].Body);
		}

		#endregion

		EDIMessage CreateMessage(Xsd.CustomerServiceResponse response)
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			ZXmlSerializer ser = ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse));
			MemoryStream xmlStream = new MemoryStream();
			ser.Serialize(xmlStream, response);
			message.SetEM_MessageTextSource(new TextReaderSource(xmlStream));
			return message;
		}
	}
}
