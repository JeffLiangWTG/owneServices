using System.Collections;
using System.Collections.Generic;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	/// <summary>
	/// Compares 2 document wrappers based only on the wrapped object,
	/// differences in the wrappers themselves are ignored.
	/// </summary>
	/// <typeparam name="T">The specific document wrapper type to compare.</typeparam>
	public class WrappedObjectEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
		where T : DocumentWrapper
	{
		public bool Equals(T x, T y)
		{
			if (x == null && y == null)
			{
				return true; // both null
			}

			if (x == null || y == null)
			{
				return false; // only one is null
			}

			return object.Equals(x.WrappedObject, y.WrappedObject);
		}

		public int GetHashCode(T obj)
		{
			return obj == null || obj.WrappedObject == null ? 0 : obj.WrappedObject.GetHashCode();
		}

		#region IEqualityComparer Members

		bool IEqualityComparer.Equals(object x, object y)
		{
			return Equals((T)x, (T)y);
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			return GetHashCode((T)obj);
		}

		#endregion
	}
}
