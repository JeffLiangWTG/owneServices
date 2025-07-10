using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.OpenIDConnect.Token;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class TrustedContextV3<TRequest, TResponse> : ITrustedContext<TRequest, TResponse> where TRequest : SystemToSystemTrustedInfo
	{
		TrustedContextV3()
		{
		}

		public static TrustedContextV3<TRequest, TResponse> New(TRequest request, TrustedController controller, SystemToSystemTrustHelper trustHelper = null)
		{
			var sessionId = Guid.NewGuid().ToString();
			var context = new TrustedContextV3<TRequest, TResponse>()
			{
				Success = false,
				Product = request.Product,
				RequestInfo = request,
				Messages = null,
				Controller = controller,
				SessionId = sessionId,
			};

			if (trustHelper == null)
			{
				trustHelper = new SystemToSystemTrustHelper();
			}

			var authorityUrl = trustHelper.GetAuthorityUrl(controller.Logger);

			if (string.IsNullOrEmpty(authorityUrl))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Server_Error, ErrorCodes.Descriptions.Server_Error);
				return context;
			}

			var audience = SystemDataRegistry.Instance.EDIClientID.Value;
			var accessToken = SystemToSystemTrustHelper.GetBearerToken(controller.Request);

			try
			{
				context.Token = TokenValidator.ValidateAccessToken(authorityUrl, audience, accessToken, ConfigurationHelper.ConfigurationManagerCache, new TokenValidationLogger(new BaseLogger(typeof(TrustedContextV3<,>))), CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				controller.AddErrorLog("The token is invalid.", ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: controller.RoutingPath, ex: e);
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_MissingSystemToSystemToken, ErrorCodes.Descriptions.Validation_MissingSystemToSystemToken);
				return context;
			}

			if (context.Token == null)
			{
				controller.AddErrorLog("The token is invalid.", ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: controller.RoutingPath);
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_MissingSystemToSystemToken, ErrorCodes.Descriptions.Validation_MissingSystemToSystemToken);
				return context;
			}

			if (string.IsNullOrWhiteSpace(request.Product))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidSystem, ErrorCodes.Descriptions.Validation_InvalidSystem);
				return context;
			}

			var timestampThreshold = ZDateTime.UtcNow.AddHours(-24);
			if (request.InfoTimestamp == DateTime.MinValue || request.InfoTimestamp < timestampThreshold)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Critical_InfoExpired, ErrorCodes.Descriptions.Critical_InfoExpired);
				return context;
			}

			var approvedAzps = EDIDataRegistry.Instance.AzpsApprovedForMyAccount.Value;
			var azp = context.Token.Claims.FirstOrDefault(claim => claim.Type == "azp")?.Value;
			if (string.IsNullOrEmpty(azp) || !Guid.TryParse(azp, out _) || !approvedAzps.Contains(new CodeDescriptionPair(azp, request.Product)))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_UnauthorizedParty, ErrorCodes.Descriptions.Validation_UnauthorizedParty);
				return context;
			}

			context.Success = true;
			return context;
		}

		public IHttpActionResult CreateHttpActionResult()
		{
			if (ResponseInfo is bool response && response)
			{
				LogOkResponse();
				return new OkResult(Controller);
			}

			if (ResponseInfo == null || ResponseInfo is bool)
			{
				SystemToSystemTrustMessagingLogger.LogBadRequestWithErrorMessage(Controller.Logger, Controller.RoutingPath, this);
				return new BadRequestWithErrorMessages(Messages, Controller);
			}
			else
			{
				LogOkResponse();
				return new OkNegotiatedContentResult<TResponse>(ResponseInfo, Controller);
			}
		}

		void LogOkResponse()
		{
			var systemId = Token.Claims.FirstOrDefault(claim => claim.Type == "azp")?.Value ?? string.Empty;
			var userId = "";
			if (RequestInfo is TrustedUserInfo userInfo && !string.IsNullOrWhiteSpace(userInfo?.UserId))
			{
				userId = userInfo.UserId;
			}

			var logText = "success";
			switch (ResponseInfo)
			{
				case AutoLoginResponse autoLoginResponse:
					logText += $",AutoLoginUrl: {autoLoginResponse.AutoLoginUrl}";
					break;

				case OAuthLoginResponse oAuthLoginResponse:
					logText += $",RedirectUrl: {oAuthLoginResponse.RedirectUrl}";
					break;
			}

			Controller.AddInfoLog(logText, ((int)HttpStatusCode.OK), SessionId, userId, routingPath: Controller.RoutingPath, product: Product, systemId: systemId);
		}

		public string Product { get; protected set; }
		public string SessionId { get; protected set; }
		public bool Success { get; set; }
		public TRequest RequestInfo { get; protected set; }
		public TResponse ResponseInfo { get; set; }
		public ErrorMessages Messages { get; set; }
		public TrustedController Controller { get; protected set; }

		public JwtSecurityToken Token { get; protected set; }
	}
}
