using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class DocumentAdditionalInformationProvider : IDocumentAdditionalInformationProvider
	{
		public DocumentAdditionalInformationProvider(IDocumentAdditionalInformation documentAdditionalInformation)
		{
			this.documentAdditionalInformation = documentAdditionalInformation;
		}
		readonly IDocumentAdditionalInformation documentAdditionalInformation;

		public ZString DocumentType => documentAdditionalInformation.DocumentType;

		public ZString RequestInformation => documentAdditionalInformation.RequestInformation;

		public ZString CcQualifier
		{
			get
			{
				if (documentAdditionalInformation is IAISDocumentAdditionalInformation aisDocumentAdditionalInformation)
				{
					return aisDocumentAdditionalInformation.CcQualifier;
				}
				else
				{
					return null;
				}
			}
		}
	}
}
