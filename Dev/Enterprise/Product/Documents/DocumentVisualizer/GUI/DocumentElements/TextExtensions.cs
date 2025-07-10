using System;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	static class TextExtensions
	{
		public static string[] GetUrls(this IText text)
		{
			if (string.IsNullOrWhiteSpace(text?.Content))
			{
				return Array.Empty<string>();
			}

			var matches = UrlRegex.Matches(text.Content);

			var urls = matches
				.Cast<Match>()
				.Select(m => m.Groups["url"]?.Value)
				.ToArray();

			return urls;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant (regex expression)")]
		static Regex UrlRegex
		{
			get
			{
				if (urlRegex == null)
				{
					const string pattern = "(?<url>(ht|f)tp(s?)\\:\\/\\/[0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*(:(0-9)*)*(\\/?)([a-zA-Z0-9\\-\\.\\?\\,\\'\\/\\\\\\+&amp;%\\$#_]*)?)";
					urlRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				}

				return urlRegex;
			}
		}

		[ThreadStatic]
		static Regex urlRegex;
	}
}
