using System;

namespace Enterprise.Billing.StlCollector.Retriever
{
	class RecurringRange : BaseDateTimeRange
	{
		public RecurringRange(DateTime utcStartDateTimeInclusive, DateTime utcEndDateTimeExclusive)
			: base(utcStartDateTimeInclusive, utcEndDateTimeExclusive)
		{
			InitialiseRangesIfApplicable(
				utcStartDateTimeInclusive,
				utcEndDateTimeExclusive,
				out monthlyRangeStartDate,
				out monthlyRangeEndDateExclusive,
				out dailyRangeStartInclusive,
				out dailyRangeEndInclusive
			);
		}

		void InitialiseRangesIfApplicable(DateTime utcStartDateTimeInclusive, DateTime utcEndDateTimeExclusive, out DateTime? ausydBomInUtc, out DateTime? ausydNextBomInUtc, out DateTime? dailyRangeStartInclusive, out DateTime? dailyRangeEndInclusive)
		{
			ausydBomInUtc = null;
			ausydNextBomInUtc = null;
			dailyRangeStartInclusive = null;
			dailyRangeEndInclusive = null;

			if (IsValid)
			{
				var ausydStartDateTimeInclusive = GetBillingDateTimeFromUtc(utcStartDateTimeInclusive);
				var ausydEndDateTimeExclusive = GetBillingDateTimeFromUtc(utcEndDateTimeExclusive);

				if (ausydStartDateTimeInclusive.Date != ausydEndDateTimeExclusive.Date)
				{
					var utcAtAusydDateChange = GetUtcFromBillingDateTime(ausydEndDateTimeExclusive.Date);
					dailyRangeStartInclusive = DateTime.SpecifyKind(utcAtAusydDateChange.AddDays(-1), DateTimeKind.Utc);
					dailyRangeEndInclusive = DateTime.SpecifyKind(utcAtAusydDateChange.AddTicks(-1), DateTimeKind.Utc);

					if (ausydStartDateTimeInclusive.Month != ausydEndDateTimeExclusive.Month)
					{
						var ausydBom = new DateTime(ausydStartDateTimeInclusive.Year, ausydStartDateTimeInclusive.Month, 1);
						var ausydNextBom = ausydBom.AddMonths(1);

						ausydBomInUtc = GetUtcFromBillingDateTime(ausydBom);
						ausydNextBomInUtc = GetUtcFromBillingDateTime(ausydNextBom);
					}
				}
			}
		}

		protected override bool ValidateRangeSpecific()
		{
			return (
				// Recurring Range must not be greater than a day.
				// The collection cycle and daily milestone information rely on it.
				collectionStartDateTimeInclusive.AddDays(1) >= collectionEndDateTimeExclusive
			);
		}

		public override DateTime? MonthlyRangeStartInclusive => monthlyRangeStartDate;
		readonly DateTime? monthlyRangeStartDate;

		public override DateTime? MonthlyRangeEndExclusive => monthlyRangeEndDateExclusive;
		readonly DateTime? monthlyRangeEndDateExclusive;

		public override DateTime? StlMilestoneTimestamp => dailyRangeEndInclusive;

		public override DateTime? DailyRangeStartInclusive => dailyRangeStartInclusive;
		readonly DateTime? dailyRangeStartInclusive;

		public override DateTime? DailyRangeEndInclusive => dailyRangeEndInclusive;
		readonly DateTime? dailyRangeEndInclusive;
	}
}
