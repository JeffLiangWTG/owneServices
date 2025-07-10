namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPEDeclarationQueueLookupsHelperTest : UPECustomsQueueLookupsHelperTestCase
	{
		protected override DefaultQueueCodeDescriptionPairList ExpectedQueueList
		{
			get
			{
				return new DeclarationQueueCodeDescriptionPairList();
			}
		}

		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new NonPersistentDeclarationQueue(Factory);
		}

		protected override UPEProcessQueue GetNewUPEProcessQueue()
		{
			return Factory.New<UPEDeclarationQueue>();
		}

		protected override UPEProcessQueueLookupsHelper GetNewProcessQueueLookupsHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return new UPEDeclarationQueueLookupsHelper((NonPersistentDeclarationQueue)queue);
		}

		protected override UPEProcessQueueLookupsHelper GetNewProcessQueueLookupsHelperForUPEProcessQueue(UPEProcessQueue queue)
		{
			return new UPEDeclarationQueueLookupsHelper((UPEDeclarationQueue)queue);
		}
	}
}
