using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	[TestedType(typeof(ReportStatisticsFilterBusinessObject))]
	sealed class ReportStatisticsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestNextScheduleDateFilterHasConvertFromLocalToUTC()
		{
			var filterStrip = new ReportStatisticsFilterBusinessObject();
			var filter = filterStrip.ModuleFilters["Start Date"] as ModuleDateFilter;

			AssertNotNull("Start Date Filter should exist.", filter);
			AssertEquals("filter.ConvertFromLocalToUTC", true, filter.ConvertFromLocalToUTC);

			filter = filterStrip.ModuleFilters["End Date"] as ModuleDateFilter;

			AssertNotNull("End Date Filter should exist.", filter);
			AssertEquals("filter.ConvertFromLocalToUTC", true, filter.ConvertFromLocalToUTC);
		}

		public void TestQueueStartDate()
		{
			var filterStrip = new ReportStatisticsFilterBusinessObject();
			var filter = filterStrip.ModuleFilters["Queue Start Date"] as ModuleDateFilter;

			AssertEquals("Description of filter is correct", "Queue Start Date", filter.MultilingualDescription);
			AssertEquals("Filter queries the correct column", StmReportRunSchema.RRI_StartTimeInQueueUtc, filter.FilterColumn);
		}

		public void TestDuration()
		{
			Db.Connection.ExecuteNonQuery("delete from dbo.StmReportRun");

			var stmReportRun1 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun1.RRI_StartTimeUtc = new ZDateTime(2000, 1, 1, 12, 4, 4);
			stmReportRun1.RRI_EndTimeUtc = new ZDateTime(2000, 1, 1, 12, 4, 5);
			var stmReportRun2 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun2.RRI_StartTimeUtc = new ZDateTime(2000, 1, 1, 12, 4, 4);
			stmReportRun2.RRI_EndTimeUtc = new ZDateTime(2000, 1, 1, 12, 4, 14);
			var stmReportRun3 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun3.RRI_StartTimeUtc = new ZDateTime(2000, 1, 1, 12, 4, 4);
			stmReportRun3.RRI_EndTimeUtc = ZDateTime.Empty;
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)GetNewFilterStripBusinessObject().ModuleFilters["Duration (Seconds)"];
			AssertEquals(3, Factory.Load<StmReportRun>(filter.Query).Length);
			filter.Property1 = 2;
			filter.Property2 = filter.MaxValue;
			AssertEquals(1, Factory.Load<StmReportRun>(filter.Query).Length);
			filter.Property1 = 1;
			filter.Property2 = filter.MaxValue;
			AssertEquals(2, Factory.Load<StmReportRun>(filter.Query).Length);
			filter.Property1 = 1;
			filter.Property2 = 10;
			AssertEquals(2, Factory.Load<StmReportRun>(filter.Query).Length);
			filter.Property1 = 1;
			filter.Property2 = 9;
			AssertEquals(1, Factory.Load<StmReportRun>(filter.Query).Length);
			filter.Property1 = 2;
			filter.Property2 = 9;
			AssertEquals(0, Factory.Load<StmReportRun>(filter.Query).Length);
		}

		public void TestGetPrintUserQuery()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "Test1";
			staff1.GS_Code = "111";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "Test2";
			staff2.GS_Code = "222";

			var stmReportRun1 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun1.RRI_GS_NKPrintUser = "111";
			var stmReportRun2 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun2.RRI_GS_NKPrintUser = "111";
			var stmReportRun3 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun3.RRI_GS_NKPrintUser = "222";
			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)GetNewFilterStripBusinessObject().ModuleFilters["Print User"];

			filter.Property = staff1.PK;

			ZQuery query = filter.Query;
			AssertEquals("RRI_GS_NKPrintUser = '111'", query.LiteralTextADO);

			StmReportRun[] statistics = Factory.Load<StmReportRun>(query);
			AssertEquals(2, statistics.Length);
			AssertCollectionContains(stmReportRun1, statistics);
			AssertCollectionContains(stmReportRun2, statistics);

			filter.Property = staff2.PK;

			query = filter.Query;
			AssertEquals("RRI_GS_NKPrintUser = '222'", query.LiteralTextADO);

			statistics = Factory.Load<StmReportRun>(query);
			AssertEquals(1, statistics.Length);
			AssertCollectionContains(stmReportRun3, statistics);
		}

		public void TestReportSourceQuery()
		{
			var stmReportRun1 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun1.RRI_IsSystemDefined = true;
			var stmReportRun2 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun2.RRI_IsSystemDefined = true;
			var stmReportRun3 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun3.RRI_IsSystemDefined = false;
			Factory.Save();

			var filter = (ModuleTextFilter)GetNewFilterStripBusinessObject().ModuleFilters["Is System Defined"];
			AssertNotNull(filter);

			filter.Property = "System";

			var query = filter.Query;
			AssertEquals("RRI_IsSystemDefined = 1", query.LiteralTextADO);

			var statistics = Factory.Load<StmReportRun>(query);
			AssertEquals(2, statistics.Length);
			AssertCollectionContains(stmReportRun1, statistics);
			AssertCollectionContains(stmReportRun2, statistics);

			filter.Property = "Customized";

			query = filter.Query;
			AssertEquals("RRI_IsSystemDefined = 0", query.LiteralTextADO);

			statistics = Factory.Load<StmReportRun>(query);
			AssertEquals(1, statistics.Length);
			AssertCollectionContains(stmReportRun3, statistics);
		}

		public void TestIsSystemDefinedForIndexSearch()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.IndexSearchFields = GetSearchFieldCollection();
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				var filter = (IndexSearchModuleTextFilter)filterStrip.ModuleFilters["Is System Defined"];
				AssertNotNull(filter);

				AssertEquals("All", filter.DefaultProperty);

				filter.Property = "System";
				AssertEquals("((ISSYSTEMDEFINED eq true) or (ISSYSTEMDEFINED eq null))", filter.GetGlowIndexQuery().ToUrlComponent());

				filter.Property = "Customized";
				AssertEquals("(ISSYSTEMDEFINED eq false)", filter.GetGlowIndexQuery().ToUrlComponent());

				filter.Property = "All";
				AssertEquals("", filter.GetGlowIndexQuery().ToUrlComponent());
			}

			SearchFieldCollection GetSearchFieldCollection()
			{
				var field = SearchField.Create("ISSYSTEMDEFINED", "Is System Defined");
				var ret = new SearchFieldCollection(null, [field]);
				return ret;
			}
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ReportStatisticsFilterBusinessObject();
		}

		#endregion
	}
}
