using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class PreviousDocumentWrapper : IPreviousAdministrativeReference
	{
		public PreviousDocumentWrapper(NctsPreviousDocument previousDocument)
		{
			this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		}

		public ZString PreviousDocumentType => previousDocument.CSI_Code;

		public ZString PreviousDocumentReference => previousDocument.CSI_ReferenceNumber + (previousDocument.CSI_DateOfIssue.IsValid ? "-" + previousDocument.DateOfIssueInFormat : string.Empty);

		public ZString PreviousDocumentReferenceLanguage => "";

		public ZString ComplementOfInformation => previousDocument.CSI_Description;

		public ZString ComplementOfInformationLanguage => "";

		readonly NctsPreviousDocument previousDocument;
	}
}
