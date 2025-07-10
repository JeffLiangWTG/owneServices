using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class LinkedDocumentProvider : ILinkedDocument
	{
		LinkedDocumentProvider(PreviousDocument previousDocument)
		{
			this.previousDocument = previousDocument;
		}
		readonly PreviousDocument previousDocument;

		public static LinkedDocumentProvider New(PreviousDocument previousDocument) => previousDocument == null ? null : new LinkedDocumentProvider(previousDocument);

		public string Type => ImportPreviousDocumentList.MapToCustomsCode(previousDocument.CSI_Code);

		public string Number => previousDocument.CSI_ReferenceNumber;

		public int ItemNumber => previousDocument.CSI_ItemNumber;
	}
}
