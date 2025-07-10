using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;

namespace Enterprise.Customs.EU.Business
{
	public class CryptokiWindowsCertificateProvider : CryptokiCertificateProvider
	{
		protected override CertificateInfo GetCertificate(string chipset, byte[] serialNumber) => ObjectFactory.Get<ICryptoApi>().GetCertificateFromWindowsCertificateStore(serialNumber);

		protected override CertificateInfo[] GetCertificates(string chipset) => ObjectFactory.Get<ICryptoApi>().GetCertificatesFromWindowsCertificateStore();
	}
}
