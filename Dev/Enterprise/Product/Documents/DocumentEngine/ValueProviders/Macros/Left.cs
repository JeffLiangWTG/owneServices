using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Left : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Left(\"{text}\", {length})>",
				ResString.GetMultilingualString("053b702e-dbcc-47a5-ad90-48d0f1948de2", @"Returns the leftmost characters from within the supplied string up to the maximum length specified."),
				new List<(string example, object expectedResult)> { ("<Left(\"<OH_FullName>\", 4)>", (NoResString)"Wise") });
		}

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);

			ZString text = match.Groups["text"].ToString();
			string lengthAsString = (match.Groups["length"] ?? (object)-1).ToString();

			int length;
			if (!int.TryParse(lengthAsString, out length))
			{
				length = -1;
			}

			return length < 1 ? ZString.Empty : text.Left(length);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<[\s]*Left[\s]*\([\s]*""(?<text>[\s\S]*)""[\s]*,[\s]*(?<length>[0-9]+)[\s]*\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
