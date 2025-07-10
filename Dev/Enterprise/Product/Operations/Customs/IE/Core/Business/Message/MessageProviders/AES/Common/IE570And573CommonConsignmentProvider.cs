using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	internal class IE570And573CommonConsignmentProvider : IIE570And573Consignment, ILocationOfGoods
	{
		public IE570And573CommonConsignmentProvider(EntryHeaderWrapper entryHeaderWrapper)
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

		public string ContainerIndicator => declaration.CusContainers.Count > 0 ? AESFlagCodeList.Codes.Yes : AESFlagCodeList.Codes.No;
		public ICarrier Carrier => CachedValueHelper.GetValue(ref carrierCached, () => CarrierProvider.New(declaration.ShippingLine));
		CachedValue<ICarrier> carrierCached;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors = instruction.CusSupplyChainActorReferences.Cast<EU.Business.Declaration.CusSupplyChainActorReference>().Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipments => transportEquipment ?? (transportEquipment = TransportEquipmentWithSealsProvider.GetEquipments(entryHeader));
		IReadOnlyCollection<ITransportEquipmentWithSeals> transportEquipment;

		public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = entryHeader.SupportingDocuments.Select(x => new SupportingDocumentProvider(x)).ToArray());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = entryHeader.PreviousDocuments.Select(x => new PreviousDocumentProvider(x)).ToArray());
		IReadOnlyCollection<IDocument> previousDocuments;

		public IReadOnlyCollection<IDocument> TransportDocument => transportDocument ?? (transportDocument = entryHeader.AdditionalInfos.Where(info => info.IsATransportDocument).Select(x => new TransportDocumentProvider(x)).ToArray<IDocument>());
		IReadOnlyCollection<IDocument> transportDocument;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences = MessageProviderHelper.FindAdditionalInfos(entryHeader, EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference).Select(x => new AdditionalReferenceProvider(x)).ToArray());
		IDocument[] additionalReferences;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ?? (additionalInformations = MessageProviderHelper.FindAdditionalInfos(entryHeader, EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation).Select(x => AdditionalInformationProvider.New(x)).ToArray());
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public ILocationOfGoods LocationOfGoods => this;

		public IReadOnlyCollection<IIE570And573ConsignmentItem> ConsignmentItems => consignmentItems ?? (consignmentItems = entryHeader.MergedLines.Select(line => new IE570And573CommonConsignmentItemProvider(line, entryHeaderWrapper)).ToArray());
		IReadOnlyCollection<IIE570And573ConsignmentItem> consignmentItems;

		#region ILocationOfGoods Members;

		public string LocationCodeType => declaration.JE_LocationOtherInformation;

		public string UNLocode => declaration.JE_LocationOfGoods;

		#endregion
	}
}
