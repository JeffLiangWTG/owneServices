using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ChangeDataCapture.Service.Testing
{
	[TestedType(typeof(CleanupTask))]
	class CleanupTaskTest : ServiceTaskTestCase<CleanupTask>
	{
		public void TestInitialiseSchedule()
		{
			var testTask = new CleanupTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			AssertEquals("ScheduleTask - IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("ScheduleTask - TaskPeriod", taskSchedule.Recurrence.HoursRange);
			AssertEquals("ScheduleTask - TaskPeriodCount", 1, taskSchedule.Recurrence.Period);
			AssertEquals("ScheduleTask - DailyStartTime", "01:00", taskSchedule.Recurrence.CalcDailyStartTimeLocalText);
			AssertEquals("ScheduleTask - WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
		}

		public void TestAllowMultipleInstances()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			if (thisClassAttribute != null)
			{
				AssertEquals("AllowsMultipleInstances", false, thisClassAttribute.AllowsMultipleInstances);
			}
		}

		public void TestLsnComparison_Audit()
		{
			var auditLsn = "0x00018AC8000072FB000D"; // lower LSN
			var edwLsn = "0x00018AC80000781C0005";

			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var sqlText = $@"
					DELETE FROM [biadmin].[MasterState] WHERE ParamName in ('{BiConstants.LastMaxLsnProcessed}')
					INSERT INTO [biadmin].[MasterState] (ParamName, ParamValue) VALUES ('{BiConstants.LastMaxLsnProcessed}', '{auditLsn}')";
				TestConnection.ExecuteNonQuery(sqlText);
			}
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				var sqlText = $"UPDATE [biadmin].[StagingTableState] SET CurrentMaxLsn = {edwLsn}";
				TestConnection.ExecuteNonQuery(sqlText);
			}

			var lowerLsn = new CleanupTaskForTesting().GetLsnWatermark_Exposed(TestConnection);

			AssertEquals("Lower LSN", auditLsn, lowerLsn);
		}

		public void TestLsnComparison_EDW()
		{
			var auditLsn = "0x00018AC80000781C0005";
			var edwLsn = "0x00018AC8000072FB000D"; // lower LSN

			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var sqlText = $@"
					DELETE FROM [biadmin].[MasterState] WHERE ParamName in ('{BiConstants.LastMaxLsnProcessed}')
					INSERT INTO [biadmin].[MasterState] (ParamName, ParamValue) VALUES ('{BiConstants.LastMaxLsnProcessed}', '{auditLsn}')";
				TestConnection.ExecuteNonQuery(sqlText);
			}
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				var sqlText = $"UPDATE [biadmin].[StagingTableState] SET CurrentMaxLsn = {edwLsn}";
				TestConnection.ExecuteNonQuery(sqlText);
			}

			var lowerLsn = new CleanupTaskForTesting().GetLsnWatermark_Exposed(TestConnection);

			AssertEquals("Lower LSN", edwLsn, lowerLsn);
		}

		public void TestExceptionDuplicateDataInMasterState_Audit()
		{
			var auditLsn = "0x00018AC80000781C0005";
			var sqlText = $"INSERT INTO [biadmin].[MasterState] (ParamName, ParamValue) VALUES ('LAST_MAX_LSN_PROCESSED', '{auditLsn}')";
			try
			{
				using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
				{
					TestConnection.ExecuteNonQuery(sqlText);
					TestConnection.ExecuteNonQuery(sqlText);
				}
				Fail("Duplicate Data Insert success in Audit");
			}
			catch (SqlException ex)
			{
				if (DbErrorMatch.GetExceptionType(ex) == DbErrorType.CannotInsertDuplicateUniqueIndexKey || DbErrorMatch.GetExceptionType(ex) == DbErrorType.CannotInsertDuplicateConstraintKey) 
				{
					Assert("Duplicate Data Insert fail in Audit", true);
				}
				else
				{
					Fail("Duplicate Data Insert success in Audit");
				}
			}
		}

		public void TestExceptionDuplicateDataInMasterState_EDW()
		{
			var auditLsn = "0x00018AC80000781C0005";
			var sqlText = $"INSERT INTO [biadmin].[MasterState] (ParamName, ParamValue) VALUES ('LAST_MAX_LSN_PROCESSED', '{auditLsn}')";
			try
			{
				using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
				{
					TestConnection.ExecuteNonQuery(sqlText);
					TestConnection.ExecuteNonQuery(sqlText);
				}
				Fail("Duplicate Data Insert success in EDW");
			}
			catch (SqlException ex)
			{
				if (DbErrorMatch.GetExceptionType(ex) == DbErrorType.CannotInsertDuplicateUniqueIndexKey || DbErrorMatch.GetExceptionType(ex) == DbErrorType.CannotInsertDuplicateConstraintKey)
				{
					Assert("Duplicate Data Insert fail in EDW", true);
				}
				else
				{
					Fail("Duplicate Data Insert success in EDW");
				}
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		class CleanupTaskForTesting : CleanupTask
		{
			public string GetLsnWatermark_Exposed(DbConnection biConnection)
			{
				var lsn = GetLsnWatermark(biConnection, biConnection);
				return "0x" + BitConverter.ToString(lsn).Replace("-", "");
			}

			protected override byte[] GetLsnByTimeOffset(byte[] minLSN, TimeSpan timespan)
			{
				return minLSN;
			}
		}
	}
}
