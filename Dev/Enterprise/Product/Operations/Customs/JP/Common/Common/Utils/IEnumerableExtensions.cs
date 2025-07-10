using System.Collections.Generic;

namespace Enterprise.Customs.JP.Common;

public static class IEnumerableExtensions
{
	public static string ToStringWithSeparator<T>(this IEnumerable<T> items, string separator = ", ")
	{
		return string.Join(separator, items);
	}
}
