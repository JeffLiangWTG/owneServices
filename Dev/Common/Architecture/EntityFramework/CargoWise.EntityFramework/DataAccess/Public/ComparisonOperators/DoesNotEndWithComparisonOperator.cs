using System;

namespace CargoWise.EntityFramework
{
	public sealed class DoesNotEndWithComparisonOperator : InexactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "sql operator literals")]
		public DoesNotEndWithComparisonOperator() : base("not like", "%", "")
		{
		}

		public override Func<T, bool> GetPredicate<T>(T value)
		{
			var valueString = value.ToString();
			return x => !x.ToString().EndsWith(valueString, StringComparison.CurrentCultureIgnoreCase);
		}
	}
}
