using System;
using Enterprise.Integration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ServiceManager.Logging.CW.Test
{
	public class LoggerAdapterTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestLog_WithAllLogTypes_LogsCorrectType()
		{
			// Arrange
			Test(LogType.Debug, LogLevel.Debug);
			Test(LogType.Information, LogLevel.Information);
			Test(LogType.Warning, LogLevel.Warning);
			Test(LogType.Error, LogLevel.Error);
			
			void Test(LogType logType, LogLevel logLevel)
			{
				// Act
				loggerAdapter.Log(logType, string.Empty);

				// Assert
				loggerMock.VerifyLog(logLevel, string.Empty, Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestLog_WithUnsupportedLogType_LogsDebug()
		{
			// Arrange
			var invalidLogType = (LogType)99;

			// Act
			loggerAdapter.Log(invalidLogType, string.Empty);

			// Assert
			loggerMock.VerifyLog(LogLevel.Debug, string.Empty, Times.Once);
		}

		[ExpectNoExceptions]
		public void TestLog_WithMessage_LogsMessage()
		{
			// Arrange
			const string expectedMessage = "message";

			// Act
			loggerAdapter.Log(LogType.Debug, expectedMessage);

			// Assert
			loggerMock.VerifyLog(LogLevel.Debug, expectedMessage, Times.Once);
		}

		[ExpectNoExceptions]
		public void TestLog_WithException_LogsException()
		{
			// Arrange
			var expectedException = new Exception("message");

			// Act
			loggerAdapter.Log(LogType.Debug, string.Empty, expectedException);

			// Assert
			loggerMock.VerifyLog(LogLevel.Debug, string.Empty, expectedException, Times.Once);
		}

		protected override void SetUp()
		{
			loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
			loggerAdapter = new LoggerAdapter(loggerMock.Object);
		}

		Mock<Microsoft.Extensions.Logging.ILogger> loggerMock;
		LoggerAdapter loggerAdapter;
	}
}
