using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	[TestedType(typeof(ReportManagementFilterBusinessObject))]
	sealed class ReportManagementFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestPrintUserQuery()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "Test1";
			staff1.GS_Code = "111";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "Test2";
			staff2.GS_Code = "222";

			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask1.UserFK = staff1.PK;
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.UserFK = staff1.PK;
			var scheduleTask3 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask3.UserFK = staff2.PK;
			Factory.Save();

			var filter = (ModuleGuidFilter)GetNewFilterStripBusinessObject().ModuleFilters["Print User"];
			filter.Property = staff1.PK;

			var query = filter.Query;
			AssertEquals("S5_GS_NKPrintUser = '111'", query.LiteralTextADO);

			ReportScheduleTask[] tasks = Factory.Load<ReportScheduleTask>(query);
			AssertEquals(2, tasks.Length);
			AssertCollectionContains(scheduleTask1, tasks);
			AssertCollectionContains(scheduleTask2, tasks);

			filter.Property = staff2.PK;

			query = filter.Query;
			AssertEquals("S5_GS_NKPrintUser = '222'", query.LiteralTextADO);

			tasks = Factory.Load<ReportScheduleTask>(query);
			AssertEquals(1, tasks.Length);
			AssertCollectionContains(scheduleTask3, tasks);
		}

		public void TestRunningServerQuery()
		{
			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			var scheduleTask3 = Factory.NewWithValidTestData<ReportScheduleTask>();

			var stmReportRun1 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun1.RRI_Status = Constants.StmReportRunState.Running;
			stmReportRun1.RRI_RunningServer = "RunningServer1";
			stmReportRun1.RRI_S5_Schedule = scheduleTask1.PK;

			var stmReportRun2 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun2.RRI_Status = Constants.StmReportRunState.Running;
			stmReportRun2.RRI_RunningServer = "RunningServer2";
			stmReportRun2.RRI_S5_Schedule = scheduleTask2.PK;

			var stmReportRun3 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun3.RRI_Status = Constants.StmReportRunState.Finished;
			stmReportRun3.RRI_RunningServer = "RunningServer3";
			stmReportRun3.RRI_S5_Schedule = scheduleTask3.PK;
			Factory.Save();

			var filter = (ModuleTextFilter)GetNewFilterStripBusinessObject().ModuleFilters["Running Server"];
			filter.Property = "RunningServer1";
			var query = filter.Query;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertEquals("S5_PK IN (SELECT RRI_S5_Schedule FROM dbo.StmReportRun WHERE RRI_S5_Schedule IS NOT NULL AND RRI_Status = 'RUN' and RRI_RunningServer like 'RunningServer1%')", query.LiteralTextADO);
			}

			var tasks = Factory.Load<ReportScheduleTask>(query);
			AssertEquals(1, tasks.Length);
			AssertEquals(scheduleTask1.PK, tasks[0].PK);

			filter.Property = "RunningServer2";
			tasks = Factory.Load<ReportScheduleTask>(filter.Query);
			AssertEquals(1, tasks.Length);
			AssertEquals(scheduleTask2.PK, tasks[0].PK);

			filter.Property = "RunningServer3";
			tasks = Factory.Load<ReportScheduleTask>(filter.Query);
			AssertEquals(0, tasks.Length);
		}

		public void TestLastProcessedTimeQuery()
		{
			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();

			var stmReportRun1 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun1.RRI_Status = Constants.StmReportRunState.Running;
			stmReportRun1.RRI_StartTimeUtc = new ZDateTime(2018, 1, 1, 12, 0, 0);
			stmReportRun1.RRI_S5_Schedule = scheduleTask1.PK;

			var stmReportRun2 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun2.RRI_Status = Constants.StmReportRunState.Finished;
			stmReportRun2.RRI_S5_Schedule = scheduleTask2.PK;
			stmReportRun2.RRI_StartTimeUtc = new ZDateTime(2018, 1, 1, 12, 0, 0);
			stmReportRun2.RRI_EndTimeUtc = new ZDateTime(2018, 1, 1, 12, 0, 10);

			Factory.Save();

			var filter = (ModuleNumberRangeFilter)GetNewFilterStripBusinessObject().ModuleFilters["Last Processed Time (Seconds)"];
			filter.Property1 = 0;
			filter.Property2 = 9;
			var query = filter.Query;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertEquals("S5_PK IN (SELECT RRI_S5_Schedule FROM dbo.StmReportRun WHERE RRI_S5_Schedule IS NOT NULL AND RRI_Status = 'FIN' and (DATEDIFF(second, RRI_StartTimeUtc, RRI_EndTimeUtc) >= 0) and (DATEDIFF(second, RRI_StartTimeUtc, RRI_EndTimeUtc) <= 9))", query.LiteralTextADO);
			}
			var tasks = Factory.Load<ReportScheduleTask>(query);
			AssertEquals(0, tasks.Length);

			filter.Property2 = 10;
			tasks = Factory.Load<ReportScheduleTask>(filter.Query);
			AssertEquals(1, tasks.Length);
			AssertEquals(scheduleTask2.PK, tasks[0].PK);
		}

		public void TestTimeBeingProcessedQuery()
		{
			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();

			var stmReportRun1 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun1.RRI_Status = Constants.StmReportRunState.Running;
			stmReportRun1.RRI_StartTimeUtc = DateTime.UtcNow;
			stmReportRun1.RRI_S5_Schedule = scheduleTask1.PK;

			var stmReportRun2 = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun2.RRI_Status = Constants.StmReportRunState.Finished;
			stmReportRun2.RRI_S5_Schedule = scheduleTask2.PK;
			stmReportRun2.RRI_StartTimeUtc = new ZDateTime(2018, 1, 1, 12, 0, 0);
			stmReportRun2.RRI_EndTimeUtc = new ZDateTime(2018, 1, 1, 12, 0, 8);

			Factory.Save();

			var filter = (ModuleNumberRangeFilter)GetNewFilterStripBusinessObject().ModuleFilters["Time Being Processed (Seconds)"];
			filter.Property1 = 0;
			filter.Property2 = filter.MaxValue;
			var query = filter.Query;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertEquals("S5_PK IN (SELECT RRI_S5_Schedule FROM dbo.StmReportRun WHERE RRI_S5_Schedule IS NOT NULL AND RRI_Status = 'RUN' and (DATEDIFF(second, RRI_StartTimeUtc, GetUTCDate()) >= 0) and (DATEDIFF(second, RRI_StartTimeUtc, GetUTCDate()) <= 99999999999999.99))", query.LiteralTextADO);
			}
			var tasks = Factory.Load<ReportScheduleTask>(query);
			AssertNotNull(tasks.FirstOrDefault(x => x.PK == scheduleTask1.PK));
		}

		[TestDate(2018, 3, 19)]
		public void TestAlwaysAppliedAndHiddenQuery()
		{
			var filter = (ModuleTextFilter)GetNewFilterStripBusinessObject().ModuleFilters["Schedule Report"];
			var query = filter.Query;
			var originalTaskCount = Factory.Load<ReportScheduleTask>(query).Length;

			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask1.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2018, 3, 10);

			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2018, 3, 20);

			Factory.Save();

			var tasks = Factory.Load<ReportScheduleTask>(query);
			AssertEquals(originalTaskCount + 1, tasks.Length);
			AssertNull(tasks.FirstOrDefault(x => x.PK == scheduleTask2.PK));
			AssertNotNull(tasks.FirstOrDefault(x => x.PK == scheduleTask1.PK));
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ReportManagementFilterBusinessObject();
		}

		#endregion

		public void TestIndexSearchFiltersOfStmScheduleTask()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ReportManagement))
			using (var mocker = new GlowIndexQueryEngineMock(
				mock =>
				{
					_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection());
					_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IStmScheduleTask" });
				}))
			{
				var reportNameFilter = (IndexSearchModuleGuidFilter)module.FilterBusinessObject["ReportNameByParentID"];
				AssertNotNull(reportNameFilter);
				AssertEquals(FilterCategories.Other, reportNameFilter.Category);
				AssertEquals("Report Name", reportNameFilter.MultilingualDescription.ToString());

				var nextScheduledPrintRunTimeUtcFilter = (IndexSearchModuleDateFilter)module.FilterBusinessObject["NextScheduledPrintRunTimeUtc"];
				AssertNotNull(nextScheduledPrintRunTimeUtcFilter);
				AssertEquals(FilterCategories.Dates, nextScheduledPrintRunTimeUtcFilter.Category);
				AssertEquals(FilterVisibility.AlwaysAppliedAndHidden, nextScheduledPrintRunTimeUtcFilter.Visibility);
				AssertEquals(ModuleDateFilter.Past, nextScheduledPrintRunTimeUtcFilter.PropertySearch);
				AssertEquals("Next Scheduled Print Run Time UTC", nextScheduledPrintRunTimeUtcFilter.MultilingualDescription.ToString());

				AssertNull(module.FilterBusinessObject["StartDate"]);
				AssertNull(module.FilterBusinessObject["EndDate"]);
				AssertNull(module.FilterBusinessObject["DeliveryAddress"]);
				AssertNull(module.FilterBusinessObject["AddressOverride"]);
				AssertNull(module.FilterBusinessObject["EmailBCC"]);
				AssertNull(module.FilterBusinessObject["EmailCC"]);
				AssertNull(module.FilterBusinessObject["BranchPK"]);
			}
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var field1 = SearchField.Create("ReportNameByParentID", "Report Name");
			var field2 = SearchField.Create("NextScheduledPrintRunTimeUtc", "Next Scheduled Print Run Time UTC");
			var field3 = SearchField.Create("StartDate", "Start Date");
			var field4 = SearchField.Create("EndDate", "End Date");
			var field5 = SearchField.Create("DeliveryAddress", "Delivery Address");
			var field6 = SearchField.Create("AddressOverride", "Address Override");
			var field7 = SearchField.Create("EmailBCC", "Email BCC");
			var field8 = SearchField.Create("EmailCC", "Email CC");
			var field9 = SearchField.Create("BranchPK", "Branch");
			var ret = new SearchFieldCollection("IStmScheduleTask", [field1, field2, field3, field4, field5, field6, field7, field8, field9]);
			return ret;
		}
	}
}
