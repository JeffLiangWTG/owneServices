using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.Environment;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing
{
	[TestedType(typeof(JobConversationMessageLogSubscriber))]
	class JobConversationMessageLogSubscriberTest : LogSubscriberTest<JobConversationMessageLogSubscriber>
	{
		public void TestDbHits()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "staff2@wisetechglobal.com";

			for (var index = 0; index < 20; index++)
			{
				var parentJob = DummyConversationProvider.CreateConversationWithSubscribers(staff1, staff2);
				var conversation = parentJob.eConversation;
				var message1 = conversation.AddMessageFromCurrentUser("Message 1", isInternal: true);
				var message2 = conversation.AddMessageFromCurrentUser("Message 2", isInternal: true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				message2.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var expectedDbHits = new Dictionary<string, int>
			{
				{ DummyBizoSchema.Constants.TableName, 2 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ JobConversationSchema.Constants.TableName, 2 },
				{ JobConversationMessageSchema.Constants.TableName, 4 },
				{ JobConversationParticipantSchema.Constants.TableName, 2 },
			};

			using (AssertDbHitsForAllFactories(expectedDbHits, useOnlyNewFactories: true, includeFactoryPredicate: f => f.NameForDebugging.StartsWith("Subscriber")))
			{
				RunLogWalkerCycleForTest();

				CombineAssertions("Adding conversation in non CW1 module, should send notification", () =>
				{
					AssertEquals("Total Email sent", 40, Env.OutgoingMailManager.EmailsCreated.Count);
					var email = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals("Email recipient", 1, email.Recipients.Count);
					AssertContains("Email recipient address", "@wisetechglobal.com", email.Recipients[0].Email);
				});
			}
		}

		[TestDate(2018, 1, 1)]
		[TestDateIncremental(0, 0, 0, 10)]
		public void TestEmailSent()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";

			var parentJob = DummyConversationProvider.CreateConversationWithSubscribers(staff1);
			var conversation = parentJob.eConversation;
			var message1 = conversation.AddMessageFromCurrentUser("Existing Message 1", isInternal: true);
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(10);
			var message2 = conversation.AddMessageFromCurrentUser("Message 2", isInternal: true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			message2.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(10);
			var message3 = conversation.AddMessageFromCurrentUser("Message 3", isInternal: true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			message3.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(10);
			var message4 = conversation.AddMessageFromCurrentUser("Message 4", isInternal: true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			message4.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunLogWalkerCycleForTest();

			CombineAssertions("Adding conversation in non CW1 module, should send notification", () =>
			{
				AssertEquals("Total Email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertContainsExactElementsInAnyOrder("Email recipient address", new[] { "staff1@wisetechglobal.com" }, email.Recipients.Cast<RecipientDef>().Select(recipient => recipient.Email));
				AssertEquals("Email subject", "New Messages in DummyBizo", email.Subject);
				AssertContains("Email Header", $@"<h1>DummyBizo - New Messages</h1>", email.Body);
				AssertContains("Email - job url", $@"New messages have been added to <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK={parentJob.PK}", email.Body);
				AssertContains("Email - body", $@"You will find the new messages below, with some previous messages to provide additional context.", email.Body);
				AssertContains("Email - footer", $@"<p><em>If you no longer wish to be notified about <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK={parentJob.PK}", email.Body);

				var newMessageHeaderIndex = email.Body.IndexOf("<h2>New Messages</h2>");
				var previousMessageHeaderIndex = email.Body.IndexOf("<h2>Previous Messages</h2>");

				var message2Index = email.Body.IndexOf("Message 2");
				var message3Index = email.Body.IndexOf("Message 3");
				var message4Index = email.Body.IndexOf("Message 4");
				Assert("Message 2 should be categorised as 'New Message'", message2Index > newMessageHeaderIndex && message2Index < previousMessageHeaderIndex);
				Assert("Message 3 should be categorised as 'New Message'", message3Index > newMessageHeaderIndex && message3Index < previousMessageHeaderIndex);
				Assert("Message 4 should be categorised as 'New Message'", message4Index > newMessageHeaderIndex && message4Index < previousMessageHeaderIndex);

				var message1Index = email.Body.IndexOf("Existing Message 1");
				Assert("Existing Message 1 should be categorised as 'Previous Message'", message1Index > previousMessageHeaderIndex);
			});
		}

		public void TestEmailNotSent_ForIncident()
		{
			JobConversation conversation = AssertNotificationCreated();

			conversation.JCC_ParentTableCode = IncidentRequestSchema.Constants.Prefix;
			var message = conversation.AddMessageFromCurrentUser("Message 2", isInternal: true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			message.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			AssertNoNotificationCreated(noOfLogProcessed: 2);
		}

		public void TestEmailNotSent_ForCandidateApplication()
		{
			//Pre-condition
			_ = AssertNotificationCreated();

			//Create Candididate
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "borris@gmail.com";
			applicant.HA_FullName = "Borris Johnson";

			var application = applicant.Applications.AddNew();
			application.FillWithValidTestData();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(application);
			_ = candidate.EConversation; //Simulate control binding

			//Meessage added to Rootconversation
			var rootConversation = candidate.EConversation.RootConversation;
			var message1 = rootConversation.AddMessageFromCurrentUser("TestMessage", true, false);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			message1.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			AssertNoNotificationCreated(noOfLogProcessed: 2);

			//Meessage added to different conversation
			var convo = JobConversation.CreateWithoutCheckingForExistingConversation(candidate.Application, candidate.Factory);
			convo.Participants.AddNewParticipant("borris@gmail.com");
			convo.Participants.AddNewParticipant("recruiter1@gmail.com");

			var message2 = convo.AddMessageFromCurrentUser("Normal Message1", true, false);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			message2.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			AssertNoNotificationCreated(noOfLogProcessed: 3);
		}

		[TestDate(2018, 1, 1)]
		[TestDateIncremental(0, 0, 0, 10)]
		public void TestEmailSent_MessagesFromMultipleSenders()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";

			var parentJob = DummyConversationProvider.CreateConversationWithSubscribers(staff1);
			var conversation = parentJob.eConversation;
			var message1 = conversation.AddMessageFromCurrentUser("Message from one user", isInternal: true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			message1.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var message2 = conversation.AddMessageFromCurrentUser("Another message from one user", isInternal: true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			message2.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			var anotherUser = Factory.NewWithValidTestData<GlbStaff>();
			using (Env.SetTemporaryUserContext(new UserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(10);
				var message3 = conversation.AddMessageFromCurrentUser("Message from another user", isInternal: true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				message3.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				var message4 = conversation.AddMessageFromCurrentUser("Another message from another user", isInternal: true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				message4.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

				Factory.Save();
			}

			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunLogWalkerCycleForTest();

			CombineAssertions("Adding conversation in non CW1 module, should send notifications", () =>
			{
				AssertEquals("Total Email sent", 2, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];

				var newMessageHeaderIndex = email.Body.IndexOf("<h2>New Messages</h2>");
				var previousMessageHeaderIndex = email.Body.IndexOf("There are no previous messages");

				var message1Index = email.Body.IndexOf("Message from one user");
				var message2Index = email.Body.IndexOf("Another message from one user");
				Assert("Message1 should be categorised as 'New Message'", message1Index > newMessageHeaderIndex && message1Index < previousMessageHeaderIndex);
				Assert("Message2 should be categorised as 'New Message'", message2Index > newMessageHeaderIndex && message2Index < previousMessageHeaderIndex);

				var secondEmail = Env.OutgoingMailManager.EmailsCreated[1];

				var secondNewMessageHeaderIndex = secondEmail.Body.IndexOf("<h2>New Messages</h2>");
				var secondPreviousMessageHeaderIndex = secondEmail.Body.IndexOf("<h2>Previous Messages</h2>");

				var message3Index = secondEmail.Body.IndexOf("Message from another user");
				var message4Index = secondEmail.Body.IndexOf("Another message from another user");
				Assert("Message3 should be categorised as 'New Message'", message3Index > secondNewMessageHeaderIndex && message3Index < secondPreviousMessageHeaderIndex);
				Assert("Message4 should be categorised as 'New Message'", message4Index > secondNewMessageHeaderIndex && message4Index < secondPreviousMessageHeaderIndex);

				message1Index = secondEmail.Body.IndexOf("Message from one user");
				message2Index = secondEmail.Body.IndexOf("Another message from one user");
				Assert("Message from one user should be categorised as 'Previous Message'", message1Index > secondPreviousMessageHeaderIndex);
				Assert("Message from one user should be categorised as 'Previous Message'", message2Index > secondPreviousMessageHeaderIndex);
			});
		}

		[TestDate(2018, 1, 1)]
		public void TestEmailSent_SystemMessage()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";

			var parentJob = DummyConversationProvider.CreateConversationWithSubscribers(staff1);
			var conversation = parentJob.eConversation;
			var systemMessage = conversation.Messages.AddNew();
			systemMessage.JCM_Body = "System Message";
			systemMessage.JCM_IsSystem = true;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			systemMessage.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunLogWalkerCycleForTest();

			CombineAssertions("Adding conversation in non CW1 module, should send notifications", () =>
			{
				AssertEquals("Total Email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];

				var newMessageHeaderIndex = email.Body.IndexOf("<h2>New Messages</h2>");
				var previousMessageHeaderIndex = email.Body.IndexOf("There are no previous messages");

				var messageIndex = email.Body.IndexOf("System Message");
				Assert("System Message should be categorised as 'New Message'", messageIndex > newMessageHeaderIndex && messageIndex < previousMessageHeaderIndex);
			});
		}

		JobConversation AssertNotificationCreated()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";

			var parentJob = DummyConversationProvider.CreateConversationWithSubscribers(staff1);
			var conversation = parentJob.eConversation;

			var message = conversation.AddMessageFromCurrentUser("Message 1", isInternal: true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			message.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunLogWalkerCycleForTest();

			CombineAssertions("Adding conversation in non CW1 module for NON-INCIDENT, should send notification", () =>
			{
				AssertEquals("Total Email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertLog(noOfLogProcessed: 1);

				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertContainsExactElementsInAnyOrder("Email recipient address", new[] { "staff1@wisetechglobal.com" }, email.Recipients.Cast<RecipientDef>().Select(recipient => recipient.Email));
			});
			return conversation;
		}

		void AssertNoNotificationCreated(int noOfLogProcessed)
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunLogWalkerCycleForTest();

			CombineAssertions("Adding conversation in non CW1 module for INCIDENT, should NOT send notification BECAUSE it has been handled by EdiIncidentConversationMessageQueue", () =>
			{
				AssertEquals("Total Email sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertLog(noOfLogProcessed);
			});
		}

		void AssertLog(int noOfLogProcessed)
		{
			AssertEquals("No error should be logged", string.Empty,
								string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(notifiedEvent => notifiedEvent.StartsWith("[Job Conversation Message Log Subscriber]"))));
		}
	}
}
