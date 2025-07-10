using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;

namespace Enterprise.ServiceManager.Module.Testing
{
	class ServiceTaskNudgerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNudgeServiceTask()
		{
			const string taskCode = "LWK";

			serviceTaskNudger.NudgeServiceTask(taskCode);

			AssertNoExceptionThrown(() =>
			{
				nudgingControllerMock.Verify(controller => controller.ReportNudgeStarted(It.IsAny<IEnumerable<string>>(), It.IsAny<StackTrace>()), Times.Once);
				nudgingControllerMock.Verify(controller => controller.ReportNudgeStarted(It.Is<IEnumerable<string>>(enumerable => enumerable.SequenceEqual(new[] { taskCode })), It.IsAny<StackTrace>()), Times.Once);
				nudgingControllerMock.Verify(controller => controller.ScheduleTasks(It.IsAny<IEnumerable<string>>(), It.IsAny<bool?>(), It.IsAny<TimeSpan?>()), Times.Once);
				nudgingControllerMock.Verify(controller => controller.ScheduleTasks(It.Is<IEnumerable<string>>(enumerable => enumerable.SequenceEqual(new[] { taskCode })), null, null), Times.Once);
			});
		}

		public void TestNudgeServiceTask_Delayed()
		{
			const string taskCode = "LWK";
			var delay = TimeSpan.FromSeconds(5);

			serviceTaskNudger.NudgeServiceTask(taskCode, delay);

			AssertNoExceptionThrown(() =>
			{
				nudgingControllerMock.Verify(controller => controller.ReportNudgeStarted(It.IsAny<IEnumerable<string>>(), It.IsAny<StackTrace>()), Times.Once);
				nudgingControllerMock.Verify(controller => controller.ReportNudgeStarted(It.Is<IEnumerable<string>>(enumerable => enumerable.SequenceEqual(new[] { taskCode })), It.IsAny<StackTrace>()), Times.Once);
				nudgingControllerMock.Verify(controller => controller.ScheduleTasks(It.IsAny<IEnumerable<string>>(), It.IsAny<bool?>(), It.IsAny<TimeSpan?>()), Times.Once);
				nudgingControllerMock.Verify(controller => controller.ScheduleTasks(It.Is<IEnumerable<string>>(enumerable => enumerable.SequenceEqual(new[] { taskCode })), null, delay), Times.Once);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nudgingControllerMock = new Mock<INudgingController>();
			serviceTaskNudger = new ServiceTaskNudger(nudgingControllerMock.Object);
		}

		//Remove the = null! after upgrading from NUnitCore to NUnit4, as CS8618 gets suppressed by NUnit3002 https://docs.nunit.org/articles/nunit-analyzers/NUnit3002.html
		Mock<INudgingController> nudgingControllerMock = null!;
		ServiceTaskNudger serviceTaskNudger = null!;
	}
}
