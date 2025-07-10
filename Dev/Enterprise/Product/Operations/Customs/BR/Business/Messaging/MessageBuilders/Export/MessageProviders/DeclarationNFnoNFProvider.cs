using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationNFnoNFProvider : IDeclarationNFnoNF
	{
		public DeclarationNFnoNFProvider(ExportDeclarationMessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		readonly ExportDeclarationMessageSendingObject sendingObject;

		CusEntryInstruction EntryInstruction => sendingObject.Header?.EntryInstruction;

		CusEntryHeader EntryHeader => sendingObject.Header;

		JobDeclaration Declaration => sendingObject.Header?.Declaration;

		JobComInvoiceHeader InvoiceHeader => sendingObject.Header?.InvoiceHeaders.FirstOrDefault();

		public string ID => IsRectification ? EntryHeader.MovementReferenceNumber.ToString() : string.Empty;

		public IDeclarationOffice DeclarationOffice => fDeclarationOffice ?? (fDeclarationOffice = new DeclarationOfficeProvider(Declaration));
		IDeclarationOffice fDeclarationOffice;

		public string AdditionalInformation => EntryInstruction?.AdditionalInformation.SubstringSafe(0, CusEntryInstruction.Schema.ExportAdditionalInformationMaxLength) ?? string.Empty;

		public string TypeOfOperationExport => Declaration?.JE_DeclarantType ?? string.Empty;

		public string SpecialClearance => EntryInstruction?.CEI_SpecialCustomsClearance ?? string.Empty;

		public string SpecialTransport => Declaration?.JE_SpecialTransport ?? string.Empty;

		public bool IsConsortedExport => EntryInstruction?.CEI_IsConsortedExport ?? false;

		public bool IsRectification => sendingObject.MessageType == ExportEntryActionCodeList.Codes.RET;

		public string RectificationReason => sendingObject.VOCReason;

		public string DetailsOfTheOperation => EntryInstruction?.CEI_DetailWithoutLegalDoc ?? string.Empty;

		public string CurrencyCode => InvoiceHeader?.JZ_RX_NKInvoice_Currency ?? string.Empty;

		public string JustificationForWaivingTheInvoice => EntryInstruction?.Justification ?? string.Empty;

		public IDeclarationDeclarant Declarant => fDeclarant ?? (fDeclarant = new DeclarationDeclarantProvider(EntryInstruction));
		IDeclarationDeclarant fDeclarant;

		public IDeclarationOffice ExitOffice => fDeclarationExitOffice ?? (fDeclarationExitOffice = new DeclarationExitOfficeProvider(Declaration));
		IDeclarationOffice fDeclarationExitOffice;

		public IEnumerable<IDeclarationGoodsShipment> GoodsShipments => fGoodsShipments ?? (fGoodsShipments =
			(from CusEntryLine entryLine in EntryHeader.MergedLines orderby entryLine.CL_LineNumber select (IDeclarationGoodsShipment)new DeclarationGoodsShipmentProvider(entryLine)).ToArray());
		IDeclarationGoodsShipment[] fGoodsShipments;

		public string UCRNumber => EntryInstruction.IsUCROverridden ? EntryInstruction.UCRNumber : EntryHeader?.UniqueConsignmentReference;
	}
}
