using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public sealed class DocumentAdditionalInformationProvider : IDocumentAdditionalInformationProvider
	{
		public DocumentAdditionalInformationProvider(DocumentAdditionalInformationType documentAdditionalInformation)
		{
			this.documentAdditionalInformation = Argument.NotNull(documentAdditionalInformation, nameof(documentAdditionalInformation));
		}
		readonly DocumentAdditionalInformationType documentAdditionalInformation;

		public ZString DocumentType => documentAdditionalInformation.DocumentType;

		public ZString RequestInformation => documentAdditionalInformation.DocumentComplementaryInformation;

		public ZString CcQualifier => ZString.Empty;
	}
}
