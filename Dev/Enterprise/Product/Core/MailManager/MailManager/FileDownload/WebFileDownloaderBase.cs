using System;
using System.Net;
using System.Threading;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Res = MailManager.Res;

namespace Enterprise.MailManager.FileDownload
{
	public abstract class WebFileDownloaderBase : Disposable
	{
		public WebFileDownloaderBase(string url)
		{
			WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;

			fUrl = url;
			fCancelEvent = new ManualResetEvent(false);
			fFinishedEvent = new ManualResetEvent(false);
		}

		public void Cancel()
		{
			SetStatusMessageIfHasntBeenSet(Res.GetString("B39AC85B-1CAD-4A06-A55F-39AD2B64A0D6", "Process canceled by user."));
			SetEventSafely(fCancelEvent);
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (WaitingForResponse)
				{
					fRequest.Abort();
					fAsyncGettingResponse = null;
				}

				if (fCancelEvent != null)
				{
					fCancelEvent.Close();
					fCancelEvent = null;
				}

				if (fFinishedEvent != null)
				{
					// Give some time to shut down
					fFinishedEvent.WaitOne(1000, false);

					fFinishedEvent.Close();
					fFinishedEvent = null;
				}

				CloseResponse(fResponse);
			}
		}

		public string Url
		{
			get { return fUrl; }
		}

		public string FileName
		{
			get { return fFileName; }
		}

		public long FileSize
		{
			get { return fFileSize; }
		}

		public DateTime FileModifiedDateTime
		{
			get { return fFileModifiedDateTime; }
		}

		public bool HasFinished
		{
			get { return CheckEventSafely(fFinishedEvent); }
		}

		public WaitHandle FinishedWaitHandle
		{
			get { return FinishedEvent; }
		}

		public string StatusMessage
		{
			get { return fStatusMessage; }
		}

		public IWebProxy Proxy { get; set; }

		#region Implementation

		protected WebProtocolSupport ProtocolSupport
		{
			get
			{
				if (fProtocolSupport == null)
				{
					fProtocolSupport = GetProtocolSupport();
				}
				return fProtocolSupport;
			}
		}

		WebProtocolSupport fProtocolSupport;

		WebProtocolSupport GetProtocolSupport()
		{
			if (fUrl.ToLower().StartsWith((NoResString)"http"))
			{
				return new HttpProtocolSupport(fUrl, Proxy);
			}

			if (fUrl.ToLower().StartsWith((NoResString)"ftp"))
			{
				return new FtpProtocolSupport(fUrl);
			}

			return null;
		}

		protected virtual void StartAsyncRequest()
		{
			if (fFinishedEvent.WaitOne(0, false))
			{
				fFinishedEvent.Reset();
			}

			fRequest = GetRequest();

			if (fRequest != null)
			{
				fFileName = ProtocolSupport.GetFileNameFromUri(fRequest.RequestUri);
				RequestState state = new RequestState(this);
				fAsyncGettingResponse = fRequest.BeginGetResponse(new AsyncCallback(ObtainAsyncResponse), state);

				ThreadPool.RegisterWaitForSingleObject(
					fAsyncGettingResponse.AsyncWaitHandle,
					new WaitOrTimerCallback(ResponseTimeoutCallback),
					state,
					fRequest.Timeout,
					true);
			}
		}

		protected abstract WebRequest GetRequest();

		void ObtainAsyncResponse(IAsyncResult result)
		{
			if (result == fAsyncGettingResponse)
			{
				fAsyncGettingResponse = null;
			}

			if (HasUserCancelled)
			{
				NotifyWhenFinished();
				return;
			}

			fResponse = GetResponseFromAsyncResult(result);

			if (HasUserCancelled || fResponse == null || !ResponseHasValidStatus(fResponse))
			{
				NotifyWhenFinished();
				return;
			}
			else
			{
				try
				{
					GetFileInfoFromResponse(fResponse);

					ProcessWebResponse(fResponse);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					fStatusMessage = ex.Message;
					NotifyWhenFinished();
					return;
				}
			}
		}

