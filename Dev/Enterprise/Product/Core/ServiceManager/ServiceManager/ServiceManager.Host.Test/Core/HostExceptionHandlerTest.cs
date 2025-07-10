using System;
using System.Collections.Generic;
using System.Net;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceHostClient.Exceptions;
using ServiceManager.Logging.CW.Test;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	class HostExceptionHandlerTest
	{
		[SetUp]
		public void SetUp()
		{
			loggerMock = new Mock<ILogger>();
			exceptionHandler = new HostExceptionHandler();
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
				var result = exceptionHandler.HandleSpecificExceptions(exception, message, lazy);

				// Assert
				Assert.DoesNotThrow(() =>
				{
					switch (action)
					{
						case ExceptionHandler.ExceptionAction.Ignore:
							NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo(true));
							loggerMock.VerifyLogAny(Times.Never);
							break;

						case ExceptionHandler.ExceptionAction.LogError:
							NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo(true));
							loggerMock.VerifyLog(LogLevel.Error, message, exception, Times.Once);
							break;

						case ExceptionHandler.ExceptionAction.LogWarning:
							NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo(true));
							loggerMock.VerifyLog(LogLevel.Warning, message, exception, Times.Once);
							break;

						case ExceptionHandler.ExceptionAction.Report:
							NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo(false));
							loggerMock.VerifyLogAny(Times.Never);
							break;

						default:
							throw new ArgumentOutOfRangeException(nameof(action), action, null);
					}
				}, $"Unexpected handling of {exception.GetType().Name}");
			}
		}

		static readonly IEnumerable<(ExceptionHandler.ExceptionAction action, string message, Exception exception)> HandledExceptions = new[]
		{
			(ExceptionHandler.ExceptionAction.Report, "message", new Exception()),
			(ExceptionHandler.ExceptionAction.Ignore, "message", new TestHostInternalException("aaa")),
			(ExceptionHandler.ExceptionAction.Ignore, "message1", new ProcessControllerConfigurationException("aaa")),
			(ExceptionHandler.ExceptionAction.Ignore, "message2", new ProcessControllerConfigurationException("bbb")),
			(ExceptionHandler.ExceptionAction.Ignore, "message", new ProcessControllerConfigurationException(string.Empty)),
			(ExceptionHandler.ExceptionAction.LogError, "message", new ServiceHostDeserializeException(string.Empty, null)),
			(ExceptionHandler.ExceptionAction.LogError, "does not have access rights to listen HTTP prefix", new HttpListenerException(5)),
			(ExceptionHandler.ExceptionAction.LogError, "Could not initialise HTTP listener", new HttpListenerException(1229)),
		};

		HostExceptionHandler exceptionHandler;
		Mock<ILogger> loggerMock;

		[Serializable]
		class TestHostInternalException : HostInternalException
		{
			public TestHostInternalException(string message)
				: base(message)
			{
			}

#if NETFRAMEWORK
			protected TestHostInternalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}
	}
}
