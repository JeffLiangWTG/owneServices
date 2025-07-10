using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public static class DecimalExtensions
{
	public static ZString FormatNumberInDocument(this decimal? number) => number.HasValue ? number.Value.FormatNumberInDocument() : ZString.Empty;

	public static ZString FormatNumberInDocument(this decimal number) => new ZDecimal(number).FormatNumber();

	public static ZString FormatNumberInDocument(this int? number) => number.HasValue ? new ZInt(number.Value).FormatNumber(0) : ZString.Empty;

	public static ZString FormatNumberInDocument(this int number) => new ZInt(number).FormatNumber(0);

	public static ZString FormatNumber(this INumericZType num, int decimalPlace)
	{
		var format = decimalPlace > 0 ? string.Format(CultureInfo.InvariantCulture, "{0}0:###,##0{1}{2}", "{", ".".PadRight(decimalPlace + 1, '0'), "}") : "{0:###,##0}";
		return string.Format(CultureInfo.InvariantCulture, format, num);
	}

	public static ZString FormatNumber(this ZDecimal num)
	{
		return FormatNumber(num, num.DecimalPlaces);
	}
}
