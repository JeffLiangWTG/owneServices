using System;

namespace CargoWise.EntityFramework
{
	public sealed class NotContainsComparisonOperator : InexactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "sql operators")]
		public NotContainsComparisonOperator() : base("not like", "%", "%")
		{
		}

		public override Func<T, bool> GetPredicate<T>(T value)
		{
			var valueString = value.ToString();
			return x => x.ToString().IndexOf(valueString, StringComparison.CurrentCultureIgnoreCase) == -1;
		}
	}
}
