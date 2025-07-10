using System;

namespace CargoWise.EntityFramework
{
	public sealed class StartsWithComparisonOperator : InexactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "sql operator")]
		public StartsWithComparisonOperator() : base("like", "", "%")
		{
		}

		public override Func<T, bool> GetPredicate<T>(T value)
		{
			var valueString = value.ToString();
			return x => x.ToString().StartsWith(valueString, StringComparison.CurrentCultureIgnoreCase);
		}
	}
}
