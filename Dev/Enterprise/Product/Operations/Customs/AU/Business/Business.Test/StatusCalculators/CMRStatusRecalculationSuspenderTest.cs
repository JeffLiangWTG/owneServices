using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRStatusRecalculationSuspenderTest : TestCaseWithFactory
	{
		public void TestStateChanges()
		{
			AssertEquals(false, CMRStatusRecalculationSuspender.IsStatusRecalculationSuspended(Factory));

			CMRStatusRecalculationSuspender.SuspendStatusRecalculation(Factory);
			AssertEquals(true, CMRStatusRecalculationSuspender.IsStatusRecalculationSuspended(Factory));

			// checking flag second time to make sure that reading flag does not change it
			AssertEquals(true, CMRStatusRecalculationSuspender.IsStatusRecalculationSuspended(Factory));

			CMRStatusRecalculationSuspender.SuspendStatusRecalculation(Factory);
			AssertEquals(true, CMRStatusRecalculationSuspender.IsStatusRecalculationSuspended(Factory));

			// resume should always resume recalculation, regardless of the number of previous calls to SuspendStatusRecalculations
			CMRStatusRecalculationSuspender.ResumeStatusRecalculation(Factory);
			AssertEquals(false, CMRStatusRecalculationSuspender.IsStatusRecalculationSuspended(Factory));
		}

		public void TestStateDoesNotLeakToOtherFactories()
		{
			var existingFactory = NewFactory();

			CMRStatusRecalculationSuspender.SuspendStatusRecalculation(Factory);
			AssertEquals(true, CMRStatusRecalculationSuspender.IsStatusRecalculationSuspended(Factory));

			var newFactory = NewFactory();

			// while sharing status between factories may be desirable in some scenarios, it can cause many side-effects
			AssertEquals(false, CMRStatusRecalculationSuspender.IsStatusRecalculationSuspended(existingFactory));
			AssertEquals(false, CMRStatusRecalculationSuspender.IsStatusRecalculationSuspended(newFactory));
		}
	}
}
