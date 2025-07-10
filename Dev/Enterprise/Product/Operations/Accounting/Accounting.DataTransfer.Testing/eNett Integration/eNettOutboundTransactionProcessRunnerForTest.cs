using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.BatchProcessor.Accounting.Testing
{
	public class eNettOutboundTransactionProcessRunnerForTest : IENettOutboundTransactionProcessRunnerForTest
	{
		public void ProcessLogs()
		{
			try
			{
				new eNettOutboundTransactionSubscriberTest().ProcessLogs();
			}
			finally
			{
				MockENettWebService.ClearInstance();
			}
		}
	}
}
