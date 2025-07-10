
#region SuppressResourceStringsCheckRegion

using System;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_PT_PT : IndoEuropeanCurrencyConverter
	{
		public CurrencyToWords_PT_PT()
			: base(new NumberToString_PT_PT(), "um", "{0} {1}", "{0}", "{0} e {1} {2}")
		{ }

		protected override string Pluralize(string currency)
		{
			if (currency.Equals("metical", StringComparison.CurrentCultureIgnoreCase))
			{
				return "meticais";
			}
			return currency + "s";
		}
	}
}

#endregion
