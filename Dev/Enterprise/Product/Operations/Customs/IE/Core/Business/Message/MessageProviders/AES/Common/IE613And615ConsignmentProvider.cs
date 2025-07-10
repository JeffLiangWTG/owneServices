using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	internal class IE613And615ConsignmentProvider : IIE613And615Consignment, ILocationOfGoods
	{
		public IE613And615ConsignmentProvider(EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryHeaderWrapper = entryHeaderWrapper;
			entryHeader = entryHeaderWrapper.EntryHeader;
			declaration = entryHeaderWrapper.Declaration;
			instruction = entryHeaderWrapper.Instruction;
		}
		internal readonly EntryHeaderWrapper entryHeaderWrapper;
		internal readonly CusEntryHeader entryHeader;
		internal readonly JobDeclaration declaration;
		internal readonly CusEntryInstruction instruction;

		public string ContainerIndicator => declaration.IsExpressConsignmentsOfExitSummary ? string.Empty : AESFlagCodeList.GetContainerIndicator(declaration);

		public decimal GrossMass => entryHeader.TotalInvoiceLinesGrossWeightInKG;

		public ICarrier Carrier => CachedValueHelper.GetValue(ref carrier, () => CarrierProvider.New(declaration.ShippingLine));
		CachedValue<ICarrier> carrier;

		public IReadOnlyCollection<ITransportEquipmentWithSeals> TransportEquipment => transportEquipment ?? (
			transportEquipment = declaration.IsExpressConsignmentsOfExitSummary
				? Array.Empty<ITransportEquipmentWithSeals>()
				: TransportEquipmentWithSealsProvider.GetEquipments(entryHeader)
		);
		IReadOnlyCollection<ITransportEquipmentWithSeals> transportEquipment;

		public ILocationOfGoods LocationOfGoods => this;

		public IReadOnlyCollection<string> CountryOfRoutingConsignment => countryOfRoutingConsignment ?? (countryOfRoutingConsignment = declaration.GetCountryOfRoutingConsignment());
		IReadOnlyCollection<string> countryOfRoutingConsignment;

		public ITransportMeans ActiveTransportMeans => CachedValueHelper.GetValue(ref activeTransportMeans, () => new ActiveTransportMeansProvider(declaration));
		CachedValue<ITransportMeans> activeTransportMeans;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (
			additionalSupplyChainActors = instruction.CusSupplyChainActorReferences.Cast<EU.Business.Declaration.CusSupplyChainActorReference>().Select(a => new AdditionalSupplyChainActorProvider(a)).ToArray()
		);
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

		public IReadOnlyCollection<IDocument> TransportDocument => transportDocument ?? (transportDocument = entryHeader.GetTransportDocuments<TransportDocumentProvider>());
		IReadOnlyCollection<IDocument> transportDocument;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences = declaration.IsTransitionPeriodAES30 ? Array.Empty<IDocument>() : entryHeader.GetAdditionalReferences<AdditionalReferenceProvider>());
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ?? (additionalInformations = declaration.IsTransitionPeriodAES30 ? Array.Empty<IAdditionalInformation>() : entryHeader.GetAdditionalInformations<AdditionalInformationProvider>());
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = declaration.IsTransitionPeriodAES30 ? Array.Empty<IDocument>() : entryHeader.PreviousDocuments.Select(x => new PreviousDocumentProvider(x)).ToArray());
		IReadOnlyCollection<IDocument> previousDocuments;

		public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ?? (
			supportingDocuments = declaration.IsExpressConsignmentsOfExitSummary || declaration.IsTransitionPeriodAES30
				? Array.Empty<IDocument>()
				: entryHeader.SupportingDocuments.Select(x => new SupportingDocumentProvider(x)).ToArray<IDocument>()
		);
		IReadOnlyCollection<IDocument> supportingDocuments;

		public IReadOnlyCollection<IConsignmentItemType03> ConsignmentItem => consignmentItem ?? (consignmentItem = entryHeader.MergedLines.Select(entryLine => new ConsignmentItemType03Provider(entryLine, entryHeaderWrapper)).ToArray());
		IReadOnlyCollection<IConsignmentItemType03> consignmentItem;

		#region ILocationOfGoods Members;

		public string LocationCodeType => declaration.JE_LocationOtherInformation;

		public string UNLocode => declaration.JE_LocationOfGoods;

		#endregion
	}
}
