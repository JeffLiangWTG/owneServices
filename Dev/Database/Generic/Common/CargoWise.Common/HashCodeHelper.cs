using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CargoWise.Common
{
	public static class HashCodeHelper
	{
		public static int GetCompositeHashCode<T>(IEnumerable<T> elements)
		{
			Argument.NotNull(elements, nameof(elements));

			const int primeSeed = 17;
			return GetCompositeHashCode(primeSeed, elements);
		}

		public static int GetCompositeHashCode<T>(int seed, IEnumerable<T> elements)
		{
			Argument.NotNull(elements, nameof(elements));

			unchecked
			{
				return elements.Where(elem => elem != null).Aggregate(seed, (total, current) => total * primeMuliplier + current.GetHashCode());
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "seed*23")]
		public static int GetCompositeHashCode(int seed, int hashCode)
		{
			unchecked
			{
				return seed * primeMuliplier + hashCode;
			}
		}

		const int primeMuliplier = 23;
	}
}
