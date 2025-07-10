using CargoWise.Application;
using CargoWise.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DeferTriggerOnUpdateConditionStrategyTest : TestCaseWithFactory
	{
		public void TestRunType_ReturnsInsertOrUpdate()
		{
			IDeferTriggerConditionStrategy strategy = new DummyUpdateConditionStrategy();
			AssertEquals(strategy.RunType, TriggerRunType.InsertOrUpdate);
		}

		public void TestShouldDeferTrigger_ReturnsTrueWhenTriggeringColumnIsUpdated()
		{
			IDeferTriggerConditionStrategy strategy = new DummyUpdateConditionStrategy();
			var bizO = Factory.New<DummyBusinessObject>();
			Factory.Save();
			AssertEquals("Trigger should not be deferred if there are no changes to the bizO", false, strategy.ShouldDeferTrigger(bizO));

			bizO.Z0_IsSystem = !bizO.Z0_IsSystem;
			AssertEquals("Trigger should not be deferred if a column that does not require trigger deferral is updated", false, strategy.ShouldDeferTrigger(bizO));

			bizO.Z0_Decimal = 10m;
			AssertEquals("Trigger should be deferred if a column that requires trigger deferral is updated", true, strategy.ShouldDeferTrigger(bizO));
		}

		public void TestDeferAndReturnTriggers_ReturnsNoneTriggersWhenShouldInsertsBeDeferredIsFalseOnInsert()
		{
			var dummyStrategy = new DummyUpdateConditionStrategy();
			dummyStrategy.ShouldInsertsBeDeferredForTesting = false;
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(dummyStrategy))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			{
				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();

				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();

				AssertEquals("Precondition - bizO1 is not in database", false, bizO1.IsInDatabase);
				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1 });
				AssertEquals("Should return none deferred triggers", 0, deferredTriggers.Count);
			}
		}

		public void TestDeferAndReturnTriggers_ReturnsTriggersWhenShouldInsertsBeDeferredIsTrueOnInsert()
		{
			var dummyStrategy = new DummyUpdateConditionStrategy();
			dummyStrategy.ShouldInsertsBeDeferredForTesting = true;
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(dummyStrategy))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			{
				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();

				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();

				AssertEquals("Precondition - bizO1 is not in database", false, bizO1.IsInDatabase);
				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1 });
				AssertEquals("Should return the deferred trigger", 1, deferredTriggers.Count);
			}
		}
	}
}
