using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;
using NLog;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class TrustedContextV1<TRequest, TResponse> : TrustedContext<TRequest, TResponse> where TRequest : TrustedInfo
	{
		TrustedContextV1()
		{
		}

		public static TrustedContextV1<TRequest, TResponse> New(TrustedRequest request, TrustedController controller)
		{
			var sessionId = Guid.NewGuid().ToString();
			TrustedMessagingLogger.LogTrustedRequest(controller.Logger, controller.RoutingPath, sessionId, request);

			var result = new MyAccountTrustedMessenger<TRequest>().ReadMessage(controller.Request, request, controller.Factory);
			var context = new TrustedContextV1<TRequest, TResponse>()
			{
				Product = request.Product,
				SystemId = request.SystemId,
				SessionId = sessionId,
				RequestInfo = result.Info,
				TrustedSystem = result.TrustedSystem,
				Messages = result.ErrorMessages,
				Controller = controller,
			};
			if (result.Info == null || result.ProcessStatus != MyAccountTrustedMessageProcessStatus.Successful)
			{
				context.Success = false;
				TrustedMessagingLogger.LogTrustedMessageDecryptionFalure(controller.Logger, controller.RoutingPath, sessionId, request, result.ErrorMessages);
			}
			else
			{
				context.Success = true;
				TrustedMessagingLogger.LogTrustedMessageDecryptionSuccess(controller.Logger, controller.RoutingPath, sessionId, context);
			}

			return context;
		}

		public override IHttpActionResult CreateHttpActionResult()
		{
			if (ResponseInfo is bool response && response)
			{
				LogOkResponse();
				return new OkResult(Controller);
			}

			if (RequestInfo == null) //Decryption Falure
			{
				return new BadRequestWithErrorMessages(Messages, Controller);
			}

			if (ResponseInfo == null || ResponseInfo is bool)
			{
				TrustedMessagingLogger.LogBadRequestWithErrorMessage(Controller.Logger, Controller.RoutingPath, SessionId, this);
				return new BadRequestWithErrorMessages(Messages, Controller);
			}
			else
			{
				LogOkResponse();
				var jsonMessage = JsonConvert.SerializeObject(ResponseInfo, typeof(TResponse), new JsonSerializerSettings());
				return new EncryptedMessageResponse(jsonMessage, TrustedSystem, Controller);
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

			Controller.Logger?.AddLog(LogLevel.Info, logText, ((int)HttpStatusCode.OK), SessionId, userId, routingPath: Controller.RoutingPath, product: Product, systemId: SystemId, tenantId: RequestInfo.TenantId);
		}
	}
}
