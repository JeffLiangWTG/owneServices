namespace CargoWise.EntityFramework
{
	public interface IDeferTriggerConditionStrategy
	{
		bool ShouldDeferTrigger(BusinessObject businessEntity);
		TriggerRunType RunType { get; }
	}
}
