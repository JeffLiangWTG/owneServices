using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class ShipmentToNctsDepartureGoodsItemSynchroniser : EU.NCTS.Business.ShipmentToNctsDepartureGoodsItemSynchroniser
	{
		public ShipmentToNctsDepartureGoodsItemSynchroniser(NctsDepartureCargoDesc destination, ForwardingShipment source, bool hasCommonCountryOfDispatch, bool hasCommonCountryOfDestination, bool hasCommonConsignor, bool hasCommonConsignee, bool hasCommonCTStatus)
		: base(destination, source, hasCommonCountryOfDispatch, hasCommonCountryOfDestination, hasCommonConsignor, hasCommonConsignee, hasCommonCTStatus)
		{
		}

		protected override void AddDestinationPortFieldSynchroniser()
		{
		}

		protected override void AddImportLoadPortFieldSynchroniser()
		{
		}
	}
}
