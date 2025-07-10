using System;
using System.Reflection;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleDateTimeOffsetFilter))]
	sealed class ModuleDateTimeOffsetFilterTest : ModuleFilterTestCase<ModuleDateTimeOffsetFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#region Testing the Query results

		#region TestQueryWithSpecifiedDates

		[TestDate(2007, 1, 1)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestQueryWithSpecifiedDates()
		{
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var dateToFind = new ZDateTime(TestDateAttribute.Date);
			var dateToNotFind = new ZDateTime(TestDateAttribute.Date.AddDays(1));

			Dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(dateToFind);
			Dummy2.Z0_DateTimeOffset = new ZDateTimeOffset(dateToNotFind);
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			// to date only
			Filter.Property1 = dateToFind;
			Filter.Property2 = ZDateTime.Empty;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);

			// from date only
			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = dateToFind;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			// both to and from dates
			Filter.Property1 = dateToFind;
			Filter.Property2 = dateToFind;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			// both to and from dates (ranged check)
			Filter.Property1 = dateToFind.AddDays(-1);
			Filter.Property2 = dateToFind.AddDays(1);
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);

			Dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(dateToFind);
			Dummy2.Z0_DateTimeOffset = new ZDateTimeOffset(TestDateAttribute.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997));
			Factory.Save();

			Filter.Property1 = dateToFind;
			Filter.Property2 = dateToFind;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);

			Filter.Property1 = dateToFind.AddDays(1);
			Filter.Property2 = dateToFind.AddDays(1);
			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);
		}

		#endregion

		#region TestQueryUsingDateTimeRange

		[TestDate(2007, 1, 1, 1, 1, 1)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestQueryUsingDateTimeRange()
		{
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			ZDateTime dateTimeToFind = TestDateAttribute.Date;
			ZDateTime dateTimeToNotFind = TestDateAttribute.Date.AddSeconds(1);

			Dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(dateTimeToFind);
			Dummy2.Z0_DateTimeOffset = new ZDateTimeOffset(dateTimeToNotFind);
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			// to date only
			Filter.Property1 = dateTimeToFind;
			Filter.Property2 = ZDateTime.Empty;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);

			// from date only
			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = dateTimeToFind;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			// both to and from dates
			Filter.Property1 = dateTimeToFind;
			Filter.Property2 = dateTimeToFind;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			// both to and from dates (ranged check)
			Filter.Property1 = dateTimeToFind.AddSeconds(-1);
			Filter.Property2 = dateTimeToFind.AddSeconds(1);
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);
		}

		#endregion

		#region TestQueryUsingHasDateEntered

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestQueryUsingHasDateEntered()
		{
			Filter.PropertySearch = ModuleDateFilter.HasDateEntered;

			Dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(2009, 9, 1, 13, 35, 23, TimeSpan.FromHours(10));
			Dummy2.Z0_DateTimeOffset = ZDateTimeOffset.Empty;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			Filter.Property1 = new ZDateTime(2009, 9, 1, 13, 35, 23);
			Filter.Property2 = ZDateTime.Empty;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = new ZDateTime(2009, 9, 1, 13, 35, 23);
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestQueryUsingHasDateEnteredNullable()
		{
			Filter.PropertySearch = ModuleDateFilter.HasDateEntered;

			Dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(2009, 9, 1, 13, 35, 23, TimeSpan.FromHours(10));
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);
			Filter.Property1 = new ZDateTime(2009, 9, 1, 13, 35, 23);

			var originalValue = Filter.FilterColumn.IsNullable;

			try
			{
				typeof(SchemaDateTimeOffsetColumn)
					   .GetField("IsNullable", BindingFlags.Instance | BindingFlags.Public)
					   .SetValue(Filter.FilterColumn, false);

				dummies.Load(Filter.Query);
				AssertCollectionContains(Dummy1, dummies);
			}
			finally
			{
				typeof(SchemaDateTimeOffsetColumn)
					   .GetField("IsNullable", BindingFlags.Instance | BindingFlags.Public)
					   .SetValue(Filter.FilterColumn, originalValue);
			}
		}

		#endregion

		#region TestQueryUsingHasNoDateEntered

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestQueryUsingHasNoDateEntered()
		{
			Filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			Dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(2009, 9, 1, 13, 35, 23, TimeSpan.FromHours(10));
			Dummy2.Z0_DateTimeOffset = ZDateTimeOffset.Empty;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			Filter.Property1 = new ZDateTime(2009, 9, 1, 13, 35, 23);
			Filter.Property2 = ZDateTime.Empty;
			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);

			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = new ZDateTime(2009, 9, 1, 13, 35, 23);
			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);
		}

		#endregion

		#region TestQueryUsingHasNoDateEnteredUsingDataView

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestQueryUsingHasNoDateEnteredUsingDataView()
		{
			var rowFactory = Factory.GetType().GetProperty("RowFactory",
				BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Factory, null) as RowFactory;
			typeof(RowFactory).GetProperty("MaximumRowsBeforeUsingIndex").SetValue(rowFactory, 0, null);//To make sure the query will use dataview

			Filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(2009, 10, 1, 13, 35, 23, TimeSpan.FromHours(10));
			Dummy2.Z0_DateTimeOffset = ZDateTimeOffset.Empty;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);
		}

		#endregion

		#region TestQueryWithCommonDates

		[TestDate(2007, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestQueryWithCommonDates()
		{
			// eagle bill, tt club bill in US diff to AU tt club.

			ZDateTime today = TestDateAttribute.Date;
			var tomorrow = today.AddDays(1);
			var yesterday = today.AddDays(-1);
			var sevenDaysAgo = today.AddDays(-6);
			var sevenDaysFromNow = today.AddDays(6);
			var fourteenDaysAgo = today.AddDays(-13);
			var fourteenDaysFromNow = today.AddDays(13);
			var oneMonthAgo = today.AddMonths(-1);
			var oneMonthFromNow = today.AddMonths(1);
			var twoMonthsAgo = today.AddMonths(-2);
			var twoMonthsFromNow = today.AddMonths(2);
			var threeMonthsAgo = today.AddMonths(-3);
			var threeMonthsFromNow = today.AddMonths(3);
			var sixMonthsAgo = today.AddMonths(-6);
			var sixMonthsFromNow = today.AddMonths(6);
			var oneYearAgo = today.AddYears(-1);
			var oneYearFromNow = today.AddYears(1);

			AssertResultsUsingCurrentFilter(today, tomorrow, ModuleDateFilter.DateRangeSearchTexts.Today);

			AssertResultsUsingCurrentFilter(yesterday.AddHours(2), today, ModuleDateFilter.DateRangeSearchTexts.Yesterday);
			AssertResultsUsingCurrentFilter(sevenDaysAgo, sevenDaysAgo.AddDays(-1), ModuleDateFilter.DateRangeSearchTexts.Last7Days);
			AssertResultsUsingCurrentFilter(fourteenDaysAgo, fourteenDaysAgo.AddDays(-1), ModuleDateFilter.DateRangeSearchTexts.Last14Days);
			AssertResultsUsingCurrentFilter(oneMonthAgo, oneMonthAgo.AddDays(-1), ModuleDateFilter.DateRangeSearchTexts.LastMonth);
			AssertResultsUsingCurrentFilter(twoMonthsAgo, twoMonthsAgo.AddDays(-1), ModuleDateFilter.DateRangeSearchTexts.Last2Mths);
			AssertResultsUsingCurrentFilter(threeMonthsAgo, threeMonthsAgo.AddDays(-1), ModuleDateFilter.DateRangeSearchTexts.Last3Mths);
			AssertResultsUsingCurrentFilter(sixMonthsAgo, sixMonthsAgo.AddDays(-1), ModuleDateFilter.DateRangeSearchTexts.Last6Mths);
			AssertResultsUsingCurrentFilter(oneYearAgo, oneYearAgo.AddDays(-1), ModuleDateFilter.DateRangeSearchTexts.Last12Mths);

			AssertResultsUsingCurrentFilter(tomorrow, today, ModuleDateFilter.DateRangeSearchTexts.Tomorrow);
			AssertResultsUsingCurrentFilter(sevenDaysFromNow, sevenDaysFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next7Days);
			AssertResultsUsingCurrentFilter(fourteenDaysFromNow, fourteenDaysFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next14Days);
			AssertResultsUsingCurrentFilter(oneMonthFromNow, oneMonthFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.NextMonth);
			AssertResultsUsingCurrentFilter(twoMonthsFromNow, twoMonthsFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next2Mths);
			AssertResultsUsingCurrentFilter(threeMonthsFromNow, threeMonthsFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next3Mths);
			AssertResultsUsingCurrentFilter(sixMonthsFromNow, sixMonthsFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next6Mths);
			AssertResultsUsingCurrentFilter(oneYearFromNow, oneYearFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next12Mths);
		}

		#endregion

		#region TestQueryWithWeekAndMonthDateRanges

		[TestDate(2007, 1, 1)] // Monday
		[TestUtcOffset(10, 0, 0)]
		public void TestQueryWithWeekAndMonthDateRanges()
		{
			// this week / month
			var firstDayOfThisWeek = new ZDateTime(2006, 12, 31); // Sunday
			var lastDayOfThisWeek = new ZDateTime(2007, 1, 6); // Saturday
			var firstDayOfThisMonth = new ZDateTime(2007, 1, 1); // Sunday
			var lastDayOfThisMonth = new ZDateTime(2007, 1, 31); // Saturday

			// last week / month
			var firstDayOfLastWeek = new ZDateTime(2006, 12, 24); // Sunday
			var lastDayOfLastWeek = new ZDateTime(2006, 12, 30); // Saturday
			var firstDayOfLastMonth = new ZDateTime(2006, 12, 1);
			var lastDayOfLastMonth = new ZDateTime(2006, 12, 31);

			// next week / month
			var firstDayOfNextWeek = new ZDateTime(2007, 1, 7); // Sunday
			var lastDayOfNextWeek = new ZDateTime(2007, 1, 13); // Saturday
			var firstDayOfNextMonth = new ZDateTime(2007, 2, 1);
			var lastDayOfNextMonth = new ZDateTime(2007, 2, 28);

			AssertResultsUsingCurrentFilter(firstDayOfThisWeek, firstDayOfThisWeek.AddDays(-1), "This Week");
			AssertResultsUsingCurrentFilter(lastDayOfThisWeek, lastDayOfThisWeek.AddDays(1), "This Week");

			AssertResultsUsingCurrentFilter(firstDayOfLastWeek, firstDayOfLastWeek.AddDays(-1), "Last Week");
			AssertResultsUsingCurrentFilter(lastDayOfLastWeek, lastDayOfLastWeek.AddDays(1), "Last Week");

			//AssertResultsUsingCurrentFilter(firstDayOfThisMonth, firstDayOfThisMonth.AddDays(-1), "This Month");
			//AssertResultsUsingCurrentFilter(lastDayOfThisMonth, lastDayOfThisMonth.AddDays(1), "This Month");

			//AssertResultsUsingCurrentFilter(firstDayOfLastMonth, firstDayOfLastMonth.AddDays(-1), "Last Month");
			//AssertResultsUsingCurrentFilter(lastDayOfLastMonth, lastDayOfLastMonth.AddDays(1), "Last Month");

			AssertResultsUsingCurrentFilter(firstDayOfNextWeek, firstDayOfNextWeek.AddDays(-1), "Next Week");
			AssertResultsUsingCurrentFilter(lastDayOfNextWeek, lastDayOfNextWeek.AddDays(1), "Next Week");

			//AssertResultsUsingCurrentFilter(firstDayOfNextMonth, firstDayOfNextMonth.AddDays(-1), "Next Month");
			//AssertResultsUsingCurrentFilter(lastDayOfNextMonth, lastDayOfNextMonth.AddDays(1), "Next Month");
		}

		#endregion

		#region TestQueryUsingDateTimeRangeConvertToUTC

		[TestUtcOffset(+7, 0, 0)]
		public void TestQueryUsingDateTimeRangeConvertToUTC()
		{
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			Filter.ConvertFromLocalToUTC = true;
			Filter.Property1 = new ZDateTime(2011, 1, 28, 11, 0, 0); // 4:00 by UTC
			Filter.Property2 = new ZDateTime(2011, 1, 28, 13, 0, 0); // 6:00 by UTC

			AssertEquals(new ZDateTime(2011, 1, 28, 4, 0, 0), Filter.FromDate);
			AssertEquals(new ZDateTime(2011, 1, 28, 6, 0, 0), Filter.ToDate);

			Dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(2011, 1, 28, 10, 0, 0, TimeSpan.FromHours(7));
			Dummy2.Z0_DateTimeOffset = new ZDateTimeOffset(2011, 1, 28, 12, 0, 0, TimeSpan.FromHours(7));
			Dummy3.Z0_DateTimeOffset = new ZDateTimeOffset(2011, 1, 28, 14, 0, 0, TimeSpan.FromHours(7));
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			// to date only
			Filter.Property1 = new ZDateTime(2011, 1, 28, 11, 0, 0); // 4:00 by UTC
			Filter.Property2 = ZDateTime.Empty;
			dummies.Load(Filter.Query);
			AssertEquals(2, dummies.Count);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);
			AssertCollectionContains(Dummy3, dummies);

			// from date only
			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = new ZDateTime(2011, 1, 28, 13, 0, 0); // 6:00 by UTC
			dummies.Load(Filter.Query);
			AssertEquals(2, dummies.Count);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);
			AssertCollectionNotContains(Dummy3, dummies);

			// both to and from dates
			Filter.Property1 = new ZDateTime(2011, 1, 28, 11, 0, 0); // 4:00 by UTC
			Filter.Property2 = new ZDateTime(2011, 1, 28, 13, 0, 0); // 6:00 by UTC
			dummies.Load(Filter.Query);
			AssertEquals(1, dummies.Count);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);
			AssertCollectionNotContains(Dummy3, dummies);
		}

		#endregion

		#region TestTodayFilter

		[TestDate(2007, 1, 1, 1, 1, 1)]
		[TestUtcOffset(+7, 0, 0)]
		public void TestTodayFilter()
		{
			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			Filter.IsActive = true;

			var expectedFrom = new DateTime(2007, 1, 1, 0, 0, 0);
			var expectedTo = new DateTime(2007, 1, 2, 0, 0, 0, 0);

			var expectedFromUTC = new DateTime(2006, 12, 31, 17, 0, 0); // 24:00 - 7:00 = 17:00
			var expectedToUTC = new DateTime(2007, 1, 1, 16, 59, 59, 997); // 24:00 - 7:00 = 17:00

			Filter.ConvertFromLocalToUTC = false;
			AssertEquals("CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) >= #" + ConvertExpectedDateTimeOffset(expectedFrom) + " +07:00# and CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) < #" + ConvertExpectedDateTimeOffset(expectedTo) + " +07:00#", Filter.Query.LiteralTextADO);

			Filter.ConvertFromLocalToUTC = true;
			AssertEquals("CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) >= #" + ConvertExpectedDateTimeOffset(expectedFromUTC) + " +00:00# and CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) <= #" + ConvertExpectedDateTimeOffset(expectedToUTC) + " +00:00#", Filter.Query.LiteralTextADO);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Yesterday;
			Filter.IsActive = true;

			expectedFrom = new DateTime(2006, 12, 31, 0, 0, 0);
			expectedTo = new DateTime(2007, 1, 1, 0, 0, 0, 0);

			Filter.ConvertFromLocalToUTC = false;
			AssertEquals("CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) >= #" + ConvertExpectedDateTimeOffset(expectedFrom) + " +07:00# and CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) < #" + ConvertExpectedDateTimeOffset(expectedTo) + " +07:00#", Filter.Query.LiteralTextADO);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			Filter.IsActive = true;

			expectedFrom = new DateTime(2007, 1, 2, 0, 0, 0);
			expectedTo = new DateTime(2007, 1, 3, 0, 0, 0, 0);

			Filter.ConvertFromLocalToUTC = false;
			AssertEquals("CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) >= #" + ConvertExpectedDateTimeOffset(expectedFrom) + " +07:00# and CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) < #" + ConvertExpectedDateTimeOffset(expectedTo) + " +07:00#", Filter.Query.LiteralTextADO);
		}

		string ConvertExpectedDateTimeOffset(DateTimeOffset dateTime)
		{
			return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
		}

		#endregion

		void AssertResultsUsingCurrentFilter(ZDateTime dateToFind, ZDateTime dateToNotFind, ZString propertySearchText)
		{
			Filter.PropertySearch = propertySearchText;
			Dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(dateToFind);
			Dummy2.Z0_DateTimeOffset = new ZDateTimeOffset(dateToNotFind);
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			dummies.Load(Filter.Query);

			AssertCollectionContains(Dummy1.PK, dummies.Select(x => x.PK));
			AssertCollectionNotContains(Dummy2.PK, dummies.Select(x => x.PK));
		}

		#endregion

		#region Future/Past Query

		[TestDate(2014, 6, 10)]
		[TestUtcOffset(10, 0, 0)]
		public void TestQuery_FutureAndPast()
		{
			var bizoInPast = Factory.New<DummyBusinessObject>();
			var bizoInPresent = Factory.New<DummyBusinessObject>();
			var bizoInFuture = Factory.New<DummyBusinessObject>();

			bizoInPast.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-1);
			bizoInPresent.Z0_DateTimeOffset = ZDateTimeOffset.Now;
			bizoInFuture.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(1);

			var filter = new ModuleDateTimeOffsetFilter("Dat Date", DummyBizoSchema.Z0_DateTimeOffset);
			filter.PropertySearch = ModuleDateFilter.Future;

			var results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertEquals(1, results.Length);
			AssertEquals(bizoInFuture, results[0]);

			filter.PropertySearch = ModuleDateFilter.Past;
			results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertEquals(2, results.Length);
			AssertCollectionContains(bizoInPast, results);
			AssertCollectionContains(bizoInPresent, results);
		}

		[TestDate(2014, 6, 10)]
		[TestUtcOffset(10, 0, 0)]
		public void TestQuery_FutureAndPast_UTC()
		{
			var bizoInPast = Factory.New<DummyBusinessObject>();
			var bizoInPresent = Factory.New<DummyBusinessObject>();
			var bizoInFuture = Factory.New<DummyBusinessObject>();

			bizoInPast.Z0_DateTimeOffset = ZDateTimeOffset.UtcNow.AddMinutes(-1);
			bizoInPresent.Z0_DateTimeOffset = ZDateTimeOffset.UtcNow;
			bizoInFuture.Z0_DateTimeOffset = ZDateTimeOffset.UtcNow.AddMinutes(1);

			var filter = new ModuleDateTimeOffsetFilter("Dat Date", DummyBizoSchema.Z0_DateTimeOffset) { ConvertFromLocalToUTC = true };
			filter.PropertySearch = ModuleDateFilter.Future;

			var results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertEquals(1, results.Length);
			AssertEquals(bizoInFuture, results[0]);

			filter.PropertySearch = ModuleDateFilter.Past;
			results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertEquals(2, results.Length);
			AssertCollectionContains(bizoInPast, results);
			AssertCollectionContains(bizoInPresent, results);
		}

		[TestDate(2014, 6, 10)]
		public void TestQuery_FutureAndPast_WithQueryDelegate()
		{
			var bizoInPast = Factory.New<DummyBusinessObject>();
			var bizoInPresent = Factory.New<DummyBusinessObject>();
			var bizoInFuture = Factory.New<DummyBusinessObject>();

			bizoInPast.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-1);
			bizoInPresent.Z0_DateTimeOffset = ZDateTimeOffset.Now;
			bizoInFuture.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(1);

			GetDateTimeOffsetQuery getZ0DateTimeOffsetRangeQuery = (DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2) =>
			{
				return new DateQueryBuilder().CreateDateTimeOffsetRange(comparisonOperator, DummyBizoSchema.Z0_DateTimeOffset, value1, value2, false, false);
			};

			var filter = new ModuleDateTimeOffsetFilter("Dat Date", getZ0DateTimeOffsetRangeQuery);
			filter.PropertySearch = ModuleDateFilter.Future;

			var results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertEquals(1, results.Length);
			AssertEquals(bizoInFuture, results[0]);

			filter.PropertySearch = ModuleDateFilter.Past;
			results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertEquals(2, results.Length);
			AssertCollectionContains(bizoInPast, results);
			AssertCollectionContains(bizoInPresent, results);
		}

		#endregion

		#region Hour Offset Range Query

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Ago_LocalTime()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var bizoLessThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNowFromFencepost = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			var minutesInPast = new ZInt(90);

			bizoLessThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast).AddMinutes(-1);
			bizoEqualToFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast);
			bizoGreaterThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast).AddMinutes(1);
			bizoOnOppositeSideOfNowFromFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(10);
			bizoWithCurrentTime.Z0_DateTimeOffset = ZDateTimeOffset.Now;

			var filter = new ModuleDateTimeOffsetFilter("Dat Date", DummyBizoSchema.Z0_DateTimeOffset);
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			filter.Property2 = minutesInPast.GetDateTimeFromMinutes(); // Between 0 and 90 mins into the past.

			var results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionContains(bizoWithCurrentTime, results);

			filter.Property1 = new ZInt(10).GetDateTimeFromMinutes(); // Between 10 and 90 mins into the past.

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);

			filter.Property1 = minutesInPast.GetDateTimeFromMinutes(); // Anywhere >= 90 mins into the past.
			filter.Property2 = ZDateTime.Empty;

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Ago_LocalTime_WithQueryDelegate()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var bizoLessThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNowFromFencepost = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			var minutesInPast = new ZInt(90);

			bizoLessThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast).AddMinutes(-1);
			bizoEqualToFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast);
			bizoGreaterThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast).AddMinutes(1);
			bizoOnOppositeSideOfNowFromFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(10);
			bizoWithCurrentTime.Z0_DateTimeOffset = ZDateTimeOffset.Now;

			GetDateTimeOffsetQuery getZ0DateTimeOffsetRangeQuery = (DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2) =>
			{
				return new DateQueryBuilder().CreateDateTimeOffsetRange(comparisonOperator, DummyBizoSchema.Z0_DateTimeOffset, value1, value2, false, false);
			};

			var filter = new ModuleDateTimeOffsetFilter("Dat Date", getZ0DateTimeOffsetRangeQuery);
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			filter.Property2 = minutesInPast.GetDateTimeFromMinutes(); // Between 0 and 90 mins into the past.

			var results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionContains(bizoWithCurrentTime, results);

			filter.Property1 = new ZInt(10).GetDateTimeFromMinutes(); // Between 10 and 90 mins into the past.

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);

			filter.Property1 = minutesInPast.GetDateTimeFromMinutes(); // Anywhere >= 90 mins into the past.
			filter.Property2 = ZDateTime.Empty;

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Ago_UTCTime()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var bizoLessThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNowFromFencepost = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			var minutesInPast = new ZInt(90);

			bizoLessThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast).AddMinutes(-1);
			bizoEqualToFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast);
			bizoGreaterThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast).AddMinutes(1);
			bizoOnOppositeSideOfNowFromFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(10);
			bizoWithCurrentTime.Z0_DateTimeOffset = ZDateTimeOffset.Now;

			var filter = new ModuleDateTimeOffsetFilter("Dat Date", DummyBizoSchema.Z0_DateTimeOffset) { ConvertFromLocalToUTC = true };
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			filter.Property2 = minutesInPast.GetDateTimeFromMinutes(); // Between 0 and 90 mins into the past.

			var results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionContains(bizoWithCurrentTime, results);

			filter.Property1 = new ZInt(10).GetDateTimeFromMinutes(); // Between 10 and 90 mins into the past.

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);

			filter.Property1 = minutesInPast.GetDateTimeFromMinutes(); // Anywhere >= 90 mins into the past.
			filter.Property2 = ZDateTime.Empty;

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Ago_UTCTime_WithQueryDelegate()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var bizoLessThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNowFromFencepost = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			var minutesInPast = new ZInt(90);

			bizoLessThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast).AddMinutes(-1);
			bizoEqualToFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast);
			bizoGreaterThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-minutesInPast).AddMinutes(1);
			bizoOnOppositeSideOfNowFromFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(10);
			bizoWithCurrentTime.Z0_DateTimeOffset = ZDateTimeOffset.Now;

			GetDateTimeOffsetQuery getZ0DateTimeOffsetRangeQuery = (DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2) =>
			{
				return new DateQueryBuilder().CreateDateTimeOffsetRange(comparisonOperator, DummyBizoSchema.Z0_DateTimeOffset, value1, value2, false, false);
			};

			var filter = new ModuleDateTimeOffsetFilter("Dat Date", getZ0DateTimeOffsetRangeQuery) { ConvertFromLocalToUTC = true };
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			filter.Property2 = minutesInPast.GetDateTimeFromMinutes(); // Between 0 and 90 mins into the past.

			var results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionContains(bizoWithCurrentTime, results);

			filter.Property1 = new ZInt(10).GetDateTimeFromMinutes(); // Between 10 and 90 mins into the past.

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);

			filter.Property1 = minutesInPast.GetDateTimeFromMinutes(); // Anywhere >= 90 mins into the past.
			filter.Property2 = ZDateTime.Empty;

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Away_LocalTime()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var bizoLessThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNowFromFencepost = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			var minutesInFuture = new ZInt(90);

			bizoLessThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture).AddMinutes(-1);
			bizoEqualToFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture);
			bizoGreaterThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture).AddMinutes(1);
			bizoOnOppositeSideOfNowFromFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-10);
			bizoWithCurrentTime.Z0_DateTimeOffset = ZDateTimeOffset.Now;

			var filter = new ModuleDateTimeOffsetFilter("Dat Date", DummyBizoSchema.Z0_DateTimeOffset);
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
			filter.Property2 = minutesInFuture.GetDateTimeFromMinutes(); // Between 0 and 90 mins into the future.

			var results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionContains(bizoWithCurrentTime, results);

			filter.Property1 = new ZInt(10).GetDateTimeFromMinutes(); // Between 10 and 90 mins into the future.

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);

			filter.Property1 = minutesInFuture.GetDateTimeFromMinutes(); // Anywhere >= 90 mins into the future.
			filter.Property2 = ZDateTime.Empty;

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Away_LocalTime_WithQueryDelegate()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var bizoLessThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNowFromFencepost = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			var minutesInFuture = new ZInt(90);

			bizoLessThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture).AddMinutes(-1);
			bizoEqualToFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture);
			bizoGreaterThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture).AddMinutes(1);
			bizoOnOppositeSideOfNowFromFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-10);
			bizoWithCurrentTime.Z0_DateTimeOffset = ZDateTimeOffset.Now;

			GetDateTimeOffsetQuery getZ0DateTimeOffsetRangeQuery = (DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2) =>
			{
				return new DateQueryBuilder().CreateDateTimeOffsetRange(comparisonOperator, DummyBizoSchema.Z0_DateTimeOffset, value1, value2, false, false);
			};

			var filter = new ModuleDateTimeOffsetFilter("Dat Date", getZ0DateTimeOffsetRangeQuery);
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
			filter.Property2 = minutesInFuture.GetDateTimeFromMinutes(); // Between 0 and 90 mins into the future.

			var results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionContains(bizoWithCurrentTime, results);

			filter.Property1 = new ZInt(10).GetDateTimeFromMinutes(); // Between 10 and 90 mins into the future.

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);

			filter.Property1 = minutesInFuture.GetDateTimeFromMinutes(); // Anywhere >= 90 mins into the future.
			filter.Property2 = ZDateTime.Empty;

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Away_UTCTime()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var bizoLessThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNowFromFencepost = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			var minutesInFuture = new ZInt(90);

			bizoLessThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture).AddMinutes(-1);
			bizoEqualToFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture);
			bizoGreaterThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture).AddMinutes(1);
			bizoOnOppositeSideOfNowFromFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-10);
			bizoWithCurrentTime.Z0_DateTimeOffset = ZDateTimeOffset.Now;

			var filter = new ModuleDateTimeOffsetFilter("Dat Date", DummyBizoSchema.Z0_DateTimeOffset) { ConvertFromLocalToUTC = true };
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
			filter.Property2 = minutesInFuture.GetDateTimeFromMinutes(); // Between 0 and 90 mins into the future.

			var results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionContains(bizoWithCurrentTime, results);

			filter.Property1 = new ZInt(10).GetDateTimeFromMinutes(); // Between 10 and 90 mins into the future.

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);

			filter.Property1 = minutesInFuture.GetDateTimeFromMinutes(); // Anywhere >= 90 mins into the future.
			filter.Property2 = ZDateTime.Empty;

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Away_UTCTime_WithQueryDelegate()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var bizoLessThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNowFromFencepost = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			var minutesInFuture = new ZInt(90);

			bizoLessThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture).AddMinutes(-1);
			bizoEqualToFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture);
			bizoGreaterThanFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(minutesInFuture).AddMinutes(1);
			bizoOnOppositeSideOfNowFromFencepost.Z0_DateTimeOffset = ZDateTimeOffset.Now.AddMinutes(-10);
			bizoWithCurrentTime.Z0_DateTimeOffset = ZDateTimeOffset.Now;

			GetDateTimeOffsetQuery getZ0DateTimeOffsetRangeQuery = (DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2) =>
			{
				return new DateQueryBuilder().CreateDateTimeOffsetRange(comparisonOperator, DummyBizoSchema.Z0_DateTimeOffset, value1, value2, false, false);
			};

			var filter = new ModuleDateTimeOffsetFilter("Dat Date", getZ0DateTimeOffsetRangeQuery) { ConvertFromLocalToUTC = true };
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
			filter.Property2 = minutesInFuture.GetDateTimeFromMinutes(); // Between 0 and 90 mins into the future.

			var results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionContains(bizoWithCurrentTime, results);

			filter.Property1 = new ZInt(10).GetDateTimeFromMinutes(); // Between 10 and 90 mins into the future.

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);

			filter.Property1 = minutesInFuture.GetDateTimeFromMinutes(); // Anywhere >= 90 mins into the future.
			filter.Property2 = ZDateTime.Empty;

			results = Factory.Load<DummyBusinessObject>(filter.Query);

			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNowFromFencepost, results);
			AssertCollectionNotContains(bizoWithCurrentTime, results);
		}

		[TestDate(2015, 7, 14)]
		public void TestChangingPropertySearchToHourOffset_ShouldRemoveSillyOffsets()
		{
			var filter = new ModuleDateTimeOffsetFilter("Dat Date", DummyBizoSchema.Z0_DateTimeOffset);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(1000);
			filter.Property2 = ZDateTime.UtcNow.AddDays(2000);

			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			AssertEquals(ZDateTime.Empty, filter.Property1);
			AssertEquals(ZDateTime.Empty, filter.Property2);
		}

		[TestDate(2015, 7, 14)]
		public void TestChangingPropertySearchToTimeRange_ShouldLeavePropertyValues()
		{
			var filter = new ModuleDateTimeOffsetFilter("Dat Date", DummyBizoSchema.Z0_DateTimeOffset);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(1000);
			filter.Property2 = ZDateTime.UtcNow.AddDays(2000);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;

			AssertNotEquals(ZDateTime.Empty, filter.Property1);
			AssertNotEquals(ZDateTime.Empty, filter.Property2);
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			var filter = new ModuleDateTimeOffsetFilter("Nope. Nope. Nope.", DummyBizoSchema.Z0_DateTimeOffset);

			AssertEquals(DateOffsetRangeFilterOptions.Codes.Past, filter.FilterOption);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			Dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			Dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
		}

		DummyBusinessObject Dummy1;
		DummyBusinessObject Dummy2;
		DummyBusinessObject Dummy3;

		protected override ModuleDateTimeOffsetFilter GetNewModuleFilter()
		{
			return new ModuleDateTimeOffsetFilter("moo", DummyBizoSchema.Z0_DateTimeOffset);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		#endregion

		#region DummyModuleDateTimeOffsetFilter

		[CodeAlive("Will be used in the future")]
		public class DummyModuleDateTimeOffsetFilter : ModuleDateTimeOffsetFilter
		{
			public DummyModuleDateTimeOffsetFilter()
				: base("DummyDateTimeOffsetFilter", DummyBizoSchema.Z0_DateTimeOffset)
			{
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				string sqlDate2 = Property2.IsValid ? Property2.SqlFormat : ZString.Empty;
				writer.WriteElementString("Property2", sqlDate2);
			}
		}

		#endregion
	}
}
