using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Enterprise.Integration;

namespace Enterprise.Security.Shared
{
	public class OidcLoginClaimData
	{
		public string Error { get; }
		public string ErrorDescription { get; }
		public IEnumerable<Claim> LoginClaims { get; }

		public Dictionary<string, string> DbIdentifierToClaimMap { get; } = new Dictionary<string, string>();

		public OidcLoginClaimData(string error, string errorDescription = null)
		{
			Error = error;
			ErrorDescription = errorDescription;
		}

		public OidcLoginClaimData(IEnumerable<Claim> claims, IOIDCConfig oidcConfig)
		{
			LoginClaims = claims;

			MapOidcConfigToClaims(claims, oidcConfig);

			if (!DbIdentifierToClaimMap.Any())
			{
				Error = Res.GetString("DA20A9D3-31A0-42AD-B4DB-40C701261643",
					"Requested claim not found. Returned claims: {0}",
					string.Join(", ", claims.Select(c => c.Type)));
			}
		}

		public string this[string claimType]
		{
			get
			{
				return LoginClaims?.FirstOrDefault(c => c.Type == claimType)?.Value;
			}
		}

		void MapOidcConfigToClaims(IEnumerable<Claim> claims, IOIDCConfig oidcConfig)
		{
			var claimToIdentifierMap = new Dictionary<string, string>();
			foreach (var claimMap in oidcConfig.ClaimsMappings)
			{
				if (!claimToIdentifierMap.ContainsKey(claimMap.ClaimName))
				{
					claimToIdentifierMap.Add(claimMap.ClaimName, claimMap.Identifier);
				}
			}

			foreach (var claim in claims)
			{
				if (claimToIdentifierMap.ContainsKey(claim.Type) && !DbIdentifierToClaimMap.ContainsKey(claim.Type))
				{
					var mapping = claimToIdentifierMap[claim.Type];
					DbIdentifierToClaimMap.Add(mapping, claim.Value);
				}
			}
		}
	}
}
