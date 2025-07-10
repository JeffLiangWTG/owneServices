using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public class WeakTargetDelegateList<T> : IEnumerable<WeakTargetDelegate<T>> where T : class
	{
		[SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		public WeakTargetDelegateList()
		{
			if (!typeof(Delegate).IsAssignableFrom(typeof(T)))
			{
				throw new ArgumentException("T must be a delegate type", "T");
			}
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d")]
		public void Add(WeakTargetDelegate<T> d)
		{
			list.Add(d);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d")]
		public void Remove(WeakTargetDelegate<T> d)
		{
			list.Remove(d);
		}

		public void Clear()
		{
			list.Clear();
		}

		public bool IsEmpty
		{ get { return !GetEnumerator().MoveNext(); } }

		public T Delegate
		{
			get
			{
				Delegate result = null;
				foreach (WeakTargetDelegate<T> itemRef in this)
				{
					if (itemRef != null)
					{
						T item = itemRef.ToDelegate();
						result = System.Delegate.Combine(result, item as Delegate);
					}
				}
				return result as object as T;
			}
		}

		#region IEnumerable Members

		public IEnumerator<WeakTargetDelegate<T>> GetEnumerator()
		{
			for (int i = 0; i < list.Count; i++)
			{
				var handlerRef = list[i];
				T handler = handlerRef.ToDelegate();
				if (!handlerRef.IsAlive)
				{
					list.RemoveAt(i--);
				}
				else
				{
					yield return handler;
				}
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation

		readonly List<WeakTargetDelegate<T>> list = new List<WeakTargetDelegate<T>>();

		#endregion
	}
}
