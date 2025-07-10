using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
{
	public AsycudaBillCollection(AsycudaManifestHeader master) : base(master) { }

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var bill = (AsycudaBill)child;
		var consolType = bill.Header?.Consol?.JK_AgentType ?? ZString.Empty;
		if (consolType == Core.Constants.ShipmentTypes.CoLoadMaster)
		{
			bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
		}
	}
}

