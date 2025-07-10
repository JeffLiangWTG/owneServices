using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ServiceTasksLoggerWrapper))]
sealed class ServiceTasksLogWrapperTest : TestCase
{
	public void TestLogWithoutException()
	{
		var message = "this is the message";
		var loggerMock = new Mock<LoggingInformationBase>();
		var wrapper = new ServiceTasksLoggerWrapper(loggerMock.Object);
		wrapper.Log(Integration.LogType.Debug, message);

		AssertNoExceptionThrown(() =>
		{
			loggerMock.Verify(logger => logger.Log(It.Is<LogType>(type => type == LogType.Debug), It.Is<string>(msg => msg == message)), Times.Once);
		});
	}

	public void TestLogWithException()
	{
		var message = "this is the message";
		var exception = new System.Exception("this is the exception message");
		var loggerMock = new Mock<LoggingInformationBase>();
		var wrapper = new ServiceTasksLoggerWrapper(loggerMock.Object);
		wrapper.Log(Integration.LogType.Debug, message, exception);

		AssertNoExceptionThrown(() =>
		{
			loggerMock.Verify(logger => logger.Log(It.Is<LogType>(type => type == LogType.Debug), It.Is<string>(msg => msg == $"{message} - {exception.ToString()}")), Times.Once);
		});
	}
}
