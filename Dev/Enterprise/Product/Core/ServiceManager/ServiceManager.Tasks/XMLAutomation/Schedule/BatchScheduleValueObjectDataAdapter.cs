using CargoWise.Types;
using Enterprise.Freight.DataTransfer;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BatchScheduleValueObjectDataAdapter : ScheduleValueObjectDataAdapter
	{
		protected override SailingValueObjectDataAdapter NewSailingDataAdapter(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vesselName, ZString voyageNo, ZGuid carrierPK)
		{
			return new BatchSailingValueObjectDataAdapter(transportMode, loadPort, dischargePort, vesselName, voyageNo, carrierPK);
		}
	}
}
