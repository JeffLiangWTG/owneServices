using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module
{
	public class CalloutBulkStatusUpdatingQueue : NonPersistentCalloutQueue
	{
		public CalloutBulkStatusUpdatingQueue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override UPEProcessQueueValidationHelper GetNewUPEProcessQueueValidationHelper()
		{
			return new CalloutBulkStatusUpdatingValidationHelper(this);
		}
	}
}
