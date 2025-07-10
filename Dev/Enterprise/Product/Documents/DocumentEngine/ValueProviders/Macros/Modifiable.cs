using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Visualisation;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Modifiable : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Modifiable({macros})>",
				ResString.GetMultilingualString("f5ba5613-9444-413c-8cac-08fcc4fc2315", @"Ensures that the macros will be modifiable in visualization as a single field."),
				new List<(string example, object expectedResult)> { ("<Modifiable(<Z0_VarCharMax>: <Z0_Number>)>", "<Z0_VarCharMax>: <Z0_Number>") });
		}

		public override VisualiserComponentTypes ComponentType => VisualiserComponentTypes.TextEdit;

		internal override bool ShouldEscapeAngleBrackets => false;

		protected override object GetReplacementCore(string macro, Report report)
		{
			object result = "";

			var match = Regex.Match(macro);
			result = match.Groups[MacrosGroupTag];

			return result.ToString();
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		public static Regex MacroRegex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(@"^" + RegexPatternToFindMacroAnyWhereInString + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex RegexToFindMacroAnyWhereInString = new Regex(RegexPatternToFindMacroAnyWhereInString, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular Expression")]
		internal const string RegexPatternToFindMacroAnyWhereInString = @"<[\s]*Modifiable[\s]*\((?<macros>.+)\)[\s]*>";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal Developer Only Tag")]
		internal const string MacrosGroupTag = "macros";
	}
}
