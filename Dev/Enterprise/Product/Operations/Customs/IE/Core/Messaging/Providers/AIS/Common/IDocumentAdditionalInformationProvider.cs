using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public interface IDocumentAdditionalInformationProvider
	{
		ZString DocumentType { get; }

		ZString RequestInformation { get; }

		ZString CcQualifier { get; }
	}
}
