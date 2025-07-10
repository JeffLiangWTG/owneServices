using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.BufferManagement.Service
{
	//Possible became the PaveServiceLogger
	public class SchematicServiceLogger : ILogger
	{
		public IList<string> Logs { get; private set; } = new List<string>();
		readonly ILogger logger;

		public SchematicServiceLogger(string programCode, string suffix)
		{
			var loggerFactory = ObjectFactory.Get<ILoggerFactory>();
			logger = loggerFactory?.NewServiceTaskLogger(Db.ServerName, Db.DatabaseName, programCode, suffix);
		}

		public void Log(LogType type, string message)
		{
			AddLog(type, message);
			logger?.Log(type, message);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			AddLog(type, message, ex);
			logger?.Log(type, message, ex);
		}

		void AddLog(LogType type, string message, Exception ex = null)
		{
			var log = FormattableString.Invariant($"{nameof(SchematicService)}|{type}: {message}"); // System Notification

			if (ex != null)
			{
				log += FormattableString.Invariant($" {nameof(Exception)}: {ex.Message}"); // System Notification
			}

			Logs.Add(log);
		}
	}
}
