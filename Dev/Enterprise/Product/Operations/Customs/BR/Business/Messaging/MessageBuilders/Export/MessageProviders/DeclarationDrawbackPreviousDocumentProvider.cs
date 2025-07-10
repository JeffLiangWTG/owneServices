using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationDrawbackPreviousDocumentProvider : IDeclarationDrawbackPreviousDocument
	{
		public DeclarationDrawbackPreviousDocumentProvider(SuspensionDrawbackImportEntryDocument drawbackImportEntryDocument)
		{
			this.drawbackImportEntryDocument = Argument.NotNull(drawbackImportEntryDocument, nameof(drawbackImportEntryDocument));
		}
		readonly SuspensionDrawbackImportEntryDocument drawbackImportEntryDocument;

		public string ID => drawbackImportEntryDocument.CSI_ReferenceNumber;
		public decimal QuantityQuantity => drawbackImportEntryDocument.CSI_Quantity;
		public decimal AmountAmount => drawbackImportEntryDocument.CSI_Value;
		public string CategoryCode => drawbackImportEntryDocument.CSI_SubType;
		public int ItemID => drawbackImportEntryDocument.CSI_LineNo;
	}
}
