
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module
{
	public class JobDecBulkStatusUpdatingValidationHelper : UPEDeclarationQueueValidationHelper
	{
		public JobDecBulkStatusUpdatingValidationHelper(UPEDeclarationQueue queue)
			: base(queue)
		{
		}

		public JobDecBulkStatusUpdatingValidationHelper(JobDecBulkStatusUpdatingQueue queue)
			: base(queue)
		{
		}

		protected override bool AllowEmptyQueueNameAndStatuses
		{
			get { return true; }
		}
	}
}
