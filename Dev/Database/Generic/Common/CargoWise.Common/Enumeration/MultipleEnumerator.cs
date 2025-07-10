using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Common
{
	public sealed class MultipleEnumerator<T> : IEnumerator<T>
	{
		internal MultipleEnumerator(IEnumerable<IEnumerable<T>> collections)
		{
			index = 0; // Assuming that there is at least one enumerator, but not asserting this for performance.
			enumerators = collections.Select(e => e.GetEnumerator()).ToArray();
		}

		int index;
		readonly IEnumerator<T>[] enumerators;

		public T Current => CurrentEnumerator.Current;

		object IEnumerator.Current => Current;

		IEnumerator<T> CurrentEnumerator => enumerators[index];

		public void Dispose()
		{
			foreach (var enumerator in enumerators)
			{
				enumerator.Dispose();
			}
		}

		public bool MoveNext()
		{
			while (index < enumerators.Length)
			{
				if (CurrentEnumerator.MoveNext())
				{
					return true;
				}
				else
				{
					index++;
				}
			}

			return false;
		}

		public void Reset()
		{
			index = 0;
			foreach (var enumerator in enumerators)
			{
				enumerator.Reset();
			}
		}
	}
}
