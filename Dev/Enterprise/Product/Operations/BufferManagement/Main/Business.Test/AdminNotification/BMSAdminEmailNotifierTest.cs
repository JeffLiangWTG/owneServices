using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business.Test
{
	public class BMSAdminEmailNotifierTest : TestCaseWithFactory
	{
		public void TestShouldReportImmediately_IfThereIsNoEventOfSuchKind()
		{
			AssertEmailNotSent("Precondition");
			NotifyAndAssertEmailSent("Event should be reported immediately", "A", "Wow!", "Surprise!", 1);
			NotifyAndAssertEmailSent("Event should be reported immediately as it has a different key", "B", "Fantastic!", "I can't believe it!", 1);
		}

		public void TestShouldDeferReporting_IfReportedRecently()
		{
			AssertEmailNotSent("Precondition");
			NotifyAndAssertEmailSent("Precondition", "A", "Wow!", "Surprise!", 1);

			NotifyAndAssertEmailNotSent("Report should be deferred as reported recently", "A", "Wow!", "Surprise!");
			NotifyAndAssertEmailNotSent("Report should be deferred as reported recently", "A", "Wow!", "Surprise!");

			TimeProvider.Sleep(initialAccumulateTime);
			AssertEmailSent("Should report about two occurrences", "A", "Wow!", "Surprise!", 2);
		}

		public void TestShouldDeferReporting_IfNotReachedThresholdAndReleaseTimePassed()
		{
			AssertEmailNotSent("Precondition");

			BMSRegistry.Instance.NotificationThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);

			NotifyAndAssertEmailNotSent("Report should be deferred as reported recently", "A", "Wow!", "Surprise!");
			NotifyAndAssertEmailNotSent("Report should be deferred as reported recently", "A", "Wow!", "Surprise!");

			TimeProvider.Sleep(TimeSpan.FromMinutes(initialAccumulateTime.Minutes));

			AssertEmailNotSent("Do not reach threshold");

			NotifyAndAssertEmailNotSent("Report should be deferred as reported recently", "A", "Wow!", "Surprise!");
			NotifyAndAssertEmailNotSent("Report should be deferred as reported recently", "A", "Wow!", "Surprise!");
			NotifyAndAssertEmailNotSent("Report should be deferred as reported recently", "A", "Wow!", "Surprise!");
			NotifyAndAssertEmailNotSent("Report should be deferred as reported recently", "A", "Wow!", "Surprise!");

			TimeProvider.Sleep(initialAccumulateTime);
			AssertEmailSent("Should report about four occurrences", "A", "Wow!", "Surprise!", 4);
		}

		public void TestShouldReportImmediately_IfReportedAWhileAgo()
		{
			AssertEmailNotSent("Precondition");
			NotifyAndAssertEmailSent("Precondition", "A", "Wow!", "Surprise!", 1);
			NotifyAndAssertEmailNotSent("Precondition: Report should be deferred as reported recently", "A", "Wow!", "Surprise!");

			TimeProvider.Sleep(initialAccumulateTime);
			AssertEmailSent("Precondition: Should report about the occurrence", "A", "Wow!", "Surprise!", 1);

			TimeProvider.Sleep(TimeSpan.FromMinutes(initialAccumulateTime.Minutes * 4));
			NotifyAndAssertEmailSent("Precondition: Report should be reported immediately", "B", "Fantastic!", "I can't believe it!", 1);
			NotifyAndAssertEmailSent("Event should be reported immediately as the last occurrence with the same key was a while ago", "A", "Wow!", "Surprise!", 1);
		}

		public void TestShouldBeAbleToChangeItsAccumulateTime()
		{
			AssertEmailNotSent("Precondition");
			NotifyAndAssertEmailSent("Precondition", "A", "Wow!", "Surprise!", 1);
			NotifyAndAssertEmailNotSent("Precondition: Report should be deferred as reported recently", "A", "Wow!", "Surprise!");

			int initialAccumulateTime = BMSRegistry.Instance.NotificationPeriod.Value;
			int shorterAccumulateTime = initialAccumulateTime / 2;
			BMSRegistry.Instance.NotificationPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shorterAccumulateTime); //this will change the timer at the next iteration of reporting

			TimeProvider.Sleep(TimeSpan.FromMinutes(initialAccumulateTime));
			AssertEmailSent("Precondition: Should report about the occurrence", "A", "Wow!", "Surprise!", 1);

			NotifyAndAssertEmailNotSent("Precondition: Report should be deferred as reported recently", "A", "Wow!", "Surprise!");

			TimeProvider.Sleep(TimeSpan.FromMinutes(shorterAccumulateTime));
			AssertEmailSent("Should report about one occurrence", "A", "Wow!", "Surprise!", 1);
		}

		public void TestShouldReportImmediately_IfAccumulationIsSwitchedOff()
		{
			AssertEmailNotSent("Precondition");
			BMSRegistry.Instance.NotificationPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			NotifyAndAssertEmailSent("Precondition", "A", "Wow!", "Surprise!", 1);

			NotifyAndAssertEmailSent("Should report immediately", "A", "Wow!", "Surprise!", 1);
		}

		public void TestShouldBeAbleToSwitchAccumulationOnAndOff()
		{
			AssertEmailNotSent("Precondition");
			NotifyAndAssertEmailSent("Precondition", "A", "Wow!", "Surprise!", 1);

			NotifyAndAssertEmailNotSent("Precondition: Reports should be deferred as reported recently", "A", "Wow!", "Surprise!");
			NotifyAndAssertEmailNotSent("Precondition: Reports should be deferred as reported recently", "A", "Wow!", "Surprise!");

			//switch off
			int initialAccumulateTime = BMSRegistry.Instance.NotificationPeriod.Value;
			BMSRegistry.Instance.NotificationPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			NotifyAndAssertEmailSent("Should report about three occurrences", "A", "Wow!", "Surprise!", 3);

			NotifyAndAssertEmailSent("Report should be reported immediately as accumulation is switched off", "A", "Wow!", "Surprise!", 1);
			NotifyAndAssertEmailSent("Report should be reported immediately as accumulation is switched off", "A", "Wow!", "Surprise!", 1);

			//switch on
			BMSRegistry.Instance.NotificationPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, initialAccumulateTime);

			NotifyAndAssertEmailNotSent("Reports should be deferred as the same events happened before switching off", "A", "Wow!", "Surprise!");
			NotifyAndAssertEmailNotSent("Reports should be deferred as the same events happened before switching off", "A", "Wow!", "Surprise!");

			TimeProvider.Sleep(TimeSpan.FromMinutes(initialAccumulateTime));
			AssertEmailSent("Should report about two occurrences", "A", "Wow!", "Surprise!", 2);
		}

		public void TestShouldReport_WhenUseDifferentNotificationGroup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "some@admin.com";
			var notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(notificationGroup);
			EnvProxy.Instance.Registry.RawRegistry.InfrastructureErrorsNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			Factory.Save();

			AssertEmailNotSent("Precondition");
			NotifyAndAssertEmailSent("Something happen", "BLA", "Bad news!", "Ops!", 1, EnvProxy.Instance.Registry.RawRegistry.InfrastructureErrorsNotificationGroup);
		}

		#region Implementation

		MockTimeProvider TimeProvider;
		readonly TimeSpan initialAccumulateTime = TimeSpan.FromMinutes(10);
		string staffEmail;

		protected override void SetUp()
		{
			base.SetUp();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staffEmail = "responsible@admin.com";
			staff.GS_EmailAddress = staffEmail;

			var bmsNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);

			Factory.Save();

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());
			BMSRegistry.Instance.NotificationPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, initialAccumulateTime.Minutes);
			BMSRegistry.Instance.NotificationThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			TimeProvider = new MockTimeProvider(DateTime.UtcNow, DateTime.Now); // Test time does not require to be accurate
			BMSAdminEmailNotifier.MockTimeProviderForTesting(TimeProvider);
		}

		protected override void TearDown()
		{
			BMSAdminEmailNotifier.MockTimeProviderForTesting();
			base.TearDown();
		}

		void NotifyAndAssertEmailSent(string errorMessage, string key, string subject, string body, int expectedOccurrencesCount, GuidRegistryItem notificationGroup = null)
		{
			if (notificationGroup == null)
			{
				notificationGroup = BMSRegistry.Instance.NotificationGroup;
			}

			PrepareToTheNextNotification();
			BMSAdminEmailNotifier.Notify(key, subject, body, notificationGroup);

			var glbGroup = Factory.Load<GlbGroup>(notificationGroup.Value);
			var email = glbGroup.Staff[0].GS_EmailAddress;

			AssertEmailSent(errorMessage, key, subject, body, expectedOccurrencesCount, email);
		}

		void NotifyAndAssertEmailNotSent(string errorMessage, string key, string subject, string body, GuidRegistryItem notificationGroup = null)
		{
			if (notificationGroup == null)
			{
				notificationGroup = BMSRegistry.Instance.NotificationGroup;
			}

			PrepareToTheNextNotification();
			BMSAdminEmailNotifier.Notify(key, subject, body, notificationGroup);
			AssertEmailNotSent(errorMessage);
		}

		void PrepareToTheNextNotification()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		void AssertEmailSent(string errorMessage, string expectedKey, string expectedSubject, string expectedBody, int expectedOccurrencesCount, string recipient = null)
		{
			if (recipient == null)
			{
				recipient = staffEmail;
			}

			CombineAssertions(errorMessage, () =>
			{
				AssertEquals("The administrator should be notified", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var lastSentEmail = Env.OutgoingMailManager.EmailsCreated.Last();
				AssertEquals("The email should contain the certain topic", expectedSubject, lastSentEmail.Subject);
				AssertEquals("The Email should be send to " + recipient, recipient, lastSentEmail.Recipients[0].Email);
				AssertContains($@"The email should contain the certain body. Expected email body:
					{expectedBody}

					Actual email body:
					{lastSentEmail.Body}", expectedBody, lastSentEmail.Body);

				var timeListheader = "Registered at the following UTC time:";
				AssertContains($@"The email should contain the header for occurrences. Actual email body:
					{lastSentEmail.Body}", timeListheader, lastSentEmail.Body);

				var timeList = new ZString(lastSentEmail.Body).SubstringSafe(lastSentEmail.Body.IndexOf(timeListheader) + timeListheader.Length + 2);
				AssertEquals($@"The email should list all occurrences. Actual email body:
					{lastSentEmail.Body}", expectedOccurrencesCount, timeList.Split(System.Environment.NewLine.ToArray()).Where(l => l.Length > 1).Count());
			});
		}

		void AssertEmailNotSent(string errorMessage)
		{
			AssertEquals(errorMessage, 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#endregion
	}
}

