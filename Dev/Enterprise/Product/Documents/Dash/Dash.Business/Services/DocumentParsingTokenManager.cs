using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using CargoWise.Common.Testing;
using CargoWise.SystemToSystemTrust;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Registry.Business;
using Microsoft.IdentityModel.Tokens;
using WTG.IdentitySecurity;
using WTG.OpenIDConnect.Token;
namespace Enterprise.Dash.Business.Services
{
	public class DocumentParsingTokenManager : IDocumentParsingTokenManager
	{
		#region System to System Trust Authentication Token
		public virtual string GetEndpoint(string tenantId) => FormattableString.Invariant($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token");
		static readonly object LockObject = new();
		[SuppressThreadStaticFieldMessage]
		static string cachedToken;

		[SuppressThreadStaticFieldMessage]
		static ISystemToSystemTrustInfo cachedCertificateInfo;
		public virtual string GetSystemToSystemTrustToken()
		{
			var certificateInfo = GetSystemToSystemCertificate();
			if (ValidateCachedToken(certificateInfo))
			{
				lock (LockObject)
				{
					if (ValidateCachedToken(certificateInfo))
					{
						cachedCertificateInfo = certificateInfo;
						cachedToken = GetClientAccessToken(certificateInfo);
					}
				}
			}
			return cachedToken;
		}
		bool ValidateCachedToken(ISystemToSystemTrustInfo systemToSystemTrustInfo)
		{
			return string.IsNullOrEmpty(cachedToken) || cachedCertificateInfo != systemToSystemTrustInfo || DateTime.Compare(new JwtSecurityToken(cachedToken).ValidTo, ZDateTime.UtcNow.AddMinutes(1).ToDateTime()) <= 0;
		}
		ISystemToSystemTrustInfo GetSystemToSystemCertificate()
		{
			var systemInfo = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			if (string.IsNullOrEmpty(systemInfo.TenantId)
				|| string.IsNullOrEmpty(systemInfo.ClientId)
				|| string.IsNullOrEmpty(systemInfo.PrivateKey)
				|| systemInfo.CertificateBytes.Length == 0)
			{
				throw new AuthenticationException("The system does not have a valid certificate, please update the certificate");
			}
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			return systemInfo;
		}
		protected virtual string GetClientAccessToken(ISystemToSystemTrustInfo certificateInfo)
		{
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			var tokenEndpoint = GetEndpoint(certificateInfo.TenantId);
			var privateKey = RSAKeyProvider.ImportPrivateKey(certificateInfo.PrivateKey);
			var certificate = new X509Certificate2(certificateInfo.CertificateBytes);
			var azp = certificateInfo.ClientId;
			var aud = DocManagerRegistry.Instance.DocumentParserClientId.Value;
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			return OAuthClientAssertion.GetClientAccessTokenAsync(tokenEndpoint, privateKey, certificate, azp, aud).ConfigureAwait(false).GetAwaiter().GetResult();
		}
		#endregion

		#region EDocs Authentication Token

		public virtual bool TryDecryptEDocsAuthToken(string encryptedToken, out EDocsAuthTokenDetails result)
		{
			try
			{
				var decryptedBytes = StorageDocsEncryptionHelper.DecryptDocument(Base64UrlEncoder.DecodeBytes(encryptedToken), GetEncryptionKey());
				var decryptedToken = Encoding.UTF8.GetString(decryptedBytes);
				result = JsonSerializer.Deserialize<EDocsAuthTokenDetails>(decryptedToken);

				return true;
			}
			catch
			{
				result = new EDocsAuthTokenDetails();

				return false;
			}
		}

		static byte[] GetEncryptionKey()
		{
			var keyValue = DocManagerRegistry.Instance.EDocsAuthTokenEncryptionKey.Value;
			if (string.IsNullOrEmpty(keyValue))
			{
				var key = StorageDocsEncryptionHelper.GetRandomBytes(16);
				keyValue = Convert.ToBase64String(key);
				DocManagerRegistry.Instance.EDocsAuthTokenEncryptionKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, keyValue);
				return key;
			}
			return Convert.FromBase64String(keyValue);
		}
		#endregion
	}
}
