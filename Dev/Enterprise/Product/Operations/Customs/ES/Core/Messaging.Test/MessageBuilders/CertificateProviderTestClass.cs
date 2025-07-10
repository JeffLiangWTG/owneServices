using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public class CertificateProviderTestClass : ICertificateProvider
	{
		public ZString BrokerCode { get; set; }
		public ZString CertificateName { get; set; }
		public ZString CertificateThumbPrint { get; set; }
		public ZBlob CertificateBytes { get; set; }
		public ZString DecryptedCertificatePassphrase { get; set; }
		public ZGuid CertificatePK { get; set; }
		public ZString CertificateID { get; set; }
	}
}
