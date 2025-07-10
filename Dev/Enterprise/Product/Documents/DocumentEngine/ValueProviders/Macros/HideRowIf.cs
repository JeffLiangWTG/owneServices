using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class HideRowIf : ValueProvider, INonVisualisableValueProviderThatModifyDocumentLayout, IValueProviderThatShouldBeCopiedIfNewRowAdded
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<HideRowIf({evaluatableexpression})>",
				ResString.GetMultilingualString("1d3302ac-38f6-4295-8dab-3be7f7925b4c",
				@"This macro returns no value, but will cause the Document Engine to set the height of the current row to 0 (hides the row) if the expression specified evaluates to true.
{0}
{1}
{2}
{3}
The Document Engine uses Microsoft JScript to evaluate the expressions, so any JScript expressions that evaluate to a boolean result will work, with the exception of 'greater than' or 'less than':
To do 'greater than' use '{4}' instead of '>'.
To do 'greater than or equals to' use '{4}=' instead of '>='.
To do 'less than' use '{5}' instead of '<'.
To do 'less than or equals to' use '{5}=' instead of '<='.
{6}",
				"Please note that we might get some extra blank lines when some macros are used together with this macro.",
				"E.g: <HideRowIf(<TotalPages> == 2)>",
				"E.g: <HideRowIf(<CurrencyMajorUnit(USD)> == \"dollar\")>",
				"E.g: <HideRowIf(<DeliveryCount> == 3)>",
				"&gt;", "&lt;",
				DocumentationForFormatting),
				new List<(string example, object expectedResult)> {
					("<HideRowIf(<CurrentPage> == 1)>", null),
					((NoResString)"<HideRowIf(<CurrentPage> &gt; 5)>", null),
					((NoResString)"<HideRowIf(\"<Importer.Name>\" != \"\")>", null) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var parameter = regex.Match(macro).Groups[1].Value.Replace("\r\n", " ");
			parameter = parameter.Replace("\r", " ");
			parameter = parameter.Replace("\n", " ");
			parameter = parameter.Replace("\\", "\\\\");

			object result = "";
			try
			{
				if (ExpressionEvaluator.Evaluate(parameter, report.UseJsEvaluator))
				{
					result = new RowHider();
				}
			}
			catch (ExpressionEvaluationException ex)
			{
				var message = report.ErrorManager.OuterContent == null ? (NoResString)". Input macro: [" + macro + (NoResString)"]" : ".";
				ReportMacroError(report, ex.Message + message);
			}
			return result;
		}

		protected override bool NeedCheckForScientificNotation => true;

		protected override string ReplaceNestedMacro(string macro, IMacroTranslator macroTranslator, Passes pass)
		{
			using (Culture.SetTemporarily(Culture.Default))
			{
				return base.ReplaceNestedMacro(macro, macroTranslator, pass).EscapeQuotes();
			}
		}

		public override Regex Regex => regex;
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)Hide(?:[\s]*)Row(?:[\s]*)If(?:[\s]*)\((?:[\s]*)(.+)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		/// <summary>
		/// This is the regex used by the visualiser to remove the macro text before visualising.
		/// </summary>
		internal static readonly Regex RegexToFindMacroAnyWhereInString = new Regex(@"<(?:[\s]*)Hide(?:[\s]*)Row(?:[\s]*)If(?:[\s]*)\((?:[\s]*)(.+)(?:[\s]*)\)(?:[\s]*)>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		#region INonVisualisableValueProviderThatModifyDocumentLayout

		public ZString DocumentationForFormatting => AdditionalDocumentation;
		public Regex RegexToReplaceMacro => RegexToFindMacroAnyWhereInString;

		#endregion

		#region IValueProviderThatShouldBeCopiedIfNewRowAdded

		public Regex RegexToFindMacro => RegexToFindMacroAnyWhereInString;

		#endregion
	}
}
