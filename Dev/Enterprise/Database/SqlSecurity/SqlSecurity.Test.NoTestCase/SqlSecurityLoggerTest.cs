using Moq;
using NUnit.Framework;
using IntegrationLogging = Enterprise.Integration;
using SynchroniserLogging = WTG.Data.SqlDbSecuritySynchroniser;

namespace Enterprise.SqlSecurity.Test
{
	public class SqlSecurityLoggerTest
	{
		[TestCase(SynchroniserLogging.LogType.SyncError, IntegrationLogging.LogType.Error, TestName = "Synchroniser logger SyncError log creates integration Error log")]
		[TestCase(SynchroniserLogging.LogType.BatchRunError, IntegrationLogging.LogType.Warning, TestName = "Synchroniser logger BatchRunError log creates integration Warning log")]
		[TestCase(SynchroniserLogging.LogType.StatementsAfterSync, IntegrationLogging.LogType.Warning, TestName = "Synchroniser logger StatementsAfterSync log creates integration Warning log")]
		[TestCase(SynchroniserLogging.LogType.TimerStats, IntegrationLogging.LogType.Information, TestName = "Synchroniser logger TimerStats log creates integration Information log")]
		[TestCase(SynchroniserLogging.LogType.NothingToDo, IntegrationLogging.LogType.Information, TestName = "Synchroniser logger NothingToDo log creates integration Information log")]
		[TestCase(SynchroniserLogging.LogType.Statements, IntegrationLogging.LogType.Information, TestName = "Synchroniser logger Statements log creates integration Debug log")]

		public void TestSynchroniserLogMessageAddsCorrespondingIntegrationLogMessage(SynchroniserLogging.LogType synchroniserLogType, IntegrationLogging.LogType integrationLogType)
		{
			// Arrange
			var integrationLoggerMock = new Mock<IntegrationLogging.ILogger>();
			var synchroniserLogger = new SqlSecurityLogger(integrationLoggerMock.Object);

			// Act
			synchroniserLogger.Log(synchroniserLogType, It.IsAny<string>());

			// Assert
			integrationLoggerMock.Verify(l => l.Log(integrationLogType, It.IsAny<string>()), Times.Once());
		}
	}
}
