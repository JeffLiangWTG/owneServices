using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DataTransfer.Native.Utils
{
	public static class CollectionExtension
	{
		public static bool IsEmpty<T>(this IEnumerable<T> collection)
		{
			return collection == null || !collection.Any();
		}
	}
}