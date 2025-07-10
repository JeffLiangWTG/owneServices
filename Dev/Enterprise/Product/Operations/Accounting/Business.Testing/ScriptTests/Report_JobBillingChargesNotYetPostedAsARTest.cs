

using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_JobBillingChargesNotYetPostedAsARTest : ScriptTest
	{
		public void TestJobBillingChargesNotYetPostedAsAR()
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
			job1.Charges[0].JR_LocalSellAmt = job2.Charges[0].JR_LocalSellAmt = 1;

			Factory.Save();

			DataTable resultForSettlementGroup = RunScript();

			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);
			AssertEquals("JobNum Should be 001 for record", 1, resultForSettlementGroup.Select("JobNumber = '001'").Length);
		}

		public void TestJobBillingChargesNotYetPostedAsAR_ControllingCustomerAndAgent()
		{
			var infoChecker = new CAGAndCCBInfoScriptTest();
			infoChecker.VerifyControllingCustomerAndAgentInfo(RunScript, revenueSide: true);
		}

		DataTable RunScript(string whereClause = "", ZGuid? mNGOrgPK = null)
		{
			var sql = string.Format(@"
SELECT * 
FROM Report_JobBillingChargesNotYetPostedAsAR(
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

