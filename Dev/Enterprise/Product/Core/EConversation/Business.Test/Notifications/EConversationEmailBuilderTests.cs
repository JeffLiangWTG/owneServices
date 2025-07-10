using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.EConversation.Testing.DummyConversationProvider;

namespace Enterprise.EConversation.Testing
{
	sealed class EConversationEmailBuilderTests : TestCaseWithFactory
	{
		const int numberOfIncludedMessages = 5;

		public void TestGeneratesForEveryone()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Email = "org@oh.com";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact@oc.com";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@gs.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var eConversation = CreateConversationWithSubscribers(org, contact, staff, group).eConversation;
			eConversation.Participants.AddNewParticipant("a@b.com");
			eConversation.AddMessageFromCurrentUser("Sup", false);

			var recipients = GenerateAndGetRecipients(eConversation);
			AssertContainsExactElementsInAnyOrder(new[] { "org@oh.com", "contact@oc.com", "staff@gs.com", "a@b.com" }, recipients);
		}

		public void TestOnlyGeneratesWhenSubscribed()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var subscribed = Factory.NewWithValidTestData<OrgContact>();
			subscribed.OC_Email = "sub@oc.com";

			var unsubscribed = Factory.NewWithValidTestData<OrgContact>();
			unsubscribed.OC_Email = "unsub@oc.com";

			var eConversation = CreateConversationWithSubscribers(subscribed, unsubscribed).eConversation;
			eConversation.Participants.GetOrAdd(unsubscribed).JCP_IsSubscribed = false;

			eConversation.AddMessageFromCurrentUser("Sup", false);

