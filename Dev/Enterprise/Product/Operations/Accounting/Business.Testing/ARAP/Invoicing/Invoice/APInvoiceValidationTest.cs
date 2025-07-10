using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class APInvoiceValidationTest : InvoiceValidationTest
	{
		protected override InvoiceBaseValidation GetValidation(TransactionHeader parent)
		{
			return new APInvoiceValidation((APInvoice)parent);
		}

		protected override Type InvoiceType
		{
			get { return typeof(APInvoice); }
		}

		public void TestErrorOnCheckBookBranchIfDifferent()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			APInvoice invoice1 = creator.CreateAPInvoice<APInvoice>("100", creator.AUD, 1.0M, 100, 100, 100, 100, 100, 100);
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			AccChequeBook currentBook = creator.CreateChequeBook("Hello", 2, creator.AUDBankAccount);
			AccChequeBook otherBook = creator.CreateChequeBook("Hello", 2, creator.AUDBankAccount);

			otherBook.AK_GB = creator.NonCurrentBranch.PK;
			invoice1.ReceiptPaymentAK_AB = otherBook.PK;
			((APInvoiceValidation)invoice1.Validation).ValidateReceiptPaymentAK_AB();
			AssertHasError(invoice1.ReceiptPaymentAK_ABInfo, "You cannot select a check book that is different to the invoice branch (BNE)");

			currentBook.AK_GB = GlbBranch.CurrentBranch.PK;
			invoice1.ReceiptPaymentAK_AB = currentBook.PK;
			((APInvoiceValidation)invoice1.Validation).ValidateReceiptPaymentAK_AB();
			AssertNoError(invoice1.ReceiptPaymentAK_ABInfo, "You cannot select a check book that is different to the invoice branch (BNE)");
		}

		public void TestRequisitionStatusValidation()
		{
			SetupPaymentRequisitionStatuses();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_RequisitionStatus = "BBB";
			AssertNoErrors(invoice.AH_RequisitionStatusInfo);

			Env.Security.AllowAPInvoiceDefaultRequisitionDetailsOverride.IsAllowed = false;
			invoice.AH_RequisitionStatus = "AAA";
			AssertHasError(invoice.AH_RequisitionStatusInfo, "You have not been granted security rights to change this transaction’s Payment Requisition Criticality Status. Please set this code to BBB.");
		}

		public void TestRequisitionStatusValidation_WithInvoiceStoredInDB()
		{
			SetupPaymentRequisitionStatuses();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_RequisitionStatus = "BBB";
			AssertNoErrors(invoice.AH_RequisitionStatusInfo);

			Factory.Save();

			var unapprovedCollection = new UnapprovedTransactionCandidateCollection(Factory);
			unapprovedCollection.Add(invoice);
			Assert("Precondition: invoice should have correct validation.", invoice.Validation.GetType().IsSubclassOf(typeof(APInvoiceValidation)));

			Env.Security.AllowAPInvoiceDefaultRequisitionDetailsOverride.IsAllowed = false;
			invoice.AH_RequisitionStatus = "AAA";
			AssertHasError(invoice.AH_RequisitionStatusInfo, "You have not been granted security rights to change this transaction’s Payment Requisition Criticality Status. Please set this code to BBB.");
		}

		static void SetupPaymentRequisitionStatuses()
		{
			var aaa = new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = "AAA" };
			var bbb = new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = "BBB" };
			var paymentRequisitionStatuses = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { aaa, bbb };
			paymentRequisitionStatuses.SetDefaultCode("BBB", true);
			AccountingConfigurationRegistry.Instance.PaymentRequisitionStatuses.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, paymentRequisitionStatuses);
		}

		public void TestRequisitionDateValidation()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			AssertNoErrors(invoice.AH_RequisitionDateInfo);

			Env.Security.AllowAPInvoiceDefaultRequisitionDetailsOverride.IsAllowed = false;
			invoice.AH_RequisitionDate = ZDateTime.Today.AddDays(-2);
			AssertHasError(invoice.AH_RequisitionDateInfo, string.Format("By default the Payment Requested Date is a Transaction’s Due Date. You have not been granted security rights to change this date. Please set this to {0}.", ZDateTime.Today.ToShortDateString()));

			invoice.AH_DueDate = ZDateTime.Today.AddDays(-3);
			AssertNoErrors(invoice.AH_RequisitionDateInfo);
		}

		public void TestCheckAH_OSTotalAmount()
		{
			AccHotCheque hotCheque = Factory.New<AccHotCheque>();
			hotCheque.AQ_ActualOrMaxIndicator = ActualOrMaxIndicator.Actual;
			hotCheque.AQ_Amount = 200m;

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.ImportSelectedHotCheque(hotCheque);
			APInvoiceLine invoiceLine = invoice.Lines.AddNew() as APInvoiceLine;
			invoiceLine.AL_OSExTaxAmount = 100m;
			AssertHasError("Error expected", invoice.AH_OSTotalAmountInfo, "Invoice amount must be the same as the hot check amount");

			hotCheque.AQ_ActualOrMaxIndicator = ActualOrMaxIndicator.Max;
			invoiceLine.AL_OSExTaxAmount = 300m;
			AssertHasError("Error expected", invoice.AH_OSTotalAmountInfo, "Invoice amount must be less than or equal to the hot check amount");

			invoice.ClearImportedHotCheque();
			AssertEquals("HasErrors", false, invoice.AH_OSTotalAmountInfo.HasErrors());
		}

		public void TestCheckAH_OSTotalAmount_TotalCostVariance()
		{
			GlbBranch sYD = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			GlbDepartment fEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			AccChargeCode cAF = TestObjectCreator.CC1;

			AssertNotNull(sYD);
			AssertNotNull(fEA);

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Charge charge = TestObjectCreator.CreateCharge(job, cAF, "",
				TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = sYD.PK;
			charge.JR_GE = fEA.PK;

			Job job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001002"));
			Charge charge2 = TestObjectCreator.CreateCharge(job2, cAF, "",
				TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge2.JR_GB = sYD.PK;
			charge2.JR_GE = fEA.PK;

			Factory.Save();

			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Enterprise.Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Enterprise.Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			upTo1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo1.Amount = 100M;
			upTo1.MonitorTotalInvoiceVariance = true;
			upTo1.TotalInvoiceVarianceAmount = 150M;

			CostVarianceApprovalAuthorisationRequirement upTo2 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			upTo2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo2.Amount = 200M;
			upTo2.MonitorTotalInvoiceVariance = true;
			upTo2.TotalInvoiceVarianceAmount = 300M;

			CostVarianceApprovalAuthorisationRequirement above = valuesForTest.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			above.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			above.Amount = upTo2.Amount;
			above.MonitorTotalInvoiceVariance = true;
			above.TotalInvoiceVarianceAmount = 300M;

			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = cAF.PK;
			line.AL_GB = sYD.PK;
			line.AL_GE = fEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;

			APInvoiceLine line2 = (APInvoiceLine)invoice.Lines.AddNew();
			line2.AL_JH = job2.PK;
			line2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line2.AL_AC = cAF.PK;
			line2.AL_GB = sYD.PK;
			line2.AL_GE = fEA.PK;
			line2.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line2.AL_ExchangeRate = 1M;

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			line.AL_OSExTaxAmount = 150M;
			line2.AL_OSExTaxAmount = 150M;
			AssertNoWarnings(invoice.AH_OSTotalAmountInfo);

			line.AL_OSExTaxAmount = 190M;
			line2.AL_OSExTaxAmount = 190M;
			string expectedWarning = "This Invoice will be automatically approved when you post because you already have the necessary authorization security right";
			AssertHasWarning(invoice.AH_OSTotalAmountInfo, expectedWarning);

			line.AL_OSExTaxAmount = 290M;
			line2.AL_OSExTaxAmount = 290M;
			AssertHasWarning(invoice.AH_OSTotalAmountInfo, "This Invoice requires approval on posting because it exceeds the registry defined total accrual variance threshold.");

			AssertEquals("Data should be received once and cached.", 1, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);

			var chargeInNewFactory = new BusinessObjectFactory().Load<Charge>(charge.PK);
			chargeInNewFactory.JR_OSCostAmt = 200M;
			chargeInNewFactory.Factory.Save();

			line.AL_OSExTaxAmount = 190M;
			line2.AL_OSExTaxAmount = 190M;
			AssertHasWarning("Old data from cache should be used.", invoice.AH_OSTotalAmountInfo, expectedWarning);
			AssertEquals("No more db hits should be here.", 1, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);

			invoice.RunPreSaveValidation();
			AssertNoWarnings("Cache should be cleared and new data should be used.", invoice.AH_OSTotalAmountInfo);
			AssertEquals("New data should be received so one more db hit here.", 2, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);

			invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly = 0;
			using (invoice.Lines.SuspendListChanged())
			{
				for (int i = 0; i < 10; i++)
				{
					var line3 = (APInvoiceLine)invoice.Lines.AddNew();
					line3.AL_JH = job2.PK;
					line3.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
					line3.AL_AC = cAF.PK;
					line3.AL_GB = sYD.PK;
					line3.AL_GE = fEA.PK;
					line3.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
					line3.AL_ExchangeRate = 1M;
				}
			}
			AssertEquals("New data should be received so one more db hit here.", 0, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);
		}

		public void TestCheckAH_OSTotalAmount_LineCostVariance_ForBulkLineOperations()
		{
			GlbBranch sYD = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			GlbDepartment fEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			AccChargeCode cAF = TestObjectCreator.CC1;

			AssertNotNull(sYD);
			AssertNotNull(fEA);

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Factory.Save();

			var valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Enterprise.Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Enterprise.Core.Constants.CostVarianceComparisonOption.Job;
			var upTo = valuesForTest.AuthorisationRequirements.AddNew();
			upTo.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			upTo.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo.Amount = 100M;
			var above = valuesForTest.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			above.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			above.Amount = upTo.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			invoice.AH_ExchangeRate = 0.5;

			Action<string> assertAllLinesHasWarning = expectedWarning =>
			{
				int i = 1;
				foreach (APInvoiceLine line in invoice.Lines)
				{
					AssertHasWarning("Warnings should be updated for line " + (i++), line.AL_OSExTaxAmountInfo, expectedWarning);
				}
			};

			invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly = 0;
			using (invoice.Lines.SuspendListChanged())
			{
				for (int i = 0; i < 10; i++)
				{
					var line = (APInvoiceLine)invoice.Lines.AddNew();
					line.AL_JH = job.PK;
					line.AL_AC = cAF.PK;
					line.AL_GB = sYD.PK;
					line.AL_GE = fEA.PK;
					line.AL_OSExTaxAmount = 10;
				}
			}
			AssertEquals("New data should be received so one more db hit here.", 1, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);
			assertAllLinesHasWarning("This cost requires approval on posting because it exceeds the registry defined accrual variance threshold.");

			invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly = 0;
			invoice.AH_ExchangeRate = 2;
			AssertEquals("No data should be received so no more db hit here.", 0, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);
			assertAllLinesHasWarning("This cost will be automatically approved when you post because you already have the necessary authorization security right");
		}

		public void TestCheckAH_ChequeOrReference_ChequeNumDigits()
		{
			APInvoice testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice.SubmittedFromInvoicingForm = true;
			testAPInvoice.IsInvoiceReceiptPayment = true;
			testAPInvoice.ReceiptPaymentAH_AB = fTestBank.PK;
			testAPInvoice.ReceiptPaymentAK_AB = fTestChequeBook.PK;
			testAPInvoice.ReceiptPaymentAK_AB = fTestChequeBook.PK;

			testAPInvoice.ReceiptPaymentAH_ChequeOrReference = "12345";
			AssertNoErrors("Valid cheque number", testAPInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo);

			testAPInvoice.ReceiptPaymentAH_ChequeOrReference = "123456";
			AssertHasError("Invalid cheque number", testAPInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo, GetExpectedErrorMessage(testAPInvoice.ReceiptPaymentAH_ChequeOrReference));

			testAPInvoice.ReceiptPaymentAH_ChequeOrReference = "999";
			AssertHasError("Invalid cheque number", testAPInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo, GetExpectedErrorMessage(testAPInvoice.ReceiptPaymentAH_ChequeOrReference));

			testAPInvoice.ReceiptPaymentAH_ChequeOrReference = ((decimal)int.MaxValue + 1).ToString();
			AssertHasError(testAPInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo, GetExpectedErrorMessage(testAPInvoice.ReceiptPaymentAH_ChequeOrReference));

			testAPInvoice.ReceiptPaymentAH_ChequeOrReference = "12X45";
			AssertHasError(testAPInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo, "Only numbers are allowed in this field.");
		}

		string GetExpectedErrorMessage(string chequeNum)
		{
			return string.Format(@"Check number {0} is not contained in the selected check book.
The check number must be between 010000 and 099999.", chequeNum);
		}

		public override void TestCheckAH_LocalTotalAmount_DetectsCreditedExceedInvoiced()
		{
			Assert("Test is already covered in APCreditNoteValidationTest", true);
		}

		public override void TestCheckAH_LocalTaxAmount_DetectsTaxCreditedExceedInvoiced()
		{
			Assert("Test is already covered in APCreditNoteValidationTest", true);
		}

		public override void TestCheckAH_OSTaxAmount_DetectsTaxCreditedExceedInvoiced()
		{
			Assert("Test is already covered in APCreditNoteValidationTest", true);
		}

		public override void TestCheckAH_OSTotalAmount_DetectsCreditedExceedInvoiced()
		{
			Assert("Test is already covered in APCreditNoteValidationTest", true);
		}

		public void TestCheckAH_ChequeOrReference_ChequeNumberInUse()
		{
			PrepareForTestChequeNumberInUse();

			APInvoice testAPInvoice2 = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice2.SubmittedFromInvoicingForm = true;
			testAPInvoice2.IsInvoiceReceiptPayment = true;
			testAPInvoice2.ReceiptPaymentAH_AB = fTestBank.PK;
			testAPInvoice2.ReceiptPaymentAK_AB = fTestChequeBook.PK;

			testAPInvoice2.ReceiptPaymentAH_ChequeOrReference = "10001";
			AssertHasErrors("JobCharge with this cheque number", testAPInvoice2.ReceiptPaymentAH_ChequeOrReferenceInfo);
			AssertHasErrorContaining(testAPInvoice2.ReceiptPaymentAH_ChequeOrReferenceInfo, "Check number 10001 is already used on Job ");
			testAPInvoice2.ReceiptPaymentAH_ChequeOrReference = "10002";
			AssertHasErrors("Payment Approval with this cheque number", testAPInvoice2.ReceiptPaymentAH_ChequeOrReferenceInfo);
			testAPInvoice2.ReceiptPaymentAH_ChequeOrReference = "10003";
			AssertNoErrors("Hot Cheque with this cheque number", testAPInvoice2.ReceiptPaymentAH_ChequeOrReferenceInfo);
			testAPInvoice2.ReceiptPaymentAH_ChequeOrReference = "10004";
			AssertHasErrors("AP Payment with this cheque number", testAPInvoice2.ReceiptPaymentAH_ChequeOrReferenceInfo);
			testAPInvoice2.ReceiptPaymentAH_ChequeOrReference = "10005";
			AssertHasErrors("Direct Payment with this cheque number", testAPInvoice2.ReceiptPaymentAH_ChequeOrReferenceInfo);
			testAPInvoice2.ReceiptPaymentAH_ChequeOrReference = "10006";
			AssertNoErrors("Reversed Payment with this cheque number", testAPInvoice2.ReceiptPaymentAH_ChequeOrReferenceInfo);
			testAPInvoice2.ReceiptPaymentAH_ChequeOrReference = "10007";
			AssertNoErrors("Nothing using this cheque number", testAPInvoice2.ReceiptPaymentAH_ChequeOrReferenceInfo);
		}

		public void TestCheckAH_ChequeOrReference_CancelledChequeNumber()
		{
			APInvoice testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice.SubmittedFromInvoicingForm = true;
			testAPInvoice.IsInvoiceReceiptPayment = true;
			testAPInvoice.ReceiptPaymentAH_AB = fTestBank.PK;
			testAPInvoice.ReceiptPaymentAK_AB = fTestChequeBook.PK;
			testAPInvoice.ReceiptPaymentAH_ChequeOrReference = "12345";
			AssertEquals("Valid cheque number", false, testAPInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo.HasErrors());

			testAPInvoice.AH_IsCancelled = true;
			TransactionMatchLink matchLink = ((IMatching)testAPInvoice).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = testAPInvoice.PK;
			matchLink.AP_Amount = testAPInvoice.AH_OutstandingAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(testAPInvoice);
			Factory.Save();

			AccTransactionHeader justCreatedPayment = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Payment));
			justCreatedPayment.AH_IsCancelled = true;
			TransactionMatchLink matchLink1 = ((IMatching)testAPInvoice).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink1.AP_AH = justCreatedPayment.PK;
			matchLink1.AP_Amount = justCreatedPayment.AH_OutstandingAmount;
			matchLink1.AP_MatchGroupNum = matchLink.AP_MatchGroupNum;
			TestObjectCreator.SetupMatchLinkMatchDate(testAPInvoice);
			Factory.Save();

			APInvoice testAPInvoice2 = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice2.SubmittedFromInvoicingForm = true;
			testAPInvoice2.IsInvoiceReceiptPayment = true;
			testAPInvoice2.ReceiptPaymentAH_AB = fTestBank.PK;
			testAPInvoice2.ReceiptPaymentAK_AB = fTestChequeBook.PK;
			testAPInvoice2.ReceiptPaymentAH_ChequeOrReference = "12345";
			AssertEquals("Cancelled Cheque number", false, testAPInvoice2.ReceiptPaymentAH_ChequeOrReferenceInfo.HasErrors());
			testAPInvoice2.AH_IsCancelled = false;

			Factory.Save();

			APInvoice testAPInvoice3 = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice3.SubmittedFromInvoicingForm = true;
			testAPInvoice3.IsInvoiceReceiptPayment = true;
			testAPInvoice3.ReceiptPaymentAH_AB = fTestBank.PK;
			testAPInvoice3.ReceiptPaymentAK_AB = fTestChequeBook.PK;
			testAPInvoice3.ReceiptPaymentAH_ChequeOrReference = "12345";
			AssertEquals("Used Cheque number", true, testAPInvoice3.ReceiptPaymentAH_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestCheckAH_ChequeOrReference_ValidateMiddleChequeNumber()
		{
			APInvoice testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice.SubmittedFromInvoicingForm = true;
			testAPInvoice.IsInvoiceReceiptPayment = true;
			testAPInvoice.ReceiptPaymentAH_AB = fTestBank.PK;
			testAPInvoice.ReceiptPaymentAK_AB = fTestChequeBook.PK;
			testAPInvoice.ReceiptPaymentAH_ChequeOrReference = "10005";
			AssertEquals("Valid cheque number", false, testAPInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo.HasErrors());
			Factory.Save();

			APInvoice testAPInvoice2 = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice2.SubmittedFromInvoicingForm = true;
			testAPInvoice2.IsInvoiceReceiptPayment = true;
			testAPInvoice2.ReceiptPaymentAH_AB = fTestBank.PK;
			testAPInvoice2.ReceiptPaymentAK_AB = fTestChequeBook.PK;
			testAPInvoice2.ReceiptPaymentAH_ChequeOrReference = "10007";
			AssertEquals("Valid Cheque number", false, testAPInvoice2.ReceiptPaymentAH_ChequeOrReferenceInfo.HasErrors());
			Factory.Save();

			APInvoice testAPInvoice3 = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice3.SubmittedFromInvoicingForm = true;
			testAPInvoice3.IsInvoiceReceiptPayment = true;
			testAPInvoice3.ReceiptPaymentAH_AB = fTestBank.PK;
			testAPInvoice3.ReceiptPaymentAK_AB = fTestChequeBook.PK;
			testAPInvoice3.ReceiptPaymentAH_ChequeOrReference = "10006";
			AssertEquals("Valid Cheque number", false, testAPInvoice3.ReceiptPaymentAH_ChequeOrReferenceInfo.HasErrors());
			Factory.Save();

			APInvoice testAPInvoice4 = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice4.SubmittedFromInvoicingForm = true;
			testAPInvoice4.IsInvoiceReceiptPayment = true;
			testAPInvoice4.ReceiptPaymentAH_AB = fTestBank.PK;
			testAPInvoice4.ReceiptPaymentAK_AB = fTestChequeBook.PK;
			testAPInvoice4.ReceiptPaymentAH_ChequeOrReference = "10007";
			AssertEquals("Invalid Cheque number", true, testAPInvoice4.ReceiptPaymentAH_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestCheckReceiptPaymentAH_ReceiptType_WithMatchedJournal()
		{
			var journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionNotFound;
			journal.AH_OH = TestObjectCreator.AALSHI.PK;
			journal.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			journal.AH_ExchangeRate = 1.2m;
			journal.AH_ChequeOrReference = "T001";
			journal.AH_InvoiceAmount = 200;
			journal.AH_OutstandingAmount = 200;
			journal.AH_OSTotal = 200;
			Factory.Save();

			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.USD, 1.2m, TestObjectCreator.AALSHI);
			apInvoice.SubmittedFromInvoicingForm = true;
			apInvoice.AH_TransactionNum = "T001";
			apInvoice.IsInvoiceReceiptPayment = true;
			var line = (APInvoiceLine)apInvoice.Lines.AddNew();

			line.AL_AC = TestObjectCreator.CC1.PK;
			TestObjectCreator.Job1.JH_OA_LocalChargesAddr = ZGuid.Empty;
			line.AL_JH = TestObjectCreator.Job1.PK;
			line.AL_OSExTaxAmount = 250m;

			apInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;

			var expectedErrorMessage = string.Format("Payment details cannot be entered as this AP Invoice Number matches an unpaid ‘Carried Forward’ journal’s payment reference. When this invoice is posted, it will automatically be matched against the journal, up to the value of the invoice. The unpaid journal transaction number is [{0}].", apInvoice.MatchedWithTNFJournalNum);
			AssertHasError("should found the matched journal", apInvoice.ReceiptPaymentAH_ReceiptTypeInfo, expectedErrorMessage);

			apInvoice.AH_TransactionNum = "INV100";
			apInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AssertNoErrors("should not found the matched journal", apInvoice.ReceiptPaymentAH_ReceiptTypeInfo);
		}

		protected override void AssertCreditOnHoldError(ZPropertyInfo aH_OHInfo)
		{
			AssertNoErrors("There should be no error due to credit being on hold", aH_OHInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			fTestBank = Factory.NewWithValidTestData<AccBankAccount>();
			fTestBank.AB_ChequeNumDigits = 6;
			fTestChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			fTestChequeBook.AK_AB = fTestBank.PK;
			fTestChequeBook.AK_StartNo = 10000;
			fTestChequeBook.AK_LastNo = 99999;
		}

		AccBankAccount fTestBank;
		AccChequeBook fTestChequeBook;

		#region PrepareForTestChequeNumberInUse

		void PrepareForTestChequeNumberInUse()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job testJob = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			Charge charge = creator.CreateCharge(testJob, creator.CC1, "Charge 1", creator.USD, 1m, creator.Creditor1, creator.USD, 1m, creator.Agent);
			charge.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge.JR_OSCostExRate = 1.6m;
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_AB = fTestBank.PK;
			charge.BankAccount.AB_ChequeNumDigits = 5;
			charge.JR_AK = fTestChequeBook.PK;
			charge.JR_ChequeNo = "10001";
			Assert("Cheque number is not used yet", !charge.JR_ChequeNoInfo.HasErrors());

			APPaymentApprovalWithAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_OH = creator.ABIGAS.PK;
			paymentApproval.AV_AB = fTestBank.PK;
			paymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			paymentApproval.AV_AK = fTestChequeBook.PK;
			paymentApproval.AV_ChequeOrReference = "10002";
			Assert("Cheque number is not used yet", !paymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			hotCheque.AQ_AK = fTestChequeBook.PK;
			hotCheque.AQ_ChequeNumber = "10003";
			Assert("Cheque number is not used yet", !hotCheque.AQ_ChequeNumberInfo.HasErrors());

			Payment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			payment.AH_AB = fTestBank.PK;
			payment.ChequeBook = fTestChequeBook.PK;
			payment.AH_ChequeOrReference = "10004";
			Assert("Cheque number is not used yet", !payment.AH_ChequeOrReferenceInfo.HasErrors());

			DirectPayment directPayment = Factory.NewWithValidTestData<DirectPayment>();
			directPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			directPayment.AH_AB = fTestBank.PK;
			directPayment.ChequeBookPK = fTestChequeBook.PK;
			directPayment.AH_ChequeOrReference = "10005";
			Assert("Cheque number is not used yet", !directPayment.AH_ChequeOrReferenceInfo.HasErrors());

			Payment reversedPayment = Factory.NewWithValidTestData<APPayment>();
			reversedPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			reversedPayment.AH_AB = fTestBank.PK;
			reversedPayment.ChequeBook = fTestChequeBook.PK;
			reversedPayment.AH_ChequeOrReference = "10006";
			reversedPayment.AH_IsCancelled = true;
			TransactionMatchLink matchLink = ((IMatching)reversedPayment).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = reversedPayment.PK;
			matchLink.AP_Amount = reversedPayment.AH_OutstandingAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);

			Factory.Save();
		}

		#endregion

		#region ReceiptPaymentAddressOverride/ContactOverride

		public void TestValidateReceiptPaymentAddressOverrideContactOverride()
		{
			var testAPInvoice = Factory.NewWithValidTestData<APInvoice>();

			testAPInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
			testAPInvoice.SubmittedFromInvoicingForm = true;
			testAPInvoice.IsInvoiceReceiptPayment = true;

			var aPAddress = TestObjectCreator.CreateAddress(testAPInvoice.Header, OrgAddressType.Payables, true);
			var contact1 = TestObjectCreator.CreateContact(testAPInvoice.Header, "contact 1");

			testAPInvoice.ReceiptPaymentAddressOverride = aPAddress.PK;
			testAPInvoice.ReceiptPaymentContactOverride = contact1.PK;

			AssertNoErrors(testAPInvoice.ReceiptPaymentAddressOverrideInfo);
			AssertNoErrors(testAPInvoice.ReceiptPaymentContactOverrideInfo);

			testAPInvoice.ReceiptPaymentAddressOverride = ZGuid.NewZGuid();
			testAPInvoice.ReceiptPaymentContactOverride = ZGuid.NewZGuid();

			AssertHasError(testAPInvoice.ReceiptPaymentAddressOverrideInfo, "Enter a valid Payment Address.");
			AssertHasError(testAPInvoice.ReceiptPaymentContactOverrideInfo, "Enter a valid Payment Contact.");
		}

		#endregion
	}
}
