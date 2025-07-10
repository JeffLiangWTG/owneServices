using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	class TaskInitializationRequirementsCheckerTest : TestCaseWithFactory
	{
		ServiceTaskSchedule SetupAndRunEnsureTaskRequirements(bool canRunInAnyBranch, ZGuid initialBranchGuid, out Mock<IHostLogger> loggerMock)
		{
			// Arrange
			loggerMock = new Mock<IHostLogger>();

			var config = new HostedServiceAttribute("ABC", "ABC Description", "", typeof(object))
			{
				CanRunInAnyBranch = canRunInAnyBranch
			};
			var taskInfo = new ServiceTaskInfo(config);

			var serviceTaskSchedule = Factory.New<ServiceTaskSchedule>();
			serviceTaskSchedule.S5_ScheduleType = "ABC";
			serviceTaskSchedule.S5_GB = initialBranchGuid;
			var tasksGovernor = new SchedulerServiceTaskCollectionGovernor(new[] { serviceTaskSchedule });
			var remover = new TaskInitializationRequirementsChecker(loggerMock.Object, new GlbCompanyProvider());

			// Act
			remover.EnsureTaskRequirementsSatisfied(new[] { new ScheduleWithInfo(taskInfo, tasksGovernor.GovernedTasks.FirstOrDefault()) }, tasksGovernor);

			return serviceTaskSchedule;
		}

		[ExpectNoExceptions]
		public void TestTaskRequirementsSetsBranchWhenNotCanRunInAnyBranch()
		{
			var schedule = SetupAndRunEnsureTaskRequirements(canRunInAnyBranch: false, ZGuid.Empty, out var loggerMock);

			// Assert
			var expectedWarning = "ABC - 'ABC Description' had missing branch assignment and requires a branch. Branch assignment was set to EDIHQ as the branch satisfies task requirements";
			loggerMock.Verify(x => x.Log(LogLevel.Information, It.Is<string>(message => message.Contains(expectedWarning))), Times.Once);
			NUnit.Framework.Assert.That(ZGuid.Empty, Is.Not.EqualTo(schedule.S5_GB));
		}

		[ExpectNoExceptions]
		public void TestTaskRequirementsNullsBranchWhenCanRunInAnyBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var schedule = SetupAndRunEnsureTaskRequirements(canRunInAnyBranch: true, branch.PK, out var loggerMock);

			// Assert
			var expectedWarning = "ABC - 'ABC Description' had non null branch assignment. Branch assignment was set to null as the task can run in any branch";
			loggerMock.Verify(x => x.Log(LogLevel.Information, It.Is<string>(message => message.Contains(expectedWarning))), Times.Once);
			NUnit.Framework.Assert.That(ZGuid.Empty, Is.EqualTo(schedule.S5_GB));
		}

		[ExpectNoExceptions]
		public void TestTaskRequirementsLeavesBranchNullWhenCanRunInAnyBranch()
		{
			var schedule = SetupAndRunEnsureTaskRequirements(canRunInAnyBranch: true, ZGuid.Empty, out var loggerMock);

			// Assert
			loggerMock.Verify(x => x.Log(LogLevel.Information, It.IsAny<string>()), Times.Never);
			NUnit.Framework.Assert.That(ZGuid.Empty, Is.EqualTo(schedule.S5_GB));
		}

		[ExpectNoExceptions]
		public void TestTaskRequirementsLeavesBranchSetWhenNotCanRunInAnyBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var schedule = SetupAndRunEnsureTaskRequirements(canRunInAnyBranch: false, branch.PK, out var loggerMock);

			// Assert
			loggerMock.Verify(x => x.Log(LogLevel.Information, It.IsAny<string>()), Times.Never);
			NUnit.Framework.Assert.That(branch.PK, Is.EqualTo(schedule.S5_GB));
		}

		public void TestRemoveTasksLogging()
		{
			var config = new HostedServiceAttribute("ABC", "ABC Description", "", typeof(object));
			var task = new ServiceTaskInfo(config);
			var loggerMock = new Mock<IHostLogger>();

			var remover = new TaskInitializationRequirementsChecker(loggerMock.Object, new GlbCompanyProvider());
			remover.EnsureTaskRequirementsSatisfied(new[] { new ScheduleWithInfo(task, null) }, Mock.Of<IServiceTaskCollectionGovernor>());

			var schedule = TaskSchedulerTest.CreateSchedule("ABC", Factory);
			var brokenConfig = new HostedServiceAttribute() { TypeName = "foo", Code = "ABC", Description = "ABC Description", TypeAssemblyName = "foo.dll" };
			var brokenTask = new ServiceTaskInfo(brokenConfig);
			var tasksGovernor = new SchedulerServiceTaskCollectionGovernor(new[] { schedule });
			remover.EnsureTaskRequirementsSatisfied(new[] { new ScheduleWithInfo(brokenTask, tasksGovernor.GovernedTasks.FirstOrDefault()) }, tasksGovernor);

			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(x =>
					x.Log(LogLevel.Debug, It.Is<string>(message => message.Contains("Service task: ABC - 'ABC Description' has been added to the system"))),
					Times.Once);
				loggerMock.Verify(x =>
					x.Log(
						LogLevel.Warning,
						It.Is<string>(message => message.Contains("Task ABC will be deactivated. The following configuration errors need to be resolved for the task to be re-activated on the next process controller restart:\r\n\tThe service task type has been configured incorrectly."))),
					Times.Once);
			});
		}

		public void TestTaskCannotBeAddedLogging()
		{
			var config = new HostedServiceAttribute() { TypeName = "foo", Code = "XYZ", Description = "XYZ Description", TypeAssemblyName = "foo.dll" };
			var task = new ServiceTaskInfo(config);
			var loggerMock = new Mock<IHostLogger>();
			var checker = new TaskInitializationRequirementsChecker(loggerMock.Object, new GlbCompanyProvider());
			checker.EnsureTaskRequirementsSatisfied(new[] { new ScheduleWithInfo(task, null) }, Mock.Of<IServiceTaskCollectionGovernor>());

			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(x =>
					x.Log(
						LogLevel.Warning,
						It.Is<string>(message => message.Contains("The Task XYZ cannot be added to the system until the following configuration errors have been addressed and the process controller is restarted:\r\n\tThe service task type has been configured incorrectly."))),
					Times.Once);
			});
		}

		[ExpectNoExceptions]
		public void TestTaskRequirementCheckTimedOutDoesNotBlockOtherTasks()
		{
			// Arrange
			var fastConfig = new HostedServiceAttribute("ABC", "ABC Description", "", typeof(object));
			var fastTask = new ServiceTaskInfo(fastConfig);

			var blockingConfig = new HostedServiceAttribute("ZZZ", "ZZZ Description", "", typeof(TaskForTestingWithBlockingRequirement));
			var blockingTask = new ServiceTaskInfo(blockingConfig);

			var loggerMock = new Mock<IHostLogger>();
			var allTasks = new[] {
				new ScheduleWithInfo(fastTask, null),
				new ScheduleWithInfo(blockingTask, null)
			};

			var requirementsChecker = new TaskInitializationRequirementsChecker(loggerMock.Object, new GlbCompanyProvider());

			using (SystemDataRegistry.Instance.ServiceTaskRequirementCheckTimeLimitSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				// Act
				var result = requirementsChecker.EnsureTaskRequirementsSatisfied(allTasks, Mock.Of<IServiceTaskCollectionGovernor>());

				// Assert
				NUnit.Framework.Assert.That(result, Is.EquivalentTo(new[] { new ScheduleWithInfo(fastTask, null) }));
			}

			NUnit.Framework.Assert.That(ExceptionReporterTestListener.Instance.Count, Is.EqualTo(1));
			NUnit.Framework.Assert.That(ExceptionReporterTestListener.Instance[0].Message, Does.Contain($@"Service task {blockingConfig.Code} ({blockingConfig.Description}) timed out on the {typeof(TaskForTestingWithBlockingRequirement).GetMethods()[0].Name} requirement check (5s limit)."));
			ExceptionReporterTestListener.Instance.Clear();
		}

		[ExpectNoExceptions]
		public void TestTaskRequirementCheckTimedOutStopsChecking()
		{
			// Arrange
			var blockingConfig = new HostedServiceAttribute("ZZZ", "ZZZ Description", "", typeof(TaskForTestingWithBlockingRequirementAndFailingRequirement));
			var blockingTask = new ServiceTaskInfo(blockingConfig);

			var loggerMock = new Mock<IHostLogger>();
			var allTasks = new[] {
				new ScheduleWithInfo(blockingTask, null)
			};

			var requirementsChecker = new TaskInitializationRequirementsChecker(loggerMock.Object, new GlbCompanyProvider());

			using (SystemDataRegistry.Instance.ServiceTaskRequirementCheckTimeLimitSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				// Act
				var result = requirementsChecker.EnsureTaskRequirementsSatisfied(allTasks, Mock.Of<IServiceTaskCollectionGovernor>());

				// Assert
				NUnit.Framework.Assert.That(result.IsNullOrEmpty(), Is.True);
			}

			NUnit.Framework.Assert.That(ExceptionReporterTestListener.Instance.Count, Is.GreaterThanOrEqualTo(2));
			NUnit.Framework.Assert.That(ExceptionReporterTestListener.Instance[0].Message, Does.Contain($@"Service task {blockingConfig.Code} ({blockingConfig.Description}) timed out on the {typeof(TaskForTestingWithBlockingRequirement).GetMethods()[0].Name} requirement check (5s limit)."));
			NUnit.Framework.Assert.That(ExceptionReporterTestListener.Instance.Select(e => e.Message), Does.Contain("Operation is not valid due to the current state of the object."));
			ExceptionReporterTestListener.Instance.Clear();
		}

		[ExpectNoExceptions]
		public void TestTaskRequirementLongRunningCheckCompletesBeforeTimeout()
		{
			// Arrange
			var slowConfig = new HostedServiceAttribute("ABC", "ABC Description", "", typeof(TaskForTestingWithBlockingRequirement));
			var slowTask = new ServiceTaskInfo(slowConfig);

			var loggerMock = new Mock<IHostLogger>();
			var allTasks = new[] {
				new ScheduleWithInfo(slowTask, null)
			};

			var requirementsChecker = new TaskInitializationRequirementsChecker(loggerMock.Object, new GlbCompanyProvider());

			using (SystemDataRegistry.Instance.ServiceTaskRequirementCheckTimeLimitSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 20))
			{
				// Act
				var result = requirementsChecker.EnsureTaskRequirementsSatisfied(allTasks, Mock.Of<IServiceTaskCollectionGovernor>());

				// Assert
				NUnit.Framework.Assert.That(result, Is.EquivalentTo(new[] { new ScheduleWithInfo(slowTask, null) }));
			}
		}

		public void TestTaskRequirementTimedOutCheckLogAndReportAreCorrect()
		{
			// Arrange
			var timeout = TimeSpan.FromSeconds(5);
			var slowConfig = new HostedServiceAttribute("SLO", "SLO Description", "", typeof(TaskForTestingWithBlockingRequirement));
			var slowTask = new ServiceTaskInfo(slowConfig);
			var loggerMock = new Mock<IHostLogger>();
			var checker = new TaskInitializationRequirementsChecker(loggerMock.Object, new GlbCompanyProvider());
			var errorReporter = new Mock<IErrorReporter>();

			var timeLimitError = $"Time limit ({timeout.TotalSeconds}s) exceeded while checking requirement method BlockingRequirementCheck.";
			var expectedLogMessage = "The Task SLO cannot be added to the system until the following configuration errors have been addressed and the process controller is restarted:\r\n\t" + timeLimitError;
			var expectedErrorMessage = "Service task SLO: " + timeLimitError;

			using (SystemDataRegistry.Instance.ServiceTaskRequirementCheckTimeLimitSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, timeout.Seconds))
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporter.Object))
			{
				// Act
				checker.EnsureTaskRequirementsSatisfied(new[] { new ScheduleWithInfo(slowTask, null) }, Mock.Of<IServiceTaskCollectionGovernor>());

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock.Verify(x =>
							x.Log(LogLevel.Warning, expectedLogMessage),
						Times.Once);

					errorReporter.Verify(x =>
							x.ReportDeveloperExceptionOrHandleSilently(
								It.IsAny<string>(),
								expectedErrorMessage,
								It.IsAny<ServiceTaskRequirementCheckTimeoutException>()),
						Times.Once);
				});
			}
		}

		class TaskForTestingWithBlockingRequirement
		{
			[HostedServiceRequirements]
			public static string[] BlockingRequirementCheck()
			{
				Thread.Sleep(TimeSpan.FromSeconds(10));

				return Array.Empty<string>();
			}
		}

		class TaskForTestingWithBlockingRequirementAndFailingRequirement
		{
			[HostedServiceRequirements]
			public static string[] BlockingRequirementCheck()
			{
				Thread.Sleep(TimeSpan.FromSeconds(10));

				return Array.Empty<string>();
			}

			[HostedServiceRequirements]
			public static string[] FastRequirementCheck()
			{
				throw new InvalidOperationException();
			}
		}
	}
}
