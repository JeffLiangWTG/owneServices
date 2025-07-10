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
	sealed class AusydMonthRangeTest : TestCaseWithFactory
	{
		public void TestRange()
		{
			var testRangeCalculator = AusydMonthRange.New(2014, 6);

			AssertEquals("IsValid?", true, testRangeCalculator.IsValid);

			AssertEquals("StartDateTimeInclusive", new DateTime(2014, 5, 31, 14, 0, 0), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2014, 6, 30, 14, 0, 0), testRangeCalculator.EndDateTimeExclusive);

			AssertEquals("MonthlyRangeStartInclusive.HasValue?", true, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("MonthlyRangeEndExclusive.HasValue?", true, testRangeCalculator.MonthlyRangeEndExclusive.HasValue);
			AssertEquals("MonthlyRangeStartInclusive", testRangeCalculator.StartDateTimeInclusive, testRangeCalculator.MonthlyRangeStartInclusive.Value);
			AssertEquals("MonthlyRangeEndExclusive", testRangeCalculator.EndDateTimeExclusive, testRangeCalculator.MonthlyRangeEndExclusive.Value);

			AssertEquals("StlMilestoneTimestamp.HasValue?", true, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StlMilestoneTimestamp", testRangeCalculator.EndDateTimeExclusive.AddTicks(-1), testRangeCalculator.StlMilestoneTimestamp.Value);
			AssertEquals("StlMilestoneTimestamp Kind", DateTimeKind.Utc, testRangeCalculator.StlMilestoneTimestamp.Value.Kind);

			AssertEquals("Is transactional script applicable to range?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable to range?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable to range?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestRangeOverNewYear()
		{
			// Last month of the year
			var testRangeCalculator = AusydMonthRange.New(2011, 12);

			AssertEquals("IsValid?", true, testRangeCalculator.IsValid);

			AssertEquals("StartDateTimeInclusive", new DateTime(2011, 11, 30, 13, 0, 0), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2011, 12, 31, 13, 0, 0), testRangeCalculator.EndDateTimeExclusive);

			AssertEquals("MonthlyRangeStartInclusive.HasValue?", true, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("MonthlyRangeEndExclusive.HasValue?", true, testRangeCalculator.MonthlyRangeEndExclusive.HasValue);
			AssertEquals("MonthlyRangeStartInclusive", testRangeCalculator.StartDateTimeInclusive, testRangeCalculator.MonthlyRangeStartInclusive.Value);
			AssertEquals("MonthlyRangeEndExclusive", testRangeCalculator.EndDateTimeExclusive, testRangeCalculator.MonthlyRangeEndExclusive.Value);

			AssertEquals("StlMilestoneTimestamp.HasValue?", true, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StlMilestoneTimestamp", testRangeCalculator.EndDateTimeExclusive.AddTicks(-1), testRangeCalculator.StlMilestoneTimestamp.Value);
			AssertEquals("StlMilestoneTimestamp Kind", DateTimeKind.Utc, testRangeCalculator.StlMilestoneTimestamp.Value.Kind);

			AssertEquals("Is transactional script applicable to range?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable to range?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable to range?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());

			// First month of the year
			testRangeCalculator = AusydMonthRange.New(2012, 1);

			AssertEquals("IsValid?", true, testRangeCalculator.IsValid);

			AssertEquals("StartDateTimeInclusive", new DateTime(2011, 12, 31, 13, 0, 0), testRangeCalculator.StartDateTimeInclusive);
			AssertEquals("EndDateTimeExclusive", new DateTime(2012, 1, 31, 13, 0, 0), testRangeCalculator.EndDateTimeExclusive);

			AssertEquals("MonthlyRangeStartInclusive.HasValue?", true, testRangeCalculator.MonthlyRangeStartInclusive.HasValue);
			AssertEquals("MonthlyRangeEndExclusive.HasValue?", true, testRangeCalculator.MonthlyRangeEndExclusive.HasValue);
			AssertEquals("MonthlyRangeStartInclusive", testRangeCalculator.StartDateTimeInclusive, testRangeCalculator.MonthlyRangeStartInclusive.Value);
			AssertEquals("MonthlyRangeEndExclusive", testRangeCalculator.EndDateTimeExclusive, testRangeCalculator.MonthlyRangeEndExclusive.Value);

			AssertEquals("StlMilestoneTimestamp.HasValue?", true, testRangeCalculator.StlMilestoneTimestamp.HasValue);
			AssertEquals("StlMilestoneTimestamp", testRangeCalculator.EndDateTimeExclusive.AddTicks(-1), testRangeCalculator.StlMilestoneTimestamp.Value);
			AssertEquals("StlMilestoneTimestamp Kind", DateTimeKind.Utc, testRangeCalculator.StlMilestoneTimestamp.Value.Kind);

			AssertEquals("Is transactional script applicable to range?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly allow historical script applicable to range?", true, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("Is monthly current only script applicable to range?", false, testRangeCalculator.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestWithInvalidYearMonthDay()
		{
			var start = new DateTime(2020, 07, 27);
			var end = new DateTime(2020, 07, 26);
			AusydMonthRange testRangeCalculator = AusydMonthRange.NewByStartEnd(start, end);

			AssertNotNull("testRangeCalculator", testRangeCalculator);
			Assert("Should be invalid range", !testRangeCalculator.IsValid);
		}

		public void TestWithInvalidYearMonth()
		{
			AusydMonthRange testRangeCalculator = null;

			AssertExceptionThrown(
				"Should throw an exception with invalid year 99999.",
				typeof(ArgumentOutOfRangeException),
				"Year, Month, and Day parameters describe an un-representable DateTime.",
				() => testRangeCalculator = AusydMonthRange.New(99999, 1));

			AssertNull("testRangeCalculator", testRangeCalculator);

			AssertExceptionThrown(
				"Should throw an exception with invalid year -1.",
				typeof(ArgumentOutOfRangeException),
				"Year, Month, and Day parameters describe an un-representable DateTime.",
				() => testRangeCalculator = AusydMonthRange.New(-1, 1));

			AssertNull("testRangeCalculator", testRangeCalculator);

			AssertExceptionThrown(
				"Should throw an exception with invalid month 13.",
				typeof(ArgumentOutOfRangeException),
				"Year, Month, and Day parameters describe an un-representable DateTime.",
				() => testRangeCalculator = AusydMonthRange.New(2015, 13));

			AssertNull("testRangeCalculator", testRangeCalculator);

			AssertExceptionThrown(
				"Should throw an exception with invalid month 21.",
				typeof(ArgumentOutOfRangeException),
				"Year, Month, and Day parameters describe an un-representable DateTime.",
				() => testRangeCalculator = AusydMonthRange.New(2010, 21));

			AssertNull("testRangeCalculator", testRangeCalculator);
		}

		public void TestSelectApplicableScripts()
		{
			var dateRange = AusydMonthRange.New(2014, 8);
			AssertEquals("[Date range within calendar year] Is transactional script applicable?", true, dateRange.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range within calendar year] Is monthly allow historical script applicable?", true, dateRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range within calendar year] Is monthly current only script applicable?", false, dateRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());

			dateRange = AusydMonthRange.New(2013, 1);
			AssertEquals("[Date range over new year] Is transactional script applicable?", true, dateRange.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range over new year] Is monthly allow historical script applicable?", true, dateRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Date range over new year] Is monthly current only script applicable?", false, dateRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());

			DateTime utcNow = EnvProxy.Instance.Time.CurrentUtcDateTime;
			dateRange = AusydMonthRange.New(utcNow.Year, utcNow.Month);
			AssertEquals("[Current month range] Is transactional script applicable?", true, dateRange.SelectApplicableScripts(new ScriptWithConfig(new TestTransactionalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Current month range] Is monthly allow historical script applicable?", true, dateRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyAllowHistoricalStlItem(), new StlItemRegistrySettingsStub())).Any());
			AssertEquals("[Current month range] Is monthly current only script applicable?", true, dateRange.SelectApplicableScripts(new ScriptWithConfig(new TestMonthlyCurrentStlItem(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestSelectApplicableScriptsWithProductionStlScripts()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var allScriptsWithConfig = new ScriptLoader().Load(Factory);
				var allStlScripts = allScriptsWithConfig.Select(t => t.Script);

				// Mid-year
				var loadedScripts = AusydMonthRange.New(2015, 7).SelectApplicableScripts(allScriptsWithConfig).Select(t => t.Script);

				AssertEquals("Were any scripts loaded?",
					true, loadedScripts.Any());
				AssertEquals("Loaded script count",
					allStlScripts.Count(s => s.StlGrain != StlDataGrain.MonthlyCurrentDataOnly), loadedScripts.Count());
				AssertEquals("Are there any scripts where StlGrain = Transactional?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.Transactional));
				AssertEquals("Are there any scripts where StlGrain = MonthlyAllowHistoricalData?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyAllowHistoricalData));
				AssertEquals("Are there any scripts where StlGrain = MonthlyAllowHistoricalData?",
					false, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyCurrentDataOnly));

				// End of the year
				loadedScripts = AusydMonthRange.New(2012, 12).SelectApplicableScripts(allScriptsWithConfig).Select(t => t.Script);

				AssertEquals("Were any scripts loaded?",
					true, loadedScripts.Any());
				AssertEquals("Loaded script count",
					allStlScripts.Count(s => s.StlGrain != StlDataGrain.MonthlyCurrentDataOnly), loadedScripts.Count());
				AssertEquals("Are there any scripts where StlGrain = Transactional?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.Transactional));
				AssertEquals("Are there any scripts where StlGrain = MonthlyAllowHistoricalData?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyAllowHistoricalData));
				AssertEquals("Are there any scripts where StlGrain = MonthlyAllowHistoricalData?",
					false, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyCurrentDataOnly));

				// Beginning of the year
				loadedScripts = AusydMonthRange.New(2013, 1).SelectApplicableScripts(allScriptsWithConfig).Select(t => t.Script);

				AssertEquals("Were any scripts loaded?",
					true, loadedScripts.Any());
				AssertEquals("Loaded script count",
					allStlScripts.Count(s => s.StlGrain != StlDataGrain.MonthlyCurrentDataOnly), loadedScripts.Count());
				AssertEquals("Are there any scripts where StlGrain = Transactional?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.Transactional));
				AssertEquals("Are there any scripts where StlGrain = MonthlyAllowHistoricalData?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyAllowHistoricalData));
				AssertEquals("Are there any scripts where StlGrain = MonthlyAllowHistoricalData?",
					false, loadedScripts.Any(s => s.StlGrain == StlDataGrain.MonthlyCurrentDataOnly));

				// Current Month
				DateTime utcNow = EnvProxy.Instance.Time.CurrentUtcDateTime;
				loadedScripts = AusydMonthRange.New(utcNow.Year, utcNow.Month).SelectApplicableScripts(allScriptsWithConfig).Select(t => t.Script);

				AssertEquals("Were any scripts loaded?",
					true, loadedScripts.Any());
				AssertEquals("Loaded script count",
					allStlScripts.Count(), loadedScripts.Count());
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
