using System;

namespace CargoWise.EntityFramework
{
	public sealed class GreaterThanComparisonOperator : ExactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		public GreaterThanComparisonOperator() : base(">", false)
		{
		}

		public override Func<T, bool> GetPredicate<T>(T value)
		{
			return x => AsIComparable(x).CompareTo(value) > 0;
		}
	}
}
