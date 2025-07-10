using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class WebRequestNotificationSenderTest : TestCaseWithFactory
	{
		public void TestAddHyperlinks()
		{
			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			var email = SupportIncidentEmail.New(incident);
			email.Body = "Line 1";
			var sender = new WebRequestNotificationSender();
			sender.AddHyperlinks(email);
			var expected = @"<a href='https://localhost/inc/" + incident.Request.PK + "?OrgCode=DDDCOM" + "'>Incident number CS000009</a>\r\n\r\nLine 1\r\n";
			AssertEquals(expected, email.Body);
		}

		public void TestSendChanges_CreateEmailFromConversationMessages()
		{
			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl);

			var testTemplate = new NotificationEmailTemplate();
			testTemplate.EmailBody = "ABCDEFG **/// NTZ /// $$ HIJKLM";
			testTemplate.EmailSubject = "AWR Subject";

			EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testTemplate);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			incident.AddStaffMessageToCustomer("Message 1 <html>");
			incident.AddStaffMessageToCustomer("Message 2");
			var sender = new WebRequestNotificationSender();
			sender.SendChanges(incident, null);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			var expectedSubject = EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.Value.EmailSubject;
			var expectedBody = EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate.Value.EmailBody;

			AssertEquals(expectedSubject, email.Subject);
			AssertContains(expectedBody, email.Body);

			var tokenPattern = @"token=[a-zA-Z0-9]+";
			var actionButtonsIgnoreToken = Regex.Replace(RequestHyperlinkButtonChecker.GetActionButtons(incident, shouldAddConfirmResolvedButton: true), tokenPattern, "token=IGNORED");
			var confirmResolvedButtonIgnoreToken = Regex.Replace(SupportIncidentEmailBodyGeneralControls.GetConfirmResolvedButton(incident), tokenPattern, "token=IGNORED");
			var emailBodyIgnoreToken = Regex.Replace(email.Body, tokenPattern, "token=IGNORED");

			AssertContains(actionButtonsIgnoreToken, emailBodyIgnoreToken);
			AssertContains(confirmResolvedButtonIgnoreToken, emailBodyIgnoreToken);
			AssertContains(SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident, isPrimaryButton: false), email.Body);
		}

		public void TestSendChanges_WithEmail()
		{
			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			incident.AddStaffMessageToCustomer("Message 1 <html>");
			incident.AddStaffMessageToCustomer("Message 2");

			var standardEmail = SupportIncidentEmail.New(incident);
			standardEmail.MarkAsNoNeedSaveToEDocs();
			standardEmail.Contact = incident.Contact;
			standardEmail.Subject = "Standard subject";
			standardEmail.Body = "Standard body";

			var sender = new WebRequestNotificationSender();
			sender.SendChanges(incident, standardEmail);

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated.First(x => x.Subject == standardEmail.Subject);
			AssertEquals("Standard subject", email.Subject);
			AssertContains("Standard body", email.Body);

			AssertNotContains("message is added in notifier, not in sender", "Message 1", email.Body);
			AssertNotContains("Message 2", email.Body);
		}

		public void TestSendChanges_WithEmail_AwaitingClient()
		{
			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.CustomerNotifier = ObjectFactory.Get<IInteractiveIncidentCustomerNotifierFactory>().CreateNotifier(incident, IncidentCustomerNotifierFactory.CreateSender(incident));
			incident.AddStaffMessageToCustomer("Message 1 <html>");
			incident.AddStaffMessageToCustomer("Message 2");
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate, "comment", true);
			Factory.Save();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(string.Format("Update on Incident: {0} - {1}", incident.IM_IncidentNumber, incident.IM_Description), email.Subject);
			AssertContains("This is an update to Incident", email.Body);
		}

		public void TestSendChanges_WithEmail_AwaitingClient_FeatureRequest()
		{
			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl);
			EDIDataRegistry.Instance.GlowEditERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_IncidentNumber = "CS000009";
			incident.IM_Description = "Some subject";
			incident.DetailNoteText = "Some detail";
			incident.IM_Priority = "CR7";
			incident.CustomerNotifier = ObjectFactory.Get<IInteractiveIncidentCustomerNotifierFactory>().CreateNotifier(incident, IncidentCustomerNotifierFactory.CreateSender(incident));
			incident.AddStaffMessageToCustomer("Message 1 <html>");
			incident.AddStaffMessageToCustomer("Message 2");
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate, "comment", true);
			Factory.Save();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(string.Format("Update on Incident: {0} - {1}", incident.IM_IncidentNumber, incident.IM_Description), email.Subject);
			AssertContains("This is an update to Incident", email.Body);
		}

		public void TestSendChanges_AllEmailsShouldTakeHyperlink()
		{
			var rootUrl = "https://localhost";
			var pageUrl = "inc/{*PK*}";
			using (EDIDataRegistry.Instance.GlowPortalRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rootUrl))
			using (EDIDataRegistry.Instance.GlowEditERequestPageUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pageUrl))
			{
				var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
				incident.IM_IncidentNumber = "CS000009";
				incident.IM_Description = "Some subject";
				incident.DetailNoteText = "Some detail";
				incident.IM_Priority = "CR7";
				incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
				Factory.Save();

				Env.OutgoingMailManager.EmailsCreated.Clear();

				var buttonNameNumber = Regex.Matches(SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident), SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID).Count;

				var sender = new WebRequestNotificationSender();

				var email1 = SupportIncidentEmail.New(incident);
				email1.Body = SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident);

				var email2 = SupportIncidentEmail.New(incident);

				var email3 = SupportIncidentEmail.New(incident);
				var body3 = $"<a id='{SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID}{incident.Number}'>";
				email3.Body = body3;

				var email4 = SupportIncidentEmail.New(incident);
				var body4 = SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident);
				email4.Body = body4;

				sender.SendChanges(incident, email1);
				var outEmail1 = Env.OutgoingMailManager.EmailsCreated.First(x => x.Subject == email1.Subject);
				AssertEquals("Button should be only added once", buttonNameNumber, Regex.Matches(outEmail1.Body, SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID).Count);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				sender.SendChanges(incident, email2);
				var outEmail2 = Env.OutgoingMailManager.EmailsCreated.First(x => x.Subject == email2.Subject);
				AssertEquals("Button should be added to body", buttonNameNumber, Regex.Matches(outEmail2.Body, SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID).Count);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				sender.SendChanges(incident, email3);
				var outEmail3 = Env.OutgoingMailManager.EmailsCreated.First(x => x.Subject == email3.Subject);
				AssertContains("Button should not be added to body if Body contains ID", body3, outEmail3.Body);
				AssertEquals("Button should not be added to body if Body contains ID (The only one is its own ID)", 1, Regex.Matches(outEmail3.Body, SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID).Count);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				sender.SendChanges(incident, email4);
				var outEmail4 = Env.OutgoingMailManager.EmailsCreated.First(x => x.Subject == email4.Subject);
				AssertContains("Button should not be added if email's body has Button already", body4, outEmail4.Body);
				AssertEquals("Button should have one button", 1, Regex.Matches(outEmail4.Body, SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID).Count);
			}
		}
	}
}
