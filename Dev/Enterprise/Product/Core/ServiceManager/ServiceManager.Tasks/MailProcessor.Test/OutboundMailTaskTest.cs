using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.ExternalMailInterface.Testing;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.MailProcessor.Testing
{
	[TestedType(typeof(OutboundMailTask))]
	class OutboundMailTaskTest : ServiceTaskTestCase<OutboundMailTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "OMS", hostedServiceAttribute.Code);
				AssertEquals("Description", "Outbound Mail Service", hostedServiceAttribute.Description);
				AssertEquals("Category", "MAI", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", true, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("MaximumPeriod", "1hour", hostedServiceAttribute.MaximumPeriod);
				AssertEquals(typeof(OMSSpecificValidation), hostedServiceAttribute.TaskSpecificValidationType);
			});
		}

		[TestDate(2024, 1, 1, 0, 0, 0)]
		public void TestRunTask_SMTPServerMustNotBeEmpty_WhenNeitherGraphAPIIsEnabledNorRunOMSInSimulationModeIsEnabled()
		{
			var serviceManagerGovernorMock = new Mock<IServiceManagerGovernor>();
			var serviceManagerQuerierMock = new Mock<IServiceManagerQuerier>();

			serviceManagerQuerierMock
				.Setup(querier => querier.CheckStateOfNamedServiceTask(It.IsAny<string>()))
				.Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (ObjectFactory.Substitute(serviceManagerGovernorMock.Object))
			using (ObjectFactory.Substitute(serviceManagerQuerierMock.Object))
			using (Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SystemDataRegistry.Instance.RunOMSInSimulationMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var schedule = Factory.New<ServiceTaskSchedule>();
				schedule.S5_ParentTableCode = StmServiceHostSchema.Constants.Prefix;
				schedule.S5_ScheduleType = "OMS";
				schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
				Factory.Save();

				var outboundMailTask = new OutboundMailTaskForTesting(false);
				InitialiseTaskSchedule(outboundMailTask);
				RunTaskSchedule(outboundMailTask);

				AssertEquals($@"Error|As a result of incomplete mail configurations in the Registry, the Outbound Mail Service Task has been paused and has been rescheduled to run again on 08-Jan-24 00:00:00.
Please verify the mail configuration settings in the Registry at Registry -> {RawDataRegistry.Instance.SMTPServer.GetLocation()}.
Once the mail configuration has been set, the Service Task will automatically resume running after {RegistryRefresh.FrequencyInSeconds} seconds.
", outboundMailTask.ServiceLogger.ToString());

				serviceManagerGovernorMock.Verify(governor => governor.SetServiceTaskNextRuntime("OMS", new DateTimeOffset(2024, 1, 8, 0, 0, 0, TimeSpan.Zero)));
			}
		}

		public void TestRunTask_NoCheckOnSMTPServer_WhenGraphAPIIsEnabledOrRunOMSInSimulationModeIsEnabled()
		{
			var outboundMailTask = new OutboundMailTaskForTesting(false);
			InitialiseTaskSchedule(outboundMailTask);

			using (Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				using (Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (SystemDataRegistry.Instance.RunOMSInSimulationMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					RunTaskSchedule(outboundMailTask);

					AssertEndsWith("Service task should be finished successfully", "Information|Outbound Mail Service task finished.\r\n", outboundMailTask.ServiceLogger.ToString());
				}

				using (Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (SystemDataRegistry.Instance.RunOMSInSimulationMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					RunTaskSchedule(outboundMailTask);

					AssertEndsWith("Service task should be finished successfully", "Information|Outbound Mail Service task finished.\r\n", outboundMailTask.ServiceLogger.ToString());
				}
			}
		}

		public void TestSendEmail()
		{
			CreateEmail("Test1", "", "a1@bbb.ccc");
			CreateEmail("Test2", "", "a2@bbb.ccc");
			CreateEmail("Test3", "", "a3@bbb.ccc");

			var outboundMailTask = new OutboundMailTaskForTesting(false);
			var log = InitialiseAndRunTaskSchedule(outboundMailTask);

			AssertEquals(@"Information|3 email(s) sent From: hank Server: testHost. Total sent in this batch - 3
  To:a1@bbb.ccc Subject:Test1
  To:a2@bbb.ccc Subject:Test2
  To:a3@bbb.ccc Subject:Test3", log[1]);
		}

		public void TestGetQueuedSendableMailQueryShouldApplyMaxBatchSize()
		{
			CreateEmail("Test1", "", "a1@bbb.ccc");
			CreateEmail("Test2", "", "a2@bbb.ccc");
			CreateEmail("Test3", "", "a3@bbb.ccc");
			DataRegistry.Instance.MaximumNumberOfMailItemsToSendInABatch = 2;

			var outboundMailTask = new OutboundMailTaskForTesting(false);
			var outgoingMailQueue = new DbOnlyBusinessObjectQueue<MailItem>(outboundMailTask.GetQueuedSendableMailQuery());
			var executionCount = 0;
			outgoingMailQueue.Process((_, _) =>
			{
				executionCount++;
			});

			AssertEquals("The number of mails in the queue", 3, executionCount);
		}

		public void TestGetQueuedSendableMailQueryShouldDelayQueueWithLowPriority()
		{
			CreateEmail("Test1", "", true, "a1@bbb.ccc");
			CreateEmail("Test2", "", "a2@bbb.ccc");
			CreateEmail("Test3", "", "a3@bbb.ccc");

			var outboundMailTask = new OutboundMailTaskForTesting(false);
			var outgoingMailQueue = new DbOnlyBusinessObjectQueue<MailItem>(outboundMailTask.GetQueuedSendableMailQuery());
			var executionSequence = new List<string>();
			outgoingMailQueue.Process((mailPK, _) =>
			{
				var mail = Factory.Load<MailItem>(mailPK);
				executionSequence.Add(mail.MI_Subject);
			});

			AssertEquals("The number of mails in the queue", 3, executionSequence.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Execution Sequence", "Test2", executionSequence[0]);
				AssertEquals("Execution Sequence", "Test3", executionSequence[1]);
				AssertEquals("Execution Sequence", "Test1", executionSequence[2]);
			});
		}

		public void TestSendingEmailWithAcknowledgementShouldDelayQueueWithLowPriority()
		{
			CreateEmailWithAck("Test1", "", true, "a1@bbb.ccc");
			CreateEmailWithAck("Test2", "", "a2@bbb.ccc");
			CreateEmailWithAck("Test3", "", "a3@bbb.ccc");
			var outboundMailTask = new OutboundMailTaskForTesting(supportSendingEmailWithAcknowledgement: true);
			var log = InitialiseAndRunTaskSchedule(outboundMailTask);

			var expectLog = @"Information|3 email(s) with acknowledgment sent From: hank Server: testHost. Total sent in this batch - 3
  To:a2@bbb.ccc Subject:Test2
  To:a3@bbb.ccc Subject:Test3
  To:a1@bbb.ccc Subject:Test1";
			AssertEquals(expectLog, log[1]);
			AssertEquals("Information|Outbound Mail Service task finished.", log[2]);
		}

		public void TestSendEmailWithInvalidAddress()
		{
			CreateEmail("Test1", "", "");
			CreateEmail("Test2", "", "Test2@Test2.com");
			CreateEmail("Test3", "", "Test3@Test3");

			var outboundMailTask1 = new OutboundMailTaskForTesting(false);
			var log1 = InitialiseAndRunTaskSchedule(outboundMailTask1).ToString();

			AssertContains("Log should contain detailed info about the error sending one email.", "Error|Enterprise.MailManager.ExternalMailInterface.FailedToSendMessageException: Failed to send email with subject Test1 because one or more recipients have empty or invalid email address", log1);
			AssertContains("Log should contain detailed info about the error sending one email.", "Error|Enterprise.MailManager.ExternalMailInterface.FailedToSendMessageException: Failed to send email with subject Test3 because one or more recipients have empty or invalid email address", log1);
			AssertContains("Information|1 email(s) sent From: hank Server: testHost. Total sent in this batch - 1", log1);

			CreateEmailWithAck("Test1", "", "");
			CreateEmailWithAck("Test2", "", "a1@bbb.ccc");
			CreateEmailWithAck("Test3", "", "a1@bbb");

			var outboundMailTask2 = new OutboundMailTaskForTesting(true);
			var log2 = InitialiseAndRunTaskSchedule(outboundMailTask2).ToString();

			AssertContains("Log should contain detailed info about the error sending one email with subject Test1.", "Error|Enterprise.MailManager.ExternalMailInterface.FailedToSendMessageException: Failed to send email with subject Test1 because one or more recipients have empty or invalid email address", log2);
			AssertContains("Log should contain detailed info about the error sending one email with subject Test3.", "Error|Enterprise.MailManager.ExternalMailInterface.FailedToSendMessageException: Failed to send email with subject Test3 because one or more recipients have empty or invalid email address", log2);
			AssertContains("Information|1 email(s) with acknowledgment sent From: hank Server: testHost. Total sent in this batch - 1", log2);
		}

		public void TestSendEmailInSimulationMode()
		{
			using (SystemDataRegistry.Instance.RunOMSInSimulationMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateEmail("Test1", "", "a1@bbb.ccc");
				CreateEmail("Test2", "", "a2@bbb.ccc");
				CreateEmail("Test3", "", "a3@bbb.ccc");

				var query = new ZQuery { OrderBy = "MI_SystemCreateTimeUtc DESC" };
				var mailItems = Factory.Load<MailItem>(query).Take(3).ToArray();

				foreach (var email in mailItems)
				{
					AssertEquals(MailStatus.Queued, email.MI_Status);
				}

				var outboundMailTask = new OutboundMailTaskForTesting(true);
				var log = InitialiseAndRunTaskSchedule(outboundMailTask);

				AssertEquals("Information|3 email(s) marked as sent as per simulation mode.", log[1]);

				var newFactory = new BusinessObjectFactory();

				foreach (var email in mailItems)
				{
					AssertEquals(MailStatus.Sent, newFactory.Load<MailItem>(email.PK).MI_Status);
				}
			}
		}

		public void TestSendEmailMoreThan10()
		{
			DataRegistry.Instance.MaximumNumberOfMailItemsToSendInABatch = 10;

			CreateEmail("Test01", "", "a1@bbb.ccc");
			CreateEmail("Test02", "", "a2@bbb.ccc");
			CreateEmail("Test03", "", "a3@bbb.ccc");
			CreateEmail("Test04", "", "a4@bbb.ccc");
			CreateEmail("Test05", "", "a5@bbb.ccc");
			CreateEmail("Test06", "", "a1@bbb.ccc");
			CreateEmail("Test07", "", "a2@bbb.ccc");
			CreateEmail("Test08", "", "a3@bbb.ccc");
			CreateEmail("Test09", "", "a4@bbb.ccc");
			CreateEmail("Test10", "", "a5@bbb.ccc");
			CreateEmail("Test11", "", "a1@bbb.ccc");
			CreateEmail("Test12", "", "a2@bbb.ccc");
			CreateEmail("Test13", "", "a3@bbb.ccc");
			CreateEmail("Test14", "", "a4@bbb.ccc");
			CreateEmail("Test15", "", "a5@bbb.ccc");

			var outboundMailTask = new OutboundMailTaskForTesting(false);
			var log = InitialiseAndRunTaskSchedule(outboundMailTask);

			AssertStartsWith("", "Information|10 email(s) sent From: hank Server: testHost. Total sent in this batch - 10", log[1]);
			AssertStartsWith("", "Information|5 email(s) sent From: hank Server: testHost. Total sent in this batch - 15", log[2]);
			AssertEquals("Information|Outbound Mail Service task finished.", log[3]);
		}

		public void TestSendEmailWithAck()
		{
			DataRegistry.Instance.MaximumNumberOfMailItemsToSendInABatch = 10;

			CreateEmailWithAck("Test01", "", "a1@bbb.ccc");
			CreateEmailWithAck("Test02", "", "a2@bbb.ccc");
			CreateEmailWithAck("Test03", "", "a3@bbb.ccc");
			CreateEmailWithAck("Test04", "", "a4@bbb.ccc");
			CreateEmailWithAck("Test05", "", "a5@bbb.ccc");
			CreateEmailWithAck("Test06", "", "a1@bbb.ccc");
			CreateEmailWithAck("Test07", "", "a2@bbb.ccc");
			CreateEmailWithAck("Test08", "", "a3@bbb.ccc");
			CreateEmailWithAck("Test09", "", "a4@bbb.ccc");
			CreateEmailWithAck("Test10", "", "a5@bbb.ccc");
			CreateEmailWithAck("Test11", "", "a1@bbb.ccc");
			CreateEmailWithAck("Test12", "", "a2@bbb.ccc");
			CreateEmailWithAck("Test13", "", "a3@bbb.ccc");
			CreateEmailWithAck("Test14", "", "a4@bbb.ccc");
			CreateEmailWithAck("Test15", "", "a5@bbb.ccc");

			var outboundMailTask = new OutboundMailTaskForTesting(supportSendingEmailWithAcknowledgement: true);
			var log = InitialiseAndRunTaskSchedule(outboundMailTask);

			AssertStartsWith("", "Information|10 email(s) with acknowledgment sent From: hank Server: testHost. Total sent in this batch - 10", log[1]);
			AssertStartsWith("", "Information|5 email(s) with acknowledgment sent From: hank Server: testHost. Total sent in this batch - 15", log[2]);
			AssertEquals("Information|Outbound Mail Service task finished.", log[3]);
		}

		public void TestSendEmailMixedWithAckAndWithout()
		{
			CreateEmail("Test01", "", "a1@bbb.ccc");
			CreateEmailWithAck("Test02", "", "a2@bbb.ccc");
			CreateEmail("Test03", "", "a3@bbb.ccc");
			CreateEmailWithAck("Test04", "", "a4@bbb.ccc");
			CreateEmail("Test05", "", "a5@bbb.ccc");
			CreateEmailWithAck("Test06", "", "a1@bbb.ccc");
			CreateEmail("Test07", "", "a2@bbb.ccc");
			CreateEmailWithAck("Test08", "", "a3@bbb.ccc");
			CreateEmail("Test09", "", "a4@bbb.ccc");
			CreateEmailWithAck("Test10", "", "a5@bbb.ccc");
			CreateEmail("Test11", "", "a1@bbb.ccc");
			CreateEmailWithAck("Test12", "", "a2@bbb.ccc");
			CreateEmail("Test13", "", "a3@bbb.ccc");
			CreateEmailWithAck("Test14", "", "a4@bbb.ccc");
			CreateEmail("Test15", "", "a5@bbb.ccc");

			var outboundMailTask = new OutboundMailTaskForTesting(supportSendingEmailWithAcknowledgement: true);
			var log = InitialiseAndRunTaskSchedule(outboundMailTask);

			AssertStartsWith("", "Information|8 email(s) sent From: hank Server: testHost. Total sent in this batch - 8", log[1]);
			AssertStartsWith("", "Information|7 email(s) with acknowledgment sent From: hank Server: testHost. Total sent in this batch - 15", log[2]);
			AssertEquals("Information|Outbound Mail Service task finished.", log[3]);
		}

		public void TestSendEmailUsesMaxNumberOfMailItemsInBatchRegistry()
		{
			DataRegistry.Instance.MaximumNumberOfMailItemsToSendInABatch = 3;
			CreateEmail("Test01", "", "a1@bbb.ccc");
			CreateEmail("Test02", "", "a2@bbb.ccc");
			CreateEmail("Test03", "", "a3@bbb.ccc");
			CreateEmail("Test04", "", "a4@bbb.ccc");
			CreateEmail("Test05", "", "a5@bbb.ccc");
			CreateEmail("Test06", "", "a1@bbb.ccc");
			CreateEmail("Test07", "", "a2@bbb.ccc");
			CreateEmail("Test08", "", "a3@bbb.ccc");
			CreateEmail("Test09", "", "a4@bbb.ccc");
			CreateEmail("Test10", "", "a5@bbb.ccc");
			CreateEmail("Test11", "", "a1@bbb.ccc");
			CreateEmail("Test12", "", "a2@bbb.ccc");
			CreateEmail("Test13", "", "a3@bbb.ccc");
			CreateEmail("Test14", "", "a4@bbb.ccc");
			CreateEmail("Test15", "", "a5@bbb.ccc");
			var outboundMailTask = new OutboundMailTaskForTesting(false);
			var log = InitialiseAndRunTaskSchedule(outboundMailTask);
			AssertStartsWith("", "Information|3 email(s) sent From: hank Server: testHost. Total sent in this batch - 3", log[1]);
			AssertEquals("Information|Outbound Mail Service task finished.", log[log.Count - 1]);

			DataRegistry.Instance.MaximumNumberOfMailItemsToSendInABatch = 15;
			CreateEmail("Test01", "", "a1@bbb.ccc");
			CreateEmail("Test02", "", "a2@bbb.ccc");
			CreateEmail("Test03", "", "a3@bbb.ccc");
			CreateEmail("Test04", "", "a4@bbb.ccc");
			CreateEmail("Test05", "", "a5@bbb.ccc");
			CreateEmail("Test06", "", "a1@bbb.ccc");
			CreateEmail("Test07", "", "a2@bbb.ccc");
			CreateEmail("Test08", "", "a3@bbb.ccc");
			CreateEmail("Test09", "", "a4@bbb.ccc");
			CreateEmail("Test10", "", "a5@bbb.ccc");
			CreateEmail("Test11", "", "a1@bbb.ccc");
			CreateEmail("Test12", "", "a2@bbb.ccc");
			CreateEmail("Test13", "", "a3@bbb.ccc");
			CreateEmail("Test14", "", "a4@bbb.ccc");
			CreateEmail("Test15", "", "a5@bbb.ccc");
			outboundMailTask = new OutboundMailTaskForTesting(false);
			log = InitialiseAndRunTaskSchedule(outboundMailTask);
			AssertStartsWith("", "Information|15 email(s) sent From: hank Server: testHost. Total sent in this batch - 15", log[1]);
		}

		public void TestSendEmailThrowingAnSqlLockLostException()
		{
			CreateEmail("Test1", "", "a1@bbb.ccc");
			CreateEmail("Test2", "", "a1@bbb.ccc");
			CreateEmail("Test3", "", "a1@bbb.ccc");
			CreateEmail("Test4", "", "a1@bbb.ccc");
			CreateEmail("Test5", "", "a1@bbb.ccc");

			sent += (_, e) =>
			{
				if (e.Item.MI_Subject == "Test3")
				{
					throw new SqlLockLostException();
				}
			};

			var outboundMailTask = new OutboundMailTaskForTesting(false);
			var log = InitialiseAndRunTaskSchedule(outboundMailTask);

			const string expectedValue =
@"Information|Outbound Mail Service task started.
Warning|One mail item was not sent because of repeated connection issues. It has been removed from this list and will be retried in another run.
Information|4 email(s) sent From: hank Server: testHost. Total sent in this batch - 4
  To:a1@bbb.ccc Subject:Test1
  To:a1@bbb.ccc Subject:Test2
  To:a1@bbb.ccc Subject:Test4
  To:a1@bbb.ccc Subject:Test5
Warning|One mail item was not sent because of repeated connection issues. It has been removed from this list and will be retried in another run.
Information|Outbound Mail Service task finished.";

			AssertMultilineASCIIEquals("Expected Log", expectedValue, log.ToString());
		}

		public void TestSendEmailWithUnhandledException()
		{
			CreateEmail("Test1", "", "a1@bbb.ccc");
			CreateEmail("Test2", "", "a2@bbb.ccc");
			CreateEmail("Test3", "", "a3@bbb.ccc");
			CreateEmail("Test4", "", "a4@bbb.ccc");

			sent += (_, _) => throw new Exception("Fail!");
			var outboundMailTask = new OutboundMailTaskForTesting(false);
			var log = InitialiseTaskSchedule(outboundMailTask);

			HostedServiceException exceptionThrown = null;
			try
			{
				RunTaskSchedule(outboundMailTask);
			}
			catch (HostedServiceException ex)
			{
				exceptionThrown = ex;
			}
			AssertNotNull(exceptionThrown);

			AssertEquals(log.ToString(), 7, log.Count);
			AssertStartsWith("", "Error|System.Exception: Fail!", log[1]);
			AssertStartsWith("", "Error|System.Exception: Fail!", log[2]);
			AssertStartsWith("", "Error|System.Exception: Fail!", log[3]);
			AssertStartsWith("", "Error|System.Exception: Fail!", log[4]);
			AssertContains("System.Exception: Fail!", exceptionThrown.ToString());
		}

		public void TestSendEmailWithUnhandledException2()
		{
			CreateEmail("Test1", "", "a1@bbb.ccc");

			sent += (_, _) => throw new FailedToConnectException("Connection problem!");
			var outboundMailTask = new OutboundMailTaskForTesting(false);
			InitialiseTaskSchedule(outboundMailTask);
			HostedServiceException exceptionThrown = null;

			try
			{
				RunTaskSchedule(outboundMailTask);
			}
			catch (HostedServiceException ex)
			{
				exceptionThrown = ex;
			}

			AssertNotNull(exceptionThrown);
			AssertStartsWith("", "Failed to connect to the SMTP server.\r\n\r\nConnection problem!", exceptionThrown.Message);
		}

		public void TestSendEmailWithHandledException()
		{
			var configBak = mailSender.Configuration;

			using (new DisposableAction(
				() => mailSender.Configuration = new SmtpConfiguration("", 0, "", ""),
				() => mailSender.Configuration = configBak))
			{
				CreateEmail("Test1", "", "a1@bbb.ccc");
				CreateEmail("Test2", "", "a2@bbb.ccc");
				CreateEmail("Test3", "", "a3@bbb.ccc");
				CreateEmail("Test4", "", "a4@bbb.ccc");

				sent += (_, e) =>
				{
					if (e.Item.MI_Subject == "Test3")
					{
						throw new FailedToSendMessageException("Handled!");
					}
				};
				var outboundMailTask = new OutboundMailTaskForTesting(false);
				var log = InitialiseTaskSchedule(outboundMailTask);
				var exceptionThrown = false;
				try
				{
					RunTaskSchedule(outboundMailTask);
				}
				catch (HostedServiceException)
				{
					exceptionThrown = true;
				}
				Assert(!exceptionThrown);

				Assert("Log should contain detailed info about the error sending one email.", log[1].StartsWith("Error|Enterprise.MailManager.ExternalMailInterface.FailedToSendMessageException: Handled!"));
				AssertStartsWith("", "Information|3 email(s) sent From: hank Server: . Total sent in this batch - 3", log[2]);
				AssertEquals(@"Warning|1 email(s) failed to be sent From: hank Server: . Total sent in this batch - 3
  To:a3@bbb.ccc Subject:Test3", log[3]);
			}
		}

		public void TestSendEmailWithSmtpConfigurationException()
		{
			CreateEmail("Test1", "", "a1@bbb.ccc");
			sent += (_, _) => throw new SmtpConfigurationException("Bad Config");

			var outboundMailTask = new OutboundMailTaskForTesting(false);
			InitialiseTaskSchedule(outboundMailTask);
			HostedServiceException exceptionThrown = null;

			try
			{
				RunTaskSchedule(outboundMailTask);
			}
			catch (HostedServiceException ex)
			{
				exceptionThrown = ex;
			}

			AssertNotNull("Expected a HostedServiceException to be thrown for a for SmtpConfigurationException", exceptionThrown);
			AssertContains("There is a configuration error with your SMTP server: Bad Config", exceptionThrown.Message);
			AssertContains("This is not a system defect, this is a configuration issue. Please contact your SMTP administrator to check the Security SMTP settings in the System Registry", exceptionThrown.Message);
			AssertContains("Please refer to our eLearning Portal -> FAQs -> 'We cannot email from our system, what should we do?' for more information before contacting CW1 Support.", exceptionThrown.Message);
		}

		public void TestSendEmailWithSupportSendingEmailWithAcknowledgement()
		{
			DisableExistingMail();

			var databaseEmailManager = new Mock<DatabaseEmailManagement>(MockBehavior.Strict);
			DatabaseEmailManagement.OverridableInstance.Value = databaseEmailManager.Object;

			databaseEmailManager.Setup(d => d.StopUnsuccessfulUpgradeSending()).Returns(4);

			var outboundMailTask = new OutboundMailTaskForTesting(true);
			var log = InitialiseAndRunTaskSchedule(outboundMailTask);

			AssertEquals("Warning|Stopped unsuccessful sending of 4 upgrade(s).", log[1]);
		}

		public void TestIContinuousServiceTask()
		{
			DataRegistry.Instance.MaximumNumberOfMailItemsToSendInABatch = 10;

			CreateEmail("Test01", "", "a1@bbb.ccc");
			CreateEmail("Test02", "", "a2@bbb.ccc");
			CreateEmail("Test03", "", "a3@bbb.ccc");
			CreateEmail("Test04", "", "a4@bbb.ccc");
			CreateEmail("Test05", "", "a5@bbb.ccc");
			CreateEmail("Test06", "", "a1@bbb.ccc");
			CreateEmail("Test07", "", "a2@bbb.ccc");
			CreateEmail("Test08", "", "a3@bbb.ccc");
			CreateEmail("Test09", "", "a4@bbb.ccc");
			CreateEmail("Test10", "", "a5@bbb.ccc");
			CreateEmail("Test11", "", "a1@bbb.ccc");
			CreateEmail("Test12", "", "a2@bbb.ccc");
			CreateEmail("Test13", "", "a3@bbb.ccc");
			CreateEmail("Test14", "", "a4@bbb.ccc");
			CreateEmail("Test15", "", "a5@bbb.ccc");

			var outboundMailTask = new OutboundMailTaskForTesting(false);
			sent += (_, e) =>
			{
				if (e.Item.MI_Subject == "Test04")
				{
					outboundMailTask.CancellationTokenSource.Cancel();
				}
			};

			var log = InitialiseAndRunTaskSchedule(outboundMailTask);

			AssertEquals(3, log.Count);
			AssertStartsWith("", "Information|10 email(s) sent From: hank Server: testHost. Total sent in this batch - 10", log[1]);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new[]
				{
					new (
						MailDBItemsSchema.Constants.TableName,
						"Outbound Mail",
						MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Transmit,
						MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued),

					new TaskNudgeInformationForTest(
						MailDBItemsSchema.Constants.TableName,
						"Outbound Mail with acknowledgement",
						MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Transmit,
						MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.QueuedWithAck),
				};
			}
		}

		public void TestConcurrencyError()
		{
			DisableExistingMail();
			var email1 = CreateEmail("Test1", "", MailStatus.Queued, false, "a1@bbb.ccc");
			var email2 = CreateEmail("Test1", "", MailStatus.Queued, false, "a1@bbb.ccc");
			var mailItems = new[] { email1, email2 };

			var mailTaskForTestingConcurrencyError = new OutboundMailTaskForTestingConcurrencyError(true) { ServiceLogger = new TestServiceLogger() };
			var totalSent = 0;
			var sender = new MailSender();
			AssertNoExceptionThrown(() => mailTaskForTestingConcurrencyError.SendEmailForTest(sender, mailItems, false, ref totalSent));
			AssertEquals("The email status should be SNT",email1.MI_Status,MailStatus.Sent);
		}

		public void TestConcurrencyError_MailDBRecipients()
		{
			DisableExistingMail();
			var email1 = CreateEmail("Test1", "", MailStatus.Queued, false, "a1@bbb.ccc");
			var email2 = CreateEmail("Test1", "", MailStatus.Queued, false, "a1@bbb.ccc");
			var mailItems = new[] { email1, email2 };

			var mailTaskForTestingConcurrencyError = new OutboundMailTaskForTestingConcurrencyErrorWhenSavingMailDBRecipients(true) { ServiceLogger = new TestServiceLogger() };
			var totalSent = 0;
			var sender = new MailSender();
			AssertNoExceptionThrown(() => mailTaskForTestingConcurrencyError.SendEmailForTest(sender, mailItems, false, ref totalSent));
			AssertEquals("The email status should be SNT", email2.MI_Status, MailStatus.Sent);
			AssertNotContains("Information While you have been working with this form, another user has made changes.", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		[ExpectNoExceptions]
		public void TestProcessUnhandledFailuresIfMailDeleted()
		{
			var email1 = CreateEmail("Test1", "", MailStatus.Queued, false, "a1@bbb.ccc");
			var email2 = CreateEmail("Test1", "", MailStatus.Queued, false, "a1@bbb.ccc");
			var outboundMailTask = new OutboundMailTaskForTestingConcurrencyError(false) { ServiceLogger = new TestServiceLogger() };
			var emailsSentByServer = new List<MailSender.EmailSentResult>();
			emailsSentByServer.Add(new MailSender.EmailSentResult(email1, null, true));
			emailsSentByServer.Add(new MailSender.EmailSentResult(email2, null, true));
			var mailItems = new[] { email1, email2 };
			var totalSent = 0;
			email2.Delete();
			outboundMailTask.ProcessSendResults_exposed(new List<(MailItem mi, Exception ex)>(), mailItems, emailsSentByServer, true, ref totalSent);
			AssertContains("The Email has been deleted", outboundMailTask.ServiceLogger.ToString().Trim());
		}

		class OutboundMailTaskForTestingConcurrencyError : OutboundMailTask
		{
			public OutboundMailTaskForTestingConcurrencyError(bool testConcurrencyError)
			{
				this.testConcurrencyError = testConcurrencyError;
			}

			readonly bool testConcurrencyError;

			void TestConcurrencyError()
			{
				var omt = new OutboundMailTaskForTestingConcurrencyError(false)
					{ ServiceLogger = new TestServiceLogger() };
				var senderForTest = new MailSender();
				var totalSent = 0;
				var factory = new BusinessObjectFactory { RefreshEnabled = false };
				var items = factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Status, MailStatus.Queued));
				omt.SendEmailForTest(senderForTest, items, false, ref totalSent);
			}

			public void SendEmailForTest(MailSender mailSender, MailItem[] mailItems, bool withAcknowledgment, ref int totalSent)
			{
				SendEmail(mailSender, mailItems, withAcknowledgment, ref totalSent);
			}

			protected override void ProcessSendResults(List<(MailItem mi, Exception ex)> unhandledFailures, MailItem[] mailItems, List<MailSender.EmailSentResult> emailsSentByServer, bool withAcknowledgment,
				ref int totalSent)
			{
				if (testConcurrencyError)
				{
					unhandledFailures.Add((mailItems[0], new Exception("Test Concurrency Error")));
					mailItems[0].MI_Status = MailStatus.Queued;
					mailItems[0].Factory.Save();
					TestConcurrencyError();
				}
				base.ProcessSendResults(unhandledFailures, mailItems, emailsSentByServer, withAcknowledgment, ref totalSent);
			}

			public void ProcessSendResults_exposed(List<(MailItem mi, Exception ex)> unhandledFailures, MailItem[] mailItems, List<MailSender.EmailSentResult> emailsSentByServer, bool withAcknowledgment,
				ref int totalSent)
			{
				base.ProcessSendResults(unhandledFailures, mailItems, emailsSentByServer, withAcknowledgment, ref totalSent);
			}
		}

		class OutboundMailTaskForTestingConcurrencyErrorWhenSavingMailDBRecipients : OutboundMailTask
		{
			public OutboundMailTaskForTestingConcurrencyErrorWhenSavingMailDBRecipients(bool testConcurrencyError) 
			{
				this.testConcurrencyError = testConcurrencyError;
			}

			readonly bool testConcurrencyError;

			protected override void ProcessSendResults(List<(MailItem mi, Exception ex)> unhandledFailures, MailItem[] mailItems, List<MailSender.EmailSentResult> emailsSentByServer, bool withAcknowledgment,
				ref int totalSent)
			{
				if (testConcurrencyError)
				{
					unhandledFailures.Add((mailItems[1], new Exception("Test Concurrency Error")));
					mailItems[1].MI_Status = MailStatus.Queued;
					var lastAckAttempt = mailItems[1].MailRecipients[0].MR_AckAttempt;
					mailItems[1].MailRecipients[0].MR_AckAttempt = 0;
					mailItems[1].Factory.Save();
					mailItems[1].MailRecipients[0].MR_AckAttempt = lastAckAttempt;
					TestConcurrencyError();
				}
				base.ProcessSendResults(unhandledFailures, mailItems, emailsSentByServer, withAcknowledgment, ref totalSent);
			}
			public void SendEmailForTest(MailSender mailSender, MailItem[] mailItems, bool withAcknowledgment, ref int totalSent)
			{
				SendEmail(mailSender, mailItems, withAcknowledgment, ref totalSent);
			}

			void TestConcurrencyError()
			{
				var omt = new OutboundMailTaskForTestingConcurrencyErrorWhenSavingMailDBRecipients(false)
				{ ServiceLogger = new TestServiceLogger() };
				var senderForTest = new MailSender();
				var totalSent = 0;
				var factory = new BusinessObjectFactory { RefreshEnabled = false };
				var items = factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Status, MailStatus.Queued));
				omt.SendEmailForTest(senderForTest, items, false, ref totalSent);
			}
		}

		MailItem CreateEmail(string subject, string body, string status, bool lowPriority, params string[] recipients)
		{
			if (!existingMailDisabled)
			{
				DisableExistingMail();
				existingMailDisabled = true;
			}

			var item = Factory.New<MailItem>();
			item.MI_From = "a@b.c";
			item.MI_Subject = subject;
			item.MI_Direction = MailDirection.Transmit;

			item.MI_Status = status;
			item.MI_QueueWithLowPriority = lowPriority;

			var utcnow = ZDateTime.UtcNow;
			item.MI_SendDateTime = utcnow;
			item.MI_ReceivedDateTime = utcnow;

			foreach (var recipient in recipients)
			{
				item.AddRecipientForSystemCommunication(recipient);
			}

			item.MI_Body = body;
			Factory.Save();
			return item;
		}

		void CreateEmail(string subject, string body, params string[] recipients)
		{
			CreateEmail(subject, body, MailStatus.Queued, false, recipients);
		}

		void CreateEmail(string subject, string body, bool lowPriority, params string[] recipients)
		{
			CreateEmail(subject, body, MailStatus.Queued, lowPriority, recipients);
		}

		void CreateEmailWithAck(string subject, string body, params string[] recipients)
		{
			CreateEmail(subject, body, MailStatus.QueuedWithAck, false, recipients);
		}

		void CreateEmailWithAck(string subject, string body, bool lowPriority, params string[] recipients)
		{
			CreateEmail(subject, body, MailStatus.QueuedWithAck, lowPriority, recipients);
		}

		bool existingMailDisabled;

		void DisableExistingMail()
		{
			var sqlText = string.Format("UPDATE {0} SET {1} = '{2}' WHERE {3} = '{4}'",
					MailDBItemsSchema.Constants.TableName,
					MailDBItemsSchema.MI_Status.Name, MailStatus.Failed,
					MailDBItemsSchema.MI_Direction.Name, MailDirection.Transmit);
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		class OutboundMailTaskForTesting : OutboundMailTask
		{
			public OutboundMailTaskForTesting(bool supportSendingEmailWithAcknowledgement)
			{
				SupportSendingEmailWithAcknowledgement = supportSendingEmailWithAcknowledgement;
			}

			protected override bool SupportSendingEmailWithAcknowledgement { get; }

			public override void RunTask(CancellationToken token)
			{
				base.RunTask(CancellationTokenSource.Token);
			}

			public CancellationTokenSource CancellationTokenSource { get; } = new ();
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			mailSender = new MailSenderForTesting((mi) =>
			{
				sent?.Invoke(this, new SentEventArgs(mi));
			}, "testHost", "hank");
			mailSenderForTestingHolder = ObjectFactory.Substitute<IMailSender>(mailSender);
		}

		protected override void TearDownCore()
		{
			mailSenderForTestingHolder.Dispose();
			base.TearDownCore();
		}

		IDisposable mailSenderForTestingHolder;
		MailSenderForTesting mailSender;
		EventHandler<SentEventArgs> sent;

		class SentEventArgs : EventArgs
		{
			public SentEventArgs(MailItem item)
			{
				Item = item;
			}

			public MailItem Item { get; }
		}
	}
}
