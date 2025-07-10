using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class AutoHeight : ValueProvider, INonVisualisableValueProviderThatModifyDocumentLayout
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<AutoHeight[({minimumRows}[, RemoveLineBreaksToFit])]>",
				ResString.GetMultilingualString("d6461c21-695c-4262-8514-666299513d1f", @"Used as a prefix to a subsequent macro. 
If the value inserted by the subsequent macro won't fit in the cell in the template, additional rows will be inserted below to show any text that won't fit on the first. 

NB: This was designed for use in {0} sections only to make it so that large body sections could be broken up over multiple pages. May cause pagination issues if you try and use it in header or footer sections. You can use 'Wrap Text' (Format the Cell in Excel) if you want a single unmerged cell to expand in a header or footer, or put your {1}'ed field in a new '{2}' section so that pagination can be taken care of properly.

You can not use this {1} macro within a formula or nested within another macro.

Optionally the minimum number of rows can be set; The row will always expand to the minimum number of rows specified.

Optionally specify the '{3}' flag which, if the content in the row is too tall to fit within the Excel size limit, will remove line breaks from the text to try and fit. If the content is still too big it will be split across multiple rows as per normal.
{4}",
"#SectionBody", "<AutoHeight>", "#SectionBody:Data=DummyCollection", "RemoveLineBreaksToFit",
DocumentationForFormatting),
				new List<(string example, object expectedResult)> { ("<AutoHeight><InvoiceLine.GoodsDescription>", null), ("<AutoHeight(5)><InvoiceLine.GoodsDescription>", null), ((NoResString)"<AutoHeight(5, RemoveLineBreaksToFit)><InvoiceLine.GoodsDescription>", null), ("<AutoHeight(RemoveLineBreaksToFit)><InvoiceLine.GoodsDescription>", null) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return "";
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex("^" + RegexPattern + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		/// <summary>
		/// This is the regex really used to interpret this line in the class Enterprise.DocumentEngine.Areas.Area 
		/// and by the visualiser to remove the macro text before visualising.
		/// </summary>
		internal static readonly Regex RegexToFindMacroAnyWhereInString = new Regex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular Expression")]
		public const string RegexPattern = @"<[\s]*Auto[\s]*Height[\s]*(|(?<removeLineBreaks>\(\s*RemoveLineBreaksToFit\s*\))|\([\s]*(?<minimumRows>[0-9.]+)(?<removeLineBreaks>(,[\s]*RemoveLineBreaksToFit)?)[\s]*\))[\s]*>";

		internal static int GetMinimumRows(string macro)
		{
			int result = 1;

			var match = RegexToFindMacroAnyWhereInString.Match(macro);

			if (match.Success)
			{
				var groups = match.Groups;

				if (!int.TryParse(groups["minimumRows"].Value, out result))
				{
					result = 1;
				}
			}

			return result;
		}

		internal static bool ShouldRemoveLineBreaksToFit(string macro)
		{
			var match = RegexToFindMacroAnyWhereInString.Match(macro);
			return match.Success && !string.IsNullOrEmpty(match.Groups["removeLineBreaks"].Value);
		}

		#region INonVisualisableValueProviderThatModifyDocumentLayout

		public ZString DocumentationForFormatting => AdditionalDocumentation;
		public Regex RegexToReplaceMacro => RegexToFindMacroAnyWhereInString;

		#endregion
	}
}
