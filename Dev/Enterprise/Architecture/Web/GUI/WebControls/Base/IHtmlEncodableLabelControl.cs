using System;
using System.Text.RegularExpressions;
using System.Web.Security.AntiXss;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public interface IHtmlEncodableLabelControl
	{
		bool EnableHtmlEncoding { get; set; }
	}

	public static class IHtmlEncodableLabelControlExtensions
	{
		public static string GetHtmlEncodableLabelContent(this IHtmlEncodableLabelControl control, string text)
		{
			if (control.EnableHtmlEncoding)
			{
				var corrected = SpaceSeparatorsRegex.Value.Replace(text, " ");
				var safe = AntiXssEncoder.HtmlEncode(corrected, false);
				var safeWithLineBreaks = EncodedNewLineRegex.Value.Replace(safe, "<br />"); // html

				return safeWithLineBreaks;
			}

			return text;
		}

		static readonly Lazy<Regex> EncodedNewLineRegex = new Lazy<Regex>(() => new Regex(AntiXssEncoder.HtmlEncode(System.Environment.NewLine, false)));
		static readonly Lazy<Regex> SpaceSeparatorsRegex = new Lazy<Regex>(() => new Regex(@"[\p{Zs}]"));
	}
}

// Tested on the implementors
