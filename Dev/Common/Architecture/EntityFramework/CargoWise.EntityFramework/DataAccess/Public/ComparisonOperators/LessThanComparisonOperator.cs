using System;

namespace CargoWise.EntityFramework
{
	public sealed class LessThanComparisonOperator : ExactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		public LessThanComparisonOperator() : base("<", false)
		{
		}

		public override Func<T, bool> GetPredicate<T>(T value)
		{
			return x => AsIComparable(x).CompareTo(value) < 0;
		}
	}
}
