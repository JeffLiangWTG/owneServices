using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Common
{
	/// <summary>
	/// This collection exists to allows for eagerly evaluated O(1) concatenation for quickly building result sets, similar to the result Linq's 'Concat' function.
	/// (But this collection is eager and read only in order to avoid recalculating the result on every enumeration.)
	///
	/// This class is useful because it makes it guarantee's the safety of the building collections and caches Count.
	/// </summary>
	public class ConcatCollection<T> : IReadOnlyCollection<T>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ConcatCollection(IEnumerable<IReadOnlyCollection<T>> collections)
		{
			Argument.NotNull(collections, nameof(collections));
			this.collections = collections;
			Count = collections.Sum(c => c.Count);
		}

		readonly IEnumerable<IReadOnlyCollection<T>> collections;

		public int Count { get; }
		public IEnumerator<T> GetEnumerator() => new MultipleEnumerator<T>(collections);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
