using System;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using WTG.TrustedMessaging;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class CertificatesProvider : ICertificatesProvider
	{
		public CertificatesProvider(object context)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory();
				if (context is string product)
				{
					LocalCertificate = EdiTrustedMessagingConfig.GetGlobalCertificateConfig(factory, product, CertificateTypeList.Codes.CentralSystemCertificate)?.GetCertificate();
					RemoteCertificate = EdiTrustedMessagingConfig.GetGlobalCertificateConfig(factory, product, CertificateTypeList.Codes.PreDeploymentCertificate)?.GetCertificate();
					return;
				}
				else if (context is EdiTrustedSystem trustedSystem)
				{
					LocalCertificate = EdiTrustedMessagingConfig.GetGlobalCertificateConfig(factory, trustedSystem.ETS_Product, CertificateTypeList.Codes.CentralSystemCertificate)?.GetCertificate();
					RemoteCertificate = trustedSystem?.CertificateConfig?.GetCertificate();
					return;
				}
			}

			throw new ArgumentOutOfRangeException(context?.ToString());
		}

		public X509Certificate2 LocalCertificate { get; private set; }

		public X509Certificate2 RemoteCertificate { get; private set; }
	}
}
