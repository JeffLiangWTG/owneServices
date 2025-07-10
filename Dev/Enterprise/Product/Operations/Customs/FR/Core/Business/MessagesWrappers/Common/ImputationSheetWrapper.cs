using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class ImputationSheetWrapper : IImputationSheet
	{
		public ImputationSheetWrapper(SupportingDocument supportingDocument, CusEntryLine entryLine)
		{
			this.itemSupportingDocument = Argument.NotNull(supportingDocument, "SupportingDocument cannot be null");
			this.itemEntryLine = Argument.NotNull(entryLine, "Entry line cannot be null");
		}

		public ZString LineNumber => itemSupportingDocument.CSI_LineNo.ToString();
		public ZString ProductReference => itemSupportingDocument.CSI_AdditionalDescription;
		public ZString ProductName => itemSupportingDocument.CSI_Description;
		public ZString ImputationUnit => itemSupportingDocument.CSI_UnitOfQuantityForMessageSending;
		public ZDecimal ImputationQuantity => itemSupportingDocument.CSI_Quantity.Round(4);
		public ZBool ShouldWriteImputationQuantity => ImputationQuantity > 0;
		public IWeight ProvisionalWeight => new WeightWrapper(itemSupportingDocument);
		public ZBool ShouldWriteProvisionalWeight => ProvisionalWeight.Weight > 0;
		public ZString ImputationCurrency => itemSupportingDocument.CSI_RX_NKCurrency;
		public ZDecimal ImputationAmount => itemSupportingDocument.CSI_Value.Round(3);
		public ZBool ShouldWriteImputationAmount => ImputationAmount > 0;
		public ZDecimal NetWeight => itemEntryLine.CustomsQuantity.Round(6);
		public ZBool ShouldWriteNetWeight => NetWeight > 0;

		protected readonly SupportingDocument itemSupportingDocument;
		protected readonly CusEntryLine itemEntryLine;
	}
}
