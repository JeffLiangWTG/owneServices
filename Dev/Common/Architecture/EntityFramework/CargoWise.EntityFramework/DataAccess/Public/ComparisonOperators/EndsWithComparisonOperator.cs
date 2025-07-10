using System;

namespace CargoWise.EntityFramework
{
	public sealed class EndsWithComparisonOperator : InexactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "checking for sql operators")]
		public EndsWithComparisonOperator() : base("like", "%", "")
		{
		}

		public override Func<T, bool> GetPredicate<T>(T value)
		{
			var valueString = value.ToString();
			return x => x.ToString().EndsWith(valueString, StringComparison.CurrentCultureIgnoreCase);
		}
	}
}
