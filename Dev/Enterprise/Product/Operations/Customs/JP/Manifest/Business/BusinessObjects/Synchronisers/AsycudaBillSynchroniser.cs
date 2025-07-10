using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Manifest.Business;

public class AsycudaBillSynchroniser(ASYCUDA.Business.AsycudaBill destination, ForwardingShipment shipmentSource) : ASYCUDA.Business.AsycudaBillSynchroniser(destination, shipmentSource)
{
	protected override void HookSynchronisers()
	{
		base.HookSynchronisers();
		Synchronisers.Add(new FieldSynchroniser(Destination.ABL_GoodsLocationInfo, () => (consolSource.JK_OA_UnpackDepotAddress_ZAddress.OrgAddress as OrgAddress)?.CustomsCodes.GetCustomsRegNoMatching(OrgCusCode.CodeTypes.ControlledPremisesID) ?? ZString.Empty, () => new[] { consolSource.JK_OA_UnpackDepotAddressInfo }));
	}

	protected override void AddPacksSynchroniser()
	{
	}
}
