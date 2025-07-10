using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationPreviousDocumentProvider : IDeclarationPreviousDocument
	{
		public DeclarationPreviousDocumentProvider(PreviousDocument previousDocument)
		{
			this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		}
		readonly PreviousDocument previousDocument;

		public string ID => previousDocument.CSI_ReferenceNumber;
		public string TypeOfEntry => previousDocument.CSI_Code;
		public int LineNo => previousDocument.CSI_LineNo;
		public decimal CustomsQuantity => previousDocument.CSI_Quantity;
		public string DigitalServiceDossiers => previousDocument.CSI_ReferenceNumber2;
	}
}


