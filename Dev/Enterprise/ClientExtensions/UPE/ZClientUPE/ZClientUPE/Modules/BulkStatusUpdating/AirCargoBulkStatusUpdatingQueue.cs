using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module
{
	public class AirCargoBulkStatusUpdatingQueue : NonPersistentCargoReportQueue
	{
		public AirCargoBulkStatusUpdatingQueue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override UPEProcessQueueValidationHelper GetNewUPEProcessQueueValidationHelper()
		{
			return new AirCargoBulkStatusUpdatingValidationHelper(this);
		}
	}
}
