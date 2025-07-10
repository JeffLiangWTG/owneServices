using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business.Testing
{
	sealed class CustomerServiceResponseHelperTest : TestCaseWithFactory
	{
		public void TestProcessResponse()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_LicenceCode = "DDDCOMSYD";
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.SupportTeam;
			incident.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.Account;
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";
			incident.IA_ClientReference = "SR00001001";
			incident.IA_IncidentNumber = "CS00003000";
			Factory.Save();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.Action = IncidentApprovalLookups.Actions.Update;
			response.ClientReferenceNumber = "SR00001001";
			response.IncidentNumber = "CS00003000";
			response.Status = IncidentApprovalLookups.StatusCodes.DevelopmentTeam;
			response.LicenceCode = "DDDCOMMEL";
			response.Module = ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing;
			response.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			response.IncidentSummary = "Test Summary (Modified)";
			response.IncidentDetails = "Test Details (Modified)";
			response.SentTimeUtc = new ZDateTime(2012, 6, 11, 15, 32, 0).ToString("o");

			CustomerServiceResponseHelper helper = new CustomerServiceResponseHelper(response, incident);
			helper.ProcessAndSave();

			IncidentApproval loadedIncident = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			AssertEquals(IncidentApprovalLookups.StatusCodes.DevelopmentTeam, loadedIncident.IA_Status);
			AssertEquals(Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, loadedIncident.IA_Criticality);
			AssertEquals("Module should not change", ModuleTreeCustomerServiceMenuSectionList.Codes.Account, loadedIncident.IA_Module);
			AssertEquals("DDDCOMMEL", loadedIncident.IA_LicenceCode);
			AssertEquals("Test Summary (Modified)", loadedIncident.IA_IncidentSummary);
			AssertEquals("Test Details (Modified)", loadedIncident.IA_IncidentDetails);
			AssertEquals(new ZDateTime(2012, 6, 11, 15, 32, 0), loadedIncident.LastSyncTime);
		}

		public void TestProcessReponse_UpdateResponse_SetModuleToOtherOnModuleListTypeChange()
		{
			var incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.Account;
			var response = new Xsd.CustomerServiceResponse();
			response.Action = IncidentApprovalLookups.Actions.Update;
			response.Criticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			response.Module = Cr8ModuleList.Codes.CarbonEnvironmentalCompliance;

			new CustomerServiceResponseHelper(response, incident).ProcessAndSave();

			CombineAssertions(() =>
			{
				AssertEquals("IA_Criticality", Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement, incident.IA_Criticality);
				AssertEquals("IA_Module: Should set to 'Other' when ModuleListType changed", Cr8ModuleList.Codes.OtherComplianceIssue, incident.IA_Module);
			});
		}

		public void TestProcessReponse_UpdateResponse_DoesNotUpdateModuleWhenModuleListTypeUnchange()
		{
			var incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.Account;
			var response = new Xsd.CustomerServiceResponse();
			response.Action = IncidentApprovalLookups.Actions.Update;
			response.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			response.Module = MandatoryCustomerServiceMenuSectionList.Codes.Webtracker;

			new CustomerServiceResponseHelper(response, incident).ProcessAndSave();

			CombineAssertions(() =>
			{
				AssertEquals("IA_Criticality", Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, incident.IA_Criticality);
				AssertEquals("IA_Module: Should not update when ModuleListType unchanged", ModuleTreeCustomerServiceMenuSectionList.Codes.Account, incident.IA_Module);
			});
		}

		public void TestProcessReponse_AddResponse_SetsModuleWhenValidModuleCode()
		{
			{
				var incident = Factory.New<IncidentApproval>();
				var response = new Xsd.CustomerServiceResponse();
				response.Action = IncidentApprovalLookups.Actions.Add;
				response.Criticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				response.Module = Cr8ModuleList.Codes.DangerousGoodsManagement;

				new CustomerServiceResponseHelper(response, incident).ProcessAndSave();

				CombineAssertions(() =>
				{
					AssertEquals("IA_Criticality", Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement, incident.IA_Criticality);
					AssertEquals("IA_Module", Cr8ModuleList.Codes.DangerousGoodsManagement, incident.IA_Module);
				});
			}

			// test still works when module tree is empty (e.g. when run from service task)
			using (ModuleTree.OverrideTreeForTest(new ModuleTree()))
			{
				var incident = Factory.New<IncidentApproval>();
				var response = new Xsd.CustomerServiceResponse();
				response.Action = IncidentApprovalLookups.Actions.Add;
				response.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
				response.Module = ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing;

				new CustomerServiceResponseHelper(response, incident).ProcessAndSave();

				CombineAssertions(() =>
				{
					AssertEquals("IA_Criticality", Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, incident.IA_Criticality);
					AssertEquals("IA_Module", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing, incident.IA_Module);
				});
			}
		}

		public void TestProcessReponse_AddResponse_SetsModuleToOtherWhenUnknownModuleCode()
		{
			var incident = Factory.New<IncidentApproval>();
			var response = new Xsd.CustomerServiceResponse();
			response.Action = IncidentApprovalLookups.Actions.Add;
			response.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			response.Module = "XXX";

			new CustomerServiceResponseHelper(response, incident).ProcessAndSave();

			CombineAssertions(() =>
			{
				AssertEquals("IA_Criticality", Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, incident.IA_Criticality);
				AssertEquals("IA_Module: Should have set to 'Other'", MandatoryCustomerServiceMenuSectionList.Codes.Other, incident.IA_Module);
			});
		}

		public void TestProcessResponse_DoNotClearFieldsIfV1Message()
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

			CustomerServiceResponseHelper helper = new CustomerServiceResponseHelper(response, incident);
			helper.ProcessAndSave();

			IncidentApproval loadedIncident = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			AssertEquals("Old message should not clear Criticality", Constants.CustomerService.CriticalityCodes.CR5_Training, loadedIncident.IA_Criticality);
			AssertEquals("Old message should not clear Module", "COR", loadedIncident.IA_Module);
			AssertEquals("Old message should not clear Incident Summary", "Test Summary", loadedIncident.IA_IncidentSummary);
			AssertEquals("Old message should not clear Incident Details", "Test Details", loadedIncident.IA_IncidentDetails);
		}

		[TestDate(2013, 7, 29)]
		public void TestProcessResponse_EConversation()
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

			incident.IA_SystemLastEditUser = "EAU";
			incident.IA_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			string currentUserInitials = Env.CurrentUser.Initials;

			CustomerServiceResponseHelper helper = new CustomerServiceResponseHelper(response, incident);

			Assert(incident.IA_SystemLastEditTimeUtc == new ZDateTime(2013, 7, 27));
			Assert(incident.IA_SystemLastEditUser == "EAU");
			Assert(incident.IA_SystemLastEditUser != currentUserInitials);

			helper.ProcessAndSave();

			Assert("Incident LastEditTimeUtc should change", incident.IA_SystemLastEditTimeUtc == ZDateTime.UtcNow);
			Assert("Incident LastEditUser should change", incident.IA_SystemLastEditUser == currentUserInitials);

			incident = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("Local Message 1", messageList[0].Body);
			AssertEquals("Remote Message 1", messageList[1].Body);
			AssertEquals("Remote Message 2", messageList[2].Body);
		}

		public void TestProcessResponse_DiscardOldResponse()
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
			response.Status = IncidentApprovalLookups.StatusCodes.DevelopmentTeam;
			response.LicenceCode = "DDDCOMMEL";
			response.Module = "SAL";
			response.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			response.IncidentSummary = "Test Summary (Modified)";
			response.IncidentDetails = "Test Details (Modified)";
			response.SentTimeUtc = new ZDateTime(2012, 6, 11, 15, 32, 0).ToString("o");

			incident.LastSyncTime = new ZDateTime(2012, 6, 11, 15, 32, 30);
			Factory.Save();

			var notifications = new NotificationsForTest();
			CustomerServiceResponseHelper helper = new CustomerServiceResponseHelper(response, incident, notifications);
			helper.ProcessAndSave();

			AssertEquals(notifications.ToString(), "Response message is outdated. Changes are not saved.");

			IncidentApproval loadedIncident = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			AssertEquals("Old change not accepted", "DDDCOMSYD", loadedIncident.IA_LicenceCode);
			AssertEquals("Old change not accepted", Constants.CustomerService.CriticalityCodes.CR5_Training, loadedIncident.IA_Criticality);
			AssertEquals("Old change not accepted", IncidentApprovalLookups.StatusCodes.SupportTeam, loadedIncident.IA_Status);
			AssertEquals("Old change not accepted", "COR", loadedIncident.IA_Module);
			AssertEquals("Old change not accepted", "Test Summary", loadedIncident.IA_IncidentSummary);
			AssertEquals("Old change not accepted", "Test Details", loadedIncident.IA_IncidentDetails);
		}

		public void TestSendEmailsFromResponse()
		{
			DataRegistry.Instance.MailboxEmailAddress = "client@client.domain.com";
			DataRegistry.Instance.MailboxDisplayName = "Client System";

			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details Is Long Enough";
			incident.IA_GS_NKReportingStaff = "AAA";
			Factory.Save();

			incident.Approve();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.Action = IncidentApprovalLookups.Actions.Email;
			response.ClientReferenceNumber = incident.IA_ClientReference;
			response.IncidentNumber = "CS00001234";
			Xsd.CustomerServiceResponseEmail incidentEmail = response.IncidentEmails.AddNew();
			incidentEmail.Subject = "Test Email Subject";
			incidentEmail.Body = "Test Email Body";
			incidentEmail.ToEmailAddress = "luong@cargowise.com";
			incidentEmail.Cc = "adl@cargowise.com";

			CustomerServiceResponseHelper helper = new CustomerServiceResponseHelper(response, incident);
			helper.ProcessAndSave();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Test Email Subject", email.Subject);
			AssertEquals("Test Email Body", email.Body);
			AssertEquals("NoReply@client.domain.com", email.ReplyTo);
			AssertEquals("client@client.domain.com", email.FromAddress);
			AssertEquals("Client System", email.FromDisplayName);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("luong@cargowise.com", email.Recipients[0].Email);
			AssertEquals(1, email.CCRecipients.Count);
			AssertEquals("adl@cargowise.com", email.CCRecipients[0].Email);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Xsd.CustomerServiceResponse response2 = new Xsd.CustomerServiceResponse();
			response2.Action = IncidentApprovalLookups.Actions.Email;
			response2.ClientReferenceNumber = incident.IA_ClientReference;
			response2.IncidentNumber = "CS00001234";
			Xsd.CustomerServiceResponseEmail incidentEmail2 = response2.IncidentEmails.AddNew();
			incidentEmail2.Subject = "Test Email Subject with no reply to";
			incidentEmail2.Body = "Test Email Body with no reply to";
			incidentEmail2.ToEmailAddress = "luong@cargowise.com; andrew@cargowise.com";
			incidentEmail2.Cc = "andrew.luong@cargowise.com; adl@cargowise.com";

			helper = new CustomerServiceResponseHelper(response2, incident);
			helper.ProcessAndSave();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Test Email Subject with no reply to", email.Subject);
			AssertEquals("Test Email Body with no reply to", email.Body);
			AssertEquals("client@client.domain.com", email.FromAddress);
			AssertEquals("Client System", email.FromDisplayName);
			AssertEquals("NoReply@client.domain.com", email.ReplyTo);
			AssertEquals(2, email.Recipients.Count);
			AssertEquals("luong@cargowise.com", email.Recipients[0].Email);
			AssertEquals("andrew@cargowise.com", email.Recipients[1].Email);
			AssertEquals(2, email.CCRecipients.Count);
			AssertEquals("andrew.luong@cargowise.com", email.CCRecipients[0].Email);
			AssertEquals("adl@cargowise.com", email.CCRecipients[1].Email);

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "support@cargowise.com";
			incident.IA_GS_NKReportingStaff = staff1.GS_Code;
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "sam@test.com";
			incident.IA_GS_NKApprovingStaff = staff2.GS_Code;

			Xsd.CustomerServiceResponse response3 = new Xsd.CustomerServiceResponse();
			response3.Action = IncidentApprovalLookups.Actions.Email;
			response3.ClientReferenceNumber = incident.IA_ClientReference;
			response3.IncidentNumber = "CS00001234";
			response3.SystemReservedEmailAddresses = "SUPPORT@cargowise.com";
			Xsd.CustomerServiceResponseEmail incidentEmail3 = response3.IncidentEmails.AddNew();
			incidentEmail3.Subject = "Test Email Subject with no recipient email address";
			incidentEmail3.Body = "Test Email Body with no recipient eamil address";
			incidentEmail3.ToEmailAddress = "";
			incidentEmail3.Cc = "";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			helper = new CustomerServiceResponseHelper(response3, incident);
			helper.ProcessAndSave();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("sam@test.com", email.Recipients[0].Email);
			AssertEquals(0, email.CCRecipients.Count);
		}

		public void TestSendEmailsFromResponse_NoException()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details Is Long Enough";
			incident.IA_GS_NKReportingStaff = "AAA";
			Factory.Save();

			Xsd.CustomerServiceResponse response3 = new Xsd.CustomerServiceResponse();
			response3.Action = IncidentApprovalLookups.Actions.Email;
			response3.ClientReferenceNumber = incident.IA_ClientReference;
			response3.IncidentNumber = "CS00001234";
			Xsd.CustomerServiceResponseEmail incidentEmail3 = response3.IncidentEmails.AddNew();
			incidentEmail3.Subject = "Test Email Subject with no reply to";
			incidentEmail3.Body = "Test Email Body with no reply to";
			incidentEmail3.ToEmailAddress = "sam@test.com";

			DataRegistry.Instance.MailboxEmailAddress = "";
			DataRegistry.Instance.SMTPDefaultReturnEmailAddress = "";
			var notifications = new NotificationsForTest();
			var helper = new CustomerServiceResponseHelper(response3, incident, notifications);
			helper.ProcessAndSave();
			AssertNotEquals("Should contains error message", "", notifications.ToString());
		}

		public void TestSendEmailsToNotificationRecipientsFromResponse()
		{
			SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, IncidentApprovalLookups.EmailRecipientSettings.AllParties);
			DataRegistry.Instance.MailboxEmailAddress = "client@client.domain.com";
			DataRegistry.Instance.MailboxDisplayName = "Client System";

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

			CustomerServiceResponseHelper helper = new CustomerServiceResponseHelper(response, incident);
			helper.ProcessAndSave();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(3, email.Recipients.Count);
			AssertEquals("Sam@test.com.au", email.Recipients[0].Email);
			AssertEquals("Tester@test.com.au", email.Recipients[1].Email);
			AssertEquals("aaa@test.com.au", email.Recipients[2].Email);
			AssertEquals("Test Email Subject", email.Subject);
			AssertEquals("Test Email Body", email.Body);

			AssertEquals("client@client.domain.com", email.FromAddress);
			AssertEquals("Client System", email.FromDisplayName);
			AssertEquals("NoReply@client.domain.com", email.ReplyTo);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			using (Env.SetTemporaryUserContext(reportedByStaff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				helper.ProcessAndSave();

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(3, email.Recipients.Count);
				AssertEquals("Sam@test.com.au", email.Recipients[0].Email);
				AssertEquals("Tester@test.com.au", email.Recipients[1].Email);
				AssertEquals("aaa@test.com.au", email.Recipients[2].Email);
			}

			Env.OutgoingMailManager.EmailsCreated.Clear();
			using (Env.SetTemporaryUserContext(approvingStaff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				helper.ProcessAndSave();

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(3, email.Recipients.Count);
				AssertEquals("Sam@test.com.au", email.Recipients[0].Email);
				AssertEquals("Tester@test.com.au", email.Recipients[1].Email);
				AssertEquals("aaa@test.com.au", email.Recipients[2].Email);
			}

			Env.OutgoingMailManager.EmailsCreated.Clear();
			using (Env.SetTemporaryUserContext(reportingStaff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				helper.ProcessAndSave();

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(3, email.Recipients.Count);
				AssertEquals("Sam@test.com.au", email.Recipients[0].Email);
				AssertEquals("Tester@test.com.au", email.Recipients[1].Email);
				AssertEquals("aaa@test.com.au", email.Recipients[2].Email);
			}
		}

		public void TestLinkToIncidentShouldUseIncidentLicenceCode()
		{
			GlbStaff reportingStaff = Factory.NewWithValidTestData<GlbStaff>();
			reportingStaff.GS_Code = "AAA";
			reportingStaff.GS_EmailAddress = "aaa@test.com.au";
			reportingStaff.GS_LoginName = "aaa";

			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details Is Long Enough";
			incident.IA_GS_NKApprovingStaff = "AAA";
			incident.IA_LicenceCode = "AAABBBCCC";
			Factory.Save();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.ClientReferenceNumber = incident.IA_ClientReference;
			response.IncidentNumber = "CS00001234";
			response.Status = IncidentApprovalLookups.StatusCodes.SupportTeam;
			response.Action = IncidentApprovalLookups.Actions.Update;
			Xsd.CustomerServiceResponseEmail incidentEmail = response.IncidentEmails.AddNew();
			incidentEmail.Subject = "Test Email Subject";
			incidentEmail.Body = "(*ServiceRequestLink*)";

			CustomerServiceResponseHelper helper = new CustomerServiceResponseHelper(response, incident);
			helper.ProcessAndSave();

			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("Link should contain no licence code", "edient:Command=ShowEditForm&LicenceCode=AAABBBCCC&ControllerID=ServiceRequest&BusinessEntityPK=" + incident.PK.ToString(), email.Body);
		}

		public void TestMutex()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_ClientReference = "SR00001001";
			incident.IA_IncidentNumber = "CS00003000";
			incident.IA_LicenceCode = "DDDCOMSYD";
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.SupportTeam;
			incident.IA_Module = "COR";
			incident.IA_IncidentSummary = "Test Summary";
			incident.IA_IncidentDetails = "Test Details";
			Factory.Save();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.ClientReferenceNumber = "SR00001001";
			response.IncidentNumber = "CS00003000";
			response.LicenceCode = "DDDCOMMEL";
			response.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			response.Status = IncidentApprovalLookups.StatusCodes.DevelopmentTeam;
			response.Module = "SAL";
			response.IncidentSummary = "Test Summary (Modified)";
			response.IncidentDetails = "Test Details (Modified)";
			response.SentTimeUtc = new ZDateTime(2012, 6, 11, 15, 30, 0).ToString("o");

			var notifications = new NotificationsForTest();
			CustomerServiceResponseHelper helper = new CustomerServiceResponseHelper(response, incident, notifications);
			string log = string.Empty;

			ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.IncidentApprovalUpdate, incident.PK.ToString());
			try
			{
				if (mutex.Lock())
				{
					helper.ProcessAndSave();

					IncidentApproval loadedIncident1 = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
					loadedIncident1.LastSyncTime = new ZDateTime(2012, 6, 11, 15, 30, 1);
					loadedIncident1.Factory.Save();
				}
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}

			AssertEquals(notifications.ToString(), "Reached maximum trial count to acquire lock to save. Changes are not saved.");

			IncidentApproval loadedIncident2 = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			AssertEquals("Last sync time has been updated", new ZDateTime(2012, 6, 11, 15, 30, 1), loadedIncident2.LastSyncTime);
			AssertEquals("Old change not accepted", "DDDCOMSYD", loadedIncident2.IA_LicenceCode);
			AssertEquals("Old change not accepted", Constants.CustomerService.CriticalityCodes.CR5_Training, loadedIncident2.IA_Criticality);
			AssertEquals("Old change not accepted", IncidentApprovalLookups.StatusCodes.SupportTeam, loadedIncident2.IA_Status);
			AssertEquals("Old change not accepted", "COR", loadedIncident2.IA_Module);
			AssertEquals("Old change not accepted", "Test Summary", loadedIncident2.IA_IncidentSummary);
			AssertEquals("Old change not accepted", "Test Details", loadedIncident2.IA_IncidentDetails);
		}

		[TestDate(2012, 7, 13, 1, 45, 0)]
		public void TestLastSyncTimeIsSetInUtcKind()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_ClientReference = "SR00001001";
			incident.IA_IncidentNumber = "CS00003000";

			incident.LastSyncTime = new ZDateTime(2012, 7, 13, 1, 44, 58);
			Factory.Save();

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.ClientReferenceNumber = "SR00001001";
			response.IncidentNumber = "CS00003000";
			response.SentTimeUtc = ZDateTime.UtcNow.ToString("o");
			string packedMessage = eHubMessaging.Business.SystemMessage.Pack(ZArchitecture.Xml.ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse)), response, "a");
			string unpackedMessage = eHubMessaging.Business.SystemMessage.Unpack(packedMessage, "a");
			Xsd.CustomerServiceResponse receivedResponse = ValueObjectEncoder.Deserialize<Xsd.CustomerServiceResponse>(unpackedMessage);

			var notifications = new NotificationsForTest();
			CustomerServiceResponseHelper helper = new CustomerServiceResponseHelper(receivedResponse, incident, notifications);
			helper.ProcessAndSave();
			AssertEquals("", notifications.ToString());

			incident = new BusinessObjectFactory().Load<IncidentApproval>(incident.PK);
			ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "LastSyncTime");
			var logs = incident.Logs.Find(query);
			AssertEquals(1, logs.Length);
			AssertEquals("LastSyncTime 2012-07-13T01:45:00.0000000Z", logs[0].SL_Reference);
			AssertEquals(new ZDateTime(2012, 7, 13, 1, 45, 0), incident.LastSyncTime);
			AssertEquals(DateTimeKind.Utc, incident.LastSyncTime.ToDateTime().Kind);
		}

		#region IsValidModuleCode

		public void TestIsValidModuleCode()
		{
			var incident = Factory.New<IncidentApproval>();

			CombineAssertions("Should return true for all module codes in Lookups", () =>
			{
				foreach (ICodeDescription item in incident.Lookups.MenuSectionList)
				{
					AssertEquals(string.Format("IsValidModule({0}, ModuleListType.MenuSection)", item.Code), true, CustomerServiceResponseHelper.IsValidModuleCode_ExposedForTesting(item.Code, ModuleListType.MenuSection));
				}

				foreach (ICodeDescription item in incident.Lookups.Cr8ModuleList)
				{
					AssertEquals(string.Format("IsValidModule({0}, ModuleListType.Cr8)", item.Code), true, CustomerServiceResponseHelper.IsValidModuleCode_ExposedForTesting(item.Code, ModuleListType.Cr8));
				}

				foreach (ICodeDescription item in incident.Lookups.Cr9ModuleList)
				{
					AssertEquals(string.Format("IsValidModule({0}, ModuleListType.Cr9)", item.Code), true, CustomerServiceResponseHelper.IsValidModuleCode_ExposedForTesting(item.Code, ModuleListType.Cr9));
				}
			});

			CombineAssertions("Should return true for all module codes in ModuleTreeCustomerServiceMenuSectionList", () =>
			{
				foreach (var item in new ModuleTreeCustomerServiceMenuSectionList().Values)
				{
					AssertEquals(string.Format("IsValidModule({0}, ModuleListType.MenuSection)", item.Code), true, CustomerServiceResponseHelper.IsValidModuleCode_ExposedForTesting(item.Code, ModuleListType.MenuSection));
				}
			});
		}

		#endregion

		class NotificationsForTest : INotifications
		{
			readonly List<string> notifications = new List<string>();

			void INotifications.Add(INotification notification)
			{
				notifications.Add(notification.Message);
			}

			public override string ToString()
			{
				return string.Join("\r\n", notifications.ToArray());
			}
		}
	}
}
