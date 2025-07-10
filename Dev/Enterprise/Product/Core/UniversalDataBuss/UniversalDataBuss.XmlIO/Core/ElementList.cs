using System.Collections;
using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.XmlIO
{
	class ElementList<T> : IEnumerable<T> where T : Element
	{
		internal ElementList()
		{
			this.internalList = new List<T>();
		}

		readonly List<T> internalList;

		internal void Add(T elementConverter)
		{
			internalList.Add(elementConverter);
		}

		internal void Sort()
		{
			internalList.Sort((x, y) => { return string.Compare(x.KeyForSorting, y.KeyForSorting); });
		}

		#region IEnumerable Implementation

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return internalList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return internalList.GetEnumerator();
		}

		#endregion
	}
}
