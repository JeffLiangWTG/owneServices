using System.Collections.Immutable;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobManagementModuleDateFilter))]
	public class JobManagementModuleDateFilterTest : ModuleDateFilterTest
	{
		protected override ModuleDateFilter GetNewModuleFilter()
		{
			return new JobManagementModuleDateFilter("moo", DummyBizoSchema.Z0_Date);
		}

		protected override ImmutableArray<string> ExpectedPropertySearchListCore
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
					Enterprise.ZArchitecture.Business.ModuleDateFilter.Past,

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
					Enterprise.ZArchitecture.Business.ModuleDateFilter.Future,

					string.Empty,
					"Ranges Category",
					ModuleDateFilter.SpecifiedDateRange,
					ModuleDateFilter.SpecifiedDateTimeRange,
					ModuleDateFilter.SpecifiedHourOffsetRange,
					ModuleDateFilter.SpecifiedWorkHourOffsetRange,
					ModuleDateFilter.SpecifiedDayOffsetRange,

					string.Empty,
					"Date Entered Category",
					ModuleDateFilter.HasDateEntered
				);
			}
		}

		public void TestPropertySearch_ListCoreForJobManagement()
		{
			var revenueRecognitionDateFilter = (JobManagementModuleDateFilter)Filter;
			var revenueRecognitionDateSearchList = revenueRecognitionDateFilter.PropertySearch_List;

			AssertEquals("Search list item count", this.ExpectedPropertySearchListCore.Length, revenueRecognitionDateSearchList.Count);
			AssertCollectionNotContains("Job Revenue Recognition Date search list does not contain 'Has No Date'", ModuleDateFilter.HasNoDateEntered, revenueRecognitionDateSearchList);
		}

		public override void TestPropertySearchUsesHideFutureDatesFlag()
		{
			AssertEquals("HideFutureDates", false, Filter.HideFutureDates);
			AssertEquals("PropertySearch_List.Count", 39, Filter.PropertySearch_List.Count);
			AssertEquals("PropertySearch_List.ContainsCode(\"Tomorrow\")", true, Filter.PropertySearch_List.ContainsCode("Tomorrow"));

			Filter.HideFutureDates = true;
			AssertEquals("PropertySearch_List.Count", 26, Filter.PropertySearch_List.Count);
			AssertEquals("PropertySearch_List.ContainsCode(\"Tomorrow\")", false, Filter.PropertySearch_List.ContainsCode("Tomorrow"));

			Filter.HideFutureDates = false;
			AssertEquals("PropertySearch_List.Count", 39, Filter.PropertySearch_List.Count);
			AssertEquals("PropertySearch_List.ContainsCode(\"Tomorrow\")", true, Filter.PropertySearch_List.ContainsCode("Tomorrow"));
		}

		public override void TestPropertySearch_ListContainsDateEntered()
		{
			AssertEquals(true, Filter.PropertySearch_List.ContainsCode(ModuleDateFilter.HasDateEntered));
		}
	}
}
