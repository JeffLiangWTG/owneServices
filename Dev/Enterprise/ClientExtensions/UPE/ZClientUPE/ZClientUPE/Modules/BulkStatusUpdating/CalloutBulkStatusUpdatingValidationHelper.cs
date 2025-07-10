
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module
{
	public class CalloutBulkStatusUpdatingValidationHelper : UPECommercialQueueValidationHelper
	{
		public CalloutBulkStatusUpdatingValidationHelper(UPECalloutQueue queue)
			: base(queue)
		{
		}

		public CalloutBulkStatusUpdatingValidationHelper(CalloutBulkStatusUpdatingQueue queue)
			: base(queue)
		{
		}

		protected override bool AllowEmptyQueueNameAndStatuses
		{
			get { return true; }
		}
	}
}
