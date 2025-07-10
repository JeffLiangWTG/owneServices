using System;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared.Testing.Logging;
using Moq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.EnvironmentCheckers;

class LoggerConfigurationCheckerTest : TestCase
{
	protected override void SetUp()
	{
		base.SetUp();
		serviceTaskMock = new Mock<IServiceTaskHandler>();
		serviceTaskConfigMock = new Mock<IHostedServiceAttribute>();
		serviceTaskConfigMock.Setup(x => x.Code).Returns("xxx");
		serviceTaskConfigMock.Setup(x => x.TypeName).Returns("typeName1");
		serviceTaskConfigMock.Setup(x => x.TypeAssemblyName).Returns("assemblyName1");
		serviceTaskMock.Setup(x => x.HostedServiceAttribute).Returns(serviceTaskConfigMock.Object);
	}
	[ExpectNoExceptions]
	public void TestMatches()
	{
		using (LoggerTestHelper.TemporaryLoggingConfiguration())
		{
			// Arrange
			var configuration = new LoggingConfiguration();
			configuration.AddRule(new LoggingRule("test"));
			LogManager.Configuration.AddRule(new LoggingRule("test"));

			var checker = new LoggerConfigurationChecker();

			// Act
			// Assert
			checker.Initialize(It.IsAny<IRunCommandInfo>());
			checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object);
		}
	}

	public void TestDoesNotMatchAdded1Rule()
	{
		AssertDoesNotMatch(
			"Logging rules added: [badRule]",
			new[] { "badRule" },
			Array.Empty<string>(),
			Array.Empty<string>(),
			Array.Empty<string>());
	}

	public void TestDoesNotMatchRemoved1Rule()
	{
		AssertDoesNotMatch(
			"Logging rules removed: [goodRule1]",
			Array.Empty<string>(),
			new[] { "goodRule1" },
			Array.Empty<string>(),
			Array.Empty<string>());
	}

	public void TestDoesNotMatchAdded1Target()
	{
		AssertDoesNotMatch(
			"Logging targets added: [badTarget]",
			Array.Empty<string>(),
			Array.Empty<string>(),
			new[] { "badTarget" },
			Array.Empty<string>());
	}

	public void TestDoesNotMatchRemoved1Target()
	{
		AssertDoesNotMatch(
			"Logging targets removed: [goodTarget1]",
			Array.Empty<string>(),
			Array.Empty<string>(),
			Array.Empty<string>(),
			new[] { "goodTarget1" });
	}

	public void TestDoesNotMatchAdded2RuleRemoved2RuleAdded2TargetRemoved2Target()
	{
		AssertDoesNotMatch(
			"Logging rules added: [badRule1, badRule2]\r\nLogging rules removed: [goodRule1, goodRule2]\r\nLogging targets added: [badTarget1, badTarget2]\r\nLogging targets removed: [goodTarget1, goodTarget2]",
			new[] { "badRule1", "badRule2" },
			new[] { "goodRule1", "goodRule2" },
			new[] { "badTarget1", "badTarget2" },
			new[] { "goodTarget1", "goodTarget2" });
	}

	public void AssertDoesNotMatch(string expectedViolations, string[] addedRules, string[] removedRules,
		string[] addedTargets, string[] removedTargets)
	{
		using (LoggerTestHelper.TemporaryLoggingConfiguration())
		{
			// Arrange
			using var debugTarget = new DebugTarget("goodTarget1");
			using var debugTarget2 = new DebugTarget("goodTarget2");
			LogManager.Configuration.AddRule(new LoggingRule("goodRule1"));
			LogManager.Configuration.AddRule(new LoggingRule("goodRule2"));
			LogManager.Configuration.AddTarget(debugTarget);
			LogManager.Configuration.AddTarget(debugTarget2);

			var checker = new LoggerConfigurationChecker();
			checker.Initialize(It.IsAny<IRunCommandInfo>());

			foreach (var rule in addedRules)
			{
				LogManager.Configuration.AddRule(new LoggingRule(rule));
			}

			foreach (var rule in removedRules)
			{
				LogManager.Configuration.RemoveRuleByName(rule);
			}

			foreach (var target in addedTargets)
			{
				using var debugTarget3 = new DebugTarget(target);
				LogManager.Configuration.AddTarget(debugTarget3);
			}

			foreach (var target in removedTargets)
			{
				LogManager.Configuration.RemoveTarget(target);
			}

			// Act
			var exception = AssertExceptionThrown<LoggerConfigurationCorruptedException>(() =>
			{
				checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object);
			});

			// Assert
			AssertNotNull(exception);
			AssertEquals(true, exception.Message.Contains(exception.Message));
			AssertEquals(true, exception.Message.StartsWith("The service task 'xxx - typeName1, assemblyName1'"));
			AssertEquals(true,
				exception.Message.EndsWith(
					$"has corrupted the logging configuration. Ensure that the logging configuration is not altered during the run and that added rules and targets are removed at the end of the service task's run.\r\n{expectedViolations}"));
		}
	}

	public void TestLoadConfigurationTriggersFailure()
	{
		using (LoggerTestHelper.TemporaryLoggingConfiguration())
		{
			// Arrange
			var checker = new LoggerConfigurationChecker();
			checker.Initialize(It.IsAny<IRunCommandInfo>());

			var logger = LogManager.Setup().LoadConfiguration(builder =>
				{
					builder.ForLogger(ruleName: "someRule").FilterMinLevel(LogLevel.Error)
						.WriteTo(new DebugTarget("someTarget"));
				})
				.GetLogger("someRule");

			// Act
			var exception = AssertExceptionThrown<LoggerConfigurationCorruptedException>(() =>
			{
				checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object);
			});

			// Assert
			AssertNotNull(exception);
			AssertEquals(true, exception.Message.Contains(exception.Message));
			AssertEquals(true,
				exception.Message.StartsWith(
					"The service task 'xxx - typeName1, assemblyName1'"));
			AssertEquals(true,
				exception.Message.EndsWith(
					"has corrupted the logging configuration. Ensure that the logging configuration is not altered during the run and that added rules and targets are removed at the end of the service task's run.\r\nLogging rules added: [someRule]\r\nLogging targets added: [someTarget]"));
		}
	}

	Mock<IServiceTaskHandler> serviceTaskMock;
	Mock<IHostedServiceAttribute> serviceTaskConfigMock;
}
