using System;

namespace CargoWise.EntityFramework
{
	public sealed class LessThanOrEqualToComparisonOperator : ExactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		public LessThanOrEqualToComparisonOperator() : base("<=", false)
		{
		}

		public override Func<T, bool> GetPredicate<T>(T value)
		{
			return x => AsIComparable(x).CompareTo(value) <= 0;
		}
	}
}
