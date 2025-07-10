using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Enterprise.Registry.Business;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTG.IdentitySecurity;

namespace Enterprise.Client.EDI.OAuth2
{
	public class DiscoveryDocument : IDiscoveryDocument
	{
		internal DiscoveryDocument(HttpClient httpClient, string discoveryEndpoint)
		{
			this.httpClient = httpClient;
			this.discoveryEndpoint = discoveryEndpoint;
		}

		public DiscoveryDocument(string discoveryEndpoint)
		{
			this.discoveryEndpoint = discoveryEndpoint;
		}

		string GetIssuer()
		{
			var discoveryDocument = HttpClient.GetStringAsync(discoveryEndpoint).GetAwaiter().GetResult();
			var jsonObject = JsonConvert.DeserializeObject<JObject>(discoveryDocument);
			var issuer = jsonObject["issuer"]?.ToString();
			return issuer;
		}

		public string GetAccessToken(JwtPayload customizedPayload)
		{
			if (customizedPayload == null)
			{
				throw new ArgumentNullException(nameof(customizedPayload));
			}

			var issuer = GetIssuer();
			var systemInfo = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
#pragma warning disable CS0618
			var privateKey = RSAKeyProvider.ImportPrivateKey(systemInfo.PrivateKey);
#pragma warning restore CS0618

			var payload = new JwtPayload
			{
				{ "iss", issuer },
				{ "sub", systemInfo.ClientId },
				{ "azp", systemInfo.ClientId },
				{ "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
				{ "nbf", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
				{ "exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() },
				{ "jti", Guid.NewGuid().ToString() },
			};

			foreach (var claim in customizedPayload.Claims)
			{
				if (!payload.ContainsKey(claim.Type))
				{
					payload.AddClaim(claim);
				}
			}

			var certificate = new X509Certificate2(systemInfo.CertificateBytes);
			var accessToken = JwtSecurity.GenerateSignedJwt(privateKey, certificate, payload);
			return accessToken;
		}

		HttpClient HttpClient => httpClient ??= new HttpClient();
		HttpClient httpClient;

		readonly string discoveryEndpoint;
	}
}
