using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace Enterprise.DocumentEngine
{
	public class WebRequestRedirectResponseHelper
	{
		public WebRequestRedirectResponseHelper(string serviceUrl, string hostName, string apiPath)
		{
			ServiceUrl = serviceUrl;
			HostName = hostName ?? "";
			ApiPath = string.IsNullOrEmpty(apiPath) ? "" : "/" + apiPath.Trim('/') + "/";
		}

		string ServiceUrl { get; }
		string HostName { get; }
		string TemporaryRedirectUrl { get; set; }
		internal bool ShouldRetry { get; set; }
		internal bool SendNudgeViaIPAddress { get; set; }

		string ApiPath { get; }

		StringBuilder lgs;
		internal StringBuilder Logs => lgs ??= new StringBuilder();
		internal bool ShouldIgnoreRedirectError { get; private set; }

		public void HandleRedirectResponse(RequestMethodDelegate method)
		{
			for (var i = 0; i < RedirectMaxRetries; i++)
			{
				var serviceUrl = GetServiceUrl();
				var times = i + 1;
				Logs.AppendLine($"Running handle redirect response. URL: {serviceUrl}, Times: {times}.");

				method.Invoke(serviceUrl);

				if (!ShouldRetry)
				{
					Logs.AppendLine($"Skip retry. URL: {serviceUrl}, Times: {times}.");
					break;
				}
			}
		}

		public bool ShouldHandledWithRedirectResponse(HttpWebResponse webResponse)
		{
			TemporaryRedirectUrl = null;
			ShouldRetry = false;

			var statusCode = (int)webResponse.StatusCode;
			switch (statusCode)
			{
				case (int)HttpStatusCode.MovedPermanently:
				case (int)HttpStatusCode.Found:
				case (int)HttpStatusCode.TemporaryRedirect:
				case 308: //PermanentRedirect
					var newLocationUrl = webResponse.Headers["Location"];
					if (Uri.IsWellFormedUriString(newLocationUrl, UriKind.Absolute))
					{
						var newUri = new Uri(newLocationUrl);

						var appPath = newUri.AbsolutePath;
						if (!string.IsNullOrEmpty(ApiPath))
						{
							var apiPathIndex = appPath.IndexOf(ApiPath, StringComparison.OrdinalIgnoreCase);
							if (apiPathIndex > 0)
							{
								appPath = appPath.Substring(0, apiPathIndex);
							}
						}

						var newUrl = new UriBuilder(newUri.Scheme, newUri.Host, newUri.Port, appPath).ToString();

						if (statusCode == (int)HttpStatusCode.Found || statusCode == (int)HttpStatusCode.TemporaryRedirect)
						{
							TemporaryRedirectUrl = newUrl;
						}
						else
						{
							if (RedirectUrls.TryGetValue(HostName, out var redirectUrl))
							{
								if (IsDifferentUri(newUri, new Uri(redirectUrl)))
								{
									RedirectUrls[HostName] = newUrl;
								}
							}
							else
							{
								RedirectUrls.Add(HostName, newUrl);
							}
						}

						ShouldRetry = true;
					}
					else if (newLocationUrl.StartsWith("/Error.aspx", StringComparison.OrdinalIgnoreCase))
					{
						ShouldIgnoreRedirectError = true;
						ShouldRetry = true;
					}
					else
					{
						Logs.AppendLine($"Redirect URL is not valid. URL: {newLocationUrl}");
					}
					break;
				default:
					break;
			}

			return ShouldRetry;
		}

		string GetServiceUrl()
		{
			if (!string.IsNullOrEmpty(TemporaryRedirectUrl))
			{
				return TemporaryRedirectUrl;
			}
			else if (RedirectUrls.TryGetValue(HostName, out var redirectUrl))
			{
				return redirectUrl;
			}
			else if (SendNudgeViaIPAddress)
			{
				return ServiceUrl;
			}
			else
			{
				var serviceUri = new Uri(ServiceUrl);
				return new UriBuilder(serviceUri.Scheme, HostName, serviceUri.Port, serviceUri.AbsolutePath).ToString();
			}
		}

		bool IsDifferentUri(Uri uri1, Uri uri2) => uri1.Scheme != uri2.Scheme || uri1.Host != uri2.Host || uri1.Port != uri2.Port;

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static readonly Lazy<Dictionary<string, string>> redirectUrlsLazy = new Lazy<Dictionary<string, string>>(() => new Dictionary<string, string>());
		internal static Dictionary<string, string> RedirectUrls => redirectUrlsLazy.Value;
		const int RedirectMaxRetries = 3;
	}

	public delegate void RequestMethodDelegate(string serviceUrl);
}
