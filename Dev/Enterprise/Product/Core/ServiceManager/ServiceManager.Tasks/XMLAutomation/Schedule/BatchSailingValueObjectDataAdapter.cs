using CargoWise.Types;
using Enterprise.Freight.DataTransfer;
using Enterprise.Registry.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BatchSailingValueObjectDataAdapter : SailingValueObjectDataAdapter
	{
		public BatchSailingValueObjectDataAdapter(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vesselName, ZString voyageNo, ZGuid carrierPK)
			: base(transportMode, loadPort, dischargePort, vesselName, voyageNo, carrierPK)
		{
		}

		protected override bool RegistryDefaultForImporting
		{
			get { return SystemDataRegistry.Instance.UpdateSchedulesDuringAutomaticImport.Value; }
		}
	}
}
