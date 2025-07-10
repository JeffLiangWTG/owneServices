using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Testing;

namespace Enterprise.Client.UPE.Module
{
	internal class AirCargoBulkStatusUpdatingValidationHelperTest : UPECargoReportQueueValidationHelperTest
	{
		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new AirCargoBulkStatusUpdatingQueue(Factory);
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForUPEProcessQueue(UPEProcessQueue queue)
		{
			return new AirCargoBulkStatusUpdatingValidationHelper((UPECargoReportQueue)queue);
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return new AirCargoBulkStatusUpdatingValidationHelper((AirCargoBulkStatusUpdatingQueue)queue);
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
