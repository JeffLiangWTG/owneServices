using System.Collections;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public class ZIterator<T> : IEnumerable<T>
	{
		public ZIterator(T firstElement, params IEnumerable<T>[] innerEnumerables)
		{
			IsFirstElementSpecified = true;
			this.FirstElement = firstElement;
			this.InnerEnumerables = innerEnumerables;
		}

		public ZIterator(params IEnumerable<T>[] innerEnumerables)
		{
			this.InnerEnumerables = innerEnumerables;
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (IsFirstElementSpecified)
			{
				yield return FirstElement;
			}

			foreach (IEnumerable<T> innerEnumerable in InnerEnumerables)
			{
				foreach (T element in innerEnumerable)
				{
					yield return element;
				}
			}
		}

		readonly T FirstElement;
		readonly bool IsFirstElementSpecified;
		readonly IEnumerable<T>[] InnerEnumerables;

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
