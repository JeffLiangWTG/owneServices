using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class FallbackIfEmpty : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<FallbackIfEmpty(\"{Macros}\",\"{FallbackWithMacros}\")>",
				ResString.GetMultilingualString("80dd16ee-49ed-464b-a5e1-11a2e4f670e5",
				@"Gets the value of Macros and displays them unless empty or zero. If Macros results in an empty string or zero, {0} is evaluated and displayed instead. Macros can contain macros or a string, but if Macros contains a string the fallback will never be used. The result of Macros is Trimmed to remove spaces before checking if it is empty.",
				"FallbackWithMacros"),
				new List<(string example, object expectedResult)> { ("<FallbackIfEmpty(\"<Z0_VarCharMax><Z0_Number>\",\"Text, <Z0_Fallback>\")>", (NoResString)"Text, Fallback") });
		}

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			object macros = "";

			Match match = Regex.Match(macro);
			macros = match.Groups["macros"];

			string result = macros.ToString();

			if (string.IsNullOrWhiteSpace(result) || result == "0")
			{
				result = match.Groups["fallback"].ToString();
			}

			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<[\s]*Fallback[\s]*If[\s]*Empty[\s]*\(""(?<macros>.*)"",""(?<fallback>.*)""\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
