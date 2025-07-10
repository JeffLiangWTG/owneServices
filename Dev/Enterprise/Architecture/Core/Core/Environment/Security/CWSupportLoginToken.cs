using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Microsoft.IdentityModel.Tokens;
using WTG.IdentitySecurity;
using static CargoWise.Definitions.Authentication.SupportLogonRole;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	public static class CWSupportLoginToken
	{
		#region Validation

		public static bool IsValidToken(string token) => Validate(token).IsValid;

		public static bool IsValidToken(string token, out CWSupportTokenValidationResult tokenValidationResult)
		{
			tokenValidationResult = Validate(token);
			return tokenValidationResult.IsValid;
		}

		public static CWSupportTokenValidationResult Validate(string token)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			if (!tokenHandler.CanReadToken(token))
			{
				return new CWSupportTokenValidationResult(Res.GetString("E98E3695-64F6-415A-B487-68D9303A4BDD", "The {0} token provided is invalid.", User.SupportUserName));
			}

			try
			{
				var securityToken = ValidateToken(token);
				var jwtSecurityToken = securityToken as JwtSecurityToken;
				var productReg = ObjectFactory.Get<IProductRegistration>();

				if (!productReg.IsWiseTechGlobalInternalSystem())
				{
					var registryKey = productReg.Key;
					var expectCode = registryKey.EnterpriseCode + registryKey.ServerCode;

					var aud = jwtSecurityToken.GetSystemCode();
					if (aud != expectCode)
					{
						return new CWSupportTokenValidationResult(Res.GetString("15FFEE64-3868-4977-B2AE-BCB4281BA4C0", "The {0} token has the incorrect system code: {1}.", User.SupportUserName, aud));
					}
				}
				else if (!jwtSecurityToken.HasCW1InternalRoles())
				{
					return new CWSupportTokenValidationResult(Res.GetString("B256EEED-2BD4-44C5-8A23-5394D26BA35E", "The {0} token can not login internal system.", User.SupportUserName));
				}

				var incidentNumber = jwtSecurityToken.GetIncident();

				return new CWSupportTokenValidationResult(incidentNumber, jwtSecurityToken.GetUserCode(), jwtSecurityToken.GetUserName());
			}
			catch (SecurityTokenExpiredException)
			{
				return new CWSupportTokenValidationResult(Res.GetString("793A9621-6C6C-4E19-BDA2-389E5B4FF28D", "The {0} token is expired.", User.SupportUserName));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return new CWSupportTokenValidationResult(Res.GetString("1461C20C-D23B-4E98-88A3-C1131E530B66", "The {0} token is invalid, error: {1}", User.SupportUserName, ex.Message));
			}
		}

		static SecurityToken ValidateToken(string token)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				return ValidateTokenForTest(Encoding.UTF8.GetBytes(Certificate), token);
			}
#endif
			var publicKey = RawDataRegistry.Instance.CWSupportLoginTokenCertificate.Value;
			var x509 = new X509Certificate2(publicKey);
			var rsa = x509.GetRSAPublicKey();
			var securityToken = JwtSecurity.VerifySignedJwt(rsa, token);
			return securityToken;
		}

		#endregion

#if DEBUG

		static SecurityToken ValidateTokenForTest(byte[] publicKey, string token)
		{
			var x509 = new X509Certificate2(publicKey);
			var rsa = x509.GetRSAPublicKey();

			var parameters = new TokenValidationParameters()
			{
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = true,
				IssuerSigningKey = new RsaSecurityKey(rsa)
			};

			parameters.LifetimeValidator = new LifetimeValidator((DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
			{
				var now = ZDateTime.UtcNow.ToDateTime();

				if (notBefore != null && notBefore > now)
				{
					throw new SecurityTokenNotYetValidException();
				}

				if (expires != null && expires < now)
				{
					throw new SecurityTokenExpiredException();
				}
				return true;
			});

			new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var validatedToken);
			return validatedToken;
		}

		public static string TokenForTest => GenerateTokenForTest("ABC", "", "", ZDateTime.UtcNow, true, ZGuid.BrettsGuid);

		public static string TokenWithNameAndRolesForTest => GenerateTokenForTest("ABC", "", "", ZDateTime.UtcNow, true, ZGuid.BrettsGuid, "UserName");

		public static string GenerateTokenForTest(string userCode, string incident, string systemCode, ZDateTime utcNow, bool isInternal = true, ZGuid jti = new ZGuid(), string userName = null)
		{
			var time = utcNow.IsValid ? utcNow.ToDateTime() : ZDateTime.UtcNow.ToDateTime();
			var payload = new JwtPayload(null, null, null, time, time.AddHours(1), time)
			{
				{ "aud", systemCode },
				{ "sub", userCode },
				{ "incident", incident },
				{ "jti", jti == ZGuid.Empty ? ZGuid.NewZGuid().ToString() : jti.ToString() },
			};

			if (userName != null)
			{
				payload.Add("name", userName);
			}

			var privateKey = RSAKeyProvider.ImportPrivateKey(PrivateKey);
			if (isInternal)
			{
				payload.Add("roles", "cw1inttest");
			}

			var cert = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));
			var token = JwtSecurity.GenerateSignedJwt(privateKey, cert, payload);
			return token;
		}

		const string PrivateKey = @"
