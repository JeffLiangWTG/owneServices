using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	class LogFileArchiveCleanerTaskTest : TestCase
	{
		protected override void SetUp()
		{
			logFileArchiveCleanerMock = new Mock<ILogFileArchiveCleaner>();
			loggerMock = new Mock<IHostLogger>();
			logFileArchiveCleanerTask = new LogFileArchiveCleanerTask(logFileArchiveCleanerMock.Object, loggerMock.Object);
		}

		[ExpectNoExceptions]
		public void TestLogFileArchiveCleanerTask_RunsPerformCleaning()
		{
			// Arrange

			// Act
			logFileArchiveCleanerTask.Run(CancellationToken.None);

			// Assert
			NUnit.Framework.Assert.DoesNotThrow(() => logFileArchiveCleanerMock.Verify(l => l.PerformCleaning(), Times.Once()));
		}

		[ExpectNoExceptions]
		public void TestWrongConstructorParamsCall()
		{
			var result = NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new LogFileArchiveCleanerTask(null, loggerMock.Object));
			NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("logFileArchiveCleaner"));

			result = NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new LogFileArchiveCleanerTask(new Mock<ILogFileArchiveCleaner>().Object, null));
			NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("logger"));
		}

		[ExpectNoExceptions]
		public void TestDatabaseAccessIsPermitted()
		{
			// Arrange
			var logFileArchiveCleanerMock = new Mock<ILogFileArchiveCleaner>();
			var loggerMock = new Mock<IHostLogger>();
			var logFileArchiveCleanerTask = new LogFileArchiveCleanerTask(logFileArchiveCleanerMock.Object, loggerMock.Object);

			loggerMock
				.Setup(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>()))
				.Callback(ServiceManagerApplicationTest.ExecuteTestSql);

			Exception exception = null;

			// Act
			var thread = new Thread(() =>
			{
				try
				{
					logFileArchiveCleanerTask.Run(CancellationToken.None);
				}
				catch (Exception e)
				{
					exception = e;
				}
			});
			thread.Start();
			thread.Join();

			// Assert
			NUnit.Framework.Assert.That(exception, Is.EqualTo(default(Exception)));
		}

		Mock<ILogFileArchiveCleaner> logFileArchiveCleanerMock;
		LogFileArchiveCleanerTask logFileArchiveCleanerTask;
		Mock<IHostLogger> loggerMock;
	}
}
