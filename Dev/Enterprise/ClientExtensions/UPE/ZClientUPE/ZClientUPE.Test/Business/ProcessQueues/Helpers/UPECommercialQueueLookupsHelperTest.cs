using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPECommercialQueueLookupsHelperTest : UPEProcessQueueLookupsHelperTestCase
	{
		protected override ProcessQueueType.Enum ExpectedQueueType
		{
			get
			{
				return ProcessQueueType.Enum.Commercial;
			}
		}

		protected override DefaultQueueCodeDescriptionPairList ExpectedQueueList
		{
			get
			{
				return new CommercialQueueCodeDescriptionPairList();
			}
		}

		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new NonPersistentCalloutQueue(Factory);
		}

		protected override UPEProcessQueue GetNewUPEProcessQueue()
		{
			return Factory.New<UPECargoReportQueue>();
		}

		protected override UPEProcessQueueLookupsHelper GetNewProcessQueueLookupsHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return new UPECommercialQueueLookupsHelper((NonPersistentCalloutQueue)queue);
		}

		protected override UPEProcessQueueLookupsHelper GetNewProcessQueueLookupsHelperForUPEProcessQueue(UPEProcessQueue queue)
		{
			return new UPECommercialQueueLookupsHelper((UPECargoReportQueue)queue);
		}
	}
}
