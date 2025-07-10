using System.Collections.Immutable;
using System.Globalization;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	abstract public class SlavicCurrencyConverter : CurrencyConvertor
	{
		internal SlavicCurrencyConverter(INumberToWords numberToWords, string singleUnitName, string majorUnitPattern, string onlyMajorPattern, string majorMinorPattern)
		{
			this.language = this.GetType().FullName.Substring(this.GetType().FullName.IndexOf('_') + 1).Replace('_', '-');
			this.numberToWords = numberToWords;
			this.singleUnitName = singleUnitName;
			this.majorUnitPattern = majorUnitPattern;
			this.onlyMajorPattern = onlyMajorPattern;
			this.majorMinorPattern = majorMinorPattern;
		}

		public override string ConvertToWords(double amount, string currencyCode, SpecialFormat specialFormat = SpecialFormat.None)
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
			string majorUnitName = "";
			string minorUnitName = "";
			int ratio = 0;

			long majorUnitsInAmount = MajorUnitsInAmount(amount);
			long minorUnitsInAmount = 0;

			if (currency == null)
			{
				majorUnitName = currencyCode;
			}
			else
			{
				ratio = currency.RX_SubUnitRatio;
				minorUnitsInAmount = MinorUnitsInAmount(amount, ratio);
				majorUnitName = Pluralize(majorUnitsInAmount, currency, false);
				minorUnitName = Pluralize(minorUnitsInAmount, currency, true);
			}

			string result = "";

			if (MajorUnitsInAmount(amount) != 1)
			{
				result = numberToWords.GetNumberAsString(MajorUnitsInAmount(amount));
			}
			else
			{
				result = singleUnitName;
			}

			result = string.Format(CultureInfo.InvariantCulture, majorUnitPattern, result, majorUnitName);
			result = minorUnitsInAmount == 0 ? string.Format(CultureInfo.InvariantCulture, onlyMajorPattern, result) : string.Format(CultureInfo.InvariantCulture, majorMinorPattern, result, MinorUnitsAmountToString(minorUnitsInAmount), minorUnitName);
			return result;
		}

		protected virtual string MinorUnitsAmountToString(long value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		protected virtual string Pluralize(long amount, RefCurrency currency, bool isSubUnit)
		{
			string pluralizedCurrencyName;

			int units = (int)(amount % 10);
			int tens = (int)(amount / 10);

			if (units == 1 && tens != 1)
			{
				pluralizedCurrencyName = GetNumberForm(currency, NumberForms.Form1, isSubUnit);
			}
			else if (units >= 2 && units <= 4 && tens != 1)
			{
				pluralizedCurrencyName = GetNumberForm(currency, NumberForms.Form2, isSubUnit);
			}
			else
			{
				pluralizedCurrencyName = GetNumberForm(currency, NumberForms.Form3, isSubUnit);
			}

			return pluralizedCurrencyName;
		}

		string GetNumberForm(RefCurrency currency, NumberForms numberForms, bool isSubUnit)
		{
			var currencyNameFromRefCurrency = isSubUnit ? currency.RX_SubUnitNameMultilingual.ToString(language) : currency.RX_UnitNameMultilingual.ToString(language);

			var dictionary = CurrenciesDictionary;
			if (dictionary.ContainsKey(currency.Code))
			{
				var slavicCurrencyGrammar = dictionary[currency.Code];
				if (isSubUnit)
				{
					switch (numberForms)
					{
						case NumberForms.Form1:
							return slavicCurrencyGrammar.SubUnitForm1;
						case NumberForms.Form2:
							return slavicCurrencyGrammar.SubUnitForm2;
						case NumberForms.Form3:
							return slavicCurrencyGrammar.SubUnitForm3;
					}
				}
				else
				{
					switch (numberForms)
					{
						case NumberForms.Form1:
							return slavicCurrencyGrammar.UnitForm1;
						case NumberForms.Form2:
							return slavicCurrencyGrammar.UnitForm2;
						case NumberForms.Form3:
							return slavicCurrencyGrammar.UnitForm3;
					}
				}
			}

			return currencyNameFromRefCurrency;
		}

		protected enum NumberForms
		{
			Form1,
			Form2,
			Form3,
		}

		internal abstract ImmutableDictionary<string, SlavicCurrencyForms> CurrenciesDictionary { get; }

		internal struct SlavicCurrencyForms
		{
			public string UnitForm1;
			public string UnitForm2;
			public string UnitForm3;
			public string SubUnitForm1;
			public string SubUnitForm2;
			public string SubUnitForm3;

			public SlavicCurrencyForms(string unitForm1, string unitForm2, string unitForm3, string subUnitForm1, string subUnitForm2, string subUnitForm3)
			{
				UnitForm1 = unitForm1;
				UnitForm2 = unitForm2;
				UnitForm3 = unitForm3;
				SubUnitForm1 = subUnitForm1;
				SubUnitForm2 = subUnitForm2;
				SubUnitForm3 = subUnitForm3;
			}
		}

		readonly string language;
		readonly internal INumberToWords numberToWords;
		readonly string singleUnitName;
		readonly string majorUnitPattern;
		readonly string onlyMajorPattern;
		readonly string majorMinorPattern;
	}
}
