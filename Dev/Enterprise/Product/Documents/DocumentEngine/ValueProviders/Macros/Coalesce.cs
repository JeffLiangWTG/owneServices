using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Visualisation;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Coalesce : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Coalesce(\"{fieldPath}\", \"{defaultValue}\")>",
						ResString.GetMultilingualString("79615794-44bf-4a9a-a340-5f9ee6e5dc72", @"If {0} cannot be found, is empty or is null, returns {1}. Otherwise, returns {0}.",
						"{fieldPath}", "{defaultValue}"),
						new List<(string example, object expectedResult)> { ("<Coalesce(\"<JS_ActualChargeable>\", \"0\")>", "0") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = regex.Match(macro);
			var contents = match.Groups["Content"].Value;

			var parameters = innerRegex.Matches(contents);

			if (parameters.Count != 2)
			{
				ReportInvalidParametersError(report, contents);
				return string.Empty;
			}

			var fieldPath = parameters[0].Groups["Parameter"];
			var defaultValue = parameters[1].Groups["Parameter"];

			if (!fieldPath.Success || !defaultValue.Success)
			{
				ReportInvalidParametersError(report, contents);
				return string.Empty;
			}

			try
			{
				string result;
				using (report.ErrorManager.ReportAllErrorsWithSeverity(ReportProcessingErrorSeverity.Information))
				{
					result = report.TranslateMacros(fieldPath.Value, true);
				}

				return !string.IsNullOrEmpty(result) ? result : report.TranslateMacros(defaultValue.Value);
			}
			catch (FieldNotFoundException)
			{
				return report.TranslateMacros(defaultValue.Value);
			}
		}

		protected override bool ShouldEvaluateInnerMacrosCore(string macro) => false;

		public override bool EvaluateAllInnerMacrosWhenGettingReplacement => true;

		public override VisualiserComponentTypes ComponentType => VisualiserComponentTypes.TextEdit;

		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<\s*Coalesce\s*\(\s*(?<Content>.*)\)\s*>$",
					RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		static readonly string macroContents = $@"(?>{RegexProvider.OutermostMacroRegex}|\\.|[^""])*";

		static readonly Regex innerRegex = new Regex($@"(,|^)\s*""(?<Parameter>{macroContents})""\s*",
					RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		void ReportInvalidParametersError(Report report, string parameters)
		{
			ReportMacroError(report, Res.GetString("A45B18FA-BF2F-4F38-A8AC-518A0F88CA90", "Invalid parameters provided: {0}, expected format: <Coalesce(\"{{fieldPath}}\", \"{{defaultValue}}\")>.", parameters));
		}
	}
}
