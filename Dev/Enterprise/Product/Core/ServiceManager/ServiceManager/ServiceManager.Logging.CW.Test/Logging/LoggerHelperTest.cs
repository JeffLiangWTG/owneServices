using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NUnit.Framework;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Shared.Testing.Logging
{
	public class LoggerHelperTests : TestCase
	{
		public void TestConfigureTargetAndRuleWithFactoryAddsTargetAndRule()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange, Act
				LoggerHelper.ConfigureTargetAndRule(new NLogColoredConsoleTargetFactory(), NLogColoredConsoleTargetFactory.TargetName, LogLevel.Info);

				// Assert
				var target = LogManager.Configuration.FindTargetByName(NLogColoredConsoleTargetFactory.TargetName);
				AssertNotNull(target);

				var rule = LogManager.Configuration.FindRuleByName(NLogColoredConsoleTargetFactory.TargetName);
				AssertNotNull(rule);
				AssertEquals(LogLevel.Info, rule.Levels.Min());
			}
		}

		public void TestConfigureTargetAndRuleWithExistingTargetAndRuleDoeNotAddTargetAndRule()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				LoggerHelper.ConfigureTargetAndRule(new NLogColoredConsoleTargetFactory(), NLogColoredConsoleTargetFactory.TargetName, LogLevel.Info);

				// Act
				LoggerHelper.ConfigureTargetAndRule(new NLogColoredConsoleTargetFactory(), NLogColoredConsoleTargetFactory.TargetName, LogLevel.Info);

				// Assert
				AssertEquals(1, LogManager.Configuration.AllTargets.Count);
				AssertEquals(1, LogManager.Configuration.LoggingRules.Count);
			}
		}

		public void TestConfigureTargetWithExistingConfigurationAddsTargetDoeNotAddRule()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				LoggerHelper.ConfigureTargetAndRule(new NLogColoredConsoleTargetFactory(), string.Empty, LogLevel.Info);

				// Act
				LoggerHelper.ConfigureTargetAndRule(new NLogEventLogTargetFactory(), string.Empty, LogLevel.Info);

				// Assert
				AssertEquals(2, LogManager.Configuration.AllTargets.Count);
				AssertEquals(1, LogManager.Configuration.LoggingRules.Count);
				AssertEquals(2, LogManager.Configuration.LoggingRules[0].Targets.Count);
			}
		}

		public void TestConfigureTargetWithMultipleThreadsAddsSingleTargetAndRule()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			using (var syncStart = new ManualResetEvent(false))
			{
				// Arrange, Act
				var tasks = Enumerable.Range(0, 10).Select(o =>
					Task.Run(() =>
					{
						syncStart.WaitOne();
						LoggerHelper.ConfigureTargetAndRule(new NLogColoredConsoleTargetFactory(), NLogColoredConsoleTargetFactory.TargetName, LogLevel.Info);
					}))
					.ToArray();

				_ = syncStart.Set();

				Task.WaitAll(tasks);

				// Assert
				AssertEquals(1, LogManager.Configuration.AllTargets.Count);
				AssertEquals(1, LogManager.Configuration.LoggingRules.Count);
			}
		}
	}
}
