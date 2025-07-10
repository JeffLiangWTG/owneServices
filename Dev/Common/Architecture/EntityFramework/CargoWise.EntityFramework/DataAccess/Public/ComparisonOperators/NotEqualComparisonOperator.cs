using System;

namespace CargoWise.EntityFramework
{
	public class NotEqualComparisonOperator : ExactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		public NotEqualComparisonOperator() : base("<>", true)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "comparison operator")]
		public const string IsNot = "is not";

		protected override string ComparisonTextCore(object value)
		{
			return ValueIsNull(value) ? IsNot : base.ComparisonTextCore(value);
		}

		public override Func<T, bool> GetPredicate<T>(T value)
		{
			var valueString = value.ToString();
			return x => !x.ToString().Equals(valueString, StringComparison.CurrentCultureIgnoreCase);
		}
	}
}
