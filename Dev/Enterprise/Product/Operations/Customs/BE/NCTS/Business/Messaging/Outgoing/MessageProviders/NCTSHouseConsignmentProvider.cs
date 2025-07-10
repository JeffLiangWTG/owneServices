using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class NCTSHouseConsignmentProvider : IHouseConsignmentType10
	{
		readonly NctsBill bill;

		public NCTSHouseConsignmentProvider(NctsBill bill, ZInt sequenceNumber)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string CountryOfDispatch => bill.B0_RN_NKCountryOfExport;

		public decimal GrossMass => bill.B0_Weight;

		public string ReferenceNumberUCR => bill.B0_ReferenceID;

		public IParty Consignor => ConsignorAddress != null ? consignor ?? (consignor = new PartyProvider(ConsignorAddress)) : null;
		IParty consignor;

		protected JobDocAddress ConsignorAddress => consignorAddress ?? (consignorAddress = JobDocAddress.Load(bill, MasterFiles.Integration.DocAddressType.ConsignorDocumentaryAddress));
		JobDocAddress consignorAddress;

		public IParty Consignee => ConsigneeAddress != null ? consignee ?? (consignee = new ConsigneeProvider(ConsigneeAddress)) : null;
		IParty consignee;

		protected JobDocAddress ConsigneeAddress => consigneeAddress ?? (consigneeAddress = JobDocAddress.Load(bill, MasterFiles.Integration.DocAddressType.ConsigneeAddress));
		JobDocAddress consigneeAddress;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors = NctsDataRetrieveMethods.GetCusReferences(bill.Factory, bill.PK).Select((cr, index) => new AdditionalSupplyChainActorProvider(cr, index + 1)).ToArray<IAdditionalSupplyChainActor>());
		IAdditionalSupplyChainActor[] additionalSupplyChainActors;

		public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = bill.DepartureTransportInfos.Cast<DepartureCusTransportMeans>().Select(ccd => new NCTSBillDepartureTransportMeansProvider(ccd, bill.InlandTransportModeAtDeparture)).ToArray<IDepartureTransportMeans>());
		IDepartureTransportMeans[] departureTransportMeans;

		public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = NctsDataRetrieveMethods.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.PreviousDocument).Select(csi => new PreviousDocumentProvider(csi)).ToArray<IPreviousDocument>());
		IPreviousDocument[] previousDocuments;

		public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ?? (transportDocuments = NctsDataRetrieveMethods.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.TransportDocument).Select(csi => new TransportDocumentProvider(csi)).ToArray<ITransportDocument>());
		ITransportDocument[] transportDocuments;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences = NctsDataRetrieveMethods.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalReference).Select(csi => new AdditionalReferenceProvider(csi)).ToArray<IDocument>());
		IDocument[] additionalReferences;

		public string TransportChargesMethodOfPayment => bill.B0_TransportPaymentMethod;

		public IReadOnlyCollection<IConsignmentItemType09> ConsignmentItems => consignmentItems ?? (consignmentItems = bill.GoodsItems.Select(item => new ConsignmentItemType09Provider(item)).ToArray<IConsignmentItemType09>());
		IConsignmentItemType09[] consignmentItems;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = NctsDataRetrieveMethods.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.SupportingDocument).Select(csi => new SupportingDocumentProvider(csi)).ToArray<ISupportingDocument>());
		ISupportingDocument[] supportingDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => additionalInformation ?? (additionalInformation = NctsDataRetrieveMethods.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalInformation).Select(csi => new AdditionalInformationProvider(csi)).ToArray<IAdditionalInformation>());
		IAdditionalInformation[] additionalInformation;
	}
}
