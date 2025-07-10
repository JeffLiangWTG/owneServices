using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface IDocumentCertificate
{
	ZString DocumentDescription { get; }
	ZString DocumentDescriptionLanguage { get; }
	ZString ReferenceOfDocument { get; }
	ZString ReferenceOfDocumentLanguage { get; }
}
