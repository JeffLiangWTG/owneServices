

using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ExceptionReportingChargesPreparedNotYetPostedTest : ScriptTest
	{
		public void TestExceptionReportingChargesPreparedNotYetPosted()
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "001";

			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_JobNum = "002";

			job1.JH_ParentTableCode = "JS";
			job2.JH_ParentTableCode = "TH";

			job1.Charges.AddNew();
			job2.Charges.AddNew();

			job1.Charges[0].JR_AC = job2.Charges[0].JR_AC = TestObjectCreator.CC1.PK;
			job1.Charges[0].JR_LocalCostAmt = job2.Charges[0].JR_LocalCostAmt = -1;

			Factory.Save();

			DataTable resultForSettlementGroup = RunScript();

			AssertEquals("Result should contain 2 record", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("JobNum should be 001", 2, resultForSettlementGroup.Select("JH_JobNum = '001'").Length);
		}

		public void TestExceptionReportingChargesPreparedNotYetPosted_JobInactive()
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "001";

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_JobNum = "002";

			job1.JH_ParentTableCode = "JS";
			job2.JH_ParentTableCode = "JS";

			job1.Charges.AddNew();
			job2.Charges.AddNew();

			job1.Charges[0].JR_AC = job2.Charges[0].JR_AC = TestObjectCreator.CC1.PK;
			job1.Charges[0].JR_LocalCostAmt = job2.Charges[0].JR_LocalCostAmt = -1;

			Factory.Save();

			string deActivateJobSql = $@"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = '{job2.PK}'";
			DataUtils.GetDataTableFromQuery(Db.Connection, deActivateJobSql);

			var resultForSettlementGroup = RunScript();

			AssertEquals("Result should contain 4 record", 4, resultForSettlementGroup.Rows.Count);
			AssertEquals("JobNum should be 001", 2, resultForSettlementGroup.Select("JH_JobNum = '001'").Length);
			AssertEquals("Should have inactive record", 2, resultForSettlementGroup.Select("JH_JobNum = '002'").Length);
		}

		DataTable RunScript()
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_ExceptionReportingChargesPreparedNotYetPosted(
'{0}','',NULL
)  
",
			GlbCompany.CurrentCompany.PK
			));
		}
	}
}


