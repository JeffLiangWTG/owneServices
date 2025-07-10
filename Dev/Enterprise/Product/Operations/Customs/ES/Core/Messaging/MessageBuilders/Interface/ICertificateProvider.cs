using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface ICertificateProvider
	{
		ZString BrokerCode { get; }
		ZString CertificateName { get; }
		ZString CertificateThumbPrint { get; }
		ZBlob CertificateBytes { get; }
		ZString DecryptedCertificatePassphrase { get; }
		ZGuid CertificatePK { get; }
		ZString CertificateID { get; }
	}
}
