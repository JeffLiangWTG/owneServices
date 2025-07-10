using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public static class NctsMessageGeneratorHelper
	{
		public static void AddPermitRecords(NctsHeader nctsHeader, EDIMessage message)
		{
			var permitProcessor = new NCTSCusPermitCusDecProcessor(nctsHeader, message);
			try
			{
				permitProcessor.AddPermitRecordsAndLockMutexIfNeeded();
				permitProcessor.AddPermitTransactions(message, (msg) => NctsPermitHelper.GetPermitAppIdForMessage(message));
			}
			finally
			{
				permitProcessor?.UnlockPermitMutexes();
			}
		}
	}
}
