using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class HtmlHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html strings")]
		public static HtmlTableCreator GetHtmlTableCreator(string border = "1", string width = "100%")
		{
			return new HtmlTableCreator(new NameValueCollection
			{
				{ "border", border },
				{ "cellpadding", "1" },
				{ "cellspacing", "0" },
				{ "width", width },
				{ "class", "table" }
			}) { EnableHTMLEncoding = false };
		}

		public static string ToKeyValuePairSection(IEnumerable<(string Key, string Value)> sections)
		{
			var content = sections
						.Where(p => !string.IsNullOrEmpty(p.Value))
						.Select(p => new ZString($"{ToStrongIfNotEmpty($"{p.Key}: ")}{p.Value}".Trim(' ', ':')))
						.Where(p => !string.IsNullOrEmpty(p))
						.JoinAsString("<br>");

			return ToPIfNotEmpty(content);
		}

		public static string ToStrongIfNotEmpty(string strong) => !string.IsNullOrEmpty(strong) ? $"<strong>{strong}</strong>" : string.Empty;

		public static string ToH3IfNotEmpty(string h3) => !string.IsNullOrEmpty(h3) ? $"<H3>{h3}</H3>" : string.Empty;

		public static string ToPIfNotEmpty(string p) => !string.IsNullOrEmpty(p) ? $"<p>{p}</p>" : string.Empty;
	}
}
