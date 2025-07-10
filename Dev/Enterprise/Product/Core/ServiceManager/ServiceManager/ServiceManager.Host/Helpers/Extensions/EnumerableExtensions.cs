using System.Collections.Generic;

namespace Enterprise.ServiceManager.Host
{
	public static class EnumerableExtensions
	{
		public static int IndexOf<T>(this IEnumerable<T> source, T value)
		{
			var index = 0;
			var comparer = EqualityComparer<T>.Default;
			foreach (var item in source)
			{
				if (comparer.Equals(item, value))
				{
					return index;
				}

				index++;
			}
			return -1;
		}
	}
}

