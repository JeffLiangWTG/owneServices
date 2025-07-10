using System;
using System.IdentityModel.Tokens.Jwt;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.OAuth2
{
	public class AccessTokenProvider
	{
		[ThreadSafe]
		static readonly Lazy<AccessTokenProvider> LazyInstance = new(() => new AccessTokenProvider());

		public static AccessTokenProvider Instance => LazyInstance.Value;

		AccessTokenProvider()
		{
			discoveryDocument = new DiscoveryDocument(EDIDataRegistry.Instance.TokenValidationServiceDiscoveryEndpoint.Value);
		}

#if DEBUG
		internal AccessTokenProvider(IDiscoveryDocument discoveryDocument)
		{
			this.discoveryDocument = discoveryDocument;
		}
#endif

		public string GetAccessToken()
		{
			if (!string.IsNullOrEmpty(cachedAccessToken))
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var jwtToken = tokenHandler.ReadJwtToken(cachedAccessToken);
				if (jwtToken.ValidTo > ZDateTime.UtcNow.ToDateTime())
				{
					return cachedAccessToken;
				}
			}

			var customizedPayload = new JwtPayload
			{
				{ "aud", "api://AzureADTokenExchange" }
			};
			cachedAccessToken = discoveryDocument.GetAccessToken(customizedPayload);
			return cachedAccessToken;
		}

		readonly IDiscoveryDocument discoveryDocument;
		string cachedAccessToken;
	}
}
