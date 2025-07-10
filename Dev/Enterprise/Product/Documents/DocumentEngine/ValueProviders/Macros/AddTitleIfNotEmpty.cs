using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class AddTitleIfNotEmpty : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<AddTitleIfNotEmpty(\"{StartingTitle}\", \"{Value}\", \"{EndingTitle}\")>",
						ResString.GetMultilingualString("65ad5dd9-ded2-49b9-835e-43c64b8eb793", @"If {0} is not empty returns {1} + {0} + {2}. If {0} is empty, returns a blank string.",
						"{Value}", "{StartingTitle}", "{EndingTitle}"),
						new List<(string example, object expectedResult)> { ((NoResString)"<AddTitleIfNotEmpty(\"Consignor: \", \"<ConsignorName>\", \", \")>", (NoResString)"Consignor: Test Organization, ") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			ZString startingTitle = regex.Match(macro).Groups["StartingTitle"].Value;
			ZString endingTitle = regex.Match(macro).Groups["EndingTitle"].Value;
			ZString value = regex.Match(macro).Groups["Value"].Value;

			return value.IsEmpty ? "" : startingTitle + value + endingTitle;
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(
					@"^<(?:[\s]*)AddTitleIfNotEmpty(?:[\s]*)\((?:[\s]*)""(?<StartingTitle>.*)""(?:[\s]*)\,(?:[\s]*)""(?<Value>.*)""(?:[\s]*)\,(?:[\s]*)""(?<EndingTitle>.*)""(?:[\s]*)\)(?:[\s]*)>$",
					RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}
	}
}
