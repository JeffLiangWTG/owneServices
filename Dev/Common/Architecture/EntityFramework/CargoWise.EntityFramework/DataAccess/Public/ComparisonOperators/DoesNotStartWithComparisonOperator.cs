using System;

namespace CargoWise.EntityFramework
{
	public sealed class DoesNotStartWithComparisonOperator : InexactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "checking for sql operators")]
		public DoesNotStartWithComparisonOperator() : base("not like", "", "%")
		{
		}

		public override Func<T, bool> GetPredicate<T>(T value)
		{
			var valueString = value.ToString();
			return x => !x.ToString().StartsWith(valueString, StringComparison.CurrentCultureIgnoreCase);
		}
	}
}
