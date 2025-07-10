using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class ShouldJobBeClosedByDsbBatchTest : ScriptTest
	{
		public void TestShouldJobBeClosedByDsbBatchProperly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARBuyersConsolInvoicingStyle = Enterprise.Core.Constants.ConsolInvoicingStyles.Master;
			var chargeCode = TestObjectCreator.CreateChargeCode("TCDSB", "Test dsb charge type", Core.Constants.ChargeType.Disbursement, 1.0m, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			chargeCode.AC_ChargeGroup = Enterprise.Core.Constants.ChargeType.Disbursement;

			var subShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			subShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;
			subShipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_ReleaseType = Enterprise.Core.Constants.ShipmentReleaseTypes.BankSightDraft;

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.LocalChargesPK = org.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = subShipment.PK;

			Factory.Save();

			var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.CNY, 1M, TestObjectCreator.ABIGAS);
			var revenueLine1 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, chargeCode, TestObjectCreator.CNY, 1M, "Test", 100m);
			revenueLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
			var charge1 = TestObjectCreator.CreateCharge(revenueLine1);

			Factory.Save();

			var charge2 = TestObjectCreator.CreateCharge(job, chargeCode, 100m, 190m);

			Factory.Save();

			var linesWithTargetCharge = Factory.CreateNewFactory().Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_AC, chargeCode.PK));
			AssertEquals("Pre-condition: 1 REV / 2 ACR / 1 WIP line", 4, linesWithTargetCharge.Length);
			AssertEquals(2, linesWithTargetCharge.Count(x => x.AL_LineType == TransactionLineTypes.Accrual));
			AssertEquals(1, linesWithTargetCharge.Count(x => x.AL_LineType == TransactionLineTypes.Revenue));
			AssertEquals(1, linesWithTargetCharge.Count(x => x.AL_LineType == TransactionLineTypes.WIP));
			AssertNotEquals(charge1.PK, charge2.PK);
			AssertEquals(charge1.JR_AC, charge2.JR_AC);

			var result = RunScript(job.PK);
			AssertEquals("Pre-condition", false, result);

			chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;
			Factory.Save();

			result = RunScript(job.PK);
			AssertEquals("This job contains unbalance posted disbursement charges.", true, result);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var costLine1 = TestObjectCreator.CreateCostLine(charge1, invoice2.PK);
			costLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			linesWithTargetCharge = Factory.CreateNewFactory().Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_AC, chargeCode.PK));
			var totalAmount = 0m;
			foreach (var line in linesWithTargetCharge.Where(x => x.AL_LineType == TransactionLineTypes.Cost || x.AL_LineType == TransactionLineTypes.Revenue))
			{
				AssertEquals("The Line GL Account should be GLHeader1", line.AL_AG, TestObjectCreator.GLHeader1.PK);
				AssertEquals("The branch of the line should be current branch", line.AL_GB, GlbBranch.CurrentBranch.PK);
				AssertEquals("The department of the line should be current department", line.AL_GE, GlbDepartment.CurrentDepartment.PK);
				totalAmount += line.AL_LineAmount;
			}
			AssertEquals("The total sum of the posted lines which related to DSB charges should be 0.", 0m, totalAmount);
			result = RunScript(job.PK);
			AssertEquals("This job contains balance posted disbursement charges.", false, result);
		}

		public void TestShouldJobBeClosedByDsbBatchProperlyWithJCJRJ()
		{
			var surplusGLAccount1 = TestObjectCreator.CreateGLHeader("99991001");
			var shortfallGLAccount1 = TestObjectCreator.CreateGLHeader("99991002");

			var costGLAccount1 = TestObjectCreator.CreateGLHeader("39991001");
			var revenueGLAccount1 = TestObjectCreator.CreateGLHeader("39991002");

			var chargeCode1 = TestObjectCreator.DSBChargeCode;
			chargeCode1.AC_AG_CostAccount = costGLAccount1.PK;
			chargeCode1.AC_AG_RevenueAccount = revenueGLAccount1.PK;
			chargeCode1.AC_AG_DisbursementShortfallAccount = shortfallGLAccount1.PK;
			chargeCode1.AC_AG_DisbursementSurplusAccount = surplusGLAccount1.PK;

			Factory.Save();

			var shipment1 = TestObjectCreator.CreateShipment("111111", true);
			var shipment2 = TestObjectCreator.CreateShipment("222222", true);

			var jobWtihDSBCharge = TestObjectCreator.CreateJobHeader();
			jobWtihDSBCharge.JH_JobNum = "J001";
			jobWtihDSBCharge.JH_ParentID = shipment1.PK;

			var jobWtihOutDSBCharge = TestObjectCreator.CreateJobHeader();
			jobWtihOutDSBCharge.JH_JobNum = "J002";
			jobWtihOutDSBCharge.JH_ParentID = shipment2.PK;

			TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, jobWtihOutDSBCharge, 100);

			Factory.Save();

			var linesWithTargetCharge = Factory.CreateNewFactory().Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_AC, TestObjectCreator.CC1.PK));
			AssertEquals("Pre-condition: 2 REV", 2, linesWithTargetCharge.Length);
			AssertEquals(2, linesWithTargetCharge.Count(x => x.AL_LineType == TransactionLineTypes.Revenue));

			var result = RunScript(jobWtihOutDSBCharge.PK);
			AssertEquals("Pre-condition", false, result);

			JobRevenueJournal journal = Factory.New<JobRevenueJournal>();
			journal.AH_Desc = "Desc";
			var journalLine1 = TestObjectCreator.CreateJobRevenueJournalLine(journal, chargeCode1, jobWtihDSBCharge.PK, 100, DebitCreditDataEntry.DR);
			var journalLine2 = TestObjectCreator.CreateJobRevenueJournalLine(journal, TestObjectCreator.CC1, jobWtihOutDSBCharge.PK, 100, DebitCreditDataEntry.CR);
			journalLine1.CostRevenueType = "CST";
			journalLine2.CostRevenueType = "REV";

			Factory.Save();

			result = RunScript(jobWtihDSBCharge.PK);
			AssertEquals("This job contains unbalance posted disbursement charges.", true, result);

			JobRevenueJournal journal1 = Factory.New<JobRevenueJournal>();
			journal1.AH_Desc = "Desc";
			var journalLine3 = TestObjectCreator.CreateJobRevenueJournalLine(journal1, chargeCode1, jobWtihDSBCharge.PK, 100, DebitCreditDataEntry.CR);
			var journalLine4 = TestObjectCreator.CreateJobRevenueJournalLine(journal1, TestObjectCreator.CC1, jobWtihOutDSBCharge.PK, 100, DebitCreditDataEntry.DR);
			journalLine3.CostRevenueType = "CST";
			journalLine4.CostRevenueType = "REV";

			Factory.Save();

			linesWithTargetCharge = Factory.CreateNewFactory().Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_AC, chargeCode1.PK));
			var totalAmount = 0m;
			foreach (var line in linesWithTargetCharge.Where(x => x.AL_LineType == TransactionLineTypes.Cost || x.AL_LineType == TransactionLineTypes.Revenue))
			{
				AssertEquals("The Line GL Account should be GLHeader1", line.AL_AG, TestObjectCreator.GLHeader1.PK);
				AssertEquals("The branch of the line should be current branch", line.AL_GB, GlbBranch.CurrentBranch.PK);
				AssertEquals("The department of the line should be current department", line.AL_GE, GlbDepartment.CurrentDepartment.PK);
				totalAmount += line.AL_LineAmount;
			}
			AssertEquals("The total sum of the posted lines which related to DSB charges should be 0.", 0m, totalAmount);
			result = RunScript(jobWtihDSBCharge.PK);
			AssertEquals("This job contains balance posted disbursement charges.", false, result);
		}

		public void TestShouldJobBeClosedByDsbBatchWithJR_ChargeTypeIsEmpty()
		{
			var job = TestObjectCreator.CreateJobHeader();
			job.JH_JobNum = "J001";

			//Create non disbursement charges
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge2", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, transaction2.PK));

			Factory.Save();

			var result = RunScript(job.PK);
			Assert("Should return false as not exist DSB charges", !result);

			//Create disbursement charges which are not balanced for different charge codes
			var surplusGLAccount1 = TestObjectCreator.CreateGLHeader("99991001");
			var shortfallGLAccount1 = TestObjectCreator.CreateGLHeader("99991002");

			var costRevenueGLAccount = TestObjectCreator.CreateGLHeader("39991001");

			Factory.Save();

			var chargeCode1 = TestObjectCreator.DSBChargeCode;
			chargeCode1.AC_AG_CostAccount = costRevenueGLAccount.PK;
			chargeCode1.AC_AG_RevenueAccount = costRevenueGLAccount.PK;
			chargeCode1.AC_AG_DisbursementShortfallAccount = shortfallGLAccount1.PK;
			chargeCode1.AC_AG_DisbursementSurplusAccount = surplusGLAccount1.PK;

			Factory.Save();

			var charge3 = TestObjectCreator.CreateCharge(job, chargeCode1, 100m, 100m);

			var header1 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var line11 = TestObjectCreator.CreateCostLine(charge3, header1.PK);
			line11.AL_AG = charge3.ChargeCode.AC_AG_CostAccount;

			Factory.Save();

			var reloadCharge = Factory.Load<JobCharge>(charge3.PK);
			Assert("Pre-condition: JR_ChargeType should be empty as only cost has been posted for charge", reloadCharge.JR_ChargeType.IsEmpty);

			result = RunScript(job.PK);
			Assert("Should return true as exist DSB charges which are not balanced", result);

			//Create line related to disbursement charges which are balanced for one charge code
			var header3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var line31 = TestObjectCreator.CreateRevenueLine(charge3, header3.PK);
			line31.AL_AG = charge3.ChargeCode.AC_AG_RevenueAccount;

			Factory.Save();

			result = RunScript(job.PK);
			Assert("Should return false as exist DSB charges which are balanced", !result);
		}

		public void TestShouldJobBeClosedByDsbBatchWithReverseTransactionAndDoesNotCreateDsbJobCloseBatch()
		{
			var job = TestObjectCreator.CreateJobHeader();
			job.JH_JobNum = "J001";

			//Create non disbursement charges
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge2", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, transaction2.PK));

			Factory.Save();

			var result = RunScript(job.PK);
			Assert("Should return false as not exist DSB charges", !result);

			//Create disbursement charges which are not balanced for different charge codes
			var surplusGLAccount1 = TestObjectCreator.CreateGLHeader("99991001");
			var shortfallGLAccount1 = TestObjectCreator.CreateGLHeader("99991002");

			var costRevenueGLAccount = TestObjectCreator.CreateGLHeader("39991001");

			Factory.Save();

			var chargeCode1 = TestObjectCreator.DSBChargeCode;
			chargeCode1.AC_AG_CostAccount = costRevenueGLAccount.PK;
			chargeCode1.AC_AG_RevenueAccount = costRevenueGLAccount.PK;
			chargeCode1.AC_AG_DisbursementShortfallAccount = shortfallGLAccount1.PK;
			chargeCode1.AC_AG_DisbursementSurplusAccount = surplusGLAccount1.PK;

			Factory.Save();

			var charge3 = TestObjectCreator.CreateCharge(job, chargeCode1, 100m, 100m);

			var header1 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var line11 = TestObjectCreator.CreateCostLine(charge3, header1.PK);
			line11.AL_AG = charge3.ChargeCode.AC_AG_CostAccount;
			header1.Lines.Add(line11);

			Factory.Save();

			result = RunScript(job.PK);
			Assert("Should return true as exist DSB charges which are not balanced", result);

			//Reverse the posted transaction
			var msg = string.Empty;
			TestObjectCreator.ReverseTransaction(header1, out msg);
			header1.ReverseTransaction.TransactionNumber = "123456";
			Assert("Should no error while doing reverse", string.IsNullOrEmpty(msg));

			Factory.Save();

			result = RunScript(job.PK);
			Assert("Should return false as exist DSB charges which are balanced", !result);
		}

		public void TestShouldJobBeClosedByDsbBatchWithReverseTransactionAndCreateDsbJobCloseBatch()
		{
			var dsbJobCloseBatch = TestObjectCreator.CreateDsbJobCloseBatch("3210");
			var job = TestObjectCreator.CreateJobHeader();
			job.JH_JobNum = "J001";

			//Create non disbursement charges
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge2", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, transaction2.PK));

			Factory.Save();

			var result = RunScript(job.PK);
			Assert("Should return false as not exist DSB charges", !result);

			//Create disbursement charges which are not balanced for different charge codes
			var surplusGLAccount1 = TestObjectCreator.CreateGLHeader("99991001");
			var shortfallGLAccount1 = TestObjectCreator.CreateGLHeader("99991002");

			var costRevenueGLAccount = TestObjectCreator.CreateGLHeader("39991001");

			Factory.Save();

			var chargeCode1 = TestObjectCreator.DSBChargeCode;
			chargeCode1.AC_AG_CostAccount = costRevenueGLAccount.PK;
			chargeCode1.AC_AG_RevenueAccount = costRevenueGLAccount.PK;
			chargeCode1.AC_AG_DisbursementShortfallAccount = shortfallGLAccount1.PK;
			chargeCode1.AC_AG_DisbursementSurplusAccount = surplusGLAccount1.PK;

			Factory.Save();

			var charge3 = TestObjectCreator.CreateCharge(job, chargeCode1, 100m, 100m);

			var header1 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var line11 = TestObjectCreator.CreateCostLine(charge3, header1.PK);
			line11.AL_AG = charge3.ChargeCode.AC_AG_CostAccount;
			line11.AL_JBB = dsbJobCloseBatch.PK;
			header1.Lines.Add(line11);

			Factory.Save();

			result = RunScript(job.PK);
			Assert("Should return false as exist DSB charges which are balanced", !result);

			//Reverse the posted transaction
			var msg = string.Empty;
			TestObjectCreator.ReverseTransaction(header1, out msg);
			header1.ReverseTransaction.TransactionNumber = "654321";
			Assert("Should no error while doing reverse", string.IsNullOrEmpty(msg));

			Factory.Save();

			result = RunScript(job.PK);
			Assert("Should return true as exist DSB charges which are not balanced", result);

			var header2 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var line21 = TestObjectCreator.CreateCostLine(charge3, header2.PK);
			line21.AL_AG = charge3.ChargeCode.AC_AG_CostAccount;

			Factory.Save();

			result = RunScript(job.PK);
			Assert("Should return false as exist DSB charges which are balanced", !result);
		}

		public void TestShouldJobBeClosedByDsbBatchWithChangeNonDSBChargeToDSBCharge()
		{
			var job = TestObjectCreator.CreateJobHeader();
			job.JH_JobNum = "J001";

			//Create non disbursement charges
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction2 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			transaction2.Lines.Add(TestObjectCreator.CreateCostLine(charge2, transaction2.PK));

			Factory.Save();

			var result = RunScript(job.PK);
			Assert("Should return false as not exist DSB charges", !result);

			//Create disbursement charges which are not balanced for different charge codes
			var surplusGLAccount1 = TestObjectCreator.CreateGLHeader("99991001");
			var shortfallGLAccount1 = TestObjectCreator.CreateGLHeader("99991002");

			var costRevenueGLAccount = TestObjectCreator.CreateGLHeader("39991001");

			Factory.Save();

			var chargeCode1 = TestObjectCreator.DSBChargeCode;
			chargeCode1.AC_AG_CostAccount = costRevenueGLAccount.PK;
			chargeCode1.AC_AG_RevenueAccount = costRevenueGLAccount.PK;

			Factory.Save();

			var charge3 = TestObjectCreator.CreateCharge(job, chargeCode1, 100m, 100m);

			var header1 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var line11 = TestObjectCreator.CreateCostLine(charge3, header1.PK);
			line11.AL_AG = charge3.ChargeCode.AC_AG_CostAccount;

			Factory.Save();

			result = RunScript(job.PK);
			Assert("Should return false as didn't exist DSB charges", !result);

			//Change charge type from Non DSB to DSB
			chargeCode1.AC_AG_DisbursementShortfallAccount = shortfallGLAccount1.PK;
			chargeCode1.AC_AG_DisbursementSurplusAccount = surplusGLAccount1.PK;

			Factory.Save();

			result = RunScript(job.PK);
			Assert("Should return true as exist DSB charges which are not balanced", result);

			//Create line related to disbursement charges which are balanced for one charge code
			var header2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var line21 = TestObjectCreator.CreateRevenueLine(charge3, header2.PK);
			line21.AL_AG = charge3.ChargeCode.AC_AG_RevenueAccount;

			Factory.Save();

			result = RunScript(job.PK);
			Assert("Should return false as exist DSB charges which are balanced", !result);

			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			TestObjectCreator.CC1.AC_AG_DisbursementShortfallAccount = shortfallGLAccount1.PK;
			TestObjectCreator.CC1.AC_AG_DisbursementSurplusAccount = surplusGLAccount1.PK;

			Factory.Save();

			result = RunScript(job.PK);
			Assert("Should return false as exist DSB charges which are balanced", !result);

			TestObjectCreator.CC2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			TestObjectCreator.CC2.AC_AG_DisbursementShortfallAccount = shortfallGLAccount1.PK;
			TestObjectCreator.CC2.AC_AG_DisbursementSurplusAccount = surplusGLAccount1.PK;

			Factory.Save();

			result = RunScript(job.PK);
			Assert("Should return true as exist DSB charges which are not balanced", result);
		}

		ZBool RunScript(ZGuid jobPK)
		{
			var sql = string.Format(@"
FROM ShouldJobBeClosedByDsbBatch(
	'{0}'				-- JobPK
)",
jobPK
	);
			return new ZBool(Db.Connection.Exists(sql));
		}
	}
}
