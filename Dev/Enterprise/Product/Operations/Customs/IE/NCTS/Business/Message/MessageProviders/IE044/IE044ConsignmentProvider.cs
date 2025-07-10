using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE044ConsignmentProvider : IIE044Consignment
	{
		public IE044ConsignmentProvider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			nctsArrivalMovementHeader = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
		}
		readonly NctsHeader nctsHeader;
		readonly NctsArrivalMovementHeader nctsArrivalMovementHeader;

		public decimal GrossMass => nctsHeader.ArrivalMovementHeader.BM_GrossWeightUnloaded;

		public IReadOnlyCollection<ITransportEquipmentWithSeals> TransportEquipments => transportEquipments ?? (transportEquipments =
			nctsHeader.ArrivalHeaderContainers
				.Where(x => !x.BC_UnloadedState.EqualsIgnoringCase(EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC)
					|| (x.BC_UnloadedState == NctsUnloadedStateList.Codes.DEC && x.Seals.Cast<CusSeal>().Any(s => !s.BK_UnloadingState.EqualsIgnoringCase(EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC))))
				.Select(x => new ArrivalTransportEquipmentWithSealsProvider(x))
				.ToArray());
		IReadOnlyCollection<ITransportEquipmentWithSeals> transportEquipments;

		public IReadOnlyCollection<ITransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans =
			nctsArrivalMovementHeader.ArrivalTransportInfos
				.Where(t => !t.TPM_TransportState.EqualsIgnoringCase(EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC))
				.Select(t => new IE044DepartureTransportMeansProvider(t))
				.ToArray());
		IReadOnlyCollection<ITransportMeans> departureTransportMeans;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments =
			nctsArrivalMovementHeader.SupportingDocuments
				.Where(x => !x.CSI_Status.EqualsIgnoringCase(EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC))
				.Select(x => new SupportingDocumentProvider(x))
				.ToArray());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ?? (transportDocuments =
			nctsArrivalMovementHeader.AdditionalDocuments
				.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument && !x.CSI_Status.EqualsIgnoringCase(EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC))
				.Select(x => new DocumentProvider(x))
				.ToArray());
		IReadOnlyCollection<IDocument> transportDocuments;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences =
			nctsArrivalMovementHeader.AdditionalDocuments
				.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && !x.CSI_Status.EqualsIgnoringCase(EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC))
				.Select(x => new DocumentProvider(x))
				.ToArray());
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IIE044HouseConsignment> HouseConsignments => houseConsignments ?? (houseConsignments =
			nctsHeader.Bills?
			.Select(b => new IE044HouseConsignmentProvider(b))
			.ToArray());
		IReadOnlyCollection<IIE044HouseConsignment> houseConsignments;
	}
}
