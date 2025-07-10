using System;

namespace CargoWise.EntityFramework
{
	public sealed class GreaterThanOrEqualToComparisonOperator : ExactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		public GreaterThanOrEqualToComparisonOperator() : base(">=", false)
		{
		}

		public override Func<T, bool> GetPredicate<T>(T value)
		{
			return x => AsIComparable(x).CompareTo(value) >= 0;
		}
	}
}
