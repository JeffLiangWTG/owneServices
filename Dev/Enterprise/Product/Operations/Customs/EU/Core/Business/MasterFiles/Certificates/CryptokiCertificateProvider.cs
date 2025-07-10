using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Cryptoki.Common.ClientServerApi;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public abstract class CryptokiCertificateProvider : ICryptokiCertificateProvider
	{
		#region ICryptokiCertificateProvider

		IReadOnlyList<CryptokiCertificate> ICryptokiCertificateProvider.GetCertificateList(string chipset)
		{
			var certificates = GetCertificates(chipset);
			return WrapCertificates(chipset, certificates);
		}

		CryptokiCertificate ICryptokiCertificateProvider.ReadCertificate(string chipset, byte[] serialNumber)
		{
			var certificateInfo = GetCertificate(chipset, serialNumber);
			return (certificateInfo == null)
				? null
				: ParseCertificate(chipset, certificateInfo);
		}

		#endregion

		#region Implementation

		protected abstract CertificateInfo GetCertificate(string chipset, byte[] serialNumber);

		protected abstract CertificateInfo[] GetCertificates(string chipset);

		protected CryptokiCertificate ParseCertificate(string chipset, CertificateInfo certificateInfo)
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
					TokenModel = certificateInfo.TokenModel,
					TokenChipset = chipset
				};
			}
		}

		IReadOnlyList<CryptokiCertificate> WrapCertificates(string chipset, CertificateInfo[] certificates)
		{
			var wrappedCertificates = new List<CryptokiCertificate>();

			foreach (var cert in certificates)
			{
				wrappedCertificates.Add(ParseCertificate(chipset, cert));
			}

			return wrappedCertificates;
		}

		#endregion
	}
}
