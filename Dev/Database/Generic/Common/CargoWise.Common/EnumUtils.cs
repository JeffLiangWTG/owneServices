using System;
using System.Linq;

namespace CargoWise.Common
{
	public static class EnumUtils
	{
		public static bool HasAnyFlag<T>(this T item, params T[] flags) where T : struct
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("This is only valid for enum types");
			}

			return (((IConvertible)item).ToInt32(null) & flags.Aggregate(0, (l, r) => l | ((IConvertible)r).ToInt32(null))) > 0;
		}
	}
}