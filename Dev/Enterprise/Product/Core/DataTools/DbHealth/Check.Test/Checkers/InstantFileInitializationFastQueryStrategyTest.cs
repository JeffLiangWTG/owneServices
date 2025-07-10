using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check
{
	sealed class InstantFileInitializationFastQueryStrategyTest : TestCase
	{
		readonly TestServiceLogger logger = new TestServiceLogger();

		public void TestInstantIsInstantFileInitializationEnabledWhenQueryGivesYReturnsTrue()
		{
			var strategy = new InstantFileInitializationFastQueryStrategy("select N'Y'");

			var result = strategy.IsInstantFileInitializationEnabled(logger);

			AssertEquals(true, result);
		}

		public void TestInstantIsInstantFileInitializationEnabledWhenQueryGivesNReturnsFalse()
		{
			var strategy = new InstantFileInitializationFastQueryStrategy("select N'N'");

			var result = strategy.IsInstantFileInitializationEnabled(logger);

			AssertEquals(false, result);
		}

		public void TestInstantIsInstantFileInitializationEnabledWhenQueryGivesNullReturnsNull()
		{
			var strategy = new InstantFileInitializationFastQueryStrategy("select null");

			var result = strategy.IsInstantFileInitializationEnabled(logger);

			AssertEquals(null, result);
		}

		public void TestInstantIsInstantFileInitializationEnabledWhenQueryGivesUnexpectedResultReturnsNull()
		{
			var strategy = new InstantFileInitializationFastQueryStrategy("select N'X'");

			var result = strategy.IsInstantFileInitializationEnabled(logger);

			AssertEquals(null, result);
		}
	}
}
