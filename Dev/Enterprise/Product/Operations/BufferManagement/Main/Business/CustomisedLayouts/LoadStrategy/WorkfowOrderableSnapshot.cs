using CargoWise.Types;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowOrderableSnapshot : IWorkflowOrderable
	{
		public WorkflowOrderableSnapshot(IWorkflowOrderable orderable, PropertyCache cache)
		{
			Identifier = orderable.Identifier;
			EffectiveNudge = cache.GetCachedValue(Identifier, BMBoardSectionViewModel.CacheConstants.EffectiveNudge, () => orderable.EffectiveNudge);
			AgreedDeliveryDate = orderable.AgreedDeliveryDate;
			ReleaseDateTime = orderable.ReleaseDateTime;
			ReleaseSequenceSortDate = orderable.ReleaseSequenceSortDate;
			CreateTime = orderable.CreateTime;
			ReleaseSequence = orderable.ReleaseSequence;
		}

		public ZGuid Identifier { get; private set; }
		public ZDecimal EffectiveNudge { get; private set; }
		public ZDateTime AgreedDeliveryDate { get; private set; }
		public ZDateTime ReleaseDateTime { get; private set; }
		public ZDateTime ReleaseSequenceSortDate { get; private set; }
		public ZDateTime CreateTime { get; private set; }
		public ZString ReleaseSequence { get; private set; }
	}
}