-----BEGIN PRIVATE KEY-----
MIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQCcuf4gxhYrUCRl
52pwUJmal84UffcegB6sla+HCXb/kHseHGLgZP0G8yInEKHaMO6Pa/ZHyOtxbzBn
O2lvCMzHfimg0gNyvg6Euto+/RMQ3WApXlug+ecrb6YtTnWB/8P4n8lBd2Jhgt4Q
p/vssqTMYoOeKkEAqI7RhfdNpviPQh8NqFPhNfX11kwlMI6GlWcX1qfIlkLufGAP
GVx4ZKAzM25ASmdj77RGvFFoxeXi2ateBFCQQ2tLsHS741eqZw5UhVI74tuFwk7Z
riWCnhAwCn7CzNm1Bk7kkA1bfyuuZKS9oH9fWMwv2+qrhXBlrl/V7dU4j5LE9Em5
AHEy3LJRAgMBAAECggEAAtYS1UEAcbQ3lGiFspai0PtiTViiOjkTVQg4bvcq4iEZ
OUhGOc1aDuAjhBV2+F3eU1Ye1Nm7+QeMFrIqjEnaetm7ox9p1I+/74L6JB2sVKfM
77K3cchT+KNCnJtI4RthzoQFvluMujsGDcMcegIZnWFDBH+7QWnLe9syyIL3CJCg
eo7MQI1og/r1kADizIIL/EAwe+NsN3mw1ZtoNRWE2gVKfGYoHBiTS7WBQx/Zmt/Z
YqfjpvckuHldPCoEYwks2kT/cmSRxJawh8xlIBVoTKEPiqU3Fvh8Xtu9dp+7NeIK
1rZbmVKNxraJ6Pzh0RoeUx7RP99fMCIvySrQosx3yQKBgQDcsvaI7c/tdH4c0EeP
tHHBiH9uvmq0eb88msGm4LKpWhO2gRA7f3ZPaI1B4Mt6v638JbvGLCcGemz0pWcD
tm39o2PxS/CfxrfsSrmaVUirj8vz3ksG24oM8IHpqSGC7pDCY04L/jrTgC9d7Wp2
mljBi4vu+PVOF9rdmOJ7OjZD7wKBgQC1y4Zyl4TipbiNqrjnNT+Z1vc54ufBooTz
QVl56Vidlb9WrkCJhHz6m4dZpfUUo++fI0lWZvYKPgjZ++GoxXGI00RoaGNODz0V
nCNmYQXghFJdMgzjpVdF+vBIUByB+rNZxY7SbE7yzPY8jEnblBclJXDt3sRYpCsj
vSCX8YEtvwKBgQCGeRT2gdB7ostkyxOPYCcgAQeEdsmVhckcKD9uauLxhU0VmrZF
SnjQEahgR5Q0Cq78QBGVwjlVHSteNZn7DSEftnqi/95xvCc89pr4ipZo0ok24m9c
klAfajv+H2un16ykhNV9QT6euDAkTxDbi1ghNeHhjhgJqUdWVl1JMEL4SQKBgQCl
3rEpVP42Haap+58Bth/1qaGg+1sh3bsYiAOvrCzie4M8/3h7AJtXvRhdv4JB9uHn
fTao0wCij79bxOo8JwpUIQ9FhBk3W8gMM45JFzVZiOHtjoEcI/vhKfiawh2AqtGB
kzOg4pmWB1SUIhwxcIk9p0/GeX8EsaV60lEu0Xdf2QKBgQDVWSqsFlXoh0yXIFc5
0nH63lwSeHKGUeTl2lOV8R5E9WT70rMqMC7tARS7DET/kO90BF4R9FlC2XQnuNFJ
2KVoDvTgciiYNhrH5yVi1Nm2/sjvUF7Pqm0SJmHO7I8QITPvyKjjbP82eeLSEeaD
94pZuFVcB0kr7Oz5+kuh4Ia90Q==
-----END PRIVATE KEY-----
";

		const string Certificate = @"
