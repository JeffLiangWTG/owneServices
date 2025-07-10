using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.Business.Interfaces;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.ServiceTasks.HealthChecks.FailedEDIInterchangeHealthCheck;
using Enterprise.eHubMessaging.Tests.Business.DownloadHandler;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.HealthCheck
{
	// ReSharper disable once InconsistentNaming
	[TestedType(typeof(eHubHealthCheckServiceTask))]
	class eHubHealthCheckServiceTaskTests : ServiceTaskTestCase<eHubHealthCheckServiceTask>
	{
		[TestDate(2016, 06, 12)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_CheckMethod_InvalidLastSuccessDateTime()
		{
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = DateTime.MinValue;
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });

			Assert("Precondition", !mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			mockServiceTask.Object.RunTask();
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages("First time running Failed EDI Interchange Health Check. Record a starting time.");
			TestHelpers.AssertNoErrorEmail();
		}

		[TestDate(2016, 06, 12)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_CheckCore()
		{
			var lastReportedTimePointForEdiFailedInterchanges = (ZDateTime)TestDateAttribute.Date.AddMinutes(-15);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{GlbCompany.CurrentCompany.Branches[0].PK}=5,invalid,{Guid.NewGuid()}=5"); // If interval is less than 1 day, not use FailedEDIInterchangesCountInPeriod
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.FrequencySetting).Returns(HealthCheckConstants.NotificationFrequencyConstants.Periodically);
			mockServiceTaskHealthCheckJob.Setup(m => m.IntervalInMinute).Returns(15);

			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange.IsValid);
			var companies = mockServiceTaskHealthCheckJob.Object.Check();
			Assert(companies.FoundError());
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			AssertEquals("LastReportedTimeForFailedEDIInterchange should only be updated in Notify() not Check()", lastReportedTimePointForEdiFailedInterchanges, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(
				$"1 failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between {lastReportedTimePointForEdiFailedInterchanges} and {mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange}.");
		}

		[TestDate(2016, 06, 12)]
		public void TestEHubOutboundHealthCheckJob_JobRunsToCompletion()
		{
			var lastReportedTimePointForEdiFailedInterchanges = (ZDateTime)TestDateAttribute.Date.AddMinutes(-30);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{GlbCompany.CurrentCompany.Branches[0].PK}=5,invalid,{Guid.NewGuid()}=5"); // If interval is less than 1 day, not use FailedEDIInterchangesCountInPeriod
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.FrequencySetting).Returns(HealthCheckConstants.NotificationFrequencyConstants.Periodically);
			mockServiceTaskHealthCheckJob.Setup(m => m.IntervalInMinute).Returns(15);
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
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

		[TestDate(2016, 06, 12)]
		public void TestEHubOutboundHealthCheckJob_FirstJobRunsToCompletionWhenLastJobHasNothingToDo()
		{
			var lastReportedTimePointForEdiFailedInterchanges = (ZDateTime)TestDateAttribute.Date.AddMinutes(-30);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			var mockJob = new Mock<IEHubServiceTaskHealthCheckJob>();
			mockJob.Setup(m => m.Execute()).Returns(false);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{GlbCompany.CurrentCompany.Branches[0].PK}=5,invalid,{Guid.NewGuid()}=5"); // If interval is less than 1 day, not use FailedEDIInterchangesCountInPeriod
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object, mockJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.FrequencySetting).Returns(HealthCheckConstants.NotificationFrequencyConstants.Periodically);
			mockServiceTaskHealthCheckJob.Setup(m => m.IntervalInMinute).Returns(15);
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange.IsValid);
			mockServiceTask.Object.RunTask();
			mockJob.VerifyAll();
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			AssertEquals("LastReportedTimeForFailedEDIInterchange should be updated", TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(
				$"1 failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between {lastReportedTimePointForEdiFailedInterchanges.AddMinutes(15)} and {mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange}.");
		}

		[TestDate(2016, 06, 12)]
		public void TestServiceTaskSchedule_UpdatesToMatchIntervalConfiguration()
		{
			var mockServiceManagerGovernor = new Mock<IServiceManagerGovernor>();
			var mockServiceManagerQuerier = new Mock<IServiceManagerQuerier>();
			var previousRunTime = ZDateTime.UtcNow.ToNullableDateTimeOffset();
			mockServiceManagerQuerier.Setup(q => q.TryGetServiceTaskNextRunTime(It.IsAny<string>(), out previousRunTime)).Returns(true);

			using (ObjectFactory.Substitute(mockServiceManagerQuerier.Object))
			using (ObjectFactory.Substitute(mockServiceManagerGovernor.Object))
			{
				var mockServiceTask = CreateMockServiceTask();
				var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
				mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = ZDateTime.Invalid;
				mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = ZDateTime.Invalid;
				mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
				var setting = new NotificationFrequency
				{
					Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
					TimeInterval = 15
				};
				SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

				mockServiceTask.Object.RunTask();

				AssertNoExceptionThrown(() =>
				{
					mockServiceManagerGovernor.Verify(governor => governor.SetServiceTaskNextRuntime(It.IsAny<string>(), new DateTimeOffset(TestDateAttribute.Date.AddMinutes(15), TimeSpan.Zero)));
					mockServiceTaskHealthCheckJob.VerifyAll();
				});
			}
		}

		[TestDate(2016, 06, 12)]
		public void TestServiceTaskSchedule_UpdatesToMinimumInterval()
		{
			var mockServiceManagerGovernor = new Mock<IServiceManagerGovernor>();
			var mockServiceManagerQuerier = new Mock<IServiceManagerQuerier>();
			var previousRunTime = ZDateTime.UtcNow.ToNullableDateTimeOffset();
			mockServiceManagerQuerier.Setup(q => q.TryGetServiceTaskNextRunTime(It.IsAny<string>(), out previousRunTime)).Returns(true);

			using (ObjectFactory.Substitute(mockServiceManagerQuerier.Object))
			using (ObjectFactory.Substitute(mockServiceManagerGovernor.Object))
			{
				var mockJob1 = new Mock<IEHubServiceTaskHealthCheckJob>();
				mockJob1.Setup(m => m.ServiceTaskNextRunTimeUtc).Returns(TestDateAttribute.Date.AddMinutes(15));
				var mockJob2 = new Mock<IEHubServiceTaskHealthCheckJob>();
				mockJob2.Setup(m => m.ServiceTaskNextRunTimeUtc).Returns(TestDateAttribute.Date.AddMinutes(5));
				var mockServiceTask = CreateMockServiceTask();
				mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockJob1.Object, mockJob2.Object });

				mockServiceTask.Object.RunTask();

				AssertNoExceptionThrown(() =>
				{
					mockServiceManagerGovernor.Verify(governor => governor.SetServiceTaskNextRuntime(It.IsAny<string>(), new DateTimeOffset(TestDateAttribute.Date.AddMinutes(5), TimeSpan.Zero)));
					mockJob1.VerifyAll();
					mockJob2.VerifyAll();
				});
			}
		}

		[TestDate(2016, 06, 12)]
		public void TestServiceTaskSchedule_WhenNoIntervalSpecified()
		{
			var mockServiceManagerGovernor = new Mock<IServiceManagerGovernor>();
			var mockServiceManagerQuerier = new Mock<IServiceManagerQuerier>();
			var previousRunTime = ZDateTime.UtcNow.ToNullableDateTimeOffset();
			mockServiceManagerQuerier.Setup(q => q.TryGetServiceTaskNextRunTime(It.IsAny<string>(), out previousRunTime)).Returns(true);

			using (ObjectFactory.Substitute(mockServiceManagerQuerier.Object))
			using (ObjectFactory.Substitute(mockServiceManagerGovernor.Object))
			{
				var mockServiceTask = CreateMockServiceTask();
				var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
				mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = ZDateTime.Invalid;
				mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = ZDateTime.Invalid;
				mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
				var setting = new NotificationFrequency
				{
					Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable
				};
				SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

				mockServiceTask.Object.RunTask();

				AssertNoExceptionThrown(() =>
				{
					mockServiceManagerGovernor.Verify(governor => governor.SetServiceTaskNextRuntime(It.IsAny<string>(), new DateTimeOffset(TestDateAttribute.Date.AddMinutes(60), TimeSpan.Zero)));
				});
			}
		}

		[TestDate(2016, 06, 12)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_CheckCore_NoneFailedEdIInterchange()
		{
			var lastReportedTimePointForEdiFailedInterchanges = (ZDateTime)TestDateAttribute.Date.AddMinutes(-15);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
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
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(
				$"0 failed Interchange found between {lastReportedTimePointForEdiFailedInterchanges} and {mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange}.");
		}

		[TestDate]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_Check_Periodically_FloorDownUtcNow_ProcessInterchangesWithFloorDownEI_SystemLastEditTimeUtc()
		{
			// Because of EI_SystemLastEditTimeUtc is smalldatetime which is floored down up to 1 minute.
			// StartPoint: 00:01:00 = LastReportedTimePointForEDIFailedInterchanges
			// Interval: 1 minute
			// Add Failed Interchange: 00:01:09 => EI_SystemLastEditTimeUtc = 00:01:00
			// nextExecution: currentTime: 00:01:10 < 00:01:00 + 1 minute => ShouldCheck = false
			// Add Failed Interchange: 00:01:59 => EI_SystemLastEditTimeUtc = 00:01:00
			// nextExecution: currentTime: 00:02:10 => ShouldCheck = true: reason: collect 2 interchanges >= 00:01:00 and < 00:02:00 => LastCheckedTimeForFailedEDIInterchange = 00:02:00, 
			// Add Failed Interchange: 00:04:10 => EI_SystemLastEditTimeUtc = 00:04:00
			// nextExecution: currentTime: 00:04:20 >= 00:02:00 + 1 minute => ShouldCheck = true => collect 0 interchange >= 00:02:00 and < 00:03:00 (floored down) => LastCheckedTimeForFailedEDIInterchange = 0:03:00
			// nextExecution: currentTime: 00:04:30 >= 00:03:00 + 1 minute => ShouldCheck = true => collect 0 interchange >= 00:03:00 and < 00:04:00 (floored down) => LastCheckedTimeForFailedEDIInterchange = 0:04:00
			// nextExecution: currentTime: 00:05:00 >= 00:04:00 + 1 minute => Check = true => collect 1 interchange >= 00:04:00 and < 00:05:00 (floored down) => LastReportedTimePointForEDIFailedInterchanges = 0:05:00
			var currentTime = new ZDateTime(DateTime.UtcNow).ToSmallDateTimeFloor(); // 00:01:00
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(CreateMockServiceTask().Object);
			mockServiceTaskHealthCheckJob.Setup(m => m.FrequencySetting).Returns(HealthCheckConstants.NotificationFrequencyConstants.Periodically);
			mockServiceTaskHealthCheckJob.Setup(m => m.IntervalInMinute).Returns(1);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = currentTime;

			TestDateAttribute.Date = currentTime.AddSeconds(9).ToDateTime(); // 00:01:09
			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			Factory.Save();
			AssertEquals(currentTime.AddSeconds(9), outgoingInterchange.EI_SystemLastEditTimeUtc);

			currentTime = currentTime.AddSeconds(10);                                       // 00:01:10
			mockServiceTaskHealthCheckJob.Setup(m => m.GetSmallDateTimeUtcNowFloor()).Returns(currentTime.ToSmallDateTimeFloor());
			Assert(!mockServiceTaskHealthCheckJob.Object.ReadyToPerformCheck());

			var outgoingInterchange1 = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange1.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange1.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			TestDateAttribute.Date = currentTime.AddSeconds(49).ToDateTime(); // 00:01:59
			Factory.Save();
			AssertEquals(currentTime.AddSeconds(49), outgoingInterchange1.EI_SystemLastEditTimeUtc);

			currentTime = currentTime.AddMinutes(1); // 00:02:10
			mockServiceTaskHealthCheckJob.Setup(m => m.GetSmallDateTimeUtcNowFloor()).Returns(currentTime.ToSmallDateTimeFloor());
			Assert(mockServiceTaskHealthCheckJob.Object.ReadyToPerformCheck());
			Assert(mockServiceTaskHealthCheckJob.Object.Check().FoundError());

			var outgoingInterchange2 = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange2.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange2.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			TestDateAttribute.Date = currentTime.AddMinutes(2).ToDateTime(); // 00:04:10
			Factory.Save();
			AssertEquals(currentTime.AddMinutes(2), outgoingInterchange2.EI_SystemLastEditTimeUtc);

			currentTime = TestDateAttribute.Date = currentTime.AddMinutes(2).AddSeconds(10).ToDateTime();         // 00:04:20
			mockServiceTaskHealthCheckJob.Setup(m => m.GetSmallDateTimeUtcNowFloor()).Returns(currentTime.ToSmallDateTimeFloor());
			Assert(mockServiceTaskHealthCheckJob.Object.ReadyToPerformCheck());
			Assert(!mockServiceTaskHealthCheckJob.Object.Check().FoundError()); // 00:02 -> 00:03

			currentTime = TestDateAttribute.Date = currentTime.AddSeconds(10).ToDateTime();           // 00:04:30
			mockServiceTaskHealthCheckJob.Setup(m => m.GetSmallDateTimeUtcNowFloor()).Returns(currentTime.ToSmallDateTimeFloor());
			Assert(mockServiceTaskHealthCheckJob.Object.ReadyToPerformCheck());
			Assert(!mockServiceTaskHealthCheckJob.Object.Check().FoundError()); // 00:03 -> 00:04

			currentTime = TestDateAttribute.Date = currentTime.AddSeconds(30).ToDateTime();           // 00:05:00
			mockServiceTaskHealthCheckJob.Setup(m => m.GetSmallDateTimeUtcNowFloor()).Returns(currentTime.ToSmallDateTimeFloor());
			Assert(mockServiceTaskHealthCheckJob.Object.ReadyToPerformCheck());
			Assert(mockServiceTaskHealthCheckJob.Object.Check().FoundError()); // 00:04 -> 00:05
		}

		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_Notify_DoNotSend()
		{
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Setup(m => m.Check()).Returns(FailedEDIInterchangeHealthCheckResult.Empty);
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });

			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 20
			};
			SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			mockServiceTaskHealthCheckJob.Object.Execute();
			TestHelpers.AssertNoErrorEmail();

			setting.Settings = HealthCheckConstants.NotificationFrequencyConstants.SPAM;
			SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			mockServiceTaskHealthCheckJob.Object.Execute();
			TestHelpers.AssertNoErrorEmail();

			setting.Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable;
			SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			mockServiceTaskHealthCheckJob.Object.Execute();
			TestHelpers.AssertNoErrorEmail();
		}

		[TestDate(2016, 06, 12)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_ShouldNotify_IntervalLessThanADay_Periodically()
		{
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 25
			};
			SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);
			var lastReportedTimePointForEdiFailedInterchanges = TestDateAttribute.Date.AddMinutes(-25);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{GlbCompany.CurrentCompany.GC_Code}=5,invalid,TestCompanyCode=5"); // If interval is less than 1 day, not use FailedEDIInterchangesCountInPeriod
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			mockServiceTask.Object.RunTask();

			var currentTimePointAfterUpdated = mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange;
			AssertEquals(TestDateAttribute.Date, currentTimePointAfterUpdated);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(LogMessages.FailedInterchangesReportMessageForCompany(GlbCompany.CurrentCompany.GC_Code, 1, lastReportedTimePointForEdiFailedInterchanges, currentTimePointAfterUpdated));
			TestHelpers.AssertErrorEmails(
				$@"<b>Failed eHub Interchange Health Check report for {mockServiceTaskHealthCheckJob.Object.ServiceTaskName}:</b></big><br/><p>{LogMessages
					.FailedInterchangesReportMessage(1, lastReportedTimePointForEdiFailedInterchanges, currentTimePointAfterUpdated)}</p>
