

using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPECustomsQueueLookupsHelper : UPEProcessQueueLookupsHelper
	{
		public UPECustomsQueueLookupsHelper(UPEProcessQueue queue)
			: base(queue, ProcessQueueType.Enum.Customs)
		{
		}

		public UPECustomsQueueLookupsHelper(NonPersistentCustomsQueue queue)
			: base(queue)
		{
		}

		public override DefaultQueueCodeDescriptionPairList GetQueueNameList()
		{
			return GetCustomsQueueList();
		}

		protected abstract CustomsQueueCodeDescriptionPairList GetCustomsQueueList();
	}
}
