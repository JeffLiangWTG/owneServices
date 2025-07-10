using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SCDForwardingShipmentPlugIn : SCDForwardingPlugIn
	{
		public SCDForwardingShipmentPlugIn(IBusiness hostEntity)
			: base(hostEntity)
		{
			ForwardingShipment shipment = hostEntity as ForwardingShipment;
			if (shipment != null && shipment.ArrivalConsol != null)
			{
				consol = shipment.ArrivalConsol;
				originalUnpackDepot = consol.JK_OA_UnpackDepotAddress;
				consol.JK_OA_UnpackDepotAddressInfo.ValueChanged += JK_OA_UnpackDepotAddressInfo_ValueChanged;
			}
		}

		protected override void UnHookFormEventsCore()
		{
			base.UnHookFormEventsCore();
			if (consol != null)
			{
				consol.JK_OA_UnpackDepotAddressInfo.ValueChanged -= JK_OA_UnpackDepotAddressInfo_ValueChanged;
			}
		}
	}
}
