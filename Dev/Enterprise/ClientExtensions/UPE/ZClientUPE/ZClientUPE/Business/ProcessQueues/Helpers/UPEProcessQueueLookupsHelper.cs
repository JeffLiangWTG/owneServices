

using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPEProcessQueueLookupsHelper : UPEProcessQueueHelperBase
	{
		public UPEProcessQueueLookupsHelper(UPEProcessQueue queue, ProcessQueueType.Enum queueType)
			: base(queue, queueType)
		{
		}

		public UPEProcessQueueLookupsHelper(NonPersistentProcessQueue queue)
			: base(queue)
		{
		}

		virtual
 public ReasonCodeDescriptionPairList GetReasonCodeList(DefaultQueueCodeDescriptionPairList queueList)
		{
			return ReasonCodeDescriptionPairList.GetReasonList((DefaultQueueCodeDescriptionPairList)Queue.Lookups.QueueList, Queue.QueueName);
		}

		virtual
 public StatusCodeDescriptionPairList GetStatusCodeList()
		{
			return StatusCodeDescriptionPairList.GetStatusList(Queue.QueueName, Queue.Status);
		}

		virtual
 public GlbStaffCollection GetTaskAssignedToList()
		{
			return new GlbStaffCollection(Factory);
		}

		public abstract DefaultQueueCodeDescriptionPairList GetQueueNameList();
	}
}
