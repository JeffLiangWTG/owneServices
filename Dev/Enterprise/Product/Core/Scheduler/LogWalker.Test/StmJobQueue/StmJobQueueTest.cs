using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.LogWalker.Test
{
	[TestedType(typeof(StmJobQueue))]
	sealed class StmJobQueueTest : EnterpriseBusinessObjectTestCase
	{
		/// <summary>
		/// Overriden here just so Enterprise.LogWalker.Testing.StmJobQueueTest is in the Call Stack,
		/// allowing StmJobQueue.Delete to be called.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Overridden so class is in the call stack")]
		public override void TestSaveAndDeleteBusinessObject()
		{
			base.TestSaveAndDeleteBusinessObject();
		}

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(StmJobQueue)));
		}

		/// <summary>
		/// Autologging StmJobQueue could potentially cause infinite event log feedback.
		/// Event added to StmALog => Added to StmJobQueue => Logged back to StmALog.
		/// </summary>
		public void TestNotAutoLogged()
		{
			StmJobQueue job = Factory.New<StmJobQueue>();
			AssertEquals("IsAutoLogged", false, job.IsAutoLogged);
		}

		public void TestStmJobQueueClassIsInternal()
		{
			AssertEquals("StmJobQueue is public", false, typeof(StmJobQueue).IsPublic);
			AssertEquals("IQueuedLog is public", true, typeof(IQueuedLog).IsPublic);
		}

		public void TestIsRetry()
		{
			StmJobQueue job = Factory.New<StmJobQueue>();
			job.SJ_RetryCount++;
			AssertEquals("IsRetry - initial value", false, ((IQueuedLog)job).IsRetry);
			job.SJ_RetryCount++;
			AssertEquals("IsRetry - after retry count increment", true, ((IQueuedLog)job).IsRetry);
		}

		public void TestConcurrencyPolicy()
		{
			var defaultConcurrencyPolicyColumns = new List<string>() { StmJobQueue.Schema.PK, nameof(StmJobQueue.SJ_RetryCount), nameof(StmJobQueue.SJ_Status) };
			var bizo = Factory.New<StmJobQueue>();
			var row = ((INeedRow)bizo).Row;
			foreach (DataColumn column in row.Table.Columns)
			{
				if (defaultConcurrencyPolicyColumns.Contains(column.ColumnName))
				{
					AssertEquals(ConcurrencyPolicy.Default, ConcurrencyInfo.Get(row, column));
				}
				else
				{
					AssertEquals(ConcurrencyPolicy.Ignore, ConcurrencyInfo.Get(row, column));
				}
			}
		}

		#region IQueuedLogDelayer

		public void TestEventTime()
		{
			var job = Factory.New<StmJobQueue>();
			AssertNotEquals(ZDateTime.BrettsBirthday, job.SJ_EventTime);

			var logDelayer = job as IQueuedLogDelayer;
			logDelayer.EventTime = ZDateTime.BrettsBirthday;

			AssertEquals(ZDateTime.BrettsBirthday, job.SJ_EventTime);
		}

		public void TestIsDelayFired()
		{
			var job = Factory.New<StmJobQueue>();
			Assert(!job.SJ_IsDelayFired);

			var logDelayer = job as IQueuedLogDelayer;
			job.SJ_IsDelayFired = true;
			Assert(job.SJ_IsDelayFired);
			Assert(logDelayer.IsDelayFired);

			logDelayer.IsDelayFired = false;

			Assert(!job.SJ_IsDelayFired);
			Assert(!logDelayer.IsDelayFired);
		}

		#endregion

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestEventTimeOffsetNotUsingBranch()
		{
			var log = Factory.NewWithValidTestData<StmJobQueue>();
			var date = ZDateTimeOffset.Now;
			log.SJ_EventTime = date.ToDateTime();
			log.SJ_EventTimeUtc = date.ToDateTime().ToUniversalTime();
			Assert(log.SJ_EventTime - log.SJ_EventTimeUtc != System.TimeSpan.Zero);
			log.SJ_GB_NKBranch = "SIN";
			AssertEquals(date, ((IStmALog)log).SL_EventTimeOffset);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestEventTimeOffsetIsUsingBranchIfNoUtc()
		{
			var log = Factory.NewWithValidTestData<StmJobQueue>();
			var date = ZDateTimeOffset.Now;
			log.SJ_EventTime = date.ToDateTime();
			log.SJ_EventTimeUtc = ZDateTime.Empty;
			log.SJ_GB_NKBranch = "SIN";
			AssertEquals(StmALog.ToDateTimeOffset(Factory, log.SJ_EventTime, log.SJ_GB_NKBranch), ((IStmALog)log).SL_EventTimeOffset);
		}
	}
}
