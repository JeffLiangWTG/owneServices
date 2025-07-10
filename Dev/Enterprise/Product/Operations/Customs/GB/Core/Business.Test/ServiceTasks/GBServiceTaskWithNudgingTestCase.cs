using System;
using CargoWise.Data;
using CargoWise.Types;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.GB.Business.Testing
{
	public abstract class GBServiceTaskWithNudgingTestCase<T> : GBServiceTaskTestCase<T> where T : ServiceProviderImpl, new()
	{
		[TestDate(2008, 12, 11, 12, 0, 0)]
		public void TestExtraNudgingOfServiceTask()
		{
			SetupServiceTask();
			SetRegistryNudgeTime(DateTime.MinValue);

			AssertServiceTaskNextRunDateTimeUtc(ServiceTaskCode, "Pre-req: Next poll scheduled at 12:00:00", new ZDateTime(2008, 12, 11, 12, 0, 0));

			AssertEquals("Pre-req: no extra nudge", DateTime.MinValue, GetRegistryNudgeTime());
			var serviceTask = GetInstance();
			var logText = InitialiseAndRunTaskSchedule(serviceTask).ToString();
			AssertNotContains("Should not be nudged", "manually nudged itself ", logText);
			AssertEquals("No extra nudge", DateTime.MinValue, GetRegistryNudgeTime());

			SetRegistryNudgeTime(ZDateTime.UtcNow.ToDateTime().AddMinutes(5));
			AssertServiceTaskNextRunDateTimeUtc(ServiceTaskCode, "Pre-req: Next poll scheduled at 12:00:00", new ZDateTime(2008, 12, 11, 12, 0, 0));
			AssertEquals("Pre-req: extra nudge in 5 mins", ZDateTime.UtcNow.ToDateTime().AddMinutes(5), GetRegistryNudgeTime());
			serviceTask = GetInstance();
			logText = InitialiseAndRunTaskSchedule(serviceTask).ToString();
			AssertContains("Should be nudged", ZString.Format("{0} task nudged to start at", ServiceTaskCode), logText);
			AssertEquals("Reset extra nudge", DateTime.MinValue, GetRegistryNudgeTime());
		}

		void AssertServiceTaskNextRunDateTimeUtc(ZString taskCode, ZString assertionText, ZDateTime expectedNextRunDateTimeUtc)
		{
			var conn = Db.Connection;
			{
				var sql = string.Format(@"SELECT S5_NextScheduledPrintRunTimeUtc FROM dbo.StmScheduleTask WHERE S5_ScheduleType = '{0}'", taskCode);
				AssertEquals(assertionText, expectedNextRunDateTimeUtc, conn.ExecuteScalar(sql));
			}
		}

		void SetupServiceTask()
		{
			var conn = Db.Connection;
			{
				conn.ExecuteNonQuery(ZString.Format(@"
INSERT INTO dbo.StmScheduleTask (S5_NextScheduledPrintRunTimeUtc, S5_PK, S5_ParentTableCode, S5_ScheduleType, S5_TypeOfDocument, S5_ScheduleDescription, S5_TaskPeriod, S5_TaskPeriodCount) VALUES ('{1}', NEWID(), 'SH', '{0}', 'GBC', 'service task', 'T', 15)",
				ServiceTaskCode, new ZDateTime(2008, 12, 11, 12, 0, 0)));
			}
		}

		protected abstract void SetRegistryNudgeTime(DateTime date);

		protected abstract ZDateTime GetRegistryNudgeTime();
	}
}
