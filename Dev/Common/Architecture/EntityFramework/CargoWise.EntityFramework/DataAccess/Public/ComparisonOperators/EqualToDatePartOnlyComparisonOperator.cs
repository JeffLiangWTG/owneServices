using System;

namespace CargoWise.EntityFramework
{
	public sealed class EqualToDatePartOnlyComparisonOperator : SQLComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		public EqualToDatePartOnlyComparisonOperator() : base("", true, "", "")
		{
		}

		protected override string ComparisonTextCore(object value)
		{
			ReportError();
			return string.Empty;
		}

		void ReportError()
		{
			throw new NotSupportedException("EqualToDatePartOnlyComparisonOperator should only be used as a flag to generate other comparison types");
		}

		protected override string EscapedADOValue(string value)
		{
			ReportError();
			return string.Empty;
		}

		public override bool IsLike
		{
			get
			{
				ReportError();
				return false;
			}
		}

		protected override object ValueForLiteralADOCore(object value)
		{
			ReportError();
			return null;
		}
	}
}
