using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging;

public static class Extension
{
	public static IEnumerable<ZString> SplitByNewLine(this ZString content, StringSplitOptions splitOptions = StringSplitOptions.None)
	{
		return Regex
			.Split(content.Replace(CarriageReturn, string.Empty), EndOfLine)
			.Where(x => splitOptions != StringSplitOptions.RemoveEmptyEntries || !string.IsNullOrWhiteSpace(x))
			.Select(x => new ZString(x));
	}

	public static ZDateTime ParseToDateTimeWithFormat(this ZString dateTimeString, ZString format)
	{
		return DateTime.ParseExact(dateTimeString, format, CultureInfo.InvariantCulture);
	}

	public static ZString SubStringAndTrim(this ZString content, ZInt startIndex, ZInt length)
	{
		return content.Substring(startIndex, length).Trim();
	}

	public static ZString GetElementAtAndTrim(this IEnumerable<ZString> list, int index)
	{
		return list.ElementAt(index).Trim();
	}

	public static ZString GetElementAtAndTrimSafe(this IEnumerable<ZString> list, int index)
	{
		return list.ElementAtOrDefault(index).Trim();
	}

	public static ZDate ParseToDateDDMMYYYYSafe(this ZString dateString)
	{
		return dateString.IsEmpty ? ZDate.Empty : (ZDate)DateTime.ParseExact(dateString, FormatDateDDMMYYYY, CultureInfo.InvariantCulture);
	}

	public static ZInt? GetZIntOrNullIfEmpty(this ZString value)
	{
		return !value.IsEmpty ? ZInt.Parse(value) : null;
	}

	public static ZDecimal? GetZDecimalOrNullIfEmpty(this ZString value)
	{
		return !value.IsEmpty ? ZDecimal.Parse(value) : null;
	}

	public static ZInt? GetCountOrNullIfEmpty<T>(this IEnumerable<T> collection)
	{
		return collection.Any() ? collection.Count() : null;
	}

	const string FormatDateDDMMYYYY = "ddMMyyyy";
	const string EndOfLine = "\n";
	const string CarriageReturn = "\r";
}
