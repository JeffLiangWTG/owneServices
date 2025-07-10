using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class NonPersistentCargoReportQueue : NonPersistentCustomsQueue
	{
		public NonPersistentCargoReportQueue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override NonPersistentProcessQueueLookups GetNewNonPersistentProcessQueueLookups()
		{
			return new NonPersistentCargoReportQueueLookups(this);
		}

		protected override UPEProcessQueueValidationHelper GetNewUPEProcessQueueValidationHelper()
		{
			return new UPECargoReportQueueValidationHelper(this);
		}
	}
}
