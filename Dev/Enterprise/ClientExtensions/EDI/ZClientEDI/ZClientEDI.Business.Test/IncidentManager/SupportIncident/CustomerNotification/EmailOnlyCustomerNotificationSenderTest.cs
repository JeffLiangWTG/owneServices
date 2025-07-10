using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class EmailOnlyCustomerNotificationSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			incident.AddStaffMessageToCustomer(@"From: <authentication mode=""Windows"" /> To: <authentication mode=""None"" />");

			var sender = new EmailOnlyCustomerNotificationSender();
			sender.SendChanges(incident, null);

			var emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("Email sent", 1, emails.Count);
			var email = emails[0];
			AssertContains("From: &lt;authentication mode=&quot;Windows&quot; /&gt; To: &lt;authentication mode=&quot;None&quot; /&gt;", email.Body);
		}

		public void TestSend_MultipleConversationMessages()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_ClientIncidentReference = "SR0023492";

			var sender = new EmailOnlyCustomerNotificationSender();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			incident.AddStaffMessageToCustomer("I require more information to investigate this issue.");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			incident.AddStaffMessageToCustomer("Second message that contains more details.");
			Factory.Save();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email1 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Update on Incident: CS00000001 - Test Incident", email1.Subject);
			AssertContains("CS00000001", email1.Body);
			AssertContains("I require more information to investigate this issue.", email1.Body);
			AssertContains("Second message that contains more details.", email1.Body);
			AssertContains("Incident details", email1.Body);
		}

		public void TestSendSubscriberUpdateEmailTemplateToStaffAndEmailAndOrgSubscribers_MultipleConversationMessages()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_ClientIncidentReference = "SR0023492";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "123";
			staff.GS_EmailAddress = "123@123.com";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "newuser2";
			staff2.GS_Code = "NE2";
			staff2.GS_EmailAddress = "newuser2@123.com";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact@test.com";

			var sender = new EmailOnlyCustomerNotificationSender();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);

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
			incident.EConversation.Conversation.Participants.AddNewParticipant(contact);
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (CurrentUserChanger.SwitchToNewUserTemporarily("newuser2"))
			{
				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
				incident.AddStaffMessageToCustomer("I require more information to investigate this issue.");
				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
				incident.AddStaffMessageToCustomer("Second message that contains more details.");
				Factory.Save();

				AssertEquals(3, Env.OutgoingMailManager.EmailsCreated.Count);

				var email1 = Env.OutgoingMailManager.EmailsCreated[1];
				AssertEquals("Update on Incident: CS00000001 - Test Incident", email1.Subject);
				AssertContains("CS00000001", email1.Body);
				AssertContains("I require more information to investigate this issue.", email1.Body);
				AssertContains("Second message that contains more details.", email1.Body);
				AssertContains("Incident details", email1.Body);
				AssertEquals("Should only contain contact recipients", 1, email1.Recipients.Count);
				AssertEquals("Should only contain contact recipients", "DDD@test.com", email1.Recipients[0].Email);
				AssertEquals("Should contain 2 Cc", 2, email1.CCRecipients.Count);
				Assert(email1.CCRecipients.OfType<RecipientDef>().Any(x => x.Email == "contact@test.com"));
				Assert(email1.CCRecipients.OfType<RecipientDef>().Any(x => x.Email == "org@test.com"));

				var email2 = Env.OutgoingMailManager.EmailsCreated[0];
				AssertContains("New Messages in", email2.Subject);
				AssertEquals("Should only contain staff recipients", "123@123.com", email2.Recipients[0].Email);
				var email3 = Env.OutgoingMailManager.EmailsCreated[2];
				AssertContains("New Messages in", email3.Subject);
				AssertEquals("Should only contain email recipients", "emailParticipant@os.com", email3.Recipients[0].Email);
			}
		}

		public void TestSendSubscriberUpdateEmailShouldHideStaffName()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_ClientIncidentReference = "SR0023492";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "123";
			staff.GS_EmailAddress = "123@123.com";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "newuser2";
			staff2.GS_Code = "NE2";
			staff2.GS_EmailAddress = "newuser2@123.com";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact@test.com";

			var sender = new EmailOnlyCustomerNotificationSender();
			incident.CustomerNotifier = new StaffChangeNotifierForTest(incident, sender);

			var participantEML2 = incident.EConversation.Conversation.Participants.AddNew();
			participantEML2.EmailAddress = "emailParticipant@os.com";

			Factory.Save();

			incident.EConversation.Conversation.Participants.AddNewParticipant(staff);
			incident.EConversation.Conversation.Participants.AddNewParticipant(contact);
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (CurrentUserChanger.SwitchToNewUserTemporarily("newuser2"))
			{
				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
				incident.AddStaffMessageToCustomer("I require more information to investigate this issue.");
				Factory.Save();

				AssertEquals(3, Env.OutgoingMailManager.EmailsCreated.Count);

				var email1 = Env.OutgoingMailManager.EmailsCreated[1];
				AssertEquals("Update on Incident: CS00000001 - Test Incident", email1.Subject);
				AssertContains("CS00000001", email1.Body);
				AssertContains("I require more information to investigate this issue.", email1.Body);
				AssertEquals("Should only contain contact recipients", 1, email1.Recipients.Count);
				AssertEquals("Should only contain contact recipients", "DDD@test.com", email1.Recipients[0].Email);
				AssertEquals("Should contain 1 Cc", 1, email1.CCRecipients.Count);
				Assert(email1.CCRecipients.OfType<RecipientDef>().Any(x => x.Email == "contact@test.com"));

				var email3 = Env.OutgoingMailManager.EmailsCreated[2];
				AssertContains("New Messages in", email3.Subject);
				AssertEquals("Should only contain email recipients", "emailParticipant@os.com", email3.Recipients[0].Email);
				var pattern = @"<h2>New Messages</h2>(.*?)<h2>Previous Messages</h2>";
				var match = Regex.Match(email3.Body, pattern, RegexOptions.Singleline);
				Assert(match.Success);
				var content = match.Groups[1].Value;
				AssertContains("I require more information to investigate this issue.", content);
				AssertContains("Customer Support", content);
			}
		}

		public void TestSend_EConversationUpdateToLegacySystem()
		{
			SupportIncident incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLegacyClient();
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details & < > '";
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			incident.AddStaffMessageToCustomer("I want to send out this message.\r\nSecond line.");
			Factory.Save();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Update on Incident: CS00000001 - Test Incident", email.Subject);
			var body = email.Body;
			AssertContains("CS00000001", body);
			AssertContains("I want to send out this message.", body);
			AssertContains("Second line.", body);
			AssertContains("<i>Incident details & < > '</i>", body);
		}
	}
}