-----BEGIN CERTIFICATE-----
MIIDgzCCAmugAwIBAgIUCP33gIMaTkVQN80o+mVvzf2IvMgwDQYJKoZIhvcNAQEL
BQAwUTELMAkGA1UEBhMCQ04xCzAJBgNVBAgMAkpTMQswCQYDVQQHDAJOSjEMMAoG
A1UECgwDV1RHMQwwCgYDVQQLDANOSkcxDDAKBgNVBAMMA0pheTAeFw0yMzA0Mjcw
NjAwMTlaFw0yMzA1MjcwNjAwMTlaMFExCzAJBgNVBAYTAkNOMQswCQYDVQQIDAJK
UzELMAkGA1UEBwwCTkoxDDAKBgNVBAoMA1dURzEMMAoGA1UECwwDTkpHMQwwCgYD
VQQDDANKYXkwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCcuf4gxhYr
UCRl52pwUJmal84UffcegB6sla+HCXb/kHseHGLgZP0G8yInEKHaMO6Pa/ZHyOtx
bzBnO2lvCMzHfimg0gNyvg6Euto+/RMQ3WApXlug+ecrb6YtTnWB/8P4n8lBd2Jh
gt4Qp/vssqTMYoOeKkEAqI7RhfdNpviPQh8NqFPhNfX11kwlMI6GlWcX1qfIlkLu
fGAPGVx4ZKAzM25ASmdj77RGvFFoxeXi2ateBFCQQ2tLsHS741eqZw5UhVI74tuF
wk7ZriWCnhAwCn7CzNm1Bk7kkA1bfyuuZKS9oH9fWMwv2+qrhXBlrl/V7dU4j5LE
9Em5AHEy3LJRAgMBAAGjUzBRMB0GA1UdDgQWBBTo1MWjmpOSg/7y5ATNLHQ2YF02
7zAfBgNVHSMEGDAWgBTo1MWjmpOSg/7y5ATNLHQ2YF027zAPBgNVHRMBAf8EBTAD
AQH/MA0GCSqGSIb3DQEBCwUAA4IBAQB3h0jYTuIN7ELcbSiuF5L0Rx19XaI3XDOJ
3A6GBCF/6UOUKM8+aklQGAeUrySOvzVqe4xDiuHN9WZhLZJtSEDfwZDIJX7n9LE6
IB/lqhI/RL5JdGNJ/nBv1TEYpfdzo6CAhwfCCZMSYTYLuVWX3MbmCSsi8hSPbS2j
DTHOzf+IKwhfrVMJPdcvKNDXm1+b29X1gakcl5/X0ov9BSo1jjip+7HY2J8OXbCE
LyEr/Vq5280ljJ1RzKR43OAEH+XI/eBi1SBTncm3B+Qxn7tN4AjQ4EtriN41xSK6
6KUR9JRMYiMgwm9W/4XaPqYITba7/Q4KJm3YmiE0CxChUe9yDToM
-----END CERTIFICATE-----

";

#endif
	}

	static class JwtSecurityTokenExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Jwt token")]
		public static string GetIncident(this JwtSecurityToken jwtSecurityToken) => jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "incident")?.Value;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Jwt token")]
		public static string GetSystemCode(this JwtSecurityToken jwtSecurityToken) => jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "aud")?.Value;

		public static string GetUserCode(this JwtSecurityToken jwtSecurityToken) => jwtSecurityToken.Subject;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Jwt token")]
		public static bool HasCW1InternalRoles(this JwtSecurityToken jwtSecurityToken)
		{
			var supportLogonRole = new SupportLogonRole(UserType.CW1, DatabaseType.Test, SystemType.Internal);
			return jwtSecurityToken.Claims.Any(claim => claim.Type == "roles" && claim.Value == supportLogonRole.GetRole());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Jwt token")]
		public static string GetUserName(this JwtSecurityToken jwtSecurityToken) => jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value;
	}

	public class CWSupportTokenValidationResult
	{
		public CWSupportTokenValidationResult(string incident, string userCode, string userName)
		{
			IsValid = true;
			Incident = incident;
			UserCode = userCode;
			UserName = userName;
		}

		public CWSupportTokenValidationResult(string failedReason)
		{
			IsValid = false;
			FailedReason = failedReason;
		}

		public bool IsValid { get; }
		public string Incident { get; }
		public string UserCode { get; }
		public string FailedReason { get; }
		public string UserName { get; }
	}
}
