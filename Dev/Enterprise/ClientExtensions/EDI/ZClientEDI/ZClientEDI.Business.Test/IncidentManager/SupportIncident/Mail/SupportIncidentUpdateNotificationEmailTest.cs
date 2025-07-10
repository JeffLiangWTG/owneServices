using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentUpdateNotificationEmailTest : TestCaseWithFactory
	{
		public void TestEmailToClient_WithIncidentUpdateNotificationEmailTemplate()
		{
			EDIDataRegistry.Instance.IncidentUpdateNotificationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "", "My customer IncidentUpdateNotificationEmailTemplate email template body - (*Summary*)."));

			var incident = CreateSupportIncident();

			var snapShot = new SupportIncidentStatusSnapShot();
			snapShot.TakeSnapShot(incident, true);

			var request = new Xsd.CustomerServiceRequest();
			request.IncidentNumber = "CS00008371";
			incident.IM_Description = "New summary with 'tag' <b>!";
			Factory.Save();

			var email = new SupportIncidentUpdateNotificationEmail(incident, snapShot, null, null);
			var response = new Xsd.CustomerServiceResponse();
			email.PublishClientEmailToResponse(response);

			AssertEquals(1, response.IncidentEmails.Count);
			var responseEmail = response.IncidentEmails[0];

			AssertContains("My customer IncidentUpdateNotificationEmailTemplate email template body - New summary with &#39;tag&#39; &lt;b&gt;!.", responseEmail.Body);
			AssertContains("<a href='(*ServiceRequestLink*)'>Incident number CS00008371 / SR00001023</a>", responseEmail.Body);
		}

		public void TestEmailBody_NoTrailingForwardSlashWhenNoClientIncidentReference()
		{
			var incident = CreateSupportIncident();
			incident.IM_ClientIncidentReference = string.Empty;

			var snapShot = new SupportIncidentStatusSnapShot();
			snapShot.TakeSnapShot(incident, true);

			var request = new Xsd.CustomerServiceRequest();
			request.IncidentNumber = incident.IM_IncidentNumber;
			incident.IM_Description = "New summary with 'tag' <b>!";
			Factory.Save();

			var email = new SupportIncidentUpdateNotificationEmail(incident, snapShot, null, null);
			var response = new Xsd.CustomerServiceResponse();
			email.PublishClientEmailToResponse(response);

			AssertEquals(1, response.IncidentEmails.Count);
			var responseEmail = response.IncidentEmails[0];

			AssertContains("<a href='(*ServiceRequestLink*)'>Incident number CS00008371</a>", responseEmail.Body);
		}

		public void TestEmailToClient_eHubMessage()
		{
			GlbCompany.CurrentCompany.GC_Name = "WiseTech Global";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = Org.PK;
			incident.IM_IncidentNumber = "CS00008371";
			incident.IM_ClientIncidentReference = "SR00001023";
			incident.IM_Description = "It is a new incident";
			incident.DetailNoteText = "I have problem with the software.";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IM_Module = "COR";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.Assigned;
			Factory.Save();

			SupportIncidentStatusSnapShot snapShot = new SupportIncidentStatusSnapShot();
			snapShot.TakeSnapShot(incident, true);

			incident.IM_Description = "It is a new incident (updated)";
			incident.DetailNoteText = "I have problem with the software. (updated)";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Module = "SAL";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;

			Xsd.CustomerServiceResponseAttachmentCollection eDocsUpdate = new Xsd.CustomerServiceResponseAttachmentCollection();
			eDocsUpdate.AddNew();

			Xsd.ConversationMessageCollection eConversationUpdate = new Xsd.ConversationMessageCollection();
			Xsd.ConversationMessage message1 = eConversationUpdate.AddNew();
			message1.Body = "Thanks for your reply";
			message1.UserName = "Staff 1";
			Xsd.ConversationMessage message2 = eConversationUpdate.AddNew();
			message2.Body = "<div> is better than <span>Line 1\nLine2\n\nLine3\r\nLine4";
			message2.UserName = "Staff 2";

			Factory.Save();

			SupportIncidentUpdateNotificationEmail email = new SupportIncidentUpdateNotificationEmail(incident, snapShot, eDocsUpdate, eConversationUpdate);
			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			email.PublishClientEmailToResponse(response);

			AssertEquals(1, response.IncidentEmails.Count);
			Xsd.CustomerServiceResponseEmail responseEmail = response.IncidentEmails[0];

			AssertContains("<a href='(*ServiceRequestLink*)'>Incident number CS00008371 / SR00001023</a>", responseEmail.Body);
			AssertContains("Status has been updated from CSV (Customer Service Actioning) to DEV (Development Team Actioning)", responseEmail.Body);
			AssertContains("Criticality has been updated from CR5 (Training Questions) to CR4 (Single function not working with manual work around)", responseEmail.Body);
			AssertContains("Incident summary has been updated", responseEmail.Body);
			AssertContains("Incident details have been updated", responseEmail.Body);
			AssertContains("eConversation has been updated", responseEmail.Body);
			AssertContains("Thanks for your reply", responseEmail.Body);
			AssertContains("Should not include name+company of last messenge because of CustomerServiceLetter.html", "&lt;div&gt; is better than &lt;span&gt;Line 1<br />Line2<br /><br />Line3<br />Line4", responseEmail.Body);
			AssertContains("eDocs have been updated", responseEmail.Body);
		}

		public void TestEmailToClient_WebServiceCall()
		{
			SupportIncident incident = CreateSupportIncident();
			incident.IM_OH_Client = Org.PK;
			GlbCompany.CurrentCompany.GC_Name = "WiseTech Global";

			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.IncidentNumber = "CS00008371";
			request.ClientReferenceNumber = "SR00001023";
			request.IncidentSummary = "It is a new incident";
			request.IncidentDetails = "I have problem with the software.";
			request.Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			request.Module = "COR";
			request.Status = SupportIncidentLookups.LegacyStatusCodes.SupportTeam;

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.IncidentNumber = "CS00008371";
			response.ClientReferenceNumber = "SR00001023";
			response.IncidentSummary = "It is a new incident (updated)";
			response.IncidentDetails = "I have problem with the software. (updated)";
			response.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			response.Module = "SAL";
			response.Status = SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam;
			response.Attachments.AddNew();
			Xsd.ConversationMessage message1 = response.IncidentConversationUpdate.AddNew();
			message1.Body = "Thanks for your reply";
			message1.UserName = "Staff 1";
			Xsd.ConversationMessage message2 = response.IncidentConversationUpdate.AddNew();
			message2.Body = "<div> is better than <span>Line 1\nLine2\n\nLine3\r\nLine4";
			message2.UserName = "Staff 2";

			SupportIncidentUpdateNotificationEmail email = new SupportIncidentUpdateNotificationEmail(incident, request, response);
			email.PublishClientEmailToResponse(response);

			AssertEquals(1, response.IncidentEmails.Count);
			Xsd.CustomerServiceResponseEmail responseEmail = response.IncidentEmails[0];

			AssertContains("<a href='(*ServiceRequestLink*)'>Incident number CS00008371 / SR00001023</a>", responseEmail.Body);
			AssertContains("Status has been updated from CSV (Customer Service Actioning) to DEV (Development Team Actioning)", responseEmail.Body);
			AssertContains("Criticality has been updated from CR5 (Training Questions) to CR4 (Single function not working with manual work around)", responseEmail.Body);
			AssertContains("Incident summary has been updated", responseEmail.Body);
			AssertContains("Incident details have been updated", responseEmail.Body);
			AssertContains("eConversation has been updated", responseEmail.Body);
			AssertContains("Thanks for your reply", responseEmail.Body);
			AssertContains("Should not include name+company of last messenge because of CustomerServiceLetter.html", "&lt;div&gt; is better than &lt;span&gt;Line 1<br />Line2<br /><br />Line3<br />Line4", responseEmail.Body);
			AssertContains("eDocs have been updated", responseEmail.Body);
		}

		public void TestEmailToClient_WebServiceCallWithUnsavedChanges()
		{
			SupportIncident incident = CreateSupportIncident();

			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.IncidentNumber = "CS00008371";
			request.ClientReferenceNumber = "SR00001023";
			request.IncidentSummary = "It is a new incident 111";
			request.IncidentDetails = "I have problem with the software. 222";
			request.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			request.Module = "COR";
			request.Status = SupportIncidentLookups.LegacyStatusCodes.SupportTeam;
			request.Action = SupportIncidentLookups.LegacyActions.Update;
			request.OriginalIncidentSummary = "It is a new incident";
			request.OriginalIncidentDetails = "I have problem with the software.";
			request.OriginalCriticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			request.OriginalStatus = SupportIncidentLookups.LegacyStatusCodes.SupportTeam;

			Xsd.CustomerServiceResponse response = new Xsd.CustomerServiceResponse();
			response.IncidentNumber = "CS00008371";
			response.ClientReferenceNumber = "SR00001023";
			response.IncidentSummary = "It is a new incident";
			response.IncidentDetails = "I have problem with the software.";
			response.Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			response.Module = "COR";
			response.Status = SupportIncidentLookups.LegacyStatusCodes.SupportTeam;

			SupportIncidentUpdateNotificationEmail email = new SupportIncidentUpdateNotificationEmail(incident, request, response);
			email.PublishClientEmailToResponse(response);
			AssertEquals(0, response.IncidentEmails.Count);
		}

		public void TestEmailToStaff()
		{
			SupportIncident incident = CreateSupportIncident();
			incident.IM_OH_Client = Org.PK;
			GlbCompany.CurrentCompany.GC_Name = "WiseTech Global";

			SupportIncidentStatusSnapShot snapShot = new SupportIncidentStatusSnapShot();
			snapShot.TakeSnapShot(incident, true);

			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.IncidentNumber = "CS00008371";
			request.ClientReferenceNumber = "SR00001023";
			request.IncidentSummary = "It is a new incident (updated)";
			request.IncidentDetails = "I have problem with the software. (updated)";
			request.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			request.Module = "SAL";
			request.Status = SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam;
			request.Attachments.AddNew();
			Xsd.ConversationMessage message1 = request.IncidentConversationUpdate.AddNew();
			message1.Body = "Thanks for your reply";
			message1.UserName = "Asami Sato";
			Xsd.ConversationMessage message2 = request.IncidentConversationUpdate.AddNew();
			message2.Body = "<div> is better than <span>Line 1\nLine2\n\nLine3\r\nLine4";
			message2.UserName = "Hiroshi Sato";

			incident.IM_Description = "It is a new incident (updated)";
			incident.DetailNoteText = "I have problem with the software. (updated)";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Module = "SAL";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			Factory.Save();

			SupportIncidentUpdateNotificationEmail email = new SupportIncidentUpdateNotificationEmail(incident, request, snapShot);
			email.BuildAndSendEmailToAssignedStaff();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef responseEmail = Env.OutgoingMailManager.EmailsCreated[0];

			AssertContains("<a href=\"edient:Command=ShowEditForm&ControllerID=SupportIncident&BusinessEntityPK=", responseEmail.Body);
			AssertContains("Incident number CS00008371 / SR00001023", responseEmail.Body);
			AssertNotContains("Status has been updated from CSV (Customer Service Actioning) to DEV (Development Team Actioning)", responseEmail.Body);
			AssertContains("Criticality has been updated from CR5 (Training Questions) to CR4 (Single function not working with manual work around)", responseEmail.Body);
			AssertContains("Incident summary has been updated", responseEmail.Body);
			AssertContains("Incident details have been updated", responseEmail.Body);
			AssertContains("eConversation has been updated", responseEmail.Body);
			AssertContains("Thanks for your reply", responseEmail.Body);
			AssertContains("Should include name+company of last messenge", "&lt;div&gt; is better than &lt;span&gt;Line 1<br />Line2<br /><br />Line3<br />Line4<br /><br />Hiroshi Sato<br />Future Industries", responseEmail.Body);
			AssertContains("eDocs have been updated", responseEmail.Body);
			responseEmail.Headers.TryGetValue(SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress, out var header);
			AssertEquals(SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, header);
		}

		public void TestEmailToStaff_WhenClosedReopenRuleIsNEV()
		{
			SupportIncident incident = CreateSupportIncident();
			incident.IM_OH_Client = Org.PK;
			GlbCompany.CurrentCompany.GC_Name = "WiseTech Global";

			SupportIncidentStatusSnapShot snapShot = new SupportIncidentStatusSnapShot();
			snapShot.TakeSnapShot(incident, true);

			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.IncidentNumber = "CS00008371";
			request.ClientReferenceNumber = "SR00001023";
			request.IncidentSummary = "It is a new incident (updated)";
			request.IncidentDetails = "I have problem with the software. (updated)";
			request.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			request.Module = "SAL";
			request.Status = SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam;
			request.Attachments.AddNew();
			Xsd.ConversationMessage message1 = request.IncidentConversationUpdate.AddNew();
			message1.Body = "Thanks for your reply";
			message1.UserName = "Asami Sato";
			Xsd.ConversationMessage message2 = request.IncidentConversationUpdate.AddNew();
			message2.Body = "<div> is better than <span>Line 1\nLine2\n\nLine3\r\nLine4";
			message2.UserName = "Hiroshi Sato";

			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Description = "It is a new incident (updated)";
			incident.DetailNoteText = "I have problem with the software. (updated)";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Module = "SAL";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;

			productResolutionAndClosureBehaviour.ClosedReopenRule = "NEV";
			Factory.Save();
			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				Factory.ClearCachedValue<ResolutionAndClosureBehaviour>("SupportIncidentLookups.GetResolutionAndClosureBehaviour:" + "ENT" + ":" + criticalityResolutionAndClosureBehaviour.PK);
				var closedReopenRule = incident.GetClosedReopenRule();
				AssertEquals("ClosedReopenRule should be NEV", ResolutionAndClosureBehaviour.Constants.Code.NeverAllow, closedReopenRule);

				void AssertEmailBody(EmailDef emailDef)
				{
					CombineAssertions(() =>
					{
						var emailDefBody = emailDef.Body;
						AssertContains("<a href=\"edient:Command=ShowEditForm&ControllerID=SupportIncident&BusinessEntityPK=", emailDefBody);
						AssertContains("Incident number CS00008371 / SR00001023", emailDefBody);
						AssertNotContains("Status has been updated from CSV (Customer Service Actioning) to DEV (Development Team Actioning)", emailDefBody);
						AssertContains("Criticality has been updated from CR5 (Training Questions) to CR4 (Single function not working with manual work around)", emailDefBody);
						AssertContains("Incident summary has been updated", emailDefBody);
						AssertContains("Incident details have been updated", emailDefBody);
						AssertContains("eConversation has been updated", emailDefBody);
						AssertContains("Thanks for your reply", emailDefBody);
						AssertContains("Should include name+company of last messenge", "&lt;div&gt; is better than &lt;span&gt;Line 1<br />Line2<br /><br />Line3<br />Line4<br /><br />Hiroshi Sato<br />Future Industries", emailDefBody);
						AssertContains("eDocs have been updated", emailDefBody);
					});
				}

				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
				incident.Factory.Save();
				AssertEquals(SupportIncidentLookups.LegacyStatusCodes.ClosedAwaitingResponse, incident.Request.INC_Status);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var email = new SupportIncidentUpdateNotificationEmail(incident, request, snapShot);
				email.BuildAndSendEmailToAssignedStaff();
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var responseEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEmailBody(responseEmail);
				AssertNotContains("Only closed request should contain this text", "Attention: A reply was received to this Incident that has been permanently closed. An auto-reply email was sent to the customer with regards to the status of the ticket.", responseEmail.Body);
				AssertNotContains("Only closed request should contain this text", "You can override the status via Action > Re-open from the Incident form.", responseEmail.Body);
				responseEmail.Headers.TryGetValue(SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress, out var header);
				AssertEquals(SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, header);

				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
				incident.Factory.Save();
				AssertEquals(SupportIncidentLookups.LegacyStatusCodes.Closed, incident.Request.INC_Status);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var closedEmail = new SupportIncidentUpdateNotificationEmail(incident, request, snapShot);
				closedEmail.BuildAndSendEmailToAssignedStaff();
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var closedResponseEmail = Env.OutgoingMailManager.EmailsCreated[0];

				AssertEmailBody(closedResponseEmail);
				AssertContains("Only closed request should contain this text", "Attention: A reply was received to this Incident that has been permanently closed. An auto-reply email was sent to the customer with regards to the status of the ticket.", closedResponseEmail.Body);
				AssertContains("Only closed request should contain this text", "You can override the status via Action > Re-open from the Incident form.", closedResponseEmail.Body);
				responseEmail.Headers.TryGetValue(SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress, out header);
				AssertEquals(SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, header);
			}
		}

		public void TestBuildAndSendEmailToAssignedStaffAndOtherSubscribers()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "123";
			staff.GS_EmailAddress = "123@123.com";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var task = incident.WorkflowItems.AddNew();

			var participantEML = incident.EConversation.Conversation.Participants.AddNew();
			participantEML.EmailAddress = "os@os.com";

			var participantEML2 = incident.EConversation.Conversation.Participants.AddNew();
			participantEML2.EmailAddress = "os2@os.com";

			task.P9_Type = "WRK";
			task.P9_GS_NKAssignedStaffMember = "123";

			Factory.Save();

			var customerMessage = incident.EConversation.Conversation.Messages.AddNew();
			var participant = incident.EConversation.Conversation.Participants.AddNewParticipant(contact);
			customerMessage.JCM_Body = "11223344";
			customerMessage.JCM_JCP_Participant = participant.PK;
			customerMessage.JCM_IsInternal = false;

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			SupportIncidentUpdateNotificationEmail.SendEmailToAssignedStaffAndOtherSubscribers(incident, incident.OriginalSnapShot, new List<IConversationMessage> { customerMessage }, shouldSaveEmail: true, shouldSendIncidentUpdateEmail: true);

			var createdEmails = Env.OutgoingMailManager.EmailsCreated;

			var staffEmailDef = createdEmails.FirstOrDefault(x => x.Subject.Equals($"Customer Service Incident {incident.Number} Has Been Updated") && x.Recipients.Count == 1);
			var conversationEmailDef = createdEmails.Where(x => x.Body.Contains("You will find the new messages below, with some previous messages to provide additional context.")).ToList();

			AssertNotNull(staffEmailDef);
			AssertEquals(2, conversationEmailDef.Count);
			AssertEquals("staffEmailDef from address should be SupportEmailAddress", SupportIncidentLookups.SupportEmailAddress, staffEmailDef.FromAddress);
			AssertEquals("conversationEmailDef from address should be SupportEmailAddress", SupportIncidentLookups.SupportEmailAddress, conversationEmailDef[0].FromAddress);
			AssertEquals("conversationEmailDef from address should be SupportEmailAddress", SupportIncidentLookups.SupportEmailAddress, conversationEmailDef[1].FromAddress);

			AssertEquals("All participants should receive this email", 1, conversationEmailDef[0].Recipients.Count);
			AssertEquals("All participants should receive this email", 1, conversationEmailDef[1].Recipients.Count);
			if (conversationEmailDef[0].Recipients[0].Email == "os@os.com")
			{
				Assert(conversationEmailDef[1].Recipients[0].Email == "os2@os.com");
			}
			else
			{
				Assert(conversationEmailDef[0].Recipients[0].Email == "os2@os.com");
			}

			var factory = new BusinessObjectFactory();
			var staffEmail = factory.LoadTop1<MailItem>(new ZQuery(MailDBItemsSchema.MI_Body, staffEmailDef.Body));
			var conversationEmail = factory.LoadTop1<MailItem>(new ZQuery(MailDBItemsSchema.MI_Body, conversationEmailDef[0].Body));

			AssertNotNull("Staff email missed", staffEmail);
			Assert("Staff email missed", staffEmail.IsInDatabase);

			AssertNotNull("Customer email missed", conversationEmail);
			Assert("Customer email missed", conversationEmail.IsInDatabase);
		}

		public void TestShouldSendEmailToStaffOrgContactAndEmailSubscribers_WhenEConversationHaveNewMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "123";
			staff.GS_EmailAddress = "123@123.com";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact@test.com";
			var task = incident.WorkflowItems.AddNew();

			var participantEML2 = incident.EConversation.Conversation.Participants.AddNew();
			participantEML2.EmailAddress = "emailParticipant@os.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Future Industries";
			org.OH_RL_NKClosestPort = "AUSYD";
			var orgAddress = org.Addresses.MainAddress;
			orgAddress.OA_Email = "org@test.com";
			orgAddress.OA_Address1 = "address 1";
			Factory.Save();
			incident.EConversation.Conversation.Participants.AddNewParticipant(org);
			incident.EConversation.Conversation.Participants.AddNewParticipant(staff);

			task.P9_Type = "WRK";
			task.P9_GS_NKAssignedStaffMember = "123";

			Factory.Save();

			var customerMessage = incident.EConversation.Conversation.Messages.AddNew();
			var participant = incident.EConversation.Conversation.Participants.AddNewParticipant(contact);
			customerMessage.JCM_Body = "11223344";
			customerMessage.JCM_JCP_Participant = participant.PK;
			customerMessage.JCM_IsInternal = false;

			var customer = Factory.NewWithValidTestData<OrgContact>();
			customer.OC_Email = "customer@test.com";
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var customerMessage1 = incident.EConversation.Conversation.Messages.AddNew();
			var customerParticipant = incident.EConversation.Conversation.Participants.AddNewParticipant(customer);
			customerMessage1.JCM_Body = "666666";
			customerMessage1.JCM_JCP_Participant = customerParticipant.PK;
			customerMessage1.JCM_IsInternal = false;

			SupportIncidentUpdateNotificationEmail.SendEmailToAssignedStaffAndOtherSubscribers(incident, incident.OriginalSnapShot, new List<IConversationMessage> { customerMessage1 }, shouldSaveEmail: false);

			var createdEmails = Env.OutgoingMailManager.EmailsCreated;
			var conversationEmailDef = createdEmails.Where(x => x.Body.Contains("You will find the new messages below, with some previous messages to provide additional context."));
			AssertEquals("All participants should receive this email", 3, conversationEmailDef.Count());

			AssertEquals("conversationEmailDef from address should be SupportEmailAddress", SupportIncidentLookups.SupportEmailAddress, createdEmails[0].FromAddress);

			Assert("All the subscribed staff will receive an email", createdEmails[0].Recipients.Contains("123@123.com"));

			if (createdEmails[2].Recipients.Count == 2)
			{
				Assert("All the subscribed email follower will receive an email", createdEmails[1].Recipients.Contains("emailParticipant@os.com"));
				Assert("All the subscribed Contact will receive an email", createdEmails[2].Recipients.Contains("org@test.com"));
				Assert("All the subscribed Contact will receive an email", createdEmails[2].Recipients.Contains("contact@test.com"));
			}
			else
			{
				Assert("All the subscribed email follower will receive an email", createdEmails[2].Recipients.Contains("emailParticipant@os.com"));
				Assert("All the subscribed Contact will receive an email", createdEmails[1].Recipients.Contains("org@test.com"));
				Assert("All the subscribed Contact will receive an email", createdEmails[1].Recipients.Contains("contact@test.com"));
			}

			Assert("Those who reply to messages should not receive emails", !createdEmails[0].Recipients.Contains("customer@test.com"));
			Assert("Those who reply to messages should not receive emails", !createdEmails[1].Recipients.Contains("customer@test.com"));
			Assert("Those who reply to messages should not receive emails", !createdEmails[2].Recipients.Contains("customer@test.com"));

			Assert("The recipient should not include the sender", !createdEmails[0].Recipients.Contains(createdEmails[0].FromAddress));
			Assert("The recipient should not include the sender", !createdEmails[1].Recipients.Contains(createdEmails[0].FromAddress));
			Assert("The recipient should not include the sender", !createdEmails[2].Recipients.Contains(createdEmails[0].FromAddress));
		}

		#region Inactive User Email Subject

		public void TestEmailToStaff_ActiveCustServiceContact()
		{
			var email = SetUpAndGenerateEmail(false, true);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			email.BuildAndSendEmailToAssignedStaff();

			var emails = Env.OutgoingMailManager.EmailsCreated.Where(x => x.Subject.Contains("Has Been Updated"));
			AssertEquals(1, emails.Count());
			var response = emails.First();

			AssertContains("<a href=\"edient:Command=ShowEditForm&ControllerID=SupportIncident&BusinessEntityPK=", response.Body);
			AssertContains("Incident number CS00008371 / SR00001023", response.Body);
			AssertContains("eConversation has been updated", response.Body);
			AssertEquals(1, response.Recipients.Count);
			AssertEquals("TestMail@TestDomain.com", response.Recipients[0].Email);
			response.Headers.TryGetValue(SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress, out var header);
			AssertEquals(SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, header);
		}

		public void TestEmailToStaff_InActiveCustServiceContact()
		{
			var email = SetUpAndGenerateEmail(false, false);
			email.BuildAndSendEmailToAssignedStaff();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			var emails = Env.OutgoingMailManager.EmailsCreated.Where(x => !x.Body.Contains("has been assigned"));
			AssertEquals(1, emails.Count());
			var response = emails.First();

			AssertContains("<a href=\"edient:Command=ShowEditForm&ControllerID=SupportIncident&BusinessEntityPK=", response.Body);
			AssertContains("Incident number CS00008371 / SR00001023", response.Body);
			AssertEquals("Incident Customer Service Staff is Unavailable", response.Subject);
			AssertContains("eConversation has been updated", response.Body);
			AssertEquals(1, response.Recipients.Count);
			AssertEquals("support@wisetechglobal.com", response.Recipients[0].Email);
			response.Headers.TryGetValue(SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress, out var header);
			AssertEquals(SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, header);
		}

		public void TestEmailToStaff_ActiveAssignedContact()
		{
			var email = SetUpAndGenerateEmail(true, true);
			email.Incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			email.BuildAndSendEmailToAssignedStaff();

			var emails = Env.OutgoingMailManager.EmailsCreated.Where(x => !x.Body.Contains("has been assigned"));
			AssertEquals(1, emails.Count());
			var response = emails.First();

			AssertContains("<a href=\"edient:Command=ShowEditForm&ControllerID=SupportIncident&BusinessEntityPK=", response.Body);
			AssertContains("Incident number CS00008371 / SR00001023", response.Body);
			AssertContains("eConversation has been updated", response.Body);
			AssertEquals(1, response.Recipients.Count);
			AssertEquals("TestMail@TestDomain.com", response.Recipients[0].Email);
			response.Headers.TryGetValue(SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress, out var header);
			AssertEquals(SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, header);
		}

		public void TestEmailToStaff_InActiveAssignedContact()
		{
			using (EDIDataRegistry.Instance.IncidentStaffUnavailableSupportNotificationEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var email = SetUpAndGenerateEmail(true, false);
				email.Incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
				email.BuildAndSendEmailToAssignedStaff();

				var emails = Env.OutgoingMailManager.EmailsCreated.Where(x => !x.Body.Contains("has been assigned"));
				AssertEquals(1, emails.Count());
				var response = emails.First();

				AssertContains("<a href=\"edient:Command=ShowEditForm&ControllerID=SupportIncident&BusinessEntityPK=", response.Body);
				AssertContains("Incident number CS00008371 / SR00001023", response.Body);
				AssertEquals("No Available Contact Assigned to Incident", response.Subject);
				AssertContains("eConversation has been updated", response.Body);
				AssertEquals(1, response.Recipients.Count);
				AssertEquals("support@wisetechglobal.com", response.Recipients[0].Email);
				response.Headers.TryGetValue(SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress, out var header);
				AssertEquals(SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, header);
			}
		}

		public void TestEmailToStaff_InActiveAssignedContact_NotificationDisabled()
		{
			using (EDIDataRegistry.Instance.IncidentStaffUnavailableSupportNotificationEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var email = SetUpAndGenerateEmail(true, false);
				email.Incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
				email.BuildAndSendEmailToAssignedStaff();

				var emails = Env.OutgoingMailManager.EmailsCreated.Where(x => !x.Body.Contains("has been assigned"));
				AssertEquals(0, emails.Count());
			}
		}

		public void TestEmailToStaff_InActiveProductAreaAssignee()
		{
			GlbStaff pm1 = Factory.NewWithValidTestData<GlbStaff>();
			pm1.GS_Code = "PM1";
			pm1.GS_EmailAddress = "pm1@test.com";
			pm1.GS_IsActive = false;

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IM_IncidentNumber = "CS00004001";
			incident.IM_ClientIncidentReference = "SR00001101";

			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "Module With Product Area Staff Assignment", ProductAreaList.Codes.ARC, false);
			product.ModuleMappings.AddNew("BBB", "Module With Product Area But No Staff Assignment", ProductAreaList.Codes.DOM, false);
			product.ModuleMappings.AddNew("CCC", "Module With No Product Area", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			ProductAreaAssignmentCollection assignmentCollection = new ProductAreaAssignmentCollection();
			ProductAreaAssignment assignment1 = assignmentCollection.AddNew();
			assignment1.ProductArea = ProductAreaList.Codes.ARC;
			assignment1.Staff = pm1.GS_Code;
			EDIDataRegistry.Instance.ProductAreaAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, assignmentCollection);

			AssertEmailFallback("support@wisetechglobal.com", incident, SupportIncidentCategoriesList.Codes.Defect, "AAA", SupportIncidentLookups.Status.Closed);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("No Available Contact Assigned to Incident", email.Subject);

			AssertEmailFallback("support@wisetechglobal.com", incident, SupportIncidentCategoriesList.Codes.FeatureRequest, "AAA", SupportIncidentLookups.Status.Closed);
			email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("No Available Contact Assigned to Incident", email.Subject);
		}

		#endregion

		#region Email Fallback

		public void TestEmailToStaff_RecipientFallback()
		{
			GlbStaff pm1 = Factory.NewWithValidTestData<GlbStaff>();
			pm1.GS_Code = "PM1";
			pm1.GS_EmailAddress = "pm1@test.com";

			GlbStaff user1 = Factory.NewWithValidTestData<GlbStaff>();
			user1.GS_Code = "US1";
			user1.GS_EmailAddress = "tester@test.com";

			GlbStaff user2 = Factory.NewWithValidTestData<GlbStaff>();
			user2.GS_Code = "US2";
			user2.GS_EmailAddress = "customer.service@test.com";

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IM_IncidentNumber = "CS00004001";
			incident.IM_ClientIncidentReference = "SR00001101";

			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "Module With Product Area Staff Assignment", ProductAreaList.Codes.ARC, false);
			product.ModuleMappings.AddNew("BBB", "Module With Product Area But No Staff Assignment", ProductAreaList.Codes.DOM, false);
			product.ModuleMappings.AddNew("CCC", "Module With No Product Area", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			ProductAreaAssignmentCollection assignmentCollection = new ProductAreaAssignmentCollection();
			ProductAreaAssignment assignment1 = assignmentCollection.AddNew();
			assignment1.ProductArea = ProductAreaList.Codes.ARC;
			assignment1.Staff = pm1.GS_Code;
			EDIDataRegistry.Instance.ProductAreaAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, assignmentCollection);

			CombineAssertions(() =>
			{
				AssertEmailFallback("pm1@test.com", incident, SupportIncidentCategoriesList.Codes.Support, "AAA", SupportIncidentLookups.Status.Working, "US1");
				AssertEmailFallback("customer.service@test.com", incident, SupportIncidentCategoriesList.Codes.Support, "AAA", SupportIncidentLookups.Status.Working, "", "US2");
				AssertEmailFallback("pm1@test.com", incident, SupportIncidentCategoriesList.Codes.Support, "AAA", SupportIncidentLookups.Status.Working);
				AssertEmailFallback("pm1@test.com", incident, SupportIncidentCategoriesList.Codes.Support, "AAA", SupportIncidentLookups.Status.Closed, "US1");
				AssertEmailFallback("support@wisetechglobal.com", incident, SupportIncidentCategoriesList.Codes.Support, "BBB", SupportIncidentLookups.Status.Closed, "US1");
				AssertEmailFallback("support@wisetechglobal.com", incident, SupportIncidentCategoriesList.Codes.Support, "CCC", SupportIncidentLookups.Status.Closed, "US1");
			});

			CombineAssertions(() =>
			{
				AssertEmailFallback("tester@test.com", incident, SupportIncidentCategoriesList.Codes.Defect, "AAA", SupportIncidentLookups.Status.Working, "US1");
				AssertEmailFallback("pm1@test.com", incident, SupportIncidentCategoriesList.Codes.Defect, "AAA", SupportIncidentLookups.Status.Working, "", "US2");
				AssertEmailFallback("tester@test.com", incident, SupportIncidentCategoriesList.Codes.Defect, "AAA", SupportIncidentLookups.Status.Working, "", "", "US1");
				AssertEmailFallback("pm1@test.com", incident, SupportIncidentCategoriesList.Codes.Defect, "AAA", SupportIncidentLookups.Status.Working);
				AssertEmailFallback("pm1@test.com", incident, SupportIncidentCategoriesList.Codes.Defect, "AAA", SupportIncidentLookups.Status.Closed);
				AssertEmailFallback("support@wisetechglobal.com", incident, SupportIncidentCategoriesList.Codes.Defect, "BBB", SupportIncidentLookups.Status.Closed);
				AssertEmailFallback("support@wisetechglobal.com", incident, SupportIncidentCategoriesList.Codes.Defect, "CCC", SupportIncidentLookups.Status.Closed);
			});

			CombineAssertions(() =>
			{
				AssertEmailFallback("tester@test.com", incident, SupportIncidentCategoriesList.Codes.FeatureRequest, "AAA", SupportIncidentLookups.Status.Working, "US1");
				AssertEmailFallback("pm1@test.com", incident, SupportIncidentCategoriesList.Codes.FeatureRequest, "AAA", SupportIncidentLookups.Status.Working, "", "US2");
				AssertEmailFallback("tester@test.com", incident, SupportIncidentCategoriesList.Codes.FeatureRequest, "AAA", SupportIncidentLookups.Status.Working, "", "", "US1");
				AssertEmailFallback("pm1@test.com", incident, SupportIncidentCategoriesList.Codes.FeatureRequest, "AAA", SupportIncidentLookups.Status.Working);
				AssertEmailFallback("pm1@test.com", incident, SupportIncidentCategoriesList.Codes.FeatureRequest, "AAA", SupportIncidentLookups.Status.Closed);
				AssertEmailFallback("support@wisetechglobal.com", incident, SupportIncidentCategoriesList.Codes.FeatureRequest, "BBB", SupportIncidentLookups.Status.Closed);
				AssertEmailFallback("support@wisetechglobal.com", incident, SupportIncidentCategoriesList.Codes.FeatureRequest, "CCC", SupportIncidentLookups.Status.Closed);
			});
		}

		void AssertEmailFallback(string expectedEmail, SupportIncident incident, string stage, string module, string status, string assignedTo = "", string custServiceContact = "", string lastClosedAssignedTo = "")
		{
			incident.IM_Module = module;
			incident.IM_Category = stage;
			incident.IM_Status = status;
			incident.IM_GS_NKAssignedToCurrent = assignedTo;
			incident.IM_GS_NKCustServiceContact = custServiceContact;

			incident.WorkflowItems.RemoveAndDeleteAll();
			if (!string.IsNullOrEmpty(lastClosedAssignedTo))
			{
				var lastClosedTask = incident.WorkflowItems.AddNew();
				lastClosedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				lastClosedTask.P9_GS_NKAssignedStaffMember = lastClosedAssignedTo;
			}

			Factory.Save();

			SupportIncidentStatusSnapShot snapShot = new SupportIncidentStatusSnapShot();
			snapShot.TakeSnapShot(incident, true);

			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.IncidentNumber = incident.IM_IncidentNumber;
			request.ClientReferenceNumber = incident.IM_ClientIncidentReference;
			Xsd.ConversationMessage message = request.IncidentConversationUpdate.AddNew();
			message.Body = "Thanks for your reply";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			SupportIncidentUpdateNotificationEmail email = new SupportIncidentUpdateNotificationEmail(incident, request, snapShot);
			email.BuildAndSendEmailToAssignedStaff();

			EmailDef responseEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(expectedEmail, responseEmail.Recipients[0].Email);
		}

		#endregion

		#region SendEmailToAssignedStaff

		public void TestSendEmailToAssignedStaffAndOtherSubscribers()
		{
			SupportIncident incident = CreateSupportIncident();
			incident.IM_OH_Client = Org.PK;
			var contact = Org.Contacts.AddNew();
			contact.OC_ContactName = "Joe Customer";
			contact.OC_Email = "Joe.Customer@test.com";
			var convo = incident.EConversation.JobConversationForTest;
			var participant = convo.Participants.AddNewParticipant(contact);

			var otherSubcriber1 = Org.Contacts.AddNew();
			otherSubcriber1.OC_ContactName = "Some One";
			otherSubcriber1.OC_Email = "someone@test.com";
			convo.Participants.AddNewParticipant(otherSubcriber1);

			var otherSubcriber2 = Org.Contacts.AddNew();
			otherSubcriber2.OC_ContactName = "support";
			otherSubcriber2.OC_Email = "support@wisetechglobal.com";
			convo.Participants.AddNewParticipant(otherSubcriber2);

			incident.IM_OC_Contact = contact.PK;
			GlbCompany.CurrentCompany.GC_Name = "WiseTech Global";
			incident.IM_GS_NKCustServiceContact = Env.CurrentUser.Initials;

			incident.IM_Description = "It is a new incident";
			incident.DetailNoteText = "I have problem with the software.";
			incident.IM_Module = "SAL";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			SupportIncidentStatusSnapShot snapShot = new SupportIncidentStatusSnapShot();
			snapShot.TakeSnapShot(incident, true);

			var msg1 = convo.Messages.AddNew(participant, "Thanks for your reply");
			msg1.JCM_IsLocal = false;
			var msg2 = convo.Messages.AddNew(participant, "<div> is better than <span>Line 1\nLine2\n\nLine3\r\nLine4");

			SupportIncidentUpdateNotificationEmail.SendEmailToAssignedStaffAndOtherSubscribers(incident, snapShot, new List<IConversationMessage> { msg1, msg2 }, shouldSaveEmail: true, shouldSendIncidentUpdateEmail: true);

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);

			CombineAssertions(() =>
			{
				var staffEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertContains("eConversation has been updated", staffEmail.Body);
				AssertContains("Thanks for your reply", staffEmail.Body);
				AssertContains("&lt;div&gt; is better than &lt;span&gt;Line 1<br />Line2<br /><br />Line3<br />Line4", staffEmail.Body);

				var contactEmail = Env.OutgoingMailManager.EmailsCreated[1];
				Assert("email to otherSubcriber1", contactEmail.Recipients.Contains(otherSubcriber1.OC_Email));
				Assert("no email to otherSubcriber2", !contactEmail.Recipients.Contains(otherSubcriber2.OC_Email));
				Assert("no email to original conversation sender", !contactEmail.Recipients.Contains(contact.OC_Email));
				AssertEquals("New Messages in CS00008371 - FUTINDSYD - It is a new incident", contactEmail.Subject);
				AssertContains("Thanks for your reply", contactEmail.Body);
				AssertContains("New messages have been added to <a href=\"/INC/Desktop#/formFlow/", contactEmail.Body);
				AssertNotContains("&lt;div&gt; is better than &lt;span&gt;Line 1<br />Line2<br />", contactEmail.Body);
			});
		}

		#endregion

		#region SubscriberUpdateEConversation

		public void TestCreateNewIncidentWillNotTriggerSubscriberUpdateEConversationForContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact@org.com";
			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();

			var createdEmails = Env.OutgoingMailManager.EmailsCreated;
			var subscriberUpdateEmailDef = createdEmails.FirstOrDefault(x => x.Subject.Contains($"New Messages in") && x.Recipients.Count == 1);
			AssertNull(subscriberUpdateEmailDef);
		}

		public void TestAddOrgParticipant_WhenAddNotEligibleOrgWillNotTriggerEmailNotification()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Address Test";
			org.MainAddress.OA_Email = "orgMain@org.com";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact@org.com";
			contact.OC_ContactName = "contact";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MainAddress.OA_Address1 = "org2 Address Test";
			org2.MainAddress.OA_Email = "orgMain@org2.com";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			incident.EConversation.Conversation.RelatedParties.AddNewParticipant(org2);
			Factory.Save();

			var createdEmails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("should have no email notification", 0, createdEmails.Count);
		}

		#endregion

		#region UnsubscriberEmailNotification

		public void TestMakeParticipantsUnsubscriberdWillSendEmailNotificationSeparately()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact@nintendo.com";
			contact.OC_ContactName = "contact";
			var luigiContact = org.Contacts.AddNew();
			luigiContact.OC_Email = "luigi@nintendo.com";
			luigiContact.OC_ContactName = "luigi";
			var toadContact = org.Contacts.AddNew();
			toadContact.OC_Email = "toad@nintendo.com";
			toadContact.OC_ContactName = "toad";
			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();

			incident.EConversation.Conversation.RelatedParties.AddNewParticipant(luigiContact);
			incident.EConversation.Conversation.RelatedParties.AddNewParticipant(toadContact);
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var luigiParticipant = incident.EConversation.Conversation.RelatedParties.FirstOrDefault(x => x.EmailAddress == "luigi@nintendo.com");
			luigiParticipant.JCP_IsSubscribed = false;
			var toadParticipant = incident.EConversation.Conversation.RelatedParties.FirstOrDefault(x => x.EmailAddress == "toad@nintendo.com");
			toadParticipant.JCP_IsSubscribed = false;
			Factory.Save();

			var unsubscriberdEmails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("should have 2 unsubscriberd Emails", 2, unsubscriberdEmails.Count);
			var luigiUnsubscriberdEmail = unsubscriberdEmails[0];
			Assert(luigiUnsubscriberdEmail.Subject.Contains("New Messages in") && luigiUnsubscriberdEmail.Body.Contains("You have been unsubscribed from"));
			Assert(luigiUnsubscriberdEmail.Recipients.Count == 1 && luigiUnsubscriberdEmail.Recipients.ToList<RecipientDef>().Any(x => x.Email == "luigi@nintendo.com"));

			var toadUnsubscriberdEmail = unsubscriberdEmails[1];
			Assert(toadUnsubscriberdEmail.Subject.Contains("New Messages in") && toadUnsubscriberdEmail.Body.Contains("You have been unsubscribed from"));
			Assert(toadUnsubscriberdEmail.Recipients.Count == 1 && toadUnsubscriberdEmail.Recipients.ToList<RecipientDef>().Any(x => x.Email == "toad@nintendo.com"));
		}

		#endregion

		#region Implementation

		OrgHeader Org
		{
			get
			{
				if (org == null)
				{
					org = Factory.NewWithValidTestData<OrgHeader>();
					org.OH_FullName = "Future Industries";
					org.OH_RL_NKClosestPort = "AUSYD";
				}
				return org;
			}
		}
		OrgHeader org;

		SupportIncidentUpdateNotificationEmail SetUpAndGenerateEmail(bool assignedToCurrent, bool staffIsActive)
		{
			var staff = CreateStaffMember(staffIsActive);

			SupportIncident incident = CreateSupportIncident();
			if (assignedToCurrent)
			{
				incident.IM_GS_NKAssignedToCurrent = staff.GS_Code;
			}
			else
			{
				incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			}
			Factory.Save();

			SupportIncidentStatusSnapShot snapShot = new SupportIncidentStatusSnapShot();
			snapShot.TakeSnapShot(incident, true);

			Xsd.CustomerServiceRequest request = CreateCustomerServiceRequest();

			ModifyIncident(incident);

			return new SupportIncidentUpdateNotificationEmail(incident, request, snapShot);
		}

		GlbStaff CreateStaffMember(bool isActive)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Han Solo";
			staff.GS_EmailAddress = "TestMail@TestDomain.com";
			staff.GS_FullName = "Hanold Solod";
			staff.GS_Code = "HS";
			staff.GS_IsActive = isActive;
			return staff;
		}

		SupportIncident CreateSupportIncident()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS00008371";
			incident.IM_ClientIncidentReference = "SR00001023";
			incident.IM_Description = "It is a new incident";
			incident.DetailNoteText = "I have problem with the software.";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR5_Training;
			incident.IM_Module = "COR";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.Assigned;
			Factory.Save();
			return incident;
		}

		Xsd.CustomerServiceRequest CreateCustomerServiceRequest()
		{
			Xsd.CustomerServiceRequest request = new Xsd.CustomerServiceRequest();
			request.IncidentNumber = "CS00008371";
			request.ClientReferenceNumber = "SR00001023";
			request.IncidentSummary = "It is a new incident (updated)";
			request.IncidentDetails = "I have problem with the software. (updated)";
			request.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			request.Module = "SAL";
			request.Status = SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam;
			request.Attachments.AddNew();
			Xsd.ConversationMessage message1 = request.IncidentConversationUpdate.AddNew();
			message1.Body = "Thanks for your reply";
			Xsd.ConversationMessage message2 = request.IncidentConversationUpdate.AddNew();
			message2.Body = "<div> is better than <span>Line 1\nLine2\n\nLine3\r\nLine4";

			return request;
		}

		void ModifyIncident(SupportIncident incident)
		{
			incident.IM_Description = "It is a new incident (updated)";
			incident.DetailNoteText = "I have problem with the software. (updated)";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Module = "SAL";
			Factory.Save();
		}

		#endregion
	}
}
