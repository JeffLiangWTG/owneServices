using System.IO;
using System.Net;
using System.Threading;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = MailManager.Res;

namespace Enterprise.MailManager.FileDownload
{
	public class WebFileDownloader : WebFileDownloaderBase
	{
		public WebFileDownloader(string url) : base(url)
		{
		}

		public void StartFileDownload(string targetFolder)
		{
			fTargetFolder = targetFolder;
			ClearDataDownloader();
			StartAsyncRequest();
		}

		public long TotalDownloaded
		{
			get { return (fData != null) ? fData.TotalDownloaded : 0; }
		}

		public string TargetFilePath
		{
			get { return Path.Combine(fTargetFolder, FileName); }
		}

		public string TempFilePath
		{
			get { return TargetFilePath + TempFileExtension; }
		}

		public bool TargetFileAlreadyExists
		{
			get { return fFileAlreadyExists; }
		}

		public bool HasDownloadCompleted
		{
			get { return (fData != null) ? fData.Completed : TargetFileAlreadyExists; }
		}

		public const string TempFileExtension = ".part";

		#region implementation

		protected override WebRequest GetRequest()
		{
			WebRequest webRequest = fIsResuming ? ProtocolSupport.GetWebRequestForDownload((int)fStartPosition) : ProtocolSupport.GetWebRequestForDownload();

			if (Globals.IsTest)
			{
				webRequest.Proxy = new WebProxy() { BypassProxyOnLocal = true };
			}

			return webRequest;
		}

#if DEBUG
		internal bool ResponseHasValidStatusExposed(WebResponse response)
		{
			return ResponseHasValidStatus(response);
		}
#endif

		protected override bool ResponseHasValidStatus(WebResponse response)
		{
			WebProtocolSupport.RequestMode mode = fIsResuming ? WebProtocolSupport.RequestMode.ResumeDownload : WebProtocolSupport.RequestMode.StartDownload;
			var result = ProtocolSupport.ResponseHasValidStatus(response, mode);
			if (!result)
			{
				string message = Res.GetString("26AA6C84-9984-42CF-9B90-90192617C567", "Response has invalid status. Protocol Type: {0} Mode: {1}", ProtocolSupport.GetType().Name, mode);
				if (response != null && response.ResponseUri != null)
				{
					message += string.Format(Culture.Current, (NoResString)" Uri:{0}", response.ResponseUri.AbsoluteUri);
				}
				SetStatusMessageIfHasntBeenSet(message);
			}
			return result;
		}

		protected override void ProcessWebResponse(WebResponse response)
		{
			if (File.Exists(TargetFilePath))
			{
				fFileAlreadyExists = true;
				SetStatusMessageIfHasntBeenSet(Res.GetString("8D043DBD-12B2-495D-96CE-053F1D113089", "File already exists : {0}", TargetFilePath));
				NotifyWhenFinished();
				return;
			}
			else if (fNeedSecondRequestToResume)
			{
				CloseResponse(response);

				fNeedSecondRequestToResume = false;
				fIsResuming = true;

				StartAsyncRequest();
			}
			else
			{
				StartDataDownload(response);

				fIsResuming = false;
			}
		}

		protected override void GetFileInfoFromResponse(WebResponse response)
		{
			if (!fIsResuming)
			{
				base.GetFileInfoFromResponse(response);
			}

			if (!fIsResuming && File.Exists(TempFilePath))
			{
				if (FileSizeKnown && ProtocolSupport.IsResumeSupported)
				{
					fStartPosition = new FileInfo(TempFilePath).Length;
					fNeedSecondRequestToResume = true;
				}
				else
				{
					RemoveIncompleteFile();
				}
			}
			else if (!Directory.Exists(fTargetFolder))
			{
				Directory.CreateDirectory(fTargetFolder);
			}
		}

		void RemoveIncompleteFile()
		{
			if (File.Exists(TempFilePath))
			{
				File.Delete(TempFilePath);
			}
		}

#if DEBUG
		protected virtual
#endif
		void StartDataDownload(WebResponse response)
		{
			if (HasUserCancelled || !ResponseHasValidStatus(response))
			{
				NotifyWhenFinished();
				return;
			}

			fData = GetDownloadData(response);
			fDataDownloader = new Thread(new ThreadStart(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					fData?.DoDownload();
					if (fData != null && fData.HasException)
					{
						SetStatusMessageIfHasntBeenSet(fData.CaughtException.Message);
					}
				}
			}));
			fDataDownloader.Start();
		}

		protected virtual DownloadData GetDownloadData(WebResponse response)
		{
			return new DownloadData(response, TempFilePath, FileSize, fStartPosition, CancelEvent, FinishedEvent);
		}

		void ClearDataDownloader()
		{
			if (fData != null)
			{
				fData = null;
			}
		}

		string fTargetFolder;
		bool fFileAlreadyExists;
		bool fNeedSecondRequestToResume;
		bool fIsResuming;
		long fStartPosition;

#if DEBUG
		protected
#endif
		Thread fDataDownloader;
		DownloadData fData;
		public DownloadData DownloadData
		{
			get { return fData; }
		}

		#endregion
	}
}
