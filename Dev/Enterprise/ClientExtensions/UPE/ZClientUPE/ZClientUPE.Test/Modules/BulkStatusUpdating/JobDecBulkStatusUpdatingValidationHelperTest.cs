using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Testing;

namespace Enterprise.Client.UPE.Module
{
	internal class JobDecBulkStatusUpdatingValidationHelperTest : UPEDeclarationQueueValidationHelperTest
	{
		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new JobDecBulkStatusUpdatingQueue(Factory);
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForUPEProcessQueue(UPEProcessQueue queue)
		{
			return new JobDecBulkStatusUpdatingValidationHelper((UPEDeclarationQueue)queue);
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return new JobDecBulkStatusUpdatingValidationHelper((JobDecBulkStatusUpdatingQueue)queue);
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
