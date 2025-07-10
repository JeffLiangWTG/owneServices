using System;
using CargoWise.Common;
using Dat.Integration;
using Enterprise.Dat.Implementation;
using Moq;
using NUnit.Framework;

namespace Enterprise.Dat.Deployment.Test
{
	partial class BuildDeployerTest
	{
		sealed class IndentedTaskLoggerTest : TestCase
		{
			[ExpectNoExceptions]
			public void TestRecordInfoOutsideTask()
			{
				// Arrange
				const string message = @"first line
second line";
				var innerLogger = new Mock<ITaskLogger>();
				innerLogger.Setup(x => x.RecordInfo(@"first line
second line")).Verifiable(Times.Once());

				var logger = new BuildDeployer.IndentedTaskLogger(innerLogger.Object);

				// Act
				logger.RecordInfo(message);

				// Assert
				innerLogger.Verify();
			}

			[ExpectNoExceptions]
			public void TestRecordTask()
			{
				// Arrange
				const string taskName = "taskA";
				var innerLogger = new Mock<ITaskLogger>();
				innerLogger.Setup(x => x.RecordTask(taskName)).Verifiable(Times.Once());

				var logger = new BuildDeployer.IndentedTaskLogger(innerLogger.Object);

				// Act
				using (logger.RecordTask(taskName)) { }

				// Assert
				innerLogger.Verify();
			}

			[ExpectNoExceptions]
			public void TestRecordTask_DisposeIsInvoked()
			{
				// Arrange
				var disposable = new Mock<IDisposable>();
				disposable.Setup(x => x.Dispose()).Verifiable(Times.Once());

				const string taskName = "taskA";
				var innerLogger = new Mock<ITaskLogger>();
				innerLogger.Setup(x => x.RecordTask(taskName)).Returns(disposable.Object).Verifiable(Times.Once());

				var logger = new BuildDeployer.IndentedTaskLogger(innerLogger.Object);

				// Act
				using (logger.RecordTask(taskName)) { }

				// Assert
				disposable.Verify();
				innerLogger.Verify();
			}

			[ExpectNoExceptions]
			public void TestRecordInfoInsideTask()
			{
				// Arrange
				const string taskName = @"taskA";
				const string message = @"first line
second line";
				var innerLogger = new Mock<ITaskLogger>();
				innerLogger.Setup(x => x.RecordTask("taskA")).Returns(DisposableAction.NoAction).Verifiable(Times.Once());
				innerLogger.Setup(x => x.RecordInfo(@"    first line
    second line")).Verifiable(Times.Once());

				var logger = new BuildDeployer.IndentedTaskLogger(innerLogger.Object);

				using var disposeTask = logger.RecordTask(taskName);

				// Act
				logger.RecordInfo(message);

				// Assert
				innerLogger.Verify();
			}
		}
	}
}
