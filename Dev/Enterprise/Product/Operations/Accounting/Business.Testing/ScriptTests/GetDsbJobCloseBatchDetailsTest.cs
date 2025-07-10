using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class GetDsbJobCloseBatchDetailsTest : ScriptTest
	{
		public void TestGetBatchDetails()
		{
			//Prepare date for batch 1
			var dsbJobBatch = TestObjectCreator.CreateDsbJobCloseBatch("123456");
			var shipment = TestObjectCreator.CreateShipment("1000");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = TestObjectCreator.AALSHI.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Enterprise.Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

			var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
			var line1 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, chargeCode, TestObjectCreator.CNY, 1M, "Test", 100m);
			line1.AL_JBB = dsbJobBatch.PK;
			TestObjectCreator.CreateCharge(line1);
			var line2 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, chargeCode, TestObjectCreator.CNY, 1M, "Test", 200m);
			line2.AL_JBB = dsbJobBatch.PK;
			TestObjectCreator.CreateCharge(line2);

			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV002", TestObjectCreator.CNY, 1m, 30m, 0m, 0m, 300m, 0m, 0m, TestObjectCreator.AALSHI);
			var line3 = TestObjectCreator.CreateAPInvoiceLine(invoice2, job, chargeCode, TestObjectCreator.CNY, 1M, "Test", 300m);
			line3.AL_JBB = dsbJobBatch.PK;
			TestObjectCreator.CreateCharge(line3);

			//Prepare date for batch 2
			var dsbJobBatch1 = TestObjectCreator.CreateDsbJobCloseBatch("456789");
			var shipment1 = TestObjectCreator.CreateShipment("2000");
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job1.JH_ParentID = shipment1.PK;

			var invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV003", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
			var line4 = TestObjectCreator.CreateARInvoiceLine(invoice3, job1, chargeCode, TestObjectCreator.CNY, 1M, "Test", 400m);
			line4.AL_JBB = dsbJobBatch1.PK;
			TestObjectCreator.CreateCharge(line4);
			var line5 = TestObjectCreator.CreateARInvoiceLine(invoice3, job1, chargeCode, TestObjectCreator.CNY, 1M, "Test", 500m);
			line5.AL_JBB = dsbJobBatch1.PK;
			TestObjectCreator.CreateCharge(line5);

			Factory.Save();

			var result = RunScript(dsbJobBatch.PK);
			AssertEquals(3, result.Rows.Count);
			AssertBatchDetailLines(100);
			AssertBatchDetailLines(200);
			AssertBatchDetailLines(-300);

			void AssertBatchDetailLines(decimal lineLocalAmount)
			{
				var rows = result.Select($"LineLocalAmount = {lineLocalAmount}");
				AssertEquals("Field JH_JobNum", job.JH_JobNum, rows[0]["JobNumber"]);
				AssertEquals("Field LineBranch", GlbBranch.CurrentBranch.GB_Code, rows[0]["LineBranch"]);
				AssertEquals("Field LineDepartment", GlbDepartment.CurrentDepartment.GE_Code, rows[0]["LineDepartment"]);
				AssertEquals("Field ChargeCode", chargeCode.AC_Code, rows[0]["ChargeCode"]);
				AssertEquals("Field LineGLAccount", TestObjectCreator.GLHeader1.AG_AccountNum, rows[0]["LineGLAccount"]);
				AssertEquals("Field DSBSurplusGLAccount", TestObjectCreator.GLHeader1.AG_AccountNum, rows[0]["DSBSurplusGLAccount"]);
				AssertEquals("Field DSBShortFallGLAccount", TestObjectCreator.GLHeader2.AG_AccountNum, rows[0]["DSBShortFallGLAccount"]);
			}
		}

		DataTable RunScript(ZGuid batchPK)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format($@"
SELECT * 
FROM GetDsbJobCloseBatchDetails ('{batchPK}')
"
			));
		}
	}
}

