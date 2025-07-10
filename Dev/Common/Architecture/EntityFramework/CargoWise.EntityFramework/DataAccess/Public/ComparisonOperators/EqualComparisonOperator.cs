using System;

namespace CargoWise.EntityFramework
{
	public class EqualComparisonOperator : ExactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		public EqualComparisonOperator() : base("=", true)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is the name of an operator")]
		public const string Is = "is";

		protected override string ComparisonTextCore(object value)
		{
			return ValueIsNull(value) ? Is : base.ComparisonTextCore(value);
		}

		protected override object ValueForLiteralADOCore(object value)
		{
			return value;
		}

		public override Func<T, bool> GetPredicate<T>(T value)
		{
			var valueString = value.ToString();
			return x => x.ToString().Equals(valueString, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
