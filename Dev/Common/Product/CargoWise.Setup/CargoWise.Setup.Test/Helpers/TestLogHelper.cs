using Microsoft.Extensions.Logging;
using Moq;

namespace CargoWise.Setup.Test.Helpers;

static class TestLogHelper
{
	public static void VerifyLog<TLogCategory>(this Mock<ILogger<TLogCategory>> loggerMock, LogLevel level, string message, Times times)
	{
		loggerMock.Verify(x => x.Log(
			level,
			It.IsAny<EventId>(),
			It.Is<It.IsAnyType>((v, t) => string.Equals(message, v.ToString(), StringComparison.InvariantCultureIgnoreCase)),
			It.IsAny<Exception>(),
			It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			times);
	}

	// Verify with any message
	public static void VerifyLog<TLogCategory>(this Mock<ILogger<TLogCategory>> loggerMock, LogLevel level, Times times)
	{
		loggerMock.Verify(x => x.Log(
				level,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			times);
	}

	public static void SetupLogCallback<TLogCategory>(this Mock<ILogger<TLogCategory>> loggerMock, Action action, string? onMessage = null)
	{
		loggerMock.Setup(x => x.Log(
			It.IsAny<LogLevel>(),
			It.IsAny<EventId>(),
			It.Is<It.IsAnyType>((v, t) => onMessage != null && onMessage == v.ToString()),
			It.IsAny<Exception>(),
			It.IsAny<Func<It.IsAnyType, Exception?, string>>())
		).Callback(action);
	}
}
