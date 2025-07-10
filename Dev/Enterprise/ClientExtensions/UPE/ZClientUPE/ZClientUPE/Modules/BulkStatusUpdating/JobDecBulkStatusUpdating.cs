using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Module
{
	public class JobDecBulkStatusUpdating : BulkStatusUpdating
	{
		public JobDecBulkStatusUpdating(BusinessObjectFactory factory, IProcessQueueParent[] itemsToBulkUpdate)
			: base(factory, itemsToBulkUpdate)
		{
		}

		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new JobDecBulkStatusUpdatingQueue(Factory);
		}

		protected override ProcessQueueType.Enum QueueTypeToUpdate
		{
			get { return ProcessQueueType.Enum.Customs; }
		}

		protected override bool EnableAssignedTo
		{
			get { return true; }
		}
	}
}
