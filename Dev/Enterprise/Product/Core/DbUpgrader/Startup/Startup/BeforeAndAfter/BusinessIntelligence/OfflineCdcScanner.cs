using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.ChangeDataCapture.Common;

namespace Enterprise.DbUpgrader.Startup
{
	public class OfflineCdcScanner : CdcScanner
	{
		public OfflineCdcScanner(IUpgradeTaskWorkflowLogger logger)
		{
			CdcScannerLogger = new OfflineCdcScannerLogger(logger);
		}
		protected override DbConnection DedicatedNonPooledCdcConnection
		{
			get
			{
				return (dedicatedNonPooledCDCConnection = dedicatedNonPooledCDCConnection ?? Db.NewAdminConnection());
			}
		}
		AdminConnection dedicatedNonPooledCDCConnection;
	}
}
