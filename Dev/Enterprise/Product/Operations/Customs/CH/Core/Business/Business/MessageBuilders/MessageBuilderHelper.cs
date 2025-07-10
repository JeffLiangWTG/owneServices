using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public static class MessageBuilderHelper
{
	public static string ReturnDefaultValueIfNullOrEmpty(this ZString? inputValue, string defaultValue = null)
	{
		return inputValue.HasValue && !inputValue.Value.IsEmpty ? inputValue.Value.ToString() : defaultValue;
	}

	public static string FallbackIfEmpty(this ZString inputValue, string defaultValue)
	{
		return !inputValue.IsEmpty ? inputValue.ToString() : defaultValue;
	}

	public static string ReturnNullIfEmpty(this ZString inputValue)
	{
		return !inputValue.IsEmpty ? inputValue.ToString() : null;
	}

	public static decimal FallbackIfEmpty(this ZDecimal inputValue, decimal defaultValue)
	{
		return inputValue.IsEmpty ? defaultValue : (decimal)inputValue;
	}

	public static decimal? ReturnNullIfEmpty(this ZDecimal inputValue)
	{
		return inputValue.IsEmpty ? null : inputValue;
	}

	public static DateTime? ToOptionalDateTime(this ZDateTime inputValue)
	{
		return inputValue.IsEmpty ? null : inputValue.ToDateTime();
	}

	public static DateTime? ToOptionalDateTimeUTC(this ZDateTime inputValue)
	{
		return inputValue.IsEmpty ? null : inputValue.ToUniversalBranchTime().ToDateTime();
	}

	public static string ToStringInvariantCulture(this ZDecimal value)
	{
		return ((decimal)value).ToString(CultureInfo.InvariantCulture);
	}

	public static string ToStringInvariantCulture(this ZInt value)
	{
		return ((int)value).ToString(CultureInfo.InvariantCulture);
	}
}
