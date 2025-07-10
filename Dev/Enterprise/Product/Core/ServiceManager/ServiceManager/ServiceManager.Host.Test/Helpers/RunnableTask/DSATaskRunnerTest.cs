using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ServiceManager.Host.Testing.Core.RunnableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Helpers.Testing
{
	class DSATaskRunnerTest : TestCaseWithFactory
	{
		[TestDate(2015, 9, 14, 9, 0, 0)]
		public void TestDSARunnerShouldRunDSATaskImmediatelyIfHostRestarts()
		{
			// Arrange
			var taskQueue = new Mock<ITaskQueue>();
			var dsaTask = new Mock<IRunnableServiceTask>();
			var logger = Mock.Of<IHostLogger>();
			var request = new DirectTaskRunRequest(dsaTask.Object, echoes: false);
			IDSATaskRunner dsaRunner = new DSATaskRunner(dsaTask.Object, taskQueue.Object, logger);

			// Act
			dsaRunner.RunDsaTaskIfDbServerRestarts();

			// Assert
			taskQueue.Verify(q => q.EnqueueTask(request), Times.AtLeastOnce);
			Assert(true);
		}

		[ExpectNoExceptions]
		public void TestLogMessageWhenNewDsaRequest()
		{
			// Arrange
			var logger = new Mock<IHostLogger>();
			IDSATaskRunner dsaRunner = new DSATaskRunner(Mock.Of<IRunnableServiceTask>(x => x.Code == "DSA"), Mock.Of<ITaskQueue>(), logger.Object);

			// Act
			dsaRunner.RunDsaTaskIfDbServerRestarts();

			// Assert
			logger.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"\\[DSA/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}\\] Nudge run request is created.")), Times.Once);
			logger.VerifyNoOtherCalls();
		}

		public void TestWrongParamsCall()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var taskMock = Mock.Of<IRunnableServiceTask>();
				var taskQueueMock = Mock.Of<ITaskQueue>();
				var loggerMock = Mock.Of<IHostLogger>();

				var result = AssertExceptionThrown<ArgumentNullException>(() => new DSATaskRunner(null, taskQueueMock, loggerMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("dsaTask"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new DSATaskRunner(taskMock, null, loggerMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("taskQueue"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new DSATaskRunner(taskMock, taskQueueMock, null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostLogger"));
			});
		}
	}
}
