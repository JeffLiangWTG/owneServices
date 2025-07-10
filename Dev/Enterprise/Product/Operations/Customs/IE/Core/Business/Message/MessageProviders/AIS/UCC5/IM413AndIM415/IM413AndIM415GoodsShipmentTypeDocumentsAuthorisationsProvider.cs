using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentTypeDocumentsAuthorisationsProvider : IGoodsShipmentTypeDocumentsAuthorisations
	{
		public IM413AndIM415GoodsShipmentTypeDocumentsAuthorisationsProvider(EntryHeaderWrapper entryHeaderWrapper)
		{
			entryHeader = entryHeaderWrapper.EntryHeader;
			instruction = entryHeaderWrapper.Instruction;
			invoice = entryHeaderWrapper.RandomInvoiceHeader;
			jobDeclaration = entryHeaderWrapper.Declaration;
		}
		readonly CusEntryHeader entryHeader;
		readonly CusEntryInstruction instruction;
		readonly JobComInvoiceHeader invoice;
		readonly JobDeclaration jobDeclaration;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations
			=> additionalInformationsCached ??= entryHeader.GetAdditionalInformations<CcQualifierAdditionalInformationProvider>().ToArray<IAdditionalInformation>();
		IReadOnlyCollection<IAdditionalInformation> additionalInformationsCached;

		public IReadOnlyCollection<IIdType> ProducedDocuments
			=> producedDocumentsCached ??= entryHeader.SupportingDocuments.Select(x => ProducedDocumentsProvider.New(x)).ToArray<IIdType>();
		IReadOnlyCollection<IIdType> producedDocumentsCached;

		public IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> SimplifiedDeclarationDocuments
			=> simplifiedDeclarationDocumentsCached ??= entryHeader.PreviousDocuments.Select(x => new SimplifiedDeclarationDocumentWritingOffProvider(x)).ToArray<ISimplifiedDeclarationDocumentWritingOff>();
		IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> simplifiedDeclarationDocumentsCached;

		public string UCR =>
		!jobDeclaration.JE_UCR.IsEmpty
			? jobDeclaration.JE_UCR
			: invoice.JZ_UCR;

		public IIdType Warehouse => CachedValueHelper.GetValue(ref warehouseCached, () =>
		{
			var type = invoice.HasInvoiceLineWithPreviousProcedure0700 ? new ZString("Y") : instruction.ToWarehouseType;
			var id = instruction.ToWarehouseCode;

			if (id.IsEmpty || type.IsEmpty)
			{
				return null;
			}

			return IdTypeProvider.New(type, id);
		});
		CachedValue<IIdType> warehouseCached;
	}
}
