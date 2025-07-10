using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Billing.StlCollector.Retriever.Testing;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Billing.StlCollector.Retriever
{
	sealed class RecurringRangeTest : TestCaseWithFactory
	{
		public void TestWithStartAndEndDatesInTheSameMonth()
		{
			var testRangeCalculator = new RecurringRange(new DateTime(2014, 6, 18), new DateTime(2014, 6, 18, 1, 0, 0));

			AssertEquals("IsRangeValid?", true, testRangeCalculator.IsValid);
			AssertEquals("MonthlyRangeStartDate.HasValue?", false, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("MonthlyRangeEndDateExclusive.HasValue?", false, testRangeCalculator.MonthlyRangeEndExclusive.HasValue);
			AssertEquals("StlMilestoneTimestamp.HasValue?", false, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StartDateTimeInclusive", new DateTime(2014, 6, 18), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2014, 6, 18, 1, 0, 0), testRangeCalculator.EndDateTimeExclusive);
			AssertEquals("Is transactional script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestWithStartDateAfterEndDate()
		{
			var utcToday = new DateTime(2014, 5, 4);
			var testRangeCalculator = new RecurringRange(utcToday, utcToday.AddHours(-1));

			AssertEquals("IsRangeValid?", false, testRangeCalculator.IsValid);
			AssertEquals("MonthlyRangeStartDate.HasValue?", false, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("MonthlyRangeEndDateExclusive.HasValue?", false, testRangeCalculator.MonthlyRangeEndExclusive.HasValue);
			AssertEquals("StlMilestoneTimestamp.HasValue?", false, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StartDateTimeInclusive", new DateTime(2014, 5, 4), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2014, 5, 3, 23, 0, 0), testRangeCalculator.EndDateTimeExclusive);
			AssertEquals("Is transactional script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestWhenStartAndEndInclusiveHoursAreTheSame()
		{
			var utcToday = new DateTime(2012, 8, 12);
			var testRangeCalculator = new RecurringRange(utcToday.AddHours(-2), utcToday.AddHours(-1));

			AssertEquals("IsRangeValid?", true, testRangeCalculator.IsValid);
			AssertEquals("MonthlyRangeStartDate.HasValue?", false, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("MonthlyRangeEndDateExclusive.HasValue?", false, testRangeCalculator.MonthlyRangeEndExclusive.HasValue);
			AssertEquals("StlMilestoneTimestamp.HasValue?", false, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StartDateTimeInclusive", new DateTime(2012, 8, 11, 22, 0, 0), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2012, 8, 11, 23, 0, 0), testRangeCalculator.EndDateTimeExclusive);
			AssertEquals("Is transactional script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestWhenStartDateTimeIsTheLastHourOfTheAusydMonth()
		{
			var utcNow = new DateTime(2014, 5, 1, 0, 0, 0);
			var testRangeCalculator = new RecurringRange(utcNow.AddHours(-11), utcNow.AddHours(-10));

			AssertEquals("IsRangeValid?", true, testRangeCalculator.IsValid);
			AssertEquals("MonthlyRangeStartDate.HasValue?", true, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("MonthlyRangeEndDateExclusive.HasValue?", true, testRangeCalculator.MonthlyRangeEndExclusive.HasValue);
			AssertEquals("StlMilestoneTimestamp.HasValue?", true, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StartDateTimeInclusive", new DateTime(2014, 4, 30, 13, 0, 0), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2014, 4, 30, 14, 0, 0), testRangeCalculator.EndDateTimeExclusive);
			AssertEquals("MonthlyRangeStartDate", new DateTime(2014, 3, 31, 13, 0, 0), testRangeCalculator.MonthlyRangeStartInclusive.Value);
			AssertEquals("MonthlyRangeEndDateExclusive", new DateTime(2014, 4, 30, 14, 0, 0), testRangeCalculator.MonthlyRangeEndExclusive.Value);
			AssertEquals("StlMilestoneTimestamp", testRangeCalculator.EndDateTimeExclusive.AddTicks(-1), testRangeCalculator.StlMilestoneTimestamp.Value);
			AssertEquals("StlMilestoneTimestamp Kind", DateTimeKind.Utc, testRangeCalculator.StlMilestoneTimestamp.Value.Kind);
			AssertEquals("Is transactional script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestWhenStartAndEndAreInSubsequentAusydMonthsInDifferentYears()
		{
			DateTime ausydYearBoundary = new DateTime(2012, 1, 1);
			var testRangeCalculator = new RecurringRange(ausydYearBoundary.AddHours(-12), ausydYearBoundary.AddHours(-11));

			AssertEquals("IsRangeValid?", true, testRangeCalculator.IsValid);
			AssertEquals("MonthlyRangeStartDate.HasValue?", true, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("StlMilestoneTimestamp.HasValue?", true, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StartDateTimeInclusive", new DateTime(2011, 12, 31, 12, 0, 0), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2011, 12, 31, 13, 0, 0), testRangeCalculator.EndDateTimeExclusive);
			AssertEquals("MonthlyRangeStartDate", new DateTime(2011, 11, 30, 13, 0, 0), testRangeCalculator.MonthlyRangeStartInclusive.Value);
			AssertEquals("MonthlyRangeEndDateExclusive", new DateTime(2011, 12, 31, 13, 0, 0), testRangeCalculator.MonthlyRangeEndExclusive.Value);
			AssertEquals("StlMilestoneTimestamp", testRangeCalculator.EndDateTimeExclusive.AddTicks(-1), testRangeCalculator.StlMilestoneTimestamp.Value);
			AssertEquals("StlMilestoneTimestamp Kind", DateTimeKind.Utc, testRangeCalculator.StlMilestoneTimestamp.Value.Kind);
			AssertEquals("Is transactional script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestWithVeryOldStartDate()
		{
			var utcToday = new DateTime(2013, 10, 22);
			var testRangeCalculator = new RecurringRange(utcToday.AddDays(-70), utcToday.AddDays(-1));

			AssertEquals("IsRangeValid?", false, testRangeCalculator.IsValid);
			AssertEquals("MonthlyRangeStartDate.HasValue?", false, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("StlMilestoneTimestamp.HasValue?", false, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StartDateTimeInclusive", new DateTime(2013, 8, 13), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2013, 10, 21), testRangeCalculator.EndDateTimeExclusive);
			AssertEquals("Is transactional script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestWithVeryVeryOldStartDate()
		{
			var utcToday = new DateTime(2010, 8, 19);
			var testRangeCalculator = new RecurringRange(DateTime.MinValue, utcToday.AddDays(-1));

			AssertEquals("IsRangeValid?", false, testRangeCalculator.IsValid);
			AssertEquals("MonthlyRangeStartDate.HasValue?", false, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("StlMilestoneTimestamp.HasValue?", false, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StartDateTimeInclusive", new DateTime(1, 1, 1), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2010, 8, 18), testRangeCalculator.EndDateTimeExclusive);
			AssertEquals("Is transactional script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestWhenStartAndEndDatesAreInDifferentUtcMonthsButTheSameAusydMonth()
		{
			var testRangeCalculator = new RecurringRange(new DateTime(2013, 4, 30, 23, 0, 0), new DateTime(2013, 5, 1));

			AssertEquals("IsRangeValid?", true, testRangeCalculator.IsValid);
			AssertEquals("MonthlyRangeStartDate.HasValue?", false, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("MonthlyRangeEndDateExclusive.HasValue?", false, testRangeCalculator.MonthlyRangeEndExclusive.HasValue);
			AssertEquals("StlMilestoneTimestamp.HasValue?", false, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StartDateTimeInclusive", new DateTime(2013, 4, 30, 23, 0, 0), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2013, 5, 1, 0, 0, 0), testRangeCalculator.EndDateTimeExclusive);
			AssertEquals("Is transactional script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestWhenStartAndEndDatesAreInDifferentAusydMonths()
		{
			var testRangeCalculator = new RecurringRange(new DateTime(2016, 2, 29, 12, 30, 0), new DateTime(2016, 2, 29, 13, 30, 0));

			AssertEquals("IsRangeValid?", true, testRangeCalculator.IsValid);
			AssertEquals("MonthlyRangeStartDate.HasValue?", true, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("MonthlyRangeEndDateExclusive.HasValue?", true, testRangeCalculator.MonthlyRangeEndExclusive.HasValue);
			AssertEquals("StlMilestoneTimestamp.HasValue?", true, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StartDateTimeInclusive", new DateTime(2016, 2, 29, 12, 30, 0), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2016, 2, 29, 13, 30, 0), testRangeCalculator.EndDateTimeExclusive);
			AssertEquals("MonthlyRangeStartDate", new DateTime(2016, 1, 31, 13, 0, 0), testRangeCalculator.MonthlyRangeStartInclusive.Value);
			AssertEquals("MonthlyRangeEndDateExclusive", new DateTime(2016, 2, 29, 13, 0, 0), testRangeCalculator.MonthlyRangeEndExclusive.Value);
			AssertEquals("StlMilestoneTimestamp", new DateTime(2016, 2, 29, 13, 0, 0).AddTicks(-1), testRangeCalculator.StlMilestoneTimestamp.Value);
			AssertEquals("StlMilestoneTimestamp Kind", DateTimeKind.Utc, testRangeCalculator.StlMilestoneTimestamp.Value.Kind);
			AssertEquals("Is transactional script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestWhenStartAndEndDatesAreInDifferentCurrentAusydMonths()
		{
			DateTime utcPlusOneMonth = EnvProxy.Instance.Time.CurrentUtcDateTime.AddMonths(1);
			DateTime utcEnd = new DateTime(utcPlusOneMonth.Year, utcPlusOneMonth.Month, 1, 3, 33, 33);
			DateTime utcStart = utcEnd.AddDays(-1);
			var testRangeCalculator = new RecurringRange(utcStart, utcEnd);

			AssertEquals("IsRangeValid?", true, testRangeCalculator.IsValid);
			AssertEquals("MonthlyRangeStartDate.HasValue?", true, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("MonthlyRangeEndDateExclusive.HasValue?", true, testRangeCalculator.MonthlyRangeEndExclusive.HasValue);
			AssertEquals("StlMilestoneTimestamp.HasValue?", true, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StartDateTimeInclusive", utcStart, testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", utcEnd, testRangeCalculator.EndDateTimeExclusive);
			AssertEquals("MonthlyRangeStartDate", BaseDateTimeRange.GetUtcFromBillingDateTime(utcEnd.Date.AddMonths(-1)), testRangeCalculator.MonthlyRangeStartInclusive.Value);
			AssertEquals("MonthlyRangeEndDateExclusive", BaseDateTimeRange.GetUtcFromBillingDateTime(utcEnd.Date), testRangeCalculator.MonthlyRangeEndExclusive.Value);
			AssertEquals("StlMilestoneTimestamp", testRangeCalculator.MonthlyRangeEndExclusive.Value.AddTicks(-1), testRangeCalculator.StlMilestoneTimestamp.Value);
			AssertEquals("StlMilestoneTimestamp Kind", DateTimeKind.Utc, testRangeCalculator.StlMilestoneTimestamp.Value.Kind);
			AssertEquals("Is transactional script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestIsValidAndBoundaries()
		{
			var utcNow = DateTime.UtcNow;

			// 23 hour valid range.
			var twentyThreeHourRange = new RecurringRange(utcNow, utcNow.AddHours(23));
			AssertRangeIsValid(twentyThreeHourRange);

			// Invalid range, over one day.
			var overOneDayRange = new RecurringRange(utcNow, utcNow.AddHours(25));
			AssertEquals("IsValid?", false, overOneDayRange.IsValid);
			AssertEquals("StlMilestoneTimestamp.HasValue?", false, overOneDayRange.StlMilestoneTimestamp.HasValue);
			AssertEquals("MonthlyRangeStartDate.HasValue?", false, overOneDayRange.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("MonthlyRangeEndDateExclusive.HasValue?", false, overOneDayRange.MonthlyRangeEndExclusive.HasValue);

			// 1 hour valid range.
			var oneHourRange = new RecurringRange(utcNow, utcNow.AddHours(1));
			AssertRangeIsValid(oneHourRange);
		}

		void AssertRangeIsValid(RecurringRange range)
		{
			AssertEquals("IsValid?", true, range.IsValid);

			if (range.MonthlyRangeStartInclusive.HasValue)
			{
				AssertEquals("StlMilestoneTimestamp.HasValue?", true, range.StlMilestoneTimestamp.HasValue);
				AssertEquals("MonthlyRangeEndDateExclusive.HasValue?", true, range.MonthlyRangeEndExclusive.HasValue);
			}
			else
			{
				AssertEquals("MonthlyRangeEndDateExclusive.HasValue?", false, range.MonthlyRangeEndExclusive.HasValue);
			}
		}

		public void TestSelectApplicableScripts()
		{
			var dateTimeRange = new RecurringRange(new DateTime(2014, 8, 10), new DateTime(2014, 8, 10, 1, 0, 0));
			AssertEquals("[Date range within calendar month] Is transactional grain script applicable?", true, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range within calendar month] Is monthly grain allow historical script applicable?", false, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range within calendar month] Is monthly grain current only script applicable?", false, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());

			dateTimeRange = new RecurringRange(new DateTime(2014, 7, 31, 23, 0, 0), new DateTime(2014, 8, 1));
			AssertEquals("[Date range across two UTC calendar months, but same AUSYD month] Is transactional grain script applicable?", true, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range across two UTC calendar months, but same AUSYD month] Is monthly grain allow historical script applicable?", false, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range across two UTC calendar months, but same AUSYD month] Is monthly grain current only script applicable?", false, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());

			dateTimeRange = new RecurringRange(new DateTime(2014, 7, 31, 13, 0, 0), new DateTime(2014, 7, 31, 14, 0, 0));
			AssertEquals("[Date range across two AUSYD calendar months] Is transactional grain script applicable?", true, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range across two AUSYD calendar months] Is monthly grain allow historical script applicable?", true, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range across two AUSYD calendar months] Is monthly grain current only script applicable?", false, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());

			dateTimeRange = new RecurringRange(new DateTime(2009, 2, 28), new DateTime(2009, 2, 28));
			AssertEquals("[Empty date range] Is transactional grain script applicable?", false, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Empty date range] Is monthly grain allow historical script applicable?", false, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Empty date range] Is monthly grain current only script applicable?", false, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());

			DateTime utcPlusOneMonth = EnvProxy.Instance.Time.CurrentUtcDateTime.AddMonths(1);
			DateTime utcEnd = new DateTime(utcPlusOneMonth.Year, utcPlusOneMonth.Month, 1);
			DateTime utcStart = utcEnd.AddDays(-1);
			dateTimeRange = new RecurringRange(utcStart, utcEnd);
			AssertEquals("[Date range across two AUSYD calendar months and current] Is transactional grain script applicable?", true, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range across two AUSYD calendar months and current] Is monthly grain allow historical script applicable?", true, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range across two AUSYD calendar months and current] Is monthly grain current only script applicable?", true, dateTimeRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestSelectApplicableScriptsWithActualStlScripts()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var allScriptsWithConfig = new ScriptLoader().Load(Factory);
				var allStlScripts = allScriptsWithConfig.Select(t => t.Script);

				// Across AUSYD month boundary
				var loadedScripts = new RecurringRange(new DateTime(2015, 8, 31, 13, 0, 0), new DateTime(2015, 8, 31, 14, 0, 0)).SelectApplicableScripts(allScriptsWithConfig).Select(t => t.Script);

				AssertEquals("Were any scripts loaded?",
					true, loadedScripts.Any());
				AssertEquals("Are there any scripts where StlGrain = Transactional?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.Transactional));
				AssertEquals("Are there any scripts where StlGrain = MonthlyAllowHistoricalData?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyAllowHistoricalData));
				AssertEquals("Are there any scripts where StlGrain = MonthlyCurrentDataOnly?",
					false, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyCurrentDataOnly));

				// Within one AUSYD month
				loadedScripts = new RecurringRange(new DateTime(2015, 9, 1), new DateTime(2015, 9, 1, 1, 0, 0)).SelectApplicableScripts(allScriptsWithConfig).Select(t => t.Script);

				AssertEquals("Were any scripts loaded?",
					true, loadedScripts.Any());
				AssertEquals("Are there any scripts where StlGrain = Transactional?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.Transactional));
				AssertEquals("Are there any scripts where StlGrain = MonthlyAllowHistoricalData?",
					false, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyAllowHistoricalData));
				AssertEquals("Are there any scripts where StlGrain = MonthlyCurrentDataOnly?",
					false, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyCurrentDataOnly));

				// Across current AUSYD month boundary
				DateTime utcPlusOneMonth = EnvProxy.Instance.Time.CurrentUtcDateTime.AddMonths(1);
				DateTime utcEnd = new DateTime(utcPlusOneMonth.Year, utcPlusOneMonth.Month, 1);
				DateTime utcStart = utcEnd.AddDays(-1);
				loadedScripts = new RecurringRange(utcStart, utcEnd).SelectApplicableScripts(allScriptsWithConfig).Select(t => t.Script);

				AssertEquals("Were any scripts loaded?",
					true, loadedScripts.Any());
				AssertEquals("Are there any scripts where StlGrain = Transactional?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.Transactional));
				AssertEquals("Are there any scripts where StlGrain = MonthlyAllowHistoricalData?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyAllowHistoricalData));
				AssertEquals("Are there any scripts where StlGrain = MonthlyCurrentDataOnly?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyCurrentDataOnly));
			}
		}
	}
}
