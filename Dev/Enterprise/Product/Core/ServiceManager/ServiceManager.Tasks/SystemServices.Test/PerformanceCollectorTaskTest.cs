using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.SystemServices.Testing
{
	[TestedType(typeof(PerformanceCollectorTask))]
	sealed class PerformanceCollectorTaskTest : ServiceTaskTestCase<PerformanceCollectorTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}

		public void TestInit()
		{
			PerformanceCollectorTask task = new PerformanceCollectorTask();

			InitialiseTaskSchedule(task, out StmServiceTask taskSchedule);

			AssertEquals("ScheduleTask - IsActive", ZBool.False, taskSchedule.SST_Active);
			Assert("ScheduleTask - TaskPeriod", taskSchedule.Recurrence.HoursRange);
			AssertEquals("ScheduleTask - TaskPeriodCount", 1, taskSchedule.Recurrence.Period);
			AssertEquals("ScheduleTask - WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
		}

		public void TestCollectStatistics()
		{
			Db.Connection.ExecuteNonQuery("delete dbo.StmJobQueue");

			PerformanceCollectorTask task = new PerformanceCollectorTask();
			TestServiceLogger log = InitialiseAndRunTaskSchedule(task);
			AssertEquals(3, log.Count);
			AssertMultilineASCIIEquals("Should be no queue initially",
				@"Information|LWK Statistics:
Current backlog: 0
Added to the queue in last period: 0
Last Period (min.): 60
Oldest unprocessed job: N/A", log[0]);

			InsertQueue("WTE", true, true);
			InsertQueue("WTE", true, false);
			ZDateTime oldest = InsertQueue("WTE", false, true);
			InsertQueue("WTE", false, false);
			InsertQueue("ABC", true, true);
			InsertQueue("ABC", true, false);
			InsertQueue("ABC", false, true);
			InsertQueue("ABC", false, false);

			using (var command = TestConnection.Command("delete from dbo.StmALogQueue;delete from dbo.StmALogQueueWTE"))
			{
				command.ExecuteNonQuery();
			}

			AddLogQueue("ABC");
			AddWTELogQueue();

			task = new PerformanceCollectorTask();
			log = InitialiseAndRunTaskSchedule(task);
			AssertEquals(3, log.Count);
			AssertMultilineASCIIEquals("Backlog: 2 QUE (one outdated), Added in this period: 1 QUE, 1 PRS",
				@"Information|LWK Statistics:
Current backlog: 2
Added to the queue in last period: 2
Last Period (min.): 60
Oldest unprocessed job: " + oldest.ToString("G"), log[0]);

			AssertEquals("Information|Logs not yet converted from dbo.StmALogQueue to StmJobQueue : 1", log[1]);
			AssertEquals("Information|Logs not yet converted from dbo.StmALogQueueWTE to StmJobQueue : 1", log[2]);

			InsertQueue("WTE", true, true);
			InsertQueue("WTE", true, false);

			task = new PerformanceCollectorTask();
			log = InitialiseAndRunTaskSchedule(task);
			AssertEquals(3, log.Count);
			AssertMultilineASCIIEquals("Backlog: 2 from prev cycle, 1 new QUE, Added in this period: 2 from prev cycle, 2 new",
				@"Information|LWK Statistics:
Current backlog: 3
Added to the queue in last period: 4
Last Period (min.): 60
Oldest unprocessed job: " + oldest.ToString("G"), log[0]);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		const string InsertQueueSql = "insert into dbo.StmJobQueue (SJ_PK, SJ_SE_NKEvent, SJ_PostedTimeUtc, SJ_Status, SJ_ALogReference, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentID, SJ_ParentTableCode) values (newid(), '{0}', @date, '{1}', newid(), getdate(), getutcdate(), newid(), 'Z0')";

		ZDateTime InsertQueue(string ev, bool utc, bool queued)
		{
			ZDateTime postedTime = utc ? ZDateTime.UtcNow.ToDateTime() : ZDateTime.UtcNow.AddDays(-1).ToDateTime();
			using (var cmd = Db.Connection.Command(string.Format(InsertQueueSql, ev, queued ? "QUE" : "PRS")))
			{
				cmd.AddParameterBasedOnDbColumn("@date", postedTime.ToDateTime(), StmJobQueueSchema.SJ_PostedTimeUtc);
				cmd.ExecuteNonQuery();
			}
			return postedTime;
		}

		void AddLogQueue(string eventName)
		{
			using (var cmd = Db.Connection.Command("INSERT INTO dbo.StmALogQueue (SLQ_ALogReference, SLQ_ParentID, SLQ_ParentTableName, SLQ_SE_NKEvent, SLQ_PostedTimeUtc, SLQ_EventTime) VALUES (newid(), newid(), @TableName, @Event, @PostedTime, GETDATE())"))
			{
				cmd.AddParameter("@TableName", SqlDbType.VarChar, GlbStaffSchema.Constants.TableName);
				cmd.AddParameter("@Event", SqlDbType.VarChar, eventName);
				cmd.AddParameter("@PostedTime", SqlDbType.DateTime, ZDateTime.UtcNow.ToDateTime());
				cmd.ExecuteNonQuery();
			}
		}

		void AddWTELogQueue()
		{
			using (var cmd = Db.Connection.Command("INSERT INTO dbo.StmALogQueueWTE (WTE_ALogReference, WTE_ParentID, WTE_ParentTableName, WTE_PostedTimeUtc, WTE_EventTime) VALUES (newid(), newid(), @TableName, @PostedTime, GETDATE())"))
			{
				cmd.AddParameter("@TableName", SqlDbType.VarChar, ProcessTask.Schema.TableName);
				cmd.AddParameter("@PostedTime", SqlDbType.DateTime, ZDateTime.UtcNow.ToDateTime());
				cmd.ExecuteNonQuery();
			}
		}
	}
}
