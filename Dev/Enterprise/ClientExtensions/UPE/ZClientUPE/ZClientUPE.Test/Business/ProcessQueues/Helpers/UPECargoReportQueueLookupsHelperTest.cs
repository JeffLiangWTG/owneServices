namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPECargoReportQueueLookupsHelperTest : UPECustomsQueueLookupsHelperTestCase
	{
		protected override DefaultQueueCodeDescriptionPairList ExpectedQueueList
		{
			get
			{
				return new CargoReportQueueCodeDescriptionPairList();
			}
		}

		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new NonPersistentCargoReportQueue(Factory);
		}

		protected override UPEProcessQueue GetNewUPEProcessQueue()
		{
			return Factory.New<UPECargoReportQueue>();
		}

		protected override UPEProcessQueueLookupsHelper GetNewProcessQueueLookupsHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return new UPECargoReportQueueLookupsHelper((NonPersistentCargoReportQueue)queue);
		}

		protected override UPEProcessQueueLookupsHelper GetNewProcessQueueLookupsHelperForUPEProcessQueue(UPEProcessQueue queue)
		{
			return new UPECargoReportQueueLookupsHelper((UPECargoReportQueue)queue);
		}
	}
}
