using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host.Testing.Core.RunnableTask;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;
using WTG.NUnit;

namespace Enterprise.ServiceManager.Host.Testing
{
	sealed class InitializationTaskRunnerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestRunNudgeableTasksRequiringScheduleInitialization()
		{
			using (ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(provider =>
				provider.BusinessObjectBindings == new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("222", "DummyBizo2", Array.Empty<string>(), null),
				})))
			{
				var schedule1 = TaskSchedulerTest.CreateSchedule("111", Factory, true);
				var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule1));

				var schedule2 = TaskSchedulerTest.CreateSchedule("222", Factory, false);
				var task2 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule2));

				var activeNudgeableTask = new DirectTaskRunRequest(task1, echoes: false);
				var inactiveNudgeableTask = new DirectTaskRunRequest(task2, echoes: false);

				var allTasks = new List<IRunnableServiceTask> { activeNudgeableTask.Task, inactiveNudgeableTask.Task };
				var loggerMock = new Mock<IHostLogger>();

				var procRunPool = new Mock<IProcessRunnerPool>();
				procRunPool
					.Setup(prp => prp.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(new Mock<IServiceRunner>().Object);

				var resThrottle = new Mock<IResourceThrottler>();
				resThrottle.Setup(rt => rt.WaitForResource())
					.Returns(new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, new TimeSpan(1),
						new decimal(0), new decimal(0), new decimal(0)));

				var taskQueue = new Mock<ITaskQueue>();
				var initializer = new InitializationTaskRunner(loggerMock.Object, taskQueue.Object);

				var schemaResolver = new Mock<IApplicationSchemaResolver>();
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo2")).Returns(DummyBizoSchema.Instance);

				NUnit.Framework.Assert.That(schedule1.ServiceTaskBindingsCount, NUnit.Framework.Is.GreaterThan(0).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(schedule2.ServiceTaskBindingsCount, NUnit.Framework.Is.GreaterThan(0).Using(CustomComparers.TypeComparison));

				var serviceTaskBusinessObjectBindingEnabled = SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled;
				var tasksWithBizOBindingAttributes = HostedServiceBusinessObjectBindingsProvider.Instance.BusinessObjectBindings.Select(taskBindingAttribute =>
					allTasks.FirstOrDefault(t => t.HasSchedule && t.Code == taskBindingAttribute.ServiceTaskCode)).Where(task => task != null && task.IsActive).Distinct();

				try
				{
					SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = true;

					// Act
					initializer.EnqueueNudgeableTasks(allTasks, HostedServiceBusinessObjectBindingsProvider.Instance.BusinessObjectBindings);

					// Assert
					taskQueue.Verify(tr => tr.EnqueueTask(activeNudgeableTask), Times.Once());
					taskQueue.Verify(tr => tr.EnqueueTask(inactiveNudgeableTask), Times.Never());
					loggerMock.Verify(x =>
							x.LogSection(LogLevel.Information, "Creating Nudge run requests for service tasks with business object binding.", "Nudge run requests for service tasks are created and enqueued."),
						Times.Exactly(tasksWithBizOBindingAttributes.Count()));
				}
				finally
				{
					SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = serviceTaskBusinessObjectBindingEnabled;
				}
			}
		}

		public class LogMessageTest : TestCase
		{
			[ExpectNoExceptions]
			public void TestEnqueueingNudgeableTasks()
			{
				// Arrange
				taskMockList.ForEach(task =>
				{
					task
						.SetupGet(x => x.HasSchedule)
						.Returns(true);
					task
						.SetupGet(x => x.IsActive)
						.Returns(true);
				});

				var bindingAttribute = codes
					.Select(code => new HostedServiceBusinessObjectBindingAttribute(code, string.Empty, Array.Empty<string>(), string.Empty));

				// Act
				runner.EnqueueNudgeableTasks(taskList, bindingAttribute);

				// Assert
				logger.Verify(x => x.LogSection(LogLevel.Information, "Creating Nudge run requests for service tasks with business object binding.", "Nudge run requests for service tasks are created and enqueued."), Times.Once);
				logger.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"\\[{codes[0]}/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}\\] Nudge run request is created.")), Times.Once);
				logger.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"\\[{codes[1]}/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}\\] Nudge run request is created.")), Times.Once);
				logger.VerifyNoOtherCalls();
			}

			[ExpectNoExceptions]
			public void TestEnqueueingAlwaysRunTasks()
			{
				// Arrange
				var serviceTaskInfo = new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(x => x.AlwaysRunAtStartup));

				taskMockList.ForEach(task =>
				{
					task
						.SetupGet(x => x.HasSchedule)
						.Returns(true);
					task
						.SetupGet(x => x.IsActive)
						.Returns(true);
					task
						.SetupGet(x => x.Info)
						.Returns(serviceTaskInfo);
				});

				// Act
				runner.EnqueueTasksThatAlwaysRunOnStartup(taskList);

				// Assert
				logger.Verify(x => x.LogSection(LogLevel.Information, "Creating Nudge run requests for tasks requiring to be launched on startup.", "Nudge run requests for tasks requiring to be launched on startup are created and enqueued."), Times.Once);
				logger.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"\\[{codes[0]}/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}\\] Nudge run request is created.")), Times.Once);
				logger.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"\\[{codes[1]}/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}\\] Nudge run request is created.")), Times.Once);
				logger.VerifyNoOtherCalls();
			}

			protected override void SetUp()
			{
				base.SetUp();
				logger = new Mock<IHostLogger>();
				sequence = new MockSequence();
				runner = new InitializationTaskRunner(logger.Object, Mock.Of<ITaskQueue>());

				logger
					.InSequence(sequence)
					.Setup(x => x.LogSection(LogLevel.Information, It.IsAny<string>(), It.IsAny<string>()));
				logger
					.InSequence(sequence)
					.Setup(x => x.Log(LogLevel.Information, It.IsAny<string>()));

				taskMockList = codes
					.Select(code =>
					{
						var task = new Mock<IRunnableServiceTask>();
						task
							.Setup(y => y.Code)
							.Returns(code);

						return task;
					})
					.ToList();

				taskList = taskMockList
					.Select(x => x.Object)
					.ToList();
			}

			readonly string[] codes = new[] { "CODE1", "CODE2" };
			Mock<IHostLogger> logger;
			List<Mock<IRunnableServiceTask>> taskMockList;
			List<IRunnableServiceTask> taskList;
			MockSequence sequence;
			InitializationTaskRunner runner;
		}
	}
}
