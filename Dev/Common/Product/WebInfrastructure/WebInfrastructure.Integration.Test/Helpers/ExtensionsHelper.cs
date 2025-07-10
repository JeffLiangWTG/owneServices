using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CargoWiseOne.WebInfrastructure.Integration.Test.Helpers
{
	static class ExtensionsHelper
	{
		public static string ParseHtmlContent(this string html)
		{
			if (string.IsNullOrWhiteSpace(html))
			{
				return string.Empty;
			}

			try
			{
				var sb = new StringBuilder();
				var doc = new HtmlDocument();
				doc.LoadHtml(html);
				doc.DocumentNode.Descendants().Where(node => node.Name is "script" or "style").ToList().ForEach(node => node.Remove());

				foreach (var node in doc.DocumentNode.SelectNodes("//text()[normalize-space(.) != '']"))
				{
					var line = node.InnerText.Trim();
					if (!string.IsNullOrEmpty(line))
					{
						sb.AppendLine(line);
					}
				}

				sb.AppendLine();

				return sb.ToString();
			}
			catch
			{
				return html;
			}
		}

		public static IEnumerable<EventRecord> OrderTimeCreated(this IEnumerable<EventRecord> eventRecords)
		{
			return eventRecords.OrderBy(x => x.TimeCreated);
		}

		public static string GetKeyValue(this string keyValueGroups, string key, char delimiter = ';')
		{
			if (string.IsNullOrEmpty(keyValueGroups))
			{
				return string.Empty;
			}

			var keyValuePairs = keyValueGroups.Split(delimiter).ToList();
			var keyValue = keyValuePairs.FirstOrDefault(x => x.StartsWith(key, StringComparison.InvariantCultureIgnoreCase));
			if (string.IsNullOrEmpty(keyValue))
			{
				return string.Empty;
			}

			var match = Regex.Match(keyValue, @"(.*)=(?<value>.*)");
			if (match.Success)
			{
				return match.Groups["value"]?.Value.Trim();
			}

			return string.Empty;
		}
	}
}
