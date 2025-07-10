using System.Collections.Generic;

namespace Enterprise.ChangeDataCapture.Common.Testing
{
	public class CdcScannerLoggerForTest : CdcScannerLogger
	{
		public CdcScannerLoggerForTest()
		{
			Logs = new List<string>();
		}

		public List<string> Logs;

		public override void Log(string logMessage)
		{
			Logs.Add(logMessage);
		}

		public override void Debug(string logMessage)
		{
			Logs.Add(logMessage);
		}

		public override void Error(string logMessage)
		{
			Logs.Add(logMessage);
		}

		public override void Warning(string logMessage)
		{
			Logs.Add(logMessage);
		}
	}
}
