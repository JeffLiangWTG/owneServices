using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;
using Enterprise.Registry.Business;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.Models;

namespace Enterprise.TrustedMessaging.Business
{
	public class UserPortalClientConfiguration : ITrustedClientConfiguration
	{
		public ICertificatePairProvider CertificatePairProvider { get; } = new PairProvider();

		public ISecretKeyStorage SecretKeyStorage { get; } = new KeyStorage();

		public string RemoteEndpointRootUrl => $"{WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.TrimEnd('/')}/api";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public virtual int RetryIntervalInMilliseconds => 5000;

		public int MaxRetryCount => 3;

		#region Implements

		class PairProvider : ICertificatePairProvider
		{
			public CertificatePair GetCertificatePair(string product)
			{
				return new CertificatePair()
				{
					LocalCertificate = GetCert(WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.Value,
												WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.Value,
												includePrivateKey: true),
					RemoteCertificate = GetCert(WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.Value, string.Empty, includePrivateKey: false),
				};
			}

			public CertificatePair GetCertificatePair(string product, string systemId) => GetCertificatePair(product);

			readonly Dictionary<string, X509Certificate2> Certs = new Dictionary<string, X509Certificate2>();

			X509Certificate2 GetCert(byte[] bytes, string password, bool includePrivateKey)
			{
				var key = $"{Convert.ToBase64String(bytes ?? Array.Empty<byte>())}@{password}";

				return Certs.GetOrAdd(key, () =>
					(bytes == null || !bytes.Any()) ? null : X509Certificate2Utilities.TryMakeCertificate(bytes, password, includePrivateKey));
			}
		}

		class KeyStorage : ISecretKeyStorage
		{
			public SecretKey LoadSecretKey(string product, string systemId)
			{
				return new SecretKey()
				{
					Product = product,
					RefId = systemId,
					Key = WebDataRegistry.Instance.TrustedMessagingSecretKey.Value
				};
			}

			public void SaveSecretKey(SecretKey key)
			{
				WebDataRegistry.Instance.TrustedMessagingSecretKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, key.Key);
			}
		}

		#endregion Implements
	}
}
