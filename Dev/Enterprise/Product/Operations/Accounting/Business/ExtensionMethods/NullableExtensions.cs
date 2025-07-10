using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public static class NullableExtensions
	{
		public static bool TryGetValue<T>(this T? property, out T value)
			where T : struct
		{
			if (property.HasValue)
			{
				value = property.Value;
				return true;
			}

			value = default(T);
			return false;
		}

		public static U GetValueSafe<T, U>(this T dataObject, Func<T, U> getProperty)
			where T : class
		{
			return dataObject == null ? default(U) : getProperty(dataObject);
		}

		public static U? GetValueSafe<T, U>(this T dataObject, Func<T, U?> getProperty)
			where T : class
			where U : struct
		{
			U? defaultResult = null;
			return dataObject == null ? defaultResult : getProperty(dataObject);
		}

		public static ZString JoinExcludingEmpty(ZString separator, IEnumerable<ZString> values)
		{
			return values == null ? ZString.Empty : ZString.Join(separator, values.Where(x => !x.IsEmpty).ToArray());
		}

		public static string JoinExcludingEmpty(string separator, IEnumerable<string> values)
		{
			return values == null ? "" : string.Join(separator, values.Where(x => !string.IsNullOrEmpty(x)));
		}
	}
}
