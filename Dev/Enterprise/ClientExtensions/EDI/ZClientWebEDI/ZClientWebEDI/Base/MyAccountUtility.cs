using System;
using System.Web;
using CargoWise.Common;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class MyAccountUtility
	{
		public static bool IsSafeUrl(this BasePage page, string url, string requestUrlHost = null) => IsLocalUrl(url) || IsSafeHost(() => page.Request.Url.Host, url, requestUrlHost);

		public static bool IsSafeUrl(this ControllerWithEnvironment controller, string url, string requestUrlHost = null) => IsLocalUrl(url) || IsSafeHost(() => controller.Request.RequestUri.Host, url, requestUrlHost);

		public static string GetLiteViewModeReturnUrl(this BasePage page, string url)
		{
			var defaultUrl = page.AppInstance.HostingSiteHomePage;
			if (!string.IsNullOrEmpty(url) && !ContainsResizeInlineFramePage(url))
			{
				if (page.IsSafeUrl(url))
				{
					var referrer = page.Request.UrlReferrer;
					if (referrer != null && ContainsResizeInlineFramePage(referrer.IsAbsoluteUri ? referrer.AbsoluteUri : referrer.OriginalString))
					{
						// If request is from the iframe resize page the return URL must be within this web application, not the hosting website
						defaultUrl = page.AppInstance.GetValidRedirectURL(url);
					}
					else
					{
						if (HttpUtility.ParseQueryString(url)[LoginRedirectKey] == "1")
						{
							defaultUrl = url;
						}
						else
						{
							defaultUrl = page.AppInstance.HostingSiteRoot.TrimEnd('/') + url;
						}
					}
				}
			}

			return defaultUrl;
		}

		public const string LoginRedirectKey = "LoginRedirect";

		internal static bool ContainsResizeInlineFramePage(string rawUrl) => rawUrl.IndexOf("resize-iframe.html", StringComparison.OrdinalIgnoreCase) >= 0;

		static bool IsSafeHost(Func<string> predicate, string url, string requestUrlHost = null)
		{
			if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
			{
				var hostUrl = requestUrlHost;
				if (string.IsNullOrEmpty(hostUrl))
				{
					try
					{
						hostUrl = predicate?.Invoke();
					}
					catch (Exception ex)
					{
						ErrorReporter.ReportOnce("Could not get host URL from delegate parameter", ex);
					}
				}

				return string.Equals(hostUrl, absoluteUri.Host, StringComparison.OrdinalIgnoreCase)
					|| absoluteUri.Host.EndsWith(".cargowise.com", StringComparison.OrdinalIgnoreCase)
					|| absoluteUri.Host.EndsWith("wisetechacademy.com", StringComparison.OrdinalIgnoreCase);
			}

			return false;
		}

		//https://docs.microsoft.com/en-us/aspnet/mvc/overview/security/preventing-open-redirection-attacks   (Listing 5)
		static bool IsLocalUrl(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				return false;
			}
			else
			{
				return ((url[0] == '/' && (url.Length == 1 ||
						(url[1] != '/' && url[1] != '\\'))) ||   // "/" or "/foo" but not "//" or "/\"
						(url.Length > 1 &&
						 url[0] == '~' && url[1] == '/'));   // "~/" or "~/foo"
			}
		}
	}
}
