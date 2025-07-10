using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ITriggerConditions : IEventReferenceConditions
	{
		ZShort TriggerFiredCountdown { get; set; }
		ZString TriggerEventCode { get; set; }
		ZString TriggerFieldName { get; set; }
		ZBool Cascading { get; }
		ZString CascadingContext { get; }
		IWorkflowDescriptor Descriptor { get; }
		BusinessObject Job { get; }
	}
}
