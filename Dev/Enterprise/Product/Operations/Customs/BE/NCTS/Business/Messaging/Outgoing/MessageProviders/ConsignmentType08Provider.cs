using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class ConsignmentType08Provider : IConsignmentType08
	{
		public ConsignmentType08Provider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			depHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
		}
		readonly NctsHeader nctsHeader;
		readonly NctsDepartureMovementHeader depHeader;

		public bool ContainerIndicator => nctsHeader.DepartureHeaderContainers.Count > 0;

		public int? InlandModeOfTransport => NctsMessageProviderHelper.ConvertStringToNullableInt(depHeader.BM_InlandTransportMode);

		public int? ModeOfTransportAtTheBorder => NctsMessageProviderHelper.ConvertStringToNullableInt(depHeader.BM_ExportTransportMode);

		public IReadOnlyCollection<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments = GetTransportEquipmentsCore());
		IReadOnlyCollection<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> transportEquipments;

		IReadOnlyCollection<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> GetTransportEquipmentsCore()
		{
			return nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().Select((ctr, index) => new TransportEquipmentsForNCTSHeaderContainerProvider(ctr, index + 1)).ToArray<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment>();
		}

		public ILocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = new LocationOfGoodsProvider(nctsHeader));
		ILocationOfGoods locationOfGoods;

		public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeansCore());
		IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

		IReadOnlyCollection<IDepartureTransportMeans> GetDepartureTransportMeansCore()
		{
			var result = new List<IDepartureTransportMeans>();
			var sequence = 1;
			if (!depHeader.BM_TransportAtDeparture.IsEmpty)
			{
				result.Add(new DepartureTransportMeansTransportAtDepartureProvider(depHeader, sequence++));
			}
			if (!depHeader.BM_TransportAtDepartureTrailer1RegNo.IsEmpty)
			{
				result.Add(new DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider(depHeader, sequence++));
			}
			if (!depHeader.BM_TransportAtDepartureTrailer2RegNo.IsEmpty)
			{
				result.Add(new DepartureTransportMeansTransportAtDepartureTrailer2RegNoProvider(depHeader, sequence++));
			}
			if (!depHeader.BM_AircraftIDAtDeparture.IsEmpty)
			{
				result.Add(new DepartureTransportMeansAircraftIDAtDepartureProvider(depHeader, sequence));
			}
			return result;
		}

		public IReadOnlyCollection<IActiveBorderTransportMeans> ActiveBorderTransportMeans => activeBorderTransportMeans ??= new ActiveBorderTransportMeansCollectionProvider(depHeader);
		IReadOnlyCollection<IActiveBorderTransportMeans> activeBorderTransportMeans;

		public IPlace PlaceOfLoading => placeOfLoading ?? (placeOfLoading = new PlaceOfLoadingProvider(depHeader));
		IPlace placeOfLoading;

		public IReadOnlyCollection<IHouseConsignmentType06> HouseConsignments => houseConsignments ?? (houseConsignments = nctsHeader.Bills.Cast<NctsBill>().Select((bill, index) => new HouseConsignmentType06Provider(bill, index + 1)).ToArray<IHouseConsignmentType06>());
		IReadOnlyCollection<IHouseConsignmentType06> houseConsignments;
	}
}
