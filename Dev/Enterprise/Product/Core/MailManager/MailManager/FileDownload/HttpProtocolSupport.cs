using System;
using System.Net;

namespace Enterprise.MailManager.FileDownload
{
	class HttpProtocolSupport : WebProtocolSupport
	{
		public HttpProtocolSupport(string url, IWebProxy proxy)
			: this(url)
		{
			Proxy = proxy;
		}

		public HttpProtocolSupport(string url)
			: base(url)
		{ }

		public override WebRequest GetWebRequestForDownload(int startPosition)
		{
			HttpWebRequest request = (HttpWebRequest)base.GetWebRequestForDownload();
			request.AddRange(startPosition);

			return request;
		}

		public override WebRequest GetWebRequestForFileInfo()
		{
			WebRequest request = base.GetWebRequestForFileInfo();
			request.Method = WebRequestMethods.Http.Head;

			return request;
		}

		public override bool ResponseHasValidStatus(WebResponse response, RequestMode mode)
		{
			bool result = false;
			HttpWebResponse httpResponse = response as HttpWebResponse;
			if (httpResponse != null)
			{
				switch (mode)
				{
					case RequestMode.ResumeDownload:
						{
							result = httpResponse.StatusCode == HttpStatusCode.PartialContent;
							break;
						}
					case RequestMode.StartDownload:
					case RequestMode.GetFileInfo:
						{
							result = httpResponse.StatusCode == HttpStatusCode.OK;
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
			HttpWebResponse httpResponse = response as HttpWebResponse;
			return httpResponse.LastModified;
		}

		public override bool IsResumeSupported
		{
			get
			{
				return !Url.ToLower().StartsWith(@"https:");
			}
		}
	}
}
