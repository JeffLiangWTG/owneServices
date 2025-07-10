using System;
using System.Collections.Generic;

namespace CargoWise.Common.Collections
{
	public class ZComparer<T> : Comparer<T>
	{
		public ZComparer(Comparison<T> comparison)
		{
			Argument.NotNull(comparison, nameof(comparison));
			this.comparison = comparison;
		}

		public override int Compare(T x, T y)
		{
			return comparison(x, y);
		}

		readonly Comparison<T> comparison;

		public static implicit operator ZComparer<T>(Comparison<T> comparison)
		{
			Argument.NotNull(comparison, nameof(comparison));
			return new ZComparer<T>(comparison);
		}
	}
}