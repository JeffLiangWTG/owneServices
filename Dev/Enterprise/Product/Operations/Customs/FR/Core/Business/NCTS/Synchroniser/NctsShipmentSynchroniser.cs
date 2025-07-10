using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsShipmentSynchroniser : EU.NCTS.Business.NctsShipmentSynchroniser
	{
		public NctsShipmentSynchroniser(NctsHeader nctsHeader, ICusInBondParent source) : base(nctsHeader, source)
		{
		}

		protected override ISynchroniser GetNewNctsDepartureGoodsItemSynchroniser(EU.NCTS.Business.NctsDepartureCargoDesc goodsItem)
		{
			return new ShipmentToNctsDepartureGoodsItemSynchroniser((NctsDepartureCargoDesc)goodsItem, Source, hasCommonCountryOfDispatch: true, hasCommonCountryOfDestination: true, hasCommonConsignor: false, hasCommonConsignee: false, hasCommonCTStatus: true);
		}

		protected override void AddDestinationPortFieldSynchroniserForArrival()
		{
		}

		protected override void AddDestinationPortFieldSynchroniserForDeparture()
		{
		}

		protected override void AddPortofDispatchSynchroniser()
		{
		}

		protected override void AddImportLoadPortSynchroniser()
		{
		}

		protected override void AddForeignDestPortSynchroniser()
		{
		}

		protected override void AddExportTransportModeFieldSynchroniser()
		{
		}

		protected override ZString GetTransportModeCore() => TransportTypeList.Codes.Road;
	}
}
