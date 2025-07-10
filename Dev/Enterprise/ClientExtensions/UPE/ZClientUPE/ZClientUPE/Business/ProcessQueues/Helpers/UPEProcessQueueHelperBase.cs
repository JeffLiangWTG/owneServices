using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPEProcessQueueHelperBase
	{
		public UPEProcessQueueHelperBase(UPEProcessQueue queue, ProcessQueueType.Enum queueType)
		{
			UPEActiveProcessQueue activeQueue = new UPEActiveProcessQueue(queue);
			activeQueue.QueueType = queueType;
			this.Queue = activeQueue;
			this.Factory = queue.Factory;
		}

		public UPEProcessQueueHelperBase(NonPersistentProcessQueue queue)
		{
			this.Queue = queue;
			this.Factory = queue.Factory;
		}

		public readonly BusinessObjectFactory Factory;
		public readonly IActiveProcessQueue Queue;
	}
}
