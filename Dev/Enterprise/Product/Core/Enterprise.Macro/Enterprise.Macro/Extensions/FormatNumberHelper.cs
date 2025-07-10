using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Macro.Extensions
{
	public static class FormatNumberHelper
	{
		public static string FormatNumber(object value, object secondParam)
		{
			if (value is string || value is ZString)
			{
				var strValue = value.ToString();
				if (!decimal.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var numericValue))
				{
					return strValue;
				}
				return FormatAmount(numericValue, GetDecimals(secondParam));
			}
			return FormatAmount(GetNumericValue(value), GetDecimals(secondParam));
		}

		static decimal GetNumericValue(object value) => value switch
		{
			decimal d => d,
			double db => (decimal)db,
			float f => (decimal)f,
			int i => i,
			long l => l,
			short s => s,
			INumericZType numericZ => (decimal)new ZDecimal(numericZ),
			_ => throw new MacroRuntimeException(Res.GetString("97ce1fc7-5f33-4927-9682-f89021df9a55", "Invalid value format: {0}", value))
		};

		static int GetDecimals(object secondParam) => secondParam switch
		{
			int i => GetDecimalsFromInt(i),
			string str => GetDecimalsFromStr(str),
			_ => throw new MacroRuntimeException(Res.GetString("ae4fc78f-41f4-4518-97d1-76afd92ecdb7", "Invalid Currency Code or Decimal Places format: {0}", secondParam))
		};

		static int GetDecimalsFromStr(string secondParam)
		{
			if (int.TryParse(secondParam, out var decimals))
			{
				return GetDecimals(decimals);
			}

			var currency = RefCurrency.LoadFromCurrencyCode(new BusinessObjectFactory(), secondParam) ?? GlbCompany.CurrentCompany.LocalCurrency;
			return currency.Decimals;
		}

		static int GetDecimalsFromInt(int decimals)
		{
			if (decimals > 99)
			{
				throw new MacroRuntimeException(Res.GetString("3d68580a-6e06-40fe-857c-7c901808ee4e", "Decimal places can not be greater than 99."));
			}

			return decimals < 0 ? GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces : decimals;
		}

		static string FormatAmount(IFormattable amount, int decimalsPlaces)
		{
			var noCurrencySymbolFormat = (NumberFormatInfo)Culture.CurrentCompanyCountryCulture.NumberFormat.Clone();
			noCurrencySymbolFormat.CurrencySymbol = "";
			noCurrencySymbolFormat.CurrencyDecimalDigits = decimalsPlaces;

			return amount.ToString("C", noCurrencySymbolFormat).TrimEnd();
		}
	}
}
