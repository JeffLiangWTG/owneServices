using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class HPageBreak : ValueProvider, INonVisualisableValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<HPageBreak>",
				ResString.GetMultilingualString("b530302d-c18f-4fd7-bbf3-cb01391e03f5", "Inserts a horizontal page break marker. Will cause the Document Engine to start a new page with the next row."),
				new List<(string example, object expectedResult)> { ("<HPageBreak>", null) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			report.DocumentRendererLogger.Log(Res.GetString("2536cb93-6f40-48b7-b10f-e77ba76c286a", "Inserting horizontal page break from <HPageBreak> macro."));
			return new FlexHPageBreak();
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)HPageBreak(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		/// <summary>
		/// This is the regex used by the visualiser to remove the macro text before visualising.
		/// </summary>
		internal static readonly Regex RegexToFindMacroAnyWhereInString = new Regex(@"<(?:[\s]*)HPageBreak(?:[\s]*)>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		#region INonVisualisableValueProvider

		public Regex RegexToReplaceMacro => RegexToFindMacroAnyWhereInString;

		#endregion
	}
}
