using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Html : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Html(\"{text}\", true|false)>",
				ResString.GetMultilingualString("b29d5a6e-090e-4a2c-b087-26cbd225b578", @"Allows HTML rendering, with option to indicate whether to evaluate the inner macros.
                  Currently only supports these tags: 
					<b> or <strong> - Bold.
					<i> or <em> - Italics.
					<u> - Underline.
					<s> or <strike> - Strikeout.
					<sub> - Subscript.
					<sup> - Superscript.
					<tt> - Use monospace font.
					<pre> - Preserve spaces and returns. On normal mode, if you have 2 spaces on a string, only one will be kept.
					<font> - Change the used font. This tag behaves as normal HTML, and you can specify color, face, point-size or size as attributes. Size might be between 1 and 7, and the size in points are 8, 9, 12, 14, 18, 24, 34 for each size. You can also specify relative sizes (for example -1)</item>.
					<h1>..<h6> - Header fonts.
					<small> - Use a smaller font. This is equivalent to <font size = '-1'>.
					<big> - Use a bigger font. This is equivalent to <font size = '+1'>.
                  Font Color supported:
                    Color code in six-digit form ({0}), but only those converted from three-digit RGB notation ({1}) by replicating digits can be displayed correctly in excel.
                        e.g. #550055 is correct but #500050 is not.
                    Color name:
                        black green silver lime gray olive white yellow maroon navy red blue purple teal fuchsia aqua.
The default format of the font is {2} 9 pt.",
"#rrggbb", "#rgb", "Arial"),
				new List<(string example, object expectedResult)> {
					("<Html(\"<pre><font color=\"#0f0\">Interface ITest</font></pre>\", false)>", (NoResString)"Interface ITest"),
					("<Html(\"<DbField>\", true)>", "WTG") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var text = match.Groups["text"].ToString();
			text = CleanUpTextForHtmlRendering(text);
			var result = new TRichString();
			if (report.XlInterface == null)
			{
				var parsedString = new THtmlParsedString(text);
				result.Value = parsedString.Text;
			}
			else
			{
				result.SetFromHtml(text, report.XlInterface.Xls.GetDefaultFormat, report.XlInterface.Xls);
			}
			result.Value = EscapeMacroSyntax(result.Value);
			return result;
		}

		protected override bool ShouldEvaluateInnerMacrosCore(string macro)
		{
			return RegexToFindMacroShouldEvaluateInnerMacros.IsMatch(macro);
		}

		string CleanUpTextForHtmlRendering(string text)
		{
			var result = new StringBuilder(text);
			if (!text.StartsWith((NoResString)"<pre>"))
			{
				result.Insert(0, (NoResString)"<pre>");
				result.Append((NoResString)"</pre>");
			}
			result.Replace("\\", "").Replace("&lt;", " &lt;").Replace("&gt;", " &gt;");
			return result.ToString();
		}

		string EscapeMacroSyntax(string value)
		{
			return value.Replace(" <", "\\<").Replace(" >", "\\>");
		}

		public override System.Text.RegularExpressions.Regex Regex
		{
			get { return RegexToFindMacroInString; }
		}

		internal static readonly Regex RegexToFindMacroInString = new Regex(@"^<[\s]*Html[\s]*\([\s]*""(?<text>.*)"",[\s]*(?<shouldEvaluateInnerMacros>true|false)[\s]*\)[\s]*>$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);
		internal static readonly Regex RegexToFindMacroShouldEvaluateInnerMacros = new Regex(@"^<[\s]*Html[\s]*\([\s]*""(?<text>.*)"",[\s]*true[\s]*\)[\s]*>$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);
	}
}
