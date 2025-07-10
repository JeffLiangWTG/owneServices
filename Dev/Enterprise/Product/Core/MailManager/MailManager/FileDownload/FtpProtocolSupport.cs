using System;
using System.Net;

namespace Enterprise.MailManager.FileDownload
{
	class FtpProtocolSupport : WebProtocolSupport
	{
		public FtpProtocolSupport(string url) : base(url)
		{
		}

		public override WebRequest GetWebRequestForDownload()
		{
			FtpWebRequest request = (FtpWebRequest)base.GetWebRequestForDownload();
			request.Method = WebRequestMethods.Ftp.DownloadFile;

			return request;
		}

		public override WebRequest GetWebRequestForDownload(int startPosition)
		{
			FtpWebRequest request = (FtpWebRequest)GetWebRequestForDownload();
			request.ContentOffset = startPosition;

			return request;
		}

		public override bool ResponseHasValidStatus(WebResponse response, RequestMode mode)
		{
			bool result = false;
			FtpWebResponse ftpResponse = response as FtpWebResponse;
			if (ftpResponse != null)
			{
				switch (mode)
				{
					case RequestMode.ResumeDownload:
						{
							result = ftpResponse.StatusCode == FtpStatusCode.RestartMarker;
							break;
						}
					case RequestMode.StartDownload:
					case RequestMode.GetFileInfo:
						{
							result = ftpResponse.StatusCode == FtpStatusCode.CommandOK ||
								ftpResponse.StatusCode == FtpStatusCode.OpeningData ||
								ftpResponse.StatusCode == FtpStatusCode.DataAlreadyOpen;
							break;
						}
					default:
						break;
				}
			}

			return result;
		}

		public override DateTime GetFileLastModified(WebResponse response)
		{
			FtpWebResponse ftpResponse = response as FtpWebResponse;
			return ftpResponse.LastModified;
		}

		public override bool IsResumeSupported
		{
			get { return true; }
		}
	}
}
