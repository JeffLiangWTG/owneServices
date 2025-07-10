using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsConsolSynchroniser : EU.NCTS.Business.NctsConsolSynchroniser
	{
		public NctsConsolSynchroniser(NctsHeader nctsHeader, ICusInBondParent source) : base(nctsHeader, source)
		{
		}

		protected override ISynchroniser GetNewNctsDepartureGoodsItemSynchroniser(EU.NCTS.Business.NctsDepartureCargoDesc goodsItem, ForwardingShipment source, bool hasCommonCountryOfDispatch, bool hasCommonCountryOfDestination, bool hasCommonConsignor, bool hasCommonConsignee, bool hasCommonCTStatus)
		{
			return new ShipmentToNctsDepartureGoodsItemSynchroniser((NctsDepartureCargoDesc)goodsItem, source, hasCommonCountryOfDispatch, hasCommonCountryOfDestination, hasCommonConsignor, hasCommonConsignee, hasCommonCTStatus);
		}

		protected override void AddForeignDestPortSynchroniser()
		{
		}

		protected override ZString GetTransportModeCore() => TransportTypeList.Codes.Road;
	}
}
