using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module
{
	internal class ForceToCalloutBulkStatusUpdatingValidationHelperTest : CalloutBulkStatusUpdatingValidationHelperTest
	{
		public void TestValidateRemarks()
		{
			ForceToCalloutBulkStatusUpdatingQueue queue = (ForceToCalloutBulkStatusUpdatingQueue)GetNewNonPersistentProcessQueue();
			queue.Reason = "";
			AssertHasErrors("Error without the description 'forced to finance'", queue.ReasonInfo);
			queue.Reason = "xxx" + Callout.ForcedToFinanceQueueRemarks.ToLower() + "xxx";
			AssertNoErrors("No error with the description 'forced to finance'", queue.ReasonInfo);
			queue.Reason = "xxx" + Callout.ForcedToFinanceQueueRemarks.ToUpper() + "xxx";
			AssertNoErrors("No error with the description 'forced to finance'", queue.ReasonInfo);
		}

		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new ForceToCalloutBulkStatusUpdatingQueue(Factory);
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForUPEProcessQueue(UPEProcessQueue queue)
		{
			return new ForceToCalloutBulkStatusUpdatingValidationHelper((UPECalloutQueue)queue);
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return new ForceToCalloutBulkStatusUpdatingValidationHelper((CalloutBulkStatusUpdatingQueue)queue);
		}

		protected override bool ExpectedAllowEmptyQueueNameAndStatuses
		{
			get
			{
				return false;
			}
		}
	}
}
