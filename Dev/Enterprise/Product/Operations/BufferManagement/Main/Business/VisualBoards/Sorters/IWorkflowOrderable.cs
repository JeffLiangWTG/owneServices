using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public interface IWorkflowOrderable : IIdentified
	{
		// It is probably sensible to break this up into several interfaces, as some properties are for sorting under different contexts.
		ZDecimal EffectiveNudge { get; }
		ZDateTime ReleaseDateTime { get; }
		ZDateTime ReleaseSequenceSortDate { get; }
		ZDateTime CreateTime { get; }
		ZString ReleaseSequence { get; }
		ZDateTime AgreedDeliveryDate { get; }
	}
}
