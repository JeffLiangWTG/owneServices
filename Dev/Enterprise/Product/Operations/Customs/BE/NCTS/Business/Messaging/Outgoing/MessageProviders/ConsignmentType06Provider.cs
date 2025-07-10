using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class ConsignmentType06Provider : IConsignmentType06
	{
		readonly NctsHeader nctsHeader;

		public ConsignmentType06Provider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		public decimal GrossMass => nctsHeader.ArrivalMovementHeader.EffectiveGrossWeightUnloaded;

		public IReadOnlyCollection<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments =
			nctsHeader.ArrivalHeaderContainers
				.Where(x => IsNewOrMissing(x.BC_UnloadedState)
					|| (x.BC_UnloadedState == NctsUnloadedStateList.Codes.DEC && x.Seals.Cast<CusSeal>().Any(s => IsNewOrMissing(s.BK_UnloadingState))))
				.Select(x => new ArrivalTransportEquipmentProvider(x))
				.OrderBy(x => x.SequenceNumber)
				.ToArray());
		IReadOnlyCollection<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> transportEquipments;

		public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans =
			ArrivalMovementHeader.ArrivalTransportInfos
				.Where(t => IsNewOrMissing(t.TPM_TransportState))
				.Select(t => new CC044CDepartureTransportMeansProvider(t))
				.OrderBy(x => x.SequenceNumber)
				.ToArray());
		IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments =
			ArrivalMovementHeader.SupportingDocuments
				.Where(x => IsNewOrMissing(x.CSI_Status))
				.Select(x => new CC044CSupportingDocumentProvider(x))
				.OrderBy(x => x.SequenceNumber)
				.ToArray());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ?? (transportDocuments =
			ArrivalMovementHeader.AdditionalDocuments
				.Where(x => x.CSI_SubType == Constants.CusSupportingInfoSubTypes.TransportDocument && IsNewOrMissing(x.CSI_Status))
				.Select(x => new ArrivalDocumentProvider(x))
				.OrderBy(x => x.SequenceNumber)
				.ToArray());
		IReadOnlyCollection<ITransportDocument> transportDocuments;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences =
			ArrivalMovementHeader.AdditionalDocuments
				.Where(x => x.CSI_SubType == Constants.CusSupportingInfoSubTypes.AdditionalReference && IsNewOrMissing(x.CSI_Status))
				.Select(x => new ArrivalDocumentProvider(x))
				.OrderBy(x => x.SequenceNumber)
				.ToArray());
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IHouseConsignmentType05> HouseConsignments => houseConsignments ??= nctsHeader.Bills.Cast<NctsBill>().Where(b => IsNewOrMissing(b.UnloadedStatus) || b.UnloadedStatus == NctsUnloadedStateList.Codes.DIF).OrderBy(b => b.MovementDetail.B9_SeqNo).Select((bill) => new HouseConsignmentType05Provider(bill, bill.MovementDetail.B9_SeqNo)).ToArray();
		IHouseConsignmentType05[] houseConsignments;

		NctsArrivalMovementHeader ArrivalMovementHeader => arrivalMovementHeader ?? (arrivalMovementHeader = nctsHeader.ArrivalMovementHeader);
		NctsArrivalMovementHeader arrivalMovementHeader;

		bool IsNewOrMissing(ZString value) => value.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS });
	}
}
