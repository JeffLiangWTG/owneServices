using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module
{
	public class CalloutFilterLookups : UPEAirCargoCalloutFilterLookups
	{
		public CalloutFilterLookups(CalloutFilterBusinessObject filterBizO)
			: base(filterBizO)
		{
		}

		protected override DefaultQueueCodeDescriptionPairList NewQueueNamesList()
		{
			return new CommercialQueueCodeDescriptionPairList();
		}
	}
}
