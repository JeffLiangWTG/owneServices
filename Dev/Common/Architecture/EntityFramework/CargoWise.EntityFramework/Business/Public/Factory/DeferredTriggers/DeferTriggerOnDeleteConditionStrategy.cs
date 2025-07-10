using System;

namespace CargoWise.EntityFramework
{
	sealed class DeferTriggerOnDeleteConditionStrategy : IDeferTriggerOnDeleteConditionStrategy
	{
		DeferTriggerOnDeleteConditionStrategy() { }
		TriggerRunType IDeferTriggerConditionStrategy.RunType => TriggerRunType.Delete;
		bool IDeferTriggerConditionStrategy.ShouldDeferTrigger(BusinessObject businessEntity) => throw new InvalidOperationException("Cannot do additional checks for Deleted Objects.");
	}
}
