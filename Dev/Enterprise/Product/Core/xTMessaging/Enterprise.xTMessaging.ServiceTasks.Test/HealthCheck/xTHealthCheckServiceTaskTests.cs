using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.Business.Interfaces;
using Enterprise.eHubMessaging.ServiceTasks.HealthChecks.FailedEDIInterchangeHealthCheck;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.ServiceTasks;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.xTMessaging.Tests.ServiceTasks.HealthCheck
{
	[TestedType(typeof(XTHealthCheckServiceTask))]
	class XTHealthCheckServiceTaskTests : ServiceTaskTestCase<XTHealthCheckServiceTask>
	{
		[TestDate(2024, 01, 01)]
		public void Test_xTHealthCheck_InvalidLastCheckDateTime()
		{
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockXTOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);

			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = DateTime.MinValue;
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });

			Assert("Precondition", !mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);

			mockServiceTask.Object.RunTask();

			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages("First time running Failed EDI Interchange Health Check. Record a starting time.");
			XTHealthCheckTestHelpers.AssertNoErrorEmail();
		}

		[TestDate(2024, 01, 01)]
		public void Test_xTHealthCheck_SuccessJobRun()
		{
			var lastReportedTimePointForEdiFailedInterchanges = (ZDateTime)TestDateAttribute.Date.AddMinutes(-30);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockXTOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);

			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{GlbCompany.CurrentCompany.Branches[0].PK}=5,invalid,{Guid.NewGuid()}=5");
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.FrequencySetting).Returns(HealthCheckConstants.NotificationFrequencyConstants.Periodically);
			mockServiceTaskHealthCheckJob.Setup(m => m.IntervalInMinute).Returns(15);
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			var outgoingInterchange = XTHealthCheckTestHelpers.CreateInterchangeForHealthCheckTests(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, onlyCreateInterchange: true);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange.IsValid);

			mockServiceTask.Object.RunTask();

			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			AssertEquals("LastReportedTimeForFailedEDIInterchange should be updated", TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(
				$"1 failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between {lastReportedTimePointForEdiFailedInterchanges.AddMinutes(15)} and {mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange}.");
		}

		[TestDate(2024, 01, 01)]
		public void Test_xTHealthCheck_UpdatesToMatchIntervalConfiguration()
		{
			var mockServiceManagerGovernor = new Mock<IServiceManagerGovernor>();
			var mockServiceManagerQuerier = new Mock<IServiceManagerQuerier>();
			var previousRunTime = ZDateTime.UtcNow.ToNullableDateTimeOffset();
			mockServiceManagerQuerier.Setup(q => q.TryGetServiceTaskNextRunTime(It.IsAny<string>(), out previousRunTime)).Returns(true);

			using (ObjectFactory.Substitute(mockServiceManagerQuerier.Object))
			using (ObjectFactory.Substitute(mockServiceManagerGovernor.Object))
			{
				CreateDefaultServiceTaskSchedule();
				var mockServiceTask = CreateMockServiceTask();
				var mockServiceTaskHealthCheckJob = CreateMockXTOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
				mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = ZDateTime.Invalid;
				mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = ZDateTime.Invalid;
				mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
				var setting = new NotificationFrequency
				{
					Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
					TimeInterval = 15
				};
				SetValueXTOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);
				mockServiceTask.Object.RunTask();

				AssertNoExceptionThrown(() =>
				{
					mockServiceManagerGovernor.Verify(g => g.SetServiceTaskNextRuntime("XHC", new DateTimeOffset(TestDateAttribute.Date.AddMinutes(15), TimeSpan.Zero)));
					mockServiceTaskHealthCheckJob.VerifyAll();
				});
			}
		}

		[TestDate(2024, 01, 01)]
		public void Test_xTHealthCheck_WhenNoIntervalSpecified()
		{
			var mockServiceManagerGovernor = new Mock<IServiceManagerGovernor>();
			var mockServiceManagerQuerier = new Mock<IServiceManagerQuerier>();
			var previousRunTime = ZDateTime.UtcNow.ToNullableDateTimeOffset();
			mockServiceManagerQuerier.Setup(q => q.TryGetServiceTaskNextRunTime(It.IsAny<string>(), out previousRunTime)).Returns(true);

			using (ObjectFactory.Substitute(mockServiceManagerQuerier.Object))
			using (ObjectFactory.Substitute(mockServiceManagerGovernor.Object))
			{
				CreateDefaultServiceTaskSchedule();
				var mockServiceTask = CreateMockServiceTask();
				var mockServiceTaskHealthCheckJob = CreateMockXTOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
				mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = ZDateTime.Invalid;
				mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = ZDateTime.Invalid;
				mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
				var setting = new NotificationFrequency
				{
					Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable
				};
				SetValueXTOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);
				mockServiceTask.Object.RunTask();

				AssertNoExceptionThrown(() =>
				{
					mockServiceManagerGovernor.Verify(g => g.SetServiceTaskNextRuntime("XHC", new DateTimeOffset(TestDateAttribute.Date.AddMinutes(60), TimeSpan.Zero)));
				});
			}
		}

		[TestDate(2024, 01, 01)]
		public void Test_xTHealthCheck_NoFailedEdIInterchange()
		{
			var lastReportedTimePointForEdiFailedInterchanges = (ZDateTime)TestDateAttribute.Date.AddMinutes(-15);

			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockXTOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);

			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.FrequencySetting).Returns(HealthCheckConstants.NotificationFrequencyConstants.Periodically);
			mockServiceTaskHealthCheckJob.Setup(m => m.IntervalInMinute).Returns(15);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);

			var companies = mockServiceTaskHealthCheckJob.Object.Check();

			Assert(!companies.FoundError());
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
		}

		public void Test_xTHealthCheck_NotifyNotSend()
		{
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockXTOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Setup(m => m.Check()).Returns(FailedEDIInterchangeHealthCheckResult.Empty);
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });

			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 20
			};
			SetValueXTOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			mockServiceTaskHealthCheckJob.Object.Execute();
			XTHealthCheckTestHelpers.AssertNoErrorEmail();

			setting.Settings = HealthCheckConstants.NotificationFrequencyConstants.SPAM;
			SetValueXTOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			mockServiceTaskHealthCheckJob.Object.Execute();
			XTHealthCheckTestHelpers.AssertNoErrorEmail();

			setting.Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable;
			SetValueXTOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			mockServiceTaskHealthCheckJob.Object.Execute();
			XTHealthCheckTestHelpers.AssertNoErrorEmail();
		}

		[TestDate(2024, 01, 01)]
		public void Test_xTHealthCheck_Notify_Periodically()
		{
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 25
			};
			SetValueXTOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			var lastReportedTimePointForEdiFailedInterchanges = TestDateAttribute.Date.AddMinutes(-25);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockXTOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);

			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{GlbCompany.CurrentCompany.GC_Code}=5,invalid,TestCompanyCode=5");
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);
			XTHealthCheckTestHelpers.CreateInterchangeForHealthCheckTests(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);

			mockServiceTask.Object.RunTask();

			var currentTimePointAfterUpdated = mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange;
			AssertEquals(TestDateAttribute.Date, currentTimePointAfterUpdated);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(LogMessages.FailedInterchangesReportMessageForCompany(GlbCompany.CurrentCompany.GC_Code, 1, lastReportedTimePointForEdiFailedInterchanges, currentTimePointAfterUpdated));
			XTHealthCheckTestHelpers.AssertErrorEmails(
				$@"<b>Failed Direct xT Interchange Health Check report for {mockServiceTaskHealthCheckJob.Object.ServiceTaskName}:</b></big><br/><p>{LogMessages
					.FailedInterchangesReportMessage(1, lastReportedTimePointForEdiFailedInterchanges, currentTimePointAfterUpdated)}</p>
