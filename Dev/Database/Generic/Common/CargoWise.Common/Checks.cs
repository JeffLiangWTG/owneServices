using System;

namespace CargoWise.Common
{
	public static class Checks
	{
		public static class AnyElement
		{
			public static bool IsNull<T>(params T[] elements)
			{
				Argument.NotNull(elements, nameof(elements));
				return Array.Exists(elements, e => e == null);
			}

			public static bool IsNotNull<T>(params T[] elements)
			{
				Argument.NotNull(elements, nameof(elements));
				return Array.Exists(elements, e => e != null);
			}

			public static bool MatchesPredicate<T>(Predicate<T> predicate, params T[] elements)
			{
				Argument.NotNull(predicate, nameof(predicate));
				Argument.NotNull(elements, nameof(elements));
				return Array.Exists(elements, e => predicate(e));
			}
		}
	}
}
