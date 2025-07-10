using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(ScheduledReportRunner))]
	sealed class ScheduledReportRunnerTest : ServiceTaskTestCase<ScheduledReportRunner>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "SRR", hostedServiceAttribute.Code);
				AssertEquals("Description", "Scheduled Report Runner", hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", true, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals(typeof(SRRSpecificValidation), hostedServiceAttribute.TaskSpecificValidationType);
			});
		}

		[TestDate(2021, 08, 27)]
		public void TestGetCountOfPendingTasks()
		{
			var task = Factory.New<DummyStmScheduleTask>();
			task.S5_ParentTableCode = StmMenuItemSchema.Constants.Prefix;
			var timeAgo = TimeSpan.FromHours(1);
			task.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow - timeAgo;
			task.S5_IsActive = true;
			task.S5_ParentID = Guid.NewGuid();

			Factory.Save();

			var hostedServiceQueueProvider = new ScheduledReportRunner() as IHostedServiceQueueProvider;
			var result = hostedServiceQueueProvider.QueueResult;
			AssertEquals(1, result.QueueSize);

			var expectedSecondsAgo = (DateTime.UtcNow - TestDateAttribute.Date + timeAgo).TotalSeconds;
			AssertCloseEnough(((int)expectedSecondsAgo), ((int)result.MaximumItemAge.TotalSeconds), 60);
		}

		[TestDate(2005, 1, 4)]
		public void TestScheduledReportRunner_CanRunInAnyBranch_UseDisposableEnvironment()
		{
			var mockScheduleTaskRunner = new Mock<IScheduleTaskRunner>();
			_ = mockScheduleTaskRunner
				.Setup(runner => runner.Process(It.IsAny<ZString>(), It.IsAny<bool>(), It.IsAny<INotifications>(), It.IsAny<CancellationToken>()))
				.Callback(() => _ = Env.CurrentBranch);

			using (ObjectFactory.Substitute(mockScheduleTaskRunner.Object))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("SRR", canRunInAnyBranch: true))
			{
				var scheduledReportRunner = new ScheduledReportRunner()
				{
					ServiceLogger = new TestServiceLogger()
				};
				scheduledReportRunner.RunTask();
			}

			AssertNotContains("Service Task: SRR accesses environment current branch without setting the environment first.", ErrorReporter.LastMessageReported);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
