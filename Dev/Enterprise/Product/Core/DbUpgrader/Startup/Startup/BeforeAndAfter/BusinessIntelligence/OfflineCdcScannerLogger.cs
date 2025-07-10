using CargoWise.DbUpgrader.Foundation;
using Enterprise.ChangeDataCapture.Common;

namespace Enterprise.DbUpgrader.Startup
{
	public class OfflineCdcScannerLogger : CdcScannerLogger
	{
		readonly IUpgradeTaskWorkflowLogger cdcLogger;

		public OfflineCdcScannerLogger(IUpgradeTaskWorkflowLogger serviceLogger)
		{
			cdcLogger = serviceLogger;
		}

		public override void Log(string logMessage)
		{
			cdcLogger.ShowInfoMessage(logMessage);
		}

		public override void Debug(string logMessage)
		{
			cdcLogger.ShowInfoMessage(logMessage);
		}

		public override void Error(string logMessage)
		{
			cdcLogger.ShowTaskError(logMessage);
		}

		public override void Warning(string logMessage)
		{
			cdcLogger.ShowTaskError(logMessage);
		}
	}
}
