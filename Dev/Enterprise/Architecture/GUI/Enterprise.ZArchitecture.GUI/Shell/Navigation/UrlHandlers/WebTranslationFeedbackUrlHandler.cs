using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	class WebTranslationFeedbackUrlHandler : UrlHandler
	{
		protected override string ExpectedCommandText
		{
			get { return "WebTranslationFeedback"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception")]
		protected override bool HandleCore(QueryString queryString)
		{
			var language = queryString["l"];
			var caption = queryString["c"];
			var usageUrl = queryString["u"];

			if (!string.IsNullOrEmpty(usageUrl))
			{
				var host = new Uri(usageUrl).GetLeftPart(UriPartial.Authority);
				var trustedDomains = EnvProxy.Instance.Registry.ResourceStringUsageTrustedDomains;

				if (!trustedDomains.Contains(host))
				{
					Globals.Message.ShowError($"Invalid parameter, Detect untrusted domain {host}.");
					return true;
				}
			}

			string[] keys = null;

			if (usageUrl == usageKeysCacheUrl)
			{
				keys = usageKeysCache;
			}
			else
			{
				try
				{
#pragma warning disable SYSLIB0014 // 'WebRequest.Create(string)' is obsolete: 'WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.'
					using (var responseStream = HttpWebRequest.Create(usageUrl).GetResponse().GetResponseStream())
#pragma warning restore SYSLIB0014
					{
						keys = Encoding.UTF8.GetString(Compressor.Uncompress(responseStream.ToByteArray())).Split('\n');
						if (IsHtml(keys))
						{
							throw new Exception("Server returned an error page");
						}

						usageKeysCache = keys;
						usageKeysCacheUrl = usageUrl;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError("Failed to retrieve context translation data: " + ex.Message);
				}
			}

			if (keys != null)
			{
				ObjectFactory.Get<ITranslationFeedbackProvider>().Feedback(language, caption, keys);
			}

			return true;
		}

		bool IsHtml(string[] keys)
		{
			if (htmlRegex == null)
			{
				htmlRegex = new Regex(@"^\s+\<", RegexOptions.Compiled);
			}

			return keys.Any(line => htmlRegex.IsMatch(line));
		}

		string[] usageKeysCache;
		string usageKeysCacheUrl;
		Regex htmlRegex;
	}
}
