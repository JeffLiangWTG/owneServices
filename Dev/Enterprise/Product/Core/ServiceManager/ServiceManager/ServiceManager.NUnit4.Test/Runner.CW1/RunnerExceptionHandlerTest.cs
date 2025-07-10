using System;
using System.Collections.Generic;
using System.Net;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using Enterprise.ServiceManager.Runner.Exceptions;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Logging.CW.Test;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Exceptions
{
	class RunnerExceptionHandlerTest
	{
		[SetUp]
		public void SetUp()
		{
			loggerMock = new Mock<ILogger>();
			runnerExceptionHandler = new RunnerExceptionHandler();
		}

		[Test]
		public void TestHandleSpecificExceptions()
		{
			Assert.Multiple(() =>
			{
				foreach (var (action, message, exception) in HandledExceptions)
				{
					Test(action, message, exception);
				}
			});

			void Test(ExceptionHandler.ExceptionAction action, string message, Exception exception)
			{
				// Arrange
				loggerMock.Reset();
				var lazy = new Lazy<ILogger>(() => loggerMock.Object);

				// Act
				var result = runnerExceptionHandler.HandleSpecificExceptions(exception, message, lazy);

				// Assert
				switch (action)
				{
					case ExceptionHandler.ExceptionAction.Ignore:
						Assert.That(result, Is.True);
						loggerMock.VerifyLogAny(Times.Never);
						break;

					case ExceptionHandler.ExceptionAction.LogError:
						Assert.That(result, Is.True);
						loggerMock.VerifyLog(LogLevel.Error, message, exception, Times.Once);
						break;

					case ExceptionHandler.ExceptionAction.LogWarning:
						Assert.That(result, Is.True);
						loggerMock.VerifyLog(LogLevel.Warning, message, exception, Times.Once);
						break;

					case ExceptionHandler.ExceptionAction.Report:
						Assert.That(result, Is.False);
						loggerMock.VerifyLogAny(Times.Never);
						break;

					default:
						throw new ArgumentOutOfRangeException(nameof(action), action, null);
				}
			}
		}

		static readonly IEnumerable<(ExceptionHandler.ExceptionAction action, string message, Exception exception)> HandledExceptions = new[]
		{
			(ExceptionHandler.ExceptionAction.Report, "message", new Exception()),
			(ExceptionHandler.ExceptionAction.LogError, "message", new RunnerInternalException()),
			(ExceptionHandler.ExceptionAction.LogError, "message", new CouldNotReadNextRunTimeException()),
			(ExceptionHandler.ExceptionAction.LogError, "message", new RunnerGrpcInitializationException("1")),
			(ExceptionHandler.ExceptionAction.Report, "message3", new HttpListenerException()),
			(ExceptionHandler.ExceptionAction.Report, "message4", new HttpListenerException(123)),
			(ExceptionHandler.ExceptionAction.Report, "message5", new Mock<DatabaseUpgradeException>(string.Empty).Object),
			(ExceptionHandler.ExceptionAction.Report, "message6", new OutOfMemoryException()),
			(ExceptionHandler.ExceptionAction.Report, "message7", new SqlLockLostException()),
			(ExceptionHandler.ExceptionAction.Report, "message8", new EmailHasNoFromAddressException(string.Empty)),
			(ExceptionHandler.ExceptionAction.Report, "message9", new EmailNotCompleteException(string.Empty)),
			(ExceptionHandler.ExceptionAction.Report, "message0", new EmailHasNoRecipientsException(string.Empty)),
			(ExceptionHandler.ExceptionAction.Report, "messageA", new ZSaveException(new ZDataException(default, default, default), default)),
			(ExceptionHandler.ExceptionAction.Report, "messageB", new OdysseyDataException(string.Empty)),
			(ExceptionHandler.ExceptionAction.LogError, "messageC", new HostedServiceException()),
			(ExceptionHandler.ExceptionAction.Ignore, "messageD", new HostedServiceException { LogException = false }),
		};

		Mock<ILogger> loggerMock = null!;
		RunnerExceptionHandler runnerExceptionHandler = null!;
	}
}
