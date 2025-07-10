using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using EUCusSupplyChainActorReference = Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference;

namespace Enterprise.Customs.IE.Business.AES
{
	internal class ConsignmentItemType03Provider : IConsignmentItemType03
	{
		public ConsignmentItemType03Provider(CusEntryLine entryLine, EntryHeaderWrapper entryHeaderWrapper)
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

		public short GoodsItemNumber => entryLine.CL_LineNumber;

		public IParty Consignor => CachedValueHelper.GetValue(ref consignorCached, () =>
		{
			if ((invoiceLine.ExporterAddress ?? invoiceHeader.SupplierAddress) is OrgAddress consignorAddress)
			{
				return PartyProvider.New(consignorAddress, PartyProvider.FallBackStyle.Consignor);
			}
			else
			{
				return PartyProvider.New(declaration.SupplierDocumentaryAddress, PartyProvider.FallBackStyle.Consignor);
			}
		});
		CachedValue<IParty> consignorCached;

		public IParty Consignee => CachedValueHelper.GetValue(ref consigneeCached, () =>
		{
			if ((invoiceLine.ConsigneeAddress ?? invoiceHeader.BuyerAddress) is OrgAddress consigneeAddress)
			{
				return PartyProvider.New(consigneeAddress);
			}
			else
			{
				return PartyProvider.New(declaration.ImporterDocumentaryAddress);
			}
		});
		CachedValue<IParty> consigneeCached;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (
			additionalSupplyChainActors = entryLine.CusSupplyChainActorReferences.Cast<EUCusSupplyChainActorReference>().Select(a => new AdditionalSupplyChainActorProvider(a)).ToArray()
		);
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

		public ICommodityTypeWithGrossMass Commodity => commodity ?? (commodity = new CommodityTypeWithGrossMassProvider(entryLineWrapper));
		ICommodityTypeWithGrossMass commodity;

		public IReadOnlyCollection<IPackaging> Packages => packages ?? (packages = declaration.IsExpressConsignmentsOfExitSummary ? Array.Empty<IPackaging>() : MessageProviderHelper.GetPackingDetail(invoiceLine));
		IReadOnlyCollection<IPackaging> packages;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences = MessageProviderHelper.GetE1301AdditionalReferences(declaration, entryHeader, entryLine));
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ?? (additionalInformations = MessageProviderHelper.GetE1301AdditionalInformations(declaration, entryHeader, entryLine));
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = MessageProviderHelper.GetE1301PreviousDocuments(declaration, entryHeader, entryLine, true));
		IReadOnlyCollection<IDocument> previousDocuments;

		public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = GetSupportingDocuments());
		IReadOnlyCollection<IDocument> supportingDocuments;

		IReadOnlyCollection<IDocument> GetSupportingDocuments()
		{
			if (declaration.IsExpressConsignmentsOfExitSummary)
			{
				return Array.Empty<IDocument>();
			}
			else
			{
				return MessageProviderHelper.GetE1301SupportingDocuments(declaration, entryHeader, entryLine);
			}
		}

		public string ReferenceNumberUCR => invoiceHeader.JZ_UCR;

		public string TransportMOP => invoiceHeader.ZG_TransportChargesMethodOfPayment;
	}
}
