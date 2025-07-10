using System;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public static class DecimalHelper
	{
		const string SpanishCultureCode = "es-ES";

		public static ZString GetESStringDecimalFormat(ZDecimal unformattedDecimal)
		{
			var esNumberFormat = CultureInfo.GetCultureInfo(SpanishCultureCode).NumberFormat;
#if NET
			esNumberFormat = (NumberFormatInfo)esNumberFormat.Clone();
			esNumberFormat.NumberDecimalDigits = 2;
			esNumberFormat.PercentDecimalDigits = 2;
#endif
			var sep = esNumberFormat.NumberDecimalSeparator;
			var strFormatted = unformattedDecimal.ToString("N", esNumberFormat);
			return strFormatted.Contains(sep) ? strFormatted.TrimEnd('0').TrimEnd(sep.ToCharArray()) : strFormatted;
		}

		public static ZDecimal DecimalParseToESFormat(ZString value)
		{
			var numberFormat = CultureInfo.GetCultureInfo(SpanishCultureCode).NumberFormat;

			var result = ZDecimal.Zero;
			try
			{
				result = ZDecimal.Parse(value, numberFormat);
			}
			catch (OverflowException) { }
			catch (FormatException) { }

			return result;
		}
	}
}
