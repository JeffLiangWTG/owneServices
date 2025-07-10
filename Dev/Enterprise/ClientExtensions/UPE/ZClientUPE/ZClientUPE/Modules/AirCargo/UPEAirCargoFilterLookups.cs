using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module
{
	public class UPEAirCargoFilterLookups : UPEAirCargoCalloutFilterLookups
	{
		public UPEAirCargoFilterLookups(UPEAirCargoFilterBusinessObject filterBizO)
			: base(filterBizO)
		{
		}

		protected override DefaultQueueCodeDescriptionPairList NewQueueNamesList()
		{
			return new CargoReportQueueCodeDescriptionPairList();
		}
	}
}