<p>For further details, please search for the [EDI Interchange] module and view the notes on the relevant failed Interchange(s).</p>",
				$"<title>EHO Failed Interchange Health Check Report for company {GlbCompany.CurrentCompany.GC_Name}</title>");
		}

		[TestDate(2016, 06, 12, 12, 0, 0)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_ShouldNotify_IntervalMoreThanTwoDay_Periodically()
		{
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 60 * 24 * 3
			};
			SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			((IRegistryItemInternals)mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			((IRegistryItemInternals)eHubMessagingRegistry.Instance.eHubLastReportedTimeForFailedEDIInterchange).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			((IRegistryItemInternals)eHubMessagingRegistry.Instance.eHubLastCheckedTimeForFailedEDIInterchange).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			var day1 = TestDateAttribute.Date;
			Factory.Save();

			outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			var day2 = TestDateAttribute.Date;
			Factory.Save();

			outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			var day3 = TestDateAttribute.Date;
			Factory.Save();

			outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			var day4 = TestDateAttribute.Date;
			Factory.Save();

			// Day 1
			TestDateAttribute.Date = day1;
			mockServiceTask.Object.RunTask();
			AssertEquals(day1, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			AssertEquals(day1, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages("First time running Failed EDI Interchange Health Check. Record a starting time.");
			AssertEquals(1, mockServiceTask.Object.Notifier.GetNotificationBuffer().Events.Length);
			TestHelpers.AssertNoErrorEmail();

			// Half Day 1
			TestDateAttribute.Date = day1.AddHours(12);
			mockServiceTask.Object.RunTask();
			AssertEquals("Should not perform any check and not update registry", day1, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			AssertEquals("Should not perform any check and not update registry", day1, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			AssertEquals(1, mockServiceTask.Object.Notifier.GetNotificationBuffer().Events.Length);

			// Day 2
			TestDateAttribute.Date = day2;
			mockServiceTask.Object.RunTask();
			AssertEquals("Should not notify and update LastReportedTimeForFailedEDIInterchange until last run on day 4.", day1, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			AssertEquals(day2, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages($"[Failed eHub Interchange] 1 failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between 12-Jun-16 12:00:00 and 13-Jun-16 12:00:00.");
			AssertEquals(2, mockServiceTask.Object.Notifier.GetNotificationBuffer().Events.Length);
			TestHelpers.AssertNoErrorEmail();

			// Day 3
			TestDateAttribute.Date = day3;
			mockServiceTask.Object.RunTask();
			AssertEquals("Should not notify and update LastReportedTimeForFailedEDIInterchange until last run on day 4.", day1, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			AssertEquals(day3, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages($"[Failed eHub Interchange] 1 failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between 13-Jun-16 12:00:00 and 14-Jun-16 12:00:00.");
			AssertEquals(3, mockServiceTask.Object.Notifier.GetNotificationBuffer().Events.Length);
			TestHelpers.AssertNoErrorEmail();

			// Day 4
			TestDateAttribute.Date = day4;
			mockServiceTask.Object.RunTask();
			AssertEquals(day4, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			AssertEquals(day4, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(
				$"[Failed eHub Interchange] 1 failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between 14-Jun-16 12:00:00 and 15-Jun-16 12:00:00.",
				"[Failed eHub Interchange] Notification Frequency: PERIODICALLY.",
				"[Failed eHub Interchange] Notification(s) sent for period 12-Jun-16 12:00:00 to 15-Jun-16 12:00:00.");
			AssertEquals(6, mockServiceTask.Object.Notifier.GetNotificationBuffer().Events.Length);
			TestHelpers.AssertErrorEmails(
				$@"<b>Failed eHub Interchange Health Check report for {mockServiceTaskHealthCheckJob.Object.ServiceTaskName}:</b></big><br/><p>3 failed Interchange(s) found between 12-Jun-16 12:00:00 and 15-Jun-16 12:00:00.</p>
<p>For further details, please search for the [EDI Interchange] module and view the notes on the relevant failed Interchange(s).</p>",
				$"<title>EHO Failed Interchange Health Check Report for company {GlbCompany.CurrentCompany.GC_Name}</title>");
		}

		[TestDate(2016, 06, 12)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_NotifyShouldNotify_SentEmailNotification_Periodically_ContainsInvalidCompanyCode()
		{
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 20
			};

			SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);
			var lastReportedTimePointForEdiFailedInterchanges = TestDateAttribute.Date.AddMinutes(-25);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges; // should report 25 >= 20
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{GlbCompany.CurrentCompany.GC_Code}=5,invalid,TestCompanyCode=5"); // If interval is less than 1 day, not use FailedEDIInterchangesCountInPeriod
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-6);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(6);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			mockServiceTask.Object.RunTask();

			var currentTimePointAfterUpdated = mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange;
			AssertEquals(lastReportedTimePointForEdiFailedInterchanges.AddMinutes(20), currentTimePointAfterUpdated);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(
				LogMessages.FailedInterchangesReportMessageForCompany(GlbCompany.CurrentCompany.GC_Code, 1, lastReportedTimePointForEdiFailedInterchanges, currentTimePointAfterUpdated));

			TestHelpers.AssertErrorEmails(
				$@"<b>Failed eHub Interchange Health Check report for {mockServiceTaskHealthCheckJob.Object.ServiceTaskName}:</b></big><br/><p>{LogMessages
					.FailedInterchangesReportMessage(1, lastReportedTimePointForEdiFailedInterchanges, currentTimePointAfterUpdated)}</p>
<p>For further details, please search for the [EDI Interchange] module and view the notes on the relevant failed Interchange(s).</p>",
				$"<title>EHO Failed Interchange Health Check Report for company {GlbCompany.CurrentCompany.GC_Name}</title>");
		}

		[TestDate(2016, 06, 12)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_Notify_ShouldNotify_SentEmailNotification_SPAM()
		{
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.SPAM
			};

			SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);
			var lastReportedTimePointForEdiFailedInterchanges = TestDateAttribute.Date.AddMinutes(-5);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{GlbCompany.CurrentCompany.GC_Code}=5,invalid,TestCompanyCode=5"); // If interval is less than 1 day, not use FailedEDIInterchangesCountInPeriod
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			mockServiceTask.Object.RunTask();

			var lastTimePoint = mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange;
			AssertEquals(TestDateAttribute.Date, lastTimePoint);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages($"1 failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between 11-Jun-16 23:55:00 and 12-Jun-16 00:00:00.");
			TestHelpers.AssertErrorEmails(
				$@"<big><b>Failed eHub Interchange Health Check report for {mockServiceTaskHealthCheckJob.Object.ServiceTaskName}:</b></big><br/><p>{LogMessages
					.FailedInterchangesReportMessage(1, lastReportedTimePointForEdiFailedInterchanges, lastTimePoint)}</p>
<p>For further details, please search for the [EDI Interchange] module and view the notes on the relevant failed Interchange(s).</p>",
				$"<title>EHO Failed Interchange Health Check Report for company {GlbCompany.CurrentCompany.GC_Name}</title>");
		}

		[TestDate(2016, 06, 12, 1, 0, 0)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_Check_PerformCheck_HaveResult_Disabled()
		{
			var mockServiceTaskHealthCheckJob =
				new Mock<eHubOutboundFailedEDIInterchangeCheckJob>(new NotificationBuffer()) { CallBase = true };

			var invalidCompanyCodeOutgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			invalidCompanyCodeOutgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			invalidCompanyCodeOutgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable
			};
			SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = TestDateAttribute.Date.AddDays(-1);
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = TestDateAttribute.Date.AddDays(-1);

			var result = mockServiceTaskHealthCheckJob.Object.Check();
			Assert(result.FoundError());

			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			AssertEquals("LastReportedTimeForFailedEDIInterchange should only be updated in Notify() not Check()", TestDateAttribute.Date.AddDays(-1), mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			TestHelpers.AssertNoErrorEmail();
		}

		[TestDate(2016, 06, 12, 1, 0, 0)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_Check_PerformCheck_After24Hours_HaveNoResult_Disabled()
		{
			var mockServiceTaskHealthCheckJob =
				new Mock<eHubOutboundFailedEDIInterchangeCheckJob>(new NotificationBuffer()) { CallBase = true };

			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable
			};
			SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = TestDateAttribute.Date.AddDays(-1).AddHours(-1);
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = TestDateAttribute.Date.AddDays(-1).AddHours(-1);

			var result = mockServiceTaskHealthCheckJob.Object.Check();
			Assert(!result.FoundError());

			AssertEquals(TestDateAttribute.Date.AddHours(-1), mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			TestHelpers.AssertNoErrorEmail();
		}

		[TestDate(2016, 06, 12, 23, 0, 0)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_Check_NotPerformCheck_NotUpdateRegistries_BeforeMidnight_Disabled()
		{
			var mockServiceTaskHealthCheckJob =
				new Mock<eHubOutboundFailedEDIInterchangeCheckJob>(new NotificationBuffer()) { CallBase = true };

			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable
			};
			SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			var previousMidNight = TestDateAttribute.Date.Date;
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = previousMidNight;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = previousMidNight;

			mockServiceTaskHealthCheckJob.Object.Execute();

			previousMidNight = TestDateAttribute.Date.AddHours(-23);
			AssertEquals(previousMidNight, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			AssertEquals(previousMidNight, mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange);
			TestHelpers.AssertNoErrorEmail();
		}

		[TestDate(2016, 06, 12)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_Notify_ShouldNotifyWithoutSendEmail_Disabled()
		{
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob();
			mockServiceTaskHealthCheckJob.Setup(m => m.ReadyToPerformCheck()).Returns(true);
			mockServiceTaskHealthCheckJob.Setup(m => m.Check())
					.Returns((Func<FailedEDIInterchangeHealthCheckResult>)(delegate
				{
					mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = TestDateAttribute.Date;
					var result = new FailedEDIInterchangeHealthCheckResult();
					result.Add(GlbCompany.CurrentCompany.PK.ToGuid(), 1);
					return result;
				}));
			SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable
			});

			// Testing all registry items will reset when it successfully sent a notification.
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = TestDateAttribute.Date.AddDays(-1);

			mockServiceTaskHealthCheckJob.Setup(m => m.Notify(It.IsAny<IHealthCheckResult>()));
			mockServiceTaskHealthCheckJob.Object.Execute();
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTaskHealthCheckJob.VerifyAll();
			TestHelpers.AssertNoErrorEmail();
		}

		[TestDate(2016, 06, 12)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_Notify_ShouldNotNotifyWhenFailureIsInbound()
		{
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(CreateMockServiceTask().Object);
			AssertFailedEhubEDIInterchange_Notify_ShouldNotNotifyWhenFailureIsInbound(mockServiceTaskHealthCheckJob, EDIInterchangeTransportTypeList.Codes.eHub);
		}

		[TestDate(2016, 06, 12)]
		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_CheckMethod_InvalidLastSuccessConnection()
		{
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = DateTime.MinValue;
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 15
			});

			Assert("Precondition", !mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			mockServiceTask.Object.RunTask();
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages("First time running Failed EDI Interchange Health Check. Record a starting time.");
			TestHelpers.AssertNoErrorEmail();
		}

		[TestDate(2016, 06, 12)]
		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_CheckCore()
		{
			var lastReportedTimePointForEdiFailedInterchanges = (ZDateTime)TestDateAttribute.Date.AddMinutes(-15);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 15
			});
			AssertEquals(mockServiceTaskHealthCheckJob.Object.IntervalInMinute, 15);

			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			var companies = mockServiceTaskHealthCheckJob.Object.Check();
			Assert(companies.FoundError());
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(
				$"{1} failed Interchange(s) found for company {GlbCompany.CurrentCompany.GC_Code} between {lastReportedTimePointForEdiFailedInterchanges} and {mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange}.");
		}

		[TestDate(2016, 06, 12)]
		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_CheckCore_NoneFailedEdIInterchange()
		{
			var lastReportedTimePointForEdiFailedInterchanges = TestDateAttribute.Date.AddMinutes(-15);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 15
			});

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			var companies = mockServiceTaskHealthCheckJob.Object.Check();
			Assert(!companies.FoundError());
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
		}

		[TestDate]
		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_Check_Periodically_FloorDownUtcNow_ProcessInterchangesWithFloorDownEI_SystemLastEditTimeUtc()
		{
			// Because of EI_SystemLastEditTimeUtc is smalldatetime which is floored down up to 1 minute.
			// StartPoint: 00:01:00 = LastReportedTimePointForEDIFailedInterchanges
			// Interval: 1 minute
			// Add Failed Interchange: 00:01:09 => EI_SystemLastEditTimeUtc = 00:01:00
			// nextExecution: currentTime: 00:01:10 < 00:01:00 + 1 minute => ShouldCheck = false
			// Add Failed Interchange: 00:01:59 => EI_SystemLastEditTimeUtc = 00:01:00
			// nextExecution: currentTime: 00:02:10 => ShouldCheck = true: reason: collect 2 interchanges >= 00:01:00 and < 00:02:00 => LastCheckedTimeForFailedEDIInterchange = 00:02:00, 
			// Add Failed Interchange: 00:04:10 => EI_SystemLastEditTimeUtc = 00:04:00
			// nextExecution: currentTime: 00:04:20 >= 00:02:00 + 1 minute => ShouldCheck = true => collect 0 interchange >= 00:02:00 and < 00:03:00 (floored down) => LastCheckedTimeForFailedEDIInterchange = 0:03:00
			// nextExecution: currentTime: 00:04:30 >= 00:03:00 + 1 minute => ShouldCheck = true => collect 0 interchange >= 00:03:00 and < 00:04:00 (floored down) => LastCheckedTimeForFailedEDIInterchange = 0:04:00
			// nextExecution: currentTime: 00:05:00 >= 00:04:00 + 1 minute => Check = true => collect 1 interchange >= 00:04:00 and < 00:05:00 (floored down) => LastReportedTimePointForEDIFailedInterchanges = 0:05:00

			var currentTime = new ZDateTime(DateTime.UtcNow).ToSmallDateTimeFloor(); // 00:01:00
			var mockServiceTaskHealthCheckJob = CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(CreateMockServiceTask().Object);
			mockServiceTaskHealthCheckJob.Setup(m => m.FrequencySetting).Returns(HealthCheckConstants.NotificationFrequencyConstants.Periodically);
			mockServiceTaskHealthCheckJob.Setup(m => m.IntervalInMinute).Returns(1);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = currentTime;

			currentTime = TestDateAttribute.Date = currentTime.AddSeconds(9).ToDateTime(); // 00:01:09
			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			Factory.Save();
			AssertEquals(currentTime, outgoingInterchange.EI_SystemLastEditTimeUtc);

			currentTime = TestDateAttribute.Date = currentTime.AddSeconds(1).ToDateTime(); // 00:01:10
			mockServiceTaskHealthCheckJob.Setup(m => m.GetSmallDateTimeUtcNowFloor()).Returns(currentTime.ToSmallDateTimeFloor());
			Assert(!mockServiceTaskHealthCheckJob.Object.ReadyToPerformCheck());

			var outgoingInterchange1 = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange1.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange1.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			currentTime = TestDateAttribute.Date = currentTime.AddSeconds(49).ToDateTime(); // 00:01:59
			Factory.Save();
			AssertEquals(currentTime, outgoingInterchange1.EI_SystemLastEditTimeUtc);

			currentTime = TestDateAttribute.Date = currentTime.AddSeconds(11).ToDateTime();     // 00:02:10
			mockServiceTaskHealthCheckJob.Setup(m => m.GetSmallDateTimeUtcNowFloor()).Returns(currentTime.ToSmallDateTimeFloor());
			Assert(mockServiceTaskHealthCheckJob.Object.ReadyToPerformCheck());
			Assert(mockServiceTaskHealthCheckJob.Object.Check().FoundError());

			var outgoingInterchange2 = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange2.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange2.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			TestDateAttribute.Date = currentTime.AddMinutes(2).ToDateTime(); // 00:04:10
			Factory.Save();
			TestDateAttribute.Date = currentTime.ToDateTime(); // 00:02:10
			AssertEquals(currentTime.AddMinutes(2), outgoingInterchange2.EI_SystemLastEditTimeUtc);

			currentTime = TestDateAttribute.Date = currentTime.AddMinutes(2).AddSeconds(10).ToDateTime();         // 00:04:20
			mockServiceTaskHealthCheckJob.Setup(m => m.GetSmallDateTimeUtcNowFloor()).Returns(currentTime.ToSmallDateTimeFloor());
			Assert(mockServiceTaskHealthCheckJob.Object.ReadyToPerformCheck());
			Assert(!mockServiceTaskHealthCheckJob.Object.Check().FoundError()); // 00:02 -> 00:03

			currentTime = TestDateAttribute.Date = currentTime.AddSeconds(10).ToDateTime();           // 00:04:30
			mockServiceTaskHealthCheckJob.Setup(m => m.GetSmallDateTimeUtcNowFloor()).Returns(currentTime.ToSmallDateTimeFloor());
			Assert(mockServiceTaskHealthCheckJob.Object.ReadyToPerformCheck());
			Assert(!mockServiceTaskHealthCheckJob.Object.Check().FoundError()); // 00:03 -> 00:04

			currentTime = TestDateAttribute.Date = currentTime.AddSeconds(30).ToDateTime();           // 00:05:00
			mockServiceTaskHealthCheckJob.Setup(m => m.GetSmallDateTimeUtcNowFloor()).Returns(currentTime.ToSmallDateTimeFloor());
			Assert(mockServiceTaskHealthCheckJob.Object.ReadyToPerformCheck());
			Assert(mockServiceTaskHealthCheckJob.Object.Check().FoundError()); // 00:04 -> 00:05
		}

		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_Notify_DoNotSend()
		{
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Setup(m => m.Check()).Returns(FailedEDIInterchangeHealthCheckResult.Empty);
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });

			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 20
			};
			SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			mockServiceTaskHealthCheckJob.Object.Execute();
			TestHelpers.AssertNoErrorEmail();

			setting.Settings = HealthCheckConstants.NotificationFrequencyConstants.SPAM;
			SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			mockServiceTaskHealthCheckJob.Object.Execute();
			TestHelpers.AssertNoErrorEmail();

			setting.Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable;
			SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			mockServiceTaskHealthCheckJob.Object.Execute();
			TestHelpers.AssertNoErrorEmail();
		}

		[TestDate(2016, 06, 12)]
		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_NotifyShouldNotify_SentEmailNotification_Periodically()
		{
			AssertEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_NotifyShouldNotify_SentEmailNotification();
		}

		void AssertEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_NotifyShouldNotify_SentEmailNotification(GlbBranch branch = null, GlbCompany company = null)
		{
			company ??= GlbCompany.CurrentCompany;
			branch ??= company.Branches[0];

			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 20
			};
			SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);
			var lastCheckedTime = TestDateAttribute.Date.AddMinutes(-20);
			var lastReportedTime = TestDateAttribute.Date.AddMinutes(-20);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastCheckedTime;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTime;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{((ICompany)GlbCompany.CurrentCompany).PK}=5,invalid,TestCompanyCode=5"); // If interval is less than 1 day, not use FailedEDIInterchangesCountInPeriod
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			var outgoingInterchange = TestInterchangeMessage.GetNew(branch.PK.ToGuid(), company.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			using (EnvProxy.Instance.TemporaryServiceTaskContext("EHC", canRunInAnyBranch: true))
			{
				mockServiceTask.Object.RunTask();
			}

			var lastTimePoint = mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange;
			AssertEquals(TestDateAttribute.Date, lastTimePoint);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(LogMessages.FailedInterchangesReportMessageForCompany(company.GC_Code, 1, lastReportedTime, lastTimePoint));
			TestHelpers.AssertErrorEmails(
				$@"<big><b>Failed eAdaptor Interchange Health Check report for {mockServiceTaskHealthCheckJob.Object.ServiceTaskName}:</b></big><br/><p>{LogMessages
					.FailedInterchangesReportMessage(1, lastReportedTime, lastTimePoint)}</p>
<p>For further details, please search for the [EDI Interchange] module and view the notes on the relevant failed Interchange(s).</p>",
				$"<title>EAM Failed Interchange Health Check Report for company {company.GC_Name}</title>");
		}

		[TestDate(2016, 06, 12)]
		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_NotifyShouldNotify_SentEmailNotification_InactiveBranch()
		{
			var company = ServiceTaskJobWithAdapterTests<ServiceTaskJobWithAdapter>.CreateCompanyWithBranch(Factory);
			var branch = company.FirstActiveBranch;
			branch.GB_IsActive = false;
			AssertEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_NotifyShouldNotify_SentEmailNotification(branch, company);
		}

		[TestDate(2016, 06, 12)]
		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_Notify_ShouldNotify_SentEmailNotification_SPAM()
		{
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.SPAM
			};
			SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);
			var lastReportedTimePointForEdiFailedInterchanges = TestDateAttribute.Date.AddMinutes(-5);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(true);

			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			mockServiceTask.Object.RunTask();

			var lastTimePoint = mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange;
			AssertEquals(TestDateAttribute.Date, lastTimePoint);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(LogMessages.FailedInterchangesReportMessageForCompany(GlbCompany.CurrentCompany.GC_Code, 1, lastReportedTimePointForEdiFailedInterchanges, lastTimePoint));
			TestHelpers.AssertErrorEmails(
				$@"<big><b>Failed eAdaptor Interchange Health Check report for {mockServiceTaskHealthCheckJob.Object.ServiceTaskName}:</b></big><br/><p>{LogMessages
					.FailedInterchangesReportMessage(1, lastReportedTimePointForEdiFailedInterchanges, lastTimePoint)}</p>
