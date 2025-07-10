using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ModuleDateFilterTest : ModuleFilterTestCase<ModuleDateFilter>
	{
		public void TestCopyTransientProperties()
		{
			var copiedFilter = GetNewModuleFilter();
			copiedFilter.Property1 = new ZDateTime(2022, 1, 10);
			copiedFilter.Property2 = new ZDateTime(2022, 1, 11);
			copiedFilter.PropertyDecimal1 = 1m;
			copiedFilter.PropertyDecimal2 = 2m;

			Filter.Property1 = new ZDateTime(2022, 1, 13);
			Filter.Property2 = new ZDateTime(2022, 1, 14);
			Filter.PropertyDecimal1 = 3m;
			Filter.PropertyDecimal2 = 4m;
			copiedFilter.CopyTransientProperties(Filter);

			AssertEquals(new ZDateTime(2022, 1, 13), copiedFilter.Property1);
			AssertEquals(new ZDateTime(2022, 1, 14), copiedFilter.Property2);
			AssertEquals(3m, copiedFilter.PropertyDecimal1);
			AssertEquals(4m, copiedFilter.PropertyDecimal2);
		}

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			ZString searchValue = "Today";
			var dateValue1 = new ZDateTime(2006, 12, 25);
			var dateValue2 = new ZDateTime(2006, 12, 26);

			Filter.PropertySearch = searchValue;
			Filter.Property1 = dateValue1;
			Filter.Property2 = dateValue2;

			AssertEquals("Precondition", searchValue, Filter.PropertySearch);
			AssertEquals("Precondition", dateValue1, Filter.Property1);
			AssertEquals("Precondition", dateValue2, Filter.Property2);

			Filter.Clear();
			AssertEquals("", Filter.PropertySearch);
			AssertEquals(ZDateTime.Empty, Filter.Property1);
			AssertEquals(ZDateTime.Empty, Filter.Property2);
		}

		#endregion

		#region TestIsEmpty

		public virtual void TestIsEmpty()
		{
			Filter.Property1 = ZDateTime.Today;
			Filter.Property2 = ZDateTime.Today;

			Filter.PropertySearch = "";
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = "Tomorrow";
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = "crap data";
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			AssertEquals(false, Filter.IsEmpty);

			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = ZDateTime.Empty;
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = "Tomorrow";
			AssertEquals(false, Filter.IsEmpty);
		}

		#endregion

		#region TestPropertySearch_ListContainsSpecifiedDateRange

		public void TestPropertySearch_ListContainsSpecifiedDateRange()
		{
			AssertEquals(true, Filter.PropertySearch_List.ContainsCode(ModuleDateFilter.SpecifiedDateRange));
		}

		#endregion

		#region TestIsPropertySearchUsingSpecifiedDateRange

		public void TestIsPropertySearchUsingSpecifiedDateRange()
		{
			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			AssertEquals(false, Filter.IsPropertySearchUsingSpecifiedDateRange);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedDateRange);
		}

		#endregion

		#region TestPropertyValidationWhenSearchForOldDateUsingSpecifiedDateRange

		[TestDate(2015, 05, 27)]
		public void TestPropertyValidationWhenSearchForOldDateUsingSpecifiedDateRange()
		{
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedDateRange);

			Filter.Property1 = new ZDateTime(1941, 06, 22);
			Filter.Property2 = new ZDateTime(1945, 05, 09);

			// Should be no errors related to very old date
			AssertNoErrors(Filter.Property1Info);
			AssertNoErrors(Filter.Property2Info);
			AssertHasWarning(Filter.Property1Info, "The date '22-Jun-1941' is more than 1 year old.");
			AssertHasWarning(Filter.Property2Info, "The date '09-May-1945' is more than 1 year old.");
		}

		#endregion

		#region TestPropertyValidationWhenSearchForOldDateUsingSpecifiedDateTimeRange

		[TestDate(2015, 05, 27)]
		public void TestPropertyValidationWhenSearchForOldDateUsingSpecifiedDateTimeRange()
		{
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedDateTimeRange);

			Filter.Property1 = new ZDateTime(1941, 06, 22);
			Filter.Property2 = new ZDateTime(1945, 05, 09);

			// Should be no errors related to very old date
			AssertNoErrors(Filter.Property1Info);
			AssertNoErrors(Filter.Property2Info);
			AssertHasWarning(Filter.Property1Info, "The date '22-Jun-1941' is more than 1 year old.");
			AssertHasWarning(Filter.Property2Info, "The date '09-May-1945' is more than 1 year old.");
		}

		#endregion

		#region TestIsPropertySearchUsingSpecifiedDateTimeRange

		public void TestIsPropertySearchUsingSpecifiedDateTimeRange()
		{
			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			AssertEquals(false, Filter.IsPropertySearchUsingSpecifiedDateTimeRange);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedDateTimeRange);
		}

		#endregion

		#region TestPropertySearchUsesHideFutureDatesFlag

		public virtual void TestPropertySearchUsesHideFutureDatesFlag()
		{
			AssertEquals("HideFutureDates", false, Filter.HideFutureDates);
			AssertEquals("PropertySearch_List.Count", 40, Filter.PropertySearch_List.Count);
			AssertEquals("PropertySearch_List.ContainsCode(\"Tomorrow\")", true, Filter.PropertySearch_List.ContainsCode("Tomorrow"));

			Filter.HideFutureDates = true;
			AssertEquals("PropertySearch_List.Count", 27, Filter.PropertySearch_List.Count);
			AssertEquals("PropertySearch_List.ContainsCode(\"Tomorrow\")", false, Filter.PropertySearch_List.ContainsCode("Tomorrow"));

			Filter.HideFutureDates = false;
			AssertEquals("PropertySearch_List.Count", 40, Filter.PropertySearch_List.Count);
			AssertEquals("PropertySearch_List.ContainsCode(\"Tomorrow\")", true, Filter.PropertySearch_List.ContainsCode("Tomorrow"));
		}

		#endregion

		#region TestPropertySearch_List

		protected virtual ImmutableArray<string> ExpectedPropertySearchListCore
		{
			get
			{
				return ImmutableArray.Create<string>(
					string.Empty,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Today,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.ThisWeek,

					string.Empty,
					"Past Dates Category",
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Yesterday,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.LastWeek,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Last7Days,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Last14Days,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.LastMonth,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.LastCalendarMonth,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Last2Mths,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Last3Mths,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Last6Mths,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Last12Mths,
					ModuleDateFilter.Past,

					string.Empty,
					"Future Dates Category",
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Tomorrow,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.NextWeek,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Next7Days,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Next14Days,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.NextMonth,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.NextCalendarMonth,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Next2Mths,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Next3Mths,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Next6Mths,
					Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Next12Mths,
					ModuleDateFilter.Future,

					string.Empty,
					"Ranges Category",
					ModuleDateFilter.SpecifiedDateRange,
					ModuleDateFilter.SpecifiedDateTimeRange,
					ModuleDateFilter.SpecifiedHourOffsetRange,
					ModuleDateFilter.SpecifiedWorkHourOffsetRange,
					ModuleDateFilter.SpecifiedDayOffsetRange,

					string.Empty,
					"Date Entered Category",
					ModuleDateFilter.HasDateEntered,
					ModuleDateFilter.HasNoDateEntered
				);
			}
		}

		public void TestPropertySearch_List()
		{
			var dateRangePairList = Filter.PropertySearch_List;

			AssertEquals("List count", ExpectedPropertySearchListCore.Length, dateRangePairList.Count);

			foreach (var code in ExpectedPropertySearchListCore)
			{
				Assert("Search List contains code: " + code, dateRangePairList.ContainsCode(code));
			}

			foreach (CodeDescriptionPair pair in dateRangePairList)
			{
				if (pair.MultilingualCode.IsEmpty || pair.GetType().Equals(typeof(CategoryCodeDescriptionPair)))
				{
					continue;
				}
				Assert("Code of every element in ProperySearch_List of ModuleDateFilter should be MultilingualString.", !pair.MultilingualCode.GetType().Equals(typeof(NoResString)));
			}
		}

		#endregion

		#region TestPropertySearch_ListContainsDateEntered

		ZQuery DummyDelegate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return new ZQuery();
		}

		public virtual void TestPropertySearch_ListContainsDateEntered()
		{
			var filterNullable = new ModuleDateFilter("Nullable", DummyDelegate, true);
			var filterNotNullable = new ModuleDateFilter("Not Nullable", DummyDelegate, false);

			AssertEquals(true, filterNullable.PropertySearch_List.ContainsCode(ModuleDateFilter.HasNoDateEntered));
			AssertEquals(true, filterNullable.PropertySearch_List.ContainsCode(ModuleDateFilter.HasDateEntered));
			AssertEquals(false, filterNotNullable.PropertySearch_List.ContainsCode(ModuleDateFilter.HasNoDateEntered));
			AssertEquals(false, filterNotNullable.PropertySearch_List.ContainsCode(ModuleDateFilter.HasDateEntered));
		}

		#endregion

		#region TestIsPropertySearchUsingHasDateEntered

		public void TestIsPropertySearchUsingHasDateEntered()
		{
			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			AssertEquals(false, Filter.IsPropertySearchUsingHasDateEntered);

			Filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals(true, Filter.IsPropertySearchUsingHasDateEntered);
		}

		#endregion

		#region TestIsPropertySearchUsingHasNoDateEntered

		public void TestIsPropertySearchUsingHasNoDateEntered()
		{
			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			AssertEquals(false, Filter.IsPropertySearchUsingHasNoDateEntered);

			Filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals(true, Filter.IsPropertySearchUsingHasNoDateEntered);
		}

		#endregion

		#region Testing the Query results

		#region TestQueryWithSpecifiedDates

		[TestDate(2007, 1, 1)]
		public virtual void TestQueryWithSpecifiedDates()
		{
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			ZDateTime dateToFind = TestDateAttribute.Date;
			ZDateTime dateToFind2 = TestDateAttribute.Date.AddSeconds(1);
			ZDateTime dateToFind3 = TestDateAttribute.Date.AddDays(1).AddSeconds(-1);
			ZDateTime dateToNotFind = TestDateAttribute.Date.AddDays(1);

			var dummy1b = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy1c = Factory.NewWithValidTestData<DummyBusinessObject>();

			Dummy1.Z0_Date = dateToFind;
			dummy1b.Z0_Date = dateToFind2;
			dummy1c.Z0_Date = dateToFind3;
			Dummy2.Z0_Date = dateToNotFind;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			// from date only
			Filter.Property1 = dateToFind;
			Filter.Property2 = ZDateTime.Empty;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(dummy1b, dummies);
			AssertCollectionContains(dummy1c, dummies);
			AssertCollectionContains(Dummy2, dummies);

			// to date only
			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = dateToFind;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(dummy1b, dummies);
			AssertCollectionContains(dummy1c, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			// both to and from dates
			Filter.Property1 = dateToFind;
			Filter.Property2 = dateToFind;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(dummy1b, dummies);
			AssertCollectionContains(dummy1c, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			// both to and from dates (ranged check)
			Filter.Property1 = dateToFind.AddDays(-1);
			Filter.Property2 = dateToFind.AddDays(1);
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(dummy1b, dummies);
			AssertCollectionContains(dummy1c, dummies);
			AssertCollectionContains(Dummy2, dummies);

			Dummy1.Z0_Date = dateToFind;
			Dummy2.Z0_Date = TestDateAttribute.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997);
			Factory.Save();

			Filter.Property1 = dateToFind;
			Filter.Property2 = dateToFind;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(dummy1b, dummies);
			AssertCollectionContains(dummy1c, dummies);
			AssertCollectionContains(Dummy2, dummies);

			Filter.Property1 = dateToFind.AddDays(1);
			Filter.Property2 = dateToFind.AddDays(1);
			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionNotContains(dummy1b, dummies);
			AssertCollectionNotContains(dummy1c, dummies);
			AssertCollectionNotContains(Dummy2, dummies);
		}

		#endregion

		#region TestQueryUsingDateTimeRange

		[TestDate(2007, 1, 1, 1, 1, 1)]
		public virtual void TestQueryUsingDateTimeRange()
		{
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			ZDateTime dateTimeToFind = TestDateAttribute.Date;
			ZDateTime dateTimeToNotFind = TestDateAttribute.Date.AddSeconds(1);

			Dummy1.Z0_Date = dateTimeToFind;
			Dummy2.Z0_Date = dateTimeToNotFind;
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

		public virtual void TestQueryUsingHasDateEntered()
		{
			Filter.PropertySearch = ModuleDateFilter.HasDateEntered;

			Dummy1.Z0_Date = new ZDateTime(2009, 10, 1, 13, 35, 23);
			Dummy2.Z0_Date = ZDateTime.Empty;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			Filter.Property1 = new ZDateTime(2009, 10, 1, 13, 35, 23);
			Filter.Property2 = ZDateTime.Empty;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = new ZDateTime(2009, 10, 1, 13, 35, 23);
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);
		}

		public virtual void TestQueryUsingHasDateEnteredNullable()
		{
			Filter.PropertySearch = ModuleDateFilter.HasDateEntered;

			Dummy1.Z0_Date = new ZDateTime(2009, 10, 1, 13, 35, 23);
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);
			Filter.Property1 = new ZDateTime(2009, 10, 1, 13, 35, 23);

			var originalValue = Filter.FilterColumn.IsNullable;

			try
			{
				typeof(SchemaDateTimeColumn)
						 .GetField("IsNullable", BindingFlags.Instance | BindingFlags.Public)
						 .SetValue(Filter.FilterColumn, false);

				dummies.Load(Filter.Query);
				AssertCollectionContains(Dummy1, dummies);
			}
			finally
			{
				typeof(SchemaDateTimeColumn)
						 .GetField("IsNullable", BindingFlags.Instance | BindingFlags.Public)
						 .SetValue(Filter.FilterColumn, originalValue);
			}
		}

		#endregion

		#region TestQueryUsingHasNoDateEntered

		public virtual void TestQueryUsingHasNoDateEntered()
		{
			Filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			Dummy1.Z0_Date = new ZDateTime(2009, 10, 1, 13, 35, 23);
			Dummy2.Z0_Date = ZDateTime.Empty;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			Filter.Property1 = new ZDateTime(2009, 10, 1, 13, 35, 23);
			Filter.Property2 = ZDateTime.Empty;
			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);

			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = new ZDateTime(2009, 10, 1, 13, 35, 23);
			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);
		}

		#endregion

		#region TestQueryUsingHasNoDateEnteredUsingDataView

		public virtual void TestQueryUsingHasNoDateEnteredUsingDataView()
		{
			var rowFactory = Factory.GetType().GetProperty("RowFactory",
				BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Factory, null) as RowFactory;
			typeof(RowFactory).GetProperty("MaximumRowsBeforeUsingIndex").SetValue(rowFactory, 0, null);//To make sure the query will use dataview

			Filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Dummy1.Z0_Date = new ZDateTime(2009, 10, 1, 13, 35, 23);
			Dummy2.Z0_Date = ZDateTime.Empty;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);
		}

		#endregion

		#region TestQueryWithCommonDates

		[TestDate(2007, 1, 1)]
		public virtual void TestQueryWithCommonDates()
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
		public virtual void TestQueryWithWeekAndMonthDateRanges()
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
		public virtual void TestQueryUsingDateTimeRangeConvertToUTC()
		{
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			Filter.ConvertFromLocalToUTC = true;
			Filter.Property1 = new ZDateTime(2011, 1, 28, 11, 0, 0); // 4:00 by UTC
			Filter.Property2 = new ZDateTime(2011, 1, 28, 13, 0, 0); // 6:00 by UTC

			AssertEquals(new ZDateTime(2011, 1, 28, 4, 0, 0), Filter.FromDate);
			AssertEquals(new ZDateTime(2011, 1, 28, 6, 0, 0), Filter.ToDate);

			Dummy1.Z0_Date = new ZDateTime(2011, 1, 28, 3, 0, 0);
			Dummy2.Z0_Date = new ZDateTime(2011, 1, 28, 5, 0, 0);
			Dummy3.Z0_Date = new ZDateTime(2011, 1, 28, 7, 0, 0);
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
		public virtual void TestTodayFilter()
		{
			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			Filter.IsActive = true;

			var expectedFrom = new DateTime(2007, 1, 1, 0, 0, 0);
			var expectedTo = new DateTime(2007, 1, 2, 0, 0, 0, 0);

			var expectedFromUTC = new DateTime(2006, 12, 31, 17, 0, 0); // 24:00 - 7:00 = 17:00
			var expectedToUTC = new DateTime(2007, 1, 1, 16, 59, 59, 997); // 24:00 - 7:00 = 17:00

			Filter.ConvertFromLocalToUTC = false;
			AssertEquals("Z0_Date >= #" + ConvertExpectedDate(expectedFrom) + "# and Z0_Date < #" + ConvertExpectedDate(expectedTo) + "#", Filter.Query.LiteralTextADO);

			Filter.ConvertFromLocalToUTC = true;
			AssertEquals("Z0_Date >= #" + ConvertExpectedDate(expectedFromUTC) + "# and Z0_Date <= #" + ConvertExpectedDate(expectedToUTC) + "#", Filter.Query.LiteralTextADO);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Yesterday;
			Filter.IsActive = true;

			expectedFrom = new DateTime(2006, 12, 31, 0, 0, 0);
			expectedTo = new DateTime(2007, 1, 1, 0, 0, 0, 0);

			Filter.ConvertFromLocalToUTC = false;
			AssertEquals("Z0_Date >= #" + ConvertExpectedDate(expectedFrom) + "# and Z0_Date < #" + ConvertExpectedDate(expectedTo) + "#", Filter.Query.LiteralTextADO);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			Filter.IsActive = true;

			expectedFrom = new DateTime(2007, 1, 2, 0, 0, 0);
			expectedTo = new DateTime(2007, 1, 3, 0, 0, 0, 0);

			Filter.ConvertFromLocalToUTC = false;
			AssertEquals("Z0_Date >= #" + ConvertExpectedDate(expectedFrom) + "# and Z0_Date < #" + ConvertExpectedDate(expectedTo) + "#", Filter.Query.LiteralTextADO);
		}

		protected string ConvertExpectedDate(DateTime dateTime)
		{
			return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
		}

		#endregion

		protected virtual void AssertResultsUsingCurrentFilter(ZDateTime dateToFind, ZDateTime dateToNotFind, ZString propertySearchText)
		{
			Filter.PropertySearch = propertySearchText;
			Dummy1.Z0_Date = dateToFind;
			Dummy2.Z0_Date = dateToNotFind;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			dummies.Load(Filter.Query);

			AssertCollectionContains(Dummy1.PK, dummies.Select(x => x.PK));
			AssertCollectionNotContains(Dummy2.PK, dummies.Select(x => x.PK));
		}

		#endregion

		#region Serialisation

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleDateFilter();

			ZString searchValue = "Today";
			var dateValue1 = new ZDateTime(2007, 08, 09);
			var dateValue2 = new ZDateTime(2006, 08, 10);

			filter.PropertySearch = searchValue;
			filter.Property1 = dateValue1;
			filter.Property2 = dateValue2;

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleDateFilter)filterStripBizO[filter.Description];

			AssertEquals(ModuleDateFilter.SpecifiedDateRange, loadedFilter.PropertySearch);
			AssertEquals(ZDateTime.Empty, loadedFilter.Property1);
			AssertEquals(dateValue2, loadedFilter.Property2);
		}

		public void TestDeserializeInvalidDateFromXml()
		{
			var filter = new ModuleDateFilter("Filter", DummyBizoSchema.Z0_Date);

			using (var stringReader = new StringReader("<Property1>somethingInvalid</Property1>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Nothing should happen when deserializing invalid data", ZDateTime.Empty, filter.Property1);
			}

			using (var stringReader = new StringReader("<Property2>somethingInvalid</Property2>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Nothing should happen when deserializing invalid data", ZDateTime.Empty, filter.Property2);
			}

			using (var stringReader = new StringReader("<Property1>2008-01-01 10:00:00.000</Property1>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Valid value should be deserialised correctly", new ZDateTime(2008, 01, 01, 10, 0, 0), filter.Property1);
			}

			using (var stringReader = new StringReader("<Property2>2008-01-01 10:00:00.000</Property2>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Valid value should be deserialised correctly", new ZDateTime(2008, 01, 01, 10, 0, 0), filter.Property2);
			}
		}

		public void TestDeserialise_WithAndWithoutFilterOptionProperty()
		{
			var filter = new ModuleDateFilter("Iiii'm the ahh Prime Minister", DummyBizoSchema.Z0_Date);

			using (var stringReader = new StringReader("<FilterBlah><Property1>2015-07-14 00:00:00.000</Property1></FilterBlah>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				xmlReader.Read(); // Move past the root element
				filter.DeserializeProperties(xmlReader);
				AssertEquals(new ZDateTime(2015, 7, 14), filter.Property1);
				AssertEquals(DateOffsetRangeFilterOptions.Codes.Past, filter.FilterOption);
			}

			using (var stringReader = new StringReader("<FilterBlah><Property1>2015-07-14 00:00:00.000</Property1><FilterOption>Future</FilterOption></FilterBlah>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				xmlReader.Read(); // Move past the root element
				filter.DeserializeProperties(xmlReader);
				AssertEquals(new ZDateTime(2015, 7, 14), filter.Property1);
				AssertEquals(DateOffsetRangeFilterOptions.Codes.Future, filter.FilterOption);
			}
		}

		public void TestSerialise_ShouldSerialiseFilterOption()
		{
			var filter = new ModuleDateFilter("Iiii'm the ahh Prime Minister", DummyBizoSchema.Z0_Date);
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			filter.Property1 = new ZDateTime(2015, 7, 14);
			filter.Property2 = new ZDateTime(2015, 7, 14);
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;

			var result = new StringBuilder();

			using (var xmlWriter = XmlWriter.Create(result, new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment }))
			{
				((IXmlSerializable)filter).WriteXml(xmlWriter);
			}

			AssertEquals("<SearchProperty>Offset range</SearchProperty><Property1>2015-07-14 00:00:00.000</Property1><Property2>2015-07-14 00:00:00.000</Property2><FilterOption>Future</FilterOption><PropertyDecimal1>0.00</PropertyDecimal1><PropertyDecimal2>0.00</PropertyDecimal2>", result.ToString());
		}

		#endregion

		#region TestToAndFromDate
		public virtual void TestFromDate_RemoveTimeComponentWhenIsPropertySearchUsingSpecifiedDateRange()
		{
			var testFromDate = new ZDateTime(2014, 02, 13, 12, 34, 56);
			Filter.Property1 = testFromDate;

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedDateTimeRange);
			AssertEquals(false, Filter.IsPropertySearchUsingSpecifiedDateRange);
			AssertEquals(testFromDate, Filter.FromDate);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedDateRange);
			AssertEquals(false, Filter.IsPropertySearchUsingSpecifiedDateTimeRange);
			AssertEquals(testFromDate.Date, Filter.FromDate);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestToDate_ConvertFromLocalToUTC()
		{
			var testToDate = new ZDateTime(2014, 2, 14, 1, 0, 0);
			// expected is testToDate.Date + 1 day, minus 10hrs for UTC offset
			var expectedUtcDate = new ZDateTime(2014, 2, 14, 13, 59, 59, 997);
			Filter.Property2 = testToDate;

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedDateTimeRange);
			AssertEquals(false, Filter.IsPropertySearchUsingSpecifiedDateRange);
			AssertEquals(testToDate, Filter.ToDate);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Filter.ConvertFromLocalToUTC = true;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedDateRange);
			AssertEquals(false, Filter.IsPropertySearchUsingSpecifiedDateTimeRange);
			AssertEquals(expectedUtcDate, Filter.ToDate);
		}

		public void TestToDate_UseEndOfDayWhenIsPropertySearchUsingSpecifiedDateRange()
		{
			var testToDate = new ZDateTime(2014, 02, 13, 12, 34, 56);
			Filter.Property2 = testToDate;

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedDateTimeRange);
			AssertEquals(false, Filter.IsPropertySearchUsingSpecifiedDateRange);
			AssertEquals(testToDate, Filter.ToDate);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedDateRange);
			AssertEquals(false, Filter.IsPropertySearchUsingSpecifiedDateTimeRange);
			AssertEquals(testToDate.EndOfDay().AddMilliseconds(997), Filter.ToDate);
		}
		#endregion

		#region Future/Past Query

		[TestDate(2014, 6, 10)]
		public void TestQuery_FutureAndPast()
		{
			var bizoInPast = Factory.New<DummyBusinessObject>();
			var bizoInPresent = Factory.New<DummyBusinessObject>();
			var bizoInFuture = Factory.New<DummyBusinessObject>();

			bizoInPast.Z0_Date = ZDateTime.Now.AddMinutes(-1);
			bizoInPresent.Z0_Date = ZDateTime.Now;
			bizoInFuture.Z0_Date = ZDateTime.Now.AddMinutes(1);

			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date);
			filter.PropertySearch = ModuleDateFilter.Future;

			var results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertEquals(1, results.Length);
			AssertEquals(bizoInFuture, results[0]);
			AssertEquals(ZDateTime.Now.AddMilliseconds(1), filter.FromDate);
			AssertEquals(ZDateTime.Empty, filter.ToDate);

			filter.PropertySearch = ModuleDateFilter.Past;
			results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertEquals(2, results.Length);
			AssertCollectionContains(bizoInPast, results);
			AssertCollectionContains(bizoInPresent, results);
			AssertEquals(ZDateTime.Empty, filter.FromDate);
			AssertEquals(ZDateTime.Now, filter.ToDate);
		}

		[TestDate(2014, 6, 10)]
		[TestUtcOffset(10, 0, 0)]
		public void TestQuery_FutureAndPast_UTC()
		{
			var bizoInPast = Factory.New<DummyBusinessObject>();
			var bizoInPresent = Factory.New<DummyBusinessObject>();
			var bizoInFuture = Factory.New<DummyBusinessObject>();

			bizoInPast.Z0_Date = ZDateTime.UtcNow.AddMinutes(-1);
			bizoInPresent.Z0_Date = ZDateTime.UtcNow;
			bizoInFuture.Z0_Date = ZDateTime.UtcNow.AddMinutes(1);

			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date) { ConvertFromLocalToUTC = true };
			filter.PropertySearch = ModuleDateFilter.Future;

			var results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertEquals(1, results.Length);
			AssertEquals(bizoInFuture, results[0]);
			AssertEquals(ZDateTime.UtcNow.AddMilliseconds(1), filter.FromDate);
			AssertEquals(ZDateTime.Empty, filter.ToDate);

			filter.PropertySearch = ModuleDateFilter.Past;
			results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertEquals(2, results.Length);
			AssertCollectionContains(bizoInPast, results);
			AssertCollectionContains(bizoInPresent, results);
			AssertEquals(ZDateTime.Empty, filter.FromDate);
			AssertEquals(ZDateTime.UtcNow, filter.ToDate);
		}

		[TestDate(2014, 6, 10)]
		public void TestQuery_FutureAndPast_WithQueryDelegate()
		{
			var bizoInPast = Factory.New<DummyBusinessObject>();
			var bizoInPresent = Factory.New<DummyBusinessObject>();
			var bizoInFuture = Factory.New<DummyBusinessObject>();

			bizoInPast.Z0_Date = ZDateTime.Now.AddMinutes(-1);
			bizoInPresent.Z0_Date = ZDateTime.Now;
			bizoInFuture.Z0_Date = ZDateTime.Now.AddMinutes(1);

			GetDateQuery getZ0DateTimeRangeQuery = (DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) =>
			{
				return new DateQueryBuilder().CreateDateTimeRange(comparisonOperator, DummyBizoSchema.Z0_Date, value1, value2, false, false);
			};

			var filter = new ModuleDateFilter("Dat Date", getZ0DateTimeRangeQuery);
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

			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date);
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			// Between 0 and 90 mins into the past + Anywhere >= 90 mins into the past
			AssertHourOffsetQueryWorksForPastTime(filter, minuteOffset: 90, fencepost: ZDateTime.Now.AddMinutes(-90), opposite: ZDateTime.Now.AddMinutes(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: true);

			// Between 10 and 90 mins into the past
			AssertHourOffsetQueryWorksForPastTime(filter, innerMinuteOffset: 10, outerMinuteOffset: 90, innerFencepost: ZDateTime.Now.AddMinutes(-10), outerFencepost: ZDateTime.Now.AddMinutes(-90), opposite: ZDateTime.Now.AddMinutes(10), now: ZDateTime.Now, nowShouldBeIncluded: false);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Ago_LocalTime_WithQueryDelegate()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			GetDateQuery getZ0DateTimeRangeQuery = (DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) =>
			{
				return new DateQueryBuilder().CreateDateTimeRange(comparisonOperator, DummyBizoSchema.Z0_Date, value1, value2, false, false);
			};
			var filter = new ModuleDateFilter("Dat Date", getZ0DateTimeRangeQuery);
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			// Between 0 and 90 mins into the past + Anywhere >= 90 mins into the past
			AssertHourOffsetQueryWorksForPastTime(filter, minuteOffset: 90, fencepost: ZDateTime.Now.AddMinutes(-90), opposite: ZDateTime.Now.AddMinutes(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: true);

			// Between 10 and 90 mins into the past
			AssertHourOffsetQueryWorksForPastTime(filter, innerMinuteOffset: 10, outerMinuteOffset: 90, innerFencepost: ZDateTime.Now.AddMinutes(-10), outerFencepost: ZDateTime.Now.AddMinutes(-90), opposite: ZDateTime.Now.AddMinutes(10), now: ZDateTime.Now, nowShouldBeIncluded: false);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Ago_UTCTime()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date) { ConvertFromLocalToUTC = true };
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			// Between 0 and 90 mins into the past + Anywhere >= 90 mins into the past
			AssertHourOffsetQueryWorksForPastTime(filter, minuteOffset: 90, fencepost: ZDateTime.UtcNow.AddMinutes(-90), opposite: ZDateTime.UtcNow.AddMinutes(10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: true);

			// Between 10 and 90 mins into the past
			AssertHourOffsetQueryWorksForPastTime(filter, innerMinuteOffset: 10, outerMinuteOffset: 90, innerFencepost: ZDateTime.UtcNow.AddMinutes(-10), outerFencepost: ZDateTime.UtcNow.AddMinutes(-90), opposite: ZDateTime.UtcNow.AddMinutes(10), now: ZDateTime.UtcNow, nowShouldBeIncluded: false);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Ago_UTCTime_WithQueryDelegate()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			GetDateQuery getZ0DateTimeRangeQuery = (DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) =>
			{
				return new DateQueryBuilder().CreateDateTimeRange(comparisonOperator, DummyBizoSchema.Z0_Date, value1, value2, false, false);
			};
			var filter = new ModuleDateFilter("Dat Date", getZ0DateTimeRangeQuery) { ConvertFromLocalToUTC = true };
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			// Between 0 and 90 mins into the past + Anywhere >= 90 mins into the past
			AssertHourOffsetQueryWorksForPastTime(filter, minuteOffset: 90, fencepost: ZDateTime.UtcNow.AddMinutes(-90), opposite: ZDateTime.UtcNow.AddMinutes(10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: true);

			// Between 10 and 90 mins into the past
			AssertHourOffsetQueryWorksForPastTime(filter, innerMinuteOffset: 10, outerMinuteOffset: 90, innerFencepost: ZDateTime.UtcNow.AddMinutes(-10), outerFencepost: ZDateTime.UtcNow.AddMinutes(-90), opposite: ZDateTime.UtcNow.AddMinutes(10), now: ZDateTime.UtcNow, nowShouldBeIncluded: false);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Away_LocalTime()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date);
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			// Between 0 and 90 mins into the future + Anywhere >= 90 mins into the future
			AssertHourOffsetQueryWorksForFutureTime(filter, minuteOffset: 90, fencepost: ZDateTime.Now.AddMinutes(90), opposite: ZDateTime.Now.AddMinutes(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: true);

			// Between 10 and 90 mins into the future
			AssertHourOffsetQueryWorksForFutureTime(filter, innerMinuteOffset: 10, outerMinuteOffset: 90, innerFencepost: ZDateTime.Now.AddMinutes(10), outerFencepost: ZDateTime.Now.AddMinutes(90), opposite: ZDateTime.Now.AddMinutes(-10), now: ZDateTime.Now, nowShouldBeIncluded: false);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Away_LocalTime_WithQueryDelegate()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			GetDateQuery getZ0DateTimeRangeQuery = (DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) =>
			{
				return new DateQueryBuilder().CreateDateTimeRange(comparisonOperator, DummyBizoSchema.Z0_Date, value1, value2, false, false);
			};
			var filter = new ModuleDateFilter("Dat Date", getZ0DateTimeRangeQuery);
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			// Between 0 and 90 mins into the future + Anywhere >= 90 mins into the future
			AssertHourOffsetQueryWorksForFutureTime(filter, minuteOffset: 90, fencepost: ZDateTime.Now.AddMinutes(90), opposite: ZDateTime.Now.AddMinutes(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: true);

			// Between 10 and 90 mins into the future
			AssertHourOffsetQueryWorksForFutureTime(filter, innerMinuteOffset: 10, outerMinuteOffset: 90, innerFencepost: ZDateTime.Now.AddMinutes(10), outerFencepost: ZDateTime.Now.AddMinutes(90), opposite: ZDateTime.Now.AddMinutes(-10), now: ZDateTime.Now, nowShouldBeIncluded: false);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Away_UTCTime()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date) { ConvertFromLocalToUTC = true };
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			// Between 0 and 90 mins into the future + Anywhere >= 90 mins into the future
			AssertHourOffsetQueryWorksForFutureTime(filter, minuteOffset: 90, fencepost: ZDateTime.UtcNow.AddMinutes(90), opposite: ZDateTime.UtcNow.AddMinutes(-10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: true);

			// Between 10 and 90 mins into the future
			AssertHourOffsetQueryWorksForFutureTime(filter, innerMinuteOffset: 10, outerMinuteOffset: 90, innerFencepost: ZDateTime.UtcNow.AddMinutes(10), outerFencepost: ZDateTime.UtcNow.AddMinutes(90), opposite: ZDateTime.UtcNow.AddMinutes(-10), now: ZDateTime.UtcNow, nowShouldBeIncluded: false);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Away_UTCTime_WithQueryDelegate()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			GetDateQuery getZ0DateTimeRangeQuery = (DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) =>
			{
				return new DateQueryBuilder().CreateDateTimeRange(comparisonOperator, DummyBizoSchema.Z0_Date, value1, value2, false, false);
			};
			var filter = new ModuleDateFilter("Dat Date", getZ0DateTimeRangeQuery) { ConvertFromLocalToUTC = true };
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			// Between 0 and 90 mins into the future + Anywhere >= 90 mins into the future
			AssertHourOffsetQueryWorksForFutureTime(filter, minuteOffset: 90, fencepost: ZDateTime.UtcNow.AddMinutes(90), opposite: ZDateTime.UtcNow.AddMinutes(-10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: true);

			// Between 10 and 90 mins into the future
			AssertHourOffsetQueryWorksForFutureTime(filter, innerMinuteOffset: 10, outerMinuteOffset: 90, innerFencepost: ZDateTime.UtcNow.AddMinutes(10), outerFencepost: ZDateTime.UtcNow.AddMinutes(90), opposite: ZDateTime.UtcNow.AddMinutes(-10), now: ZDateTime.UtcNow, nowShouldBeIncluded: false);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Ago_WhenUserEntersUtcValue()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date) { ConvertFromLocalToUTC = false, UserEntersUtcValue = true };
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			// Between 0 and 90 mins into the past + Anywhere >= 90 mins into the past
			AssertHourOffsetQueryWorksForPastTime(filter, minuteOffset: 90, fencepost: ZDateTime.UtcNow.AddMinutes(-90), opposite: ZDateTime.UtcNow.AddMinutes(10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: true);

			// Between 10 and 90 mins into the past
			AssertHourOffsetQueryWorksForPastTime(filter, innerMinuteOffset: 10, outerMinuteOffset: 90, innerFencepost: ZDateTime.UtcNow.AddMinutes(-10), outerFencepost: ZDateTime.UtcNow.AddMinutes(-90), opposite: ZDateTime.UtcNow.AddMinutes(10), now: ZDateTime.UtcNow, nowShouldBeIncluded: false);
		}

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestHourOffsetQuery_Away_WhenUserEntersUtcValue()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);

			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date) { ConvertFromLocalToUTC = false, UserEntersUtcValue = true };
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			// Between 0 and 90 mins into the future + Anywhere >= 90 mins into the future
			AssertHourOffsetQueryWorksForFutureTime(filter, minuteOffset: 90, fencepost: ZDateTime.UtcNow.AddMinutes(90), opposite: ZDateTime.UtcNow.AddMinutes(-10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: true);

			// Between 10 and 90 mins into the future
			AssertHourOffsetQueryWorksForFutureTime(filter, innerMinuteOffset: 10, outerMinuteOffset: 90, innerFencepost: ZDateTime.UtcNow.AddMinutes(10), outerFencepost: ZDateTime.UtcNow.AddMinutes(90), opposite: ZDateTime.UtcNow.AddMinutes(-10), now: ZDateTime.UtcNow, nowShouldBeIncluded: false);
		}

		[TestDate(2015, 7, 14)]
		public void TestChangingPropertySearchToHourOffset_ShouldRemoveSillyOffsets()
		{
			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date);

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
			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(1000);
			filter.Property2 = ZDateTime.UtcNow.AddDays(2000);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;

			AssertNotEquals(ZDateTime.Empty, filter.Property1);
			AssertNotEquals(ZDateTime.Empty, filter.Property2);
		}

		#endregion

		#region Working Hour Offset Range Query

		const string NineToSixWorkingTime =
			"                  ******************            "; // 9:00-18:00
		const string Eight30ToFiveWorkingTime =
			"                 *****************              "; // 8:30-17:00
		const string NineToFive30WorkingTime =
			"                  *****************             "; // 9:00-17:30

		[TestDate(2016, 11, 17, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWorkingHourOffsetQuery()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);
			AssertEquals(12, ZDateTime.UtcNow.Hour);
			AssertEquals(22, ZDateTime.Now.Hour);
			AssertEquals("We need to be sure about the day of the week to setup the department's schedule", DayOfWeek.Thursday, ZDateTime.Now.DayOfWeek);

			using (var context = new TestingUserContextWithDeptSchedule(NineToSixWorkingTime))
			{
				var filterLocalTime = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date)
				{
					PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange
				};
				GetDateQuery getZ0DateTimeRangeQuery = (DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) =>
				{
					return new DateQueryBuilder().CreateDateTimeRange(comparisonOperator, DummyBizoSchema.Z0_Date, value1, value2, false, false);
				};
				var filterLocalTimeWithQueryDelegate = new ModuleDateFilter("Dat Date", getZ0DateTimeRangeQuery)
				{
					PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange
				};
				var filterUTCTime = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date)
				{
					ConvertFromLocalToUTC = true,
					PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange
				};

				var filterUTCTimeWithQueryDelegate = new ModuleDateFilter("Dat Date", getZ0DateTimeRangeQuery) { ConvertFromLocalToUTC = true };
				filterUTCTimeWithQueryDelegate.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;

				//local time

				var fencepostLocalPast1 = ZDateTime.Now.AddHours(-4);               // 6 pm (it's 4 hours after work now, work hours are from 9 am to 6 pm)
				var fencepostLocalPast2 = ZDateTime.Now.AddHours(-4).AddHours(-2);  // 4 pm
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, minuteOffset: 0, fencepost: fencepostLocalPast1, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, minuteOffset: 120, fencepost: fencepostLocalPast2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, innerMinuteOffset: 0, outerMinuteOffset: 120, innerFencepost: fencepostLocalPast1, outerFencepost: fencepostLocalPast2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncluded: false);

				var fencepostLocalFuture1 = ZDateTime.Now.AddHours(11); // 9 am (it's 11 hours before work now, work hours are from 9 am to 6 pm)
				var fencepostLocalFuture2 = ZDateTime.Now.AddHours(11).AddHours(1); // 10 am
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, minuteOffset: 0, fencepost: fencepostLocalFuture1, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, minuteOffset: 60, fencepost: fencepostLocalFuture2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, innerMinuteOffset: 0, outerMinuteOffset: 60, innerFencepost: fencepostLocalFuture1, outerFencepost: fencepostLocalFuture2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncluded: false);

				//local time, with query delegate

				AssertHourOffsetQueryWorksForPastTime(filterLocalTimeWithQueryDelegate, minuteOffset: 0, fencepost: fencepostLocalPast1, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTimeWithQueryDelegate, minuteOffset: 120, fencepost: fencepostLocalPast2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTimeWithQueryDelegate, innerMinuteOffset: 0, outerMinuteOffset: 120, innerFencepost: fencepostLocalPast1, outerFencepost: fencepostLocalPast2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncluded: false);

				AssertHourOffsetQueryWorksForFutureTime(filterLocalTimeWithQueryDelegate, minuteOffset: 0, fencepost: fencepostLocalFuture1, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTimeWithQueryDelegate, minuteOffset: 60, fencepost: fencepostLocalFuture2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTimeWithQueryDelegate, innerMinuteOffset: 0, outerMinuteOffset: 60, innerFencepost: fencepostLocalFuture1, outerFencepost: fencepostLocalFuture2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncluded: false);

				//UTC time

				var fencepostUTCPast1 = ZDateTime.UtcNow.AddHours(-4);
				var fencepostUTCPast2 = ZDateTime.UtcNow.AddHours(-4).AddHours(-2);
				AssertHourOffsetQueryWorksForPastTime(filterUTCTime, minuteOffset: 0, fencepost: fencepostUTCPast1, opposite: ZDateTime.UtcNow.AddHours(10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterUTCTime, minuteOffset: 120, fencepost: fencepostUTCPast2, opposite: ZDateTime.UtcNow.AddHours(10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterUTCTime, innerMinuteOffset: 0, outerMinuteOffset: 120, innerFencepost: fencepostUTCPast1, outerFencepost: fencepostUTCPast2, opposite: ZDateTime.UtcNow.AddHours(10), now: ZDateTime.UtcNow, nowShouldBeIncluded: false);

				var fencepostUTCFuture1 = ZDateTime.UtcNow.AddHours(11);
				var fencepostUTCFuture2 = ZDateTime.UtcNow.AddHours(11).AddHours(1);
				AssertHourOffsetQueryWorksForFutureTime(filterUTCTime, minuteOffset: 0, fencepost: fencepostUTCFuture1, opposite: ZDateTime.UtcNow.AddHours(-10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterUTCTime, minuteOffset: 60, fencepost: fencepostUTCFuture2, opposite: ZDateTime.UtcNow.AddHours(-10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterUTCTime, innerMinuteOffset: 0, outerMinuteOffset: 60, innerFencepost: fencepostUTCFuture1, outerFencepost: fencepostUTCFuture2, opposite: ZDateTime.UtcNow.AddHours(-10), now: ZDateTime.UtcNow, nowShouldBeIncluded: false);

				//UTC time, with query delegate

				AssertHourOffsetQueryWorksForPastTime(filterUTCTimeWithQueryDelegate, minuteOffset: 0, fencepost: fencepostUTCPast1, opposite: ZDateTime.UtcNow.AddHours(10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterUTCTimeWithQueryDelegate, minuteOffset: 120, fencepost: fencepostUTCPast2, opposite: ZDateTime.UtcNow.AddHours(10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterUTCTimeWithQueryDelegate, innerMinuteOffset: 0, outerMinuteOffset: 120, innerFencepost: fencepostUTCPast1, outerFencepost: fencepostUTCPast2, opposite: ZDateTime.UtcNow.AddHours(10), now: ZDateTime.UtcNow, nowShouldBeIncluded: false);

				AssertHourOffsetQueryWorksForFutureTime(filterUTCTimeWithQueryDelegate, minuteOffset: 0, fencepost: fencepostUTCFuture1, opposite: ZDateTime.UtcNow.AddHours(-10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterUTCTimeWithQueryDelegate, minuteOffset: 60, fencepost: fencepostUTCFuture2, opposite: ZDateTime.UtcNow.AddHours(-10), now: ZDateTime.UtcNow, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterUTCTimeWithQueryDelegate, innerMinuteOffset: 0, outerMinuteOffset: 60, innerFencepost: fencepostUTCFuture1, outerFencepost: fencepostUTCFuture2, opposite: ZDateTime.UtcNow.AddHours(-10), now: ZDateTime.UtcNow, nowShouldBeIncluded: false);
			}
		}

		[TestDate(2016, 11, 17, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWorkingHourOffsetQuery_RespectsMinutes_WhenEight30ToFiveWorkingTime()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);
			AssertEquals(12, ZDateTime.UtcNow.Hour);
			AssertEquals(22, ZDateTime.Now.Hour);
			AssertEquals("We need to be sure about the day of the week to setup the department's schedule", DayOfWeek.Thursday, ZDateTime.Now.DayOfWeek);

			using (var context = new TestingUserContextWithDeptSchedule(Eight30ToFiveWorkingTime))
			{
				var filterLocalTime = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date);
				filterLocalTime.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;

				//past

				//checking minutes
				var fencepostLocalPast1 = ZDateTime.Now.AddHours(-5).AddMinutes(-1);  // 4:59 pm (1 work minute ago - it's 5 hours after work now, work hours are from 8:30 am to 5 pm)
				AssertEquals(16, fencepostLocalPast1.Hour);
				AssertEquals(59, fencepostLocalPast1.Minute);
				var fencepostLocalPast2 = ZDateTime.Now.AddHours(-5).AddMinutes(-59); // 4:01 pm (59 work minutes ago)
				AssertEquals(16, fencepostLocalPast2.Hour);
				AssertEquals(1, fencepostLocalPast2.Minute);

				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, minuteOffset: 1, fencepost: fencepostLocalPast1, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, minuteOffset: 59, fencepost: fencepostLocalPast2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, innerMinuteOffset: 1, outerMinuteOffset: 59, innerFencepost: fencepostLocalPast1, outerFencepost: fencepostLocalPast2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncluded: false);

				//checking when minutes go off work hours
				var fencepostLocalPastAfterHours1 = ZDateTime.Now.AddHours(-5).AddHours(-24).AddMinutes(-1);  // 4:59 pm, a day earlier (9 work hours and 1 work minute ago)
				AssertEquals(16, fencepostLocalPastAfterHours1.Hour);
				AssertEquals(59, fencepostLocalPastAfterHours1.Minute);
				var fencepostLocalPastAfterHours2 = ZDateTime.Now.AddHours(-5).AddHours(-24).AddMinutes(-59); // 4:01 pm, a day earlier (9 work hours and 59 work minutes ago)
				AssertEquals(16, fencepostLocalPastAfterHours2.Hour);
				AssertEquals(1, fencepostLocalPastAfterHours2.Minute);

				var workDayLenghInMinutes = 8 * 60 + 30;
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, minuteOffset: workDayLenghInMinutes + 1, fencepost: fencepostLocalPastAfterHours1, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, minuteOffset: workDayLenghInMinutes + 59, fencepost: fencepostLocalPastAfterHours2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, innerMinuteOffset: workDayLenghInMinutes + 1, outerMinuteOffset: workDayLenghInMinutes + 59, innerFencepost: fencepostLocalPastAfterHours1, outerFencepost: fencepostLocalPastAfterHours2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncluded: false);

				//future

				//checking minutes
				var fencepostLocalFuture1 = ZDateTime.Now.AddHours(10).AddMinutes(30).AddMinutes(1);  // 8:31 am (1 work minute away - it's 10.5 hours before work now, work hours are from 8:30 am to 5 pm)
				AssertEquals(8, fencepostLocalFuture1.Hour);
				AssertEquals(31, fencepostLocalFuture1.Minute);
				var fencepostLocalFuture2 = ZDateTime.Now.AddHours(10).AddMinutes(30).AddMinutes(59); // 9:29 am
				AssertEquals(9, fencepostLocalFuture2.Hour);
				AssertEquals(29, fencepostLocalFuture2.Minute);

				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, minuteOffset: 1, fencepost: fencepostLocalFuture1, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, minuteOffset: 59, fencepost: fencepostLocalFuture2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, innerMinuteOffset: 1, outerMinuteOffset: 59, innerFencepost: fencepostLocalFuture1, outerFencepost: fencepostLocalFuture2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncluded: false);

				//checking when minutes go off work hours
				var fencepostLocalAfterHours1 = ZDateTime.Now.AddHours(10).AddMinutes(30).AddDays(3).AddMinutes(1);  // 8:31 am, 3 days later (it's 10.5 hours before work now, work hours are from 8:30 am to 5 pm + weekend)
				AssertEquals(8, fencepostLocalAfterHours1.Hour);
				AssertEquals(31, fencepostLocalAfterHours1.Minute);
				var fencepostLocalAfterHours2 = ZDateTime.Now.AddHours(10).AddMinutes(30).AddDays(3).AddMinutes(59); // 9:29 am, 3 days later
				AssertEquals(9, fencepostLocalAfterHours2.Hour);
				AssertEquals(29, fencepostLocalAfterHours2.Minute);

				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, minuteOffset: workDayLenghInMinutes + 1, fencepost: fencepostLocalAfterHours1, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, minuteOffset: workDayLenghInMinutes + 59, fencepost: fencepostLocalAfterHours2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, innerMinuteOffset: workDayLenghInMinutes + 1, outerMinuteOffset: workDayLenghInMinutes + 59, innerFencepost: fencepostLocalAfterHours1, outerFencepost: fencepostLocalAfterHours2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncluded: false);
			}
		}

		[TestDate(2016, 11, 17, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWorkingHourOffsetQuery_RespectsMinutes_WhenNineToFive30WorkingTime()
		{
			AssertNotEquals("Now and UtcNow should be different to ensure we're testing the right thing.", ZDateTime.UtcNow, ZDateTime.Now);
			AssertEquals(12, ZDateTime.UtcNow.Hour);
			AssertEquals(22, ZDateTime.Now.Hour);
			AssertEquals("We need to be sure about the day of the week to setup the department's schedule", DayOfWeek.Thursday, ZDateTime.Now.DayOfWeek);

			using (var context = new TestingUserContextWithDeptSchedule(NineToFive30WorkingTime))
			{
				var filterLocalTime = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date);
				filterLocalTime.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;

				//past

				//checking minutes
				var fencepostLocalPast1 = ZDateTime.Now.AddHours(-4).AddMinutes(-30).AddMinutes(-1);  // 5:29 pm (1 work minute ago - it's 4.5 hours after work now, work hours are from 9 am to 5:30 pm)
				AssertEquals(17, fencepostLocalPast1.Hour);
				AssertEquals(29, fencepostLocalPast1.Minute);
				var fencepostLocalPast2 = ZDateTime.Now.AddHours(-4).AddMinutes(-30).AddMinutes(-59); // 4:31 pm (59 work minutes ago)
				AssertEquals(16, fencepostLocalPast2.Hour);
				AssertEquals(31, fencepostLocalPast2.Minute);

				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, minuteOffset: 1, fencepost: fencepostLocalPast1, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, minuteOffset: 59, fencepost: fencepostLocalPast2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, innerMinuteOffset: 1, outerMinuteOffset: 59, innerFencepost: fencepostLocalPast1, outerFencepost: fencepostLocalPast2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncluded: false);

				//checking when minutes go off work hours
				var fencepostLocalPastAfterHours1 = ZDateTime.Now.AddHours(-4).AddMinutes(-30).AddHours(-24).AddMinutes(-1);  // 5:29 pm, a day earlier (9 work hours and 1 work minute ago)
				AssertEquals(17, fencepostLocalPastAfterHours1.Hour);
				AssertEquals(29, fencepostLocalPastAfterHours1.Minute);
				var fencepostLocalPastAfterHours2 = ZDateTime.Now.AddHours(-4).AddMinutes(-30).AddHours(-24).AddMinutes(-59); // 4:31 pm, a day earlier (9 work hours and 59 work minutes ago)
				AssertEquals(16, fencepostLocalPastAfterHours2.Hour);
				AssertEquals(31, fencepostLocalPastAfterHours2.Minute);

				var workDayLenghInMinutes = 8 * 60 + 30;
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, minuteOffset: workDayLenghInMinutes + 1, fencepost: fencepostLocalPastAfterHours1, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, minuteOffset: workDayLenghInMinutes + 59, fencepost: fencepostLocalPastAfterHours2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForPastTime(filterLocalTime, innerMinuteOffset: workDayLenghInMinutes + 1, outerMinuteOffset: workDayLenghInMinutes + 59, innerFencepost: fencepostLocalPastAfterHours1, outerFencepost: fencepostLocalPastAfterHours2, opposite: ZDateTime.Now.AddHours(10), now: ZDateTime.Now, nowShouldBeIncluded: false);

				//future

				//checking minutes
				var fencepostLocalFuture1 = ZDateTime.Now.AddHours(11).AddMinutes(1);  // 9:01 am (1 work minute away - it's 10.5 hours before work now, work hours are from 9 am to 5:30 pm)
				AssertEquals(9, fencepostLocalFuture1.Hour);
				AssertEquals(01, fencepostLocalFuture1.Minute);
				var fencepostLocalFuture2 = ZDateTime.Now.AddHours(11).AddMinutes(59); // 9:59 am
				AssertEquals(9, fencepostLocalFuture2.Hour);
				AssertEquals(59, fencepostLocalFuture2.Minute);

				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, minuteOffset: 1, fencepost: fencepostLocalFuture1, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, minuteOffset: 59, fencepost: fencepostLocalFuture2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, innerMinuteOffset: 1, outerMinuteOffset: 59, innerFencepost: fencepostLocalFuture1, outerFencepost: fencepostLocalFuture2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncluded: false);

				//checking when minutes go off work hours
				var fencepostLocalAfterHours1 = ZDateTime.Now.AddHours(11).AddDays(3).AddMinutes(1);  // 9:01 am, 3 days later (it's 10.5 hours before work now, work hours are from 9 am to 5:30 pm + weekend)
				AssertEquals(9, fencepostLocalAfterHours1.Hour);
				AssertEquals(01, fencepostLocalAfterHours1.Minute);
				var fencepostLocalAfterHours2 = ZDateTime.Now.AddHours(11).AddDays(3).AddMinutes(59); // 9:59 am, 3 days later
				AssertEquals(9, fencepostLocalAfterHours2.Hour);
				AssertEquals(59, fencepostLocalAfterHours2.Minute);

				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, minuteOffset: workDayLenghInMinutes + 1, fencepost: fencepostLocalAfterHours1, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, minuteOffset: workDayLenghInMinutes + 59, fencepost: fencepostLocalAfterHours2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncludedForZeroOffset: false);
				AssertHourOffsetQueryWorksForFutureTime(filterLocalTime, innerMinuteOffset: workDayLenghInMinutes + 1, outerMinuteOffset: workDayLenghInMinutes + 59, innerFencepost: fencepostLocalAfterHours1, outerFencepost: fencepostLocalAfterHours2, opposite: ZDateTime.Now.AddHours(-10), now: ZDateTime.Now, nowShouldBeIncluded: false);
			}
		}

		[TestDate(2016, 11, 17)]
		public void TestChangingPropertySearchFromWorkingHourOffsetToSomethingIncompatibleAndBack_ShouldClearPropertyValues()
		{
			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date);

			//CASE A
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(1000);
			filter.Property2 = ZDateTime.UtcNow.AddDays(2000);

			filter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;

			AssertEquals(ZDateTime.Empty, filter.Property1);
			AssertEquals(ZDateTime.Empty, filter.Property2);

			//CASE B
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(1000);
			filter.Property2 = ZDateTime.UtcNow.AddDays(2000);

			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.LastMonth;
			filter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;

			AssertEquals(ZDateTime.Empty, filter.Property1);
			AssertEquals(ZDateTime.Empty, filter.Property2);

			//CASE C
			filter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;
			filter.Property1 = new ZInt(60).GetDateTimeFromMinutes();
			filter.Property2 = new ZInt(120).GetDateTimeFromMinutes();

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(ZDateTime.Empty, filter.Property1);
			AssertEquals(ZDateTime.Empty, filter.Property2);

			//CASE D
			filter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;
			filter.Property1 = new ZInt(60).GetDateTimeFromMinutes();
			filter.Property2 = new ZInt(120).GetDateTimeFromMinutes();

			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.LastMonth;
			AssertEquals(ZDateTime.Empty, filter.Property1);
			AssertEquals(ZDateTime.Empty, filter.Property2);
		}

		[TestDate(2016, 11, 17)]
		public void TestChangingPropertySearchFromHourOffsetToWorkingHourOffsetAndBack_ShouldLeavePropertyValues()
		{
			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date);

			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			var offset1 = new ZDateTime(2015, 7, 14, 5, 0, 0);
			var offset2 = new ZDateTime(2015, 7, 14, 10, 0, 0);
			filter.Property1 = offset1;
			filter.Property2 = offset2;
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;

			filter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;

			AssertEquals(offset1, filter.Property1);
			AssertEquals(offset2, filter.Property2);
			AssertEquals(DateOffsetRangeFilterOptions.Codes.Future, filter.FilterOption);

			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Past;
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			AssertEquals(offset1, filter.Property1);
			AssertEquals(offset2, filter.Property2);
			AssertEquals(DateOffsetRangeFilterOptions.Codes.Past, filter.FilterOption);
		}

		public void TestOffsetWorkHours_MustSelectFilterOption()
		{
			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date)
			{
				PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange,
				FilterOption = ZString.Empty
			};
			AssertHasError(filter.FilterOptionInfo, "Please enter a value.");

			filter.FilterOption = "Shalala";
			AssertHasError(filter.FilterOptionInfo, "Enter a valid selection.");

			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Past;
			AssertNoErrors(filter.FilterOptionInfo);
		}

		#endregion

		#region Offset Range (days)

		[TestDate(2015, 7, 14)]
		public void TestDayOffset_Past()
		{
			var filter = SetUpDayOffsetTestDataAndFilter();
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Past;

			var query = filter.Query;
			var results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Both values were left at 0, so all dates from the past and 'Now' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "Now", "1 day in the past", "1.5 days in the past", "2 days in the past" }, results.Select(x => x.Z0_Description));

			filter.PropertyDecimal2 = 0.5;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0-0.5, so only 'Now' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "Now" }, results.Select(x => x.Z0_Description));

			filter.PropertyDecimal1 = 0.01;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0.01-0.5, so no dates should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				Array.Empty<ZString>(), results.Select(x => x.Z0_Description));

			filter.PropertyDecimal2 = 1;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0.01-1, so only '1 day in the past' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "1 day in the past" }, results.Select(x => x.Z0_Description));

			filter.PropertyDecimal1 = 0;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0-1, so 'Now' and '1 day in the past' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "Now", "1 day in the past" }, results.Select(x => x.Z0_Description));

			filter.PropertyDecimal2 = 1.49;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0-1.49, so 'Now' and '1 day in the past' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "Now", "1 day in the past" }, results.Select(x => x.Z0_Description));

			filter.PropertyDecimal2 = 1.5;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0-1.5, so 'Now', '1 day in the past', and '1.5 days in the past' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "Now", "1 day in the past", "1.5 days in the past" }, results.Select(x => x.Z0_Description));
		}

		[TestDate(2015, 7, 14)]
		public void TestDayOffset_Future()
		{
			var filter = SetUpDayOffsetTestDataAndFilter();
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;

			var query = filter.Query;
			var results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Both values were left at 0, so all dates from the future and 'Now' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "Now", "1 day in the future", "1.5 days in the future", "2 days in the future" }, results.Select(x => x.Z0_Description));

			filter.PropertyDecimal2 = 0.5;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0-0.5, so only 'Now' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "Now" }, results.Select(x => x.Z0_Description));

			filter.PropertyDecimal1 = 0.01;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0.01-0.5, so no dates should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				Array.Empty<ZString>(), results.Select(x => x.Z0_Description));

			filter.PropertyDecimal2 = 1;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0.01-1, so only '1 day in the future' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "1 day in the future" }, results.Select(x => x.Z0_Description));

			filter.PropertyDecimal1 = 0;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0-1, so 'Now' and '1 day in the future' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "Now", "1 day in the future" }, results.Select(x => x.Z0_Description));

			filter.PropertyDecimal2 = 1.49;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0-1.49, so 'Now' and '1 day in the future' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "Now", "1 day in the future" }, results.Select(x => x.Z0_Description));

			filter.PropertyDecimal2 = 1.5;
			query = filter.Query;
			results = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Entered range 0-1.5, so 'Now', '1 day in the future', and '1.5 days in the future' should have been matched, and yet..." + query.LiteralTextSqlFormatted,
				new[] { "Now", "1 day in the future", "1.5 days in the future" }, results.Select(x => x.Z0_Description));
		}

		ModuleDateFilter SetUpDayOffsetTestDataAndFilter()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyBusinessObject>();
			var dummy4 = Factory.New<DummyBusinessObject>();
			var dummy5 = Factory.New<DummyBusinessObject>();
			var dummy6 = Factory.New<DummyBusinessObject>();
			var dummy7 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_Date = ZDateTime.Now;
			dummy2.Z0_Date = ZDateTime.Now.AddDays(1);
			dummy3.Z0_Date = ZDateTime.Now.AddDays(1).AddHours(12);
			dummy4.Z0_Date = ZDateTime.Now.AddDays(2);
			dummy5.Z0_Date = ZDateTime.Now.AddDays(-1);
			dummy6.Z0_Date = ZDateTime.Now.AddDays(-1).AddHours(-12);
			dummy7.Z0_Date = ZDateTime.Now.AddDays(-2);

			dummy1.Z0_Description = "Now";
			dummy2.Z0_Description = "1 day in the future";
			dummy3.Z0_Description = "1.5 days in the future";
			dummy4.Z0_Description = "2 days in the future";
			dummy5.Z0_Description = "1 day in the past";
			dummy6.Z0_Description = "1.5 days in the past";
			dummy7.Z0_Description = "2 days in the past";

			var filterBizo = new DummyFilterBusinessObjectWithDateFilter();
			var strip = filterBizo.FilterStrips.AddNew("Dat Date");
			var filter = (ModuleDateFilter)strip.CurrentModuleFilter;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange;

			return filter;
		}

		[TestDate(2015, 7, 14)]
		public void TestMaxValues_ShouldNotCreateInvalidDateTimes()
		{
			var filterBizo = new DummyFilterBusinessObjectWithDateFilter();
			var strip = filterBizo.FilterStrips.AddNew("Dat Date");
			var filter = (ModuleDateFilter)strip.CurrentModuleFilter;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange;
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
			filter.PropertyDecimal1 = 9999;
			filter.PropertyDecimal2 = 9999;

			var query = filterBizo.Filter;
			AssertNoExceptionThrown("The max values for date offsets should not create invalid datetime values, and yet...", () => Factory.Load<DummyBusinessObject>(query));

			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Past;
			query = filterBizo.Filter;
			AssertNoExceptionThrown("The max values for date offsets should not create invalid datetime values, and yet...", () => Factory.Load<DummyBusinessObject>(query));
		}

		[TestDate(2015, 7, 14)]
		public void TestDayOffset_ChangeToOtherSearchOption_ShouldKeepPropertyValues()
		{
			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date)
			{
				PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange,
				PropertyDecimal1 = 123,
				PropertyDecimal2 = 456
			};

			AssertEquals(123m, filter.PropertyDecimal1);
			AssertEquals(456m, filter.PropertyDecimal2);
			AssertEquals(ZDateTime.Empty, filter.Property1);
			AssertEquals(ZDateTime.Empty, filter.Property2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Now;
			filter.Property2 = ZDateTime.Now;

			AssertEquals(123m, filter.PropertyDecimal1);
			AssertEquals(456m, filter.PropertyDecimal2);
			AssertEquals(new ZDateTime(2015, 7, 14), filter.Property1);
			AssertEquals(new ZDateTime(2015, 7, 14), filter.Property2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange;
			AssertEquals(123m, filter.PropertyDecimal1);
			AssertEquals(456m, filter.PropertyDecimal2);
			AssertEquals(ZDateTime.Empty, filter.Property1);
			AssertEquals(ZDateTime.Empty, filter.Property2);
		}

		public void TestDayOffset_PrecisionShouldBeStored()
		{
			var filterBizo = new DummyFilterBusinessObjectWithDateFilter();
			var strip = filterBizo.FilterStrips.AddNew("Dat Date");
			var filter = (ModuleDateFilter)strip.CurrentModuleFilter;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange;
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
			filter.PropertyDecimal1 = 1.23;
			filter.PropertyDecimal2 = 4.56;

			var layout = filterBizo.SaveLayout("I don't care!");

			var newFilterBizo = new DummyFilterBusinessObjectWithDateFilter();
			newFilterBizo.LoadLayout(layout);
			var loadedFilter = (ModuleDateFilter)newFilterBizo.ActiveModuleFilters.Single();

			AssertEquals(1.23m, loadedFilter.PropertyDecimal1);
			AssertEquals(4.56m, loadedFilter.PropertyDecimal2);
			AssertEquals(DateOffsetRangeFilterOptions.Codes.Future, filter.FilterOption);
		}

		public void TestDayOffset_MaxValueValidation()
		{
			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date)
			{
				PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange,
				PropertyDecimal1 = 9999.1,
				PropertyDecimal2 = 9999.1
			};

			AssertHasError(filter.PropertyDecimal1Info, "Please enter an 'At least value' within the range 0 to 9999.");
			AssertHasError(filter.PropertyDecimal2Info, "Please enter an 'At most value' within the range 0 to 9999.");

			filter.PropertyDecimal1 = 9999;
			filter.PropertyDecimal2 = 9999;

			AssertNoErrors(Filter.PropertyDecimal1Info);
			AssertNoErrors(Filter.PropertyDecimal2Info);
		}

		public void TestPropertyDecimal2_MustBeZeroOrGreaterThanOrEqualToPropertyDecimal1()
		{
			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date)
			{
				PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange
			};

			AssertEquals(0m, filter.PropertyDecimal1);
			AssertEquals(0m, filter.PropertyDecimal2);
			AssertNoErrors(filter.PropertyDecimal1Info);
			AssertNoErrors(filter.PropertyDecimal2Info);

			filter.PropertyDecimal1 = 1;
			AssertNoErrors(filter.PropertyDecimal1Info);
			AssertNoErrors(filter.PropertyDecimal2Info);

			filter.PropertyDecimal2 = 1;
			AssertNoErrors(filter.PropertyDecimal1Info);
			AssertNoErrors(filter.PropertyDecimal2Info);

			filter.PropertyDecimal2 = 0.9;
			AssertHasError(filter.PropertyDecimal1Info, "Please enter an 'At least value' within the range 0 to 0.9.");
		}

		public void TestPropertyDecimal1_MustBeGreaterThanOrEqualToZero()
		{
			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date)
			{
				PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange,
				PropertyDecimal1 = -1
			};

			AssertHasError(filter.PropertyDecimal1Info, "Please enter an 'At least value' within the range 0 to 9999.");

			filter.PropertyDecimal1 = 0;
			AssertNoErrors(filter.PropertyDecimal1Info);
		}

		public void TestPropertyDecimal2_MustBeGreaterThanOrEqualToZero()
		{
			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date)
			{
				PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange,
				PropertyDecimal2 = -1
			};

			AssertHasError(filter.PropertyDecimal2Info, "Please enter an 'At most value' within the range 0 to 9999.");

			filter.PropertyDecimal2 = 0;
			AssertNoErrors(filter.PropertyDecimal2Info);
		}

		public void TestOffsetDays_MustSelectFilterOption()
		{
			var filter = new ModuleDateFilter("Dat Date", DummyBizoSchema.Z0_Date)
			{
				PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange,
				FilterOption = ZString.Empty
			};
			AssertHasError(filter.FilterOptionInfo, "Please enter a value.");

			filter.FilterOption = "Shalala";
			AssertHasError(filter.FilterOptionInfo, "Enter a valid selection.");

			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Past;
			AssertNoErrors(filter.FilterOptionInfo);
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			var filter = new ModuleDateFilter("Nope. Nope. Nope.", DummyBizoSchema.Z0_Date);

			AssertEquals(DateOffsetRangeFilterOptions.Codes.Past, filter.FilterOption);
		}

		#endregion

		#region HideOffsetFilters

		public void TestHideOffsetFilters()
		{
			var filter = new ModuleDateFilter("ABC", DummyBizoSchema.Z0_Date);
			var list = filter.PropertySearch_List;
			Assert(list.ContainsCode(ModuleDateFilter.SpecifiedHourOffsetRange));

			filter = new ModuleDateFilter("ABC", DummyBizoSchema.Z0_Date);
			filter.HideOffsetFilters = true;
			list = filter.PropertySearch_List;
			Assert(!list.ContainsCode(ModuleDateFilter.SpecifiedHourOffsetRange));
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

		class TestingUserContextWithDeptSchedule : IDisposable
		{
			readonly IDisposable context;
			public TestingUserContextWithDeptSchedule(string workingHours)
			{
				var helper = ObjectFactory.Get<IDepartmentScheduleTestHelper>();
				context = helper.GetTestingUserContextWithDepartmentSchedule(workingHours);
			}

			public void Dispose()
			{
				context.Dispose();
			}
		}

		protected override ModuleDateFilter GetNewModuleFilter()
		{
			return new ModuleDateFilter("moo", DummyBizoSchema.Z0_Date);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		void AssertHourOffsetQueryWorksForPastTime(ModuleDateFilter filter, ZInt minuteOffset, ZDateTime fencepost, ZDateTime opposite, ZDateTime now, bool nowShouldBeIncludedForZeroOffset)
		{
			Assert(minuteOffset >= 0);

			var bizoLessThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNow = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			bizoLessThanFencepost.Z0_Date = fencepost.AddMinutes(-1);
			bizoEqualToFencepost.Z0_Date = fencepost;
			bizoGreaterThanFencepost.Z0_Date = fencepost.AddMinutes(1);
			bizoOnOppositeSideOfNow.Z0_Date = opposite;
			bizoWithCurrentTime.Z0_Date = now;

			// Between now and the number of hours / working hours into the past expressed by minuteOffset 
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Past;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = minuteOffset.GetDateTimeFromMinutes();

			var results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNow, results);
			AssertCollectionContains(bizoWithCurrentTime, results);

			// Anywhere >= the number of hours / working hours into the past expressed by minuteOffset
			filter.Property1 = minuteOffset.GetDateTimeFromMinutes();
			filter.Property2 = ZDateTime.Empty;

			results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNow, results);
			if (minuteOffset == 0 && nowShouldBeIncludedForZeroOffset)
			{
				AssertCollectionContains(bizoWithCurrentTime, results);
			}
			else
			{
				AssertCollectionNotContains(bizoWithCurrentTime, results);
			}
		}

		void AssertHourOffsetQueryWorksForPastTime(ModuleDateFilter filter, ZInt innerMinuteOffset, ZInt outerMinuteOffset, ZDateTime innerFencepost, ZDateTime outerFencepost, ZDateTime opposite, ZDateTime now, bool nowShouldBeIncluded)
		{
			Assert(innerMinuteOffset >= 0);
			Assert(outerMinuteOffset >= 0);

			var bizoLessThanInnerFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToInnerFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanInnerFencepost = Factory.New<DummyBusinessObject>();
			var bizoLessThanOuterFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToOuterFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanOuterFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNow = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			bizoLessThanInnerFencepost.Z0_Date = innerFencepost.AddMinutes(-1);
			bizoEqualToInnerFencepost.Z0_Date = innerFencepost;
			bizoGreaterThanInnerFencepost.Z0_Date = innerFencepost.AddMinutes(1);
			bizoLessThanOuterFencepost.Z0_Date = outerFencepost.AddMinutes(-1);
			bizoEqualToOuterFencepost.Z0_Date = outerFencepost;
			bizoGreaterThanOuterFencepost.Z0_Date = outerFencepost.AddMinutes(1);
			bizoOnOppositeSideOfNow.Z0_Date = opposite;
			bizoWithCurrentTime.Z0_Date = now;

			// Within the range of hours / working hours in the past expressed by innerMinuteOffset and outerMinuteOffset
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Past;
			filter.Property1 = innerMinuteOffset.GetDateTimeFromMinutes();
			filter.Property2 = outerMinuteOffset.GetDateTimeFromMinutes();

			var results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertCollectionContains(bizoLessThanInnerFencepost, results);
			AssertCollectionContains(bizoEqualToInnerFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanInnerFencepost, results);
			AssertCollectionNotContains(bizoLessThanOuterFencepost, results);
			AssertCollectionContains(bizoEqualToOuterFencepost, results);
			AssertCollectionContains(bizoGreaterThanOuterFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNow, results);
			if (nowShouldBeIncluded)
			{
				AssertCollectionContains(bizoWithCurrentTime, results);
			}
			else
			{
				AssertCollectionNotContains(bizoWithCurrentTime, results);
			}
		}

		void AssertHourOffsetQueryWorksForFutureTime(ModuleDateFilter filter, ZInt minuteOffset, ZDateTime fencepost, ZDateTime opposite, ZDateTime now, bool nowShouldBeIncludedForZeroOffset)
		{
			Assert(minuteOffset >= 0);

			var bizoLessThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNow = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			bizoLessThanFencepost.Z0_Date = fencepost.AddMinutes(-1);
			bizoEqualToFencepost.Z0_Date = fencepost;
			bizoGreaterThanFencepost.Z0_Date = fencepost.AddMinutes(1);
			bizoOnOppositeSideOfNow.Z0_Date = opposite;
			bizoWithCurrentTime.Z0_Date = now;

			// Between now and the number of hours / working hours into the future expressed by minuteOffset
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = minuteOffset.GetDateTimeFromMinutes();

			var results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertCollectionContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNow, results);
			AssertCollectionContains(bizoWithCurrentTime, results);

			// Anywhere >= the number of hours / working hours into the future expressed by minuteOffset
			filter.Property1 = minuteOffset.GetDateTimeFromMinutes();
			filter.Property2 = ZDateTime.Empty;

			results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertCollectionNotContains(bizoLessThanFencepost, results);
			AssertCollectionContains(bizoEqualToFencepost, results);
			AssertCollectionContains(bizoGreaterThanFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNow, results);
			if (minuteOffset == 0 && nowShouldBeIncludedForZeroOffset)
			{
				AssertCollectionContains(bizoWithCurrentTime, results);
			}
			else
			{
				AssertCollectionNotContains(bizoWithCurrentTime, results);
			}
		}

		void AssertHourOffsetQueryWorksForFutureTime(ModuleDateFilter filter, ZInt innerMinuteOffset, ZInt outerMinuteOffset, ZDateTime innerFencepost, ZDateTime outerFencepost, ZDateTime opposite, ZDateTime now, bool nowShouldBeIncluded)
		{
			Assert(innerMinuteOffset >= 0);
			Assert(outerMinuteOffset >= 0);

			var bizoLessThanInnerFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToInnerFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanInnerFencepost = Factory.New<DummyBusinessObject>();
			var bizoLessThanOuterFencepost = Factory.New<DummyBusinessObject>();
			var bizoEqualToOuterFencepost = Factory.New<DummyBusinessObject>();
			var bizoGreaterThanOuterFencepost = Factory.New<DummyBusinessObject>();
			var bizoOnOppositeSideOfNow = Factory.New<DummyBusinessObject>();
			var bizoWithCurrentTime = Factory.New<DummyBusinessObject>();

			bizoLessThanInnerFencepost.Z0_Date = innerFencepost.AddMinutes(-1);
			bizoEqualToInnerFencepost.Z0_Date = innerFencepost;
			bizoGreaterThanInnerFencepost.Z0_Date = innerFencepost.AddMinutes(1);
			bizoLessThanOuterFencepost.Z0_Date = outerFencepost.AddMinutes(-1);
			bizoEqualToOuterFencepost.Z0_Date = outerFencepost;
			bizoGreaterThanOuterFencepost.Z0_Date = outerFencepost.AddMinutes(1);
			bizoOnOppositeSideOfNow.Z0_Date = opposite;
			bizoWithCurrentTime.Z0_Date = now;

			// Within the range of hours / working hours in the past expressed by innerMinuteOffset and outerMinuteOffset
			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
			filter.Property1 = innerMinuteOffset.GetDateTimeFromMinutes();
			filter.Property2 = outerMinuteOffset.GetDateTimeFromMinutes();

			var results = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertCollectionNotContains(bizoLessThanInnerFencepost, results);
			AssertCollectionContains(bizoEqualToInnerFencepost, results);
			AssertCollectionContains(bizoGreaterThanInnerFencepost, results);
			AssertCollectionContains(bizoLessThanOuterFencepost, results);
			AssertCollectionContains(bizoEqualToOuterFencepost, results);
			AssertCollectionNotContains(bizoGreaterThanOuterFencepost, results);
			AssertCollectionNotContains(bizoOnOppositeSideOfNow, results);
			if (nowShouldBeIncluded)
			{
				AssertCollectionContains(bizoWithCurrentTime, results);
			}
			else
			{
				AssertCollectionNotContains(bizoWithCurrentTime, results);
			}
		}

		#endregion
	}
}