<p>For further details, please search for the [EDI Interchange] module and view the notes on the relevant failed Interchange(s).</p>",
				$"<title>XTO Failed Interchange Health Check Report for company {GlbCompany.CurrentCompany.GC_Name}</title>");
		}

		[TestDate(2024, 01, 01, 12, 0, 0)]
		public void Test_xTHealthCheck_Notify_Periodically_IntervalMoreThanTwoDays()
		{
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 60 * 24 * 3
			};
			SetValueXTOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockXTOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);

			((IRegistryItemInternals)mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			((IRegistryItemInternals)DirectxTMessagingRegistry.Instance.xTLastReportedTimeForFailedEDIInterchange).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			((IRegistryItemInternals)DirectxTMessagingRegistry.Instance.xTLastCheckedTimeForFailedEDIInterchange).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			XTHealthCheckTestHelpers.CreateInterchangeForHealthCheckTests(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			var day1 = TestDateAttribute.Date;
			Factory.Save();

			XTHealthCheckTestHelpers.CreateInterchangeForHealthCheckTests(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			var day2 = TestDateAttribute.Date;
			Factory.Save();

			XTHealthCheckTestHelpers.CreateInterchangeForHealthCheckTests(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			var day3 = TestDateAttribute.Date;
			Factory.Save();

			XTHealthCheckTestHelpers.CreateInterchangeForHealthCheckTests(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			var day4 = TestDateAttribute.Date;
			Factory.Save();

			TestDateAttribute.Date = day1;
			mockServiceTask.Object.RunTask();

			AssertEquals(day1, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			AssertEquals(day1, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages("First time running Failed EDI Interchange Health Check. Record a starting time.");
			AssertEquals(1, mockServiceTask.Object.Notifier.GetNotificationBuffer().Events.Length);
			XTHealthCheckTestHelpers.AssertNoErrorEmail();

			TestDateAttribute.Date = day1.AddHours(12);
			mockServiceTask.Object.RunTask();

			AssertEquals("Should not perform any check and not update registry", day1, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			AssertEquals("Should not perform any check and not update registry", day1, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			AssertEquals(1, mockServiceTask.Object.Notifier.GetNotificationBuffer().Events.Length);

			TestDateAttribute.Date = day2;
			mockServiceTask.Object.RunTask();

			AssertEquals("Should not notify and update LastReportedTimeForFailedEDIInterchange until last run on day 4.", day1, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			AssertEquals(day2, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages($"[Failed Direct xT Interchange] 1 failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between 01-Jan-24 12:00:00 and 02-Jan-24 12:00:00.");
			AssertEquals(2, mockServiceTask.Object.Notifier.GetNotificationBuffer().Events.Length);
			XTHealthCheckTestHelpers.AssertNoErrorEmail();

			TestDateAttribute.Date = day3;
			mockServiceTask.Object.RunTask();

			AssertEquals("Should not notify and update LastReportedTimeForFailedEDIInterchange until last run on day 4.", day1, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			AssertEquals(day3, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages($"[Failed Direct xT Interchange] 1 failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between 02-Jan-24 12:00:00 and 03-Jan-24 12:00:00.");
			AssertEquals(3, mockServiceTask.Object.Notifier.GetNotificationBuffer().Events.Length);
			XTHealthCheckTestHelpers.AssertNoErrorEmail();

			TestDateAttribute.Date = day4;
			mockServiceTask.Object.RunTask();

			AssertEquals(day4, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			AssertEquals(day4, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(
				$"[Failed Direct xT Interchange] 1 failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between 03-Jan-24 12:00:00 and 04-Jan-24 12:00:00.",
				"[Failed Direct xT Interchange] Notification(s) sent for period 01-Jan-24 12:00:00 to 04-Jan-24 12:00:00.");
			AssertEquals(6, mockServiceTask.Object.Notifier.GetNotificationBuffer().Events.Length);
			AssertEquals(HealthCheckConstants.NotificationFrequencyConstants.Periodically, DirectxTMessagingRegistry.Instance.xTFailedEDIInterchangeNotificationFrequency.Value.Settings);
			XTHealthCheckTestHelpers.AssertErrorEmails(
				$@"<b>Failed Direct xT Interchange Health Check report for {mockServiceTaskHealthCheckJob.Object.ServiceTaskName}:</b></big><br/><p>3 failed Interchange(s) found between 01-Jan-24 12:00:00 and 04-Jan-24 12:00:00.</p>
<p>For further details, please search for the [EDI Interchange] module and view the notes on the relevant failed Interchange(s).</p>",
				$"<title>XTO Failed Interchange Health Check Report for company {GlbCompany.CurrentCompany.GC_Name}</title>");
		}

		[TestDate(2024, 01, 01)]
		public void Test_xTHealthCheck_ShouldNotify_Periodically()
		{
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 20
			};

			SetValueXTOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);
			var lastReportedTimePointForEdiFailedInterchanges = TestDateAttribute.Date.AddMinutes(-25);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockXTOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{GlbCompany.CurrentCompany.GC_Code}=5,invalid,TestCompanyCode=5");
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			XTHealthCheckTestHelpers.CreateInterchangeForHealthCheckTests(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-6);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(6);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			mockServiceTask.Object.RunTask();

			var currentTimePointAfterUpdated = mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange;
			AssertEquals(lastReportedTimePointForEdiFailedInterchanges.AddMinutes(20), currentTimePointAfterUpdated);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(
				LogMessages.FailedInterchangesReportMessageForCompany(GlbCompany.CurrentCompany.GC_Code, 1, lastReportedTimePointForEdiFailedInterchanges, currentTimePointAfterUpdated));

			XTHealthCheckTestHelpers.AssertErrorEmails(
				$@"<b>Failed Direct xT Interchange Health Check report for {mockServiceTaskHealthCheckJob.Object.ServiceTaskName}:</b></big><br/><p>{LogMessages
					.FailedInterchangesReportMessage(1, lastReportedTimePointForEdiFailedInterchanges, currentTimePointAfterUpdated)}</p>
<p>For further details, please search for the [EDI Interchange] module and view the notes on the relevant failed Interchange(s).</p>",
				$"<title>XTO Failed Interchange Health Check Report for company {GlbCompany.CurrentCompany.GC_Name}</title>");
		}

		[TestDate(2024, 01, 01)]
		public void Test_xTHealthCheck_Notify_SPAM()
		{
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.SPAM
			};
			SetValueXTOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			var lastReportedTimePointForEdiFailedInterchanges = TestDateAttribute.Date.AddMinutes(-5);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockXTOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);

			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{GlbCompany.CurrentCompany.GC_Code}=5,invalid,TestCompanyCode=5");
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			XTHealthCheckTestHelpers.CreateInterchangeForHealthCheckTests(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			mockServiceTask.Object.RunTask();

			var lastTimePoint = mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange;
			AssertEquals(TestDateAttribute.Date, lastTimePoint);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages($"1 failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between 31-Dec-23 23:55:00 and 01-Jan-24 00:00:00.");
			XTHealthCheckTestHelpers.AssertErrorEmails(
				$@"<big><b>Failed Direct xT Interchange Health Check report for {mockServiceTaskHealthCheckJob.Object.ServiceTaskName}:</b></big><br/><p>{LogMessages
					.FailedInterchangesReportMessage(1, lastReportedTimePointForEdiFailedInterchanges, lastTimePoint)}</p>
<p>For further details, please search for the [EDI Interchange] module and view the notes on the relevant failed Interchange(s).</p>",
				$"<title>XTO Failed Interchange Health Check Report for company {GlbCompany.CurrentCompany.GC_Name}</title>");
		}

		[TestDate(2024, 01, 01)]
		public void Test_xTHealthCheck_Notify_ShouldNotifyWithoutSendEmail_Disabled()
		{
			var mockServiceTaskHealthCheckJob = CreateMockXTOutboundFailedEDIInterchangeCheckJob();
			mockServiceTaskHealthCheckJob.Setup(m => m.ReadyToPerformCheck()).Returns(true);
			mockServiceTaskHealthCheckJob.Setup(m => m.Check())
				.Returns((Func<FailedEDIInterchangeHealthCheckResult>)(delegate
				{
					mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = TestDateAttribute.Date;
					var result = new FailedEDIInterchangeHealthCheckResult();
					result.Add(GlbCompany.CurrentCompany.PK.ToGuid(), 1);
					return result;
				}));
			SetValueXTOutboundFailedEDIInterchangeNotificationFrequencyRegistry(new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable
			});

			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = TestDateAttribute.Date.AddDays(-1);

			mockServiceTaskHealthCheckJob.Setup(m => m.Notify(It.IsAny<IHealthCheckResult>()));
			mockServiceTaskHealthCheckJob.Object.Execute();
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTaskHealthCheckJob.VerifyAll();
			XTHealthCheckTestHelpers.AssertNoErrorEmail();
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		void CreateDefaultServiceTaskSchedule()
		{
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = ServiceTaskCodeList.Codes.XHC;
			schedule.S5_IsActive = true;
			schedule.S5_TaskPeriod = "T";
			schedule.S5_TaskPeriodCount = 1;
			schedule.S5_NextScheduledPrintRunTimeUtc = TestDateAttribute.Date.AddMinutes(1);
			Factory.Save();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();
			_ = XTHealthCheckTestHelpers.CreateStaff("TST", Factory);
		}

		static Mock<XTHealthCheckServiceTask> CreateMockServiceTask()
		{
			var mockServiceTask = new Mock<XTHealthCheckServiceTask>() { CallBase = true };
			mockServiceTask.Object.Notifier = new NotificationBuffer();
			mockServiceTask.Object.ServiceLogger = new TestServiceLogger();

			return mockServiceTask;
		}

		static void SetValueXTOutboundFailedEDIInterchangeNotificationFrequencyRegistry(NotificationFrequency setting)
		{
			DirectxTMessagingRegistry.Instance.xTFailedEDIInterchangeNotificationFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, setting);
		}

		static Mock<XTOutboundFailedEDIInterchangeCheckJob> CreateMockXTOutboundFailedEDIInterchangeCheckJob(XTHealthCheckServiceTask mockServiceTask)
		{
			return new Mock<XTOutboundFailedEDIInterchangeCheckJob>(mockServiceTask.Notifier) { CallBase = true };
		}

		static Mock<XTOutboundFailedEDIInterchangeCheckJob> CreateMockXTOutboundFailedEDIInterchangeCheckJob()
		{
			return new Mock<XTOutboundFailedEDIInterchangeCheckJob>(new NotificationBuffer()) { CallBase = true };
		}

		static ZGuid CurrentBranchPk => ((IBranch)GlbCompany.CurrentCompany.Branches[0]).PK;
	}
}
