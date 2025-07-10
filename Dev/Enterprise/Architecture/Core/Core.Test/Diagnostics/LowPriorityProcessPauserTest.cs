using System;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class LowPriorityProcessPauserTest : TestCase
	{
		public void TestWait_LogsDebugAfter5Seconds()
		{
			// Arrange
			backlogInfoProviderMock.SetupGet(m => m.Timespans).Returns(new[] { TimeSpan.FromSeconds(3) });
			backlogInfoProviderMock.SetupGet(m => m.AcceptableBacklog).Returns(10);
			var unsuccessfulAttempt = new AttemptInGettingBacklog(true, string.Empty, new BacklogResult { BacklogDescription = "test", BacklogSize = 11 });
			var successfulAttempt = new AttemptInGettingBacklog(true, string.Empty, new BacklogResult { BacklogDescription = "test", BacklogSize = 9 });
			backlogInfoProviderMock
				.SetupSequence(m => m.GetCurrentBacklog())
				.Returns(unsuccessfulAttempt) // Initial check
				.Returns(unsuccessfulAttempt) // Check after 3 seconds is unsuccessful
				.Returns(unsuccessfulAttempt) // Check after 6 seconds is unsuccessful - should log
				.Returns(successfulAttempt) // Check after 9 seconds is successful
				.Returns(successfulAttempt);

			// Act
			lowPriorityProcessPauser.Wait(loggerMock.Object);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Once);
				loggerMock.Verify(m => m.Log(LogType.Debug, It.Is<string>(s => s.StartsWith("Waiting for test backlog to clear. Total wait time = 00:00:06") && s.Contains(", next wait time = 00:00:03."))), Times.Once);
			});
		}

		public void TestWait_LogsInfoAfter10Seconds()
		{
			// Arrange
			backlogInfoProviderMock.SetupGet(m => m.Timespans).Returns(new[] { TimeSpan.FromSeconds(3.5) });
			backlogInfoProviderMock.SetupGet(m => m.AcceptableBacklog).Returns(10);
			var unsuccessfulAttempt = new AttemptInGettingBacklog(true, string.Empty, new BacklogResult { BacklogDescription = "test", BacklogSize = 11 });
			var successfulAttempt = new AttemptInGettingBacklog(true, string.Empty, new BacklogResult { BacklogDescription = "test", BacklogSize = 9 });
			backlogInfoProviderMock
				.SetupSequence(m => m.GetCurrentBacklog())
				.Returns(unsuccessfulAttempt) // Initial check
				.Returns(unsuccessfulAttempt) // Check after 3.5 seconds is unsuccessful
				.Returns(unsuccessfulAttempt) // Check after 7 seconds is unsuccessful - should log debug
				.Returns(unsuccessfulAttempt) // Check after 10.5 seconds is unsuccessful - should log info
				.Returns(successfulAttempt) // Check after 14 seconds is successful
				.Returns(successfulAttempt);

			// Act
			lowPriorityProcessPauser.Wait(loggerMock.Object);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Exactly(2));
				loggerMock.Verify(m => m.Log(LogType.Debug, It.Is<string>(s => s.StartsWith("Waiting for test backlog to clear. Total wait time = 00:00:07") && s.Contains(", next wait time = 00:00:03."))), Times.Once);
				loggerMock.Verify(m => m.Log(LogType.Information, It.Is<string>(s => s.StartsWith("Waiting for test backlog to clear. Total wait time = 00:00:10") && s.Contains(", next wait time = 00:00:03."))), Times.Once);
			});
		}

		public void TestWait_LogsWarningAndThrowsAfterMaxWaitTime()
		{
			// Arrange
			backlogInfoProviderMock.SetupGet(m => m.Timespans).Returns(new[] { TimeSpan.FromSeconds(5) });
			backlogInfoProviderMock.SetupGet(m => m.AcceptableBacklog).Returns(10);
			var unsuccessfulAttempt = new AttemptInGettingBacklog(true, string.Empty, new BacklogResult { BacklogDescription = "test", BacklogSize = 11 });
			backlogInfoProviderMock.Setup(m => m.GetCurrentBacklog()).Returns(unsuccessfulAttempt);

			// Act
			AssertExceptionThrown<BacklogWaiterTimeoutException>(() =>
				lowPriorityProcessPauser.Wait(loggerMock.Object, TimeSpan.FromSeconds(2)));

			// Assert
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Once);
				loggerMock.Verify(m => m.Log(LogType.Warning, It.Is<string>(s => s.StartsWith("Gave up waiting. Total wait time = 00:00:02."))), Times.Once);
			});
		}

		public void TestWait_NoExceptionIfLogsIsNull()
		{
			// Arrange
			backlogInfoProviderMock.SetupGet(m => m.Timespans).Returns(new[] { TimeSpan.FromSeconds(1) });
			backlogInfoProviderMock.SetupGet(m => m.AcceptableBacklog).Returns(10);
			var unsuccessfulAttempt = new AttemptInGettingBacklog(true, string.Empty, new BacklogResult { BacklogDescription = "test", BacklogSize = 11 });
			backlogInfoProviderMock.Setup(m => m.GetCurrentBacklog()).Returns(unsuccessfulAttempt);

			// Act
			// Assert
			var exception = AssertExceptionThrown<BacklogWaiterTimeoutException>(() => lowPriorityProcessPauser.Wait(null, TimeSpan.FromSeconds(3)));
			loggerMock.Verify(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never);
			AssertContains("IBacklogInfoProviderProxy check backlog result: Success, backlog size: 11, acceptable level: 10, failed reason:", exception.Message);
		}

		public void TestWait_ReturnsTotalWaitTimeOnSuccess()
		{
			// Arrange
			backlogInfoProviderMock.SetupGet(m => m.Timespans).Returns(new[] { TimeSpan.FromSeconds(2) });
			backlogInfoProviderMock.SetupGet(m => m.AcceptableBacklog).Returns(10);
			var unsuccessfulAttempt = new AttemptInGettingBacklog(true, string.Empty, new BacklogResult { BacklogDescription = "test", BacklogSize = 11 });
			var successfulAttempt = new AttemptInGettingBacklog(true, string.Empty, new BacklogResult { BacklogDescription = "test", BacklogSize = 9 });
			backlogInfoProviderMock
				.SetupSequence(m => m.GetCurrentBacklog())
				.Returns(unsuccessfulAttempt) // Initial check
				.Returns(successfulAttempt) // Check after 2 seconds is successful
				.Returns(successfulAttempt);

			// Act
			var result = lowPriorityProcessPauser.Wait();

			// Assert
			AssertEquals(2, result.Seconds);
		}

		public void TestCallbackIsActuallyCalledOnWaitDelay()
		{
			// Arrange
			backlogInfoProviderMock.SetupGet(m => m.Timespans).Returns(new[] { TimeSpan.FromSeconds(2) });
			backlogInfoProviderMock.SetupGet(m => m.AcceptableBacklog).Returns(10);
			var unsuccessfulAttempt = new AttemptInGettingBacklog(true, string.Empty, new BacklogResult { BacklogDescription = "test", BacklogSize = 11 });
			var successfulAttempt = new AttemptInGettingBacklog(true, string.Empty, new BacklogResult { BacklogDescription = "test", BacklogSize = 9 });

			backlogInfoProviderMock
				.SetupSequence(m => m.GetCurrentBacklog())
				.Returns(unsuccessfulAttempt) // Initial check
				.Returns(unsuccessfulAttempt) // Check after 2 seconds is unsuccessful
				.Returns(unsuccessfulAttempt) // Check after 4 seconds is unsuccessful
				.Returns(unsuccessfulAttempt) // Check after 6 seconds is unsuccessful - should call callback
				.Returns(unsuccessfulAttempt) // Check after 8 seconds is unsuccessful - should call callback
				.Returns(successfulAttempt) // Check after 10 seconds is successful
				.Returns(successfulAttempt);

			bool functionUsed = false;
			Action dummyFunction = () => { functionUsed = true; };

			// Act
			lowPriorityProcessPauser.Wait(loggerMock.Object, null, dummyFunction);

			Assert(functionUsed);
		}

		[UseSnapshotProtection]
		public void TestCheckCdcBacklogFirst()
		{
			var pauser = new LowPriorityProcessPauser();
			var waiter = typeof(LowPriorityProcessPauser).GetField("waiter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pauser);
			var provider = typeof(BacklogWaiter).GetField("strategies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(waiter) as IBacklogInfoProvider[];

			Assert("The first latency provider should be CdcLatencyProvider", provider[0] is CdcLatencyProvider);
		}

		Mock<ILogger> loggerMock;
		LowPriorityProcessPauser lowPriorityProcessPauser;
		Mock<IBacklogInfoProvider> backlogInfoProviderMock;

		protected override void SetUp()
		{
			base.SetUp();
			backlogInfoProviderMock = new Mock<IBacklogInfoProvider>();
			lowPriorityProcessPauser = new LowPriorityProcessPauser(new BacklogWaiter(new[] { backlogInfoProviderMock.Object }));
			loggerMock = new Mock<ILogger>();
		}
	}
}
