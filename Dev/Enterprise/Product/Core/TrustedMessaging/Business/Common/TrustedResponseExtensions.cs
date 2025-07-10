using System.Linq;
using Enterprise.Integration;
using WTG.TrustedMessaging.Models;

namespace Enterprise.TrustedMessaging.Business
{
	public static class TrustedResponseExtensions
	{
		public static void Log<TResponse>(this TrustedResponse<TResponse> response, ILogger logger)
		{
			if (response != null && logger != null && response.Messages != null && response.Messages.Any())
			{
				var logType = response.Messages.Any(x => x.Code == WTG.TrustedMessaging.Constants.ErrorCodes.SecretKeyNotUpToDate)
					? LogType.Warning
					: LogType.Error;
				logger.Log(logType, string.Join(System.Environment.NewLine, response.Messages.Select(x => $"{x.Code} : {x.Message}")));
			}
		}
	}
}
