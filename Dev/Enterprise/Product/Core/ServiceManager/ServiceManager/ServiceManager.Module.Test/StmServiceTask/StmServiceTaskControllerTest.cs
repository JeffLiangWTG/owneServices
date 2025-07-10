using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Module.ScheduleNow;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Module.Testing
{
	[TestedType(typeof(StmServiceTaskController))]
	class StmServiceTaskControllerTest : ZControllerBasherTest
	{
		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestViewForm()
		{
			Assert(true);
		}

		public void TestActivateDeactivate()
		{
			AssertControllerNotNull();

			var serviceTaskScheduleCollection = new StmServiceTaskCollection(Factory);

			var task1a = serviceTaskScheduleCollection.AddNew();
			task1a.NextRunTimeCalculator = new NextRunTimeCalculatorDays() { Period = 1 };
			task1a.SST_Active = true;
			task1a.SST_ServiceTaskCode = "T1A";
			task1a.SST_NextRunTime = nextRunTime;

			var task1b = serviceTaskScheduleCollection.AddNew();
			task1b.NextRunTimeCalculator = new NextRunTimeCalculatorDays() { Period = 1 };
			task1b.SST_Active = true;
			task1b.SST_ServiceTaskCode = "T1B";
			task1b.SST_NextRunTime = nextRunTime;

			var task1c = serviceTaskScheduleCollection.AddNew();
			task1c.NextRunTimeCalculator = new NextRunTimeCalculatorDays() { Period = 1 };
			task1c.SST_Active = true;
			task1c.SST_ServiceTaskCode = "T1C";
			task1c.SST_NextRunTime = nextRunTime;

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			((StmServiceTaskController)Controller).ActivateDeactivate(new BusinessObject[] { task1a, task1c }, false);
			AssertEquals("Do you really want to deactivate the selected 2 task(s)?", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(task1a.SST_Active);
			Assert(task1b.SST_Active);
			Assert(task1c.SST_Active);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			((StmServiceTaskController)Controller).ActivateDeactivate(new BusinessObject[] { task1a, task1c }, false);
			AssertEquals("Do you really want to deactivate the selected 2 task(s)?", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(!task1a.SST_Active);
			Assert(task1b.SST_Active);
			Assert(!task1c.SST_Active);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			((StmServiceTaskController)Controller).ActivateDeactivate(new BusinessObject[] { task1a, task1b }, true);
			AssertEquals("Do you really want to activate the selected 2 task(s)?", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(!task1a.SST_Active);
			Assert(task1b.SST_Active);
			Assert(!task1c.SST_Active);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			((StmServiceTaskController)Controller).ActivateDeactivate(new BusinessObject[] { task1a, task1b }, true);
			AssertEquals("Do you really want to activate the selected 2 task(s)?", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(task1a.SST_Active);
			Assert(task1b.SST_Active);
			Assert(!task1c.SST_Active);
		}

		public void TestActivateShouldShowErrorWhenNoTasksSelected()
		{
			((StmServiceTaskController)Controller).ActivateDeactivate(Array.Empty<BusinessObject>(), true);
			AssertEquals("There are no Tasks Selected.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[TestDate(2025, 1, 1)]
		public void TestNextRunTimeIsUpdatedWhenTaskIsActivated()
		{
			// Arrange
			CombineAssertions(() =>
			{
				Test(false, ZDateTime.Now.AddYears(-1), true, "T1A");
				Test(false, ZDateTime.Now.AddDays(2), true, "T2A");
				Test(true, ZDateTime.Now.AddDays(-1), false, "T3A");
			});

			void Test(bool isActive, ZDateTime nextRunTime, bool nextRunTimeShouldUpdate, string serviceTaskCode)
			{
				var serviceTaskScheduleCollection = new StmServiceTaskCollection(Factory);
				var task = serviceTaskScheduleCollection.AddNew();
				task.SST_Active = isActive;
				task.SST_ServiceTaskCode = serviceTaskCode;
				task.NextRunTimeCalculator = new NextRunTimeCalculatorDays() { Period = 1 };
				task.SST_NextRunTime = nextRunTime.ToDateTime();
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				// Act
				((StmServiceTaskController)Controller).ActivateDeactivate(new BusinessObject[] { task }, !isActive);

				// Assert
				AssertEquals(!isActive, task.SST_Active);
				if (nextRunTimeShouldUpdate)
				{
					AssertNotEquals(nextRunTime, task.SST_NextRunTime.ToDateTime());
				}
				else
				{
					AssertEquals(nextRunTime, task.SST_NextRunTime.ToDateTime());
				}
			}
		}

		class ScheduleNowTest : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				base.SetUp();
				stmServiceTaskController = new StmServiceTaskController();
				nextRunTime = ZDateTimeOffset.Now;
			}

			public void TestListOfTasksWhichIncludesUpgServiceTask()
			{
				var serviceTaskScheduleCollection = new StmServiceTaskCollection(Factory);

				var taskUpgarde = serviceTaskScheduleCollection.AddNew();
				taskUpgarde.SST_ServiceTaskCode = "UPG";
				taskUpgarde.SST_NextRunTime = nextRunTime;

				Factory.Save();

				CombineAssertions(() =>
				{
					Test(DialogResult.OK, Times.Once(), "T01");
					Test(DialogResult.Cancel, Times.Never(), "T02");
					Test(DialogResult.No, Times.Never(), "T03");
				});

				void Test(DialogResult dialogResult, Times times, string code1)
				{
					// Arrange

					var task1a = serviceTaskScheduleCollection.AddNew();
					task1a.SST_ServiceTaskCode = code1;
					task1a.SST_NextRunTime = nextRunTime;

					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(dialogResult);

					var scheduleControllerMock = new Mock<IScheduleNowController>();

					// Act
					stmServiceTaskController.ScheduleNow(new BusinessObject[] { task1a, taskUpgarde }, scheduleControllerMock.Object, forceRestart: false);

					// Assert
					AssertEquals("One of the selected tasks is System Upgrade service task. Triggering this task may cause the system to be upgraded to a new version. Do you really want to continue? Type 'yes' to proceed.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					scheduleControllerMock.Verify(controller => controller.ScheduleNow(It.IsAny<IEnumerable<string>>(), It.IsAny<Action<string>>(), It.IsAny<bool>()), times);
				}
			}

			public void TestListOfTasksWhichDoesNotIncludeUpgServiceTask()
			{
				CombineAssertions(() =>
				{
					Test(DialogResult.Yes, Times.Once(), "T01", "T02");
					Test(DialogResult.Cancel, Times.Never(), "T03", "T04");
					Test(DialogResult.No, Times.Never(), "T05", "T06");
				});

				void Test(DialogResult dialogResult, Times times, string code1, string code2)
				{
					// Arrange
					var serviceTaskScheduleCollection = new StmServiceTaskCollection(Factory);

					var task1a = serviceTaskScheduleCollection.AddNew();
					task1a.SST_ServiceTaskCode = code1;
					task1a.SST_NextRunTime = nextRunTime;

					var task1b = serviceTaskScheduleCollection.AddNew();
					task1b.SST_ServiceTaskCode = code2;
					task1b.SST_NextRunTime = nextRunTime;

					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(dialogResult);

					var scheduleControllerMock = new Mock<IScheduleNowController>();

					// Act
					stmServiceTaskController.ScheduleNow(new BusinessObject[] { task1a, task1b }, scheduleControllerMock.Object, forceRestart: false);

					// Assert
					AssertEquals("Do you really want to schedule the selected 2 task(s)?", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					scheduleControllerMock.Verify(controller => controller.ScheduleNow(It.IsAny<IEnumerable<string>>(), It.IsAny<Action<string>>(), It.IsAny<bool>()), times);
				}
			}

			public void TestShowsErrorWhenNoTasksSelected()
			{
				// Arrange
				var scheduleControllerMock = new Mock<IScheduleNowController>();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				// Act
				stmServiceTaskController.ScheduleNow(Array.Empty<BusinessObject>(), scheduleControllerMock.Object, forceRestart: false);

				// Assert
				AssertEquals("Failed to Schedule Tasks:\r\n\r\nThere are no Tasks Selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				scheduleControllerMock.Verify(controller => controller.ScheduleNow(It.IsAny<IEnumerable<string>>(), It.IsAny<Action<string>>(), It.IsAny<bool>()), Times.Never);
			}

			public void TestProvidesExpectedValuesToController()
			{
				// Arrange
				var serviceTaskScheduleCollection = new StmServiceTaskCollection(Factory);

				var expectedTaskCodes = new[] { "T1A", "T1B", "T2C" };
				var tasks = expectedTaskCodes
					.Select(s =>
					{
						var task = serviceTaskScheduleCollection.AddNew();
						task.SST_ServiceTaskCode = s;
						task.SST_NextRunTime = nextRunTime;
						return task;
					})
					.Cast<BusinessObject>()
					.ToArray();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				IEnumerable<string> taskCodes = null;
				var scheduleControllerMock = new Mock<IScheduleNowController>();
				scheduleControllerMock
					.Setup(controller => controller.ScheduleNow(It.IsAny<IEnumerable<string>>(), It.IsAny<Action<string>>(), It.IsAny<bool>()))
					.Callback<IEnumerable<string>, Action<string>, bool>((enumerable, action, forceRestart) => taskCodes = enumerable.ToList());

				// Act
				stmServiceTaskController.ScheduleNow(tasks, scheduleControllerMock.Object, forceRestart: false);

				// Assert
				scheduleControllerMock.Verify(controller => controller.ScheduleNow(It.IsAny<IEnumerable<string>>(), It.IsAny<Action<string>>(), It.IsAny<bool>()), Times.Once);
				AssertContainsExactElementsInAnyOrder(expectedTaskCodes, taskCodes);
			}

			public void TestShowsTheResult()
			{
				CombineAssertions(() =>
				{
					Test("result1", "T01");
					Test("result2", "T02");
				});

				void Test(string result, string code)
				{
					// Arrange
					var serviceTaskScheduleCollection = new StmServiceTaskCollection(Factory);

					var task = serviceTaskScheduleCollection.AddNew();
					task.SST_ServiceTaskCode = code;
					task.SST_NextRunTime = nextRunTime;
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var scheduleControllerMock = new Mock<IScheduleNowController>();
					scheduleControllerMock
						.Setup(controller => controller.ScheduleNow(It.IsAny<IEnumerable<string>>(), It.IsAny<Action<string>>(), It.IsAny<bool>()))
						.Callback<IEnumerable<string>, Action<string>, bool>((tasks, action, forceRestart) => action(result));

					// Act
					stmServiceTaskController.ScheduleNow(new BusinessObject[] { task }, scheduleControllerMock.Object, forceRestart: false);

					// Assert
					AssertEquals(result, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			public void TestWrongParamsCall()
			{
				var scheduleControllerMock = new Mock<IScheduleNowController>();
				CombineAssertions(() =>
				{
					var result = AssertExceptionThrown<ArgumentNullException>(() => stmServiceTaskController.ScheduleNow(null, scheduleControllerMock.Object, forceRestart: false));
					AssertEquals("selectedElements", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => stmServiceTaskController.ScheduleNow(Enumerable.Empty<BusinessObject>(), null, forceRestart: false));
					AssertEquals("scheduleNowController", result.ParamName);
				});
			}

			StmServiceTaskController stmServiceTaskController;
			ZDateTimeOffset nextRunTime;
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(StmServiceTask);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var task = Factory.NewWithValidTestData<StmServiceTask>();
			Factory.Save();
			return task;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.StmServiceTask;
		}

		protected override void SetUp()
		{
			base.SetUp();
			nextRunTime = ZDateTimeOffset.Now;
			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);
			hostedServiceAttributeProviderDisposable = ObjectFactory.Substitute(hostedServiceProviderMock.Object);
		}

		protected override void TearDown()
		{
			hostedServiceAttributeProviderDisposable.Dispose();
			base.TearDown();
		}

		ZDateTimeOffset nextRunTime;
		IDisposable hostedServiceAttributeProviderDisposable;
	}
}
