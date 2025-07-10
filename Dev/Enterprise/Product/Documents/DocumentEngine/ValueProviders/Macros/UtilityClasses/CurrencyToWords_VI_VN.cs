using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_VI_VN : CurrencyConvertor
	{
		public override string ConvertToWords(double amount, string currencyCode, SpecialFormat specialFormat = SpecialFormat.None)
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
			var result = "";
			var majorUnitName = "";
			var minorUnitName = "";
			var ratio = 0;

			if (currency == null)
			{
				majorUnitName = currencyCode;
			}
			else
			{
				majorUnitName = currency.RX_UnitNameMultilingual.ToString(Core.Constants.Languages.Vietnamese);
				minorUnitName = currency.RX_SubUnitNameMultilingual.ToString(Core.Constants.Languages.Vietnamese);
				ratio = currency.RX_SubUnitRatio;
			}

			var numberToString = new NumberToString_VI_VN();
			long majorUnits = MajorUnitsInAmount(amount);
			long minorUnits = MinorUnitsInAmount(amount, ratio);

			result = numberToString.GetNumberAsString(majorUnits) + " " + majorUnitName;

			if (minorUnits > 0)
			{
				result += " " + numberToString.GetNumberAsString(minorUnits) + " " + minorUnitName;
			}
			return result;
		}
	}
}
