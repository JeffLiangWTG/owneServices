using System.Net;
using NLog;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class TrustedMessagingLogger
	{
		public static void LogTrustedRequest(NLogWrapper logger, string path, string sessionId, TrustedRequest trustedRequest)
		{
			logger?.AddLog(LogLevel.Info, "Request received", ((int)HttpStatusCode.OK), sessionId, routingPath: path, product: trustedRequest.Product, systemId: trustedRequest.SystemId);
		}

		public static void LogTrustedRequest(NLogWrapper logger, string path, string sessionId, TrustedInfo info)
		{
			logger?.AddLog(LogLevel.Info, "Request received", ((int)HttpStatusCode.OK), sessionId, routingPath: path, product: info.Product, systemId: info.SystemId, tenantId: info.TenantId);
		}

		public static void LogBadRequestWithErrorMessage(NLogWrapper logger, string path, string sessionId, TrustedRequest trustedRequest, TrustedUserInfo info, ErrorMessages errorMessages)
		{
			var errorMsg = errorMessages.GetSingleLineMessages().TrimEnd();
			logger?.AddLog(LogLevel.Warn, errorMsg, ((int)HttpStatusCode.BadRequest), sessionId, info.UserId, path, trustedRequest.Product, trustedRequest.SystemId, info.TenantId);
		}

		public static void LogBadRequestWithErrorMessage(NLogWrapper logger, string path, string sessionId, TrustedRequest trustedRequest, TrustedInfo info, ErrorMessages errorMessages)
		{
			var warnMsg = errorMessages.GetSingleLineMessages().TrimEnd();
			logger?.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: path, product: trustedRequest.Product, systemId: trustedRequest.SystemId, tenantId: info.TenantId);
		}

		public static void LogBadRequestWithErrorMessage<TRequest, TResponse>(NLogWrapper logger, string path, string sessionId, TrustedContext<TRequest, TResponse> context) where TRequest : TrustedInfo
		{
			if (context != null)
			{
				var userId = "";
				var tenantId = "";
				if (context.RequestInfo != null)
				{
					if (!string.IsNullOrWhiteSpace(context.RequestInfo.TenantId))
					{
						tenantId = context.RequestInfo.TenantId;
					}
					if (context.RequestInfo is TrustedUserInfo userInfo && !string.IsNullOrWhiteSpace(userInfo?.UserId))
					{
						userId = userInfo.UserId;
					}
				}
				var warnMsg = context.Messages.GetSingleLineMessages().TrimEnd();
				logger?.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), sessionId, userId, path, context.Product, context.SystemId, tenantId);
			}
		}

		public static void LogBadRequestWithErrorMessage(NLogWrapper logger, string path, string sessionId, string clientId, ErrorMessages errorMessages)
		{
			var warnMsg = $"{clientId} | {errorMessages.GetSingleLineMessages()}".TrimEnd();
			logger?.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: path);
		}

		public static void LogTrustedMessageDecryptionSuccess(NLogWrapper logger, string path, string sessionId, TrustedRequest trustedRequest, TrustedUserInfo info)
		{
			logger?.AddLog(LogLevel.Info, "Trusted message decrypted", ((int)HttpStatusCode.OK), sessionId, info.UserId, path, trustedRequest.Product, trustedRequest.SystemId, info.TenantId);
		}

		public static void LogTrustedMessageDecryptionSuccess(NLogWrapper logger, string path, string sessionId, TrustedRequest trustedRequest, TrustedInfo info)
		{
			logger?.AddLog(LogLevel.Info, "Trusted message decrypted", ((int)HttpStatusCode.OK), sessionId, routingPath: path, product: trustedRequest.Product, systemId: trustedRequest.SystemId, tenantId: info.TenantId);
		}

		public static void LogTrustedMessageDecryptionSuccess<TRequest, TResponse>(NLogWrapper logger, string path, string sessionId, TrustedContext<TRequest, TResponse> context) where TRequest : TrustedInfo
		{
			var userId = "";
			var tenantId = "";

			if (context != null)
			{
				if (context.RequestInfo is TrustedUserInfo userInfo)
				{
					userId = userInfo.UserId;
					tenantId = userInfo.TenantId;
				}
				else
				{
					tenantId = context.RequestInfo?.TenantId ?? tenantId;
				}
			}

			logger?.AddLog(LogLevel.Info, "Trusted message decrypted", ((int)HttpStatusCode.OK), sessionId, userId, path, context.Product, context.SystemId, tenantId);
		}

		public static void LogTrustedMessageDecryptionFalure(NLogWrapper logger, string path, string sessionId, TrustedRequest trustedRequest, ErrorMessages errorMessages)
		{
			logger?.AddLog(LogLevel.Warn, "Trusted message failed to decrypt", ((int)HttpStatusCode.BadRequest), sessionId, routingPath: path, product: trustedRequest.Product, systemId: trustedRequest.SystemId);
			var warnMsg = errorMessages.GetSingleLineMessages().TrimEnd();
			logger?.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: path, product: trustedRequest.Product, systemId: trustedRequest.SystemId);
		}

		public static void LogOkResponse(NLogWrapper logger, string path, string sessionId, TrustedRequest trustedRequest, TrustedUserInfo info, string message = "")
		{
			var okMessage = !string.IsNullOrEmpty(message) ? $"{message}" : "success";
			logger?.AddLog(LogLevel.Info, okMessage, ((int)HttpStatusCode.OK), sessionId, info.UserId, path, trustedRequest.Product, trustedRequest.SystemId, tenantId: info.TenantId);
		}

		public static void LogOkResponse(NLogWrapper logger, string path, string sessionId, TrustedRequest trustedRequest, TrustedInfo info, string message = "")
		{
			var okMessage = !string.IsNullOrEmpty(message) ? $"{message}" : "success";
			logger?.AddLog(LogLevel.Info, okMessage, ((int)HttpStatusCode.OK), sessionId, routingPath: path, product: trustedRequest.Product, systemId: trustedRequest.SystemId, tenantId: info.TenantId);
		}
	}
}
