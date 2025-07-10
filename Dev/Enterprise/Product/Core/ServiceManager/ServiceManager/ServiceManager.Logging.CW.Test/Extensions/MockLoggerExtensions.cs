using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace ServiceManager.Logging.CW.Test
{
	public static class MockLoggerExtensions
	{
		public static void VerifyLogException<TLogger, TException>(this Mock<TLogger> loggerMock, Func<TException, bool> matchException, Func<Times> times) where TLogger : class, ILogger
			where TException : Exception
		{
			loggerMock.Verify(log => log.Log(
				It.IsAny<LogLevel>(),
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.Is<TException>(o => matchException(o)),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()
			), times);
		}

		public static void VerifyLogAny<TLogger>(this Mock<TLogger> loggerMock, Func<Times> times) where TLogger : class, ILogger
		{
			loggerMock.Verify(log => log.Log(
				It.IsAny<LogLevel>(),
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()
			), times);
		}

		public static void VerifyLogLevel<TLogger>(this Mock<TLogger> loggerMock, LogLevel logLevel, Func<Times> times) where TLogger : class, ILogger
		{
			loggerMock.Verify(log => log.Log(
				logLevel,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()
			), times);
		}

		public static void VerifyLog<TLogger>(this Mock<TLogger> loggerMock, LogLevel logLevel, Func<string, bool> matchMessage, Func<Times> times) where TLogger : class, ILogger
		{
			loggerMock.Verify(log => log.Log(
				logLevel,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((o,t) => matchMessage(o.ToString())),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()
			), times);
		}

		public static void VerifyLog<TLogger>(this Mock<TLogger> loggerMock, LogLevel logLevel, Func<string, bool> matchMessage, Exception ex, Func<Times> times) where TLogger : class, ILogger
		{
			loggerMock.Verify(log => log.Log(
				logLevel,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((o, t) => matchMessage(o.ToString())),
				ex,
				It.IsAny<Func<It.IsAnyType, Exception, string>>()
			), times);
		}

		public static void VerifyLog<TLogger>(this Mock<TLogger> loggerMock, LogLevel logLevel, string message, Func<Times> times) where TLogger : class, ILogger
		{
			loggerMock.VerifyLog(logLevel, o => message == o, times);
		}

		public static void VerifyLog<TLogger>(this Mock<TLogger> loggerMock, LogLevel logLevel, string message, Exception exception, Func<Times> times) where TLogger : class, ILogger
		{
			loggerMock.Verify(log => log.Log(
				logLevel,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
				exception,
				It.IsAny<Func<It.IsAnyType, Exception, string>>()
			), times);
		}
	}
}
