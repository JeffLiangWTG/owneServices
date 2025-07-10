using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Logging.CW.Test;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class ServiceErrorReporterTest : TestCase
	{
		public void TestWrongConstructorParams()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<TargetInvocationException>(() => _ = new Mock<ServiceErrorReporter>((IExceptionHandler)null) { CallBase = true }.Object);
				AssertEquals("exceptionHandler", (result.InnerException as ArgumentNullException)?.ParamName);
			});
		}

		[ExpectNoExceptions]
		public void TestReportNullException()
		{
			try
			{
				// Arrange
				const string message = "message";
				Globals.IsUserInteractive = false;

				// Act
				serviceErrorReporter.Report(null, message, null);
				var result = ExceptionReporterTestListener.Instance.Single();

				// Assert
				loggerMock.VerifyLogException<ILogger, Exception>(o => o != null, Times.Never);
				loggerMock.VerifyLogAny(Times.Once);
				loggerMock.VerifyLog(LogLevel.Error, s => s.StartsWith(message, StringComparison.Ordinal), Times.Once);
				AssertType<DeveloperNotificationException>(result);
				AssertEquals(string.Empty, result.Message);
				AssertType<DeveloperNotificationException>(result.InnerException);
				AssertEquals(message, result.InnerException.Message);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestReportExceptions()
		{
			CombineAssertions(() =>
			{
				foreach (var (key, message, exception) in ReportingExceptions)
				{
					Test(key, message, exception);
				}
			});

			void Test(string key, string message, Exception exception)
			{
				try
				{
					// Arrange
					loggerMock.Invocations.Clear();
					exceptionHandlerMock.Invocations.Clear();
					Globals.IsUserInteractive = false;
					exceptionHandlerMock.Setup(handler => handler.HandleSpecificExceptions(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<Lazy<ILogger>>())).Returns(false);

					// Act
					serviceErrorReporter.Report(key, message, exception);
					var result = ExceptionReporterTestListener.Instance.Single();

					// Assert
					exceptionHandlerMock.Verify(handler => handler.HandleSpecificExceptions(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<Lazy<ILogger>>()), Times.Once);
					exceptionHandlerMock.Verify(handler => handler.HandleSpecificExceptions(exception, message, It.Is<Lazy<ILogger>>(lazy => lazy.Value == loggerMock.Object)), Times.Once);

					loggerMock.VerifyLogAny(Times.Once);
					loggerMock.VerifyLogException<ILogger, Exception>(o => o == null, Times.Never);
					loggerMock.VerifyLog(LogLevel.Error, message, exception, Times.Once);

					AssertType<DeveloperNotificationException>(result);
					AssertEquals(message, result.Message);
					AssertType(exception.GetType(), result.InnerException);
					AssertEquals(exception.Message, result.InnerException.Message);
				}
				finally
				{
					ExceptionReporterTestListener.Instance.Clear();
				}
			}
		}

		public void TestHandledExceptions()
		{
			CombineAssertions(() =>
			{
				foreach (var (key, message, exception) in ReportingExceptions)
				{
					Test(key, message, exception);
				}
			});

			void Test(string key, string message, Exception exception)
			{
				// Arrange
				loggerMock.Invocations.Clear();
				exceptionHandlerMock.Invocations.Clear();
				exceptionHandlerMock.Setup(handler => handler.HandleSpecificExceptions(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<Lazy<ILogger>>())).Returns(true);

				// Act
				serviceErrorReporter.Report(key, message, exception);

				// Assert
				exceptionHandlerMock.Verify(handler => handler.HandleSpecificExceptions(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<Lazy<ILogger>>()), Times.Once);
				exceptionHandlerMock.Verify(handler => handler.HandleSpecificExceptions(exception, message, It.Is<Lazy<ILogger>>(lazy => lazy.Value == loggerMock.Object)), Times.Once);

				loggerMock.VerifyLogAny(Times.Never);

				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		[ExpectNoExceptions]
		public void TestHandleSpecificExceptionsIsCalledWithCurrentLogger()
		{
			CombineAssertions(() =>
			{
				foreach (var (key, message, exception) in ReportingExceptions)
				{
					Test(key, message, exception);
				}
			});

			void Test(string key, string message, Exception exception)
			{
				// Arrange
				loggerMock.Invocations.Clear();
				exceptionHandlerMock.Invocations.Clear();
				exceptionHandlerMock.Setup(handler => handler.HandleSpecificExceptions(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<Lazy<ILogger>>())).Returns(true);

				// Act
				serviceErrorReporter.Report(key, message, exception);

				// Assert
				exceptionHandlerMock.Verify(handler => handler.HandleSpecificExceptions(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<Lazy<ILogger>>()), Times.Once);
				exceptionHandlerMock.Verify(handler => handler.HandleSpecificExceptions(exception, message, It.Is<Lazy<ILogger>>(lazy => !lazy.IsValueCreated && lazy.Value == loggerMock.Object)), Times.Once);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			loggerMock = new Mock<ILogger>();
			exceptionHandlerMock = new Mock<IExceptionHandler>();
			var serviceErrorReporterMock = new Mock<ServiceErrorReporter>(exceptionHandlerMock.Object)
			{
				CallBase = true,
			};
			serviceErrorReporterMock
				.Protected()
				.SetupGet<ILogger>("CurrentLogger")
				.Returns(loggerMock.Object);
			serviceErrorReporter = serviceErrorReporterMock.Object;
		}

		static readonly IEnumerable<(string key, string message, Exception exception)> ReportingExceptions = new[]
		{
			("key1", "message1", new Exception("aaa")),
			("key2", "message2", new InvalidOperationException("bbb")),
			("key3", "message3", new ObjectDisposedException("ccc")),
			("key4", "message4", new AccessViolationException("ddd")),
		};

		Mock<IExceptionHandler> exceptionHandlerMock;
		Mock<ILogger> loggerMock;
		ServiceErrorReporter serviceErrorReporter;
	}
}
