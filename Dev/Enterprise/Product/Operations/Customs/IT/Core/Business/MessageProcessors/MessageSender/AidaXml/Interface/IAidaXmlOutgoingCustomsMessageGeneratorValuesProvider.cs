using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider : IOutgoingCustomsMessageGeneratorValuesProvider
{
	ICryptokiGlbExternalPassword CryptokiCertificate { get; }
	IGlbMauExternalPassword MauCertificate { get; }
	ZString ServiceTypeNamespace { get; }
	ZString ServiceTypePrefix { get; }
	IAidaXmlSigner XmlSigner { get; }
	ZString GetMessageType();
	ZString GetServiceId();
	ZString CustomsMessageText { get; }
	bool HasValidAutomaticSignature { get; }
	ILocalReferenceNumberGenerator LocalReferenceNumberGenerator { get; }
}
