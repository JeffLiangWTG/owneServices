using System;
using System.Net;

namespace Enterprise.RemotePrinting.Client
{
	public interface IErrorResponseWebRequestProcessor
	{
		string ServiceUrl { get; }
		Action<string> LogInfo { get; set; }
		bool ShouldHandleRedirectResponse(HttpWebResponse response, out string newUrl);
		bool Process(MethodDelegate action, out MethodDelegate retryAction, out Exception outException, bool isTemporaryRetryAction = false);
	}

	public delegate bool MethodDelegate();
}
