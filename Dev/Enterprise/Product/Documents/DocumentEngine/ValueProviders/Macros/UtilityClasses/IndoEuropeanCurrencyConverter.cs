using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	abstract public class IndoEuropeanCurrencyConverter : CurrencyConvertor
	{
		internal IndoEuropeanCurrencyConverter(INumberToWords numberToWords, string singleUnitName, string majorUnitPattern, string onlyMajorPattern, string majorMinorPattern)
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
			refCurrency = (RefCurrency)Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, currencyCode);
			string majorUnit = "";
			string minorUnit = "";
			int ratio = 0;

			if (refCurrency == null)
			{
				majorUnit = currencyCode;
			}
			else
			{
				majorUnit = refCurrency.RX_UnitNameMultilingual.ToString(language);
				minorUnit = refCurrency.RX_SubUnitNameMultilingual.ToString(language);
				ratio = refCurrency.RX_SubUnitRatio;
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

			result = string.Format(majorUnitPattern, result, MajorUnitsInAmount(amount) == 1 ? majorUnit : Pluralize(majorUnit));
			result = MinorUnitsInAmount(amount, ratio) == 0
				? string.Format(onlyMajorPattern, result)
				: string.Format(majorMinorPattern, result, specialFormat == SpecialFormat.LTR
						? MinorUnitsAmountToWords(MinorUnitsInAmount(amount, ratio))
						: MinorUnitsAmountToString(MinorUnitsInAmount(amount, ratio))
					, minorUnit);
			return result;
		}

		protected virtual string MinorUnitsAmountToWords(long value)
		{
			return numberToWords.GetNumberAsString(value);
		}

		protected virtual string MinorUnitsAmountToString(long value)
		{
			return value.ToString();
		}

		protected abstract string Pluralize(string currency);

		protected RefCurrency refCurrency;
		readonly string language;
		readonly internal INumberToWords numberToWords;
		readonly string singleUnitName;
		readonly string majorUnitPattern;
		readonly string onlyMajorPattern;
		readonly string majorMinorPattern;
	}
}
