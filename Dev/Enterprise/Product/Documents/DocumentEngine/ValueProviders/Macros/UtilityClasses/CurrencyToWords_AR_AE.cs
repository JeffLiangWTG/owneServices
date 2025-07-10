using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_AR_AE : CurrencyConvertor
	{
		public override string ConvertToWords(double amount, string currencyCode, SpecialFormat specialFormat = SpecialFormat.None)
		{
			var isNegative = amount < 0;
			amount = Math.Abs(amount);

			var result = string.Empty;
			var refCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);

			var majorUnitName = refCurrency?.RX_UnitNameMultilingual.ToString("AR-AE") ?? currencyCode;

			var minorUnitName = refCurrency?.RX_SubUnitNameMultilingual.ToString("AR-AE") ?? string.Empty;
			var minorUnitNameTwo = minorUnitName;
			var minorUnitNameThreeToTen = minorUnitName;

			var ratio = refCurrency?.RX_SubUnitRatio ?? 0;

			switch (refCurrency?.Code)
			{
				case CurrencyCodes.SaudiArabia:
					minorUnitName = (NoResString)"هللة";
					minorUnitNameTwo = (NoResString)"هللتين";
					minorUnitNameThreeToTen = (NoResString)"هللات";
					break;
				default:
					break;
			}

			var majorUnitsInAmount = MajorUnitsInAmount(amount);
			var minorUnitsInAmount = MinorUnitsInAmount(amount, ratio);

			if (isNegative)
			{
				result += string.Format("{0} ", negativeSign);
			}
			var numberToString = new NumberToString_AR_AE();
			var majorUnitsInAmountString = numberToString.GetNumberAsString(majorUnitsInAmount);
			result += string.IsNullOrEmpty(majorUnitsInAmountString) ? majorUnitName : $"{majorUnitsInAmountString} {majorUnitName}";

			if (minorUnitsInAmount != 0)
			{
				var minorUnitsInAmountString = specialFormat == SpecialFormat.LTR ? numberToString.GetNumberAsString(minorUnitsInAmount) : minorUnitsInAmount.ToString();
				result += (NoResString)" و ";
				result += $"{minorUnitsInAmountString} {GetMinorUnit(minorUnitsInAmount)}";
			}

			string GetMinorUnit(long minorAmount)
			{
				if (minorAmount == 2)
				{
					return minorUnitNameTwo;
				}
				if (minorAmount >= 3 && minorAmount <= 10)
				{
					return minorUnitNameThreeToTen;
				}
				return minorUnitName;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string negativeSign = "سلبية";
	}
}
