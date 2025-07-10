using System;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.Messaging.Business.EDICommunicationAuthInbound
{
	public class CertificateDataCollection : NonPersistentBusinessObjectCollection<CertificateData>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CertificateData();
		}

		readonly string _clientId;

		public CertificateDataCollection(string clientId)
		{
			_clientId = clientId;
		}

		public void LoadCollection(out Exception exception)
		{
			exception = null;
			if (string.IsNullOrEmpty(_clientId))
			{
				return;
			}

			var result = ObjectFactory.Get<ICertificateManager>();
			var certificates = result.DownloadCertificates(_clientId);

			foreach (var cert in certificates)
			{
				if (CertificateUtility.TryReadCertificate(cert, out X509Certificate2 certificate, out string pem, out exception))
				{
					Add(new CertificateData
					{
						CommonName = CertificateUtility.GetCertificateSubjectCN(certificate),
						SerialNumber = certificate.SerialNumber,
						Issuer = CertificateUtility.GetCertificateIssuerCN(certificate),
						ValidFrom = certificate.GetEffectiveDateString(),
						ValidTo = certificate.GetExpirationDateString(),
						CertificatePem = pem,
					});
				}
			}
		}
	}
}
