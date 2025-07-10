using System.Collections.Generic;
using System.Linq;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging;
using Moq;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound.Scavenging
{
	[TestedType(typeof(UsageSubmissionFailureResetterTask))]
	class UsageSubmissionFailureResetterTaskTests : eHubServiceTaskTest<UsageSubmissionFailureResetterTask, UsageSubmissionFailureResetterJob>
	{
		protected override Mock<UsageSubmissionFailureResetterTask> CreateMock(ICompanySettingsManager companySettingsManager = null, bool runContinuously = false, bool isProduction = false)
		{
			var mockServiceTask = new Mock<UsageSubmissionFailureResetterTask>() { CallBase = true };
			SetupMock(mockServiceTask, companySettingsManager, runContinuously, isProduction);
			return mockServiceTask;
		}

		protected override Mock<UsageSubmissionFailureResetterJob> StubMockJob(Mock<UsageSubmissionFailureResetterTask> serviceTask)
		{
			var job = new Mock<UsageSubmissionFailureResetterJob>(serviceTask.Object, serviceTask.Object.Notifier, 1000);
			job.Setup(m => m.CanExecute).Returns(true);
			serviceTask.Setup(m => m.GetJobs()).Returns(new[] { job.Object });
			return job;
		}

		[ExpectNoExceptions]
		public void TestDefaultPeriod()
		{
			AssertEquals("1day", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
