using CargoWise.Types;

namespace Enterprise.Integration.Workflow.Triggers
{
	public interface ICanTriggerActionRunAgain
	{
		ZBool CanRunAgain(ITriggerAction action);
	}
}
