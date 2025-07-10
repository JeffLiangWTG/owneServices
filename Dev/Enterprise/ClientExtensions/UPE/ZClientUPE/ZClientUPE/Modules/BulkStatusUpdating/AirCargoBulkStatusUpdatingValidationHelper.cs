
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module
{
	public class AirCargoBulkStatusUpdatingValidationHelper : UPECargoReportQueueValidationHelper
	{
		public AirCargoBulkStatusUpdatingValidationHelper(UPECargoReportQueue queue)
			: base(queue)
		{
		}

		public AirCargoBulkStatusUpdatingValidationHelper(AirCargoBulkStatusUpdatingQueue queue)
			: base(queue)
		{
		}

		protected override bool AllowEmptyQueueNameAndStatuses
		{
			get { return true; }
		}
	}
}
