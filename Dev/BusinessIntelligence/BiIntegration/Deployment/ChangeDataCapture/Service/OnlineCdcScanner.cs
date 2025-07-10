using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;

namespace Enterprise.ChangeDataCapture.Service
{
	public class OnlineCdcScanner : CdcScanner
	{
		public OnlineCdcScanner(ILogger logger)
		{
			CdcScannerLogger = new OnlineCdcScannerLogger(logger);
		}
	}
}
