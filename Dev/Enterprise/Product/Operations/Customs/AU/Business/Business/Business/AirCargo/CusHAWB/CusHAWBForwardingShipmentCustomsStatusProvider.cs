using CargoWise.Types;
using Enterprise.Customs.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBForwardingShipmentCustomsStatusProvider : ForwardingShipmentCustomsStatusProvider, Integration.Customs.AU.ICusHAWBForwardingShipmentCustomsStatusProvider
	{
		public CusHAWBForwardingShipmentCustomsStatusProvider(ForwardingShipment shipment)
			: base(shipment)
		{
			shipment.Factory.AddFetchHint(typeof(Customs.Business.CusHAWB), CusHAWBSchema.CS_JS, Shipment.PK);
		}

		public override ZString CustomsCargoStatus()
		{
			var hawb = CusHAWB.Load(Shipment);
			return hawb != null ? hawb.CS_CustomsStatus : ZString.Empty;
		}

		public override ZString CustomsMessageStatus()
		{
			var hawb = CusHAWB.Load(Shipment);
			return hawb != null ? hawb.CS_MsgStatus : ZString.Empty;
		}
	}
}
