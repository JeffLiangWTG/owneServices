using System;
using System.Collections;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public class TypedEnumerable<T> : IEnumerable<T>
	{
		public TypedEnumerable(IEnumerable list)
		{
			this.list = list;
		}

		readonly IEnumerable list;

		#region IEnumerable<T> Members

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new TypedEnumerator<T>(list);
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion
	}

	public sealed class TypedEnumerator<T> : IEnumerator<T>
	{
		public TypedEnumerator(IEnumerable list)
		{
			this.enumerator = list.GetEnumerator();
		}
		readonly IEnumerator enumerator;

		#region IEnumerator<T> Members

		public T Current
		{
			get { return (T)enumerator.Current; }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			IDisposable disposableEnumerator = enumerator as IDisposable;
			if (disposableEnumerator != null)
			{
				disposableEnumerator.Dispose();
			}
		}

		#endregion

		#region IEnumerator Members

		object IEnumerator.Current
		{
			get { return enumerator.Current; }
		}

		public bool MoveNext()
		{
			return enumerator.MoveNext();
		}

		public void Reset()
		{
			enumerator.Reset();
		}

		#endregion
	}
}
