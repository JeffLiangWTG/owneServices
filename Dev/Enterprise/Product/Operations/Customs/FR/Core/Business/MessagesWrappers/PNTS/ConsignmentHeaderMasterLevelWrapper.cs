using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class ConsignmentHeaderMasterLevelWrapper : IConsignmentHeaderMasterLevel
	{
		ConsignmentHeaderMasterLevelWrapper(TemporaryStorageHeader temporaryStorageHeader)
		{
			this.temporaryStorageHeader = Argument.NotNull(temporaryStorageHeader, nameof(temporaryStorageHeader));
		}
		readonly TemporaryStorageHeader temporaryStorageHeader;

		public IArrivalTransportMeans ArrivalTransportMeans => arrivalTransportMeans ?? (arrivalTransportMeans = ArrivalTransportMeansWrapper.New(temporaryStorageHeader.ArrivalTransportMeans));
		IArrivalTransportMeans arrivalTransportMeans;

		public ICarrier Carrier => carrier ?? (carrier = CarrierWrapper.New(temporaryStorageHeader.Carrier));
		ICarrier carrier;

		public ICollection<IConsignmentHouseLevel> ConsignmentHouseLevel => consignmentHouseLevel ?? (consignmentHouseLevel = GetConsignmentHouseLevelCollection());
		ICollection<IConsignmentHouseLevel> consignmentHouseLevel;

		ICollection<IConsignmentHouseLevel> GetConsignmentHouseLevelCollection()
		{
			var result = new Collection<IConsignmentHouseLevel>();
			temporaryStorageHeader.HouseBills?.Cast<TemporaryStorageBill>().ForEach(bill => result.Add(ConsignmentHouseLevelWrapper.New(bill)));
			return result;
		}

		public IConsignmentMasterLevel ConsignmentMasterLevel => consignmentMasterLevel ?? (consignmentMasterLevel = temporaryStorageHeader.HasNoMasterBill ? null : ConsignmentMasterLevelWrapper.New(temporaryStorageHeader.MasterBill));
		IConsignmentMasterLevel consignmentMasterLevel;

		public ILocationOfGoods LocationOfGoods => locationOfGoods ?? (temporaryStorageHeader.GoodsLocation != null ? locationOfGoods = LocationOfGoodsWrapper.New(temporaryStorageHeader.GoodsLocation) : null);
		ILocationOfGoods locationOfGoods;

		public IPlaceOfUnloading PlaceOfUnloading => placeOfUnloading ?? (placeOfUnloading = PlaceOfUnloadingWrapper.New(temporaryStorageHeader));
		IPlaceOfUnloading placeOfUnloading;

		public IWarehouse Warehouse => warehouse ?? (temporaryStorageHeader.AuthorizationUsage != null ? warehouse = WarehouseWrapper.New(temporaryStorageHeader.AuthorizationUsage) : null);
		IWarehouse warehouse;

		public static ConsignmentHeaderMasterLevelWrapper New(TemporaryStorageHeader temporaryStorageHeader) => temporaryStorageHeader == null ? null : new ConsignmentHeaderMasterLevelWrapper(temporaryStorageHeader);
	}
}
