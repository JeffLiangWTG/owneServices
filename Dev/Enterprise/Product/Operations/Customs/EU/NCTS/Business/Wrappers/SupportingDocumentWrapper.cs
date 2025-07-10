using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class SupportingDocumentWrapper : IProducedDocumentCertificate
	{
		public SupportingDocumentWrapper(NctsSupportingDocument supportDocument)
		{
			this.supportDocument = Argument.NotNull(supportDocument, nameof(supportDocument));
		}

		public ZString DocumentType => supportDocument.CSI_Code;

		public ZString DocumentReference => supportDocument.CSI_ReferenceNumber.Left(35);

		public ZString DocumentReferenceLanguage => "";

		public ZString ComplementOfInformation => supportDocument.CSI_Description.Left(26);

		public ZString ComplementOfInformationLanguage => "";

		readonly NctsSupportingDocument supportDocument;
	}
}
