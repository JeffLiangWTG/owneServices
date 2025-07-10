using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public interface ICryptokiCertificateProvider
	{
		IReadOnlyList<CryptokiCertificate> GetCertificateList(string chipset);
		CryptokiCertificate ReadCertificate(string chipset, byte[] serialNumber);
	}
}
