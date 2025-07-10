using System;

namespace Enterprise.Billing.StlCollector.Retriever
{
	class AusydMonthRange : BaseDateTimeRange
	{
		public static AusydMonthRange NewByStartEnd(DateTime startInclusive, DateTime endInclusive)
		{
			var endDateInclusiveAddOne = endInclusive.AddDays(1);
			var ausydEndDateExclusive = new DateTime(endDateInclusiveAddOne.Year, endDateInclusiveAddOne.Month, endDateInclusiveAddOne.Day);
			var utcStartDateTimeInclusive = GetUtcFromBillingDateTime(startInclusive);
			var utcEndDateTimeExclusive = GetUtcFromBillingDateTime(ausydEndDateExclusive);

			return new AusydMonthRange(utcStartDateTimeInclusive, utcEndDateTimeExclusive);
		}

		public static AusydMonthRange New(int year, int month)
		{
			var ausydStartDateInclusive = new DateTime(year, month, 1);
			var ausydEndDateExclusive = ausydStartDateInclusive.AddMonths(1);

			var utcStartDateTimeInclusive = GetUtcFromBillingDateTime(ausydStartDateInclusive);
			var utcEndDateTimeExclusive = GetUtcFromBillingDateTime(ausydEndDateExclusive);
			return new AusydMonthRange(utcStartDateTimeInclusive, utcEndDateTimeExclusive);
		}

		AusydMonthRange(DateTime startDateInclusive, DateTime endDateExclusive)
			: base(startDateInclusive, endDateExclusive)
		{
			stlMilestoneTimestamp = DateTime.SpecifyKind(endDateExclusive.AddTicks(-1), DateTimeKind.Utc);
		}

		/// <summary>
		/// Returns TRUE because
		///  * Constructor is private
		///  * Range boundaries are calculated on factory method based on year + month parameters.
		/// </summary>
		/// <returns>TRUE</returns>
		protected override bool ValidateRangeSpecific()
		{
			return true;
		}

		public override DateTime? MonthlyRangeStartInclusive => collectionStartDateTimeInclusive;
		public override DateTime? MonthlyRangeEndExclusive => collectionEndDateTimeExclusive;
		public override DateTime? StlMilestoneTimestamp => stlMilestoneTimestamp;
		public override DateTime? DailyRangeStartInclusive => collectionStartDateTimeInclusive;
		public override DateTime? DailyRangeEndInclusive => collectionEndDateTimeExclusive;

		readonly DateTime stlMilestoneTimestamp;
	}
}
