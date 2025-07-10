using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class SubString : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<SubString(\"{text}\", {start}[, {length}])>",
				ResString.GetMultilingualString("f195b004-e791-4215-a0dd-fe7c0ba944e6", @"Returns a subset of characters from within the supplied string. Characters will be returned starting from the zero based position specified by {0} and will continue to the end of the string if no {1} is specified. If a {1} is specified, characters up to the length specified will be returned.",
				"{start}", "{length}"),
				new List<(string example, object expectedResult)> { ("<SubString(\"<Z0_VarCharMax>\", 0)>", (NoResString)"WiseTech Global"), ("<SubString(\"<Z0_VarCharMax>\", 0, 8)>", "WiseTech") });
		}

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);

			ZString text = match.Groups["text"].ToString();
			string startAsString = match.Groups["start"].ToString();
			string lengthAsString = (match.Groups["length"] ?? (object)-1).ToString();

			int start;
			if (!int.TryParse(startAsString, out start))
			{
				start = 0;
			}

			int length;
			if (!int.TryParse(lengthAsString, out length))
			{
				length = -1;
			}

			if (length == -1)
			{
				return text.SubstringSafe(start);
			}

			string result = text.ToString().UnEscapeAngleBrackets();
			return new ZString(result).SubstringSafe(start, length);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<[\s]*Sub[\s]*String[\s]*\(""(?<text>.*)"",[\s]*(?<start>[0-9]+)(|,[\s]*(?<length>[0-9]+))[\s]*\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);
	}
}
