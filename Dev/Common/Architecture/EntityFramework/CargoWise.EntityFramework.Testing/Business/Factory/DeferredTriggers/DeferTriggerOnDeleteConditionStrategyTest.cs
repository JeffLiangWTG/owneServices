using System;
using CargoWise.Application;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DeferTriggerOnDeleteConditionStrategyTest : TestCaseWithFactory
	{
		public void TestRunType_ReturnsDelete()
		{
			IDeferTriggerConditionStrategy strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IDeferTriggerOnDeleteConditionStrategy));
			AssertEquals(strategy.RunType, TriggerRunType.Delete);
		}

		public void TestShouldDeferTrigger_ThrowsException()
		{
			IDeferTriggerConditionStrategy strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IDeferTriggerOnDeleteConditionStrategy));
			var bizO = Factory.New<DummyBusinessObject>();
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot do additional checks for Deleted Objects.", () => strategy.ShouldDeferTrigger(bizO));
		}
	}
}
