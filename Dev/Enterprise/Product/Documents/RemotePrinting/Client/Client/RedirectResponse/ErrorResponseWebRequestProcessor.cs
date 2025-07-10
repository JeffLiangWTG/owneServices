using System;
using System.Net;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Microsoft.AspNet.SignalR.Client;

namespace Enterprise.RemotePrinting.Client
{
	public class ErrorResponseWebRequestProcessor : IErrorResponseWebRequestProcessor
	{
		public ErrorResponseWebRequestProcessor(RemotePrintingService remotePrintingService)
		{
			this.remotePrintingService = remotePrintingService;
		}

		public Action<string> LogInfo { get; set; }

		public string ServiceUrl => remotePrintingService.Url;

		public bool ShouldHandleRedirectResponse(HttpWebResponse response, out string newUrl)
		{
			newUrl = null;
			if (response != null && IsRequestRedirected((int)response.StatusCode))
			{
				newUrl = response.Headers["Location"];
				if (Uri.IsWellFormedUriString(newUrl, UriKind.Absolute))
				{
					return true;
				}
			}

			return false;
		}

		public bool Process(MethodDelegate action, out MethodDelegate retryAction, out Exception outException, bool isTemporaryRetryAction = false)
		{
			retryAction = null;
			outException = null;

			try
			{
				return action();
			}
			catch (Exception ex) when (ShouldHandleRedirectResponseException(ex, out var responseCode, out var newUrl))
			{
				outException = ex;

				if (!Uri.IsWellFormedUriString(newUrl, UriKind.Absolute))
				{
					return false;
				}

				var newServiceUrl = GetNewServiceUrl(newUrl);
				if (string.Equals(newServiceUrl, remotePrintingService.Url, System.StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}

				LogInfo?.Invoke($"[{responseCode}]Retrying operation with new Url: {newServiceUrl}");
				if (responseCode == (int)HttpStatusCode.MovedPermanently || responseCode == (int)HttpStatusCode.Redirect || responseCode == 308)
				{
					remotePrintingService.Url = newServiceUrl;
					retryAction = action;
				}
				else if (isTemporaryRetryAction)
				{
					// if isTemporaryRetryAction is true, it means that the action is temporary tetry action, so the action should be the retry action
					retryAction = action;
				}
				else
				{
					retryAction = () =>
					{
						var regularUrl = remotePrintingService.Url;
						remotePrintingService.Url = newServiceUrl;
						try
						{
							return action();
						}
						finally
						{
							remotePrintingService.Url = regularUrl;
						}
					};
				}
			}

			return false;
		}

		bool ShouldHandleRedirectResponseException(Exception exception, out int responseCode, out string newUrl)
		{
			while (exception != null)
			{
				responseCode = 0;
				newUrl = string.Empty;

				if (exception is WebException wex && wex.Status == WebExceptionStatus.ProtocolError && wex.Response is HttpWebResponse httpResponse)
				{
					responseCode = (int)httpResponse.StatusCode;
					newUrl = httpResponse.Headers["Location"];
				}
				else if (exception is HttpClientException clientException && clientException.Response != null)
				{
					responseCode = (int)clientException.Response.StatusCode;
					newUrl = clientException.Response.Headers?.Location?.ToString();
				}

				if (IsRequestRedirected(responseCode))
				{
					return true;
				}

				exception = exception.InnerException;
			}

			responseCode = 0;
			newUrl = string.Empty;
			return false;
		}

		bool IsRequestRedirected(int responseCode)
		{
			switch (responseCode)
			{
				case (int)HttpStatusCode.MovedPermanently:
				case (int)HttpStatusCode.Found:
				case (int)HttpStatusCode.TemporaryRedirect:
				case 308: //PermanentRedirect
					return true;
				default:
					return false;
			}
		}

		string GetNewServiceUrl(string newUrl)
		{
			var index = newUrl.ToLowerInvariant().IndexOf(RemotePrintingService.ToLowerInvariant());
			if (index > 0)
			{
				return newUrl.Substring(0, index + RemotePrintingService.Length);
			}
			else
			{
				var newUri = new Uri(newUrl);
				return new UriBuilder(newUri.Scheme, newUri.Host, newUri.Port, RemotePrintingService).ToString();
			}
		}

		readonly RemotePrintingService remotePrintingService;
		const string RemotePrintingService = "RemotePrintingService.asmx";
	}
}
