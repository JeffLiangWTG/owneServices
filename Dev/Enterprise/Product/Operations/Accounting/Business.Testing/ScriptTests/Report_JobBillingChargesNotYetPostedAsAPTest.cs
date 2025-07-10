
using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_JobBillingChargesNotYetPostedAsAPTest : ScriptTest
	{
		public void TestJobBillingChargesNotYetPostedAsAPWithJobRevenueJournal()
		{
			Job job1 = TestObjectCreator.Job1;
			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job1, 100M);
			journal.BranchPKFrom = GlbBranch.CurrentBranch.PK;
			journal.BranchPKTo = TestObjectCreator.NonCurrentBranch.PK;
			journal.DefaultSharing = 50m;
			journal.DepartmentPKFrom = TestObjectCreator.FISDepartment.PK;
			journal.DepartmentPKTo = TestObjectCreator.GEADepartment.PK;
			journal.CostRevenueTypeFrom = TransactionLineTypes.Cost;
			journal.CostRevenueTypeTo = TransactionLineTypes.Revenue;
			var charge = journal.JournalCharges.AddNew();
			charge.ChargeCode = TestObjectCreator.FRT.PK;
			charge.Share = 50m;
			charge.BillingTabOsAmount = 100m;
			charge.BillingTabLocalAmount = 100m;
			charge.ExchangeRate = 1m;
			charge.SharedOsAmount = 50m;
			charge.SharedLocalAmount = 50m;
			TestObjectCreator.Factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(job1);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			Factory.Save();

			string clause = $" WHERE ChargeCodePK = '{TestObjectCreator.FRT.PK}' ";
			DataTable resultForSettlementGroup = RunScript(whereClause: clause);

			AssertEquals(0, resultForSettlementGroup.Rows.Count);
		}

		public void TestJobBillingChargesNotYetPostedAsAP()
		{
			Job job1 = TestObjectCreator.Job1;
			Job job2 = TestObjectCreator.Job2;

			job1.JH_JobNum = "001";
			job2.JH_JobNum = "002";

			job1.JH_ParentTableCode = "JS";
			job2.JH_ParentTableCode = "TH";

			job1.Charges.AddNew();
			job2.Charges.AddNew();

			job1.Charges[0].JR_AC = job2.Charges[0].JR_AC = TestObjectCreator.CC1.PK;
			job1.Charges[0].JR_LocalCostAmt = job2.Charges[0].JR_LocalCostAmt = 1;

			Factory.Save();

			DataTable resultForSettlementGroup = RunScript();

			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);
			AssertEquals("JobNum Should be 001 for record", 1, resultForSettlementGroup.Select("JobNumber = '001'").Length);
		}

		public void TestJobBillingChargesNotYetPostedAsAP_ControllingCustomerAndAgent()
		{
			var infoChecker = new CAGAndCCBInfoScriptTest();
			infoChecker.VerifyControllingCustomerAndAgentInfo(RunScript);
		}

		public void TestJobBillingChargesNotYetPostedAsAP_JobInactive()
		{
			var job1 = TestObjectCreator.Job1;
			var job2 = TestObjectCreator.Job2;

			job1.JH_JobNum = "001";
			job2.JH_JobNum = "002";

			job1.JH_ParentTableCode = "JS";
			job2.JH_ParentTableCode = "JS";

			job1.Charges.AddNew();
			job2.Charges.AddNew();

			job1.Charges[0].JR_AC = job2.Charges[0].JR_AC = TestObjectCreator.CC1.PK;
			job1.Charges[0].JR_LocalCostAmt = job2.Charges[0].JR_LocalCostAmt = 1;

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

			AssertEquals("Result should contain 2 record", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("JobNum Should be 001 for record", 1, resultForSettlementGroup.Select("JobNumber = '001'").Length);
			AssertEquals("Should have inactive record", 1, resultForSettlementGroup.Select("JobNumber = '002'").Length);
		}

		DataTable RunScript(string whereClause = "", ZGuid? mNGOrgPK = null)
		{
			var sql = string.Format(@"
SELECT * 
FROM Report_JobBillingChargesNotYetPostedAsAP(
'{0}'
, @MNGListValue
, @MNGListIsEmptyValue		
)
{1}  
",
			GlbCompany.CurrentCompany.PK,
			whereClause
			);

			var command = Db.Connection.Command(sql);
			AddTVPAndIsEmptyParameters(command, "@MNGListValue", "dbo.TVP_uniqueidentifier", "@MNGListIsEmptyValue", mNGOrgPK.HasValue ? new Guid[] { mNGOrgPK.Value.ToGuid() } : Array.Empty<Guid>());
			return DataUtils.GetDataTableFromCommand(command);
		}
	}
}


