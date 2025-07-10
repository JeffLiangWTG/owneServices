using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;

namespace Enterprise.ChangeDataCapture.Service
{
	public class OnlineCdcScannerLogger : CdcScannerLogger
	{
		readonly ILogger cdcLogger;

		public OnlineCdcScannerLogger(ILogger serviceLogger)
		{
			cdcLogger = serviceLogger;
		}

		public override void Log(string logMessage)
		{
			cdcLogger.Log(LogType.Information, logMessage);
		}

		public override void Debug(string errorMesage)
		{
			cdcLogger.Log(LogType.Debug, errorMesage);
		}

		public override void Error(string errorMesage)
		{
			cdcLogger.Log(LogType.Error, errorMesage);
		}

		public override void Warning(string warningMessage)
		{
			cdcLogger.Log(LogType.Warning, warningMessage);
		}
	}
}
