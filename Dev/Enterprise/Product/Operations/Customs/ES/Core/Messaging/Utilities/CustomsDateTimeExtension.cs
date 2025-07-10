using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
public static class CustomsDateTimeExtension
{
	public const string DateTimeFormatLongWithSeconds = "yyyyMMddHHmmss";
	public const string DateFormat = "yyyyMMdd";
	public const string DateTimeFormatLong = "yyyyMMddHHmm";
	public const string DateTimeFormatShort = "yyMMddHHmm";
	public const string DateFormatWithDash = "yyyy-MM-dd";
	public const string DateTimeFormatForSignature = "yyyy-MM-ddTHH:mm:ssK";
	public const string DateTimeFormatTransaction = "yyMMddHHmmss";
	public const string DateTimeFormatTransaction14 = "yyMMddHHmmssff";
	public const string TimeFormatSpain = "HHmmss";
	public const string DateFormatSpain = "ddMMyyyy";
	public const string DateFormatSpainWithDash = "dd-MM-yyyy";
	public const string DateTimeFormatyyyyMMddTHHmmss = "yyyy-MM-ddTHH:mm:ss";

	public static ZString ToCustomsFormatString(this ZDateTime dateTime, ZString format)
	{
		return dateTime.ToString(format, CultureInfo.InvariantCulture);
	}

	public static ZString ToShortCustomsFormatDateString(this ZDateTime dateTime)
	{
		return dateTime.ToCustomsFormatString("yyMMdd");
	}

	public static ZString ToCustomsFormatTimeString(this ZDateTime dateTime)
	{
		return dateTime.ToCustomsFormatString("HHmm");
	}

	public static ZString ToLongCustomsFormatDateTimeString(this ZDateTime dateTime)
	{
		return dateTime.ToCustomsFormatString(DateTimeFormatLong);
	}

	public static ZString ToCustomsFormatDateString(this ZDateTime dateTime)
	{
		return dateTime.ToCustomsFormatString(DateFormat);
	}

	public static ZString ToCustomsFormatDateStringddMMyyyy(this ZDateTime dateTime)
	{
		return dateTime.ToCustomsFormatString(DateFormatSpain);
	}

	public static ZString ToCustomsFormatDateStringddMMyyyyWithDash(this ZDateTime dateTime)
	{
		return dateTime.ToCustomsFormatString(DateFormatSpainWithDash);
	}

	public static ZString ToCustomsFormatDateStringddMMyyyyHHmmWithDash(this ZDateTime dateTime)
	{
		return dateTime.ToCustomsFormatString("dd-MM-yyyy HH:mm");
	}

	public static ZString ToCustomsFormatDateStringddMMyyyyHHmmssWithDash(this ZDateTime dateTime)
	{
		return dateTime.ToCustomsFormatString("dd-MM-yyyy, HH:mm:ss");
	}

	public static ZString ToCustomsFormatDateStringyyyyMMddTHHmmss(this ZDateTime dateTime)
	{
		return dateTime.ToCustomsFormatString(DateTimeFormatyyyyMMddTHHmmss);
	}

	public static ZString ToCustomsFormatDateStringyyyyMMWithDash(this ZDate date)
	{
		return date.ToString("yyyy-MM", CultureInfo.InvariantCulture);
	}
}
