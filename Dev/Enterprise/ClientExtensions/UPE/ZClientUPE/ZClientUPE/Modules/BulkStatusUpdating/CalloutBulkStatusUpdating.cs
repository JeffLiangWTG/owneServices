using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Module
{
	public class CalloutBulkStatusUpdating : BulkStatusUpdating
	{
		public CalloutBulkStatusUpdating(BusinessObjectFactory factory, IProcessQueueParent[] itemsToBulkUpdate)
			: base(factory, itemsToBulkUpdate)
		{
		}

		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new CalloutBulkStatusUpdatingQueue(Factory);
		}

		protected override ProcessQueueType.Enum QueueTypeToUpdate
		{
			get { return ProcessQueueType.Enum.Commercial; }
		}

		protected override bool EnableAssignedTo
		{
			get { return false; }
		}
	}
}
