using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(APPayment))]
	public class APPaymentTest : PaymentTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<APPayment>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		#region Concurrency Policy

		[SuspendCriticalValidation]
		public void TestStrictConcurrencyForAH_InvoiceAmount()
		{
			//TODO: Hello, fellow developer! If this test is failing, uncomment the "DISABLE TRIGGER" command below and remove this TODO line
			//HACK: Temporarily allow UPDATE of AH_InvoiceAmount to test concurrency checks. This should not be possible in production. Refer to WI00559931, WI00482153.
			//TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_AccTransactionHeader_ProtectCriticalFieldsFromUpdating ON AccTransactionHeader");

			ConcurrencyTestHelper.AssertStrictConcurrencyForAccTransactionHeader<APPayment>(AccTransactionHeaderSchema.Constants.AH_InvoiceAmount, (ZDecimal)100m, (ZDecimal)90m, (ZDecimal)80m);
		}

		[SuspendCriticalValidation]
		public void TestStrictConcurrencyForAH_OSTotal()
		{
			ConcurrencyTestHelper.AssertStrictConcurrencyForAccTransactionHeader<APPayment>(AccTransactionHeaderSchema.Constants.AH_OSTotal, (ZDecimal)100m, (ZDecimal)90m, (ZDecimal)80m);
		}

		#endregion

		public void TestDocManagerCode()
		{
			AssertEquals(Constants.DocManagerCodes.APPayment, Factory.New<APPayment>().DocManagerInfo.DocManagerCode);
		}

		#region Implementation

		public override string ExceptedWorkflowType => WorkflowDescriptors.APPaymentWorkflowDescriptorCode;

		protected new APPayment ReceiptPaymentBase
		{
			get { return (APPayment)Header; }
		}

		AccHotCheque GetNewHotCheque(OrgHeader org, AccChequeBook chequeBook)
		{
			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			hotCheque.AQ_OH = org.PK;
			hotCheque.AQ_AK = chequeBook.PK;
			return hotCheque;
		}

		void SetupHotChequeTestObjects()
		{
			Org = Factory.NewWithValidTestData<OrgHeader>();
			ChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			fIsNotifyUserUneditableFired = false;
		}

		OrgHeader Org;
		AccChequeBook ChequeBook;

		AccChequeBook GetAutoPrintChequeBook()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		protected Job CreateJob(ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
			OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_JobNum = jobNumber;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = agent.PK;
			job.JH_LocalChargesCFX = localClientCFX;
			job.JH_AgentChargesCFX = agentCFX;
			return job;
		}

		protected Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, ZString desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
																	RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_Desc = desc;

			charge.JR_OH_CostAccount = creditor != null ? creditor.PK : ZGuid.Empty;
			charge.JR_RX_NKCostCurrency = costCurrency.RX_Code;
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_OSSellAmt = oSSellAmt;
			return charge;
		}

		protected void SetAPPaymentInfo(Charge charge, string paymentType, AccBankAccount bankAccount, string chequeOrReference)
		{
			charge.JR_PaymentType = paymentType;
			charge.JR_AB = bankAccount.PK;
			charge.JR_ChequeNo = chequeOrReference;
		}

		protected AccBankAccount CreateBankAccount(string code, string desc, string name, string abbreviation, RefCurrency currency)
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = code;
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_Desc = desc;

			AccGLHeader gLAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_ControlAccount, ZBool.True));
			bankAccount.AB_AG = gLAccount.PK;
			bankAccount.AB_BankName = name;
			bankAccount.AB_BankAbbreviation = abbreviation;
			bankAccount.AB_BSB = "123456";
			bankAccount.AB_AccountNum = "12345678";
			bankAccount.AB_RX_NKAccountCurrency = currency.RX_Code;

			return bankAccount;
		}

		protected AccTaxRate CreateTaxRate(string code, string description, int rate)
		{
			AccTaxRate taxRate = Factory.New<AccTaxRate>();
			taxRate.AT_Code = code;
			taxRate.AT_Description = description;
			taxRate.AT_IsActive = true;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.SetRateNumerator_ForTestOnly(rate);
			return taxRate;
		}

		#region AUD

		protected RefCurrency fAUD;
		protected RefCurrency AUD
		{
			get
			{
				if (fAUD == null)
				{
					fAUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				}
				return fAUD;
			}
		}

		#endregion

		#region ABIGAS

		protected OrgHeader fABIGAS;
		protected OrgHeader ABIGAS
		{
			get
			{
				if (fABIGAS == null)
				{
					fABIGAS = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
				}
				return fABIGAS;
			}
		}

		#endregion

		#region AALSHI

		protected OrgHeader fAALSHI;
		protected OrgHeader AALSHI
		{
			get
			{
				if (fAALSHI == null)
				{
					fAALSHI = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
				}
				return fAALSHI;
			}
		}

		#endregion

		#region ZECTRA

		protected OrgHeader fZECTRA;
		protected OrgHeader ZECTRA
		{
			get
			{
				if (fZECTRA == null)
				{
					fZECTRA = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ZECTRA");
				}
				return fZECTRA;
			}
		}

		#endregion

		protected AccChargeCode MRG100
		{
			get { return TestObjectCreator.MRG100; }
		}

		#region AUDBankAccount

		protected AccBankAccount fAUDBankAccount;
		protected AccBankAccount AUDBankAccount
		{
			get
			{
				if (fAUDBankAccount == null)
				{
					fAUDBankAccount = CreateBankAccount("ZHSBC", "HSBC AUD ACCT", "HSBC", "AUD", AUD);
				}
				return fAUDBankAccount;
			}
		}

		#endregion

		#endregion

		public override void TestDefaultDescription()
		{
			AssertEquals("Default description should be AP PAYMENT", "AP PAYMENT", ReceiptPaymentBase.AH_Desc);
		}

		public override void TestDefaultBankAccount()
		{
			DefaultBankAccountAPTest();
		}

		public override void TestOrgHeaders()
		{
			OrgHeader aPOrg = Factory.NewWithValidTestData<OrgHeader>();
			aPOrg.OH_IsCreditor = true;
			aPOrg.OH_IsDebtor = false;

			OrgHeader aROrg = Factory.NewWithValidTestData<OrgHeader>();
			aROrg.OH_IsCreditor = false;
			aROrg.OH_IsDebtor = true;

			Factory.Save();

			ReceiptPaymentBase.Lookups.Headers.Load();
			Assert("Should contain the APOrg", ReceiptPaymentBase.Lookups.Headers.Contains(aPOrg));
			Assert("Should not contain the AROrg", !ReceiptPaymentBase.Lookups.Headers.Contains(aROrg));
		}

		public override void TestMatchingBaseObject()
		{
			base.TestMatchingBaseObject();
			Assert("Matching Base should be for AP", fMatchingBaseObject is APMatchingBase);
		}

		public void TestReversingClearsPaymentDetailsOnThePaidItems()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			APPayment payment = Factory.NewWithValidTestData<APPayment>();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_AB = Factory.NewWithValidTestData<AccBankAccount>().PK;
			invoice.AH_ChequeOrReference = "12345";

			ChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook.AK_AB = invoice.AH_AB;

			APInvoiceLine line = invoice.Lines.AddNew() as APInvoiceLine;
			line.FillWithValidTestData();
			line.AL_OSAmount = -10m;
			line.AL_LineAmount = -10m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			APInvoiceLine line1 = invoice.Lines.AddNew() as APInvoiceLine;
			line1.FillWithValidTestData();
			line1.AL_OSAmount = -10m;
			line1.AL_LineAmount = -10m;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;

			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			JobConsolCost cost1 = Factory.NewWithValidTestData<JobConsolCost>();

			Charge charge = job.Charges.AddNew();
			charge.FillWithValidTestData();
			charge.JR_E6 = cost.PK;
			charge.JR_OSCostAmt = 10m;
			charge.JR_LocalCostAmt = 10m;
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;
			cost.E6_AH_APInvoice = invoice.PK;
			charge.JR_AL_APLine = line.PK;

			Charge charge1 = job.Charges.AddNew();
			charge1.FillWithValidTestData();
			charge1.JR_E6 = cost1.PK;
			charge1.JR_OSCostAmt = 10m;
			charge1.JR_LocalCostAmt = 10m;
			cost1.E6_OSCostAmount = 10m;
			cost1.E6_LocalCostAmount = 10m;
			cost1.E6_AH_APInvoice = invoice.PK;
			charge1.JR_AL_APLine = line1.PK;

			payment.AH_ReceiptType = cost.E6_PaymentType = charge.JR_PaymentType = ReceiptTypes.Cheque;
			payment.AH_AB = cost.E6_AB_BankAccount = charge.JR_AB = invoice.AH_AB;
			payment.ChequeBook = cost.E6_AK_ChequeBook = charge.JR_AK = ChequeBook.PK;
			payment.AH_ChequeOrReference = cost.E6_ChequeOrReference = charge.JR_ChequeNo = invoice.AH_ChequeOrReference;

			cost1.E6_PaymentType = charge1.JR_PaymentType = ReceiptTypes.Cash;
			cost1.E6_AB_BankAccount = charge1.JR_AB = invoice.AH_AB;
			cost1.E6_AK_ChequeBook = charge1.JR_AK = ChequeBook.PK;
			cost1.E6_ChequeOrReference = charge1.JR_ChequeNo = "6665554";

			TransactionMatchLink matchLink1 = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			matchLink1.AP_AH = invoice.PK;
			matchLink1.AP_MatchGroupNum = "98765";

			TransactionMatchLink matchLink2 = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			matchLink2.AP_AH = payment.PK;
			matchLink2.AP_MatchGroupNum = matchLink1.AP_MatchGroupNum;
			TestObjectCreator.SetupMatchLinkMatchDate(invoice);

			Factory.Save();

			PaymentReversing reversing = new PaymentReversing(payment);
			reversing.Reverse();
			AssertPaymentDetailsCleared(charge, true);
			AssertPaymentDetailsCleared(charge1, false);
		}

		void AssertPaymentDetailsCleared(Charge charge, bool shouldBeCleared)
		{
			AssertEquals("JR_PaymentType", shouldBeCleared, charge.JR_PaymentType.IsEmpty);
			AssertEquals("E6_PaymentType", shouldBeCleared, charge.ParentConsolCost.E6_PaymentType.IsEmpty);
			AssertEquals("JR_AB", shouldBeCleared, charge.JR_AB.IsEmpty);
			AssertEquals("E6_AB_BankAccount", shouldBeCleared, charge.ParentConsolCost.E6_AB_BankAccount.IsEmpty);
			AssertEquals("JR_AK", shouldBeCleared, charge.JR_AK.IsEmpty);
			AssertEquals("E6_AK_ChequeBook", shouldBeCleared, charge.ParentConsolCost.E6_AK_ChequeBook.IsEmpty);
			AssertEquals("JR_ChequeNo", shouldBeCleared, charge.JR_ChequeNo.IsEmpty);
			AssertEquals("E6_ChequeOrReference", shouldBeCleared, charge.ParentConsolCost.E6_ChequeOrReference.IsEmpty);
		}

		public void TestUnMatchingClearsPaymentDetailsOnThePaidItems()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			APPayment payment = Factory.NewWithValidTestData<APPayment>();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_AB = Factory.NewWithValidTestData<AccBankAccount>().PK;
			invoice.AH_ChequeOrReference = "12345";

			ChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook.AK_AB = invoice.AH_AB;

			APInvoiceLine line = invoice.Lines.AddNew() as APInvoiceLine;
			line.FillWithValidTestData();
			line.AL_OSAmount = -10m;
			line.AL_LineAmount = -10m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			APInvoiceLine line1 = invoice.Lines.AddNew() as APInvoiceLine;
			line1.FillWithValidTestData();
			line1.AL_OSAmount = -10m;
			line1.AL_LineAmount = -10m;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;

			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			JobConsolCost cost1 = Factory.NewWithValidTestData<JobConsolCost>();

			Charge charge = job.Charges.AddNew();
			charge.FillWithValidTestData();
			charge.JR_E6 = cost.PK;
			charge.JR_OSCostAmt = 10m;
			charge.JR_LocalCostAmt = 10m;
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;
			cost.E6_AH_APInvoice = invoice.PK;
			charge.JR_AL_APLine = line.PK;

			Charge charge1 = job.Charges.AddNew();
			charge1.FillWithValidTestData();
			charge1.JR_E6 = cost1.PK;
			charge1.JR_OSCostAmt = 10m;
			charge1.JR_LocalCostAmt = 10m;
			cost1.E6_OSCostAmount = 10m;
			cost1.E6_LocalCostAmount = 10m;
			cost1.E6_AH_APInvoice = invoice.PK;
			charge1.JR_AL_APLine = line1.PK;

			payment.AH_ReceiptType = cost.E6_PaymentType = charge.JR_PaymentType = ReceiptTypes.Cheque;
			payment.AH_AB = cost.E6_AB_BankAccount = charge.JR_AB = invoice.AH_AB;
			payment.ChequeBook = cost.E6_AK_ChequeBook = charge.JR_AK = ChequeBook.PK;
			payment.AH_ChequeOrReference = cost.E6_ChequeOrReference = charge.JR_ChequeNo = invoice.AH_ChequeOrReference;

			cost1.E6_PaymentType = charge1.JR_PaymentType = ReceiptTypes.Cash;
			cost1.E6_AB_BankAccount = charge1.JR_AB = invoice.AH_AB;
			cost1.E6_AK_ChequeBook = charge1.JR_AK = ChequeBook.PK;
			cost1.E6_ChequeOrReference = charge1.JR_ChequeNo = "6665554";

			TransactionMatchLink matchLink1 = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			matchLink1.AP_AH = invoice.PK;
			matchLink1.AP_MatchGroupNum = "98765";

			TransactionMatchLink matchLink2 = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			matchLink2.AP_AH = payment.PK;
			matchLink2.AP_MatchGroupNum = matchLink1.AP_MatchGroupNum;
			TestObjectCreator.SetupMatchLinkMatchDate(invoice);

			Factory.Save();

			((IMatching)payment).Unmatch(payment.AH_LocalTotalAmount, payment.AH_OSTotalAmount);
			AssertPaymentDetailsCleared(charge, true);
			AssertPaymentDetailsCleared(charge1, false);
		}

		public void TestUnMatchingBatch_2lines()
		{
			AssertUnMatchingBatch(2, 1);
		}
		public void TestUnMatchingBatch_5lines()
		{
			AssertUnMatchingBatch(5, 1);
		}
		public void TestUnMatchingBatch_6lines()
		{
			AssertUnMatchingBatch(6, 2);
		}
		public void TestUnMatchingBatch_10lines()
		{
			AssertUnMatchingBatch(10, 2);
		}
		public void TestUnMatchingBatch_11lines()
		{
			AssertUnMatchingBatch(11, 3);
		}

		public void AssertUnMatchingBatch(int numberOfLines, int expectedTableHits)
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			APPayment payment = Factory.NewWithValidTestData<APPayment>();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_AB = Factory.NewWithValidTestData<AccBankAccount>().PK;
			invoice.AH_ChequeOrReference = "12345";

			ChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook.AK_AB = invoice.AH_AB;

			List<Charge> charges = new List<Charge>();

			for (int iteration = 0; iteration < numberOfLines; iteration++)
			{
				APInvoiceLine line = invoice.Lines.AddNew() as APInvoiceLine;
				line.FillWithValidTestData();
				line.AL_OSAmount = -10m;
				line.AL_LineAmount = -10m;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;

				JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();

				Charge charge = job.Charges.AddNew();
				charge.FillWithValidTestData();
				charge.JR_E6 = cost.PK;
				charge.JR_OSCostAmt = 10m;
				charge.JR_LocalCostAmt = 10m;
				cost.E6_OSCostAmount = 10m;
				cost.E6_LocalCostAmount = 10m;
				cost.E6_AH_APInvoice = invoice.PK;
				charge.JR_AL_APLine = line.PK;

				if (iteration == 0)
				{
					payment.AH_ReceiptType = cost.E6_PaymentType = charge.JR_PaymentType = ReceiptTypes.Cheque;
					payment.AH_AB = cost.E6_AB_BankAccount = charge.JR_AB = invoice.AH_AB;
					payment.ChequeBook = cost.E6_AK_ChequeBook = charge.JR_AK = ChequeBook.PK;
					payment.AH_ChequeOrReference = cost.E6_ChequeOrReference = charge.JR_ChequeNo = invoice.AH_ChequeOrReference;
				}

				charges.Add(charge);
			}

			TransactionMatchLink matchLink1 = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			matchLink1.AP_AH = invoice.PK;
			matchLink1.AP_MatchGroupNum = "98765";

			TransactionMatchLink matchLink2 = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			matchLink2.AP_AH = payment.PK;
			matchLink2.AP_MatchGroupNum = matchLink1.AP_MatchGroupNum;
			TestObjectCreator.SetupMatchLinkMatchDate(invoice);

			Factory.Save();

			int preTableHitsCount = Factory.GetTableHitCount(JobChargeSchema.Constants.TableName);
			((IMatching)payment).Unmatch(payment.AH_LocalTotalAmount, 0m);
			int postTableHitsCount = Factory.GetTableHitCount(JobChargeSchema.Constants.TableName);
			foreach (var charge in charges)
			{
				AssertPaymentDetailsCleared(charge, true);
			}
			AssertEquals(string.Format("Batch size={0}: processing {1} lines and expecting {2} hits(s)", AccountingUtils.ChunkBatchSize, numberOfLines, expectedTableHits), expectedTableHits, postTableHitsCount - preTableHitsCount);
		}

		public void TestOnSaving_AP()
		{
			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			ReceiptPaymentBase.FImportedHotCheque_ForTestOnly = hotCheque;
			Factory.Save();
			AssertEquals("HotCheque should reference the payment", hotCheque.AQ_AH, ReceiptPaymentBase.PK);
		}

		public override void TestIChequeNumberAutoAllocation_AssignChequeNumber()
		{
			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook();
			autoPrintChequeBook.AK_LastNo = 5;
			autoPrintChequeBook.AK_CurrentNo = 3;

			Job job = CreateJob("Z00001000", ABIGAS, true, 10, ZECTRA, true, 10);

			Charge charge = CreateCharge(job, MRG100, "Test Charge", AUD, 200, AALSHI, AUD, 300, ABIGAS);
			charge.JR_AK = autoPrintChequeBook.PK;
			charge.JR_PaymentType = ReceiptTypes.Cheque;

			Charge charge2 = CreateCharge(job, MRG100, "Test Charge2", AUD, 200, AALSHI, AUD, 300, ABIGAS);
			charge2.JR_AK = autoPrintChequeBook.PK;
			charge2.JR_PaymentType = ReceiptTypes.Cheque;

			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			JobConsolCost cost2 = Factory.NewWithValidTestData<JobConsolCost>();
			cost.E6_OH_Creditor = AALSHI.PK;
			cost2.E6_OH_Creditor = AALSHI.PK;
			cost.E6_AT_TaxRate = charge.JR_AT_CostGSTRate;
			cost2.E6_AT_TaxRate = charge2.JR_AT_CostGSTRate;
			charge.JR_E6 = cost.PK;
			charge2.JR_E6 = cost2.PK;
			cost.E6_OSCostAmount = 200;
			cost2.E6_OSCostAmount = 200;
			cost.E6_LocalCostAmount = 200;
			cost2.E6_LocalCostAmount = 200;

			Factory.Save();

			APPayment payment = Factory.New<APPayment>();
			payment.SetValues_ForTestOnly(job, charge, ZDateTime.Now);
			payment.SetAmounts_ForTestsOnly(charge2);
			payment.AH_OutstandingAmount = 220m;
			payment.AH_FullyPaidDate = ZDateTime.Empty;

			//PaymentApproval is not null
			APPaymentApprovalWithoutAuthorisation testPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			testPaymentApproval.AV_AH = payment.PK;
			Assert("AV_ChequeOrReference should be empty on payment approval", testPaymentApproval.AV_ChequeOrReference.IsEmpty);
			((IChequeNumberAutoAllocation)payment).AssignChequeNumber("3");
			charge.Reload();
			charge2.Reload();
			AssertEquals("AH_ChequeOrReference should be set", "3", payment.AH_ChequeOrReference);
			AssertEquals("AH_ChequeOrReference should be set on the payment", "3", payment.AH_ChequeOrReference);
			Assert("Cheque number should not be assigned to Charge", charge.JR_ChequeNo.IsEmpty);
			Assert("Cheque number should not be assigned to Charge2", charge2.JR_ChequeNo.IsEmpty);
			//Assert("Cheque number should not be assigned to related Consol Cost", Cost.E6_ChequeOrReference.IsEmpty);
			//Assert("Cheque number should not be assigned to related Consol Cost", Cost2.E6_ChequeOrReference.IsEmpty);

			autoPrintChequeBook.AK_IsActive = ZBool.False;
			((IChequeNumberAutoAllocation)payment).AllocationOrPrintingFailed();
			Assert("Cheque book should be reloaded", payment.ChequeBookBizO.AK_IsActive);
			Assert("AH_ChequeOrReference should be reset", payment.AH_ChequeOrReference.IsEmpty);
			Assert("AV_ChequeOrReference should be reset on payment approval", testPaymentApproval.AV_ChequeOrReference.IsEmpty);

			//PaymentApproval is null
			payment.ResetRelatedPaymentApproval_ForTestOnly();
			testPaymentApproval.AV_AH = ZGuid.Empty;
			((IChequeNumberAutoAllocation)payment).AssignChequeNumber("4");
			Factory.Save();
			charge.Reload();
			charge2.Reload();
			AssertEquals("Cheque number should be assigned to Payment", "4", payment.AH_ChequeOrReference);
			AssertEquals("Cheque number should be assigned to Charge", "4", charge.JR_ChequeNo);
			AssertEquals("Cheque number should be assigned to Charge2", "4", charge2.JR_ChequeNo);
			AssertEquals("Cheque number should be assigned to related Consol Cost", "4", cost.E6_ChequeOrReference);
			AssertEquals("Cheque number should be assigned to related Consol Cost", "4", cost2.E6_ChequeOrReference);

			autoPrintChequeBook.AK_IsActive = ZBool.False;
			((IChequeNumberAutoAllocation)payment).AllocationOrPrintingFailed();
			Factory.Save();
			Assert("Cheque book should be reloaded", payment.ChequeBookBizO.AK_IsActive);
			Assert("AH_ChequeOrReference should be reset", payment.AH_ChequeOrReference.IsEmpty);
			Assert("Cheque number should be reset on Charge", charge.JR_ChequeNo.IsEmpty);
			Assert("Cheque number should be reset on Charge2", charge2.JR_ChequeNo.IsEmpty);
			Assert("Cheque number should be reset on Consol Cost", cost.E6_ChequeOrReference.IsEmpty);
			Assert("Cheque number should be reset on Charge related Consol Cost", cost2.E6_ChequeOrReference.IsEmpty);
		}

		public void TestJobConsolCostsAreNotChangedWhenChareDoesNotHaveRelatedJobConsolCost()
		{
			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook();
			autoPrintChequeBook.AK_LastNo = 5;
			autoPrintChequeBook.AK_CurrentNo = 3;

			Job job = CreateJob("Z00001000", ABIGAS, true, 10, ZECTRA, true, 10);

			Charge charge = CreateCharge(job, MRG100, "Test Charge", AUD, 200, AALSHI, AUD, 300, ABIGAS);
			charge.JR_AK = autoPrintChequeBook.PK;
			charge.JR_PaymentType = ReceiptTypes.Cheque;

			Charge charge2 = CreateCharge(job, MRG100, "Test Charge2", AUD, 200, AALSHI, AUD, 300, ABIGAS);
			charge2.JR_AK = autoPrintChequeBook.PK;
			charge2.JR_PaymentType = ReceiptTypes.Cheque;

			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			JobConsolCost cost2 = Factory.NewWithValidTestData<JobConsolCost>();

			Factory.Save();

			APPayment payment = Factory.New<APPayment>();
			payment.SetValues_ForTestOnly(job, charge, ZDateTime.Now);
			payment.SetAmounts_ForTestsOnly(charge2);
			payment.AH_OutstandingAmount = 220m;
			payment.AH_FullyPaidDate = ZDateTime.Empty;

			((IChequeNumberAutoAllocation)payment).AssignChequeNumber("3");
			Factory.Save();

			charge.Reload();
			charge2.Reload();
			AssertEquals("Cheque number should be assigned to Payment", "3", payment.AH_ChequeOrReference);
			AssertEquals("Cheque number should be assigned to Charge", "3", charge.JR_ChequeNo);
			AssertEquals("Cheque number should be assigned to Charge2", "3", charge2.JR_ChequeNo);
			Assert("JobConsolCost should not be changed as it is not related to the charge", cost.E6_ChequeOrReference.IsEmpty);
			Assert("JobConsolCost should not be changed as it is not related to the charge", cost2.E6_ChequeOrReference.IsEmpty);
		}

		#region Hot Cheques

		public void TestOutstandingHotChequesShownWhenOrgSet()
		{
			SetupHotChequeTestObjects();
			Org.OH_IsCreditor = true;
			ChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			hotCheque.AQ_AH = ZGuid.Empty;
			hotCheque.AQ_Cancelled = false;

			ReceiptPaymentBase.DisplayHotCheques += new APPayment.HotChequeSelectedHandler(IsDisplayHotChequeFired);
			fIsDisplayHotChequeFired = false;
			ReceiptPaymentBase.AH_OH = Org.PK;
			Assert("DisplayHotCheque should have been fired", fIsDisplayHotChequeFired);

			ReceiptPaymentBase.AH_OH = ZGuid.Empty;
			hotCheque.AQ_AH = Factory.New(typeof(APPayment)).PK;  // make Hot Cheque InActive
			fIsDisplayHotChequeFired = false;
			ReceiptPaymentBase.AH_OH = Org.PK;
			Assert("DisplayHotCheque should not have been fired", !fIsDisplayHotChequeFired);

			ReceiptPaymentBase.AH_OH = ZGuid.Empty;
			hotCheque.AQ_AH = ZGuid.Empty;
			hotCheque.AQ_Cancelled = true;  // make Hot Cheque cancelled
			fIsDisplayHotChequeFired = false;
			ReceiptPaymentBase.AH_OH = Org.PK;
			Assert("DisplayHotCheque should not have been fired", !fIsDisplayHotChequeFired);

			ReceiptPaymentBase.AH_OH = ZGuid.Empty;
			hotCheque.AQ_OH = Factory.New(typeof(OrgHeader)).PK;  // change the Hot Cheque organisation
			hotCheque.AQ_Cancelled = false;
			fIsDisplayHotChequeFired = false;
			ReceiptPaymentBase.AH_OH = Org.PK;
			Assert("DisplayHotCheque should not have been fired", !fIsDisplayHotChequeFired);
		}

		public void TestGetActiveHotCheques()
		{
			SetupHotChequeTestObjects();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			ChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;

			AccHotCheque hotCheque_Posted = GetNewHotCheque(Org, ChequeBook);
			hotCheque_Posted.AQ_Cancelled = false;
			hotCheque_Posted.AQ_AH = Factory.New(typeof(APPayment)).PK;

			AccHotCheque hotCheque_Cancelled = GetNewHotCheque(Org, ChequeBook);
			hotCheque_Cancelled.AQ_Cancelled = true;
			hotCheque_Cancelled.AQ_AH = ZGuid.Empty;

			AccHotCheque hotCheque_DiffOrg = GetNewHotCheque(org2, ChequeBook);
			hotCheque_DiffOrg.AQ_Cancelled = false;
			hotCheque_DiffOrg.AQ_AH = ZGuid.Empty;

			ReceiptPaymentBase.AH_OH = Org.PK;
			AssertEquals("The test Org should not have active hot cheques", 0, ReceiptPaymentBase.GetActiveHotCheques_ForTestOnly().Count);

			AccHotCheque hotCheque_Active = GetNewHotCheque(Org, ChequeBook);
			hotCheque_Active.AQ_Cancelled = false;
			hotCheque_Active.AQ_AH = ZGuid.Empty;

			AssertEquals("The test Org should have 1 active hot cheque", 1, ReceiptPaymentBase.GetActiveHotCheques_ForTestOnly().Count);
		}

		public void TestPopulateFieldsUsingHotCheque()
		{
			SetupHotChequeTestObjects();
			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			ChequeBook.AK_AB = bankAccount.PK;
			hotCheque.AQ_AK = ChequeBook.PK;
			hotCheque.AQ_ChequeNumber = "129990";
			hotCheque.AQ_Amount = 90.88m;

			// Reset values
			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cash;
			ReceiptPaymentBase.AH_AB = ZGuid.Empty;
			ReceiptPaymentBase.ChequeBook = ZGuid.Empty;
			ReceiptPaymentBase.AH_ChequeOrReference = ZString.Empty;
			ReceiptPaymentBase.AH_OSExTaxAmount = 0m;

			ReceiptPaymentBase.PopulateFieldsUsingHotCheque_ForTestOnly(hotCheque);
			AssertEquals("Payment type should be cheque", ReceiptTypes.Cheque, ReceiptPaymentBase.AH_ReceiptType);
			AssertEquals("Bank account should be the test BankAccount", bankAccount.PK, ReceiptPaymentBase.AH_AB);
			AssertEquals("Chequebook should be the test ChequeBook", ChequeBook.PK, ReceiptPaymentBase.ChequeBook);
			AssertEquals("Cheque number should be 129990", "129990", ReceiptPaymentBase.AH_ChequeOrReference);
			AssertEquals("OSExTax amount should be 90.88", 90.88m, ReceiptPaymentBase.AH_OSExTaxAmount);
		}

		public void TestSetHotChequeInactiveAfterPosting()
		{
			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			ReceiptPaymentBase.SetHotChequeInactiveWhenPosting(hotCheque);
			ReceiptPaymentBase.Factory.Save();
			AssertEquals("AQ_AH should reference the payment", ReceiptPaymentBase.PK, hotCheque.AQ_AH);
		}

		public void TestValidationForHotChequeImport()
		{
			SetupHotChequeTestObjects();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount bank_Diff = Factory.NewWithValidTestData<AccBankAccount>();
			ChequeBook.AK_StartNo = 1;
			ChequeBook.AK_LastNo = 100;
			AccChequeBook chequeBook_Diff = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook.AK_AB = bank.PK;
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			hotCheque.AQ_ChequeNumber = "000023";
			hotCheque.AQ_Amount = 684.33m;
			hotCheque.AQ_ActualOrMaxIndicator = ActualOrMaxIndicator.Actual;

			ReceiptPaymentBase.ImportSelectedHotCheque(hotCheque);
			AssertNoErrors(ReceiptPaymentBase.AH_ReceiptTypeInfo);
			AssertNoErrors(ReceiptPaymentBase.AH_ABInfo);
			AssertNoErrors(ReceiptPaymentBase.ChequeBookInfo);
			AssertNoErrors(ReceiptPaymentBase.AH_ChequeOrReferenceInfo);
			AssertNoErrors(ReceiptPaymentBase.AH_OSExTaxAmountInfo);
		}

		public void TestSettingAH_AB()
		{
			SetupHotChequeTestObjects();
			ReceiptPaymentBase.NotifyUserPaymentUneditable += new APPayment.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			AccBankAccount bank = Factory.New<AccBankAccount>();
			AccBankAccount bank_Diff = Factory.New<AccBankAccount>();
			ChequeBook.AK_AB = bank.PK;
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);

			ReceiptPaymentBase.ImportSelectedHotCheque(hotCheque);
			AssertEquals("Bank should be imported", bank.PK, ReceiptPaymentBase.AH_AB);
			Assert("Precondition: NotifyUserUneditable not previously fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.AH_AB = bank.PK;
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.AH_AB = bank_Diff.PK;
			AssertEquals("Payment Bank should be unchanged", bank.PK, ReceiptPaymentBase.AH_AB);
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
		}

		public void TestSettingAH_ChequeOrReference()
		{
			SetupHotChequeTestObjects();
			ReceiptPaymentBase.NotifyUserPaymentUneditable += new APPayment.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			AccBankAccount bank = Factory.New<AccBankAccount>();
			ChequeBook.AK_StartNo = 1;
			ChequeBook.AK_LastNo = 100;
			ChequeBook.AK_AB = bank.PK;
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			hotCheque.AQ_ChequeNumber = "000056";

			ReceiptPaymentBase.ImportSelectedHotCheque(hotCheque);
			AssertEquals("ReferenceNumber should be imported", "000056", ReceiptPaymentBase.AH_ChequeOrReference);
			Assert("Precondition: NotifyUserUneditable not previously fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.AH_ChequeOrReference = "000056";
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.AH_ChequeOrReference = "000074";
			AssertEquals("ReferenceNumber should be unchanged", "000056", ReceiptPaymentBase.AH_ChequeOrReference);
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
		}

		public void TestSettingChequeBook()
		{
			SetupHotChequeTestObjects();
			ReceiptPaymentBase.NotifyUserPaymentUneditable += new APPayment.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);

			ReceiptPaymentBase.ImportSelectedHotCheque(hotCheque);
			AssertEquals("ChequeBook should be imported", ChequeBook.PK, ReceiptPaymentBase.ChequeBook);
			Assert("Precondition: NotifyUserUneditable not previously fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.ChequeBook = ChequeBook.PK;
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.ChequeBook = Factory.New(typeof(AccChequeBook)).PK;
			AssertEquals("ChequeBook should be unchanged", ChequeBook.PK, ReceiptPaymentBase.ChequeBook);
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
		}

		public void TestSettingMaximumAH_OSExTaxAmount()
		{
			SetupHotChequeTestObjects();
			ReceiptPaymentBase.NotifyUserPaymentUneditable += new APPayment.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			hotCheque.AQ_ActualOrMaxIndicator = ActualOrMaxIndicator.Max;
			hotCheque.AQ_Amount = 56m;

			ReceiptPaymentBase.ImportSelectedHotCheque(hotCheque);
			AssertEquals("OSExTaxAmount should be imported", 56m, ReceiptPaymentBase.AH_OSExTaxAmount);
			Assert("Precondition: NotifyUserUneditable not previously fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.AH_OSExTaxAmount = 56m;
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.AH_OSExTaxAmount = 45m;
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			AssertEquals("AH_OSExTaxAmount should be set", 45m, ReceiptPaymentBase.AH_OSExTaxAmount);
			ReceiptPaymentBase.AH_OSExTaxAmount = 67m;
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
			AssertEquals("AH_OSExTaxAmount should be reset to previous value", 45m, ReceiptPaymentBase.AH_OSExTaxAmount);
		}

		public void TestSettingActualAH_OSExTaxAmount()
		{
			SetupHotChequeTestObjects();
			ReceiptPaymentBase.NotifyUserPaymentUneditable += new APPayment.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			hotCheque.AQ_ActualOrMaxIndicator = ActualOrMaxIndicator.Actual;
			hotCheque.AQ_Amount = 56m;

			ReceiptPaymentBase.ImportSelectedHotCheque(hotCheque);
			AssertEquals("OSExTaxAmount should be imported", 56m, ReceiptPaymentBase.AH_OSExTaxAmount);
			Assert("Precondition: NotifyUserUneditable not previously fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.AH_OSExTaxAmount = 56m;
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.AH_OSExTaxAmount = 45m;
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
			AssertEquals("AH_OSExTaxAmount should still be 56", 56m, ReceiptPaymentBase.AH_OSExTaxAmount);
		}

		public void TestSettingAH_ReceiptType()
		{
			SetupHotChequeTestObjects();
			ReceiptPaymentBase.NotifyUserPaymentUneditable += new APPayment.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cash;

			ReceiptPaymentBase.ImportSelectedHotCheque(hotCheque);
			AssertEquals("ReceiptType should be Cheque", ReceiptTypes.Cheque, ReceiptPaymentBase.AH_ReceiptType);
			Assert("Precondition: NotifyUserUneditable not previously fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cheque;
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.CreditCard;
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
			AssertEquals("ReceiptType should be unchanged", ReceiptTypes.Cheque, ReceiptPaymentBase.AH_ReceiptType);
		}

		public void TestIsImportingHotCheque()
		{
			ReceiptPaymentBase.NotifyUserPaymentUneditable += new APPayment.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			ReceiptPaymentBase.BeginImportingHotCheque_ForTestOnly();
			ReceiptPaymentBase.FireNotifyUserPaymentUneditable_ForTestOnly("Test");
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			ReceiptPaymentBase.FinishImportingHotCheque_ForTestOnly();
			ReceiptPaymentBase.FireNotifyUserPaymentUneditable_ForTestOnly("Test");
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
		}

		void IsNotifyUserUneditableFired(object sender, string message)
		{
			fIsNotifyUserUneditableFired = true;
		}

		bool fIsNotifyUserUneditableFired;

		void IsDisplayHotChequeFired(object sender, HotChequeLink link)
		{
			fIsDisplayHotChequeFired = true;
		}

		bool fIsDisplayHotChequeFired;

		#endregion
	}
}
