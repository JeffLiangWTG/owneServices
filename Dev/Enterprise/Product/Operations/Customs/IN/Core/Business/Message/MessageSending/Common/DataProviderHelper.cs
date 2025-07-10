using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Business;

public static class DataProviderHelper
{
	public static DateTime? ToDateTimeOrNullIfEmpty(this ZDate date)
	{
		return !date.IsValid ? null : date.ToDateTime();
	}

	public static DateTime? ToDateTimeOrNullIfEmpty(this ZDateTime date)
	{
		return !date.IsValid ? null : date.ToDateTime();
	}

	public static ZString GetFullAddressString(this JobDocAddress address)
	{
		var infoArray = new[] {
					address.CompanyName,
					address.Address1,
					address.Address2,
					address.City,
					address.State,
					address.Postcode };
		return ZString.Join(" ", infoArray.Where(x => !x.IsEmpty).ToArray());
	}

	public static ZString NormalizeToSingleLine(this ZString inputText)
	{
		return new ZString(Regex.Replace(inputText, "\\r\\n?|\\n", " "));
	}
}
