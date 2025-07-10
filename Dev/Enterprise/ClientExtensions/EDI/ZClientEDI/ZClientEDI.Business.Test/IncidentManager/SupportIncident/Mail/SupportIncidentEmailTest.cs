using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.Mail.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentEmail))]
	class SupportIncidentEmailTest : CustomerServiceEmailTestCase<SupportIncidentEmail>
	{
		#region Implementation

		public class SupportIncidentEmailForTest : SupportIncidentEmail
		{
			public SupportIncidentEmailForTest(SupportIncident incident)
				: base(incident)
			{
			}

			public new void AddEvent(string emailRecipients)
			{
				base.AddEvent(emailRecipients);
			}

			public new bool ShouldSaveToEDocs
			{
				get
				{
					return base.ShouldSaveToEDocs;
				}
			}
		}

		protected new SupportIncident BusinessObjectSendingEmail
		{
			get { return (SupportIncident)base.BusinessObjectSendingEmail; }
		}

		protected override BusinessObject GetNewBusinessObjectSendingEmail()
		{
			return Factory.New<SupportIncident>();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SupportIncidentEmail result = SupportIncidentEmail.New(BusinessObjectSendingEmail);
			result.PublicLogComment = "Oink.";
			return result;
		}

		protected override void AssertAddedEvents()
		{
			var messageList = BusinessObjectSendingEmail.EConversation.GetTimeOrderedMessages();
			AssertEquals("LogText", true, messageList.Any(msg => msg.Body == "Oink."));

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident = newFactory.Load<SupportIncident>(BusinessObjectSendingEmail.PK);
			messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
			AssertEquals("LogText should be persisted.", true, messageList.Any(msg => msg.Body == "Oink."));
		}

		#endregion

		public void TestImplementsIDocManagerSupport()
		{
			AssertEquals("Should implement IDocManager", true, BusinessObjectSendingEmail is IDocManagerSupport);
		}

		public void TestShouldSaveToEDocs()
		{
			SupportIncidentEmailForTest emailContactObject = new SupportIncidentEmailForTest(BusinessObjectSendingEmail);
			AssertEquals("true for CustomerServiceEmailTestCase", true, emailContactObject.ShouldSaveToEDocs);
		}

		public void TestMark()
		{
			var incident = SupportIncidentEConversationTest.CreateIncidentWithoutSave(Factory);
			var email = SupportIncidentEmail.New(incident);
			AssertNotContains("Should not include second message body", IncidentConstants.MarkName, email.Body);
			var emailDef = email.GetEmail();
			AssertContains("Should include second message body", IncidentConstants.MarkName, emailDef.Body);
		}

		public void TestCC()
		{
			var incident = SupportIncidentEConversationTest.CreateIncidentWithoutSave(Factory);
			var org = incident.Client;
			var contact1 = org.Contacts[0];
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Bob";
			contact2.OC_Email = "bob@test.com.au";
			OrgContact contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Al";
			contact3.OC_Email = "al@test.com.au";
			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_EmailAddress = "arc@use.com.au";

			var convo = incident.EConversation.JobConversationForTest;
			var contactParticipant1 = convo.Participants.AddNew();
			contactParticipant1.JCP_ParticipantTableCode = contact1.TablePrefix;
			contactParticipant1.JCP_ParticipantID = contact1.PK;
			contactParticipant1.JCP_IsSubscribed = true;
			{
				var email = SupportIncidentEmail.New(incident);
				AssertEquals("", email.Cc);
			}

			var contactParticipant2 = convo.Participants.AddNew();
			contactParticipant2.JCP_ParticipantTableCode = contact2.TablePrefix;
			contactParticipant2.JCP_ParticipantID = contact2.PK;
			contactParticipant2.JCP_IsSubscribed = true;

			var contactParticipant3 = convo.Participants.AddNew();
			contactParticipant3.JCP_ParticipantTableCode = contact3.TablePrefix;
			contactParticipant3.JCP_ParticipantID = contact3.PK;
			contactParticipant3.JCP_IsSubscribed = false;

			var emailParticipant = convo.RelatedParties.AddNewParticipant("bob2@test.com");
			emailParticipant.JCP_IsSubscribed = false;

			var staffParticipant = convo.Participants.AddNew();
			staffParticipant.JCP_ParticipantTableCode = staffRecipient.TablePrefix;
			staffParticipant.JCP_ParticipantID = staffRecipient.PK;
			staffParticipant.JCP_IsSubscribed = true;
			{
				var email = SupportIncidentEmail.New(incident);
				AssertEquals("Should not contain staff", "bob@test.com.au", email.Cc);
			}

			contactParticipant3.JCP_IsSubscribed = true;
			{
				var email = SupportIncidentEmail.New(incident);
				AssertEquals("al@test.com.au;bob@test.com.au", email.Cc);

				var emailDef = email.GetEmail();
				AssertEquals(2, emailDef.CCRecipients.Count);
			}

			contact3.OC_IsActive = false;
			{
				var email = SupportIncidentEmail.New(incident);
				AssertEquals("bob@test.com.au", email.Cc);

				var emailDef = email.GetEmail();
				AssertEquals(1, emailDef.CCRecipients.Count);
			}

			emailParticipant.JCP_IsSubscribed = true;
			{
				var email = SupportIncidentEmail.New(incident);
				AssertNotContains("bob2@test.com", email.Cc);
			}
		}

		public void TestCC_MaxLength()
		{
			var incident = SupportIncidentEConversationTest.CreateIncidentWithoutSave(Factory);
			var convo = incident.EConversation.JobConversationForTest;

			var org = incident.Client;
			for (int i = 1; i < 200; ++i)
			{
				var contact = org.Contacts.AddNew();
				var name = i.ToString("000");
				contact.OC_ContactName = name;
				contact.OC_Email = name + "@test.com.au";

				var contactParticipant = convo.Participants.AddNew();
				contactParticipant.JCP_ParticipantTableCode = contact.TablePrefix;
				contactParticipant.JCP_ParticipantID = contact.PK;
				contactParticipant.JCP_IsSubscribed = true;
			}

			var email = SupportIncidentEmail.New(incident);
			AssertNoNotifications(email);

			var emailDef = email.GetEmail();
			AssertLessThanOrEqualTo(emailDef.CCRecipients.Count, 120);
		}

		public void TestCreateSupportIncidentUpdateEmail()
		{
			var incident = SupportIncidentEConversationTest.CreateIncidentWithoutSave(Factory);
			incident.IM_Description = "incy";
			var org = incident.Client;
			var contact1 = org.Contacts[0];
			contact1.OC_Email = "or@test.com.au";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Bob";
			contact2.OC_Email = "bob@test.com.au";
			OrgContact contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Al";
			contact3.OC_Email = "al@test.com.au";
			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_EmailAddress = "arc@use.com.au";

			var convo = incident.EConversation.JobConversationForTest;
			var contactParticipant1 = convo.Participants.AddNew();
			contactParticipant1.JCP_ParticipantTableCode = contact1.TablePrefix;
			contactParticipant1.JCP_ParticipantID = contact1.PK;
			contactParticipant1.JCP_IsSubscribed = true;

			var contactParticipant2 = convo.Participants.AddNew();
			contactParticipant2.JCP_ParticipantTableCode = contact2.TablePrefix;
			contactParticipant2.JCP_ParticipantID = contact2.PK;
			contactParticipant2.JCP_IsSubscribed = true;

			var contactParticipant3 = convo.Participants.AddNew();
			contactParticipant3.JCP_ParticipantTableCode = contact3.TablePrefix;
			contactParticipant3.JCP_ParticipantID = contact3.PK;
			contactParticipant3.JCP_IsSubscribed = false;

			var staffParticipant = convo.Participants.AddNew();
			staffParticipant.JCP_ParticipantTableCode = staffRecipient.TablePrefix;
			staffParticipant.JCP_ParticipantID = staffRecipient.PK;
			staffParticipant.JCP_IsSubscribed = true;
			Factory.Save();

			var newMessage1 = convo.Messages.AddNew();
			newMessage1.JCM_Body = "Onyaaaaa";
			newMessage1.JCM_PostedTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			newMessage1.JCM_JCP_Participant = contactParticipant1.PK;
			var newMessage2 = convo.Messages.AddNew();
			newMessage2.JCM_Body = "Alrightyy";
			newMessage2.JCM_PostedTimeUtc = ZDateTime.UtcNow;
			newMessage2.JCM_JCP_Participant = contactParticipant1.PK;
			var email = SupportIncidentEmailQuickBuilder.CreateAwaitingResponseEmail(incident, convo.Messages).Item1;

			AssertEquals("default OverrideSignOffNameAndTitleWithCurrentUsersNameAndTitle should be false", false, email.OverrideSignOffNameAndTitleWithCurrentUsersNameAndTitle);
			AssertEquals("Should have sent the email to the incident contact", 1, email.Recipients.Length);
			AssertEquals("Should have sent the email to the incident contact", contact1.OC_Email, email.Recipients[0]);
			AssertEquals("Should cc the subscribed contacts (not staff)", 1, email.CcRecipients.Length);
			AssertEquals("Should cc the subscribed contacts (not staff)", contact2.OC_Email, email.CcRecipients[0]);
			AssertEquals("Subject", FormattableString.Invariant($"Update on Incident: {incident.IM_IncidentNumber} - {incident.IM_Description}"), email.Subject);
			AssertContains("Should include first message body", newMessage1.JCM_Body, email.Body);
			AssertContains("Should include second message body", newMessage2.JCM_Body, email.Body);
			AssertHeader(email);

			email.SendEmail();

			AssertEquals("Should have saved message", true, newMessage1.IsInDatabase);
			AssertEquals("Should have saved message", true, newMessage2.IsInDatabase);
		}

		public void TestCreateSupportIncidentUpdateEmail_IsForBroadcastShouldNotSaveBizOFactoryOnSent()
		{
			var incident = SupportIncidentEConversationTest.CreateIncidentWithoutSave(Factory);
			incident.IM_Description = "incy";
			var org = incident.Client;
			var contact1 = org.Contacts[0];
			contact1.OC_Email = "or@test.com.au";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Bob";
			contact2.OC_Email = "bob@test.com.au";
			OrgContact contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Al";
			contact3.OC_Email = "al@test.com.au";
			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_EmailAddress = "arc@use.com.au";

			var convo = incident.EConversation.JobConversationForTest;
			var contactParticipant1 = convo.Participants.AddNew();
			contactParticipant1.JCP_ParticipantTableCode = contact1.TablePrefix;
			contactParticipant1.JCP_ParticipantID = contact1.PK;
			contactParticipant1.JCP_IsSubscribed = true;

			var contactParticipant2 = convo.Participants.AddNew();
			contactParticipant2.JCP_ParticipantTableCode = contact2.TablePrefix;
			contactParticipant2.JCP_ParticipantID = contact2.PK;
			contactParticipant2.JCP_IsSubscribed = true;

			var contactParticipant3 = convo.Participants.AddNew();
			contactParticipant3.JCP_ParticipantTableCode = contact3.TablePrefix;
			contactParticipant3.JCP_ParticipantID = contact3.PK;
			contactParticipant3.JCP_IsSubscribed = false;

			var contactParticipant4 = convo.Participants.AddNew();
			contactParticipant4.JCP_ParticipantTableCode = staffRecipient.TablePrefix;
			contactParticipant4.JCP_ParticipantID = staffRecipient.PK;
			contactParticipant4.JCP_IsSubscribed = true;
			Factory.Save();

			var newMessage1 = convo.Messages.AddNew();
			newMessage1.JCM_Body = "Onyaaaaa";
			newMessage1.JCM_PostedTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			newMessage1.JCM_JCP_Participant = contactParticipant1.PK;
			var newMessage2 = convo.Messages.AddNew();
			newMessage2.JCM_Body = "Alrightyy";
			newMessage2.JCM_PostedTimeUtc = ZDateTime.UtcNow;
			newMessage2.JCM_JCP_Participant = contactParticipant1.PK;

			var email = SupportIncidentEmailQuickBuilder.CreateAwaitingResponseEmail(incident, convo.Messages, true).Item1;

			AssertEquals("Should have sent the email to the incident contact", 1, email.Recipients.Length);
			AssertEquals("Should have sent the email to the incident contact", contact1.OC_Email, email.Recipients[0]);
			AssertEquals("Should cc the subscribed contact", 1, email.CcRecipients.Length);
			AssertEquals("Should cc the subscribed contact", contact2.OC_Email, email.CcRecipients[0]);
			AssertEquals("Subject", FormattableString.Invariant($"Update on Incident: {incident.IM_IncidentNumber} - {incident.IM_Description}"), email.Subject);
			AssertContains("Should include first message body", newMessage1.JCM_Body, email.Body);
			AssertContains("Should include second message body", newMessage2.JCM_Body, email.Body);
			AssertHeader(email);

			email.SendEmail();

			AssertEquals("Should not have saved message", false, newMessage1.IsInDatabase);
			AssertEquals("Should not have saved message", false, newMessage2.IsInDatabase);
		}

		public void TestCreateSupportIncidentUpdateEmail_NoEConvMessages_BodyShouldShowDefaultMessage()
		{
			var incident = SupportIncidentEConversationTest.CreateIncidentWithoutSave(Factory);
			incident.IM_Description = "incy";
			var convo = incident.EConversation.JobConversationForTest;
			Factory.Save();
			incident.EConversation.JobConversationForTest.Messages.DeleteAll();

			var email = SupportIncidentEmailQuickBuilder.CreateAwaitingResponseEmail(incident, convo.Messages, true).Item1;

			AssertContains("Should include default message", "No eConversation Messages to display.", email.Body);
		}

		public void TestSupporessingEmail_CreatingObject()
		{
			var expectedLogString = $"Send {0}-{1} failed because the email was suppressed";
			var incident = Factory.New<SupportIncident>();
			var jobHeader = ProcessJobHeader.GetForParent(incident, incident.Factory);
			Factory.Save();

			var email1 = SupportIncidentEmail.New(incident);
			AssertNotNull("Empty email should be created", email1);
			AssertNullOrEmpty(email1.Body);
			AssertNullOrEmpty(email1.Subject);

			var isAllow = false;
			var template = new NewIncidentRaisedInternalEmailContentBuilder(incident, "NTZ");
			var email2 = SupportIncidentEmail.New(incident, template, out isAllow);
			var log = incident.Logs.MostRecentLogByEventTime(Enterprise.ZArchitecture.Business.Events.EmailSent);
			Assert("The email should be created because the rules doesn't contain this code", isAllow);
			AssertNotNull("The email should not be created because the rules doesn't contain this code", email2);
			AssertNotNullOrEmpty(email2.Body);
			AssertNotNullOrEmpty(email2.Subject);
			AssertNull(log);

			var blnGroup = Factory.New<TagDefinition>();
			blnGroup.TGD_Code = "BLN";
			var tag = blnGroup.Magnitudes.AddNew();
			tag.TGM_Code = template.TemplateCode;
			jobHeader.AddTag(tag);
			Factory.Save();

			var email3 = SupportIncidentEmail.New(incident, template, out isAllow);
			log = incident.Logs.MostRecentLogByEventTime(Enterprise.ZArchitecture.Business.Events.EmailSent);
			AssertNotNull(log);
			AssertEquals(string.Format(EDIEmailBuilder.LogReference, template.TemplateDescription, template.TemplateCode), log.SL_Reference);
			AssertNull(incident.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, string.Format(expectedLogString, template.TemplateCode, template.TemplateDescription))).FirstOrDefault());
			Assert("The email should not be created because the rules doesn't contain this code", !isAllow);
			AssertNull("The email should not be created because the rules doesn't contain this code", email3);
		}

		public void TestGetEmailCore_ShouldTakeButton()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var url = SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(incident);
			var button = SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident);

			var emailEmpty = SupportIncidentEmail.New(incident);
			AssertContains($"<p>{button}</p>Regards,<br/>", emailEmpty.GetEmail().Body);
			AssertEquals(false, RequestHyperlinkButtonChecker.CanAddButton(incident, emailEmpty.GetEmail().Body));

			var emailOnlyUrl = SupportIncidentEmail.New(incident);
			emailOnlyUrl.Body = SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(incident);
			AssertEquals(false, RequestHyperlinkButtonChecker.CanAddButton(incident, emailOnlyUrl.GetEmail().Body));
			AssertEquals(1, Regex.Matches(emailOnlyUrl.GetEmail().Body, SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID).Count);
			AssertEquals(2, Regex.Matches(emailOnlyUrl.GetEmail().Body, Regex.Escape(url)).Count);

			var emailFullButton = SupportIncidentEmail.New(incident);
			emailFullButton.Body = SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident);
			AssertEquals(false, RequestHyperlinkButtonChecker.CanAddButton(incident, emailFullButton.GetEmail().Body));
			AssertEquals(1, Regex.Matches(emailFullButton.GetEmail().Body, SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID).Count);

			var emailWithAnotherIncident = SupportIncidentEmail.New(incident);
			emailWithAnotherIncident.Body = SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident2);
			AssertEquals(false, RequestHyperlinkButtonChecker.CanAddButton(incident, emailWithAnotherIncident.GetEmail().Body));
			AssertEquals(2, Regex.Matches(emailWithAnotherIncident.GetEmail().Body, SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID).Count);
		}

		void AssertHeader(SupportIncidentEmail email)
		{
			AssertEquals("XAutoResponseSuppressHeader", SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, email.XAutoResponseSuppressHeaderValue);
		}
	}
}