			var recipients = GenerateAndGetRecipients(eConversation);
			AssertContainsExactElementsInAnyOrder(new[] { "sub@oc.com" }, recipients);
		}

		public void TestOnlyGeneratesForOneWhenRecipientsShareEmails()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var first = Factory.NewWithValidTestData<OrgContact>();
			first.OC_Email = "pimp@daddy.com";

			var second = Factory.NewWithValidTestData<OrgContact>();
			second.OC_Email = "random@bloke.com";

			var third = Factory.NewWithValidTestData<OrgContact>();
			third.OC_Email = "pimp@daddy.com";

			var eConversation = CreateConversationWithSubscribers(first, second, third).eConversation;

			eConversation.AddMessageFromCurrentUser("Sup", false);

			var recipients = GenerateAndGetRecipients(eConversation);
			AssertEquals("It should not create two emails when people share email addresses", 2, recipients.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "pimp@daddy.com", "random@bloke.com" }, recipients);
		}

		public void TestIgnoresInactive()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var active = CreateContact("active@oc.com");
			var inactive = CreateContact("inactive@oc.com");
			inactive.OC_IsActive = false;

			var eConversation = CreateConversationWithSubscribers(active, inactive).eConversation;

			eConversation.AddMessageFromCurrentUser("Sup", false);

			AssertContainsExactElementsInAnyOrder(new[] { "active@oc.com" }, GenerateAndGetRecipients(eConversation));
		}

		public void TestMessageBodyContainsLatestMessages()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var eConvo = CreateConversationWithSubscribers(CreateContact()).eConversation;
			for (var i = 0; i < someRandomTextForMessages.Length; i++)
			{
				var message = eConvo.AddMessageFromCurrentUser(someRandomTextForMessages[i], false);
				message.JCM_PostedTimeUtc = ZDateTime.UtcNow.AddMinutes(-i);
			}

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			eConvo.AddMessageFromCurrentUser("One more", false);

			var expectedMessages = someRandomTextForMessages.Take(numberOfIncludedMessages);
			AssertGeneratedEmailContainsMessages("The generated email should contain the last five messages", eConvo, expectedMessages);
		}

		public void TestMessageBodyHasNewlines()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var eConvo = CreateConversationWithSubscribers(CreateContact()).eConversation;
			eConvo.AddMessageFromCurrentUser(
@"Lets have some newlines.

That always makes life more fun.", false);

			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(eConvo);
			var email = Env.OutgoingMailManager.EmailsCreated.Single().Body;

			var messageBodyInEmail = Regex.Match(email, @"Lets have some newlines\..*?That always makes life more fun\.", RegexOptions.Singleline);
			Assert("PRE: The message should be included", messageBodyInEmail.Success);
			AssertEquals("The two newlines should get line breaks", 2, Regex.Matches(messageBodyInEmail.Value, @"<br />").Count);
		}

		public void TestIncludesAllNewMessagesAsWellAsOldOnes()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var oldMessages = Enumerable.Range(0, 10).Select(i => "Old Message #" + i).ToArray();
			var newMessages = Enumerable.Range(0, 10).Select(i => "New Message #" + i).ToArray();

			var eConvo = CreateConversationWithSubscribers(CreateContact()).eConversation;
			Factory.Save();
			eConvo.Messages.DeleteAll(); // Removes any system messages

			var msgDate = ZDateTime.UtcNow.AddMinutes(-newMessages.Length);
			foreach (var message in oldMessages)
			{
				var msg = eConvo.AddMessageFromCurrentUser(message, false);
				msg.JCM_PostedTimeUtc = msgDate;
				msgDate = msgDate.AddMinutes(-1);
			}

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			msgDate = ZDateTime.UtcNow;
			foreach (var message in newMessages)
			{
				var msg = eConvo.AddMessageFromCurrentUser(message, false);
				msg.JCM_PostedTimeUtc = msgDate;
				msgDate = msgDate.AddMinutes(-1);
			}

			var expectedMessages = newMessages.Concat(oldMessages.Take(5)).ToArray();
			AssertGeneratedEmailContainsMessages("The generated email should include all new messages AND the 5 old messages", eConvo, expectedMessages);
		}

		public void TestDoesntIncludeSystemMessages()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var otherUser = Factory.NewWithValidTestData<GlbStaff>();
			otherUser.GS_EmailAddress = "foo@bar.com";

			var eConvo = CreateConversationWithSubscribers(otherUser).eConversation;
			eConvo.AddMessageFromCurrentUser("Heyo m80", false);
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			eConvo.AddMessageFromCurrentUser("G'day bud", false);

			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(eConvo);
			var email = Env.OutgoingMailManager.EmailsCreated.Single();

			AssertNotContains("We don't want to include any system messages", "has been added to the conversation.", email.Body, true);
		}

		public void TestUsesGivenParticipantsLanguage()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var hanz = Factory.NewWithValidTestData<GlbStaff>();
			hanz.GS_WorkingLanguage = Core.SharedConstants.Languages.German;
			hanz.GS_EmailAddress = "hanz@arbeit.de";

			var shazza = Factory.NewWithValidTestData<GlbStaff>();
			shazza.GS_WorkingLanguage = Core.SharedConstants.Languages.English;
			shazza.GS_EmailAddress = "shazza@work.com.au";

			Factory.Save();

			using (var mockEng = Res.GetLanguageInstance(shazza.GS_WorkingLanguage).UseMockData())
			using (var mockGer = Res.GetLanguageInstance(hanz.GS_WorkingLanguage).UseMockData())
			{
				mockGer.Put(RegistryEmailSubjectResStringKey, new ResourceStringData(RegistryEmailSubjectResStringKey, "(*ID*) German Email Subject"));
				mockGer.Put(RegistryEmailBodyResStringKey, new ResourceStringData(RegistryEmailBodyResStringKey, "German Email Template"));

				var convo = CreateConversationWithSubscribers(hanz, shazza).eConversation;
				convo.AddMessageFromCurrentUser("Hey Shazza s'doin", false);

				EConversationEmailBuilder.GenerateAndQueueEmailNotifications(convo);
			}

			var forShazza = GetEmailsFor(shazza).Single();
			var forHanz = GetEmailsFor(hanz).Single();

			CombineAssertions("Email should be given in the users language", () =>
			{
				AssertContains("Actual default English template", "DummyBizo - New Messages", forShazza.Body, ignoreCase: true);
				AssertEquals("German Subject", "DummyBizo German Email Subject", forHanz.Subject);
				AssertEquals("German Body", "German Email Template", forHanz.Body);
			});
		}

		public void TestEscapesThingsThatBuggerUpHtml()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var conversation = CreateConversationWithSubscribers(CreateContact()).eConversation;
			conversation.AddMessageFromCurrentUser("Here is some ><'#& buggers up html", false);

			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(conversation);

			var email = Env.OutgoingMailManager.EmailsCreated.Single().Body;
			AssertContains("PRE: Should include the message", "buggers up html", email);
			AssertNotContains("Here is some ><'#& buggers up html", email);
		}

		public void TestInternalParticipantsGetInternalMessages()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "somethign@gtgg.com";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "something@else.com";

			var convo = CreateConversationWithSubscribers(staff, contact).eConversation;
			convo.AddMessageFromCurrentUser("Super secret existing", isInternal: true);
			convo.AddMessageFromCurrentUser("Public existing", isInternal: false);

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			convo.AddMessageFromCurrentUser("Super secret new", isInternal: true);
			convo.AddMessageFromCurrentUser("Public new", isInternal: false);

			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(convo);

			var forStaff = GetEmailsFor(staff).Single();
			var forContact = GetEmailsFor(contact).Single();

			AssertContains("Internal subscribers should have the whole conversation (1)", "Super secret existing", forStaff.Body);
			AssertContains("Internal subscribers should have the whole conversation (2)", "Super secret new", forStaff.Body);
			AssertContains("Internal subscribers should have the whole conversation (3)", "Public existing", forStaff.Body);
			AssertContains("Internal subscribers should have the whole conversation (4)", "Public new", forStaff.Body);

			AssertContains("External subscribers should have public mesages", "Public existing", forContact.Body);
			AssertContains("External subscribers should have public mesages", "Public new", forContact.Body);
			AssertNotContains("External subscribers should NOT have internal mesages", "Super secret existing", forContact.Body);
			AssertNotContains("External subscribers should NOT have internal mesages", "Super secret new", forContact.Body);
		}

		public void TestGenerateStaffNotification()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@wistetechglobal.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "staff2@wistetechglobal.com";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_EmailAddress = "staff3@wistetechglobal.com";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "something@else.com";

			var convo = CreateConversationWithSubscribers(staff1, staff2, staff3, contact).eConversation;
			var msg1 = convo.AddMessageFromCurrentUser("Super secret existing", isInternal: true);
			var msg2 = convo.AddMessageFromCurrentUser("Public existing", isInternal: false);
			var msg3 = convo.AddMessageFromCurrentUser("Super secret new", isInternal: true);
			var msg4 = convo.AddMessageFromCurrentUser("Public new", isInternal: false);

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var resultWithNoMessages = EConversationEmailBuilder.GenerateStaffNotification(Factory, convo, Array.Empty<JobConversationMessage>(), staff1.GS_EmailAddress);
			Assert("nothing to send", !resultWithNoMessages);

			var result = EConversationEmailBuilder.GenerateStaffNotification(Factory, convo, new[] { msg4, msg3 }, staff1.GS_EmailAddress);
			Assert("email sent", result);
			var emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals(1, emails.Count);
			var email = emails[0];
			AssertEquals(2, email.Recipients.Count);
			Assert(email.Recipients.Contains(staff2.GS_EmailAddress));
			Assert(email.Recipients.Contains(staff3.GS_EmailAddress));
		}

		public void TestGenerateStaffAndContactNotifications()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@wistetechglobal.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "staff2@wistetechglobal.com";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_EmailAddress = "staff3@wistetechglobal.com";
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Email = "contact1@somecustomer.com";
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Email = "contact2@somecustomer.com";

			var convo = CreateConversationWithSubscribers(staff1, staff2, staff3, contact1, contact2).eConversation;
			var msg1 = convo.AddMessageFromCurrentUser("Super secret existing", isInternal: true);
			var msg2 = convo.AddMessageFromCurrentUser("Public existing", isInternal: false);
			var msg3 = convo.AddMessageFromCurrentUser("Super secret new", isInternal: true);
			var msg4 = convo.AddMessageFromCurrentUser("Public new", isInternal: false);

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var resultWithNoMessages = EConversationEmailBuilder.GenerateStaffAndContactNotifications(Factory, convo, Array.Empty<JobConversationMessage>(), null, null, null);
			Assert("nothing to send", !resultWithNoMessages);

			var result = EConversationEmailBuilder.GenerateStaffAndContactNotifications(Factory,
				convo,
				new[] { msg4, msg3 },
				new[] { (string)contact1.OC_Email, (string)staff3.GS_EmailAddress },
				"staff link",
				"contact link");
			Assert("emails sent", result);
			var emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals(2, emails.Count);
			CombineAssertions(() =>
			{
				var email1 = emails[0];
				var recipients1 = string.Join(", ", email1.Recipients.Cast<RecipientDef>().Select(x => x.Email).OrderBy(x => x));
				AssertEquals("staff1@wistetechglobal.com, staff2@wistetechglobal.com", recipients1);
				AssertContains("staff link", email1.Body);
				AssertNotContains("contact link", email1.Body);
				AssertContains("Public existing", email1.Body);
				AssertContains("Public new", email1.Body);
				AssertContains("Super secret existing", email1.Body);
				AssertContains("Super secret new", email1.Body);
			});
			CombineAssertions(() =>
			{
				var email2 = emails[1];
				var recipients2 = string.Join(", ", email2.Recipients.Cast<RecipientDef>().Select(x => x.Email).OrderBy(x => x));
				AssertEquals("contact2@somecustomer.com", recipients2);
				AssertContains("contact link", email2.Body);
				AssertNotContains("staff link", email2.Body);
				AssertContains("Public existing", email2.Body);
				AssertContains("Public new", email2.Body);
				AssertNotContains("Super secret existing", email2.Body);
				AssertNotContains("Super secret new", email2.Body);
			});
		}

		public void TestAddMessageFromCurrentUser_WhenSpecificallyNominatedAsSystemMessage()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var eConversation = CreateConversationWithSubscribers(staff).eConversation;
			var systemGeneratedMessage = eConversation.AddMessageFromCurrentUser("W'sup", isInternal: false, isSystem: true);
			var nonSystemGeneratedMessage = eConversation.AddMessageFromCurrentUser("M'Lady", isInternal: false);
			var internalSystemGeneratedMessage = eConversation.AddMessageFromCurrentUser("M'Linda", isInternal: true, isSystem: true);
			var internalNonSystemGeneratedMessage = eConversation.AddMessageFromCurrentUser("M'pants", isInternal: true);

			AssertEquals(true, systemGeneratedMessage.JCM_IsSystem);
			AssertEquals(false, systemGeneratedMessage.JCM_IsInternal);

			AssertEquals(false, nonSystemGeneratedMessage.JCM_IsSystem);
			AssertEquals(false, nonSystemGeneratedMessage.JCM_IsInternal);

			AssertEquals(true, internalSystemGeneratedMessage.JCM_IsSystem);
			AssertEquals(true, internalSystemGeneratedMessage.JCM_IsInternal);

			AssertEquals(false, internalNonSystemGeneratedMessage.JCM_IsSystem);
			AssertEquals(true, internalNonSystemGeneratedMessage.JCM_IsInternal);
		}

		public void TestGenerateEmailNotifications()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "meat@backonthemenu.boys";

			var participant = conversation.Participants.AddNewParticipant(staff);

			Factory.Save();

			AssertEquals(0, Env.AllEmailsCreated.Count());

			conversation.AddMessageFromCurrentUser("Bleusche", false);
			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(conversation);

			AssertEquals(1, Env.AllEmailsCreated.Count());

			var body = Env.AllEmailsCreated.Single().Body;
			AssertContains("Should have included the eConversation message in the email notifiation", "Bleusche", body);
			AssertContains("Should have generated a valid hyperlink to include in the email notification ", "edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=Dummy", body);
		}

		#region Additional Participants

		public void TestGenerateEmailNotifications_ShouldUseAdditionalParticipants()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProviderWithAdditionalParticipants);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_EmailAddress = "MrsSullivan@Testimeow.com";
			staff4.GS_EmailAddress = "MrsSullivan'sCats@Testimeow.com";

			var bizo = Factory.NewWithValidTestData<DummyConversationProviderWithAdditionalParticipants>();
			bizo.AdditionalParticipants.Add(staff1);
			bizo.AdditionalParticipants.Add(staff3);
			bizo.AdditionalParticipants.Add(staff4);

			Factory.Save();

			var conversation = bizo.eConversation;
			conversation.Participants.AddNewParticipant(staff1);
			conversation.Participants.AddNewParticipant(staff2);

			conversation.AddMessageFromCurrentUser("Last Will and Testimeow", false);
			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(conversation);

			AssertContainsExactElementsInAnyOrder(new[] { "MrsSullivan@Testimeow.com", "MrsSullivan'sCats@Testimeow.com" }, Env.AllEmailsCreated.SelectMany(e => e.Recipients.Cast<RecipientDef>().Select(r => r.Email)));
			AssertEquals(2, Env.AllEmailsCreated.Count());
		}

		#endregion

		#region Overridden Hyperlinks

		public void TestGenerateEmailNotifications_ShouldUseOverriddenHyperlink()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProviderWithSpecificHyperlinks);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_EmailAddress = "davey@pw.com";
			staff2.GS_EmailAddress = "wavey@pw.com";

			var dummy = Factory.New<DummyConversationProviderWithSpecificHyperlinks>();
			dummy.HyperlinkToUse = "Links are good, convenient, happy, useful, good";
			dummy.ShouldUseProviderFunc = provider => provider == staff1;

			Factory.Save();

			dummy.eConversation.Participants.AddNewParticipant(staff1);
			dummy.eConversation.Participants.AddNewParticipant(staff2);

			dummy.eConversation.AddMessageFromCurrentUser("Cake Day!!", isInternal: false);

			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(dummy.eConversation);

			AssertEquals(2, Env.AllEmailsCreated.Count());

			var staff1Email = Env.AllEmailsCreated.Single(e => e.Recipients.Contains("davey@pw.com"));
			var staff2Email = Env.AllEmailsCreated.Single(e => e.Recipients.Contains("wavey@pw.com"));

			AssertContains("Links are good, convenient, happy, useful, good", staff1Email.Body);
			AssertNotContains("Links are good, convenient, happy, useful, good", staff2Email.Body);

			AssertContains("edient", staff2Email.Body);
			AssertNotContains("edient", staff1Email.Body);
		}

		public void TestGenerateEmailNotifications_WhenSingleEmailSentToAllParticipants_ShouldStillConsiderUsingOverriddenHyperlink()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProviderWithSpecificHyperlinks);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_EmailAddress = "davey@pw.com";
			staff2.GS_EmailAddress = "wavey@pw.com";

			var providerWasChecked = false;

			var dummy = Factory.New<DummyConversationProviderWithSpecificHyperlinks>();
			dummy.HyperlinkToUse = "Links are good, convenient, happy, useful, good";
			dummy.ShouldUseProviderFunc = provider => (providerWasChecked = provider == null);

			Factory.Save();

			dummy.eConversation.Participants.AddNewParticipant(staff1);
			dummy.eConversation.Participants.AddNewParticipant(staff2);

			var message = dummy.eConversation.AddMessageFromCurrentUser("Cake Day!!", isInternal: false);

			EConversationEmailBuilder.GenerateStaffNotification(Factory, dummy.eConversation, new[] { message }, "gravy@pw.com");

			AssertEquals(1, Env.AllEmailsCreated.Count());

			var email = Env.AllEmailsCreated.Single();

			AssertEquals(true, email.Recipients.Contains("davey@pw.com"));
			AssertEquals(true, email.Recipients.Contains("wavey@pw.com"));

			AssertContains("Links are good, convenient, happy, useful, good", email.Body);
			AssertNotContains("edient", email.Body);
			AssertEquals(true, providerWasChecked);
		}

		#endregion

		#region Overriden IConversationEmailBehaviorProvider

		public void TestGenerateEmailNotifications_WhenOverrideEmailBehaviorProvider()
		{
			CreateEmailNotifications(shouldExculdeSender: true, shouldSendEmailFromSender: true);

			AssertEquals(1, Env.AllEmailsCreated.Count());

			var email = Env.AllEmailsCreated.Single();

			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				AssertEquals(true, email.Recipients[0].Email == "something@gtgg.com");
				AssertEquals(true, email.Body.Contains("The message when override EmailBehaviorProvider"));
				AssertEquals("Test_Sender", email.FromDisplayName);
				AssertEquals("sender@pw.com", email.FromAddress);
			});
		}

		public void TestGenerateEmailNotifications_WhenOverrideEmailBehaviorProvider_ShouldSendEmailFromSenderFalse()
		{
			CreateEmailNotifications(shouldExculdeSender: true, shouldSendEmailFromSender: false);

			AssertEquals(1, Env.AllEmailsCreated.Count());

			var email = Env.AllEmailsCreated.Single();

			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				AssertEquals(true, email.Recipients[0].Email == "something@gtgg.com");
				AssertEquals(true, email.Body.Contains("The message when override EmailBehaviorProvider"));
				AssertEquals(EnvProxy.Instance.Registry.MailboxDisplayName, email.FromDisplayName);
				AssertEquals(EnvProxy.Instance.Registry.MailboxEmailAddress, email.FromAddress);
			});
		}

		public void TestGenerateEmailNotifications_WhenOverrideEmailBehaviorProvider_ShouldExcludeSenderFalse()
		{
			CreateEmailNotifications(shouldExculdeSender: false, shouldSendEmailFromSender: true);

			AssertEquals(2, Env.AllEmailsCreated.Count());

			CombineAssertions(() =>
			{
				AssertNotNull(Env.AllEmailsCreated.Single(x => x.Recipients[0].Email == "something@gtgg.com"));
				AssertNotNull(Env.AllEmailsCreated.Single(x => x.Recipients[0].Email == "sender@pw.com"));
			});

			foreach (var email in Env.AllEmailsCreated)
			{
				CombineAssertions(() =>
				{
					AssertEquals(true, email.Body.Contains("The message when override EmailBehaviorProvider"));
					AssertEquals("Test_Sender", email.FromDisplayName);
					AssertEquals("sender@pw.com", email.FromAddress);
				});
			}
		}

		public void TestGenerateEmailNotifications_WhenOverrideEmailBehaviorProvider_ShouldExcludeSenderAndShouldSendEmailFromSenderFalse()
		{
			CreateEmailNotifications(shouldExculdeSender: false, shouldSendEmailFromSender: false);

			AssertEquals(2, Env.AllEmailsCreated.Count());

			CombineAssertions(() =>
			{
				AssertNotNull(Env.AllEmailsCreated.Single(x => x.Recipients[0].Email == "something@gtgg.com"));
				AssertNotNull(Env.AllEmailsCreated.Single(x => x.Recipients[0].Email == "sender@pw.com"));
			});

			foreach (var email in Env.AllEmailsCreated)
			{
				CombineAssertions(() =>
				{
					AssertEquals(true, email.Body.Contains("The message when override EmailBehaviorProvider"));
					AssertEquals(EnvProxy.Instance.Registry.MailboxDisplayName, email.FromDisplayName);
					AssertEquals(EnvProxy.Instance.Registry.MailboxEmailAddress, email.FromAddress);
				});
			}
		}

		void CreateEmailNotifications(bool shouldExculdeSender, bool shouldSendEmailFromSender)
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProviderWithEmailBehaviorProvider);
			var parent = Factory.NewWithValidTestData<DummyConversationProviderWithEmailBehaviorProvider>();
			parent.ShouldExcludeSender = shouldExculdeSender;
			parent.ShouldSendEmailFromSender = shouldSendEmailFromSender;

			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);

			var sender = Factory.NewWithValidTestData<GlbStaff>();
			sender.GS_FullName = "Test_Sender";
			sender.GS_EmailAddress = "sender@pw.com";
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "something@gtgg.com";

			conversation.Participants.AddNewParticipant(sender);
			conversation.Participants.AddNewParticipant(staff);

			using (Env.SetTemporaryUserContext(new UserContext(sender.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				conversation.AddMessageFromCurrentUser("The message when override EmailBehaviorProvider", isInternal: true);
			}

			Env.OutgoingMailManager.EmailsCreated.Clear();

			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(conversation);
		}

		#endregion

		#region Email Text

		public void TestEmailToStaff_ShouldContainBlurbAboutUnsubscribing()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "meat@backonthemenu.boys";

			conversation.Participants.AddNewParticipant(staff);

			Factory.Save();

			conversation.AddMessageFromCurrentUser("Number fountains", isInternal: false);
			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(conversation);

			AssertEquals(1, Env.AllEmailsCreated.Count());

			var body = Env.AllEmailsCreated.Single().Body;
			AssertContains("Should have included the eConversation message in the email notifiation", "Number fountains", body);
			Assert("Should have included the blurb about unsubscribing in the email notifiation", Regex.IsMatch(body, @"<p><em>If you no longer wish to be notified about .*, please unsubscribe yourself through the eConversation tab\.<\/em><\/p>"));
		}

		public void TestEmailToContact_ShouldNotContainBlurbAboutUnsubscribing()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "MissVanjie@Vanjie.com";

			conversation.Participants.AddNewParticipant(contact);

			Factory.Save();

			conversation.AddMessageFromCurrentUser("No! Universal XML.", isInternal: false);
			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(conversation);

			AssertEquals(1, Env.AllEmailsCreated.Count());

			var body = Env.AllEmailsCreated.Single().Body;
			AssertContains("Should have included the eConversation message in the email notifiation", "No! Universal XML", body);
			AssertNotContains("Should NOT have included the blurb about unsubscribing in the email notifiation, because there's no way for people to do this who don't have access to CW1.", "If you no longer wish to be notified about", body);
		}

		public void TestNotificationEmails_ShouldContainReplyViaEConversationLink()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "meat@backonthemenu.boys";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "MissVanjie@Vanjie.com";

			conversation.Participants.AddNewParticipant(staff);
			conversation.Participants.AddNewParticipant(contact);

			Factory.Save();

			conversation.AddMessageFromCurrentUser("And EConversation on half.", isInternal: false);
			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(conversation);

			AssertEquals(2, Env.AllEmailsCreated.Count());

			var emailToStaffBody = Env.AllEmailsCreated.Single(e => e.Recipients.Contains("meat@backonthemenu.boys")).Body;
			var emailToContactBody = Env.AllEmailsCreated.Single(e => e.Recipients.Contains("MissVanjie@Vanjie.com")).Body;

			AssertContains("Should have included the eConversation message in the email notifiation", "And EConversation on half.", emailToStaffBody);
			AssertContains("Should have included the eConversation message in the email notifiation", "And EConversation on half.", emailToContactBody);

			AssertContains("Should have included the 'reply via eConversation' link", "Reply via eConversation</a>", emailToStaffBody);
			AssertContains("Should have included the 'reply via eConversation' link", "Reply via eConversation</a>", emailToContactBody);
		}

		#endregion

		#region Email Subject

		public void TestEmailSubject()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			GenerateDummyMessageAndQueueEmailNotifications(parent);

			AssertEquals(1, Env.AllEmailsCreated.Count());

			var subject = Env.AllEmailsCreated.Single().Subject;
			AssertEquals("Should have the default email subject", "New Messages in " + parent.HumanReadableName, subject);
		}

		public void TestEmailSubjectContentOverride()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			parent.SetEmailSubjectContentOverride("SubjectContentOverride");
			Factory.Save();

			GenerateDummyMessageAndQueueEmailNotifications(parent);

			AssertEquals(1, Env.AllEmailsCreated.Count());

			var subject = Env.AllEmailsCreated.Single().Subject;
			AssertEquals("Should have the overriden email subject", "New Messages in SubjectContentOverride", subject);
		}

		#endregion

		#region From Display Name

		public void TestFromDisplayName()
		{
			using (RawDataRegistry.Instance.MailboxDisplayName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Company Name"))
			{
				DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

				var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
				Factory.Save();

				GenerateDummyMessageAndQueueEmailNotifications(parent);

				AssertEquals(1, Env.AllEmailsCreated.Count());

				var fromDisplayName = Env.AllEmailsCreated.Single().FromDisplayName;
				AssertEquals("Should have display name from the registry", "Company Name", fromDisplayName);
			}
		}

		#endregion

		#region From Address

		public void TestFromAddress()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			GenerateDummyMessageAndQueueEmailNotifications(parent);

			AssertEquals(1, Env.AllEmailsCreated.Count());

			var defaultEmail = new EmailDef();
			var fromAddress = Env.AllEmailsCreated.Single().FromAddress;
			AssertEquals("Should have the default from address", defaultEmail.FromAddress, fromAddress);
		}

		public void TestFromAddressOverride()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			parent.SetFromAddressOverride("conversations@neo.cargowise.com");
			Factory.Save();

			GenerateDummyMessageAndQueueEmailNotifications(parent);

			AssertEquals(1, Env.AllEmailsCreated.Count());

			var fromAddress = Env.AllEmailsCreated.Single().FromAddress;
			AssertEquals("Should have the overriden from address", "conversations@neo.cargowise.com", fromAddress);
		}

		#endregion

		[TestDate(2024, 1, 1)]
		[TestDateIncremental(0, 0, 0, 1)]
		public void TestGenerateAndQueueEmailNotifications_BySender()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "somethign@gtgg.com";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "something@else.com";

			var convo = CreateConversationWithSubscribers(staff, contact).eConversation;
			convo.AddMessageFromCurrentUser("Super secret new from one user", isInternal: true);
			convo.AddMessageFromCurrentUser("Public new from one user", isInternal: false);

			using (Env.SetTemporaryUserContext(new UserContext(otherUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				convo.AddMessageFromCurrentUser("Super secret new from other user", isInternal: true);
				convo.AddMessageFromCurrentUser("Public new from other user", isInternal: false);
			}

			Env.OutgoingMailManager.EmailsCreated.Clear();

			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(convo);

			var staffEmails = GetEmailsFor(staff);
			var contactEmails = GetEmailsFor(contact);

			CombineAssertions(() =>
			{
				AssertEquals("One email generated for each message sender", 2, staffEmails.Count());
				AssertEquals("One email generated for each message sender", 2, contactEmails.Count());

				AssertEquals("One staff email only has older public message", 1, staffEmails.Count(x => x.Body.Contains("Public new from one user") && !x.Body.Contains("Public new from other user")));
				AssertEquals("One staff email has both public messages", 1, staffEmails.Count(x => x.Body.Contains("Public new from one user") && x.Body.Contains("Public new from other user")));
				AssertEquals("One staff email only has older internal message", 1, staffEmails.Count(x => x.Body.Contains("Super secret new from one user") && !x.Body.Contains("Super secret new from other user")));
				AssertEquals("One staff email has both internal messages", 1, staffEmails.Count(x => x.Body.Contains("Super secret new from one user") && x.Body.Contains("Super secret new from other user")));

				AssertEquals("One contact email only has older public message", 1, contactEmails.Count(x => x.Body.Contains("Public new from one user") && !x.Body.Contains("Public new from other user")));
				AssertEquals("One contact email has both public messages", 1, contactEmails.Count(x => x.Body.Contains("Public new from one user") && x.Body.Contains("Public new from other user")));
				AssertEquals("Contact emails do not have internal messages", 0, contactEmails.Count(x => x.Body.Contains("Super secret new from one user") || x.Body.Contains("Super secret new from other user")));
			});
		}

		public void TestGenerateAndQueueEmailNotifications_System()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "somethign@gtgg.com";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "something@else.com";

			var convo = CreateConversationWithSubscribers(staff, contact).eConversation;
			var systemMessage = convo.Messages.AddNew();
			systemMessage.JCM_IsSystem = true;

			Env.OutgoingMailManager.EmailsCreated.Clear();

			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(convo, null, new[] { systemMessage });

			var staffEmails = GetEmailsFor(staff);
			var contactEmails = GetEmailsFor(contact);

			CombineAssertions(() =>
			{
				AssertEquals("One staff email generated for a system message", 1, staffEmails.Count());
				AssertEquals("One contact email generated for a system message", 1, contactEmails.Count());
			});
		}

		public void TestEmailUsesRegistryTemplate()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);
			var template = new NotificationEmailTemplate
			{
				EmailSubject = "Some Email Subject for (*ID*)",
				EmailBody = "Some Email Body (*EmailIdentifier*)"
			};

			using (SystemDataRegistry.Instance.EConversationMessageEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template))
			{
				var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
				Factory.Save();

				GenerateDummyMessageAndQueueEmailNotifications(parent);

				AssertEquals(1, Env.AllEmailsCreated.Count());

				var email = Env.AllEmailsCreated.Single();

				AssertEquals($"Some Email Subject for {parent.HumanReadableName}", email.Subject);
				AssertEquals($"Some Email Body {EConversationUniqueIDUtil.GenerateElement(parent)}", email.Body);
			}
		}

		public void TestEmailAddsIdIfMissingFromSubject()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkingLanguage = Core.SharedConstants.Languages.German;
			staff.GS_EmailAddress = "staff@work.com.au";

			Factory.Save();

			using (var mockEng = Res.GetLanguageInstance(staff.GS_WorkingLanguage).UseMockData())
			{
				mockEng.Put(RegistryEmailSubjectResStringKey, new ResourceStringData(RegistryEmailSubjectResStringKey, "Malformed email subject translation"));

				var convo = CreateConversationWithSubscribers(staff).eConversation;
				convo.AddMessageFromCurrentUser("Hello", false);

				EConversationEmailBuilder.GenerateAndQueueEmailNotifications(convo);

				var email = GetEmailsFor(staff).Single();

				AssertEquals($"Malformed email subject translation {convo.Parent.HumanReadableName}", email.Subject);
			}
		}

		public void TestEmailHasCorrectHeader()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var eConvo = CreateConversationWithSubscribers(CreateContact()).eConversation;
			eConvo.AddMessageFromCurrentUser("New Header stuff is here (probably)", false);

			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(eConvo);
			var headers = Env.OutgoingMailManager.EmailsCreated.Single().Headers;

			Assert("X-Auto-Response-Suppress exists", headers.TryGetValue("X-Auto-Response-Suppress", out var xAutoSuppressHead));
			AssertEquals("X-Auto-Response-Suppress value", "All", xAutoSuppressHead);
		}

		#region Implementation

		const string RegistryEmailSubjectResStringKey = "EConversationMessageEmailTemplateDefaultSubject";
		const string RegistryEmailBodyResStringKey = "EConversationMessageEmailTemplateDefaultBody";

		IEnumerable<EmailDef> GetEmailsFor(IConversationParticipant recipient)
			=> Env.OutgoingMailManager.EmailsCreated.Where(email => email.Recipients.Contains(recipient.Email));

		void AssertGeneratedEmailContainsMessages(string assertionMessage, JobConversation eConvo, IEnumerable<string> expected)
		{
			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(eConvo);
			var email = Env.OutgoingMailManager.EmailsCreated.Single();

			CombineAssertions(assertionMessage, () =>
			{
				foreach (var message in expected)
				{
					AssertContains(message, email.Body);
				}
			});
		}

		OrgContact CreateContact(string email = null)
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = email ?? (Guid.NewGuid().ToString().Replace("-", "") + "@gmail.com");
			return contact;
		}

		ICollection<string> GenerateAndGetRecipients(JobConversation convo)
		{
			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(convo);
			return Env.OutgoingMailManager.EmailsCreated
				.SelectMany(email => email.Recipients.Cast<RecipientDef>())
				.Select(r => r.Email)
				.ToList();
		}

		void GenerateDummyMessageAndQueueEmailNotifications(DummyConversationProvider parent)
		{
			var conversation = JobConversation.GetOrCreate(parent);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@test.com";
			conversation.Participants.AddNewParticipant(staff);

			Factory.Save();

			conversation.AddMessageFromCurrentUser("Test", false);
			EConversationEmailBuilder.GenerateAndQueueEmailNotifications(conversation);
		}

		#endregion

		#region Dummy data

		readonly string[] someRandomTextForMessages = // Twenty lines of lorem ipsum
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse nec nisl et dolor posuere viverra sit amet et sem. Aenean nec lacus semper turpis imperdiet eleifend. In non sem vitae quam ornare commodo vel a leo. Fusce nec dui orci. Maecenas blandit ipsum id tortor viverra, hendrerit elementum lectus sagittis. Nulla vitae molestie tellus. Curabitur felis ipsum, ornare non euismod eget, mattis a justo. Phasellus augue lectus, gravida at nisl eu, ornare pulvinar tellus. Suspendisse a dolor mi. Sed rhoncus sem a nisi porttitor bibendum. Etiam at tempus velit. Duis eu enim suscipit, consequat purus ac, lobortis mi. Nulla vestibulum purus quis est finibus, vitae sagittis eros ultricies. Aenean quis purus sapien.
Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Maecenas vitae mauris in risus lacinia dapibus ut viverra felis. Curabitur vitae velit odio. Donec ut fermentum diam. Cras ultrices nibh nibh. Aenean porttitor justo eu elit tincidunt, a scelerisque leo maximus. Vestibulum in nisl vel mi posuere elementum. Fusce porta bibendum nulla, sed commodo est ultricies eget. Maecenas in ante turpis. In tincidunt porttitor diam in pulvinar.
Mauris mattis massa erat. Sed quis lacinia mi. Nam vehicula lectus ac massa facilisis congue. Cras iaculis elit et massa ultricies, ut posuere risus lacinia. Aliquam consectetur maximus imperdiet. Maecenas purus justo, blandit ac cursus vel, varius non diam. Integer maximus mi non egestas vestibulum. Integer quis tortor sed nunc cursus mollis egestas non nibh. Praesent imperdiet tellus et quam consectetur convallis at et dui. Nulla dictum elit in ultricies elementum. Morbi at ipsum iaculis, consectetur dui viverra, tempus odio. Pellentesque mi eros, euismod et nulla eu, maximus rhoncus lacus. Donec vel lorem efficitur, tempor dolor eget, gravida orci. Morbi ut purus eu mauris consequat placerat. Nullam faucibus sodales molestie. Morbi odio augue, pellentesque et rhoncus vel, viverra sit amet nibh.
Donec consectetur euismod felis lobortis suscipit. Interdum et malesuada fames ac ante ipsum primis in faucibus. Praesent fermentum sit amet velit ac euismod. Morbi vel est quis tortor pretium ultrices. Praesent maximus mi et quam fermentum sagittis. Nullam elementum diam ut bibendum mollis. Nullam in est ut erat pretium viverra vitae ac risus. Quisque tristique orci ac justo hendrerit, et varius tortor suscipit. Sed fringilla pellentesque tincidunt. Vestibulum purus velit, maximus nec libero et, suscipit condimentum ipsum. Vestibulum ut risus lectus.
Donec ex sapien, suscipit quis euismod vel, dignissim eu ex. Sed sagittis metus et leo pellentesque, nec convallis diam molestie. Vestibulum id laoreet dolor. Maecenas quis ante dignissim, suscipit libero eu, pellentesque arcu. Nam blandit eu mauris ut finibus. Vivamus quis varius lorem. Curabitur pulvinar nisi vel maximus cursus. Fusce eu iaculis felis. Aenean eu faucibus arcu. Morbi nisl velit, venenatis quis metus et, eleifend ornare orci.
Praesent aliquet imperdiet turpis non pretium. Mauris at felis a lacus consequat dignissim. Cras risus libero, molestie sit amet cursus sed, pharetra eget quam. Vivamus et nunc quis sem venenatis finibus sit amet ut ligula. Proin vel ligula at augue mattis imperdiet non quis arcu. Phasellus vitae enim id ipsum mattis feugiat eu mollis justo. Donec ut augue rhoncus, malesuada ligula ut, rutrum diam.
Pellentesque vehicula est et nisi vestibulum, nec maximus dolor fermentum. Ut id diam ante. Sed sit amet dui quam. In hac habitasse platea dictumst. Etiam varius aliquet erat a fringilla. Praesent nulla lorem, lacinia sit amet vehicula in, commodo vitae neque. Cras ultricies ut sem quis luctus. Phasellus ullamcorper cursus nisi, vitae varius dui tincidunt ut. Fusce ultricies, lorem eget placerat egestas, turpis elit laoreet est, sit amet accumsan ligula libero eu magna. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae;
Suspendisse euismod, enim in viverra cursus, lorem sem mollis ligula, vitae lacinia justo arcu ac nisl. In id urna ac purus accumsan finibus. Curabitur malesuada eros sit amet ipsum fringilla, nec feugiat nisi vulputate. Aliquam a lacus eu lacus auctor commodo. Phasellus vitae quam quam. Aliquam semper, urna vel malesuada varius, justo nulla commodo turpis, eget scelerisque eros massa ac libero. Ut mi leo, malesuada ac enim vitae, aliquam malesuada massa. Pellentesque ac elit tincidunt, venenatis dolor eget, vulputate neque. Sed et dui rutrum, pretium orci quis, tempus nulla. Fusce sed dignissim metus, eget fringilla diam. Nunc sit amet metus eu libero posuere porta. Mauris volutpat pulvinar imperdiet. Etiam molestie ac neque nec congue. Aliquam non nulla a felis vulputate cursus. Nunc varius augue condimentum erat interdum, sit amet fermentum dui aliquet.
Ut ac facilisis arcu, eget egestas quam. Cras fermentum sagittis mattis. Vivamus vel neque at augue commodo dictum vel bibendum urna. Pellentesque quis quam nunc. Integer posuere felis ut rutrum ultrices. Ut consectetur mauris lacus, sit amet eleifend ipsum molestie ac. In ac lorem tincidunt, accumsan lacus non, dignissim urna. Nulla faucibus, est eu tristique ullamcorper, orci dolor commodo ante, sit amet hendrerit purus mauris ornare dui. Morbi ut lobortis libero, a porta urna. Duis ipsum eros, egestas vitae eros a, imperdiet malesuada augue. Aenean ultricies, nulla sed eleifend malesuada, lacus augue consequat justo, ac tincidunt mi nulla id nulla.
Aenean et commodo neque. Ut eleifend eros quam, a egestas nulla egestas quis. In nec nunc interdum, auctor ligula non, mollis purus. Nulla eleifend velit tristique, venenatis elit interdum, fringilla turpis. Integer placerat auctor ligula, auctor vestibulum augue fringilla eget. Phasellus a fermentum elit. Nulla volutpat, quam sollicitudin convallis commodo, lectus lacus eleifend lectus, a pellentesque massa mi in libero.
Mauris vitae faucibus neque, at cursus velit. Vestibulum aliquam viverra aliquam. Vestibulum ac ante enim. Curabitur venenatis nisi in tincidunt blandit. In lobortis eu risus eu cursus. Sed et ullamcorper diam. Nam urna sapien, scelerisque sit amet ullamcorper nec, aliquam sed lorem.
Morbi nibh purus, mattis eget nunc non, porttitor pharetra orci. Mauris porta ac odio blandit mattis. Suspendisse sed consequat lectus, eget vulputate lacus. Vivamus eget sagittis turpis. Quisque a volutpat elit. Quisque faucibus erat et egestas vulputate. Sed a ultricies mauris. Maecenas pellentesque facilisis varius. Nunc id quam in justo tristique tempus vitae condimentum erat. Aenean eu orci vel diam mattis tempus eu eget ipsum. Nunc eget volutpat sapien, sed egestas ex. Quisque imperdiet odio est, vitae pretium eros pellentesque dapibus. In a venenatis lorem, at eleifend urna. Mauris non mi lectus.
Suspendisse congue elit id suscipit dapibus. Mauris tempor massa dignissim imperdiet euismod. Sed eget feugiat lorem, non sodales tellus. Nunc ornare tortor a viverra ullamcorper. In pharetra elementum erat, sed vulputate dui volutpat eu. Etiam tristique ex eu ex cursus ullamcorper. Nulla in lobortis purus. Donec in orci quam. Quisque vel magna dui. Curabitur et facilisis enim. Proin mollis tincidunt consectetur. Praesent bibendum tortor et dictum mollis. Vivamus molestie erat nunc, vitae suscipit arcu egestas id. Sed in enim libero. Nam ullamcorper lectus massa, ut accumsan leo malesuada nec. Integer at augue ornare, congue erat at, lobortis velit.
Cras at erat mi. Etiam sit amet posuere nibh. Aenean lobortis finibus ligula, sit amet efficitur arcu malesuada id. Integer sed maximus purus, quis egestas ipsum. Quisque ut cursus urna, et bibendum nisl. Sed in tincidunt ligula. Phasellus sed aliquam nulla, quis luctus justo. Curabitur tincidunt ex eget velit consequat, eu aliquam est pellentesque. Curabitur accumsan luctus arcu ac venenatis. Duis imperdiet lacinia maximus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer sodales efficitur tortor, ac sagittis turpis dignissim sagittis. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Quisque rhoncus molestie ipsum, a scelerisque urna pretium vel. Duis malesuada purus quis dolor dapibus porttitor. Curabitur venenatis hendrerit efficitur.
Suspendisse ac faucibus urna, quis maximus urna. Sed gravida velit nec diam vulputate consectetur. Etiam tortor metus, semper eu felis vitae, condimentum imperdiet lectus. Fusce eu suscipit justo, a ultricies massa. Sed molestie eget risus id interdum. Cras eu nisl ut augue iaculis accumsan a in odio. Nullam finibus posuere eros. Aliquam quis nunc in diam lacinia bibendum. Nam porttitor ex quis tortor tempor volutpat non sed sapien.
Vivamus bibendum nisi ut tincidunt dictum. Ut in vulputate massa, eu mollis eros. Nullam mattis tellus nec nisl lacinia, a aliquam quam lacinia. Mauris nunc sapien, ornare quis augue bibendum, elementum placerat turpis. Curabitur eget massa orci. Vestibulum sed neque vitae mi viverra feugiat. Morbi consectetur dui quis lectus blandit, vel aliquam ante fringilla.
Curabitur sed dui dapibus, sodales felis sed, ornare tellus. Sed erat felis, tincidunt vitae ligula in, laoreet aliquet ante. Duis id consectetur metus. Integer pulvinar, eros et ornare facilisis, augue metus eleifend diam, ut pretium erat neque eget lorem. Suspendisse semper congue elit sed malesuada. Integer tristique massa non turpis aliquam pretium. Morbi pharetra neque dolor, quis pretium risus dignissim sit amet. Aenean sit amet augue dui. Aenean pharetra scelerisque bibendum. Donec lacinia lectus sit amet nulla venenatis pulvinar. Maecenas tincidunt, velit quis tempor sagittis, sapien libero bibendum metus, et pretium elit ligula sit amet augue. Aliquam et nisl rhoncus, porttitor sapien a, congue neque. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Etiam dapibus lacus eu congue porta.
Cras porttitor, leo id ornare finibus, velit metus laoreet quam, vel consequat lacus justo sit amet nunc. Nullam suscipit auctor turpis, id egestas neque vehicula nec. Nulla nec lectus iaculis, dictum ligula efficitur, accumsan diam. Donec id tempor sapien, non varius velit. Donec eget ante at nunc sodales egestas. In in efficitur metus. Etiam ut vestibulum magna, et porttitor sem. Donec eu dui nec libero hendrerit congue ac id tellus. Vestibulum feugiat orci vitae tempor blandit. Fusce porttitor risus vitae lobortis finibus. Pellentesque vel blandit justo. Vivamus ut est vitae tortor aliquam ultricies. Aliquam et sagittis lectus, sit amet tincidunt tellus.
Nulla facilisi. Pellentesque luctus vestibulum metus at porta. Suspendisse elementum dictum elementum. Mauris placerat purus et lobortis laoreet. Nam laoreet vitae diam sit amet mattis. Vestibulum posuere arcu in malesuada lobortis. Phasellus sem nibh, lobortis eu tincidunt ut, scelerisque eget purus. Integer vulputate ullamcorper faucibus. Vestibulum nec facilisis nisi. Mauris placerat mauris faucibus, ultrices massa ut, vehicula enim. Duis auctor id turpis eu dapibus. Aenean condimentum sed nisl quis elementum. In luctus bibendum nunc. Suspendisse ornare eros eu lacus posuere malesuada. Duis mauris nunc, auctor ac libero tempor, ultrices eleifend mauris. Fusce quis ipsum lobortis, elementum orci vel, ultrices eros.
Donec at pretium libero. Fusce faucibus placerat ligula eget tincidunt. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Vivamus lobortis viverra nisl non porttitor. Donec mollis mauris urna, quis vestibulum sem aliquam id. Phasellus at ornare eros. Fusce congue felis quis arcu luctus, at elementum tortor aliquet. Maecenas gravida est quis neque porta, id ullamcorper orci hendrerit. Maecenas mattis fermentum dui eu consectetur. Morbi aliquet feugiat risus, in sodales ligula tempor id. Fusce elementum faucibus ligula et molestie. Aenean sit amet sagittis dui. Vivamus hendrerit iaculis enim, et fermentum nisi lacinia id. Nulla facilisi. Mauris eu felis varius justo egestas venenatis.".Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

		#endregion
	}
}
