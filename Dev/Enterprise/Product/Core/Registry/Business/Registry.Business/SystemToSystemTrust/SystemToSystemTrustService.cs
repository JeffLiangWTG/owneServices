using System;
using System.Security.Cryptography.X509Certificates;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.Extensions;
using WTG.IdentitySecurity;
using WTG.OpenIDConnect.Token;

namespace Enterprise.Registry.Business
{
	public class SystemToSystemTrustService
	{
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
		public virtual string GetAccessToken()
		{
			try
			{
				var systemInfo = CheckSystemToSystemCertificate();

				var url = GetEndpoint(systemInfo.TenantId);
				var privateKey = RSAKeyProvider.ImportPrivateKey(systemInfo.PrivateKey);
				var cert = new X509Certificate2(systemInfo.CertificateBytes);
				var azp = systemInfo.ClientId;
				var aud = GetAud();

				var accessToken = OAuthClientAssertion.GetClientAccessTokenAsync(url, privateKey, cert, azp, aud).ConfigureAwait(false).GetAwaiter().GetResult();
				return accessToken;
			}
			catch (InvalidOperationException ex)
			{
				throw new TokenAuthOnboardingApiException(ex.Message, ex);
			}
		}

		ISystemToSystemTrustInfo CheckSystemToSystemCertificate()
		{
			var systemInfo = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;

			if (!systemInfo.IsSetUp())
			{
				throw new InvalidOperationException(Res.GetString("C8B42ED8-7547-4862-90E4-8067015B3897", "The system doesn't have valid certificate."));
			}

			return systemInfo;
		}

		public virtual string GetEndpoint(string tenantId) => $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

		protected virtual string GetAud() => SystemDataRegistry.Instance.EDIClientID.Value;
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
	}
}
