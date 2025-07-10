
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPECustomsQueueValidationHelper : UPEProcessQueueValidationHelper
	{
		public UPECustomsQueueValidationHelper(UPEProcessQueue queue)
			: base(queue, ProcessQueueType.Enum.Customs)
		{
		}

		public UPECustomsQueueValidationHelper(NonPersistentCustomsQueue queue)
			: base(queue)
		{
		}
	}
}
