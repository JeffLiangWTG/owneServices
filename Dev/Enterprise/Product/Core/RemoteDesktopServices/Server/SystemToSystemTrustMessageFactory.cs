using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Enterprise.RemoteDesktopServices.MessageElements;
using Microsoft.IdentityModel.Tokens;

namespace Enterprise.RemoteDesktopServices.Server
{
	static public class SystemToSystemTrustMessageFactory
	{
		/// <summary>
		/// Create a SSO SystemToSystemTrust Message
		/// </summary>
		/// <param name="privateKey">
		/// RSA private key for signing the token
		/// </param>
		/// <param name="clientId">
		/// CW Client ID. Will be the "iss" and "sub" claim
		/// </param>
		/// <param name="audience">
		/// Guid that represents the Web Application, will be the "aud" claim
		/// </param>
		/// <param name="postUrl">
		/// Endpoint for the Web Application to connect to
		/// </param>
		/// <param name="additionalJwtPayload">
		/// Additional Json to merge into the token. Create one like so:
		/// var jwtPayload = JwtPayload.Deserialize(jsonString);
		/// </param>
		/// <remarks>
		/// The default claims that will always be present are as follows: "jti", "sub", "iss", "aud", "exp", "iat", "nbf"
		/// These must be validated by the consuming service, in addition to the signature
		/// </remarks>
		/// <returns>new SystemToSystemTrustMessage</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static public SystemToSystemTrustMessage Create(RSA privateKey, Guid clientId, Guid audience, Uri postUrl, JwtPayload additionalJwtPayload = null)
		{
			var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			var exp = iat + 900; // 15 minute timeout
			var nbf = iat - 120; // 2 minute clock skew

			var cryptoProviderFactory = new CryptoProviderFactory();
			cryptoProviderFactory.CacheSignatureProviders = false;
			var signingKey = new RsaSecurityKey(privateKey) { CryptoProviderFactory = cryptoProviderFactory };

			var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);
			var header = new JwtHeader(credentials);

			var defaultPayload = new JwtPayload
			{
				{ "jti", Guid.NewGuid().ToString() },
				{ "sub", clientId.ToString() },
				{ "iss", clientId.ToString() },
				{ "aud", audience.ToString() },
				{ "exp", exp },
				{ "iat", iat },
				{ "nbf", nbf }
			};

			var payload = new JwtPayload(defaultPayload.Claims);
			if (additionalJwtPayload != null)
			{
				foreach (var claim in additionalJwtPayload.Claims)
				{
					if (!defaultPayload.ContainsKey(claim.Type))
					{
						payload.AddClaim(claim);
					}
				}
			}

			var jwt = new JwtSecurityToken(header, payload);
			return new SystemToSystemTrustMessage() { AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt), PostUrl = postUrl.ToString() };
		}
	}
}
