using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Enterprise.BufferManagement.Business
{
	public class SetInterner<T>
	{
		public ImmutableHashSet<T> Intern(IEnumerable<T> set)
		{
			var ordered = set.OrderBy(s => s).ToArray();
			var interned = sets.FirstOrDefault(s => s.OrderBy(otherS => otherS).SequenceEqual(ordered));

			if (interned != null)
			{
				return interned;
			}
			else
			{
				var keySet = set.ToImmutableHashSet();
				sets.Add(keySet);
				return keySet;
			}
		}

		readonly List<ImmutableHashSet<T>> sets = new List<ImmutableHashSet<T>>();
	}
}
