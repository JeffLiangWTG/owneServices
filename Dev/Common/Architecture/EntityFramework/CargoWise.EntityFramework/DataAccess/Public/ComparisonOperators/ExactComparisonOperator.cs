namespace CargoWise.EntityFramework
{
	public abstract class ExactComparisonOperator : SQLComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		public ExactComparisonOperator(string comparisonText, bool supportsNullComparison) : base(comparisonText, supportsNullComparison, "", "")
		{
		}

		public override bool IsLike
		{
			get { return false; }
		}
	}
}
