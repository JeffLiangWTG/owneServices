using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Helpers.Testing
{
	class TaskQueueLogTest
	{
		const string code = "LWM";
		Mock<IHostLogger>? loggerMock;
		TaskQueue? taskQueue;
		Mock<IRunnableServiceTask>? taskMock;

		[SetUp]
		public void SetUp()
		{
			loggerMock = new Mock<IHostLogger>();
			taskQueue = new TaskQueue(Mock.Of<IProcessRunnerPool>(), loggerMock.Object);

			taskMock = new Mock<IRunnableServiceTask>();
			taskMock
				.Setup(x => x.Code)
				.Returns(code);
			taskMock
				.Setup(x => x.TimeSinceLastEnqueued)
				.Returns(new Stopwatch());
			taskMock
				.Setup(x => x.TimeSinceLastDequeued)
				.Returns(new Stopwatch());
		}

		Mock<T> CreateRunnableServiceTaskMock<T>() where T : class, ITaskRunRequest
		{
			var request = new Mock<T>();
			request
				.Setup(x => x.Task)
				.Returns(() => taskMock!.Object);
			return request;
		}

		[Test]
		public void TestAbsorbedRequest()
		{
			// Arrange
			var oldRequestMock = CreateRunnableServiceTaskMock<ITaskRunRequest>();
			var newRequestMock = CreateRunnableServiceTaskMock<ITaskRunRequest>();
			newRequestMock
				.Setup(x => x.FormatRequestToLogMessage(It.IsAny<LogMessageStage>(), It.IsAny<object[]>()))
				.Returns($"{LogMessageStage.AbsorbedByAnotherRequest}");
			taskQueue!.EnqueueTask(oldRequestMock.Object);
			loggerMock!.Reset();

			// Act
			taskQueue.EnqueueTask(newRequestMock.Object);

			// Assert
			Assert.DoesNotThrow(() =>
			{
				loggerMock!.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"{LogMessageStage.AbsorbedByAnotherRequest}")), Times.Once);
				loggerMock.VerifyNoOtherCalls();
				newRequestMock.Verify(x => x.FormatRequestToLogMessage(LogMessageStage.AbsorbedByAnotherRequest, oldRequestMock.Object), Times.Once);
				newRequestMock.Verify(x => x.FormatRequestToLogMessage(It.IsAny<LogMessageStage>(), It.IsAny<object[]>()), Times.Once);
			});
		}

		[Test]
		public void TestAbsorbedNudgeRunRequest()
		{
			// Arrange
			var oldRequestMock = CreateRunnableServiceTaskMock<IDirectTaskRunRequest>();
			oldRequestMock
				.SetupGet(request => request.HasRunsRemaining)
				.Returns(true);
			var newRequestMock = CreateRunnableServiceTaskMock<IDirectTaskRunRequest>();
			newRequestMock
				.SetupGet(request => request.HasRunsRemaining)
				.Returns(true);
			newRequestMock
				.Setup(x => x.HasMoreRetriesRemained(It.IsAny<IDirectTaskRunRequest>()))
				.Returns(false);
			newRequestMock
				.Setup(x => x.FormatRequestToLogMessage(It.IsAny<LogMessageStage>(), It.IsAny<object[]>()))
				.Returns($"{LogMessageStage.AbsorbedByAnotherRequest}");

			taskQueue!.EnqueueTask(oldRequestMock.Object);
			loggerMock!.Reset();

			// Act
			taskQueue.EnqueueTask(newRequestMock.Object);

			// Assert
			Assert.DoesNotThrow(() =>
			{
				loggerMock!.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"{LogMessageStage.AbsorbedByAnotherRequest}")), Times.Once);
				loggerMock.VerifyNoOtherCalls();
				newRequestMock.Verify(x => x.FormatRequestToLogMessage(LogMessageStage.AbsorbedByAnotherRequest, oldRequestMock.Object), Times.Once);
				newRequestMock.Verify(x => x.FormatRequestToLogMessage(It.IsAny<LogMessageStage>(), It.IsAny<object[]>()), Times.Once);
			});
		}

		[Test]
		public void TestReplacesNudgeRequest()
		{
			// Arrange
			var oldRequestMock = CreateRunnableServiceTaskMock<IDirectTaskRunRequest>();
			oldRequestMock
				.SetupGet(request => request.HasRunsRemaining)
				.Returns(true);
			var newRequestMock = CreateRunnableServiceTaskMock<IDirectTaskRunRequest>();
			newRequestMock
				.SetupGet(request => request.HasRunsRemaining)
				.Returns(true);
			newRequestMock
					.Setup(x => x.FormatRequestToLogMessage(It.IsAny<LogMessageStage>(), It.IsAny<object[]>()))
				.Returns($"{LogMessageStage.ReplacesRequest}");
			newRequestMock
				.Setup(x => x.HasMoreRetriesRemained(It.IsAny<IDirectTaskRunRequest>()))
				.Returns(true);

			taskQueue!.EnqueueTask(oldRequestMock.Object);
			loggerMock!.Reset();

			// Act
			taskQueue.EnqueueTask(newRequestMock.Object);

			// Assert
			Assert.DoesNotThrow(() =>
			{
				loggerMock!.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"{LogMessageStage.ReplacesRequest}")), Times.Once);
				loggerMock.VerifyNoOtherCalls();
				newRequestMock.Verify(x => x.FormatRequestToLogMessage(LogMessageStage.ReplacesRequest, oldRequestMock.Object), Times.Once);
				newRequestMock.Verify(x => x.FormatRequestToLogMessage(It.IsAny<LogMessageStage>(), It.IsAny<object[]>()), Times.Once);
			});
		}

		[Test]
		public void TestEnqueueRequest()
		{
			// Arrange
			var requestMock = CreateRunnableServiceTaskMock<ITaskRunRequest>();
			requestMock
				.Setup(x => x.FormatRequestToLogMessage(It.IsAny<LogMessageStage>(), It.IsAny<object[]>()))
				.Returns($"{LogMessageStage.EnqueuedRequest}");

			// Act
			taskQueue!.EnqueueTask(requestMock.Object);

			// Assert
			Assert.DoesNotThrow(() =>
			{
				loggerMock!.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"{LogMessageStage.EnqueuedRequest}")), Times.Once);
				loggerMock.VerifyNoOtherCalls();
				requestMock.Verify(x => x.FormatRequestToLogMessage(LogMessageStage.EnqueuedRequest), Times.Once);
				requestMock.Verify(x => x.FormatRequestToLogMessage(It.IsAny<LogMessageStage>(), It.IsAny<object[]>()), Times.Once);
			});
		}

		[Test]
		public void TestDequeueRequest()
		{
			// Arrange
			var requestMock = CreateRunnableServiceTaskMock<ITaskRunRequest>();
			requestMock
				.Setup(x => x.FormatRequestToLogMessage(It.IsAny<LogMessageStage>(), It.IsAny<object[]>()))
				.Returns($"{LogMessageStage.DequeuedRequest}");

			taskQueue!.EnqueueTask(requestMock.Object);
			loggerMock!.Reset();

			// Act
			taskQueue.TryDequeueTask(out _);

			// Assert
			Assert.DoesNotThrow(() =>
			{
				loggerMock!.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"{LogMessageStage.DequeuedRequest}")), Times.Once);
				loggerMock.VerifyNoOtherCalls();
				requestMock.Verify(x => x.FormatRequestToLogMessage(LogMessageStage.DequeuedRequest), Times.Once);
			});
		}
	}
}
