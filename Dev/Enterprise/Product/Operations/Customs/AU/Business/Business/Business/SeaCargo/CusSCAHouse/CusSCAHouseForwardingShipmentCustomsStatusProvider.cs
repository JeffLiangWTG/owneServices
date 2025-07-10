using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class CusSCAHouseForwardingShipmentCustomsStatusProvider : Customs.Business.CusSCAHouseForwardingShipmentCustomsStatusProvider
	{
		public CusSCAHouseForwardingShipmentCustomsStatusProvider(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override ZString[] ApplicationCodes
		{
			get { return CusSCAOceanBill.ApplicationCodes; }
		}
	}
}
