using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Types;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using NLog;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class TrustedContextV2<TRequest, TResponse> : TrustedContext<TRequest, TResponse> where TRequest : TrustedInfo
	{
		TrustedContextV2()
		{
		}

		public static TrustedContextV2<TRequest, TResponse> New(TRequest request, TrustedController controller)
		{
			var sessionId = Guid.NewGuid().ToString();
			TrustedMessagingLogger.LogTrustedRequest(controller.Logger, controller.RoutingPath, sessionId, request);
			var context = new TrustedContextV2<TRequest, TResponse>()
			{
				Success = false,
				Product = request.Product,
				SystemId = request.SystemId,
				SessionId = sessionId,
				RequestInfo = request,
				Messages = null,
				Controller = controller,
			};

			var requestCertificateThumbprint = string.Empty;
			var requestCertificateSerialNumber = string.Empty;
			var requestCertificate = controller.RequestContext.ClientCertificate;
			if (requestCertificate == null)
			{
				requestCertificateThumbprint = GetHeader(controller.Request, HttpHeaderClientCertificateSHA1);
				requestCertificateSerialNumber = GetHeader(controller.Request, HttpHeaderClientCertificateSerial);

				if (string.IsNullOrWhiteSpace(requestCertificateThumbprint) || string.IsNullOrWhiteSpace(requestCertificateSerialNumber))
				{
					context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_MissingClientCertificate, ErrorCodes.Descriptions.Validation_MissingClientCertificate);
					return context;
				}
			}
			else
			{
				requestCertificateThumbprint = requestCertificate.Thumbprint;
				requestCertificateSerialNumber = requestCertificate.SerialNumber;
			}

			var timestampThreshold = ZDateTime.UtcNow.AddHours(-24);
			if (request.InfoTimestamp == DateTime.MinValue || request.InfoTimestamp < timestampThreshold)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Critical_InfoExpired, ErrorCodes.Descriptions.Critical_InfoExpired);
				return context;
			}

			if (string.IsNullOrWhiteSpace(request.Product) || string.IsNullOrWhiteSpace(request.SystemId))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidSystem, ErrorCodes.Descriptions.Validation_InvalidSystem);
				return context;
			}

			context.TrustedSystem = EdiTrustedSystem.Load(controller.Factory, request.Product, request.SystemId);
			if (context.TrustedSystem == null)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidSystem, ErrorCodes.Descriptions.Validation_InvalidSystem);
				return context;
			}

			var clientCertificate = context.TrustedSystem.CertificateConfig?.GetCertificate();
			if (clientCertificate == null || clientCertificate.Thumbprint != requestCertificateThumbprint || clientCertificate.SerialNumber != requestCertificateSerialNumber)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Critical_CertificateMismatched, ErrorCodes.Descriptions.Critical_CertificateMismatched);
				return context;
			}

			context.Success = true;
			return context;
		}

		public override IHttpActionResult CreateHttpActionResult()
		{
			if (ResponseInfo is bool response && response)
			{
				LogOkResponse();
				return new OkResult(Controller);
			}

			if (ResponseInfo == null || ResponseInfo is bool)
			{
				TrustedMessagingLogger.LogBadRequestWithErrorMessage(Controller.Logger, Controller.RoutingPath, SessionId, this);
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

			Controller.Logger?.AddLog(LogLevel.Info, logText, ((int)HttpStatusCode.OK), SessionId, userId: userId, routingPath: Controller.RoutingPath, product: Product, systemId: SystemId, tenantId: RequestInfo.TenantId);
		}

		static string GetHeader(HttpRequestMessage httpRequestMessage, string name)
			=> httpRequestMessage.Headers.TryGetValues(name, out var values) && values != null && values.Any() ? values.First() : null;

		const string HttpHeaderClientCertificateSHA1 = "X-SSL-Client-SHA1";
		const string HttpHeaderClientCertificateSerial = "X-SSL-Client-Serial";
	}
}
