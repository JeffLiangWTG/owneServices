using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IMilestone
	{
		ZDateTime ActualDate { get; }
		ZDateTime EstimatedDate { get; }
		ZInt Sequence { get; }
		ZString ConditionReference { get; }
		ZString ConditionType { get; }
		ZString Description { get; }
		ZString EventCode { get; }
	}
}
