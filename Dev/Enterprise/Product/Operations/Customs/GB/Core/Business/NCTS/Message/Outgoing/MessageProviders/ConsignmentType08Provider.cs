using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class ConsignmentType08Provider : IConsignmentType08
	{
		public ConsignmentType08Provider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			depHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
		}

		public bool ContainerIndicator => nctsHeader.DepartureHeaderContainers.Count > 0;

		public string InlandModeOfTransport => GetModeStringOrNull(depHeader.BM_InlandTransportMode);

		public string ModeOfTransportAtTheBorder => GetModeStringOrNull(depHeader.BM_ExportTransportMode);

		string GetModeStringOrNull(string stringValue) => int.TryParse(stringValue, out var intValue) && intValue != 0 ? stringValue : null;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments = GetTransportEquipmentsCore());
		IReadOnlyCollection<ITransportEquipment> transportEquipments;

		IReadOnlyCollection<ITransportEquipment> GetTransportEquipmentsCore()
		{
			return nctsHeader.DepartureHeaderContainers.Cast<EU.NCTS.Business.NctsDepartureHeaderContainer>().Select((ctr, index) => new TransportEquipmentsForNCTSHeaderContainerProvider(ctr, index + 1)).ToArray<ITransportEquipment>();
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

		public IReadOnlyCollection<IHouseConsignmentType06> HouseConsignments => houseConsignments ?? (houseConsignments = nctsHeader.Bills.Cast<EU.NCTS.Business.NctsBill>().Select((bill, index) => new HouseConsignmentType06Provider(bill, index + 1)).ToArray<IHouseConsignmentType06>());
		IReadOnlyCollection<IHouseConsignmentType06> houseConsignments;

		readonly NctsHeader nctsHeader;
		readonly NctsDepartureMovementHeader depHeader;
	}
}
