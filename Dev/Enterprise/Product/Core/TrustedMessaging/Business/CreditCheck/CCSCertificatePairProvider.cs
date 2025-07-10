using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.TrustedMessaging.Business.CreditCheck
{
	public class CCSCertificatePairProvider : ICertificatePairProvider
	{
		public CCSCertificatePairProvider(ICertificateAuthority certificateAuthority, string productCode, string systemId)
		{
			this.certificateAuthority = certificateAuthority;
			this.productCode = productCode;
			this.systemId = systemId;
		}

		readonly ICertificateAuthority certificateAuthority;
		readonly string productCode;
		readonly string systemId;
		const string CCSProductCode = "CCS";

		public CertificatePair GetCertificatePair(string product)
		{
			if (IsNullOrEmpty(OrganisationsDataRegistry.Instance.CreditReportsPublicCertificate.Value))
			{
				var remoteCert = GetCcsCertificate(productCode, systemId)?.Export(X509ContentType.Cert);
				if (remoteCert != null)
				{
					OrganisationsDataRegistry.Instance.CreditReportsPublicCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, remoteCert);
				}
			}

			return new CertificatePair()
			{
				LocalCertificate = GetCert(WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.Value,
											WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.Value, includePrivateKey: true),
				RemoteCertificate = GetCert(OrganisationsDataRegistry.Instance.CreditReportsPublicCertificate.Value, "", includePrivateKey: false)
			};
		}

		public CertificatePair GetCertificatePair(string product, string systemId) => GetCertificatePair(product);

		readonly Dictionary<string, X509Certificate2> Certs = new Dictionary<string, X509Certificate2>();

		bool IsNullOrEmpty(byte[] bytes) => bytes == null || !bytes.Any();

		X509Certificate2 GetCert(byte[] bytes, string password, bool includePrivateKey)
		{
			var key = $"{Convert.ToBase64String(bytes ?? Array.Empty<byte>())}@{password}";

			return Certs.GetOrAdd(key, () =>
				(bytes == null || !bytes.Any()) ? null : X509Certificate2Utilities.TryMakeCertificate(bytes, password, includePrivateKey));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "same with sever side type")]
		X509Certificate2 GetCcsCertificate(string productCode, string systemId)
		{
			X509Certificate2 ccsCertificate = null;
			var ccsCertificateResponse = certificateAuthority.GetCertificate(new CertificateInfo()
			{
				Product = productCode,
				SystemId = systemId,
				CertificateOwnerProduct = CCSProductCode,
				CertificateOwnerSystemId = string.Empty,
				InfoExpires = DateTime.Now.AddHours(12)
			}).Result;

			if (ccsCertificateResponse == null || !ccsCertificateResponse.Success || ccsCertificateResponse.Response?.CertificateData == null)
			{
				var exceptionText = new StringBuilder();
				exceptionText.AppendLine($"Failed to retrieve public certificate for product:{productCode}, system:{systemId}.");
				if (ccsCertificateResponse != null)
				{
					exceptionText.AppendLine($"Is success:{ccsCertificateResponse.Success}.");
					if (ccsCertificateResponse.Response?.CertificateData == null)
					{
						exceptionText.AppendLine((NoResString)"No certificate data in response.");
					}

					if (ccsCertificateResponse.Messages?.Any() ?? false)
					{
						exceptionText.AppendLine((NoResString)"Error Code & Messages:");
						ccsCertificateResponse.Messages.ForEach(o => exceptionText.AppendLine($"Code/Message:{o.Code},{o.Message}"));
					}

					if (ccsCertificateResponse.InnerException != null)
					{
						var innerExceptions = ccsCertificateResponse.InnerException.FlattenInnerExceptions();
						if (innerExceptions?.Any() ?? false)
						{
							exceptionText.AppendLine((NoResString)"Flatten inner exceptions:");
							innerExceptions.ForEach(o =>
							{
								exceptionText.AppendLine($"Inner exception message: {o.Message}");
								exceptionText.AppendLine($"Inner exception stack trace: {o.StackTrace}");
							});
						}
					}
				}

				throw new Exception(exceptionText.ToString());
			}
			else
			{
				try
				{
					ccsCertificate = X509Certificate2Utilities.TryMakeCertificate(ccsCertificateResponse.Response.CertificateData, null, false);
				}
				catch
				{
					throw;
				}
			}

			return ccsCertificate;
		}
	}
}
