using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Truncate : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Truncate(\"{text}\", {length})>",
				ResString.GetMultilingualString("17e53957-c16b-4d00-bb4c-25cfffdf0632",
				@"Truncates a specified string so that the string cuts off after the length of characters specified by the user. An ellipsis '...' is used to replace the remaining characters after the specified length has been reached."),
				new List<(string example, object expectedResult)> { ((NoResString)"<Truncate(\"<JobDescription>\", 20)>", (NoResString)"The port where th...") });
		}

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			ZString text = match.Groups["text"].ToString();
			string lengthAsString = match.Groups["length"].ToString();
			int length;
			if (!int.TryParse(lengthAsString, out length) || length >= text.Length)
			{
				return text;
			}

			ZString elipsis = "...";
			if (length < 3)
			{
				return elipsis.Left(length);
			}
			else
			{
				return text.SubstringSafe(0, length - 3) + elipsis;
			}
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<[\s]*Truncate[\s]*\(""(?<text>.*)"",([\s]*(?<length>[0-9]+))[\s]*\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
