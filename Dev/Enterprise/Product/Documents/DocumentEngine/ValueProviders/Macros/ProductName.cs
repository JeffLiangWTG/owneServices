using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ProductName : ValueProvider
	{
		public ProductName() : base() { }

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(
				"<ProductName>",
				ResString.GetMultilingualString("b4f6d098-3ad4-4e23-b3f5-45c4c7d2295e",
				$"This macro is used to retrieve the product name. Currently, it replaces '{Core.Constants.ProductName}' with 'CargoWise'."),

				new List<(string example, object expectedResult)>
				{
					("<ProductName>", (NoResString)"CargoWise")
				}
			);
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string result;

			if (Regex.IsMatch(macro, fRegex.ToString(), RegexOptions.IgnoreCase))
			{
				result = (NoResString)"CargoWise";
			}
			else
			{
				result = (NoResString)$"Invalid property name: {macro}";
			}

			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"^\s*<\s*ProductName\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
