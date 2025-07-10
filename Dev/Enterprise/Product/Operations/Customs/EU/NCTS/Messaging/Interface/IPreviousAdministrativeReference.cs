using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IPreviousAdministrativeReference
	{
		ZString PreviousDocumentType { get; }
		ZString PreviousDocumentReference { get; }
		ZString PreviousDocumentReferenceLanguage { get; }
		ZString ComplementOfInformation { get; }
		ZString ComplementOfInformationLanguage { get; }
	}
}
