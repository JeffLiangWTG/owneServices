using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class HtmlEx : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<HtmlEx(\"{Html}\", True|False)>",
ResString.GetMultilingualString("f524adde-ea0c-416e-a111-ed4911ca9334", @"Allows HTML rendering, with option to indicate whether to evaluate the inner macros.
The cell will be expanded to fit the HTML if required.

Currently only supports these tags: 
  <b> or <strong> - Bold.
  <i> or <em> - Italics.
  <u> - Underline.
  <s> or <strike> - Strikeout.
  <sub> - Subscript.
  <sup> - Superscript.
  <tt> - Use monospace font.
  <pre> - Preserve spaces and returns. On normal mode, if you have 2 spaces on a string, only one will be kept.
  <font> - Change the used font. This tag behaves as normal HTML, and you can specify color, face, point-size or size as attributes. Size might be between 1 and 7, and the size in points are 8, 9, 12, 14, 18, 24, 34 for each size. You can also specify relative sizes (for example -1)</item>
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
				new List<(string example, object expectedResult)> { ("<HtmlEx(\"<pre><font color=\"#0f0\">Interface ITest</font></pre>\", false)>", (NoResString)"Interface ITest"), ("<HtmlEx(\"<DbField>\", true)>", "WTG") });
		}

		static readonly Regex regex = new Regex(@"^<[\s]*HtmlEx[\s]*\([\s]*""(?<Html>.*)""[\s]*,[\s]*(?<ShouldEvaluateInnerMacros>True|False)[\s]*\)[\s]*>$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var renderer = report.Renderer;
			var row = renderer.CurrentRow;
			var column = renderer.CurrentColumn;
			var excelFile = report.WorkSheetCurrentlyBeingProcessed.ParentExcelInterface.Xls;
			var cellFormat = excelFile.GetCellFormat(row + 1, column + 1);
			var format = excelFile.GetFormat(cellFormat);
			var html = match.Groups["Html"].Value;

			html = CleanUpTextForHtmlRendering(html);

			var result = new TRichString();
			result.SetFromHtml(html, format, excelFile);
			result.Value = EscapeMacroSyntax(result.Value);

			ExpandToFit.Expand(report.WorkSheetCurrentlyBeingProcessed.ParentExcelInterface, report.WorkSheetCurrentlyBeingProcessed.WorkSheetNumber - 1, row, column, RowToExpand.First, result.Value);

			return result;
		}

		protected override bool ShouldEvaluateInnerMacrosCore(string macro)
		{
			var result = false;
			var match = Regex.Match(macro);

			bool.TryParse(match.Groups["ShouldEvaluateInnerMacros"].Value, out result);

			return result;
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		string CleanUpTextForHtmlRendering(string text)
		{
			var result = new StringBuilder(text);

			if (!text.StartsWith((NoResString)"<pre>"))
			{
				result.Insert(0, (NoResString)"<pre>");
				result.Append((NoResString)"</pre>");
			}

			result.Replace("\\", string.Empty).Replace("&lt;", " &lt;").Replace("&gt;", " &gt;");

			return result.ToString();
		}

		string EscapeMacroSyntax(string value)
		{
			return value.Replace(" <", "\\<").Replace(" >", "\\>");
		}
	}
}
