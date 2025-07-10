using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.OpenIDConnect.Token;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/TokenAuthOnBoarding")]
	public class TokenAuthOnBoardingController : TrustedController
	{
		readonly string badRequestCode = ((int)HttpStatusCode.BadRequest).ToString();

		public TokenAuthOnBoardingController()
		{
		}

		internal TokenAuthOnBoardingController(NLogWrapper logger) : base(logger)
		{
		}

		[HttpGet]
		[Route("oidcconfig/fetch/{databasenumber}")]
		public IHttpActionResult FetchOidcConfig([FromUri] int databaseNumber)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = $"{Request.RequestUri.AbsolutePath}";

			if (!ValidateAndGetEntity(routingPath, databaseNumber, sessionId, out var httpActionResult, out var licenceDatabase, out var ediTokenAuthOnBoardingData))
			{
				return httpActionResult;
			}

			if (!CheckIfDataAreReady(ediTokenAuthOnBoardingData))
			{
				var error = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.DataNotReady}";
				AddErrorLog(error, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				return BadRequest(StatusMessages.DataNotReady);
			}

			var applicationManagement = EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.Value
				.OfType<AzureOpenIDConnectConfiguration>()
				.First(x => x.Code == AzureB2CEnvironmentCodeDescriptionList.Codes.PRD);

			var tokenAuthOnboardingData = new TokenAuthOnboardingDataResponse()
			{
				ConfigurationIdentifier = applicationManagement.ClientID,
				ClaimMappingName = ediTokenAuthOnBoardingData.TOD_ClaimMappingName,
				ClaimMappingIdentifier = ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier,
				AuthorityUrl = applicationManagement.AuthorityUrl,
				DomainHint = ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier
			};

			return Json(tokenAuthOnboardingData);
		}

		[HttpGet]
		[Route("oidcconfig/enable/{databasenumber}")]
		public IHttpActionResult RequestOidcEnbale([FromUri] int databaseNumber)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = $"{Request.RequestUri.AbsolutePath}";

			if (!ValidateAndGetEntity(routingPath, databaseNumber, sessionId, out var httpActionResult, out var licenceDatabase, out var ediTokenAuthOnBoardingData))
			{
				return httpActionResult;
			}

			bool allowEnable;
			switch (licenceDatabase.LD_LicenceType)
			{
				case DatabaseTypes.Codes.Production:
					allowEnable = (ediTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.CustomerTestCompleted
									|| ediTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.Completed)
									&& ediTokenAuthOnBoardingData.TOD_Enabled == true;
					break;
				default:
					allowEnable = CheckIfDataAreReady(ediTokenAuthOnBoardingData);
					break;
			}

			if (!allowEnable)
			{
				var error = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.DataNotReady}";
				AddErrorLog(error, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				return BadRequest(StatusMessages.DataNotReady);
			}

			var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
			var info = $"{NLogWrapper.Status.Ok}";
			AddInfoLog(info, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			return new ResponseMessageResult(httpResponse);
		}

		bool ValidateAndGetEntity(string routingPath, int databaseNumber, string sessionId, out IHttpActionResult httpActionResult, out LicenceDatabase licenceDatabase, out EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData)
		{
			httpActionResult = null;
			licenceDatabase = null;
			ediTokenAuthOnBoardingData = null;

			var accessToken = SystemToSystemTrustHelper.GetBearerToken(Request);
			if (string.IsNullOrEmpty(accessToken))
			{
				var error = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.EmptyAccessToken}";
				AddErrorLog(error, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				httpActionResult = BadRequest(badRequestCode, StatusMessages.EmptyAccessToken);
				return false;
			}

			JwtSecurityToken token = null;
			try
			{
				var authorityUrl = TrustHelper.GetAuthorityUrl(Logger);
				if (!string.IsNullOrEmpty(authorityUrl))
				{
					token = TokenValidator.ValidateAccessToken(authorityUrl, SystemDataRegistry.Instance.EDIClientID.Value, accessToken, ConfigurationHelper.ConfigurationManagerCache, new TokenValidationLogger(new BaseLogger(GetType())), CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				AddErrorLog("The token is invalid.", ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: e);
			}

			if (token == null)
			{
				var warn = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidAccessToken}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				httpActionResult = BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
				return false;
			}

			using (Db.DisposableActionForDbConnection())
			{
				licenceDatabase = Factory.LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, databaseNumber));
				if (licenceDatabase == null || licenceDatabase.LicEnterprise == null)
				{
					var warn = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidLicensePk}";
					AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
					httpActionResult = BadRequest(StatusMessages.InvalidLicensePk);
					return false;
				}

				if (!licenceDatabase.LD_IsActive)
				{
					var error = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.DatabaseNotActive}";
					AddErrorLog(error, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
					httpActionResult = BadRequest(badRequestCode, StatusMessages.DatabaseNotActive);
					return false;
				}

				if (DatabaseStatusList.Codes.REG != licenceDatabase.LD_Status && DatabaseStatusList.Codes.Preregistered != licenceDatabase.LD_Status)
				{
					var error = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.DatabaseNotRegistered}";
					AddErrorLog(error, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
					httpActionResult = BadRequest(badRequestCode, StatusMessages.DatabaseNotRegistered);
					return false;
				}

				var query = new ZQuery(EdiTokenAuthOnBoardingDataSchema.TOD_LE, licenceDatabase.LD_LE);
				ediTokenAuthOnBoardingData = Factory.LoadTop1<EdiTokenAuthOnBoardingData>(query);
				if (ediTokenAuthOnBoardingData == null)
				{
					var error = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidLicensePk}";
					AddErrorLog(error, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
					httpActionResult = BadRequest(StatusMessages.InvalidLicensePk);
					return false;
				}

				return true;
			}
		}

		bool CheckIfDataAreReady(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData) => ediTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.Verified
			|| ediTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.CustomerTestCompleted
			|| ediTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.Completed;

		protected SystemToSystemTrustHelper TrustHelper { get; set; } = new SystemToSystemTrustHelper();

		class StatusMessages
		{
			public const string InvalidLicensePk = "Database Number Is Invalid";
			public const string DatabaseNotActive = "Database Not Active";
			public const string DatabaseNotRegistered = "Database Not Registered";
			public const string DataNotReady = "OIDC Settings Are Not Ready";
			public const string EmptyAccessToken = "Access Token Not Provided";
			public const string InvalidAccessToken = "Invalid Access Token";
			public const string GetOidcError = "Unable To Get OIDC Settings";
		}
	}
}
