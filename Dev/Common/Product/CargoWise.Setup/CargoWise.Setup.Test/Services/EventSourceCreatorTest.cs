using System.Diagnostics;
using CargoWise.Setup.Services;
using CargoWise.Setup.Test.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.Setup.Test.Services;

[Property("DAT:CapabilityRequirements", "ADMIN")]
internal class EventSourceCreatorTest()
{
	[Test]
	public void TestInstallWhenEventSourceDoesNotEixst()
	{
		// Arrange
		Assert.That(EventLog.SourceExists(expectedSourceName), Is.False, "Precondition");
		var logger = new Mock<ILogger<EventSourceCreator>>();
		var component = new EventSourceCreator(logger.Object);

		// Act
		component.CreateEventSource();

		// Assert
		Assert.That(EventLog.SourceExists(expectedSourceName), Is.True);
		var expectedLog = $"Creating EventSource {expectedSourceName}";
		logger.VerifyLog(LogLevel.Information, expectedLog, Times.Once());
	}

	[Test]
	public void TestInstallWhenEventSourceExists()
	{
		// Arrange
		Assert.That(EventLog.SourceExists(expectedSourceName), Is.False, "Precondition");
		CreateEventSource();

		// Act
		var logger = CreateEventSource();

		// Assert
		var expectedLog = $"EventSource {expectedSourceName} already exists, skipping.";
		logger.VerifyLog(LogLevel.Information, expectedLog, Times.Once());
		Assert.That(EventLog.SourceExists(expectedSourceName), Is.True);

		Mock<ILogger<EventSourceCreator>> CreateEventSource()
		{
			var logger = new Mock<ILogger<EventSourceCreator>>();
			var component = new EventSourceCreator(logger.Object);
			component.CreateEventSource();
			return logger;
		}
	}

	[TearDown]
	[SetUp]
	public void Cleanup()
	{
		try
		{
			EventLog.DeleteEventSource(expectedSourceName);
		}
		catch (ArgumentException ex) when (ex.Message.Contains("is not registered on machine"))
		{
		}
	}

	const string expectedSourceName = "CargoWise Next";
}
