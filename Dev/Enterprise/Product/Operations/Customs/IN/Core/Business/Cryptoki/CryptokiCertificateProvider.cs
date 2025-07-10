using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business;

public sealed class CryptokiCertificateProvider : ICryptokiCertificateProvider
{
	IReadOnlyList<CryptokiCertificate> ICryptokiCertificateProvider.GetCertificateList(string libraryName)
	{
		var certificates = ObjectFactory.Get<ICryptoApi>().GetCertificatesFromTokenWithLibrary(libraryName);
		return certificates.Select(x => ParseCertificate(x)).ToList();
	}

	CryptokiCertificate ParseCertificate(CertificateInfo certificateInfo)
	{
		using (var x509 = new X509Certificate2(certificateInfo.Content))
		{
			return new CryptokiCertificate
			{
				TokenManufacturerId = certificateInfo.TokenManufacturerId,
				SerialNumber = x509.SerialNumber,
				Owner = x509.Subject,
				NotBefore = x509.NotBefore,
				NotAfter = x509.NotAfter,
				Thumbprint = x509.Thumbprint,
				Issuer = x509.Issuer,
				TokenModel = certificateInfo.TokenModel
			};
		}
	}
}
