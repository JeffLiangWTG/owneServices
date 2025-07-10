using System;
using Enterprise.Integration;

namespace CargoWise.Main.Startup.SqlSecurity
{
	class SqlSecurityBuildOutputCallbackLogger : ILogger
	{
		public SqlSecurityBuildOutputCallbackLogger(Action<LogType, string, Exception> callback)
		{
			this.callback = callback;
		}

		readonly Action<LogType, string, Exception> callback;

		public void Log(LogType type, string message)
		{
			callback(type, message, null);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			callback(type, message, ex);
		}
	}
}
