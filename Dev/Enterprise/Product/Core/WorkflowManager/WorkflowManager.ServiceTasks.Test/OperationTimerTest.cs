using System;
using Moq;
using NUnit.Framework;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	class OperationTimerTests : TestCase
	{
		[ExpectNoExceptions]
		public void TestExceedsTimeLimitActionIsCalled()
		{
			var actionMock = new Mock<Action<long>>();
			var stopwatchMock = new Mock<IStopwatch>();
			stopwatchMock.Setup(s => s.ElapsedMilliseconds).Returns(200);

			var timeLimit = 100;

			using (var timer = new OperationTimer(timeLimit, actionMock.Object, stopwatchMock.Object))
			{
			}

			actionMock.Verify(action => action(It.IsAny<long>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestDoesNotExceedTimeLimitActionIsNotCalled()
		{
			var actionMock = new Mock<Action<long>>();
			var stopwatchMock = new Mock<IStopwatch>();
			stopwatchMock.Setup(s => s.ElapsedMilliseconds).Returns(50);

			var timeLimit = 100;

			using (var timer = new OperationTimer(timeLimit, actionMock.Object, stopwatchMock.Object))
			{
			}

			actionMock.Verify(action => action(It.IsAny<long>()), Times.Never);
		}
	}
}
