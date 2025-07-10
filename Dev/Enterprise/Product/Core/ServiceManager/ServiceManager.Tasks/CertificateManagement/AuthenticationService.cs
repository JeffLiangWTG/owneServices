using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using CargoWise.SystemToSystemTrust.Extensions;
using Enterprise.Registry.Business;
using WTG.IdentitySecurity;
using WTG.OpenIDConnect.Token;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement
{
	class AuthenticationService : IAuthenticationService
	{
		public string GetAccessToken()
		{
			var trustInfo = SystemDataRegistry.Instance.SystemToSystemCertificate?.Value;
			if (trustInfo is null || !trustInfo.IsSetUp())
			{
				throw new InvalidOperationException($"{nameof(SystemDataRegistry.SystemToSystemCertificate)} is not valid. Please ensure System to System Trust configuration has been setup and the TCM task has run successfully");
			}
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			return GetAccessToken(trustInfo.PrivateKey, trustInfo.ClientId, trustInfo.TenantId, trustInfo.CertificateBytes);
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
		}

		public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
		{
			var tokenServicesFactory = ObjectFactory.Get<ITokenServicesFactory>();
			var tokenGeneratorService = tokenServicesFactory.GetTokenGeneratorService();
			var request = new SignCwTokenRequest(SystemDataRegistry.Instance.EDIClientID.Value);
			var response = await tokenGeneratorService.SignCwTokenAsync(request, cancellationToken).ConfigureAwait(false);
			return response.Token;
		}

		string GetAccessToken(string privateKeyPem, string clientId, string tenantId, byte[] certificate)
		{
			var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
			var privateKey = RSAKeyProvider.ImportPrivateKey(privateKeyPem);
			var cert = new X509Certificate2(certificate);
			var ediClientID = SystemDataRegistry.Instance.EDIClientID.Value;
			var accessToken = OAuthClientAssertion.GetClientAccessTokenAsync(url, privateKey, cert, clientId, ediClientID).ConfigureAwait(false).GetAwaiter().GetResult();
			return accessToken;
		}
	}
}