<p>For further details, please search for the [EDI Interchange] module and view the notes on the relevant failed Interchange(s).</p>",
				$"<title>EAM Failed Interchange Health Check Report for company {GlbCompany.CurrentCompany.GC_Name}</title>");
		}

		[TestDate(2016, 06, 12, 0, 0, 0)]
		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_ShouldNotifyWithoutSendEmail_Disabled()
		{
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable
			};
			SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			// Testing all registry items will reset when it succesfully sent a notification.
			var lastReported = TestDateAttribute.Date.AddDays(-1);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReported;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReported;

			mockServiceTaskHealthCheckJob.Setup(m => m.Notify(It.IsAny<IHealthCheckResult>()));
			mockServiceTask.Object.RunTask();
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(LogMessages.FailedInterchangesReportMessageForCompany(GlbCompany.CurrentCompany.GC_Code, 1, lastReported, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange));
			TestHelpers.AssertNoErrorEmail();
			mockServiceTaskHealthCheckJob.VerifyAll();
		}

		[TestDate(2016, 06, 12, 1, 0, 0)]
		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_ShouldNotifyWithoutSendEmail_RunAfterMidnight_Disabled()
		{
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable
			};
			SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			// Testing all registry items will reset when it succesfully sent a notification.
			var lastReported = TestDateAttribute.Date.AddDays(-1);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReported;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReported;

			mockServiceTaskHealthCheckJob.Setup(m => m.Notify(It.IsAny<IHealthCheckResult>()));
			mockServiceTask.Object.RunTask();
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertNotificationContainsMessages(LogMessages.FailedInterchangesReportMessageForCompany(GlbCompany.CurrentCompany.GC_Code, 1, lastReported, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange));
			TestHelpers.AssertNoErrorEmail();
			mockServiceTaskHealthCheckJob.VerifyAll();
		}

		[TestDate(2016, 06, 11, 23, 0, 0)]
		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_ShouldNotNotifyWithoutSendEmail_RunBeforeMidnight_Disabled()
		{
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);
			var setting = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable
			};
			SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(setting);

			// Testing all registry items will reset when it succesfully sent a notification.
			var lastReported = TestDateAttribute.Date.AddHours(-23);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReported;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReported;

			mockServiceTask.Object.RunTask();
			AssertEquals(lastReported, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			mockServiceTask.Object.Notifier.AssertEmptyNotification();
			TestHelpers.AssertNoErrorEmail();
			mockServiceTaskHealthCheckJob.VerifyAll();
			mockServiceTaskHealthCheckJob.Verify(m => m.Notify(It.IsAny<IHealthCheckResult>()), Times.Never());
		}

		[TestDate(2016, 06, 12)]
		public void TestEAdaptorOutboundHealthCheckJob_FailedEDIInterchange_Notify_ShouldNotNotifyWhenFailureIsInbound()
		{
			var mockServiceTaskHealthCheckJob = CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(CreateMockServiceTask().Object);
			AssertFailedEAdaptorEDIInterchange_Notify_ShouldNotNotifyWhenFailureIsInbound(mockServiceTaskHealthCheckJob, EDIInterchangeTransportTypeList.Codes.eAdaptor);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();
			TestHelpers.CreateStaff("TST", Factory); // create staff member to send error emails to
		}

		void AssertFailedEhubEDIInterchange_Notify_ShouldNotNotifyWhenFailureIsInbound(Mock<eHubOutboundFailedEDIInterchangeCheckJob> mockServiceTaskHealthCheckJob, ZString transportType)
		{
			mockServiceTaskHealthCheckJob.Setup(m => m.FrequencySetting).Returns(HealthCheckConstants.NotificationFrequencyConstants.Periodically);
			mockServiceTaskHealthCheckJob.Setup(m => m.IntervalInMinute).Returns(1);
			var lastCheckTime = ZDateTime.UtcNow;
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastCheckTime;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastCheckTime;

			TestDateAttribute.Date = lastCheckTime.AddSeconds(30).ToDateTime();
			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Receive, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = transportType;
			Factory.Save();

			TestDateAttribute.Date = lastCheckTime.AddMinutes(30).ToDateTime();
			Assert("Shouldn't report inbound failure as error", !mockServiceTaskHealthCheckJob.Object.Check().FoundError());
		}

		void AssertFailedEAdaptorEDIInterchange_Notify_ShouldNotNotifyWhenFailureIsInbound(Mock<eAdaptorOutboundFailedEDIInterchangeCheckJob> mockServiceTaskHealthCheckJob, ZString transportType)
		{
			mockServiceTaskHealthCheckJob.Setup(m => m.FrequencySetting).Returns(HealthCheckConstants.NotificationFrequencyConstants.Periodically);
			mockServiceTaskHealthCheckJob.Setup(m => m.IntervalInMinute).Returns(1);
			var lastCheckTime = ZDateTime.UtcNow;
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastCheckTime;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastCheckTime;

			TestDateAttribute.Date = lastCheckTime.AddSeconds(30).ToDateTime();
			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Receive, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = transportType;
			Factory.Save();

			TestDateAttribute.Date = lastCheckTime.AddMinutes(30).ToDateTime();
			Assert("Shouldn't report inbound failure as error", !mockServiceTaskHealthCheckJob.Object.Check().FoundError());
		}

		[TestDate(2016, 06, 12)]
		public void TestEHubOutboundHealthCheckJob_FailedEDIInterchange_EndToEnd_ShouldNotify_ThrowDeveloperException_NoEmail()
		{
			var exceptionReporter = ExceptionReporterTestListener.Instance;
			var lastReportedTimePointForEdiFailedInterchanges = (ZDateTime)TestDateAttribute.Date.AddMinutes(-30);
			var mockServiceTask = CreateMockServiceTask();
			var mockServiceTaskHealthCheckJob = CreateMockEHubOutboundFailedEDIInterchangeCheckJob(mockServiceTask.Object);
			mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange = lastReportedTimePointForEdiFailedInterchanges;
			mockServiceTaskHealthCheckJob.Object.FailedEDIInterchangesCountInPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				$"{GlbCompany.CurrentCompany.Branches[0].PK}=5,invalid,{Guid.NewGuid()}=5"); // If interval is less than 1 day, not use FailedEDIInterchangesCountInPeriod
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskHealthCheckJob.Object });
			mockServiceTaskHealthCheckJob.Setup(m => m.FrequencySetting).Returns(HealthCheckConstants.NotificationFrequencyConstants.Periodically);
			mockServiceTaskHealthCheckJob.Setup(m => m.IntervalInMinute).Returns(15);
			mockServiceTaskHealthCheckJob.Setup(m => m.ReportFailuresInEmail).Returns(false);

			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, GlbCompany.CurrentCompany.GC_Code, "ANOTHER_ENTERPRISE", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange.IsValid);
			Assert("Precondition", mockServiceTaskHealthCheckJob.Object.LastReportedTimeForFailedEDIInterchange.IsValid);
			mockServiceTask.Object.RunTask();
			AssertEquals(TestDateAttribute.Date, mockServiceTaskHealthCheckJob.Object.LastCheckedTimeForFailedEDIInterchange);
			AssertContains("Developer generated exception - Failed Interchange Health Check Report", exceptionReporter[0].Message);
			exceptionReporter.Clear();
			TestHelpers.AssertNoErrorEmail();
		}

		static Mock<eHubHealthCheckServiceTask> CreateMockServiceTask()
		{
			var mockServiceTask = new Mock<eHubHealthCheckServiceTask>() { CallBase = true };
			mockServiceTask.Object.Notifier = new NotificationBuffer();
			mockServiceTask.Object.ServiceLogger = new TestServiceLogger();

			return mockServiceTask;
		}

		static void SetValueEHubOutboundFailedEDIInterchangeNotificationFrequencyRegistry(NotificationFrequency setting)
		{
			eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, setting);
		}

		static Mock<eHubOutboundFailedEDIInterchangeCheckJob> CreateMockEHubOutboundFailedEDIInterchangeCheckJob(eHubHealthCheckServiceTask mockServiceTask)
		{
			return new Mock<eHubOutboundFailedEDIInterchangeCheckJob>(mockServiceTask.Notifier) { CallBase = true };
		}

		static Mock<eHubOutboundFailedEDIInterchangeCheckJob> CreateMockEHubOutboundFailedEDIInterchangeCheckJob()
		{
			return new Mock<eHubOutboundFailedEDIInterchangeCheckJob>(new NotificationBuffer()) { CallBase = true };
		}

		static void SetValueEAdaptorOutboundFailedEDIInterchangeNotificationFrequencyRegistry(NotificationFrequency setting)
		{
			eAdaptorRegistry.Instance.eAdaptorFailedEDIInterchangeNotificationFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, setting);
		}

		static Mock<eAdaptorOutboundFailedEDIInterchangeCheckJob> CreateMockEAdaptorOutboundFailedEDIInterchangeCheckJob(eHubHealthCheckServiceTask mockServiceTask)
		{
			return new Mock<eAdaptorOutboundFailedEDIInterchangeCheckJob>(mockServiceTask.Notifier) { CallBase = true };
		}

		static ZGuid CurrentBranchPk => ((IBranch)GlbCompany.CurrentCompany.Branches.First()).PK;
	}
}
