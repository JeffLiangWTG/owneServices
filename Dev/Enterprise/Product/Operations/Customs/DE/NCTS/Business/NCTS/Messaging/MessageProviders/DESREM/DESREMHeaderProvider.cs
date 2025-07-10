using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	sealed class DESREMHeaderProvider : NCTSHeaderProvider, IDESREMHeader
	{
		public DESREMHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public string MRN => nctsHeader.MovementReferenceNumber.ValueOrNullIfEmpty();

		public string OtherThingsToReport => nctsHeader.ArrivalMovementHeader.OtherThingsToReport.ValueOrNullIfEmpty();

		public string CustomsOfficeOfDestinationActualID => nctsHeader.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival.ValueOrNullIfEmpty();

		public string UnloadingRemarkConform => nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport.MapBoolTo10();

		public string UnloadingRemarkStateOfSeals =>
			nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().Any(c => !c.BC_Seal1.IsEmpty || !c.BC_Seal2.IsEmpty || c.Seals.Any())
				? nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean.MapBoolTo10()
				: null;

		public string UnloadingRemark => !nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport || !nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean
			? nctsHeader.ArrivalMovementHeader.BM_UnloadingRemarks.ValueOrNullIfEmpty()
			: null;

		public decimal? GrossMass => nctsHeader.ArrivalMovementHeader.BM_GrossWeightUnloaded.Round(3).Normalize();

		public IReadOnlyCollection<IDESREMTransportEquipment> TransportEquipments => transportEquipments ??
		(transportEquipments = nctsHeader.ArrivalHeaderContainers
			.Where(e => NCTSProviderHelpers.UnloadedStatusInNewMisDif(e.BC_UnloadedState)
						|| e.Seals.Select(s => s.BK_UnloadingState).Any(NCTSProviderHelpers.UnloadedStatusInNewMis))
			.Select(DESREMTransportEquipmentProvider.NewOrNull)
			.ToArray());

		IReadOnlyCollection<IDESREMTransportEquipment> transportEquipments;

		public IReadOnlyCollection<IDESREMTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeans());
		IReadOnlyCollection<IDESREMTransportMeans> departureTransportMeans;

		IDESREMTransportMeans[] GetDepartureTransportMeans()
			=> nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos
				.Cast<ArrivalCusTransportMeans>()
				.Where(x => NCTSProviderHelpers.UnloadedStatusInNewMisDif(x.TPM_TransportState))
				.Select(y => DESREMTransportMeansProvider.NewOrNull(y))
				.ToArray();

		public IReadOnlyCollection<IDESREMHouseConsignment> HouseConsignments => houseConsignments ?? (houseConsignments = GetHouseConsignments());
		IReadOnlyCollection<IDESREMHouseConsignment> houseConsignments;

		IDESREMHouseConsignment[] GetHouseConsignments()
			=> nctsHeader.Bills
				.Where(b => NCTSProviderHelpers.UnloadedStatusInNewMisDif(b.MovementDetail.B9_UnloadedState)
					|| b.ArrivalTransportInfos.Cast<ArrivalCusTransportMeans>().Any(t => NCTSProviderHelpers.UnloadedStatusInNewMisDif(t.TPM_TransportState))
					|| b.ArrivalGoodsItems.Any(g => NCTSProviderHelpers.UnloadedStatusInNewMisDif(g.BY_UnloadedState))
					|| b.ArrivalGoodsItems.SelectMany(g => g.Packages.Cast<NctsPackage>()).Any(p => NCTSProviderHelpers.UnloadedStatusInNewMis(p.B5_TypeOfDifference)))
				.Select(b => new DESREMHouseConsignmentProvider(b))
				.ToArray();
	}
}
