using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class NonPersistentDeclarationQueue : NonPersistentCustomsQueue
	{
		public NonPersistentDeclarationQueue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override NonPersistentProcessQueueLookups GetNewNonPersistentProcessQueueLookups()
		{
			return new NonPersistentDeclarationQueueLookups(this);
		}

		protected override UPEProcessQueueValidationHelper GetNewUPEProcessQueueValidationHelper()
		{
			return new UPEDeclarationQueueValidationHelper(this);
		}
	}
}
