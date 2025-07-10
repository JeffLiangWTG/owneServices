using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Data;
using Enterprise.MailManager;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.MailProcessor.Testing
{
	[TestedType(typeof(MailMaintananceTask))]
	internal class MailMaintananceTaskTest : ServiceTaskTestCase<MailMaintananceTask>
	{
		public void TestWhileLoopProcessesMultipleBatches()
		{
			Db.Connection.ExecuteNonQuery(@"
	insert into dbo.MailDbItems (MI_PK, MI_Direction, MI_Status, MI_SystemCreateTimeUTC, MI_ReceivedDateTime, MI_SystemLastEditTimeUtc, MI_SystemCreateUser, MI_SystemLastEditUser)
select top 1001 newid(), 'TRX', 'SNT', '2018-01-01', '2018-01-01', '2018-01-01', 'E', 'E' from dbo.StmNumberSequence
");
			var task = new MailMaintananceTask();
			var logger = new TestServiceLogger();
			task.ServiceLogger = logger;
			task.RunTask();

			AssertEquals("Information|1 sent message(s) purged.", logger[14]);
			AssertEquals(0, Db.Connection.ExecuteScalar<int>("select count(*) from dbo.MailDbItems where MI_Status = 'SNT' and MI_SystemCreateTimeUTC = '2018-01-01'"));
		}

		[TestDate(2008, 3, 8, 23, 30, 0)]
		public void TestDoDailyCleanups()
		{
			var databaseEmailManager = new Mock<DatabaseEmailManagement>(MockBehavior.Strict);

			databaseEmailManager
			.SetupSequence(x => x.PurgeProcessedIncomingEmailOlderThan(SystemDataRegistry.Instance.PurgeIncomingProcessedEmailsOlderThan.Value))
			.Returns(3)
			.Returns(0);

			databaseEmailManager
			.SetupSequence(x => x.PurgeIncomingEmailOlderThan(SystemDataRegistry.Instance.PurgeIncomingUnProcessedEmailsOlderThan.Value))
			.Returns(5)
			.Returns(0);

			databaseEmailManager
			.SetupSequence(x => x.PurgeProcessedOutgoingEmailOlderThan(SystemDataRegistry.Instance.PurgeOutgoingEmailsOlderThan.Value))
			.Returns(1)
			.Returns(0);

			databaseEmailManager
			.SetupSequence(x => x.PurgeUnsentOutgoingEmailOlderThan(SystemDataRegistry.Instance.PurgeUnsentOutgoingEmailsOlderThan.Value))
			.Returns(2)
			.Returns(0);

			var mailMaintananceTask = new MailMaintananceTaskForTesting(false);
			DatabaseEmailManagement.OverridableInstance.Value = databaseEmailManager.Object;
			var log = InitialiseAndRunTaskSchedule(mailMaintananceTask);
			AssertEquals(18, log.Count);
			AssertEquals("Information|" + BuildProcessText("Start"), log[0]);
			AssertEquals("Information|Checking for old received and processed messages to purge...", log[1]);
			AssertEquals("Information|3 processed message(s) purged.", log[2]);
			AssertEquals("Information|Checking for old unprocessed messages to purge...", log[3]);
			AssertEquals("Information|5 unprocessed message(s) purged.", log[4]);
			AssertEquals("Information|Checking for old sent messages to purge...", log[5]);
			AssertEquals("Information|1 sent message(s) purged.", log[6]);
			AssertEquals("Information|Checking for old unsent messages to purge...", log[7]);
			AssertEquals("Information|2 unsent message(s) purged.", log[8]);
			AssertEquals("Information|Checking for old received and processed messages to purge...", log[9]);
			AssertEquals("Information|None found.", log[10]);
			AssertEquals("Information|Checking for old unprocessed messages to purge...", log[11]);
			AssertEquals("Information|None found.", log[12]);
			AssertEquals("Information|Checking for old sent messages to purge...", log[13]);
			AssertEquals("Information|None found.", log[14]);
			AssertEquals("Information|Checking for old unsent messages to purge...", log[15]);
			AssertEquals("Information|None found.", log[16]);
			AssertEquals("Information|" + BuildProcessText("End"), log[17]);
		}

		[TestDate(2008, 3, 8, 23, 30, 0)]
		public void TestDoDailyCleanupsWithSupportSendingEmailWithAcknowledgement()
		{
			var databaseEmailManager = new Mock<DatabaseEmailManagement>(MockBehavior.Strict);
			DatabaseEmailManagement.OverridableInstance.Value = databaseEmailManager.Object;

			databaseEmailManager
			.SetupSequence(x => x.PurgeProcessedIncomingEmailOlderThan(SystemDataRegistry.Instance.PurgeIncomingProcessedEmailsOlderThan.Value))
			.Returns(3)
			.Returns(0);

			databaseEmailManager
			.SetupSequence(x => x.PurgeIncomingEmailOlderThan(SystemDataRegistry.Instance.PurgeIncomingUnProcessedEmailsOlderThan.Value))
			.Returns(5)
			.Returns(0);

			databaseEmailManager
			.SetupSequence(x => x.PurgeProcessedOutgoingEmailOlderThan(SystemDataRegistry.Instance.PurgeOutgoingEmailsOlderThan.Value))
			.Returns(1)
			.Returns(0);

			databaseEmailManager
			.SetupSequence(x => x.PurgeUnsentOutgoingEmailOlderThan(SystemDataRegistry.Instance.PurgeUnsentOutgoingEmailsOlderThan.Value))
			.Returns(2)
			.Returns(0);

			databaseEmailManager
			.SetupSequence(x => x.TruncateBodyOutgoingUpgradeEmailOlderThan(3))
			.Returns(2)
			.Returns(0);

			databaseEmailManager
			.SetupSequence(x => x.SetToFailedQueuedWithAckOutgoingEmailOlderThan(2))
			.Returns(8)
			.Returns(0);

			var mailMaintananceTask = new MailMaintananceTaskForTesting(true);
			DatabaseEmailManagement.OverridableInstance.Value = databaseEmailManager.Object;
			var log = InitialiseAndRunTaskSchedule(mailMaintananceTask);

			AssertEquals(26, log.Count);
			AssertEquals("Information|" + BuildProcessText("Start"), log[0]);
			AssertEquals("Information|Checking for old received and processed messages to purge...", log[1]);
			AssertEquals("Information|3 processed message(s) purged.", log[2]);
			AssertEquals("Information|Checking for old unprocessed messages to purge...", log[3]);
			AssertEquals("Information|5 unprocessed message(s) purged.", log[4]);
			AssertEquals("Information|Checking for old sent messages to purge...", log[5]);
			AssertEquals("Information|1 sent message(s) purged.", log[6]);
			AssertEquals("Information|Checking for old unsent messages to purge...", log[7]);
			AssertEquals("Information|2 unsent message(s) purged.", log[8]);
			AssertEquals("Information|Checking for old upgrade messages to truncate message body...", log[9]);
			AssertEquals("Information|2 message(s) truncated.", log[10]);
			AssertEquals("Information|Checking for old queued with acknowledgement messages to mark as failed...", log[11]);
			AssertEquals("Information|8 message(s) marked as failed.", log[12]);
			AssertEquals("Information|" + BuildProcessText("End"), log[25]);
		}

		public void TestMinimumPeriod()
		{
			var attributes = typeof(MailMaintananceTask).Assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false);
			var attribute = Array.Find((HostedServiceAttribute[])attributes, a => a.TypeName == typeof(MailMaintananceTask).FullName);
			AssertEquals("Minimum Period should be 15 minutes", "15minutes", attribute.MinimumPeriod);
		}

		static string BuildProcessText(string message)
		{
			return message + " process " + Process.GetCurrentProcess().Id;
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		class MailMaintananceTaskForTesting : MailMaintananceTask
		{
			public MailMaintananceTaskForTesting(bool supportSendingEmailWithAcknowledgement)
			{
				this.supportSendingEmailWithAcknowledgement = supportSendingEmailWithAcknowledgement;
			}

			protected override bool SupportSendingEmailWithAcknowledgement
			{
				get { return supportSendingEmailWithAcknowledgement; }
			}

			readonly bool supportSendingEmailWithAcknowledgement;
		}
	}
}
