using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public static class NctsMessageGeneratorHelper
	{
		public static void AddPermitRecords(NctsHeader nctsHeader, EDIMessage message)
		{
			var nCTSCusPermitCusDecProcessor = new NCTSCusPermitCusDecProcessor(nctsHeader, message);
			try
			{
				nCTSCusPermitCusDecProcessor.AddPermitRecordsAndLockMutexIfNeeded();
				nCTSCusPermitCusDecProcessor.AddPermitTransactions(message, (EDIMessage msg) => NctsPermitHelper.GetPermitAppIdForMessage(message));
			}
			finally
			{
				nCTSCusPermitCusDecProcessor?.UnlockPermitMutexes();
			}
		}
	}
}
