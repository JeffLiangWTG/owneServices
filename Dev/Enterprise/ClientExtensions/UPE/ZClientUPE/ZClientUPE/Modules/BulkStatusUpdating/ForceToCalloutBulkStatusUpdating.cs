using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Module
{
	public class ForceToCalloutBulkStatusUpdating : CalloutBulkStatusUpdating
	{
		public ForceToCalloutBulkStatusUpdating(BusinessObjectFactory factory, IProcessQueueParent[] itemsToBulkUpdate)
			: base(factory, itemsToBulkUpdate)
		{
		}

		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new ForceToCalloutBulkStatusUpdatingQueue(Factory);
		}

		protected override void UpdateItem(IProcessQueueParent item)
		{
			base.UpdateItem(item);
			Callout callout = (Callout)item;
			if (callout.HasChanges)
			{
				callout.IsExcludedFromBISIWarning = true;
			}
		}
	}
}
