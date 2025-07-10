using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Common
{
	public static class EnumerablesExtensions
	{
		#region IList

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void Add<T1, T2>(this IList<Tuple<T1, T2>> list, T1 item1, T2 item2) => list.Add(Tuple.Create(item1, item2));
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void Add<T1, T2, T3>(this IList<Tuple<T1, T2, T3>> list, T1 item1, T2 item2, T3 item3) => list.Add(Tuple.Create(item1, item2, item3));
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void Add<T1, T2, T3, T4>(this IList<Tuple<T1, T2, T3, T4>> list, T1 item1, T2 item2, T3 item3, T4 item4) => list.Add(Tuple.Create(item1, item2, item3, item4));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Needed to infer anonymous types")]
		public static IEqualityComparer<T> CreateComparerForElements<T>(this IEnumerable<T> enumerationToInferTFrom, Func<T, T, bool> equalityFunc, Func<T, int> hashCodeFunc)
		{
			return new LambdaComparer<T>(equalityFunc, hashCodeFunc);
		}

		#endregion
		public static bool EqualIgnoringOrder<T>(this IEnumerable<T> list, IEnumerable<T> other, IEqualityComparer<T> comparer = null, bool nullTolerant = true)
		{
			if (list == null || other == null)
			{
				return false;
			}

			var dictionary = new Dictionary<T, Counter>(comparer);
			var defaults = 0;

			foreach (var item in list)
			{
				if (Equals(item, default(T)))
				{
					if (!nullTolerant)
					{
						return false;
					}

					defaults++;
					continue;
				}

				Counter counter;

				if (dictionary.TryGetValue(item, out counter))
				{
					counter.count++;
				}
				else
				{
					dictionary.Add(item, new Counter());
				}
			}

			foreach (var item in other)
			{
				if (Equals(item, default(T)))
				{
					defaults--;

					if (defaults < 0)
					{
						return false;
					}

					continue;
				}

				Counter counter;

				if (dictionary.TryGetValue(item, out counter))
				{
					counter.count--;
					if (counter.count < 0)
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			return defaults == 0 && dictionary.Values.All(c => c.count == 0);
		}

		class Counter
		{
			public int count = 1;
		}
	}

	public class LambdaComparer<T> : IEqualityComparer<T>
	{
		public LambdaComparer(Func<T, T, bool> equals, Func<T, int> getHashcode)
		{
			this.equals = Argument.NotNull(equals, nameof(equals));
			this.getHashcode = Argument.NotNull(getHashcode, nameof(getHashcode));
		}

		readonly Func<T, T, bool> equals;
		readonly Func<T, int> getHashcode;

		bool IEqualityComparer<T>.Equals(T x, T y)
		{
			return equals(x, y);
		}

		int IEqualityComparer<T>.GetHashCode(T obj)
		{
			return getHashcode(obj);
		}
	}
}
