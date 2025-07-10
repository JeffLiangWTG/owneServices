using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Business.ModuleDateFilter;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDateRangeControlTest : TestCase
	{
		public void TestVisibility()
		{
			PrepareDateRangeControl("Date Range");

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(true, propertySearchDropEdit.Visible);
			AssertEquals(false, atLeastHoursEdit.Visible);
			AssertEquals(false, atMostHoursEdit.Visible);
			AssertEquals(false, filterOptionDropEdit.Visible);
			AssertEquals(true, fromEdit.Visible);
			AssertEquals(true, toEdit.Visible);
			AssertEquals(false, atLeastDaysEdit.Visible);
			AssertEquals(false, atMostDaysEdit.Visible);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			AssertEquals(true, propertySearchDropEdit.Visible);
			AssertEquals(false, atLeastHoursEdit.Visible);
			AssertEquals(false, atMostHoursEdit.Visible);
			AssertEquals(false, filterOptionDropEdit.Visible);
			AssertEquals(true, fromEdit.Visible);
			AssertEquals(true, toEdit.Visible);
			AssertEquals(false, atLeastDaysEdit.Visible);
			AssertEquals(false, atMostDaysEdit.Visible);

			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			AssertEquals(true, propertySearchDropEdit.Visible);
			AssertEquals(true, atLeastHoursEdit.Visible);
			AssertEquals(true, atMostHoursEdit.Visible);
			AssertEquals(true, filterOptionDropEdit.Visible);
			AssertEquals(false, fromEdit.Visible);
			AssertEquals(false, toEdit.Visible);
			AssertEquals(false, atLeastDaysEdit.Visible);
			AssertEquals(false, atMostDaysEdit.Visible);

			filter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;
			AssertEquals(true, propertySearchDropEdit.Visible);
			AssertEquals(true, atLeastHoursEdit.Visible);
			AssertEquals(true, atMostHoursEdit.Visible);
			AssertEquals(true, filterOptionDropEdit.Visible);
			AssertEquals(false, fromEdit.Visible);
			AssertEquals(false, toEdit.Visible);
			AssertEquals(false, atLeastDaysEdit.Visible);
			AssertEquals(false, atMostDaysEdit.Visible);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange;
			AssertEquals(true, propertySearchDropEdit.Visible);
			AssertEquals(false, atLeastHoursEdit.Visible);
			AssertEquals(false, atMostHoursEdit.Visible);
			AssertEquals(true, filterOptionDropEdit.Visible);
			AssertEquals(false, fromEdit.Visible);
			AssertEquals(false, toEdit.Visible);
			AssertEquals(true, atLeastDaysEdit.Visible);
			AssertEquals(true, atMostDaysEdit.Visible);
		}

		public void TestFilterOptions()
		{
			PrepareDateRangeControl("Date Range");

			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			AssertEquals(2, filterOptionDropEdit.List.Count);
			AssertEquals(DateOffsetRangeFilterOptions.Codes.Future, ((CodeDescriptionPair)filterOptionDropEdit.List[0]).Code);
			AssertEquals(DateOffsetRangeFilterOptions.Codes.Past, ((CodeDescriptionPair)filterOptionDropEdit.List[1]).Code);

			filter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;
			AssertEquals(2, filterOptionDropEdit.List.Count);
			AssertEquals(DateOffsetRangeFilterOptions.Codes.Future, ((CodeDescriptionPair)filterOptionDropEdit.List[0]).Code);
			AssertEquals(DateOffsetRangeFilterOptions.Codes.Past, ((CodeDescriptionPair)filterOptionDropEdit.List[1]).Code);
		}

		public void TestVisibleInMultilanguages()
		{
			using (var mockChs = Res.GetLanguageInstance(SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("Filter|DateRangeSearchList|SpecifiedDateRangeFilter", new ResourceStringData("Filter|DateRangeSearchList|SpecifiedDateRangeFilter", "指定日期范围"));
				mockChs.Put("Filter|DateRangeSearchList|SpecifiedTimeRangeFilter", new ResourceStringData("Filter|DateRangeSearchList|SpecifiedTimeRangeFilter", "指定时间范围"));
				mockChs.Put("Filter|DateRangeSearchList|SpecifiedHourRangeFilter", new ResourceStringData("Filter|DateRangeSearchList|SpecifiedHourRangeFilter", "指定小时范围"));
				mockChs.Put("Filter|DateRangeSearchList|SpecifiedWorkHourRangeFilter", new ResourceStringData("Filter|DateRangeSearchList|SpecifiedWorkHourRangeFilter", "指定工作时间范围"));
				mockChs.Put("Filter|DateRangeSearchList|SpecifiedDayRangeFilter", new ResourceStringData("Filter|DateRangeSearchList|SpecifiedDayRangeFilter", "指定天数"));

				using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
				{
					PrepareDateRangeControl("Date Range");

					filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
					AssertEquals(true, propertySearchDropEdit.Visible);
					AssertEquals(false, atLeastHoursEdit.Visible);
					AssertEquals(false, atMostHoursEdit.Visible);
					AssertEquals(false, filterOptionDropEdit.Visible);
					AssertEquals(true, fromEdit.Visible);
					AssertEquals(true, toEdit.Visible);
					AssertEquals(false, atLeastDaysEdit.Visible);
					AssertEquals(false, atMostDaysEdit.Visible);

					filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					AssertEquals(true, propertySearchDropEdit.Visible);
					AssertEquals(false, atLeastHoursEdit.Visible);
					AssertEquals(false, atMostHoursEdit.Visible);
					AssertEquals(false, filterOptionDropEdit.Visible);
					AssertEquals(true, fromEdit.Visible);
					AssertEquals(true, toEdit.Visible);
					AssertEquals(false, atLeastDaysEdit.Visible);
					AssertEquals(false, atMostDaysEdit.Visible);

					filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
					AssertEquals(true, propertySearchDropEdit.Visible);
					AssertEquals(true, atLeastHoursEdit.Visible);
					AssertEquals(true, atMostHoursEdit.Visible);
					AssertEquals(true, filterOptionDropEdit.Visible);
					AssertEquals(false, fromEdit.Visible);
					AssertEquals(false, toEdit.Visible);
					AssertEquals(false, atLeastDaysEdit.Visible);
					AssertEquals(false, atMostDaysEdit.Visible);

					filter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;
					AssertEquals(true, propertySearchDropEdit.Visible);
					AssertEquals(true, atLeastHoursEdit.Visible);
					AssertEquals(true, atMostHoursEdit.Visible);
					AssertEquals(true, filterOptionDropEdit.Visible);
					AssertEquals(false, fromEdit.Visible);
					AssertEquals(false, toEdit.Visible);
					AssertEquals(false, atLeastDaysEdit.Visible);
					AssertEquals(false, atMostDaysEdit.Visible);

					filter.PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange;
					AssertEquals(true, propertySearchDropEdit.Visible);
					AssertEquals(false, atLeastHoursEdit.Visible);
					AssertEquals(false, atMostHoursEdit.Visible);
					AssertEquals(true, filterOptionDropEdit.Visible);
					AssertEquals(false, fromEdit.Visible);
					AssertEquals(false, toEdit.Visible);
					AssertEquals(true, atLeastDaysEdit.Visible);
					AssertEquals(true, atMostDaysEdit.Visible);
				}
			}
		}

		[TestDate(2018, 12, 20)]
		public void TestPropertySearchDropEditFormatting()
		{
			using (var mockChs = Res.GetLanguageInstance(SharedConstants.Languages.Arabic).UseMockData())
			{
				mockChs.Put("ddaa66a4-af9b-4bdf-bcf6-7aeadd5175c5", new ResourceStringData("ddaa66a4-af9b-4bdf-bcf6-7aeadd5175c5", "{0} إلى {1}"));

				using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Arabic))
				{
					PrepareDateRangeControl("Date Range");

					filter.PropertySearch = DateRangeSearchTexts.Next7Days;
					propertySearchDropEdit.CodeBox.Text = "Next 7 Days";
					propertySearchDropEdit.CodeBox.UpdateSelectedIndexAndDescription();
					AssertEquals("20-Dec-18 إلى‎ 26-Dec-18", propertySearchDropEdit.DescriptionBox.Text);
				}
			}
		}

		#region Implementation

		void PrepareDateRangeControl(string dateRangeType)
		{
			filterStripBizO = new MockFilterStripBizO();
			mockForm = new MockFilterStripForm(filterStripBizO);
			mockForm.Show();
			zFilterStrip = mockForm.AddFilterStrip(dateRangeType);
			filterStrip = (FilterStrip)filterStripBizO.FilterStrips.Last();
			filter = (ModuleDateFilter)filterStrip.CurrentModuleFilter;

			AssertEquals("zFilterStrip.Controls.Count", 6, zFilterStrip.Controls.Count);
			dateRangeControl = (ZDateRangeControl)zFilterStrip.Controls[5];

			AssertEquals("dateRangeControl.Controls.Count", 8, dateRangeControl.Controls.Count);
			propertySearchDropEdit = (ZFilterStripDropEdit)dateRangeControl.Controls[5];
			atLeastHoursEdit = (ZTimeEditEx)dateRangeControl.Controls[1];
			atMostHoursEdit = (ZTimeEditEx)dateRangeControl.Controls[2];
			filterOptionDropEdit = (ZFilterStripDropEdit)dateRangeControl.Controls[0];
			fromEdit = (ZFilterStripDateEdit)dateRangeControl.Controls[6];
			toEdit = (ZFilterStripDateEdit)dateRangeControl.Controls[7];
			atLeastDaysEdit = (ZCalcEdit)dateRangeControl.Controls[3];
			atMostDaysEdit = (ZCalcEdit)dateRangeControl.Controls[4];
		}

		protected override void TearDown()
		{
			if (mockForm != null)
			{
				mockForm.Dispose();
			}

			base.TearDown();
		}

		MockFilterStripBizO filterStripBizO;
		FilterStrip filterStrip;
		ModuleDateFilter filter;
		MockFilterStripForm mockForm;
		ZFilterStrip zFilterStrip;
		ZDateRangeControl dateRangeControl;
		ZFilterStripDropEdit propertySearchDropEdit;
		ZFilterStripDateEdit fromEdit;
		ZFilterStripDateEdit toEdit;
		ZTimeEditEx atLeastHoursEdit;
		ZTimeEditEx atMostHoursEdit;
		ZCalcEdit atLeastDaysEdit;
		ZCalcEdit atMostDaysEdit;
		ZFilterStripDropEdit filterOptionDropEdit;

		#endregion
	}
}
