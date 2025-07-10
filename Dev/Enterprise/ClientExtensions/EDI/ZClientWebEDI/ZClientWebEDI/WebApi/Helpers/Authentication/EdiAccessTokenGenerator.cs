using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class EdiAccessTokenGenerator
	{
		#region Access Token + Fresh Token

		public static (string, string) CreateTokens(Guid ownerId, string ownderTableCode, string resourceProduct, string resourceSystemId, IEnumerable<KeyValuePair<string, string>> payload, DateTime expiry, bool createRefreshToken, bool isEncryptionRequired)
		{
			var factory = new BusinessObjectFactory();
			var resourceSystem = EdiTrustedSystem.Load(factory, resourceProduct, resourceSystemId);
			if (resourceSystem == null)
			{
				return (string.Empty, string.Empty);
			}

			var certProvider = new CertificatesProvider(resourceSystem);
			if (certProvider.LocalCertificate == null)
			{
				return (string.Empty, string.Empty);
			}

			X509SigningCredentials signingCredentials = new X509SigningCredentials(certProvider.LocalCertificate);
			X509EncryptingCredentials encryptingCredentials = null;
			if (isEncryptionRequired && certProvider.RemoteCertificate != null)
			{
				encryptingCredentials = new X509EncryptingCredentials(certProvider.RemoteCertificate);
			}

			var utcNow = ZDateTime.UtcNow.ToDateTime();
			var accessTokenExpiry = expiry;
			if (accessTokenExpiry == null)
			{
				accessTokenExpiry = utcNow.AddMinutes(15);
			}

			var claims = new List<Claim>();
			if (payload != null)
			{
				foreach (var pair in payload)
				{
					claims.Add(new Claim(pair.Key, pair.Value));
				}
			}

			var handler = new JwtSecurityTokenHandler();
			var accessToken = handler.CreateEncodedJwt(
				issuer: "myaccount",
				audience: resourceProduct + "-" + resourceSystemId,
				subject: new ClaimsIdentity(claims),
				notBefore: utcNow,
				expires: accessTokenExpiry,
				issuedAt: utcNow,
				signingCredentials: signingCredentials,
				encryptingCredentials: encryptingCredentials
			);

			var refreshToken = string.Empty;
			if (createRefreshToken)
			{
				var refreshTokenExpiry = utcNow.AddHours(24);
				var scope = JsonConvert.SerializeObject(payload);
				refreshToken = CreateRefreshToken(ownerId, ownderTableCode, resourceProduct, resourceSystemId, scope, refreshTokenExpiry);
			}

			return (accessToken, refreshToken);
		}

		#endregion

		#region Refresh Token

		public static string CreateRefreshToken(Guid ownerId, string ownerTableCode, string resourceProduct, string resourceSystemId, string scope, DateTime expiresAtUtc)
		{
			var token = GenerateRandomToken();

			using (var command = Db.Connection.Command("EdiCreateToken"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@Token", SqlDbType.VarChar, token);
				command.AddParameter("@OwnerID", SqlDbType.UniqueIdentifier, ownerId);
				command.AddParameter("@OwnerTableCode", SqlDbType.VarChar, 3, 0, 0, ownerTableCode);
				command.AddParameter("@ResourceProduct", SqlDbType.VarChar, 3, 0, 0, resourceProduct);
				command.AddParameter("@ResourceSystemId", SqlDbType.VarChar, 128, 0, 0, resourceSystemId);
				command.AddParameter("@Scope", SqlDbType.VarChar, -1, 0, 0, scope);
				command.AddParameter("@ExpiresAtUtc", SqlDbType.DateTime, expiresAtUtc);
				command.AddOutputParameter("@Result", SqlDbType.Bit, 0, 0, 0, null);
				command.ExecuteNonQuery();
				var result = (bool)command.GetParameterValue("@Result");
				if (result)
				{
					return token;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public static bool ConsumeRefreshToken(string refreshToken, out EdiAccessTokenInfo tokenInfo)
		{
			using (var command = Db.Connection.Command("EdiConsumeToken"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@Token", SqlDbType.VarChar, refreshToken);
				command.AddOutputParameter("@OwnerID", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.AddOutputParameter("@OwnerTableCode", SqlDbType.VarChar, 3, 0, 0, null);
				command.AddOutputParameter("@ResourceProduct", SqlDbType.VarChar, 3, 0, 0, null);
				command.AddOutputParameter("@ResourceSystemId", SqlDbType.VarChar, 128, 0, 0, null);
				command.AddOutputParameter("@Scope", SqlDbType.VarChar, -1, 0, 0, null);
				command.AddOutputParameter("@Result", SqlDbType.Bit, 0, 0, 0, null);
				command.ExecuteNonQuery();
				var result = (bool)command.GetParameterValue("@Result");
				if (result)
				{
					tokenInfo = new EdiAccessTokenInfo()
					{
						OwnerId = (Guid)command.GetParameterValue("@OwnerID"),
						OwnerTableCode = (string)command.GetParameterValue("@OwnerTableCode"),
						ResourceProduct = (string)command.GetParameterValue("@resourceProduct"),
						ResourceSystemId = (string)command.GetParameterValue("@resourceSystemId"),
						Scope = (string)command.GetParameterValue("@scope")
					};
				}
				else
				{
					tokenInfo = default;
				}
				return result;
			}
		}

		static string GenerateRandomToken()
		{
			const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
			int length = 128;
			var builder = new StringBuilder();
			using var rng = RandomNumberGenerator.Create();
			byte[] uintBuffer = new byte[sizeof(uint)];
			while (length-- > 0)
			{
				rng.GetBytes(uintBuffer);
				uint num = BitConverter.ToUInt32(uintBuffer, 0);
				builder.Append(valid[(int)(num % (uint)valid.Length)]);
			}
			return builder.ToString();
		}

		#endregion
	}
}
