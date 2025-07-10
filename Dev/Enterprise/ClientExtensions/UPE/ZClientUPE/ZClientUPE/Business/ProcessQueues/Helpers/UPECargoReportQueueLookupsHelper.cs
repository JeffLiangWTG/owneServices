


namespace Enterprise.Client.UPE.Business
{
	public class UPECargoReportQueueLookupsHelper : UPECustomsQueueLookupsHelper
	{
		public UPECargoReportQueueLookupsHelper(UPECargoReportQueue queue)
			: base(queue)
		{
		}

		public UPECargoReportQueueLookupsHelper(NonPersistentCargoReportQueue queue)
			: base(queue)
		{
		}

		protected override CustomsQueueCodeDescriptionPairList GetCustomsQueueList()
		{
			return new CargoReportQueueCodeDescriptionPairList();
		}
	}
}
