using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	class RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueueTest : TestCaseWithFactory
	{
		[TestDate(2016, 1, 15)]
		public void TestTrigger_WIP_ACR()
		{
			//Should Invoke Trigger
			var wipacr = Helper.CreateACRWIPLine(job, 1, 111, 110, reverseWIP: false, reverseACR: false);
			var wip = wipacr.Item1;
			var acr = wipacr.Item2;

			AssertEquals("Total number of JobCostingDataQueue records", 2, Helper.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue"));
			AssertLines(1, wip.PK, null, wip.AL_PostDate, Env.CurrentCompanyPK);
			AssertLines(1, acr.PK, null, acr.AL_PostDate, Env.CurrentCompanyPK);

			//Shouldn't Invoke Trigger, as AL_Reversedate column isn't updated
			Helper.Connection.ExecuteNonQuery($"UPDATE dbo.AccTransactionLines SET AL_Desc = 'Trying New Desc', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{wip.PK}'");
			AssertLines(1, wip.PK, null, wip.AL_PostDate, companyPK);

			//Should Invoke Trigger, as AL_Reversedate is updated
			Helper.Connection.ExecuteNonQuery($"UPDATE dbo.AccTransactionLines SET AL_ReverseDate = '{new DateTime(2016, 01, 25).ToString("dd MMM yyyy")}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{wip.PK}'");

			AssertEquals("Total number of JobCostingDataQueue records", 3, Helper.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue"));

			AssertLines(1, wip.PK, null, new DateTime(2016, 01, 15), companyPK);
			AssertLines(1, wip.PK, new DateTime(2016, 01, 25), null, companyPK);
		}

		[TestDate(2016, 1, 15)]
		public void TestTrigger_CST_REV()
		{
			//Should Invoke Trigger. But JobCostingDataQueue Table shouldn't have any row for this transaction Line, as AL_Reverse Date is missing.
			var apLine = Helper.CreateCSTLine(job, 1, 113, reverse: false);

			//Should Invoke Trigger and JobCostingDataQueue Table should have a row for this transaction Line, as AL_Reverse Date is NOT null 
			var arLine = Helper.CreateREVLine(job, 1, 112, reverse: true, reverseDate: new DateTime(2016, 01, 19));

			AssertEquals("Total number of JobCostingDataQueue records", 1, Helper.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue"));

			AssertLines(0, apLine.PK, null, new DateTime(2016, 01, 15), companyPK);
			AssertLines(1, arLine.PK, new DateTime(2016, 01, 19), null, companyPK);

			//Should Invoke Trigger, as AL_Reversedate is updated
			Helper.Connection.ExecuteNonQuery($"UPDATE dbo.AccTransactionLines SET AL_ReverseDate = '{new DateTime(2016, 01, 25).ToString("dd MMM yyyy")}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{apLine.PK}'");

			AssertEquals("Total number of JobCostingDataQueue records", 2, Helper.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue"));

			AssertLines(1, apLine.PK, new DateTime(2016, 01, 25), null, companyPK);
			AssertLines(1, arLine.PK, new DateTime(2016, 01, 19), null, companyPK);
		}

		public void TestTrigger_WIPWithoutGL()
		{
			//Should Invoke Trigger. But JobCostingDataQueue Table shouldn't have any row for this transaction Line, as AL_AG is missing.

			var glLessWIP = ObjectCreator.CreateWIP(job, ObjectCreator.CC1, 1.0M, "WIP without GL", 100M);
			glLessWIP.AL_AG = ZGuid.Empty;

			AssertEquals("Total number of JobCostingDataQueue records", 0, Helper.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue"));
		}

		public void TestTrigger_InvoiceWithoutJob()
		{
			//Should Invoke Trigger. But JobCostingDataQueue Table shouldn't have any row for this transaction Line, as AL_JH is missing.
			Helper.CreateNonJCLine(job);
			AssertEquals("Total number of JobCostingDataQueue records", 0, Helper.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue"));
		}

		void AssertLines(int totalNumber, ZGuid linePK, ZDateTime? reverseDate, ZDateTime? postDate, ZGuid al_gc)
		{
			var sql = string.Format("SELECT COUNT(1) FROM dbo.JobCostingDataQueue WHERE JCQ_ALPK = '{2}' AND JCQ_ReverseDate {0} AND JCQ_PostDate {1} AND JCQ_GCPK='{3}'", reverseDate.HasValue ? $"='{reverseDate.Value.ToString("dd MMM yyyy")}'" : "IS NULL", postDate.HasValue ? $"='{postDate.Value.ToString("dd MMM yyyy")}'" : "IS NULL", linePK, companyPK);
			AssertEquals("JobCostingDataQueue record must exist.", totalNumber, Helper.Connection.ExecuteScalar<int>(sql));
		}

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = Env.CurrentCompanyPK;

			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.PostPeriodsForEntireYear(2016, companyPK);

			job = ObjectCreator.CreateJob("JOB1", ObjectCreator.LocalClient2, 1.0M, ObjectCreator.Agent2, 1.0M);

			JCDTableAndPartitionCreator.DeleteTempTableAndIndex(TestConnection, (s) => { });

			JCDTableAndPartitionCreator.DeleteMainReportTableAndPartition(TestConnection, (s) => { });

			JCDTableAndPartitionCreator.Create(TestConnection, 0, (s) => { });

			using (AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, -1))
			{
				var logger = new TestServiceLogger();
				var versionManager = JCDDependentObjectList.GetVersionManager();
				var synchronizer = new DBObjectSynchronizer_ForTest(TestConnection, logger, versionManager);
				synchronizer.Synchronize();
			}
		}

		JobCostingReportDataTestHelper Helper
		{
			get { return helper ?? (helper = new JobCostingReportDataTestHelper(ObjectCreator)); }
		}
		JobCostingReportDataTestHelper helper;

		TestObjectCreator ObjectCreator
		{
			get { return objectCreator ?? (objectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator objectCreator;

		ZGuid companyPK;
		Job job;
	}
}
