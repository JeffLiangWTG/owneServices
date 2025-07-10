using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IProducedDocumentCertificate
	{
		ZString DocumentType { get; }
		ZString DocumentReference { get; }
		ZString DocumentReferenceLanguage { get; }
		ZString ComplementOfInformation { get; }
		ZString ComplementOfInformationLanguage { get; }
	}
}
