using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class EvaluateInnerContent : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<EvaluateInnerContent(\"{stringcontainingmacrostoevaluate}\")>",
				ResString.GetMultilingualString("fb0e5d54-5bd9-4b5c-b415-c3759a983da9", @"Evaluates inner content, expands properties/macros."),
				new List<(string example, object expectedResult)> { ("<EvaluateInnerContent(\"<Z0_NVarchar> <CustomsCode(<BranchProxy>, NZ, XYZ)>\")>", (NoResString)"ABC 123 ABC123") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var innerContent = match.Groups["stringContainingMacrosToEvaluate"].Value.UnEscapeAngleBrackets();
			var cellContentReplacer = new CellContentReplacer(report, innerContent);
			cellContentReplacer.ReplaceMacros();
			return cellContentReplacer.ContentAsString;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<[\s]*EvaluateInnerContent[\s]*\(""(?<stringContainingMacrosToEvaluate>.*)""\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled);

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}
	}
}
