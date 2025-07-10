using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentCustomerNotificationSenderTest : TestCaseWithFactory
	{
		public void TestExtensionCreateAndSendConversationMessageToCustomer()
		{
			var template = EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.Value;
			template.EmailSubject = "Awaiting";
			EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			contact.OC_Email = "132@123.com";
			incident.IM_OH_Client = contact.OC_OH;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();

			Assert(incident.HasClientAndContact);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			incident.EConversation.AddMessageFromCurrentUser("HaHaHa", false, false, SupportIncidentEConversation.LocalMessageKind.ForCustomer);
			AssertEquals(1, incident.EConversation.GetNewLocalPublishedCustomerMessages().Count());

			var sender = new DummySender();
			sender.CreateAndSendConversationMessageToCustomer(incident, null);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(template.EmailSubject, Env.OutgoingMailManager.EmailsCreated[0].Subject);
			Assert(Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("HaHaHa"));

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = SupportIncidentEmailQuickBuilder.CreateIncidentClosedEmail(incident, true, "CLS", false).Item1;
			sender.CreateAndSendConversationMessageToCustomer(incident, email);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(template.EmailSubject, Env.OutgoingMailManager.EmailsCreated[0].Subject);
			Assert(Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("HaHaHa"));
		}

		static IEnumerable<IIncidentCustomerNotificationSender> GetSender()
		{
			yield return new EmailOnlyCustomerNotificationSender();
			yield return new ERequestEmailCustomerNotificationSender();
			yield return new ERequestEHubCustomerNotificationSender();
		}

		public void TestImplementedSender_SendChanges()
		{
			var awaitingTemplate = EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.Value;
			awaitingTemplate.EmailSubject = "Awaiting";
			EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, awaitingTemplate);

			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			contact.OC_Email = "132@123.com";
			incident.IM_OH_Client = contact.OC_OH;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LD = database.PK;
			Factory.Save();
			Assert(incident.HasClientAndContact);

			foreach (var sender in GetSender())
			{
				incident.EConversation.AddMessageFromCurrentUser("HaHaHa", false, false, SupportIncidentEConversation.LocalMessageKind.ForCustomer);
				AssertEquals(1, incident.EConversation.GetNewLocalPublishedCustomerMessages().Count());
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				sender.SendChanges(incident, null);
				AssertEquals($"[{sender.GetType().Name}]Awaiting Email should be sent if there is no email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals($"[{sender.GetType().Name}]Awaiting Email should be sent if there is no email", awaitingTemplate.EmailSubject, Env.OutgoingMailManager.EmailsCreated[0].Subject);
				Assert($"[{sender.GetType().Name}]Awaiting Email should be sent if there is no email", Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("HaHaHa"));

				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var email = SupportIncidentEmailQuickBuilder.CreateIncidentClosedEmail(incident, true, "CLS", false);
				sender.SendChanges(incident, email.Item1);

				AssertEquals($"[{sender.GetType().Name}]Awaiting Email should be sent if an email was sent already", 2, Env.OutgoingMailManager.EmailsCreated.Count);
				Assert($"[{sender.GetType().Name}]Awaiting Email should be sent if an email was sent already", Env.OutgoingMailManager.EmailsCreated.Any(x => x.Subject == awaitingTemplate.EmailSubject));
				Assert($"[{sender.GetType().Name}]Awaiting Email should be sent if there is no email", Env.OutgoingMailManager.EmailsCreated.Any(x => x.Body.Contains("HaHaHa")));
				Factory.Save();
			}
		}

		public void TestSendAwaitingResponseEmailShouldTriggerSendUpdateNotificationEmail()
		{
			var template = EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.Value;
			template.EmailSubject = "Awaiting";
			EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			contact.OC_Email = "132@123.com";
			incident.IM_OH_Client = contact.OC_OH;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();

			var emailParticipant = incident.EConversation.ExistingConversation.Participants.AddNew();
			emailParticipant.JCP_IsSubscribed = true;
			emailParticipant.JCP_EmailAddress = "archie@test.com";
			Factory.Save();

			Assert(incident.HasClientAndContact);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			incident.EConversation.AddMessageFromCurrentUser("HaHaHa", false, false, SupportIncidentEConversation.LocalMessageKind.ForCustomer);
			AssertEquals(1, incident.EConversation.GetNewLocalPublishedCustomerMessages().Count());

			var sender = new DummySender();
			sender.CreateAndSendConversationMessageToCustomer(incident, null);
			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);

			var awaitingEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(template.EmailSubject, awaitingEmail.Subject);
			Assert(awaitingEmail.Body.Contains("HaHaHa"));

			var subscriberUpdateEConversationEmail = Env.OutgoingMailManager.EmailsCreated[1];
			AssertContains("New Messages in", subscriberUpdateEConversationEmail.Subject);
			AssertEquals("subscriberUpdateEConversationEmail Recipients count", 1, subscriberUpdateEConversationEmail.Recipients.Count);
			AssertEquals("subscriberUpdateEConversationEmail Recipient", "archie@test.com", subscriberUpdateEConversationEmail.Recipients[0].Email);
		}

		public void TestSendIncidentClosedEmailShouldNotSendEmptyNotificationEmail()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			contact.OC_Email = "132@123.com";
			incident.IM_OH_Client = contact.OC_OH;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();

			var emailParticipant = incident.EConversation.ExistingConversation.Participants.AddNew();
			emailParticipant.JCP_IsSubscribed = true;
			emailParticipant.JCP_EmailAddress = "archie@test.com";
			Factory.Save();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = SupportIncidentEmailQuickBuilder.CreateIncidentClosedEmail(incident, true, "CLS", false).Item1;
			email.SendEmail();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var closedEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("has been Closed with reason", closedEmail.Subject);
		}

		public void TestSendIncidentResolvedEmailShouldNotSendEmptyNotificationEmail()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var contact = Factory.NewWithValidTestData<EDIOrgContact>();
				var incident = Factory.NewWithValidTestData<SupportIncident>();
				contact.OC_Email = "132@123.com";
				incident.IM_OH_Client = contact.OC_OH;
				incident.IM_OC_Contact = contact.PK;
				Factory.Save();

				var emailParticipant = incident.EConversation.ExistingConversation.Participants.AddNew();
				emailParticipant.JCP_IsSubscribed = true;
				emailParticipant.JCP_EmailAddress = "archie@test.com";
				Factory.Save();

				incident.CloseIncident("ZZZ", "");

				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var email = SupportIncidentEmailQuickBuilder.CreateIncidentResolvedEmail(incident, true, "ZZZ", false).Item1;
				email.SendEmail();

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

				var resolvedEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertContains("has been Resolved with reason", resolvedEmail.Subject);
			}
		}

		class DummySender : IIncidentCustomerNotificationSender
		{
			public void AddHyperlinks(SupportIncidentEmail email)
			{
			}

			public void SendChanges(SupportIncident incident, SupportIncidentEmail email)
			{
			}

			public void SendEmail(SupportIncidentEmail email)
			{
				email.SendEmail();
			}
		}
	}
}
