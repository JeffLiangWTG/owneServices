namespace CargoWise.EntityFramework.Testing
{
	interface IDummyDependentUpdateConditionStrategy : IDeferTriggerConditionStrategy
	{
	}

	class DummyDependentUpdateConditionStrategy : IDummyDependentUpdateConditionStrategy
	{
		public TriggerRunType RunType => TriggerRunType.InsertOrUpdate;

		public bool ShouldDeferTrigger(BusinessObject businessEntity) => DeferTriggerForTesting;

		public bool DeferTriggerForTesting { get; set; } = true;
	}
}
