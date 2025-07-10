using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class CurrencySubUnitFormat : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CurrencySubUnitFormat({currency code})>",
				ResString.GetMultilingualString("292b150b-fdcc-411e-96ad-ab78b52df809", @"For a given currency, returns its format."),
				new List<(string example, object expectedResult)> { ("<CurrencySubUnitFormat(USD)>", "#,##0.00") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string result = string.Empty;

			RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Regex.Match(macro).Groups[1].Value);
			if (currency != null)
			{
				result = "#,##0.";
				int subunitRatio = currency.RX_SubUnitRatio;
				while (subunitRatio > 1)
				{
					subunitRatio /= 10;
					result += "0";
				}

				if (result == "#,##0.")
				{
					result = "#,##0";
				}
			}
			return result;
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"^<\s*CurrencySubUnitFormat\s*\(\s*(.+)\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
