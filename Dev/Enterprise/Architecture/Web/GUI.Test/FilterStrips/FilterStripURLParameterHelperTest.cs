using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.Web.Business.FilterStrips.Testing
{
	sealed class FilterStripURLParameterHelperTest : TestCaseWithFactory
	{
		#region TestSetupFilterStripsFromQueryString

		public void TestFilterStripSetup_Date()
		{
			ZString datePresetString1 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + ModuleDateFilter.DateRangeSearchTexts.Today;
			ZString datePresetString2 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + ModuleDateFilter.DateRangeSearchTexts.Yesterday;
			ZString datePresetString3 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + ModuleDateFilter.DateRangeSearchTexts.LastMonth;
			ZString datePresetString4 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + ModuleDateFilter.DateRangeSearchTexts.NextWeek;
			ZString invalidDatePreset1 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + "green";
			ZString invalidDatePreset2 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + "1094";
			ZString dateString1 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + "01-MAR-08";
			ZString dateString2 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + "01-MAR-08,12-FEB-09";
			ZString dateString3 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + ",12-FEB-09";
			ZString invalidDateString1 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + "monday";
			ZString invalidDateString2 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + "1.34,green";
			ZString invalidDateString3 = FilterStripBusinessObjectForTest.Schema.TestDateFilterName + "=" + ",true";
			ZDateTime expectedDate1 = new ZDateTime(2008, 03, 01);
			ZDateTime expectedDate2 = new ZDateTime(2009, 02, 12);

			ModuleDateFilter dateFilter = (ModuleDateFilter)FilterStrip[FilterStripBusinessObjectForTest.Schema.TestDateFilterName];
			AssertDatePreset(FilterStrip, dateFilter, datePresetString1, true, ModuleDateFilter.DateRangeSearchTexts.Today);
			AssertDatePreset(FilterStrip, dateFilter, datePresetString2, true, ModuleDateFilter.DateRangeSearchTexts.Yesterday);
			AssertDatePreset(FilterStrip, dateFilter, datePresetString3, true, ModuleDateFilter.DateRangeSearchTexts.LastMonth);
			AssertDatePreset(FilterStrip, dateFilter, datePresetString4, true, ModuleDateFilter.DateRangeSearchTexts.NextWeek);

			AssertDatePreset(FilterStrip, dateFilter, invalidDatePreset1, false, ZString.Empty);
			AssertDatePreset(FilterStrip, dateFilter, invalidDatePreset2, false, ZString.Empty);

			AssertDateRange(FilterStrip, dateFilter, dateString1, true, expectedDate1, ZDateTime.Empty);
			AssertDateRange(FilterStrip, dateFilter, dateString2, true, expectedDate1, expectedDate2);
			AssertDateRange(FilterStrip, dateFilter, dateString3, true, ZDateTime.Empty, expectedDate2);

			AssertDateRange(FilterStrip, dateFilter, invalidDateString1, false, ZDateTime.Empty, ZDateTime.Empty);
			AssertDateRange(FilterStrip, dateFilter, invalidDateString2, false, ZDateTime.Empty, ZDateTime.Empty);
			AssertDateRange(FilterStrip, dateFilter, invalidDateString3, false, ZDateTime.Empty, ZDateTime.Empty);
		}

		public void TestFilterStripSetup_Guid()
		{
			ZString guidString1 = FilterStripBusinessObjectForTest.Schema.TestGuidFilterName + "=" + testOrg.OH_Code;
			ZString guidString2 = FilterStripBusinessObjectForTest.Schema.TestGuidFilterName + "=" + testOrg.PK;
			ZString invalidGuid = FilterStripBusinessObjectForTest.Schema.TestGuidFilterName + "=" + ZGuid.Invalid;
			ZString invalidOHCode = FilterStripBusinessObjectForTest.Schema.TestGuidFilterName + "=" + ZGuid.Invalid;
			ZGuid expectedGuid1 = testOrg.PK;

			ModuleGuidFilter guidFilter = (ModuleGuidFilter)FilterStrip[FilterStripBusinessObjectForTest.Schema.TestGuidFilterName];
			AssertGuidFilter(FilterStrip, guidFilter, guidString1, true, expectedGuid1);
			AssertGuidFilter(FilterStrip, guidFilter, guidString2, true, expectedGuid1);
		}

		public void TestFilterStripSetup_Guids()
		{
			ZString guidsString1 = FilterStripBusinessObjectForTest.Schema.TestGuidsFilterName + "=" + testOrg.OH_Code;
			ZString guidsString2 = FilterStripBusinessObjectForTest.Schema.TestGuidsFilterName + "=" + testOrg.PK;
			ZString guidsString3 = FilterStripBusinessObjectForTest.Schema.TestGuidsFilterName + "=" + testOrg.OH_Code + "," + testOrg2.OH_Code;
			ZString guidsString4 = FilterStripBusinessObjectForTest.Schema.TestGuidsFilterName + "=" + testOrg.OH_Code + "," + testOrg2.PK;
			ZString guidsString5 = FilterStripBusinessObjectForTest.Schema.TestGuidsFilterName + "=" + testOrg.PK + "," + testOrg2.OH_Code;
			ZString guidsString6 = FilterStripBusinessObjectForTest.Schema.TestGuidsFilterName + "=" + testOrg.PK + "," + testOrg2.PK;
			ZString guidsString7 = FilterStripBusinessObjectForTest.Schema.TestGuidsFilterName + "=" + "," + testOrg2.OH_Code;
			ZString guidsString8 = FilterStripBusinessObjectForTest.Schema.TestGuidsFilterName + "=" + "," + testOrg2.PK;
			ZGuid expectedGuid1 = testOrg.PK;
			ZGuid expectedGuid2 = testOrg2.PK;

			ModuleGuidsFilter guidsFilter = (ModuleGuidsFilter)FilterStrip[FilterStripBusinessObjectForTest.Schema.TestGuidsFilterName];
			AssertGuidsFilter(FilterStrip, guidsFilter, guidsString1, true, expectedGuid1, ZGuid.Empty);
			AssertGuidsFilter(FilterStrip, guidsFilter, guidsString2, true, expectedGuid1, ZGuid.Empty);
			AssertGuidsFilter(FilterStrip, guidsFilter, guidsString3, true, expectedGuid1, expectedGuid2);
			AssertGuidsFilter(FilterStrip, guidsFilter, guidsString4, true, expectedGuid1, expectedGuid2);
			AssertGuidsFilter(FilterStrip, guidsFilter, guidsString5, true, expectedGuid1, expectedGuid2);
			AssertGuidsFilter(FilterStrip, guidsFilter, guidsString6, true, expectedGuid1, expectedGuid2);
			AssertGuidsFilter(FilterStrip, guidsFilter, guidsString7, true, ZGuid.Empty, expectedGuid2);
			AssertGuidsFilter(FilterStrip, guidsFilter, guidsString8, true, ZGuid.Empty, expectedGuid2);
		}

		public void TestFilterStripSetup_Location()
		{
			ZString locationString1 = FilterStripBusinessObjectForTest.Schema.TestLocationFilterName + "=" + "AUSTM";
			ZString locationString2 = FilterStripBusinessObjectForTest.Schema.TestLocationFilterName + "=" + "AUSTM" + "," + "AUSYD";
			ZString locationString3 = FilterStripBusinessObjectForTest.Schema.TestLocationFilterName + "=" + "," + "AUSYD";
			ZString expectedLocation1 = "AUSTM";
			ZString expectedLocation2 = "AUSYD";

			ModuleLocationFilter locationFilter = (ModuleLocationFilter)FilterStrip[FilterStripBusinessObjectForTest.Schema.TestLocationFilterName];
			AssertLocationFilter(FilterStrip, locationFilter, locationString1, true, expectedLocation1, ZString.Empty);
			AssertLocationFilter(FilterStrip, locationFilter, locationString2, true, expectedLocation1, expectedLocation2);
			AssertLocationFilter(FilterStrip, locationFilter, locationString3, true, ZString.Empty, expectedLocation2);
		}

		public void TestFilterStripSetup_Number()
		{
			ZString numberString1 = FilterStripBusinessObjectForTest.Schema.TestNumberFilterName + "=" + "abc";
			ZString numberString2 = FilterStripBusinessObjectForTest.Schema.TestNumberFilterName + "=" + "123";
			ZString numberString3 = FilterStripBusinessObjectForTest.Schema.TestNumberFilterName + "=" + "1.23";
			ZString expectedString1 = "abc";
			ZString expectedString2 = "123";
			ZString expectedString3 = "1.23";

			ModuleNumberFilter numberFilter = (ModuleNumberFilter)FilterStrip[FilterStripBusinessObjectForTest.Schema.TestNumberFilterName];
			AssertNumberFilter(FilterStrip, numberFilter, numberString1, true, expectedString1);
			AssertNumberFilter(FilterStrip, numberFilter, numberString2, true, expectedString2);
			AssertNumberFilter(FilterStrip, numberFilter, numberString3, true, expectedString3);
		}

		public void TestFilterStripSetup_NumberRange()
		{
			ZString numberRangeString1 = FilterStripBusinessObjectForTest.Schema.TestNumberRangeFilterName + "=" + "123";
			ZString numberRangeString2 = FilterStripBusinessObjectForTest.Schema.TestNumberRangeFilterName + "=" + "123" + "," + "456";
			ZString numberRangeString3 = FilterStripBusinessObjectForTest.Schema.TestNumberRangeFilterName + "=" + "," + "456";
			ZDecimal expectedNumber1 = 123m;
			ZDecimal expectedNumber2 = 456m;

			ModuleNumberRangeFilter numberRangeFilter = (ModuleNumberRangeFilter)FilterStrip[FilterStripBusinessObjectForTest.Schema.TestNumberRangeFilterName];
			AssertNumberRangeFilter(FilterStrip, numberRangeFilter, numberRangeString1, true, expectedNumber1, expectedNumber1);
			AssertNumberRangeFilter(FilterStrip, numberRangeFilter, numberRangeString2, true, expectedNumber1, expectedNumber2);
			AssertNumberRangeFilter(FilterStrip, numberRangeFilter, numberRangeString3, true, expectedNumber2, expectedNumber2);
		}

		public void TestFilterStripSetup_SingleDate()
		{
			ZString dateString1 = FilterStripBusinessObjectForTest.Schema.TestSingleDateFilterName + "=" + "1-APR-08";
			ZString dateString2 = FilterStripBusinessObjectForTest.Schema.TestSingleDateFilterName + "=" + "123";
			ZDateTime expectedDate = new ZDateTime(2008, 04, 01);
			ZDateTime invalidDate = ZDateTime.Empty;

			ModuleSingleDateFilter singleDateFilter = (ModuleSingleDateFilter)FilterStrip[FilterStripBusinessObjectForTest.Schema.TestSingleDateFilterName];
			AssertSingleDateFilter(FilterStrip, singleDateFilter, dateString1, true, expectedDate);
			AssertSingleDateFilter(FilterStrip, singleDateFilter, dateString2, false, invalidDate);
		}

		public void TestFilterStripSetup_Text()
		{
			ZString textString1 = FilterStripBusinessObjectForTest.Schema.TestTextFilterName + "=" + "abc";
			ZString textString2 = FilterStripBusinessObjectForTest.Schema.TestTextFilterName + "=" + "123";
			ZString textString3 = FilterStripBusinessObjectForTest.Schema.TestTextFilterName + "=" + "1-APR-08";
			ZString textString4 = FilterStripBusinessObjectForTest.Schema.TestTextFilterName + "=" + "c1.043";
			ZString expectedString1 = "abc";
			ZString expectedString2 = "123";
			ZString expectedString3 = "1-APR-08";
			ZString expectedString4 = "c1.043";

			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[FilterStripBusinessObjectForTest.Schema.TestTextFilterName];
			AssertTextFilter(FilterStrip, filter, textString1, true, expectedString1);
			AssertTextFilter(FilterStrip, filter, textString2, true, expectedString2);
			AssertTextFilter(FilterStrip, filter, textString3, true, expectedString3);
			AssertTextFilter(FilterStrip, filter, textString4, true, expectedString4);
		}

		public void TestFilterStripSetup_TextAndNk()
		{
			ZString string1 = FilterStripBusinessObjectForTest.Schema.TestTextAndNkFilterName + "=" + "Text";
			ZString string2 = FilterStripBusinessObjectForTest.Schema.TestTextAndNkFilterName + "=" + "Text" + "," + "Nk";
			ZString string3 = FilterStripBusinessObjectForTest.Schema.TestTextAndNkFilterName + "=" + "," + "Nk";
			ZString expectedText = "Text";
			ZString expectedNk = "Nk";

			ModuleTextAndNkFilter filter = (ModuleTextAndNkFilter)FilterStrip[FilterStripBusinessObjectForTest.Schema.TestTextAndNkFilterName];
			AssertTextAndNkFilter(FilterStrip, filter, string1, true, expectedText, ZString.Empty);
			AssertTextAndNkFilter(FilterStrip, filter, string2, true, expectedText, expectedNk);
			AssertTextAndNkFilter(FilterStrip, filter, string3, true, ZString.Empty, expectedNk);
		}

		public void TestFilterStripSetup_TextRange()
		{
			ZString string1 = FilterStripBusinessObjectForTest.Schema.TestTextRangeFilterName + "=" + "Text1";
			ZString string2 = FilterStripBusinessObjectForTest.Schema.TestTextRangeFilterName + "=" + "Text1" + "," + "Text2";
			ZString string3 = FilterStripBusinessObjectForTest.Schema.TestTextRangeFilterName + "=" + "," + "Text2";
			ZString expectedText1 = "Text1";
			ZString expectedText2 = "Text2";

			ModuleTextRangeFilter filter = (ModuleTextRangeFilter)FilterStrip[FilterStripBusinessObjectForTest.Schema.TestTextRangeFilterName];
			AssertTextRangeFilter(FilterStrip, filter, string1, true, expectedText1, ZString.Empty);
			AssertTextRangeFilter(FilterStrip, filter, string2, true, expectedText1, expectedText2);
			AssertTextRangeFilter(FilterStrip, filter, string3, true, ZString.Empty, expectedText2);
		}

		#endregion

		#region Assert methods

		void AssertDatePreset(FilterStripBusinessObject filterBizO, ModuleDateFilter filter, ZString queryString, bool expectedActive, ZString expectedPropertySearch)
		{
			Helper.SetupFilterStripsFromQueryString(filterBizO, queryString);
			AssertEquals("Date filter active", expectedActive, filter.IsActive);
			AssertEquals("Date filter property search set", expectedPropertySearch, filter.PropertySearch);
			ResetDateFilter(filter);
		}

		void AssertDateRange(FilterStripBusinessObject filterBizO, ModuleDateFilter filter, ZString queryString, bool expectedActive, ZDateTime expectedDate1, ZDateTime expectedDate2)
		{
			Helper.SetupFilterStripsFromQueryString(filterBizO, queryString);
			AssertEquals("Date filter active", expectedActive, filter.IsActive);
			if (expectedActive)
			{
				AssertEquals("Date filter property search is daterange", ModuleDateFilter.SpecifiedDateRange, filter.PropertySearch);
			}
			AssertEquals("Date filter property 1 set", expectedDate1.ToShortDateString(), filter.Property1.ToShortDateString());
			AssertEquals("Date filter property 2 set", expectedDate2.ToShortDateString(), filter.Property2.ToShortDateString());
			ResetDateFilter(filter);
		}

		void ResetDateFilter(ModuleDateFilter filter)
		{
			filter.IsActive = false;
			filter.PropertySearch = "";
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
		}

		void AssertGuidFilter(FilterStripBusinessObject filterBizO, ModuleGuidFilter filter, ZString queryString, bool expectedActive, ZGuid expectedGuid)
		{
			Helper.SetupFilterStripsFromQueryString(filterBizO, queryString);
			AssertEquals("Guid filter active", expectedActive, filter.IsActive);
			AssertEquals("Guid filter property set", expectedGuid, filter.Property);
			filter.IsActive = false;
			filter.Property = ZGuid.Empty;
		}

		void AssertGuidsFilter(FilterStripBusinessObject filterBizO, ModuleGuidsFilter filter, ZString queryString, bool expectedActive, ZGuid expectedGuid1, ZGuid expectedGuid2)
		{
			Helper.SetupFilterStripsFromQueryString(filterBizO, queryString);
			AssertEquals("Guids filter active", expectedActive, filter.IsActive);
			AssertEquals("Guids filter property set", expectedGuid1, filter.Property1);
			AssertEquals("Guids filter property set", expectedGuid2, filter.Property2);
			filter.IsActive = false;
			filter.Property1 = ZGuid.Empty;
			filter.Property2 = ZGuid.Empty;
		}

		void AssertLocationFilter(FilterStripBusinessObject filterBizO, ModuleLocationFilter filter, ZString queryString, bool expectedActive, ZString expectedLocation1, ZString expectedLocation2)
		{
			Helper.SetupFilterStripsFromQueryString(filterBizO, queryString);
			AssertEquals("Location filter active", expectedActive, filter.IsActive);
			AssertEquals("Location filter property 1 set", expectedLocation1, filter.Property1);
			AssertEquals("Location filter property 2 set", expectedLocation2, filter.Property2);
			filter.IsActive = false;
			filter.Property1 = ZString.Empty;
			filter.Property2 = ZString.Empty;
		}

		void AssertNumberFilter(FilterStripBusinessObject filterBizO, ModuleNumberFilter filter, ZString queryString, bool expectedActive, ZString expectedNumber)
		{
			Helper.SetupFilterStripsFromQueryString(filterBizO, queryString);
			AssertEquals("Number filter active", expectedActive, filter.IsActive);
			AssertEquals("Number filter property set", expectedNumber, filter.Property);
			filter.IsActive = false;
			filter.Property = ZString.Empty;
		}

		void AssertNumberRangeFilter(FilterStripBusinessObjectForTest filterBizO, ModuleNumberRangeFilter filter, ZString queryString, bool expectedActive, ZDecimal expectedNumber1, ZDecimal expectedNumber2)
		{
			Helper.SetupFilterStripsFromQueryString(filterBizO, queryString);
			AssertEquals("Number range filter active", expectedActive, filter.IsActive);
			AssertEquals("Number range filter property 1 set", expectedNumber1, filter.Property1);
			AssertEquals("Number range filter property 2 set", expectedNumber2, filter.Property2);
			filter.IsActive = false;
			filter.Property1 = ZDecimal.Zero;
			filter.Property2 = ZDecimal.Zero;
		}

		void AssertSingleDateFilter(FilterStripBusinessObjectForTest filterBizO, ModuleSingleDateFilter filter, ZString queryString, bool expectedActive, ZDateTime expectedDateTime)
		{
			Helper.SetupFilterStripsFromQueryString(filterBizO, queryString);
			AssertEquals("Single Date filter active", expectedActive, filter.IsActive);
			AssertEquals("Single Date filter property set", expectedDateTime, filter.Property1);
			filter.IsActive = false;
			filter.Property1 = ZDateTime.Empty;
		}

		void AssertTextFilter(FilterStripBusinessObjectForTest filterBizO, ModuleTextFilter filter, ZString queryString, bool expectedActive, ZString expectedString)
		{
			Helper.SetupFilterStripsFromQueryString(filterBizO, queryString);
			AssertEquals("Text filter active", expectedActive, filter.IsActive);
			AssertEquals("Text filter property set", expectedString, filter.Property);
			filter.IsActive = false;
			filter.Property = ZString.Empty;
		}

		void AssertTextAndNkFilter(FilterStripBusinessObjectForTest filterBizO, ModuleTextAndNkFilter filter, ZString queryString, bool expectedActive, ZString expectedText, ZString expectedNk)
		{
			Helper.SetupFilterStripsFromQueryString(filterBizO, queryString);
			AssertEquals("Text+Nk filter active", expectedActive, filter.IsActive);
			AssertEquals("Text+Nk filter text property set", expectedText, filter.Property);
			AssertEquals("Text+Nk filter text property set", expectedNk, filter.NkProperty);
			filter.IsActive = false;
			filter.Property = ZString.Empty;
			filter.NkProperty = ZString.Empty;
		}

		void AssertTextRangeFilter(FilterStripBusinessObjectForTest filterBizO, ModuleTextRangeFilter filter, ZString queryString, bool expectedActive, ZString expectedText1, ZString expectedText2)
		{
			Helper.SetupFilterStripsFromQueryString(filterBizO, queryString);
			AssertEquals("Text Range filter active", expectedActive, filter.IsActive);
			AssertEquals("Text Range filter text property set", expectedText1, filter.Property1);
			AssertEquals("Text Range filter text property set", expectedText2, filter.Property2);
			filter.IsActive = false;
			filter.Property1 = ZString.Empty;
			filter.Property2 = ZString.Empty;
		}

		#endregion

		#region Implementation

		FilterStripURLParameterHelper Helper;
		FilterStripBusinessObjectForTest FilterStrip;
		OrgHeader testOrg;
		OrgHeader testOrg2;
		protected override void SetUp()
		{
			base.SetUp();
			testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			Helper = new FilterStripURLParameterHelper();
			FilterStrip = new FilterStripBusinessObjectForTest();
		}

		public class FilterStripBusinessObjectForTest : FilterStripBusinessObject
		{
			public static class Schema
			{
				public static ZString TestAuditFilterName = "TestAuditFilter";
				public static ZString TestDateFilterName = "TestDateFilter";
				public static ZString TestGuidFilterName = "TestGuidFilter";
				public static ZString TestGuidsFilterName = "TestGuidsFilter";
				public static ZString TestLocationFilterName = "TestLocationFilter";
				public static ZString TestNkFilterName = "TestNkFilter";
				public static ZString TestNumberFilterName = "TestNumberFilter";
				public static ZString TestNumberRangeFilterName = "TestNumberRangeFilter";
				public static ZString TestSingleDateFilterName = "TestSingleDateFilter";
				public static ZString TestTextAndNkFilterName = "TestTextAndNkFilter";
				public static ZString TestTextFilterName = "TestTextFilter";
				public static ZString TestTextRangeFilterName = "TestTextRangeFilter";
				public static ZString UnpublishedFilterName = "UnpublishedFilter";
			}

			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				ModuleFilterCollection result = new ModuleFilterCollection();

				result.AddDateFilter(Schema.TestDateFilterName, TestQuery);
				result.AddGuidFilter(Schema.TestGuidFilterName, DummyModuleIDs.Dummy, TestQuery, OrgList);
				result.AddCustomFilter(new ModuleGuidsFilter(Schema.TestGuidsFilterName, DummyModuleIDs.Dummy, TestQuery, OrgList, OrgList));
				result.AddLocationFilter(Schema.TestLocationFilterName, TestQuery, LocationList, LocationList);
				result.AddNkFilter(Schema.TestNkFilterName, TestQueryNk, DummyModuleIDs.Dummy, TestList);
				result.AddNumberFilter(Schema.TestNumberFilterName, TestQuery);
				result.AddNumberRangeFilter(Schema.TestNumberRangeFilterName, TestQuery);
				result.AddSingleDateFilter(Schema.TestSingleDateFilterName, TestQuery);
				result.AddTextAndNkFilter(Schema.TestTextAndNkFilterName, TestQuery, DummyModuleIDs.Dummy, TestList);
				result.AddTextFilter(Schema.TestTextFilterName, TestQuery);
				result.AddTextRangeFilter(Schema.TestTextRangeFilterName, TestQuery);

				ModuleTextFilter unpublishedFilter = result.AddTextFilter(Schema.UnpublishedFilterName, TestQuery);
				unpublishedFilter.IsPublishedOnWeb = false;

				return result;
			}

			ZQuery TestQuery(SQLComparisonOperator comparisonOperator, ZString value)
			{
				return new ZQuery();
			}
			ZQuery TestQuery(SQLComparisonOperator comparisonOperator, ZString value1, ZString value2)
			{
				return new ZQuery();
			}

			ZQuery TestQuery(ZGuid value)
			{
				return new ZQuery();
			}
			ZQuery TestQuery(ZGuid value1, ZGuid value2)
			{
				return new ZQuery();
			}

			ZQuery TestQueryNk(ZString value)
			{
				return new ZQuery();
			}
			ZQuery TestQuery(ZDateTime value)
			{
				return new ZQuery();
			}
			ZQuery TestQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
			{
				return new ZQuery();
			}
			ZQuery TestQuery(ZString value1, ZString value2)
			{
				return new ZQuery();
			}
			ZQuery TestQuery(INumericZType value1, INumericZType value2)
			{
				return new ZQuery();
			}

			IBusinessObjectCollection TestList
			{
				get { return new DummyBusinessObjectCollection(Factory); }
			}
			LocationCollection LocationList
			{
				get { return new LocationCollection(Factory); }
			}
			OrgHeaderCollection OrgList
			{
				get { return new OrgHeaderCollection(Factory); }
			}
		}

		#endregion
	}
}
