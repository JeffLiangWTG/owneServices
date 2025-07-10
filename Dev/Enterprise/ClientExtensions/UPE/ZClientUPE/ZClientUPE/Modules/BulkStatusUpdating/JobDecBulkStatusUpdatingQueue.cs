using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module
{
	public class JobDecBulkStatusUpdatingQueue : NonPersistentDeclarationQueue
	{
		public JobDecBulkStatusUpdatingQueue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override UPEProcessQueueValidationHelper GetNewUPEProcessQueueValidationHelper()
		{
			return new JobDecBulkStatusUpdatingValidationHelper(this);
		}
	}
}
