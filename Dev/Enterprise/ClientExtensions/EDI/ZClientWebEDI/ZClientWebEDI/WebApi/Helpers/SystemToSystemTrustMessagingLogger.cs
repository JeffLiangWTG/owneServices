using System.Linq;
using System.Net;
using NLog;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class SystemToSystemTrustMessagingLogger
	{
		public static void LogBadRequestWithErrorMessage<TRequest, TResponse>(NLogWrapper logger, string path, TrustedContextV3<TRequest, TResponse> context) where TRequest : SystemToSystemTrustedInfo
		{
			if (context != null)
			{
				var systemId = "";
				if (context.Token != null)
				{
					var azp = context.Token.Claims.FirstOrDefault(claim => claim.Type == "azp")?.Value;
					if (!string.IsNullOrWhiteSpace(azp))
					{
						systemId = azp;
					}
				}
				var warn = context.Messages.GetSingleLineMessages().TrimEnd();
				logger.AddLog(LogLevel.Warn, warn, ((int)HttpStatusCode.BadRequest), context.SessionId, routingPath: path, product: context.Product, systemId: systemId);
			}
		}
	}
}
