using System;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using WTG.ApplicationLogging.Abstractions;
using WTG.ApplicationLogging.Builder;

namespace CargoWise.NGenInstaller.Testing
{
	public class LoggingTest
	{
		[TestCase]
		public void TestLoggingConfigurationIsValid()
		{
			// Arrange & Act
			using var loggerFactory = ApplicationLoggingBuilder.Build(o => o.Configure(Product.CargoWise));

			// Assert
			Assert.That(loggerFactory, Is.Not.Null);
		}

		[TestCase]
		public void TestLoggingOutputIsValid()
		{
			// Arrange
			using var process = System.Diagnostics.Process.GetCurrentProcess();
			using var loggingHelper = new LoggingTestHelper(Product.CargoWise, process.ProcessName);
			loggingHelper.CleanUpLogs();

			var loggerFactory = ApplicationLoggingBuilder.Build(o => o.Configure(Product.CargoWise));

			var logger = loggerFactory.CreateLogger("LoggerName");

			// Act
			logger.LogError(new ArgumentException(), "Test");

			// Assert
			loggerFactory.Dispose();

			var logEntryString = loggingHelper.GetLogEntries().Single();
			var logEntry = JsonNode.Parse(logEntryString);

			Assert.Multiple(() =>
			{
				Assert.That(logEntry["message"].GetValue<string>(), Is.EqualTo("Test"));
				Assert.That(logEntry["level"].GetValue<string>(), Is.EqualTo("Error"));
				Assert.That(logEntry["sequenceId"].GetValue<string>(), Is.EqualTo("1"));
				Assert.That(logEntry["exception"].GetValue<string>(), Does.StartWith(typeof(ArgumentException).FullName));

				var eventTime = DateTime.ParseExact(
					logEntry["eventTime"].GetValue<string>(),
					"yyyy-MM-ddTHH:mm:ss.fffK",
					CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeUniversal);

				Assert.That(eventTime, Is.Not.EqualTo(DateTime.MinValue));
			});
		}
	}
}
