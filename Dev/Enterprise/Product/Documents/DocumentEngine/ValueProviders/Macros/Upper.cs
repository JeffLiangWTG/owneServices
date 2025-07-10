using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Visualisation;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Upper : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Upper(\"{text}\", {length})>",
				ResString.GetMultilingualString("e576b703-c3f2-49f6-8f93-76185a08f2fa", @"Returns the supplied string in UPPER CASE."),
				new List<(string example, object expectedResult)> { ("<Upper(\"<Z0_VarCharMax>\")>", "CONTAINERS") });
		}

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			ZString text = match.Groups["text"].ToString();
			return text.ToUpper();
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<[\s]*Upper[\s]*\([\s]*""(?<text>.*)""[\s]*\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);
	}
}