		protected abstract bool ResponseHasValidStatus(WebResponse response);

		protected abstract void ProcessWebResponse(WebResponse response);

		protected
#if DEBUG
			virtual
#endif
		WebResponse GetResponseFromAsyncResult(IAsyncResult requestResult)
		{
			WebResponse result = null;
			RequestState state = (RequestState)requestResult.AsyncState;
			var requestFromResult = state.Request;

			try
			{
				result = requestFromResult.EndGetResponse(requestResult);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				fStatusMessage = ex.Message;
			}

			return result;
		}

		protected virtual void GetFileInfoFromResponse(WebResponse response)
		{
			fFileName = ProtocolSupport.GetFileName(response);
			fFileSize = ProtocolSupport.GetFileSize(response);
			fFileModifiedDateTime = ProtocolSupport.GetFileLastModified(response);
		}

		protected void CloseResponse(WebResponse response)
		{
			if (response != null)
			{
				response.Close();
				if (response == fResponse)
				{
					fResponse = null;
				}
			}
		}

		protected void NotifyWhenFinished()
		{
			if (WaitingForResponse)
			{
				fRequest.Abort();
				fAsyncGettingResponse = null;
			}

			SetEventSafely(fFinishedEvent);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be used as a constant")]
		protected static void ResponseTimeoutCallback(object state, bool timedOut)
		{
			RequestState reqState = (RequestState)state;

			try
			{
				if (timedOut ||
					(reqState != null && reqState.Downloader != null && reqState.Downloader.HasUserCancelled))
				{
					if (reqState != null)
					{
						reqState.Request.Abort();
						if (reqState.Downloader != null)
						{
							reqState.Downloader.fStatusMessage = "Request Aborted";
							reqState.Downloader.NotifyWhenFinished();
						}
					}
				}

				if (reqState.WaitHandle != null)
				{
					reqState.WaitHandle.Unregister(null);
					if (reqState.Downloader != null)
					{
						reqState.Downloader.fAsyncGettingResponse = null;
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
		}

		protected void SetEventSafely(ManualResetEvent @event)
		{
			if (@event != null && !@event.SafeWaitHandle.IsClosed)
			{
				@event.Set();
			}
		}

		protected bool CheckEventSafely(ManualResetEvent @event)
		{
			try
			{
				return (@event == null || @event.SafeWaitHandle.IsClosed || @event.WaitOne(0, false));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return true;
			}
		}

		protected bool WaitingForResponse
		{
			get { return fAsyncGettingResponse != null && fRequest != null; }
		}

		protected bool FileSizeKnown
		{
			get { return (fFileSize != -1); }
		}

		internal bool HasUserCancelled
		{
			get { return CheckEventSafely(fCancelEvent); }
		}

		protected ManualResetEvent CancelEvent
		{
			get { return fCancelEvent; }
		}

		protected ManualResetEvent FinishedEvent
		{
			get { return fFinishedEvent; }
		}

		protected void SetStatusMessageIfHasntBeenSet(string message)
		{
			if (fStatusMessage == DefaultStatus)
			{
				fStatusMessage = message;
			}
		}

		readonly string fUrl;
		string fFileName;
		long fFileSize = -1;
		DateTime fFileModifiedDateTime;

		WebRequest fRequest;
		WebResponse fResponse;
		protected string fStatusMessage = DefaultStatus;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string")]
		const string DefaultStatus = "Unknown";

		ManualResetEvent fCancelEvent;
		ManualResetEvent fFinishedEvent;

		IAsyncResult fAsyncGettingResponse;

		#region RequestState Utility Class

		protected class RequestState
		{
			public RequestState(WebFileDownloaderBase downloader)
			{
				this.Downloader = downloader;
				Request = downloader.fRequest;
				WaitHandle = null;
			}

			public WebFileDownloaderBase Downloader;
			public WebRequest Request;
			public RegisteredWaitHandle WaitHandle;
		}

		#endregion

		#endregion
	}
}
