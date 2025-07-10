using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public static class Extensions
	{
		public static T FallbackIfEmpty<T>(this T value, T fallbackValue) where T : IZType
		{
			return value.IsEmpty ? fallbackValue : value;
		}

		public static ZString JoinAsString<T>(this IEnumerable<T> values, string separator = null, bool distinct = true) => string.Join(separator ?? DefaultSeperator, (distinct ? values?.Distinct() : values) ?? Enumerable.Empty<T>());
		public static ZString JoinAsString<T>(this IEnumerable<T> values, char separator, bool distinct = true) => JoinAsString(values, separator.ToString(), distinct);

		const string DefaultSeperator = ",";

		const string DefaultValueSeparator = ";";

		public static string DistinctSortAndJoinForDisplay<T>(this IEnumerable<T> allValues, string separator = DefaultValueSeparator, Func<T, ZString> format = null) where T : IZType
		{
			if (format == null)
			{
				format = x => x.ToString();
			}
			return allValues.Where(str => !str.IsEmpty).Distinct().OrderBy(x => x).Select(value => format(value)).JoinAsString(separator);
		}
	}
}
