using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class CurrencyMajorUnit : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CurrencyMajorUnit({currency code})>",
				ResString.GetMultilingualString("7e74821b-af81-45b6-9584-379206e0708e", @"For a given currency, returns its major unit."),
				new List<(string example, object expectedResult)> { ("<CurrencyMajorUnit(USD)>", (NoResString)"dollar") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string result = string.Empty;
			RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Regex.Match(macro).Groups[1].Value);

			if (currency != null)
			{
				result = currency.RX_UnitNameMultilingual;
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

		static readonly Regex fRegex = new Regex(@"^<\s*CurrencyMajorUnit\s*\(\s*(.+)\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
