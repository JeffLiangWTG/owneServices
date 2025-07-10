using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	internal class IE570And573CommonConsignmentItemProvider : IIE570And573ConsignmentItem
	{
		public IE570And573CommonConsignmentItemProvider(CusEntryLine entryLine, EntryHeaderWrapper entryHeaderWrapper)
		{
			entryLineWrapper = new EntryLineWrapper(entryLine, entryHeaderWrapper);
			this.entryLine = entryLineWrapper.EntryLine;
			invoiceLine = entryLine.RandomMainPackLineOrRandomLine;
			invoiceHeader = entryLineWrapper.RandomInvoiceHeader;
			entryHeader = entryLineWrapper.EntryHeader;
			declaration = entryLineWrapper.Declaration;
			instruction = entryLineWrapper.Instruction;
		}
		internal readonly EntryLineWrapper entryLineWrapper;
		internal readonly CusEntryLine entryLine;
		internal readonly JobComInvoiceLine invoiceLine;
		internal readonly JobComInvoiceHeader invoiceHeader;
		internal readonly CusEntryHeader entryHeader;
		internal readonly JobDeclaration declaration;
		internal readonly CusEntryInstruction instruction;

		public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = MessageProviderHelper.GetE1301SupportingDocuments(declaration, entryHeader, entryLine));
		IReadOnlyCollection<IDocument> supportingDocuments;

		public IReadOnlyCollection<IPreviousDocumentLine> PreviousDocuments => previousDocuments ?? (previousDocuments = MessageProviderHelper.GetE1301PreviousDocumentLines(declaration, entryHeader, entryLine, true));
		IReadOnlyCollection<IPreviousDocumentLine> previousDocuments;

		public short DeclarationGoodsItemNumber => entryLine.CL_LineNumber;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors = entryLine.CusSupplyChainActorReferences.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray<IAdditionalSupplyChainActor>());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

		public IReadOnlyCollection<IPackaging> Packages => packages ?? (packages = MessageProviderHelper.GetPackingDetail(invoiceLine));
		IReadOnlyCollection<IPackaging> packages;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences = MessageProviderHelper.FindAdditionalInfos(entryLine, EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference).Select(x => new AdditionalReferenceProvider(x)).ToArray<IDocument>());
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ?? (additionalInformations = MessageProviderHelper.FindAdditionalInfos(entryLine, EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation).Select(x => AdditionalInformationProvider.New(x)).ToArray<IAdditionalInformation>());

		IReadOnlyCollection<IAdditionalInformation> additionalInformations;
	}
}
