using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class ConsignmentType06Provider : IConsignmentType06
	{
		public ConsignmentType06Provider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		public decimal GrossMass => nctsHeader.Bills.Sum(b =>
			b.ArrivalGoodsItems.Cast<NctsArrivalCargoDesc>().Sum(g =>
				g.UnloadedGoodsItem?.GrossMassInKilograms ?? g.GrossMassInKilograms));

		public IReadOnlyCollection<ITransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments =
			nctsHeader.ArrivalHeaderContainers
				.Where(x => x.BC_UnloadedState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS })
					|| (x.BC_UnloadedState == NctsUnloadedStateList.Codes.DEC && x.Seals.Cast<CusSeal>().Any(s => s.BK_UnloadingState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS }))))
				.Select(x => new ArrivalTransportEquipmentProvider(x))
				.OrderBy(x => x.SequenceNumber)
				.ToArray());
		IReadOnlyCollection<ITransportEquipment> transportEquipments;

		public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans =
			ArrivalMovementHeader.ArrivalTransportInfos
				.Where(t => t.TPM_TransportState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS }))
				.Select(t => new CC044CDepartureTransportMeansProvider(t))
				.OrderBy(x => x.SequenceNumber)
				.ToArray());
		IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments =
			ArrivalMovementHeader.SupportingDocuments
				.Where(x => x.CSI_Status.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS }))
				.Select(x => new CC044CSupportingDocumentProvider(x, IsInPhase5TransitionPeriod))
				.OrderBy(x => x.SequenceNumber)
				.ToArray());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ?? (transportDocuments =
			ArrivalMovementHeader.AdditionalDocuments
				.Where(x => x.CSI_SubType == Constants.CusSupportingInfoSubTypes.TransportDocument && x.CSI_Status.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS }))
				.Select(x => new ArrivalDocumentProvider(x, IsInPhase5TransitionPeriod))
				.OrderBy(x => x.SequenceNumber)
				.ToArray());
		IReadOnlyCollection<ITransportDocument> transportDocuments;

		public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => additionalReferences ?? (additionalReferences =
			ArrivalMovementHeader.AdditionalDocuments
				.Where(x => x.CSI_SubType == Constants.CusSupportingInfoSubTypes.AdditionalReference && x.CSI_Status.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS }))
				.Select(x => new ArrivalDocumentProvider(x, IsInPhase5TransitionPeriod))
				.OrderBy(x => x.SequenceNumber)
				.ToArray());
		IReadOnlyCollection<IAdditionalReference> additionalReferences;

		public IReadOnlyCollection<IHouseConsignmentType05> HouseConsignments => houseConsignments ?? (houseConsignments = nctsHeader.Bills.Cast<NctsBill>().OrderBy(b => b.PK).Select((bill, index) => new HouseConsignmentType05Provider(bill, index + 1)).ToArray<IHouseConsignmentType05>());
		IHouseConsignmentType05[] houseConsignments;

		NctsArrivalMovementHeader ArrivalMovementHeader => arrivalMovementHeader ?? (arrivalMovementHeader = nctsHeader.ArrivalMovementHeader);
		NctsArrivalMovementHeader arrivalMovementHeader;

		bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => nctsHeader.IsInPhase5TransitionPeriod);
		CachedValue<bool> isInPhase5TransitionPeriod;

		readonly NctsHeader nctsHeader;
	}
}
