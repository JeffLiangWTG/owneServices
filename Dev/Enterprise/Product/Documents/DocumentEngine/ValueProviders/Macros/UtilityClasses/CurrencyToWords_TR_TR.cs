
#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_TR_TR : IndoEuropeanCurrencyConverter
	{
		public CurrencyToWords_TR_TR()
			: base(new NumberToString_TR_TR(), "bir", "{0}{1}", "yalniz{0}", "{0}{1}{2}")
		{ }

		protected override string MinorUnitsAmountToString(long value)
		{
			return numberToWords.GetNumberAsString(value);
		}

		protected override string Pluralize(string currency)
		{
			return currency;
		}
	}
}

#endregion
