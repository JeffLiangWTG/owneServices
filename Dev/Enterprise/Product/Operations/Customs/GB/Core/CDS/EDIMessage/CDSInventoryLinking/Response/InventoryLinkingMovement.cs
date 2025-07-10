using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class InventoryLinkingMovement
	{
		public ZString MessageCode { get; set; }

		public ZString GoodsLocation { get; set; }

		public ZString GoodsArrivalDateTime { get; set; }

		public ZString GoodsDepartureDateTime { get; set; }

		public ZString MovementReference { get; set; }

		public InventoryLinkingTransportDetails TransportDetails { get; set; }
	}
}
