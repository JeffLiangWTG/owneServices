using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Delimit : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Delimit(\"{FirstValue}\", \"{DelimitingValue}\", \"{SecondValue}\")>",
					ResString.GetMultilingualString("90a5d4bd-a763-477a-9fbb-b303bba76606", @"If {0} and {1} are both not empty, returns both values with the {2} in between. If just one has a value, returns the non empty value, otherwise returns a blank string.",
					"{FirstValue}", "{SecondValue}", "{DelimitingValue}"),
				new List<(string example, object expectedResult)> { ((NoResString)"<Delimit(\"<CurrentPage>\", \" of \", \"<TotalPages>\")>", (NoResString)"1 of 2") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var matchGroups = regex.Match(macro).Groups;
			ZString firstValue = matchGroups["FirstValue"].Value;
			ZString delimiter = matchGroups["DelimitingValue"].Value;
			ZString secondValue = matchGroups["SecondValue"].Value;

			return firstValue.IsEmpty || secondValue.IsEmpty ? firstValue + secondValue : firstValue + delimiter + secondValue;
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(
				@"^<(?:[\s]*)Delimit(?:[\s]*)\((?:[\s]*)""(?<FirstValue>.*)""(?:[\s]*)\,(?:[\s]*)""(?<DelimitingValue>.*)""(?:[\s]*)\,(?:[\s]*)""(?<SecondValue>.*)""(?:[\s]*)\)(?:[\s]*)>$",
				RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}
	}
}
