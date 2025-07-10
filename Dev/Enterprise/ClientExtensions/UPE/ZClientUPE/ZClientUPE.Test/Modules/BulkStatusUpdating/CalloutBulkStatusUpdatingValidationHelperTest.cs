using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Testing;

namespace Enterprise.Client.UPE.Module
{
	internal class CalloutBulkStatusUpdatingValidationHelperTest : UPECommercialQueueValidationHelperTest
	{
		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new CalloutBulkStatusUpdatingQueue(Factory);
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForUPEProcessQueue(UPEProcessQueue queue)
		{
			return new CalloutBulkStatusUpdatingValidationHelper((UPECalloutQueue)queue);
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return new CalloutBulkStatusUpdatingValidationHelper((CalloutBulkStatusUpdatingQueue)queue);
		}

		protected override bool ExpectedAllowEmptyQueueNameAndStatuses
		{
			get
			{
				return true;
			}
		}
	}
}
