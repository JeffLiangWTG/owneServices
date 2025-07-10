using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IEventReferenceConditions
	{
		ZString TriggerCondition { get; set; }
		ZString TriggerConditionValue { get; set; }
	}
}
