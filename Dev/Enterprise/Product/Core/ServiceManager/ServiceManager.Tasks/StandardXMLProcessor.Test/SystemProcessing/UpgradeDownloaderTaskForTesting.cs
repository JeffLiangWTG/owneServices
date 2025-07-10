using System.Collections.Generic;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class UpgradeDownloaderTaskForTesting : UpgradeDownloaderTask
	{
		public UpgradeDownloaderTaskForTesting(bool[] downloadCompletedResponses, bool[] downloadInProcessResponses)
		{
			_downloadCompletedResponses = new Queue<bool>(downloadCompletedResponses);
			_downloadInProcessResponses = new Queue<bool>(downloadInProcessResponses);
		}

		protected override UpgradeDownloaderMessageProcessor GetDownloader()
		{
			return new UpgradeDownloaderForTesting(_downloadCompletedResponses, _downloadInProcessResponses);
		}

		protected override int SleepMilliseconds { get { return 0; } }

		readonly Queue<bool> _downloadCompletedResponses;
		readonly Queue<bool> _downloadInProcessResponses;
	}
}
