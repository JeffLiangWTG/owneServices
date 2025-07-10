using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Visualisation;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	#region SuppressResourceStringsCheckRegion

	class ChangeCase : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ChangeCase(\"{text}\", L|U|T)>",
				ResString.GetMultilingualString("bbb67cb0-4fc3-4da4-ba73-c77201e9cd19", @"Returns the supplied string with case changed according to second parameter value:
L - lower case;
U - UPPER CASE;
T - Title Case"),
				new List<(string example, object expectedResult)> { ("<ChangeCase(\"Hello Hello\", U)>", "HELLO HELLO"), ("<ChangeCase(\"Hello Hello\", L)>", "hello hello"), ("<ChangeCase(\"heLlo hEllo\", T)>", "Hello Hello") });
		}

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			string text = match.Groups["text"].ToString();
			string format = match.Groups["format"].ToString();
			switch (format)
			{
				case "L":
				case "l":
					text = text.ToLower();
					break;

				case "U":
				case "u":
					text = text.ToUpper();
					break;

				case "T":
				case "t":
					text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
					break;

				default:
					throw new ArgumentException("Invalid ChangeCase format string: " + format + ".\r\nValid values are L, U, and T.");
			}
			return text;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<[\s]*ChangeCase[\s]*\([\s]*""(?<text>.*)""[\s]*\,[\s]*(?<format>L|U|T)[\s]*\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);
	}

	#endregion
}
