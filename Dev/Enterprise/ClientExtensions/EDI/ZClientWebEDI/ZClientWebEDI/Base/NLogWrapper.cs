using System;
using System.Globalization;
using NLog;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class NLogWrapper
	{
		public static class Status
		{
			public const string Ok = "200 Ok";
			public const string Found = "302 Found";
			public const string BadRequest = "400 BadRequest";
			public const string Forbidden = "403 Forbidden";
			public const string NotFound = "404 NotFound";
			public const string InternalError = "500 InternalError";
		}

		public NLogWrapper(Type type)
		{
			nLogger = LogManager.GetLogger(type.Name);
		}

		readonly Logger nLogger;

		public virtual void AddLog(LogLevel logLevel, string message, int statusCode, string sessionId = "", string userId = "", string routingPath = "", string product = "", string systemId = "", string tenantId = "", Exception ex = null)
		{
			var logEventInfo = new LogEventInfo
			{
				Level = logLevel,
				Properties =
				{
					["status_code"] = statusCode,
					["session_id"] = sessionId,
					["user_id"] = userId,
					["routing_path"] = routingPath,
					["product"] = product,
					["system_id"] = systemId,
					["tenant_id"] = tenantId,
				},
				Message = FormatMessage(message, ex),
				LoggerName = nLogger.Name,
			};

			if (ex != null)
			{
				logEventInfo.Exception = ex;
			}

			nLogger.Log(logEventInfo);
		}

		static string FormatMessage(string message, Exception ex)
		{
			if (ex != null)
			{
				message = string.Format(CultureInfo.InvariantCulture, "{0}\\n{1}", message, ex.ToString());
			}
			return message.Trim().Replace("\r", "").Replace("\n", "\\n").Replace("\t", "\\t");
		}
	}
}
