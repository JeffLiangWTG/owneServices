using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Microsoft.IdentityModel.Tokens;
using WTG.IdentitySecurity;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public static class JwtHelper
	{
		public static string GenerateJWTToken(string clientID, string authURI, string certificate, string privateKey)
		{
			var jwtToken = "";

			try
			{
				using (var privateRsa = RSAKeyProvider.ImportPrivateKey(privateKey))
				using (var cert = new X509Certificate2(Encoding.ASCII.GetBytes(certificate)))
				using (var certWithPrivate = cert.CopyWithPrivateKey(privateRsa))
				{
					var securityKey = new X509SecurityKey(certWithPrivate);

					var header = new JwtHeader(new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256));

					var t = ZDateTime.UtcNow - new DateTime(1970, 1, 1);
					int iat = (int)t.TotalSeconds;
					int exp = iat + 380000;
					int nbf = iat - 100;

					var payload = new JwtPayload
					{
						{ (NoResString)"sub", clientID },
						{ (NoResString)"iss", clientID },
						{ (NoResString)"aud", authURI },
						{ (NoResString)"exp", exp },
						{ (NoResString)"iat", iat },
						{ (NoResString)"nbf", nbf }
					};
					var secToken = new JwtSecurityToken(header, payload);

					var base64Thumbprint = Convert.ToBase64String(HexStringToHex(certWithPrivate.Thumbprint));
					secToken.Header.Add("x5t", base64Thumbprint);

					jwtToken = new JwtSecurityTokenHandler().WriteToken(secToken);
				}
			}
			catch (Exception ex)
			{
				throw new OAuth2Exception("An error occured when creating the Client Assertion.", ex);
			}

			return jwtToken;
		}

		public static byte[] HexStringToHex(string inputHex)
		{
			var resultantArray = new byte[inputHex.Length / 2];
			for (var i = 0; i < resultantArray.Length; i++)
			{
				resultantArray[i] = Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
			}
			return resultantArray;
		}
	}
}
