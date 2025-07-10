using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ModuleDurationFilter))]
	public class ModuleDurationFilterTest : NonPersistentBusinessObjectTestCase
	{
		#region Query Tests

		BMBufferTimespan timespan1Hour;
		BMBufferTimespan timespan3Hours;
		BMBufferTimespan timespan6Hours;
		BMBufferTimespan timespan9Hours;
		BMBufferTimespan timespan24Hours;

		void SetUpTimespansForQueryTests()
		{
			timespan1Hour = Factory.NewWithValidTestData<BMBufferTimespan>();
			timespan1Hour.BMT_Name = "1 Hour Timespan";
			timespan1Hour.BMT_BufferTimespanInMinutes = 60;

			timespan3Hours = Factory.NewWithValidTestData<BMBufferTimespan>();
			timespan3Hours.BMT_Name = "3 Hour Timespan";
			timespan3Hours.BMT_BufferTimespanInMinutes = 180;

			timespan6Hours = Factory.NewWithValidTestData<BMBufferTimespan>();
			timespan6Hours.BMT_Name = "6 Hour Timespan";
			timespan6Hours.BMT_BufferTimespanInMinutes = 360;

			timespan9Hours = Factory.NewWithValidTestData<BMBufferTimespan>();
			timespan9Hours.BMT_Name = "9 Hour Timespan";
			timespan9Hours.BMT_BufferTimespanInMinutes = 540;

			timespan24Hours = Factory.NewWithValidTestData<BMBufferTimespan>();
			timespan24Hours.BMT_Name = "24 Hour Timespan";
			timespan24Hours.BMT_BufferTimespanInMinutes = 1440;

			Factory.Save();
		}

		public void TestBetweenRange_ShouldMatchTimespansWithinRange()
		{
			SetUpTimespansForQueryTests();

			var filter = new ModuleDurationFilter(ProcessHeader.ModuleFilterConstants.BufferTimespan, Factory);
			filter.IsActive = true;
			filter.Scope = ModuleDurationFilter.SearchTexts.Between;

			filter.SetQueryDelegate((minMinutes, maxMinutes, scope) => {
				var query = new ZDBOnlyQuery(typeof(BMBufferTimespan));

				int minValue = (int)minMinutes;
				int maxValue = (int)maxMinutes;

				if (scope == ModuleDurationFilter.SearchTexts.Between.ToString())
				{
					query.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes,
									SQLComparisonOperator.GreaterThanOrEqualTo,
									minValue);
					query.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes,
									SQLComparisonOperator.LessThanOrEqualTo,
									maxValue);
				}
				else if (scope == ModuleDurationFilter.SearchTexts.GreaterThanOrEqualTo.ToString())
				{
					query.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes,
									SQLComparisonOperator.GreaterThanOrEqualTo,
									minValue);
				}
				else if (scope == ModuleDurationFilter.SearchTexts.LessThanOrEqualTo.ToString())
				{
					query.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes,
									SQLComparisonOperator.LessThanOrEqualTo,
									minValue);
				}
				else if (scope == ModuleDurationFilter.SearchTexts.EqualTo.ToString())
				{
					query.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes,
									SQLComparisonOperator.Equal,
									minValue);
				}

				return query;
			});

			SetTimeValuesFromMinutesForTest(filter, 120, 420);

			var query = filter.Query;
			var result = Factory.Load<BMBufferTimespan>(query);

			AssertContainsExactElementsInAnyOrder(
				"Only timespans between 2 and 7 hours should match",
				new[] { timespan3Hours, timespan6Hours },
				result);
		}

		public void TestGreaterThanOrEqualTo_ShouldMatchTimespansAboveThreshold()
		{
			SetUpTimespansForQueryTests();

			var filter = new ModuleDurationFilter(ProcessHeader.ModuleFilterConstants.BufferTimespan, Factory);
			filter.IsActive = true;
			filter.Scope = ModuleDurationFilter.SearchTexts.GreaterThanOrEqualTo;

			filter.SetQueryDelegate((minMinutes, maxMinutes, scope) => {
				var query = new ZDBOnlyQuery(typeof(BMBufferTimespan));

				int minValue = (int)minMinutes;

				if (scope == ModuleDurationFilter.SearchTexts.GreaterThanOrEqualTo.ToString())
				{
					query.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes,
									SQLComparisonOperator.GreaterThanOrEqualTo,
									minValue);
				}

				return query;
			});

			SetTimeValuesFromMinutesForTest(filter, 360, 0);

			var query = filter.Query;
			var result = Factory.Load<BMBufferTimespan>(query);

			AssertContainsExactElementsInAnyOrder(
				"Only timespans 6 hours or greater should match",
				new[] { timespan6Hours, timespan9Hours, timespan24Hours },
				result);
		}

		public void TestLessThanOrEqualTo_ShouldMatchTimespansBelowThreshold()
		{
			SetUpTimespansForQueryTests();

			var filter = new ModuleDurationFilter(ProcessHeader.ModuleFilterConstants.BufferTimespan, Factory);
			filter.IsActive = true;
			filter.Scope = ModuleDurationFilter.SearchTexts.LessThanOrEqualTo;

			filter.SetQueryDelegate((minMinutes, maxMinutes, scope) => {
				var query = new ZDBOnlyQuery(typeof(BMBufferTimespan));

				int minValue = (int)minMinutes;

				if (scope == ModuleDurationFilter.SearchTexts.LessThanOrEqualTo.ToString())
				{
					query.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes,
									SQLComparisonOperator.LessThanOrEqualTo,
									minValue);
				}

				return query;
			});

			SetTimeValuesFromMinutesForTest(filter, 360, 0);

			var query = filter.Query;
			var result = Factory.Load<BMBufferTimespan>(query);

			AssertContainsExactElementsInAnyOrder(
				"Only timespans 6 hours or less should match",
				new[] { timespan1Hour, timespan3Hours, timespan6Hours },
				result);
		}

		public void TestEqualTo_ShouldMatchTimespansEqualToValue()
		{
			SetUpTimespansForQueryTests();

			var filter = new ModuleDurationFilter(ProcessHeader.ModuleFilterConstants.BufferTimespan, Factory);
			filter.IsActive = true;
			filter.Scope = ModuleDurationFilter.SearchTexts.EqualTo;

			filter.SetQueryDelegate((minMinutes, maxMinutes, scope) => {
				var query = new ZDBOnlyQuery(typeof(BMBufferTimespan));

				int minValue = (int)minMinutes;

				if (scope == ModuleDurationFilter.SearchTexts.EqualTo.ToString())
				{
					query.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes,
									SQLComparisonOperator.Equal,
									minValue);
				}

				return query;
			});

			SetTimeValuesFromMinutesForTest(filter, 360, 0);

			var query = filter.Query;
			var result = Factory.Load<BMBufferTimespan>(query);

			AssertContainsExactElementsInAnyOrder(
				"Only timespans equal to 6 hours should match",
				new[] { timespan6Hours },
				result);
		}

		public void TestDecimalPrecision_ShouldHandleCorrectly()
		{
			var timespan1Hour30Min = Factory.NewWithValidTestData<BMBufferTimespan>();
			timespan1Hour30Min.BMT_Name = "1.5 Hour Timespan";
			timespan1Hour30Min.BMT_BufferTimespanInMinutes = 90;

			var timespan2Hour45Min = Factory.NewWithValidTestData<BMBufferTimespan>();
			timespan2Hour45Min.BMT_Name = "2.75 Hour Timespan";
			timespan2Hour45Min.BMT_BufferTimespanInMinutes = 165;

			Factory.Save();

			var filter = new ModuleDurationFilter(ProcessHeader.ModuleFilterConstants.BufferTimespan, Factory);
			filter.IsActive = true;
			filter.Scope = ModuleDurationFilter.SearchTexts.Between;

			filter.SetQueryDelegate((minMinutes, maxMinutes, scope) => {
				var query = new ZDBOnlyQuery(typeof(BMBufferTimespan));

				int minValue = (int)minMinutes;
				int maxValue = (int)maxMinutes;

				if (scope == ModuleDurationFilter.SearchTexts.Between.ToString())
				{
					query.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes,
									SQLComparisonOperator.GreaterThanOrEqualTo,
									minValue);
					query.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes,
									SQLComparisonOperator.LessThanOrEqualTo,
									maxValue);
				}

				return query;
			});

			SetTimeValuesFromMinutesForTest(filter, 75, 150);

			var query = filter.Query;
			var result = Factory.Load<BMBufferTimespan>(query);

			AssertContainsExactElementsInAnyOrder(
				"Should find timespan with precise decimal values",
				new[] { timespan1Hour30Min },
				result);
		}

		public void TestColumnBoundFilter_WithMinutesColumn()
		{
			SetUpTimespansForQueryTests();

			var filter = new ModuleDurationFilter("TimespanFilter", Factory,
				BMBufferTimespanSchema.BMT_BufferTimespanInMinutes);

			filter.IsActive = true;
			filter.Scope = ModuleDurationFilter.SearchTexts.Between;

			SetTimeValuesFromMinutesForTest(filter, 120, 420);

			var query = filter.Query;
			var result = Factory.Load<BMBufferTimespan>(query);

			AssertContainsExactElementsInAnyOrder(
				"Column-bound filter should work with minutes column",
				new[] { timespan3Hours, timespan6Hours },
				result);
		}

		#endregion

		#region Serialization Tests

		public void TestSerialisation()
		{
			var serialisedFilter = new ModuleDurationFilter(ProcessHeader.ModuleFilterConstants.DurationRangeFilter, Factory);
			serialisedFilter.IsActive = true;
			serialisedFilter.Scope = ModuleDurationFilter.SearchTexts.Between;
			SetTimeValuesFromMinutesForTest(serialisedFilter, 60, 480);

			var deserialisedFilter = new ModuleDurationFilter(ProcessHeader.ModuleFilterConstants.DurationRangeFilter, Factory);
			BMSTestHelper.SerialiseAndDeSerialise(serialisedFilter, deserialisedFilter);

			AssertEquals(ModuleDurationFilter.SearchTexts.Between, deserialisedFilter.Scope);
			AssertEquals(60m, deserialisedFilter.MinDurationMinutes);
			AssertEquals(480m, deserialisedFilter.MaxDurationMinutes);
		}

		#endregion

		#region Validation Tests

		public void TestScopeValidation()
		{
			var filter = new ModuleDurationFilter(ProcessHeader.ModuleFilterConstants.DurationRangeFilter, Factory);
			filter.Scope = ModuleDurationFilter.SearchTexts.Between;
			filter.IsActive = true;

			filter.Validation.ValidateAll();
			AssertNoErrors(filter);

			filter.Scope = "INVALID";
			AssertHasError(filter.ScopeInfo, "Enter a valid selection.");

			filter.Scope = "";
			AssertHasError(filter.ScopeInfo, "Please enter a value.");
		}

		public void TestDurationValidation()
		{
			var filter = new ModuleDurationFilter(ProcessHeader.ModuleFilterConstants.DurationRangeFilter, Factory);
			filter.IsActive = true;
			filter.Scope = ModuleDurationFilter.SearchTexts.Between;

			SetTimeValuesFromMinutesForTest(filter, -10, 100);
			filter.Validation.ValidateMinDurationMinutes();
			AssertHasError(filter.MinDurationMinutesInfo, "Please enter a value greater than or equal to 0.");

			SetTimeValuesFromMinutesForTest(filter, 100, 50);
			filter.Validation.ValidateMinDurationMinutes();
			AssertHasErrors("Min duration should be less than or equal to max duration", filter.MinDurationMinutesInfo);

			SetTimeValuesFromMinutesForTest(filter, 50, 100);
			filter.Validation.ValidateAll();
			AssertNoErrors(filter);
		}

		#endregion

		#region helper

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		void SetTimeValuesFromMinutesForTest(ModuleDurationFilter filter, decimal minMinutes, decimal maxMinutes)
		{
			filter.MinDurationMinutes = minMinutes;
			filter.MaxDurationMinutes = maxMinutes;
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ModuleDurationFilter(ProcessHeader.ModuleFilterConstants.DurationRangeFilter, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
		}

		#endregion
	}
}
