using System.Collections;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public class GenericComparerWrapper<T> : IComparer
	{
		public GenericComparerWrapper(IComparer<T> comparer)
		{
			this.comparer = comparer;
		}

		internal readonly IComparer<T> comparer;

		public int Compare(object x, object y)
		{
			return comparer.Compare((T)x, (T)y);
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			GenericComparerWrapper<T> rhs = obj as GenericComparerWrapper<T>;
			return rhs != null && Equals(comparer, rhs.comparer);
		}

		public override int GetHashCode()
		{
			return comparer.GetHashCode() + 1;
		}

		#endregion
	}
}
