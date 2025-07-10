using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Microsoft.IdentityModel.Tokens;
using NLog;
using WTG.OpenIDConnect.Token;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class OIDCLoginHelper
	{
		const int proofKeyLength = 64;

		public static bool IsOIDCReady()
		{
			var domains = EDIDataRegistry.Instance.RedirectedEmailDomains.Value;
			var organisations = EDIDataRegistry.Instance.RedirectedOrganisations.Value;
			return domains != null && domains.Length > 0 && organisations != null && organisations.Length > 0;
		}

		public static bool IsMatchDomain(string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				return false;
			}

			var matchDomain = false;
			var domains = EDIDataRegistry.Instance.RedirectedEmailDomains.Value;
			foreach (var domain in domains)
			{
				if (email.EndsWith(domain, StringComparison.OrdinalIgnoreCase))
				{
					matchDomain = true;
					break;
				}
			}
			return matchDomain;
		}

		public static bool IsMatchOrganisation(BusinessObjectFactory factory, string organisationCode)
		{
			if (string.IsNullOrEmpty(organisationCode))
			{
				return false;
			}

			var organisations = EDIDataRegistry.Instance.RedirectedOrganisations.Value;
			var header = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, organisationCode));

			return header != null && organisations.Contains(header.PK.ToGuid());
		}

		public static bool CanLoginViaOIDC(BusinessObjectFactory factory, string email, string organisationCode)
		{
			return IsMatchDomain(email) && IsMatchOrganisation(factory, organisationCode);
		}

		public static bool ShouldRedirectToIDP(BusinessObjectFactory factory, string emailAddress, string organisationCode = "")
		{
			if (string.IsNullOrEmpty(organisationCode))
			{
				return IsMatchDomain(emailAddress);
			}
			else
			{
				return IsMatchOrganisation(factory, organisationCode);
			}
		}

		/*
		 * Reference: https://datatracker.ietf.org/doc/html/rfc7636#section-4.1
		 */
		public static (string verifier, string challenge) GenerateOIDCProofKeyPair()
		{
			var codeVerifier = string.Empty;
			using (var crypto = RandomNumberGenerator.Create())
			{
				var bytesArray = new byte[proofKeyLength];
				crypto.GetBytes(bytesArray);
				codeVerifier = Base64UrlEncoder.Encode(bytesArray);
			}

			var codeChallenge = GenerateCodeChallenge(codeVerifier);
			return (codeVerifier, codeChallenge);
		}

		/*
		 * Reference: https://datatracker.ietf.org/doc/html/rfc7636#section-4.2
		 */
		public static string GenerateCodeChallenge(string verifier)
		{
			var codeChallenge = string.Empty;
			using (var sha256 = SHA256.Create())
			{
				var challengeBytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(verifier));
				codeChallenge = Base64UrlEncoder.Encode(challengeBytes);
			}

			return codeChallenge;
		}

		public static Uri GetOIDCTokenUri(string code, IDictionary<string, string> customizedArguments, NLogWrapper logger, CancellationToken cancellationToken)
		{
			var documents = ConfigurationHelper.GetDiscoveryDocument(SystemDataRegistry.Instance.OIDCConfig.Value.AuthorityURL, ConfigurationHelper.ConfigurationManagerCache, new TokenValidationLogger(new BaseLogger(typeof(OIDCLoginHelper))), cancellationToken).Result;
			var uri = new UriBuilder(documents.TokenEndpoint);
			var query = HttpUtility.ParseQueryString(uri.Query);
			query.Add("code", code);
			query.Add("grant_type", "authorization_code");
			if (customizedArguments != null)
			{
				foreach (var pair in customizedArguments)
				{
					query.Add(pair.Key, pair.Value);
				}
			}

			uri.Query = query.ToString();
			return uri.Uri;
		}

		public static bool IsCurrentUserLoggingInViaOIDC => Convert.ToBoolean(HttpContext.Current?.Session[OIDCLoginHelper.Constants.OIDCSessionKey]);

		public static Uri GetOIDCCodeUrl(IDictionary<string, string> customizedArguments, NLogWrapper logger, CancellationToken cancellationToken)
		{
			var documents = ConfigurationHelper.GetDiscoveryDocument(SystemDataRegistry.Instance.OIDCConfig.Value.AuthorityURL, ConfigurationHelper.ConfigurationManagerCache, new TokenValidationLogger(new BaseLogger(typeof(OIDCLoginHelper))), cancellationToken).Result;
			var uri = new UriBuilder(documents.AuthorizationEndpoint);
			var query = HttpUtility.ParseQueryString(uri.Query);

			query.Add("client_id", EDIDataRegistry.Instance.MyAccountQueryOIDCClientID.Value);
			query.Add("scope", $"openid {EDIDataRegistry.Instance.MyAccountQueryOIDCClientID.Value}");
			query.Add("response_type", "code");

			query.Add("redirect_uri", EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.TrimEnd('/') + "/Login/OIDCLoginComplete.aspx");
			query.Add("domain_hint", SystemDataRegistry.Instance.OIDCConfig.Value.OIDCServerType.ToString());
			query.Add("response_mode", "query");
			if (customizedArguments != null)
			{
				foreach (var pair in customizedArguments)
				{
					query.Add(pair.Key, pair.Value);
				}
			}

			uri.Query = query.ToString();
			return uri.Uri;
		}

		public static string IDPErrorMessageProcessor(string error, string errorMessage, NLogWrapper logger)
		{
			var errorMsg = $"MyAccount OIDC | Received a error form | {error} | {errorMessage}";
			logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.InternalServerError));
			return ErrorList.GeneralIDPMessage + "(Unexpected response)";
		}

		public static string TokenValidationExceptionMessageProcessor(string tokenQueryResult, ValidateTokenException exception, NLogWrapper logger)
		{
			var errorMsg = $"MyAccount OIDC | Validating Token Field | {tokenQueryResult} | {exception.Message} | {exception?.InnerException?.Message ?? string.Empty}";
			logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.InternalServerError));
			return ErrorList.GeneralIDPMessage + "(Invalid token)";
		}

		public static OrgContact[] GetAllContactsViaUserEmailAndPersonPK(BusinessObjectFactory factory, string emailAddress, ZGuid personPK, string organisationCode = "")
		{
			if (string.IsNullOrEmpty(emailAddress))
			{
				return Array.Empty<OrgContact>();
			}

			var contactQuery = new ZDBOnlyQuery(typeof(EDIOrgContact));
			contactQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
			contactQuery.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
			contactQuery.AddToFilter(OrgContactSchema.OC_PER, personPK);
			contactQuery.AddToFilter(OrgContactSchema.OC_Email, emailAddress);

			if (!string.IsNullOrEmpty(organisationCode))
			{
				var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				subQuery.AddToFilter(OrgHeaderSchema.OH_Code, organisationCode);
				contactQuery.AddSubQuery(OrgContactSchema.OC_OH, subQuery, JoinCondition.And);
			}

			return factory.Load<EDIOrgContact>(contactQuery);
		}

		public static class ErrorList
		{
			public static string GeneralIDPMessage => Res.GetString("7ff67cee-1fdd-4ecc-a349-e8c48491ca6e", "We were unable to verify your identity with the external provider. Please try again.");

			public static string MissingAuthorizationCode => GeneralIDPMessage + "(Empty Code)";

			public static string MissingIDToken => GeneralIDPMessage + "(Error response)";

			public static string NullJWTSecurityToken => GeneralIDPMessage + "(Unreadable token)";

			public static string SessionIsInvalid => Res.GetString("0301782c-4227-401d-8220-0a4528c05cc0", "Invalid session. Please try again.");

			public static string MissingVerifierOrNonce => Res.GetString("55c81e64-3be5-41a6-8ebc-132243994c38", "The certificate has expired. Please try again.");

			public static string NonceDoesNotMatchCookie => MissingVerifierOrNonce + "(Invalid nonce)";

			public static string UserIsNotAvailable => Res.GetString("f84f9828-dba2-44f8-84a6-b1054bf625c5", "We were unable to match the external identity to a user in our system.");

			public static string NoUser => Res.GetString("15dfd4dd-2209-4893-865c-b0d2d979fd2f", "We were unable to match the external identity to an active user in our system.");

			public static string LoginFailedMessage => Res.GetString("b66281a8-feac-48a5-905b-56e238656443", "Login Failed");

			public static string UserNotRedirectedToIDP => Res.GetString("a6986dcb-1131-402e-af0e-a5a7df8d2bd5", "Unexpected Error - User was not redirected to IDP");

			public static string FailedToRedirectToIDP => Res.GetString("05c2517a-fe03-4049-a408-f4015b6fd6b5", "Unexpected Error - Failed to redirect to IDP");
		}

		public static class Constants
		{
			public const string OIDCCookieName = "MyAccountVerificationData";

			public const string UserOrgCodeKey = "OrganisationCode";

			public const string UserEmailAddressKey = "EmailAddress";

			public const string CodeKey = "code";

			public const string StateKey = "state";

			public const string NonceKey = "nonce";

			public const string VerifierKey = "code_verifier";

			public const string CodeChallengeKey = "code_challenge";

			public const string CodeChallengeMethodKey = "code_challenge_method";

			public const string LoginHintKey = "login_hint";

			public const string ErrorKey = "error";

			public const string ErrorDescriptionKey = "error_description";

			public const string SwitchCompanySessionKey = "SwitchCompanyFunc";

			public const string OIDCSessionKey = "IsOIDCEnabled";
		}
	}
}
