using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class HouseConsignmentType05Provider : IHouseConsignmentType05
	{
		public HouseConsignmentType05Provider(NctsBill bill, ZInt sequenceNumber)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			nctsHeader = Argument.NotNull(bill.Header, nameof(bill.Header));
			SequenceNumber = sequenceNumber;
		}
		public int SequenceNumber { get; }

		public decimal GrossMass => WeightRounding.Round(IsInPhase5TransitionPeriod, bill.B0_Weight);

		public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans =
			bill.ArrivalTransportInfos
			.Where(t => t.TPM_TransportState == NctsUnloadedStateList.Codes.NEW || t.TPM_TransportState == NctsUnloadedStateList.Codes.MIS)
			.Select(t => new CC044CDepartureTransportMeansProvider(t))
			.ToArray<IDepartureTransportMeans>());
		IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = Array.Empty<ISupportingDocument>());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences =
			NctsDataRetrieveMethods.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalReference)
			.Where(t => t.CSI_Status != NctsUnloadedStateList.Codes.DEC)
			.Select(t => new CC044CAdditionalReferenceProvider(t, IsInPhase5TransitionPeriod))
			.ToArray());
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IConsignmentItemType05> ConsignmentItems => consignmentItems ?? (consignmentItems = bill.ArrivalGoodsItems
			.Where(i => i.BY_UnloadedState != NctsUnloadedStateList.Codes.DEC)
			.Select(i => new ConsignmentItemType05Provider(i))
			.ToArray());
		IReadOnlyCollection<IConsignmentItemType05> consignmentItems;

		public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ?? (transportDocuments =
			NctsDataRetrieveMethods.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.TransportDocument)
			.Where(t => t.CSI_Status != NctsUnloadedStateList.Codes.DEC)
			.Select(t => new CC044CTransportDocumentProvider(t, IsInPhase5TransitionPeriod))
			.ToArray());
		IReadOnlyCollection<ITransportDocument> transportDocuments;

		bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => nctsHeader.IsInPhase5TransitionPeriod);
		CachedValue<bool> isInPhase5TransitionPeriod;

		readonly NctsBill bill;
		readonly EU.NCTS.Business.NctsHeader nctsHeader;
	}
}
