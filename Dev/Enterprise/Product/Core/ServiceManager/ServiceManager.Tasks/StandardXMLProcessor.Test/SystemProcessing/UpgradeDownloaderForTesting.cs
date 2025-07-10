using System.Collections.Generic;
using Enterprise.MailManager.FileDownload;
using Enterprise.MasterFiles.Business.VersionInfo;
using Enterprise.Messaging.Business;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class UpgradeDownloaderForTesting : UpgradeDownloaderMessageProcessor
	{
		public UpgradeDownloaderForTesting(Queue<bool> downloadCompletedResponses, Queue<bool> downloadInProcessResponses)
			: base(null)
		{
			_downloadCompletedResponses = downloadCompletedResponses;
			_downloadInProcessResponses = downloadInProcessResponses;
		}

		protected override WebFileDownloader CreateNewDownloader(string sourceUrl)
		{
			var result = base.CreateNewDownloader(sourceUrl);

			_downloadingFileName = sourceUrl.Substring(sourceUrl.LastIndexOf('/') + 1);

			return result;
		}

		protected override void StartDownload()
		{
		}

		protected override string DownloadingFileName
		{
			get { return _downloadingFileName; }
		}

		protected override string DownloadedFilePath
		{
			get { return _downloadingFileName; }
		}

		public override void CancelDownload()
		{
			DisposeDownloader();
		}

		protected override void DisposeDownloader()
		{
			if (UpgradeDownloader != null)
			{
				UpgradeDownloader.Dispose();
				UpgradeDownloader = null;
			}
			_downloadingFileName = "";
		}

		protected override string ImportDownloadedPackage(string packagePath, PackageVersionInfo packageVersionInfo = null)
		{
			return EDIMessage.Status.ProcessedOK;
		}

		protected override bool UpgradeDownloadCompleted
		{
			get { return _downloadCompletedResponses.Dequeue(); }
		}

		protected override bool UpgradeDownloadInProcess
		{
			get { return _downloadInProcessResponses.Dequeue(); }
		}

		readonly Queue<bool> _downloadCompletedResponses;
		readonly Queue<bool> _downloadInProcessResponses;

		string _downloadingFileName = "";
	}
}
