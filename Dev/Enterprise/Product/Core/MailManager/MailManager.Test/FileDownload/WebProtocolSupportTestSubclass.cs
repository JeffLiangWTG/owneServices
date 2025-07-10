using System;
using System.Net;

namespace Enterprise.MailManager.FileDownload
{
	sealed class WebProtocolSupportTestSubclass : WebProtocolSupport
	{
		public WebProtocolSupportTestSubclass(string url)
			: base(url)
		{
		}

		public override WebRequest GetWebRequestForDownload(int startPosition)
		{
			throw new NotImplementedException();
		}

		public override bool ResponseHasValidStatus(WebResponse response, RequestMode mode)
		{
			throw new NotImplementedException();
		}

		public override DateTime GetFileLastModified(WebResponse response)
		{
			throw new NotImplementedException();
		}
	}
}
