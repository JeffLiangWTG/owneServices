using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.Integration;

namespace Enterprise.DbUpgrader.Startup
{
	internal class UpgradeManagerLoggerProxy : ILogger
	{
		readonly IUpgradeTaskWorkflowLogger inner;
		readonly Func<Exception, IUpgradeTaskWorkflowLogger, bool> exceptionHandler;

		public UpgradeManagerLoggerProxy(IUpgradeTaskWorkflowLogger inner, Func<Exception, IUpgradeTaskWorkflowLogger, bool> exceptionHandler)
		{
			this.inner = inner;
			this.exceptionHandler = exceptionHandler;
		}

		public void Log(LogType type, string message)
		{
			inner.ShowInfoMessage(message);

			if (type == LogType.Error)
			{
				ErrorReporter.ReportOnce(message);
			}
		}

		public void Log(LogType type, string message, Exception ex)
		{
			inner.ShowInfoMessage(message + System.Environment.NewLine + ex.ToString());

			if (type == LogType.Error)
			{
				if (!exceptionHandler.Invoke(ex, inner))
				{
					ErrorReporter.ReportOnce(message, new UpgradeManagerException(ex));
				}
			}
		}
	}
}
