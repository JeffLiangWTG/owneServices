using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	internal sealed class FunctorComparer<T> : IComparer<T>
	{
		public FunctorComparer(Comparison<T> comparison)
		{
			this.comparison = comparison;
		}

		public int Compare(T x, T y)
		{
			return comparison(x, y);
		}

		readonly Comparison<T> comparison;
	}
}
