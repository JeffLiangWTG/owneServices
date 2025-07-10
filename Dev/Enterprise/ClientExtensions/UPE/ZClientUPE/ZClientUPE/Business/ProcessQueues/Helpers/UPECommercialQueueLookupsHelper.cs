

using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPECommercialQueueLookupsHelper : UPEProcessQueueLookupsHelper
	{
		public UPECommercialQueueLookupsHelper(UPECargoReportQueue queue)
			: base(queue, ProcessQueueType.Enum.Commercial)
		{
		}

		public UPECommercialQueueLookupsHelper(NonPersistentCalloutQueue queue)
			: base(queue)
		{
		}

		public override DefaultQueueCodeDescriptionPairList GetQueueNameList()
		{
			return new CommercialQueueCodeDescriptionPairList();
		}
	}
}
