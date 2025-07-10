using System.Collections;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public class GenericIEnumerableAdapter<T> : IEnumerable<T>
	{
		public GenericIEnumerableAdapter(IEnumerable innerEnumerable)
		{
			this.InnerEnumerable = innerEnumerable;
		}

		readonly IEnumerable InnerEnumerable;

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			foreach (T element in InnerEnumerable)
			{
				yield return element;
			}
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
