using NLog;
using NLog.Targets;
using NUnit.Framework;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Shared.Testing.Logging
{
	public class NLogColoredConsoleTargetFactoryTest : TransactionedTestCase
	{
		public void TestGetOrCreateTargetWithConfigurationReturnsTarget()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				var factory = new NLogColoredConsoleTargetFactory();

				// Act
				var target = factory.GetOrCreateTarget();

				// Assert
				AssertType<ColoredConsoleTarget>(target);
			}
		}

		public void TestGetOrCreateTargetWithNullConfigurationReturnsTarget()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				LogManager.Configuration = null;
				var factory = new NLogColoredConsoleTargetFactory();

				// Act
				var target = factory.GetOrCreateTarget();

				// Assert
				AssertType<ColoredConsoleTarget>(target);
			}
		}

		public void TestGetOrCreateTargetWhenTargetAlreadyRegisteredReturnsTarget()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				var factory = new NLogColoredConsoleTargetFactory();
				var expectedTarget = factory.GetOrCreateTarget();
				LogManager.Configuration.AddTarget(expectedTarget);

				// Act
				var target = factory.GetOrCreateTarget();

				// Assert
				AssertEquals(expectedTarget, target);
			}
		}
	}
}
