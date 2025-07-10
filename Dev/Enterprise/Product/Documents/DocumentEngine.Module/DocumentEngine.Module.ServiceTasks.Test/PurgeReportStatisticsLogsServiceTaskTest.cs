using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DocumentEngine.Scheduler.Module.ServiceTasks.Testing
{
	[TestedType(typeof(PurgeReportStatisticsLogsServiceTask))]
	sealed class PurgeReportStatisticsLogsServiceTaskTest : ServiceTaskTestCase<PurgeReportStatisticsLogsServiceTask>
	{
		public void TestCanRunInAnyBranch()
		{
			SetUpData();
			var task = new PurgeReportStatisticsLogsServiceTask();
			InitialiseTaskSchedule(task);
			ErrorReporter.Clear();
			RunTaskSchedule(task);
			AssertEquals("No Errors", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestIsMandatoryAndActiveByDefault()
		{
			var attributes = typeof(PurgeReportStatisticsLogsServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>();
			var attribute = attributes.Single(a => a.TypeName == typeof(PurgeReportStatisticsLogsServiceTask).FullName);
			var testTask = new PurgeReportStatisticsLogsServiceTask();
			InitialiseTaskSchedule(testTask, out var taskSchedule);

			AssertEquals("IsMandatory", true, attribute.IsMandatory);
			AssertEquals("IsActive", true, ((StmServiceTask)taskSchedule).SST_Active);
		}

		public void TestRunTask()
		{
			SetUpData();
			AssertEquals(5, new BusinessObjectFactory().Load<StmReportRun>(new ZQuery()).Length);
			var task = new PurgeReportStatisticsLogsServiceTask();
			InitialiseTaskSchedule(task);

			DataRegistry.Instance.PurgeReportStatisticsLogs = 7;
			RunTaskSchedule(task);
			AssertEquals(5, new BusinessObjectFactory().Load<StmReportRun>(new ZQuery()).Length);

			DataRegistry.Instance.PurgeReportStatisticsLogs = 5;
			RunTaskSchedule(task);
			AssertEquals(5, new BusinessObjectFactory().Load<StmReportRun>(new ZQuery()).Length);

			DataRegistry.Instance.PurgeReportStatisticsLogs = 4;
			RunTaskSchedule(task);
			AssertEquals(4, new BusinessObjectFactory().Load<StmReportRun>(new ZQuery()).Length);

			DataRegistry.Instance.PurgeReportStatisticsLogs = 3;
			RunTaskSchedule(task);
			AssertEquals(3, new BusinessObjectFactory().Load<StmReportRun>(new ZQuery()).Length);

			DataRegistry.Instance.PurgeReportStatisticsLogs = 2;
			RunTaskSchedule(task);
			AssertEquals(2, new BusinessObjectFactory().Load<StmReportRun>(new ZQuery()).Length);

			DataRegistry.Instance.PurgeReportStatisticsLogs = 1;
			RunTaskSchedule(task);
			AssertEquals("rd1", new BusinessObjectFactory().Load<StmReportRun>(new ZQuery())[0].RRI_ReportDescription);

			DataRegistry.Instance.PurgeReportStatisticsLogs = 0;
			RunTaskSchedule(task);
			AssertEquals(0, new BusinessObjectFactory().Load<StmReportRun>(new ZQuery()).Length);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		void SetUpData()
		{
			Db.Connection.ExecuteNonQuery("delete from dbo.StmReportRun");
			var srr1 = Factory.NewWithValidTestData<StmReportRun>();
			srr1.RRI_StartTimeUtc = ZDateTime.UtcNow.AddHours(-24 * 0 - 12);
			srr1.RRI_EndTimeUtc = srr1.RRI_StartTimeUtc.AddMilliseconds(500);
			srr1.RRI_ReportDescription = "rd1";
			srr1.RRI_ReportName = "rn1";
			srr1.RRI_RunningServer = "rs1";
			srr1.RRI_GS_NKPrintUser = "pu1";
			var srr2 = Factory.NewWithValidTestData<StmReportRun>();
			srr2.RRI_StartTimeUtc = ZDateTime.UtcNow.AddHours(-24 * 1 - 12);
			srr2.RRI_EndTimeUtc = srr2.RRI_StartTimeUtc.AddMilliseconds(1500);
			srr2.RRI_ReportDescription = "rd2";
			srr2.RRI_ReportName = "rn2";
			srr2.RRI_RunningServer = "rs2";
			srr2.RRI_GS_NKPrintUser = "pu2";
			var srr3 = Factory.NewWithValidTestData<StmReportRun>();
			srr3.RRI_StartTimeUtc = ZDateTime.UtcNow.AddHours(-24 * 2 - 12);
			srr3.RRI_EndTimeUtc = srr3.RRI_StartTimeUtc.AddMilliseconds(2500);
			srr3.RRI_ReportDescription = "rd3";
			srr3.RRI_ReportName = "rn3";
			srr3.RRI_RunningServer = "rs3";
			srr3.RRI_GS_NKPrintUser = "pu3";
			var srr4 = Factory.NewWithValidTestData<StmReportRun>();
			srr4.RRI_StartTimeUtc = ZDateTime.UtcNow.AddHours(-24 * 3 - 12);
			srr4.RRI_EndTimeUtc = srr4.RRI_StartTimeUtc.AddMilliseconds(3500);
			srr4.RRI_ReportDescription = "rd4";
			srr4.RRI_ReportName = "rn4";
			srr4.RRI_RunningServer = "rs4";
			srr4.RRI_GS_NKPrintUser = "pu4";
			var srr5 = Factory.NewWithValidTestData<StmReportRun>();
			srr5.RRI_StartTimeUtc = ZDateTime.UtcNow.AddHours(-24 * 4 - 12);
			srr5.RRI_EndTimeUtc = ZDateTime.MinSmallDateTimeValue;
			srr5.RRI_ReportDescription = "";
			srr5.RRI_ReportName = "";
			srr5.RRI_RunningServer = "";
			srr5.RRI_GS_NKPrintUser = "";
			Factory.Save();
		}
	}
}
