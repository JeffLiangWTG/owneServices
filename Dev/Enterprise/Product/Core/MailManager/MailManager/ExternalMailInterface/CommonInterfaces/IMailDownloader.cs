using System;
using Res = MailManager.Res;

namespace Enterprise.MailManager.ExternalMailInterface.CommonInterfaces
{
	public interface IMailDownloader : IDisposable
	{
		event EmailDownloadedHandler EmailDownloaded;
		event LogMessageHandler LogMessage;
		event DownloaderClosingHandler DownloaderClosing;

		void DownloadFromServer();
		void DeleteMessage(string messageId);
	}

	public static class MailDownloaderLogBuilder
	{
		public static string BuildInsufficientMemoryLogWithUniqueId(string uniqueId) => $"Insufficient memory to process email with unique id: {uniqueId ?? string.Empty}";

		public static string BuildOutOfMemoryLogWithUniqueId(string uniqueId) => $"Out of memory error trying to process email with unique id: {uniqueId ?? string.Empty}";

		public static string BuildDownloadingLog(long messageCount, string mailServer = null)
		{
			return string.IsNullOrEmpty(mailServer)
				? Res.GetString("C268195D-3BB0-46AB-AE6E-B2D63A4D8D81", "Downloading {0} {1} with {2}", messageCount, GetSingularOrPluralOfEmail(messageCount), "Graph API")
				: Res.GetString("2BBCED74-AD70-4383-95CF-2B0B1750DD7C", "Downloading {0} {1} from mail server at {2}", messageCount, GetSingularOrPluralOfEmail(messageCount), mailServer);
		}

		static string GetSingularOrPluralOfEmail(long count)
		{
			return count == 1
				? Res.GetString("8AFD8B4C-95E9-4C5D-87E3-3678B66540EB", "email")
				: Res.GetString("8E27E432-DC08-47AF-9CD6-7D97F2438813", "emails");
		}

		public static string BuildDownloadedLog(long messageCount) => Res.GetString("df1462fe-db03-4d37-96fb-eea7f2d50a24", "{0} {1} downloaded in this batch", messageCount, GetSingularOrPluralOfEmail(messageCount));

		public static string BuildDownloadingErrorLogWithUniqueId(string uniqueId, Exception ex) => $"Error downloading or processing Email with unique id: {uniqueId ?? string.Empty}\r\n\r\n{ex}";

		const int MaximumNumberOfMailItemsToDownloadInABatch = 1000;
		const double MailDownloadRateForOneBatch = 0.2;
		const int MaxAmountNeedMailDownloadRateForOneBatch = 5000;

		public static int CalculateTheAmountOfMailItemsShouldBeLoggedAfterDownload(int totalEmailsWaitForDownloading)
		{
			if (totalEmailsWaitForDownloading > MaxAmountNeedMailDownloadRateForOneBatch)
			{
				return MaximumNumberOfMailItemsToDownloadInABatch;
			}

			var amountInOneBatch = totalEmailsWaitForDownloading * MailDownloadRateForOneBatch;

			if ((int)amountInOneBatch <= 0)
			{
				return 1;
			}
			return (int)amountInOneBatch;
		}
	}
}
