using System;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Shared.Testing
{
	class ExceptionHandlerTest
	{
		[Test]
		public void TestFlattensAggregateExceptionAndReturnsWorstResult()
		{
			// Arrange
			var lazy = new Lazy<ILogger>(() => loggerMock.Object);
			var exceptions = new[]
			{
				new Exception(),
				new Exception(),
				new Exception(),
			};
			var exception = new AggregateException(exceptions);
			exceptionHandlerMock
				.Protected()
				.SetupSequence<ExceptionHandler.ExceptionAction>("EvaluateExceptionAction", ItExpr.IsAny<Exception>())
				.Returns(ExceptionHandler.ExceptionAction.Ignore)
				.Returns(ExceptionHandler.ExceptionAction.Report)
				.Returns(ExceptionHandler.ExceptionAction.LogError);

			// Act
			var result = exceptionHandlerMock.Object.HandleSpecificExceptions(exception, string.Empty, lazy);

			// Assert
			Assert.That(result, Is.False);
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
			exceptionHandlerMock
				.Protected()
				.Verify<ExceptionHandler.ExceptionAction>("EvaluateExceptionAction", Times.Exactly(3), ItExpr.IsAny<Exception>());
		}

		[Test]
		public void TestReturnsTheLowestSeverityResultFromInnerExceptions()
		{
			// Arrange
			var lazy = new Lazy<ILogger>(() => loggerMock.Object);
			var exception = new Exception(string.Empty,
				new Exception(string.Empty,
					new Exception()));

			exceptionHandlerMock
				.Protected()
				.SetupSequence<ExceptionHandler.ExceptionAction>("EvaluateExceptionAction", ItExpr.IsAny<Exception>())
				.Returns(ExceptionHandler.ExceptionAction.Ignore)
				.Returns(ExceptionHandler.ExceptionAction.Report)
				.Returns(ExceptionHandler.ExceptionAction.LogError);

			// Act
			var result = exceptionHandlerMock.Object.HandleSpecificExceptions(exception, string.Empty, lazy);

			// Assert
			Assert.That(result, Is.True);
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
			exceptionHandlerMock
				.Protected()
				.Verify<ExceptionHandler.ExceptionAction>("EvaluateExceptionAction", Times.Exactly(3), ItExpr.IsAny<Exception>());
		}

		[SetUp]
		public void SetUp()
		{
			loggerMock = new Mock<ILogger>();
			exceptionHandlerMock = new Mock<ExceptionHandler>();
		}

		Mock<ExceptionHandler> exceptionHandlerMock = new();
		Mock<ILogger> loggerMock = new();
	}
}
