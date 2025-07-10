using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ReplaceCarriageReturnsWithSpaces : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ReplaceCarriageReturnsWithSpaces(\"{text}\")>",
				ResString.GetMultilingualString("463835ee-610d-448c-884e-c0aec263eedf", @"Replaces all carriage returns within the specified text with 3 spaces so that you fit as much of a multi line field as you can onto one line."),
				new List<(string example, object expectedResult)> { ("<ReplaceCarriageReturnsWithSpaces(\"<Z0_VarCharMax>\")>", (NoResString)"Address   Profile") });
		}

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);

			ZString text = match.Groups["text"].ToString();
			return text.Trim().Replace("\r\n", "   ").Replace("\r", "   ").Replace("\n", "   ");
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<[\s]*Replace[\s]*Carriage[\s]*Returns[\s]*With[\s]*Spaces[\s]*\([\s]*""(?<text>.*)""[\s]*\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled);
	}
}
