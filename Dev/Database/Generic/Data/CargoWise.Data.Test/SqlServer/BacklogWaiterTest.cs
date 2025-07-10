using System;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.SqlServer.Testing
{
	sealed class BacklogWaiterTest : TestCase
	{
		public void TestMultipleProvidersAggregateWaitTime()
		{
			var mockLogfullness = new Mock<IBacklogInfoProvider>();

			var timeSpans = new TimeSpan[] { TimeSpan.FromMilliseconds(10) };
			mockLogfullness.Setup(x => x.Timespans).Returns(timeSpans);

			var attempt = new AttemptInGettingBacklog(true, string.Empty, new BacklogResult());
			mockLogfullness.Setup(x => x.GetCurrentBacklog()).Returns(attempt);

			mockLogfullness.Setup(x => x.AcceptableBacklog).Returns(5);

			var mockBacklogWaiter = new BacklogWaiter(new IBacklogInfoProvider[] { mockLogfullness.Object });

			Assert(string.Format(" Logfullness backlog: {0}%, WaitTime: {1}"
					  , attempt.BacklogResult.BacklogSize.ToString()
					  , mockBacklogWaiter.WaitUntilBacklogIsAcceptable().ToString())
			, TimeSpan.FromSeconds(0) <= mockBacklogWaiter.WaitUntilBacklogIsAcceptable());
		}

		public void TestNoOverflowExceptionForTimeSpan()
		{
			var mockLogfullness = new Mock<IBacklogInfoProvider>();

			var timeSpans = new TimeSpan[] { TimeSpan.FromMilliseconds(10) };
			mockLogfullness.Setup(x => x.Timespans).Returns(timeSpans);

			var attempt = new AttemptInGettingBacklog(true, String.Empty, new BacklogResult() { BacklogSize = 10 });
			mockLogfullness.Setup(x => x.GetCurrentBacklog()).Returns(attempt);

			const long acceptableBacklog = 5;
			mockLogfullness.Setup(x => x.AcceptableBacklog).Returns(() =>
			{
				attempt.BacklogResult.BacklogSize = 0;
				return acceptableBacklog;
			});

			var waiter = new BacklogWaiter(new IBacklogInfoProvider[] { mockLogfullness.Object });
			var result = waiter.WaitUntilBacklogIsAcceptable();

			AssertEquals(true, result.TotalSeconds < 1);
		}

		public void TestWaitUntilBacklogIsAcceptable_FailedToGetBacklogThenSucceeded()
		{
			var mockLogfullness = new Mock<IBacklogInfoProvider>();

			var timeSpans = new TimeSpan[] { TimeSpan.FromMilliseconds(10) };
			mockLogfullness.Setup(x => x.Timespans).Returns(timeSpans);

			mockLogfullness.Setup(x => x.GetCurrentBacklog()).Returns(new AttemptInGettingBacklog(false, "Failed", new BacklogResult()));
			mockLogfullness.Setup(x => x.GetCurrentBacklog()).Returns(new AttemptInGettingBacklog(true, string.Empty, new BacklogResult() { BacklogSize = 5 }));
			mockLogfullness.Setup(x => x.GetCurrentBacklog()).Returns(new AttemptInGettingBacklog(true, string.Empty, new BacklogResult() { BacklogSize = 0 }));
			mockLogfullness.Setup(x => x.AcceptableBacklog).Returns(5);

			var waiter = new BacklogWaiter(new IBacklogInfoProvider[] { mockLogfullness.Object });
			var result = waiter.WaitUntilBacklogIsAcceptable();

			AssertEquals(true, result.TotalSeconds < 1);
		}

		public void TestWaitUntilBacklogIsAcceptable_ExitsOnTimeout()
		{
			// Arrange
			var backlogInfoProviderMocks = Enumerable.Range(0, 5)
				.Select(i =>
				{
					var backlogInfoProviderMock = new Mock<IBacklogInfoProvider>();

					backlogInfoProviderMock.Setup(x => x.Timespans).Returns(new[] { TimeSpan.FromSeconds(15) });
					backlogInfoProviderMock.Setup(x => x.GetCurrentBacklog()).Returns(new AttemptInGettingBacklog(true, "Not important failure reason", new BacklogResult { BacklogSize = 10, BacklogDescription = "Test Description" }));
					backlogInfoProviderMock.Setup(x => x.AcceptableBacklog).Returns(5);

					return backlogInfoProviderMock;
				})
				.ToArray();

			var waiter = new BacklogWaiter(backlogInfoProviderMocks.Select(mock => mock.Object).ToArray());
			var maxWaitTime = TimeSpan.FromSeconds(2);
			string timeoutResultMessage = null;

			// Act
			var result = waiter.WaitUntilBacklogIsAcceptable(maxWaitTime, null, (span, message) => { timeoutResultMessage = message; });

			// Assert
			CombineAssertions(() =>
			{
				AssertNotNull(timeoutResultMessage);
				AssertContains("IBacklogInfoProviderProxy check backlog result: Success, backlog size: 10, acceptable level: 5, failed reason: Not important failure reason", timeoutResultMessage);
				AssertEquals(maxWaitTime.TotalSeconds, result.TotalSeconds, 0.5);
			});
		}

		public void TestWaitUntilBacklogIsAcceptable_TimeoutIsCalledOnlyOnce()
		{
			// Arrange
			var backlogInfoProviders = Enumerable.Range(0, 100)
				.Select(i =>
				{
					var backlogInfoProviderMock = new Mock<IBacklogInfoProvider>();

					backlogInfoProviderMock.Setup(x => x.Timespans).Returns(new[] { TimeSpan.FromSeconds(15) });
					backlogInfoProviderMock.Setup(x => x.GetCurrentBacklog()).Returns(new AttemptInGettingBacklog(true, string.Empty, new BacklogResult { BacklogSize = 10 }));
					backlogInfoProviderMock.Setup(x => x.AcceptableBacklog).Returns(5);

					return backlogInfoProviderMock.Object;
				})
				.ToArray();

			var waiter = new BacklogWaiter(backlogInfoProviders);
			var expectedAmount = 0;

			// Act
			waiter.WaitUntilBacklogIsAcceptable(TimeSpan.FromSeconds(2), null, (x, y) => { Interlocked.Increment(ref expectedAmount); });

			// Assert
			AssertEquals(1, expectedAmount);
		}
	}
}
