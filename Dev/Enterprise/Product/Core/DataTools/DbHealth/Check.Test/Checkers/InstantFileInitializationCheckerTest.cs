using System;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check
{
	[TestedType(typeof(InstantFileInitializationChecker))]
	sealed class InstantFileInitializationCheckerTest : CheckerTestCaseBase
	{
		public void TestReadErrorLogRecords_ErrorsAreLoggedOnServiceTaskLog()
		{
			var logger = new TestServiceLogger();
			var slowStrategy = new InstantFileInitializationSlowQueryStrategy("Select 'SingleColumn' Column0", _ => 18);
			IChecker checker = new InstantFileInitializationChecker(slowStrategy);
			var warningList = new DbHealthWarningList();
			checker.Check(null, warningList, logger);
			AssertEquals("Number of logs", 1, logger.Count);
			Assert("Log error", logger[0].StartsWith("Error|Failed to read SQL Server error log.|System.IndexOutOfRangeException: Index was outside the bounds of the array.", StringComparison.OrdinalIgnoreCase));
		}

		public void TestCheckGivesNoWarningsWhenFirstStrategyReturnsTrue()
		{
			var strategy1 = new InstantFileInitializationStrategyForTests(true);
			var strategy2 = new InstantFileInitializationStrategyForTests(false);
			IChecker checker = new InstantFileInitializationChecker(strategy1, strategy2);
			var warningList = new DbHealthWarningList();

			checker.Check(null, warningList, null);

			Assert("First strategy was called", strategy1.WasCalled);
			Assert("Second strategy was not called", !strategy2.WasCalled);
			AssertNoHealthWarnings(warningList);
		}

		public void TestCheckGivesWarningsWhenFirstStrategyReturnsFalse()
		{
			var strategy1 = new InstantFileInitializationStrategyForTests(false);
			var strategy2 = new InstantFileInitializationStrategyForTests(true);
			IChecker checker = new InstantFileInitializationChecker(strategy1, strategy2);
			var warningList = new DbHealthWarningList();

			checker.Check(null, warningList, null);

			Assert("First strategy was called", strategy1.WasCalled);
			Assert("Second strategy was not called", !strategy2.WasCalled);
			AssertExpectedHealthWarnings(warningList);
		}

		public void TestCheckGivesNoWarningsWhenSecondStrategyReturnsTrue()
		{
			var strategy1 = new InstantFileInitializationStrategyForTests(null);
			var strategy2 = new InstantFileInitializationStrategyForTests(true);
			var strategy3 = new InstantFileInitializationStrategyForTests(false);
			IChecker checker = new InstantFileInitializationChecker(strategy1, strategy2, strategy3);
			var warningList = new DbHealthWarningList();

			checker.Check(null, warningList, null);

			Assert("First strategy was called", strategy1.WasCalled);
			Assert("Second strategy was called", strategy2.WasCalled);
			Assert("Third strategy was not called", !strategy3.WasCalled);
			AssertNoHealthWarnings(warningList);
		}

		public void TestCheckGivesWarningWhenSecondStrategyReturnsFalse()
		{
			var strategy1 = new InstantFileInitializationStrategyForTests(null);
			var strategy2 = new InstantFileInitializationStrategyForTests(false);
			var strategy3 = new InstantFileInitializationStrategyForTests(true);
			IChecker checker = new InstantFileInitializationChecker(strategy1, strategy2, strategy3);
			var warningList = new DbHealthWarningList();

			checker.Check(null, warningList, null);

			Assert("First strategy was called", strategy1.WasCalled);
			Assert("Second strategy was called", strategy2.WasCalled);
			Assert("Third strategy was not called", !strategy3.WasCalled);
			AssertExpectedHealthWarnings(warningList);
		}

		public void TestCheckGivesWarningWhenNoStrategyCanDecide()
		{
			var strategy1 = new InstantFileInitializationStrategyForTests(null);
			var strategy2 = new InstantFileInitializationStrategyForTests(null);
			IChecker checker = new InstantFileInitializationChecker(strategy1, strategy2);
			var warningList = new DbHealthWarningList();

			checker.Check(null, warningList, null);

			Assert("First strategy was called", strategy1.WasCalled);
			Assert("Second strategy was called", strategy2.WasCalled);
			AssertUndecidedHealthWarnings(warningList);
		}

		public void TestCtorCreatesExpetcedStrategies()
		{
			var expectedStrategyTypes = new[]
			{
				typeof(InstantFileInitializationHostLocationStrategy),
				typeof(InstantFileInitializationFastQueryStrategy),
				typeof(InstantFileInitializationSlowQueryStrategy)
			};

			var checker = new InstantFileInitializationChecker();

			AssertSequencesEqual(expectedStrategyTypes, checker.StrategyTypes);
		}

		public void TestCheckInstantFileInitializationIsEnabled()
		{
			var warningList = new DbHealthWarningList();
			IChecker checker = new InstantFileInitializationChecker();

			checker.Check(null, warningList, null);

			AssertEquals("Number of DB Health Warnings", 0, warningList.Count);
		}

		static void AssertExpectedHealthWarnings(DbHealthWarningList warningList)
		{
			AssertEquals("Number of DB Health Warnings when initialization is NOT enabled", 1, warningList.Count);
			AssertEquals("Warning description", "Instant File Initialization", warningList[0].Description);
			AssertEquals("Warning action", "Instant file initialization should be enabled on your SQL Server instance.", warningList[0].Action);
			AssertEquals("Warning type", "Instant File Initialization", warningList[0].WarningType);
		}

		static void AssertUndecidedHealthWarnings(DbHealthWarningList warningList)
		{
			AssertEquals("Number of DB Health Warnings when initialization can't be detected", 1, warningList.Count);
			AssertEquals("Warning description", "Instant File Initialization", warningList[0].Description);
			AssertEquals("Warning action", "Unable to determine if instant file initialization is enabled.", warningList[0].Action);
			AssertEquals("Warning type", "Instant File Initialization", warningList[0].WarningType);
		}

		static void AssertNoHealthWarnings(DbHealthWarningList warningList)
		{
			AssertEquals("Number of DB Health Warnings when initialization is enabled", 0, warningList.Count);
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new InstantFileInitializationChecker();
		}
	}
}
