using System;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever
{
	sealed class RecurringOnlyNoMaximumSizeRangeForTransactionalGrainScriptTesting : BaseDateTimeRange
	{
		public RecurringOnlyNoMaximumSizeRangeForTransactionalGrainScriptTesting(DateTime utcStartDateTimeInclusive, DateTime utcEndDateTimeExclusive)
			: base(utcStartDateTimeInclusive, utcEndDateTimeExclusive)
		{
		}

		protected override bool ValidateRangeSpecific()
		{
			return true;
		}

		protected override bool ScriptResultsShouldBeSubmittedCore(IStlScriptWithConfig swc)
		{
			if (swc.Script.StlGrain != StlDataGrain.Transactional)
			{
				throw new InvalidOperationException("RecurringOnlyNoMaximumSizeRangeForTransactionalGrainScriptTesting can be only used for transactional grain scripts.");
			}

			return base.ScriptResultsShouldBeSubmittedCore(swc);
		}

		public override DateTime? MonthlyRangeStartInclusive => null;
		public override DateTime? MonthlyRangeEndExclusive => null;
		public override DateTime? StlMilestoneTimestamp => null;
		public override DateTime? DailyRangeStartInclusive => null;
		public override DateTime? DailyRangeEndInclusive => null;
	}
}
