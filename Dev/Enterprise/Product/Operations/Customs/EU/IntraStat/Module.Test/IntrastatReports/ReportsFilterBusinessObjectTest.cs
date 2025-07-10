using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.EU.Intrastat.Business.Testing;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Module.Testing
{
	[TestedType(typeof(ReportsFilterBusinessObject))]
	sealed class ReportsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() =>
			new ReportsFilterBusinessObject();

		public void TestGroupNumberFilter()
		{
			var filter = (ModuleTextFilter)filterBizObj[ReportsFilterBusinessObject.Schema.GroupNumber];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filter.Property = "";

			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, true),
				(report3, true),
				(report4, true)
			);

			filter.Property = "ST";
			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, false),
				(report3, false),
				(report4, false)
			);

			filter.Property = "RP";
			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, true),
				(report3, true),
				(report4, true)
			);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "P000";
			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, true),
				(report3, true),
				(report4, false)
			);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "P0001";
			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, false),
				(report3, false),
				(report4, false)
			);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "0001";
			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, false),
				(report3, false),
				(report4, false)
			);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "RP0001";
			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, false),
				(report3, false),
				(report4, false)
			);

			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);
		}

		public void TestReporterFilter()
		{
			var filter = (ModuleGuidFilter)filterBizObj[ReportsFilterBusinessObject.Schema.Reporter];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;

			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, true),
				(report3, true),
				(report4, true)
			);

			filter.Property = org1.PK;
			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, true),
				(report3, false),
				(report4, false)
			);

			filter.Property = org2.PK;
			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, false),
				(report3, true),
				(report4, false)
			);

			filter.Property = org3.PK;
			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, false),
				(report3, false),
				(report4, true)
			);

			filter.Property = org4.PK;
			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, false),
				(report3, false),
				(report4, false)
			);

			AssertEquals(FilterCategories.Organisations, filter.Category);
		}

		public void TestTraderFilter()
		{
			var filter = (ModuleGuidFilter)filterBizObj[ReportsFilterBusinessObject.Schema.Trader];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;

			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, true),
				(report3, true),
				(report4, true)
			);

			filter.Property = org1.PK;
			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, false),
				(report3, false),
				(report4, false)
			);

			filter.Property = org2.PK;

			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, false),
				(report3, false),
				(report4, false)
			);

			filter.Property = org3.PK;

			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, true),
				(report3, false),
				(report4, false)
			);

			filter.Property = org4.PK;

			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, false),
				(report3, true),
				(report4, true)
			);

			AssertEquals(FilterCategories.Organisations, filter.Category);
		}

		public void TestPeriodFilter()
		{
			var filter = (PeriodFilter)filterBizObj[ReportsFilterBusinessObject.Schema.Period];

			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;

			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, false),
				(report3, false),
				(report4, false)
			);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;

			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, true),
				(report3, true),
				(report4, true)
			);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.PeriodYear = ZInt.Parse("2023");
			filter.PeriodMonth = ZInt.Parse("01");

			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, false),
				(report3, false),
				(report4, false)
			);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, true),
				(report3, true),
				(report4, true)
			);

			AssertEquals(FilterCategories.Dates, filter.Category);
		}

		public void TestFlowFilter()
		{
			var filter = (ModuleTextFilter)filterBizObj[ReportsFilterBusinessObject.Schema.Flow];
			filter.IsActive = true;
			filter.Property = "";

			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, true),
				(report3, true),
				(report4, true)
			);

			filter.Property = ReportFlowCodeDescriptionPairList.Codes.Export;

			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, false),
				(report3, true),
				(report4, false)
			);

			filter.Property = ReportFlowCodeDescriptionPairList.Codes.Import;

			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, true),
				(report3, false),
				(report4, true)
			);

			AssertEquals(FilterCategories.Locations, filter.Category);
		}

		public void TestStatusFilter()
		{
			var filter = (ModuleTextFilter)filterBizObj[ReportsFilterBusinessObject.Schema.Status];
			filter.IsActive = true;
			filter.Property = "";

			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, true),
				(report3, true),
				(report4, true)
			);

			filter.Property = ReportStatusCodeDescriptionPairList.Codes.Reported;

			AssertQueryResults(
				filter.Query,
				(report1, true),
				(report2, true),
				(report3, false),
				(report4, false)
			);

			filter.Property = ReportStatusCodeDescriptionPairList.Codes.NotReported;

			AssertQueryResults(
				filter.Query,
				(report1, false),
				(report2, false),
				(report3, true),
				(report4, true)
			);

			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
		}

		public void TestInitialAuditFilters()
		{
			filterBizObj.QueryObjectType = typeof(CusIntrastatGroup);
			var createdTimeFilter = (ModuleDateFilter)filterBizObj[FilterDescriptions.CreatedTime];

			AssertEquals(FilterVisibility.AlwaysVisible, createdTimeFilter.Visibility);
			AssertEquals(expected: true, createdTimeFilter.Visible);
			AssertEquals(ModuleDateFilter.DateRangeSearchTexts.Last3Mths, createdTimeFilter.PropertySearch);
		}

		public void TestMultilingualDescriptions()
		{
			var groupNumberFilter = (ModuleTextFilter)filterBizObj[ReportsFilterBusinessObject.Schema.GroupNumber];
			AssertEquals(ReportsFilterBusinessObject.Schema.GroupNumber, groupNumberFilter.MultilingualDescription);

			var flowFilter = (ModuleTextFilter)filterBizObj[ReportsFilterBusinessObject.Schema.Flow];
			AssertEquals(ReportsFilterBusinessObject.Schema.Flow, flowFilter.MultilingualDescription);

			var statusFilter = (ModuleTextFilter)filterBizObj[ReportsFilterBusinessObject.Schema.Status];
			AssertEquals(ReportsFilterBusinessObject.Schema.Status, statusFilter.MultilingualDescription);

			var reporterFilter = (ModuleGuidFilter)filterBizObj[ReportsFilterBusinessObject.Schema.Reporter];
			AssertEquals(ReportsFilterBusinessObject.Schema.Reporter, reporterFilter.MultilingualDescription);

			var traderFilter = (ModuleGuidFilter)filterBizObj[ReportsFilterBusinessObject.Schema.Trader];
			AssertEquals(ReportsFilterBusinessObject.Schema.Trader, traderFilter.MultilingualDescription);

			var periodFilter = (PeriodFilter)filterBizObj[ReportsFilterBusinessObject.Schema.Period];
			AssertEquals(ReportsFilterBusinessObject.Schema.Period, periodFilter.MultilingualDescription);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = IntrastatTestDataHelper.New(Factory);
			filterBizObj = (ReportsFilterBusinessObject)GetNewFilterStripBusinessObject();
			org1 = helper.GetOrCreateOrgHeader("OR1");
			org2 = helper.GetOrCreateOrgHeader("OR2");
			org3 = helper.GetOrCreateOrgHeader("OR3");
			org4 = helper.GetOrCreateOrgHeader("OR4");
			report1 = CreateBizObj("RP0001", org1, org2, "2023-01", ReportFlowCodeDescriptionPairList.Codes.Export, ReportStatusCodeDescriptionPairList.Codes.Reported);
			report2 = CreateBizObj("RP0002", org1, org3, "2023-02", ReportFlowCodeDescriptionPairList.Codes.Import, ReportStatusCodeDescriptionPairList.Codes.Reported);
			report3 = CreateBizObj("RP0003", org2, org4, "2023-03", ReportFlowCodeDescriptionPairList.Codes.Export, ReportStatusCodeDescriptionPairList.Codes.NotReported);
			report4 = CreateBizObj("RP0014", org3, org4, "2023-04", ReportFlowCodeDescriptionPairList.Codes.Import, ReportStatusCodeDescriptionPairList.Codes.NotReported);
			Factory.Save();
		}

		OrgHeader org1;
		OrgHeader org2;
		OrgHeader org3;
		OrgHeader org4;

		CusIntrastatGroup report1;
		CusIntrastatGroup report2;
		CusIntrastatGroup report3;
		CusIntrastatGroup report4;
		ReportsFilterBusinessObject filterBizObj;
		IntrastatTestDataHelper helper;

		CusIntrastatGroup CreateBizObj(string groupNumber, OrgHeader reporter, OrgHeader trader, string period, string flow, string status)
		{
			var result = Factory.New<CusIntrastatGroup>();
			result.CIG_GroupNumber = groupNumber;
			result.CIG_OH_Reporter = reporter.PK;
			result.CIG_Period = period;
			result.CIG_Flow = flow;
			result.CIG_Status = status;
			result.CIG_GC_Company = GlbCompany.CurrentCompany.PK;
			_ = helper.NewCusIntrastatMergedLineWithValidData(result, trader);
			return result;
		}
	}
}
