using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SupportingDocumentProvider : IIdentifierTypePair
	{
		SupportingDocumentProvider(SupportingDocument supportingDocument)
		{
			this.supportingDocument = supportingDocument;
		}

		readonly SupportingDocument supportingDocument;

		public static SupportingDocumentProvider NewOrNull(SupportingDocument supportingDocument) => supportingDocument != null ? new SupportingDocumentProvider(supportingDocument) : null;

		public string Identifier => supportingDocument.CSI_ReferenceNumber;

		public string Type => supportingDocument.CSI_Code;
	}
}
