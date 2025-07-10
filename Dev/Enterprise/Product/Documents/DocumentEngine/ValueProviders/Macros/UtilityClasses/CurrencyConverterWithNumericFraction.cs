using System.Text;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public abstract class CurrencyConverterWithNumericFraction : CurrencyConvertor
	{
		internal CurrencyConverterWithNumericFraction(INumberToWords numberToWords, string singleUnitName, string majorMinorJoiningText, CurrencyLabelOptions currencyLabel)
		{
			var typeName = this.GetType().FullName;
			this.language = typeName.Substring(typeName.IndexOf('_') + 1).Replace('_', '-');
			this.numberToWords = numberToWords;
			this.singleUnitName = singleUnitName;
			this.majorMinorJoiningText = majorMinorJoiningText;
			this.currencyLabel = currencyLabel;
		}

		public override string ConvertToWords(double amount, string currencyCode, SpecialFormat specialFormat = SpecialFormat.None)
		{
			StringBuilder result = new StringBuilder();
			if (MajorUnitsInAmount(amount) != 1)
			{
				result.Append(numberToWords.GetNumberAsString(MajorUnitsInAmount(amount)));
			}
			else
			{
				result.Append(singleUnitName);
			}
			var currency = (RefCurrency)Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, currencyCode);
			int ratio = currency != null ? (int)currency.RX_SubUnitRatio : 0;
			if (!ShowMajorOnly(amount, ratio))
			{
				result.Append(majorMinorJoiningText);
				result.Append(PaddedMinorUnitsInAmount(amount, ratio));
				result.Append("/");
				result.Append(ratio);
			}
			result.Append(" ");
			switch (currencyLabel)
			{
				case CurrencyLabelOptions.Code:
					result.Append(currencyCode);
					break;

				case CurrencyLabelOptions.LocalMajorUnits:
					result.Append(GetLocalMajorUnits(currencyCode));
					break;
			}
			return result.ToString();
		}

		protected virtual bool ShowMajorOnly(double amount, int ratio)
		{
			return ratio <= 1;
		}

		string GetLocalMajorUnits(string currencyCode)
		{
			var currency = (RefCurrency)Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, currencyCode);
			if (currency == null)
			{
				return currencyCode;
			}
			else
			{
				return Pluralize(currency.RX_UnitNameMultilingual.ToString(language));
			}
		}

		protected virtual string Pluralize(string currency)
		{
			return currency;
		}

		readonly string language;
		readonly INumberToWords numberToWords;
		readonly string singleUnitName;
		readonly string majorMinorJoiningText;
		readonly CurrencyLabelOptions currencyLabel;

		internal enum CurrencyLabelOptions
		{
			Code,
			LocalMajorUnits,
		}
	}
}
