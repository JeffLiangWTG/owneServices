using System;
using System.Text.RegularExpressions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	public static class URLHelpers
	{
		public static string EscapeUrl(string url)
		{
			return Regex.Escape(url).Replace("/", "\\/");
		}

		public static bool TryGenerateURL(string endpoint, out Uri url)
		{
			var baseURL = GlowPortalsUri;
			if (string.IsNullOrEmpty(baseURL))
			{
				Globals.Message.ShowError(
					Res.GetString(
						"201824e1-a0f0-adb4-464d-10fc62f63a33",
						"This module cannot be opened in a browser as GLOW has not been configured for this client."
					)
				);
				url = default;
				return false;
			}
			url = UrlBuilder.GenerateURL(new Uri(baseURL), endpoint, string.Empty);
			return url != null;
		}

		public static string GlowPortalsUri
		{
			get
			{
				if (glowPortalsUriForTest != null)
				{
					return glowPortalsUriForTest;
				}

				return GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		[ThreadSafe]
		internal static string glowPortalsUriForTest;

		public static Regex GetFormFlowRegex(string portalCode, string urlSuffix)
		{
			if (!TryGenerateURL(portalCode, out var portalCodeUri))
			{
				return new Regex("^$");
			}

			var portalUrl = EscapeUrl(portalCodeUri.ToString().Split('?')[0]);
			return new Regex($"^{portalUrl}.*{EscapeUrl(urlSuffix)}");
		}
	}
}
