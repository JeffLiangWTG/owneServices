using System;
using System.Globalization;
using CargoWise.Licensing;
using NLog;

namespace CargoWise.ProductRegistration.Service
{
	public class NLogWrapper
	{
		public NLogWrapper()
		{
			logger = LogManager.GetLogger(nameof(RegistrationController));
		}
		readonly Logger logger;

		public void LogInfo(RegistrationKey regKey, string productVersion, DatabaseUniqueKey existingDbKey, string message, int status)
		{
			var dbUniqueKey = regKey?.DbUniqueKey;
			var logEventInfo = new LogEventInfo
			{
				Level = LogLevel.Info,
				Properties =
				{
					["database_number"] = regKey?.DatabaseNumber,
					["product_version"] = productVersion,
					["server_name"] = dbUniqueKey?.ServerName,
					["database_name"] = dbUniqueKey?.DatabaseName,
					["database_create_time"] = dbUniqueKey?.DatabaseCreated,
					["group_id"] = dbUniqueKey?.GroupId,
					["connection_server"] = dbUniqueKey?.ConnectionServerName,
					["status"] = status,
					["registered_database_name"] = existingDbKey?.DatabaseName,
					["registered_server_name"] = existingDbKey?.ServerName,
					["registered_group_id"] = existingDbKey?.GroupId,
					["registered_database_create_time"] = existingDbKey?.DatabaseCreated,
					["registered_connection_server_name"] = existingDbKey?.ConnectionServerName,
				},
				Message = message,
				LoggerName = logger.Name,
			};

			logger.Log(logEventInfo);
		}

		public void LogException(string message, int status, Exception ex)
		{
			var logEventInfo = new LogEventInfo
			{
				Level = LogLevel.Error,
				Properties =
				{
					["status"] = status,
				},
				Message = FormatMessage(message, ex),
				LoggerName = logger.Name,
			};

			if (ex != null)
			{
				logEventInfo.Exception = ex;
			}

			logger.Log(logEventInfo);
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
