using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public abstract class PaymentApprovalValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAV_EPaymentReasonCode()
		{
			var paymentReasonMandatoryError = "Specifying a Payment Reason is mandatory requirement of your FX provider. Please select a reason from the drop down list of accepted Payment Reasons.";
			var invalidReasonError = "Enter a valid selection.";
			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();

			var payment = (PaymentApprovalBase)Factory.New(GetValidationBizoType());
			payment.AV_PaymentType = ReceiptTypes.EPayment;
			payment.AV_AB = ofxBankAccount.PK;
			payment.AV_EPaymentReasonCode = ZString.Empty;
			AssertHasError(payment.AV_EPaymentReasonCodeInfo, paymentReasonMandatoryError);
			payment.AV_EPaymentReasonCode = "AAA";
			AssertNoError(payment.AV_EPaymentReasonCodeInfo, paymentReasonMandatoryError);
			AssertHasError(payment.AV_EPaymentReasonCodeInfo, invalidReasonError);
			payment.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			AssertNoError(payment.AV_EPaymentReasonCodeInfo, invalidReasonError);

			payment.AV_PaymentType = ReceiptTypes.Cheque;
			payment.AV_EPaymentReasonCode = ZString.Empty;
			AssertNoError(payment.AV_EPaymentReasonCodeInfo, paymentReasonMandatoryError);
		}

		public void TestCheckAV_OH_IsValid()
		{
			var payment = (PaymentApprovalBase)Factory.New(GetValidationBizoType());
			var validation = new PaymentApprovalValidation(payment);
			var validAccount = payment.AV_Ledger == LedgerTypes.AccountsReceivable ? TestObjectCreator.Debtor.PK : TestObjectCreator.Creditor1.PK;
			var invalidAccount = payment.AV_Ledger != LedgerTypes.AccountsReceivable ? TestObjectCreator.Debtor.PK : TestObjectCreator.Creditor1.PK;
			payment.AV_OH = invalidAccount;
			validation.ValidateAV_OH();
			AssertHasError(payment.AV_OHInfo, "Enter a valid Account.");
			payment.AV_OH = validAccount;
			validation.ValidateAV_OH();
			AssertNoErrors(payment.AV_OHInfo);
		}

		public void TestCheckAV_OH_TransactionCreationRestriction()
		{
			var payment = (PaymentApprovalBase)Factory.New(GetValidationBizoType());
			var validAccount = payment.AV_Ledger == LedgerTypes.AccountsReceivable ? TestObjectCreator.Debtor.PK : TestObjectCreator.Creditor1.PK;
			payment.AV_OH = validAccount;

			TestObjectCreator.Debtor.CompanyData.OB_ARTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;
			TestObjectCreator.Creditor1.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;
			payment.RunPreSaveValidation();
			AssertHasError("Should have proper validation error", payment.AV_OHInfo, $@"You cannot create transaction '' because organization '{payment.Header.OH_Code}' has an {payment.AV_Ledger} transaction creation restriction policy set to ALL.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data.");

			TestObjectCreator.Debtor.CompanyData.OB_ARTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.Invoice;
			TestObjectCreator.Creditor1.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.Invoice;
			payment.RunPreSaveValidation();
			AssertNoErrors("Should not have validation error", payment.AV_OHInfo);

			var invalidAccount = payment.AV_Ledger != LedgerTypes.AccountsReceivable ? TestObjectCreator.Debtor.PK : TestObjectCreator.Creditor1.PK;
			payment.AV_OH = invalidAccount;
			TestObjectCreator.Debtor.CompanyData.OB_ARTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;
			TestObjectCreator.Creditor1.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;
			payment.RunPreSaveValidation();
			AssertHasError(payment.AV_OHInfo, "Enter a valid Account.");
			AssertEquals(1, payment.AV_OHInfo.GetErrors().Count());
		}

		public void TestChequeBook()
		{
			APPaymentApprovalWithAuthorisation testPayment = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;

			PaymentApprovalValidation testValidation = new PaymentApprovalValidation(testPayment);
			testValidation.CheckAV_AK_ForTestOnly();

			Assert(!testPayment.AV_AKInfo.HasErrors());

			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testValidation.ValidateAV_AK();

			AssertHasErrors(testPayment.AV_AKInfo);
		}

		public void TestBankAccountValidation()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			AccBankAccount currentBranchAccount = creator.AUDBankAccount;
			currentBranchAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			AccBankAccount otherBranchAccount = creator.AUDBankAccount2;
			otherBranchAccount.AB_GB = creator.NonCurrentBranch.PK;

			APPaymentApprovalWithAuthorisation payment = Factory.New<APPaymentApprovalWithAuthorisation>();
			payment.AV_AB = currentBranchAccount.PK;
			payment.Validation.ValidateAV_AB();
			AssertNoErrors("Should be no errors because bank account belongs to current branch", payment.AV_ABInfo);
			payment.AV_AB = otherBranchAccount.PK;
			payment.Validation.ValidateAV_AB();
			AssertHasErrors("Should be errors because bank account belongs to another branch", payment.AV_ABInfo);

			currentBranchAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			var ofxAccount = TestObjectCreator.CreateBankAccount("OFX", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			ofxAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			ofxAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;

			var expectedError1 = "This bank account is an E-Payment Account. Please set Payment Type to EPA - E-Payment.";
			var expectedError2 = "Bank Account is not an E-Payment Account.";

			payment.AV_PaymentType = ReceiptTypes.Cheque;
			payment.AV_AB = currentBranchAccount.PK;
			AssertNoErrorContaining(payment.AV_ABInfo, expectedError1);
			AssertNoErrorContaining(payment.AV_ABInfo, expectedError2);

			payment.AV_AB = ofxAccount.PK;
			AssertHasError(payment.AV_ABInfo, expectedError1);
			AssertNoErrorContaining(payment.AV_ABInfo, expectedError2);

			payment.AV_PaymentType = ReceiptTypes.EPayment;
			payment.AV_AB = currentBranchAccount.PK;
			AssertNoErrorContaining(payment.AV_ABInfo, expectedError1);
			AssertHasError(payment.AV_ABInfo, expectedError2);

			payment.AV_AB = ofxAccount.PK;
			AssertNoErrorContaining(payment.AV_ABInfo, expectedError1);
			AssertNoErrorContaining(payment.AV_ABInfo, expectedError2);
		}

		public void TestCheckAV_ChequeOrReference_ValidateMiddleChequeNumber()
		{
			APPaymentApprovalWithAuthorisation testPayment = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_OH = TestOrgHeader.PK;
			testPayment.AV_AB = fTestBank.PK;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AV_AK = fTestChequeBook.PK;
			testPayment.AV_ChequeOrReference = "10000";
			Factory.Save();

			testPayment = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_OH = TestOrgHeader.PK;
			testPayment.AV_AB = fTestBank.PK;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AV_AK = fTestChequeBook.PK;
			testPayment.AV_ChequeOrReference = "10003";
			Factory.Save();

			testPayment = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_OH = TestOrgHeader.PK;
			testPayment.AV_AB = fTestBank.PK;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AV_AK = fTestChequeBook.PK;
			testPayment.AV_ChequeOrReference = "10002";

			AssertEquals("Should have NO error", false, testPayment.AV_ChequeOrReferenceInfo.HasErrors());

			testPayment = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_OH = TestOrgHeader.PK;
			testPayment.AV_AB = fTestBank.PK;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AV_AK = fTestChequeBook.PK;
			testPayment.AV_ChequeOrReference = "10003";

			AssertEquals("Should have Error", true, testPayment.AV_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestCheckAV_ChequeOrReference_ChequeNumberInBookRange()
		{
			APPaymentApprovalWithAuthorisation testPayment = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_PaymentType = ReceiptTypes.Cheque;
			testPayment.AV_AK = fTestChequeBook.PK;
			testPayment.AV_ChequeOrReference = "1234";
			AssertHasError(testPayment.AV_ChequeOrReferenceInfo, GetExpectedErrorMessage(testPayment.AV_ChequeOrReference));

			testPayment.AV_ChequeOrReference = "12345";
			AssertEquals("Valid Cheque Number", false, testPayment.AV_ChequeOrReferenceInfo.HasErrors());

			testPayment.AV_ChequeOrReference = "123456";
			AssertHasError(testPayment.AV_ChequeOrReferenceInfo, GetExpectedErrorMessage(testPayment.AV_ChequeOrReference));

			testPayment.AV_ChequeOrReference = ((decimal)int.MaxValue + 1).ToString();
			AssertHasError(testPayment.AV_ChequeOrReferenceInfo, GetExpectedErrorMessage(testPayment.AV_ChequeOrReference));

			testPayment.AV_ChequeOrReference = "12X45";
			AssertHasError(testPayment.AV_ChequeOrReferenceInfo, "Only numbers are allowed in this field.");
		}

		string GetExpectedErrorMessage(string chequeNum)
		{
			return string.Format(@"Check number {0} is not contained in the selected check book.
The check number must be between 10000 and 99999.", chequeNum);
		}

		public void TestCheckAV_ChequeOrReference_ChequeNumberAlreadyUsed_Case1()
		{
			var consol = TestObjectCreator.CreateConsol("AU", "AU", "C01");
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1);
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;

			var charge = Factory.Load<JobCharge>(cost.ApportionmentCharges[0].PK);
			charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge.JR_E6 = cost.PK;
			charge.JR_JH = job.PK;
			charge.JR_PaymentType = ReceiptTypes.Cheque;
			charge.JR_AB = fTestChequeBook.BankAccount.PK;
			charge.JR_ChequeNo = "10001";
			charge.JR_OSCostAmt = 10m;
			charge.JR_LocalCostAmt = 10m;
			charge.JR_AT_CostGSTRate = cost.E6_AT_TaxRate;
			charge.JR_OSCostGSTAmt_Calc = cost.E6_OSGSTAmount_Calc;

			Factory.Save();

			var payment1 = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment1.AV_PaymentType = ReceiptTypes.Cheque;
			payment1.AV_AB = fTestChequeBook.BankAccount.PK;
			payment1.AV_AK = fTestChequeBook.PK;
			payment1.AV_ChequeOrReference = "10002";

			var payment2 = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment2.AV_PaymentType = ReceiptTypes.Cheque;
			payment2.AV_AB = fTestChequeBook.BankAccount.PK;
			payment2.AV_AK = fTestChequeBook.PK;
			payment2.AV_ChequeOrReference = "10001";
			AssertHasError(payment2.AV_ChequeOrReferenceInfo, "Check number 10001 is already used on Consol " + consol.JK_UniqueConsignRef + ".");

			job.Charges[0].JR_E6 = ZGuid.Empty;

			payment2.AV_ChequeOrReference = "";
			payment2.AV_ChequeOrReference = "10001";
			AssertHasError(payment2.AV_ChequeOrReferenceInfo, "Check number 10001 is already used on Job " + job.JH_JobNum + ".");

			payment2.AV_ChequeOrReference = "10002";
			AssertHasError(payment2.AV_ChequeOrReferenceInfo, "Check number 10002 is already in use.");

			payment2.AV_ChequeOrReference = "10003";
			AssertEquals("Valid Cheque Number", false, payment2.AV_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestCheckAV_ChequeOrReference_ChequeNumberAlreadyUsed_Case2()
		{
			PrepareForTestChequeNumberInUse();
			Factory.Save();

			APPaymentApprovalWithAuthorisation testPayment2 = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPayment2.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment2.AV_AB = fTestBank.PK;
			testPayment2.AV_AK = fTestChequeBook.PK;

			testPayment2.AV_ChequeOrReference = "10001";
			AssertHasErrors("JobCharge with this cheque number", testPayment2.AV_ChequeOrReferenceInfo);

			testPayment2.AV_ChequeOrReference = "10002";
			AssertHasErrors("Payment Approval with this cheque number", testPayment2.AV_ChequeOrReferenceInfo);

			testPayment2.AV_ChequeOrReference = "10003";
			AssertNoErrors("Hot Cheque with this cheque number", testPayment2.AV_ChequeOrReferenceInfo);

			testPayment2.AV_ChequeOrReference = "10004";
			AssertHasErrors("AP Payment with this cheque number", testPayment2.AV_ChequeOrReferenceInfo);

			testPayment2.AV_ChequeOrReference = "10005";
			AssertHasErrors("Direct Payment with this cheque number", testPayment2.AV_ChequeOrReferenceInfo);

			testPayment2.AV_ChequeOrReference = "10006";
			AssertNoErrors("Reversed Payment with this cheque number", testPayment2.AV_ChequeOrReferenceInfo);

			testPayment2.AV_ChequeOrReference = "10007";
			AssertNoErrors("Nothing using this cheque number", testPayment2.AV_ChequeOrReferenceInfo);
		}

		public void TestCheckAV_ChequeOrReference_ChequeNumberAlreadyUsed_Case2_WithSavedChargeButInMemoryPaymentApproval()
		{
			var shipment = TestObjectCreator.CreateShipment("S09871");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 12m);
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ReceiptTypes.Cheque;
			charge.JR_AB = fTestBank.PK;
			charge.BankAccount.AB_ChequeNumDigits = 5;
			charge.JR_AK = fTestChequeBook.PK;
			charge.JR_ChequeNo = "10001";
			Factory.Save();

			var payment = CreatePaymentForJobShipmentWithInvoices(2, "10001");

			AssertHasErrors("JobCharge with this cheque number already used", payment.AV_ChequeOrReferenceInfo);
		}

		public void TestCheckAV_ChequeOrReference_ChequeNumberAlreadyUsed_WhenPosting()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var shipment = TestObjectCreator.CreateShipment("S09871");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 12m);
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ReceiptTypes.Cheque;
			charge.JR_AB = fTestBank.PK;
			charge.BankAccount.AB_ChequeNumDigits = 5;
			charge.JR_AK = fTestChequeBook.PK;
			charge.JR_ChequeNo = "10001";
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			ReleaseFactory();
			job = Factory.Load<Job>(job.PK);
			var transactions = new InvoicingPostManager(job).CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();
			AssertEquals("Amount of posted transactions", 1, transactions.Count);
			AssertEquals("Amount of posted APInoices", 1, transactions.GetAllAPTransactions().Length);
			AssertEquals("Amount of posted APPaymentApprovals", 1, transactions.GetAllAPPaymentApprovals().Length);
			var paymentApproval = transactions.GetAllAPPaymentApprovals()[0];
			AssertNotNull("New payment should be posted", paymentApproval.NewPayment);
		}

		public void TestCheckAV_ChequeOrReference_ChequeNumberAlreadyUsed_Case3()
		{
			var consol = TestObjectCreator.CreateConsol("AU", "AU", "C01");
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1);
			var cost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, TestObjectCreator.Creditor1);
			var cost3 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC3, TestObjectCreator.Creditor1);
			var cost4 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC4, TestObjectCreator.Creditor1);

			cost.E6_AT_TaxRate = ZGuid.Empty;
			cost.E6_OSCostAmount = 100M;

			cost2.E6_AT_TaxRate = ZGuid.Empty;
			cost2.E6_OSCostAmount = 10M;

			cost3.E6_AT_TaxRate = ZGuid.Empty;
			cost3.E6_OSCostAmount = 100M;

			cost4.E6_AT_TaxRate = ZGuid.Empty;
			cost4.E6_OSCostAmount = 10M;

			var charge1 = Factory.Load<Charge>(cost.ApportionmentCharges[0].PK);
			charge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge1.JR_E6 = cost.PK;
			charge1.JR_JH = job.PK;
			charge1.JR_PaymentType = ReceiptTypes.Cheque;
			charge1.JR_AB = fTestChequeBook.BankAccount.PK;
			charge1.JR_ChequeNo = "10001";

			var charge2 = Factory.Load<Charge>(cost2.ApportionmentCharges[0].PK);
			charge2.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge2.JR_E6 = cost2.PK;
			charge2.JR_JH = job.PK;
			charge2.JR_PaymentType = ReceiptTypes.Cheque;
			charge2.JR_AB = fTestChequeBook.BankAccount.PK;
			charge2.JR_ChequeNo = "10002";
			charge2.JR_OSCostAmt = 10m;

			var charge3 = Factory.Load<Charge>(cost3.ApportionmentCharges[0].PK);
			charge3.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge3.JR_E6 = cost3.PK;
			charge3.JR_JH = job.PK;
			charge3.JR_PaymentType = ReceiptTypes.Cheque;
			charge3.JR_AB = fTestChequeBook.BankAccount.PK;
			charge3.JR_ChequeNo = "10003";

			var charge4 = Factory.Load<Charge>(cost4.ApportionmentCharges[0].PK);
			charge4.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge4.JR_E6 = cost4.PK;
			charge4.JR_JH = job.PK;
			charge4.JR_PaymentType = ReceiptTypes.Cheque;
			charge4.JR_AB = fTestChequeBook.BankAccount.PK;
			charge4.JR_ChequeNo = "10004";
			charge4.JR_OSCostAmt = 10m;

			Factory.Save();

			APPaymentApprovalWithAuthorisation payment = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment.AV_PaymentType = ReceiptTypes.Cheque;
			payment.AV_AB = fTestChequeBook.BankAccount.PK;
			payment.AV_AK = fTestChequeBook.PK;
			APInvoice invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.AALSHI);
			PaymentApprovalItem paymentItem1 = Factory.NewWithValidTestData<PaymentApprovalItem>();
			paymentItem1.A2_AH = invoice1.PK;
			paymentItem1.A2_AV = payment.PK;
			invoice1.AH_TransactionNum = "101";
			cost.E6_AH_APInvoice = invoice1.PK;

			APInvoice invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.AALSHI);
			PaymentApprovalItem paymentItem2 = Factory.NewWithValidTestData<PaymentApprovalItem>();
			paymentItem2.A2_AH = invoice2.PK;
			paymentItem2.A2_AV = payment.PK;
			invoice2.AH_TransactionNum = "102";
			cost3.E6_AH_APInvoice = invoice2.PK;

			ARInvoice aRinvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(aRinvoice1, TestObjectCreator.AUD, 1M, 100M);
			PaymentApprovalItem paymentItem3 = Factory.NewWithValidTestData<PaymentApprovalItem>();
			paymentItem3.A2_AH = aRinvoice1.PK;
			paymentItem3.A2_AV = payment.PK;
			aRinvoice1.AH_TransactionNum = "103";

			charge1 = Factory.Load<Charge>(cost.ApportionmentCharges[0].PK);
			charge1.ReverseAccrual(ZDateTime.Now);
			charge1.JR_AL_APLine = invoice1.Lines[0].PK;
			charge1.SetAmountsFromLinkedLinesForTests();

			charge3 = Factory.Load<Charge>(cost3.ApportionmentCharges[0].PK);
			charge3.ReverseAccrual(ZDateTime.Now);
			charge3.JR_AL_APLine = invoice2.Lines[0].PK;
			charge3.SetAmountsFromLinkedLinesForTests();

			charge4 = Factory.Load<Charge>(cost4.ApportionmentCharges[0].PK);
			charge4.ReverseWIP(ZDateTime.Now);
			charge4.JR_AL_ARLine = aRinvoice1.Lines[0].PK;
			charge4.SetAmountsFromLinkedLinesForTests();
			charge4.JR_OSCostAmt = 10m;

			Factory.Save();

			payment.AV_ChequeOrReference = "10001";
			AssertEquals("'10001' is valid Cheque Number", false, payment.AV_ChequeOrReferenceInfo.HasErrors());

			payment.AV_ChequeOrReference = "10002";
			AssertHasError(payment.AV_ChequeOrReferenceInfo, "Check number 10002 is already used on Consol " + consol.JK_UniqueConsignRef + ".");

			payment.AV_ChequeOrReference = "10003";
			AssertEquals("'10003' is valid Cheque Number", false, payment.AV_ChequeOrReferenceInfo.HasErrors());

			payment.AV_ChequeOrReference = "10004";
			AssertHasError(payment.AV_ChequeOrReferenceInfo, "Check number 10004 is already used on Consol " + consol.JK_UniqueConsignRef + ".");
		}

		public void TestCheckAV_ChequeOrReference_ChequeNumberAlreadyUsed_DbHitsCountWithFiveInvoices()
		{
			var payment = CreatePaymentForJobShipmentWithInvoices(5, "1001");
			Factory.Save();

			ReleaseFactory();
			var paymentInNewFactory = Factory.Load<APPaymentApprovalWithAuthorisation>(payment.PK);
			Factory.ResetDatabaseLoadCount();
			paymentInNewFactory.Validation.ValidateAV_ChequeOrReference();

			AssertEquals("DB Hit count", 2, Factory.DatabaseLoadCount);
		}

		public void TestCheckAV_ChequeOrReference_ChequeNumberAlreadyUsed_DbHitsWithTenInvoices()
		{
			var payment = CreatePaymentForJobShipmentWithInvoices(10, "1001");
			Factory.Save();

			ReleaseFactory();
			var paymentInNewFactory = Factory.Load<APPaymentApprovalWithAuthorisation>(payment.PK);
			Factory.ResetDatabaseLoadCount();
			paymentInNewFactory.Validation.ValidateAV_ChequeOrReference();

			AssertEquals("DB Hit count", 2, Factory.DatabaseLoadCount);
		}

		public void TestCheckAV_ChequeOrReference_CancelledChequeNumber()
		{
			APPayment testPayment = Factory.NewWithValidTestData<APPayment>();
			testPayment.AH_AB = fTestBank.PK;
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.ChequeBook = fTestChequeBook.PK;
			testPayment.AH_ChequeOrReference = "12345";
			AssertEquals("Valid cheque number", false, testPayment.AH_ChequeOrReferenceInfo.HasErrors());

			testPayment.AH_IsCancelled = true;
			((IMatching)testPayment).CurrentMatchGroup.AddNew().AP_AH = testPayment.PK;

			TestObjectCreator.SetupMatchLinkMatchDate(testPayment);
			Factory.Save();

			APPaymentApprovalWithAuthorisation testPayment2 = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPayment2.AV_AB = fTestBank.PK;
			testPayment2.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment2.AV_AK = fTestChequeBook.PK;
			testPayment2.AV_ChequeOrReference = "12345";
			AssertEquals("Cancelled Cheque number", false, testPayment2.AV_ChequeOrReferenceInfo.HasErrors());
			testPayment2.AV_ChequeOrReference = "11111";
			testPayment2.Delete();

			APPayment testPayment3 = Factory.NewWithValidTestData<APPayment>();
			testPayment3.AH_AB = fTestBank.PK;
			testPayment3.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment3.ChequeBook = fTestChequeBook.PK;
			testPayment3.AH_ChequeOrReference = "12345";
			testPayment3.AH_IsCancelled = false;

			Factory.Save();

			testPayment2 = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPayment2.AV_AB = fTestBank.PK;
			testPayment2.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment2.AV_AK = fTestChequeBook.PK;
			testPayment2.AV_ChequeOrReference = "12345";
			AssertEquals("Used Cheque number", true, testPayment2.AV_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestCheckAV_ChequeOrReference_ValidationIsDisabledForAutoAllocationMode()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook();
			APPaymentApprovalWithAuthorisation testPayment = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;

			testPayment.AV_AB = testChequeBook.AK_AB;
			testPayment.AV_AK = testChequeBook.PK;
			testPayment.AV_ChequeOrReference = "BLAH!";
			AssertHasErrors("Cheque Number should have an error", testPayment.AV_ChequeOrReferenceInfo);

			testPayment.AV_AB = autoPrintChequeBook.AK_AB;
			testPayment.AV_AK = autoPrintChequeBook.PK;
			testPayment.AV_ChequeOrReference = "BLAH!";
			AssertNoErrors("Should not have any errors as now in autoallocation mode", testPayment.AV_ChequeOrReferenceInfo);
		}

		public void TestValidateAV_ChequeOrReferenceWhenFactoryHasContextSavingPaymentApprovalAsDraftAndCanSaveAsDraft()
		{
			var testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			var testPayment = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;

			testPayment.AV_AB = testChequeBook.AK_AB;
			testPayment.AV_AK = testChequeBook.PK;
			testPayment.AV_ChequeOrReference = "BLAH!";
			AssertHasErrors("Pre-condition: Cheque Number should have an error", testPayment.AV_ChequeOrReferenceInfo);
			AssertEquals("Pre-condition: IsEnableSaveAsDraft should be false by default.", false, testPayment.IsValidToSaveAsDraft);

			using (new DisposableAction(() => Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft), () => Factory.RemoveContext(BusinessContext.SavingPaymentApprovalAsDraft)))
			{
				AssertEquals("IsEnableSaveAsDraft should be true with context.", true, testPayment.IsValidToSaveAsDraft);
				testPayment.Validation.ValidateAV_ChequeOrReference();
				AssertNoErrors("Should not have any errors as skipped the validation.", testPayment.AV_ChequeOrReferenceInfo);
			}
		}

		public void TestAV_OC_ContactOverrideWhenFactoryHasContextSavingPaymentApprovalAsDraftAndCanSaveAsDraft()
		{
			var testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			var testPayment = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AV_AB = testChequeBook.AK_AB;
			testPayment.AV_AK = testChequeBook.PK;
			testPayment.AV_OC_ContactOverride = ZGuid.Missing;
			AssertHasErrors("Pre-condition: AV_OC_ContactOverride should have an error as AV_OC_ContactOverride is not valid.", testPayment.AV_OC_ContactOverrideInfo);
			AssertEquals("Pre-condition: IsEnableSaveAsDraft should be false by default.", false, testPayment.IsValidToSaveAsDraft);

			using (new DisposableAction(() => Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft), () => Factory.RemoveContext(BusinessContext.SavingPaymentApprovalAsDraft)))
			{
				AssertEquals("IsEnableSaveAsDraft should be true with context.", true, testPayment.IsValidToSaveAsDraft);
				testPayment.Validation.ValidateAV_OC_ContactOverride();
				AssertNoErrors("Should not have any errors as skipped the validation.", testPayment.AV_OC_ContactOverrideInfo);
			}
		}

		public void TestValidateAV_AKWhenFactoryHasContextSavingPaymentApprovalAsDraftAndCanSaveAsDraft()
		{
			var testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			var testPayment = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AV_AB = testChequeBook.AK_AB;
			Assert("Pre-condition: The AV_AK should be empty as didn't set value.", testPayment.AV_AK.IsEmpty);
			testPayment.Validation.ValidateAV_AK();
			AssertHasErrors("Pre-condition: AV_OC_ContactOverride should have an error as AV_AK is empty.", testPayment.AV_AKInfo);
			AssertEquals("Pre-condition: IsEnableSaveAsDraft should be false by default.", false, testPayment.IsValidToSaveAsDraft);

			using (new DisposableAction(() => Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft), () => Factory.RemoveContext(BusinessContext.SavingPaymentApprovalAsDraft)))
			{
				AssertEquals("IsEnableSaveAsDraft should be true with context.", true, testPayment.IsValidToSaveAsDraft);
				testPayment.Validation.ValidateAV_AK();
				AssertNoErrors("Should not have any errors as ChequeBook can be empty when factory contain context 'SavingPaymentApprovalAsDraft' and IsEnableSaveAsDraft is true.", testPayment.AV_AKInfo);
			}
		}

		public virtual void TestOSExTaxAmount()
		{
			APPaymentApprovalWithAuthorisation aPPay = Factory.New<APPaymentApprovalWithAuthorisation>();

			aPPay.IsProcessingPaymentDetail = true;
			aPPay.AV_Amount = 0m;
			((PaymentApprovalValidation)aPPay.Validation).ValidateAV_Amount();
			AssertEquals(false, aPPay.AV_AmountInfo.HasErrors());

			aPPay.AV_Amount = -1m;
			((PaymentApprovalValidation)aPPay.Validation).ValidateAV_Amount();
			AssertEquals(true, aPPay.AV_AmountInfo.GetErrors().Contains("Overseas amount must be greater than or equal to zero"));

			aPPay.AV_Amount = 10m;
			((PaymentApprovalValidation)aPPay.Validation).ValidateAV_Amount();
			AssertEquals(false, aPPay.AV_AmountInfo.HasErrors());

			aPPay.IsProcessingPaymentDetail = false;
			aPPay.AV_Amount = 0m;
			((PaymentApprovalValidation)aPPay.Validation).ValidateAV_Amount();
			AssertEquals(true, aPPay.AV_AmountInfo.GetErrors().Contains("Overseas amount must be greater than zero"));

			aPPay.AV_Amount = -1m;
			((PaymentApprovalValidation)aPPay.Validation).ValidateAV_Amount();
			AssertEquals(true, aPPay.AV_AmountInfo.GetErrors().Contains("Overseas amount must be greater than zero"));

			aPPay.AV_Amount = 10m;
			((PaymentApprovalValidation)aPPay.Validation).ValidateAV_Amount();
			AssertEquals(false, aPPay.AV_AmountInfo.HasErrors());
		}

		public void TestEFTDetailNoWarning()
		{
			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "123456";
			accountDetails.A1_AccountName = "BANKNAME";
			accountDetails.A1_BankAccount = "654321";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			testPayment.Validation.ValidateAV_OH();

			AssertNoWarnings(testPayment.AV_OHInfo);
		}

		public void TestEFTDetailWarningForDirectDebitFlag()
		{
			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testPayment.Validation.ValidateAV_OH();

			AssertHasWarning(testPayment.AV_OHInfo,
				$"Organization {TestObjectCreator.AALSHI.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - Auto Direct Debit" + System.Environment.NewLine +
					"   - BSB Number" + System.Environment.NewLine +
					"   - Account Name" + System.Environment.NewLine +
					"   - Bank Account" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForDirectDebitBSBNumber()
		{
			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "";
			accountDetails.A1_AccountName = "BANKNAME";
			accountDetails.A1_BankAccount = "654321";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testPayment.Validation.ValidateAV_OH();

			AssertHasWarning(testPayment.AV_OHInfo,
				$"Organization {TestObjectCreator.AALSHI.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - Auto Direct Debit" + System.Environment.NewLine +
					"   - BSB Number" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForDirectDebitAccountName()
		{
			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "123456";
			accountDetails.A1_AccountName = "";
			accountDetails.A1_BankAccount = "654321";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testPayment.Validation.ValidateAV_OH();

			AssertHasWarning(testPayment.AV_OHInfo,
				$"Organization {TestObjectCreator.AALSHI.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - Auto Direct Debit" + System.Environment.NewLine +
					"   - Account Name" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForDirectDebitBankAccount()
		{
			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "123456";
			accountDetails.A1_AccountName = "AccountName";
			accountDetails.A1_BankAccount = "";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testPayment.Validation.ValidateAV_OH();

			AssertHasWarning(testPayment.AV_OHInfo,
				$"Organization {TestObjectCreator.AALSHI.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - Auto Direct Debit" + System.Environment.NewLine +
					"   - Bank Account" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForDirectDebitCombined()
		{
			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			testPayment.Validation.ValidateAV_OH();

			AssertHasWarning(testPayment.AV_OHInfo,
				$"Organization {TestObjectCreator.AALSHI.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - Auto Direct Debit" + System.Environment.NewLine +
					"   - BSB Number" + System.Environment.NewLine +
					"   - Account Name" + System.Environment.NewLine +
					"   - Bank Account" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForBSBFormat_ASBDDRFormat()
		{
			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "123-456";
			accountDetails.A1_AccountName = "Account Name";
			accountDetails.A1_BankAccount = "654321";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			TestObjectCreator.AUDBankAccount.AB_AutoDDRFormat = "ASB";

			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testPayment.Validation.ValidateAV_OH();
			AssertHasWarning(testPayment.AV_OHInfo,
				$"Organization {TestObjectCreator.AALSHI.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - BSB Number is in incorrect format. It should be in XXXXXX format" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForBSBAndAccountNumFormat_BCSDDRFormat()
		{
			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "123-456";
			accountDetails.A1_AccountName = "Account Name";
			accountDetails.A1_BankAccount = "654321";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			TestObjectCreator.AUDBankAccount.AB_AutoDDRFormat = Constants.DDRFileFormat.BCS;

			testPayment.AV_PaymentType = ReceiptTypes.DirectDebit;
			testPayment.Validation.ValidateAV_OH();
			AssertHasWarning(testPayment.AV_OHInfo,
				$"Organization {TestObjectCreator.AALSHI.OH_Code} has following Banking Details incorrectly setup.{""}{@"
   - BSB Number is in incorrect format. It should be in XXXXXX format
   - Bank Account Number is in incorrect format. It should be in XXXXXXXX format
"}" + "You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForBSBFormat_ANZDDRFormat_NewZeland()
		{
			GlbBranch newBranch = SetupNewNewZelandCompanyAndBranch();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, NZDBankAccount);
				AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
				accountDetails.A1_BankBsb = "123-456";
				accountDetails.A1_AccountName = "Account Name";
				accountDetails.A1_BankAccount = "654321";
				accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
				accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.NewZealand;
				accountDetails.A1_IsDefaultAccount = true;
				TestObjectCreator.AUDBankAccount.AB_AutoDDRFormat = "ANZ";

				testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
				testPayment.Validation.ValidateAV_OH();
				//Because of spell parsing in New Zeland companies, please use 'Organisation' here, rather than using 'Organization'.
				AssertHasWarning(testPayment.AV_OHInfo, @"Organisation AALSHI has following Banking Details incorrectly setup.
   - BSB Number is in incorrect format. It should be in XXXXXX format
You'll need to correct this before you can generate DDR file.");

				accountDetails.A1_BankBsb = "123456";
				testPayment.Validation.ValidateAV_OH();
				AssertNoWarnings(testPayment.AV_OHInfo);
			}
		}

		public void TestEFTDetailWarningForBSBFormat_ANZDDRFormat_NotNewZeland()
		{
			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "123456";
			accountDetails.A1_AccountName = "Account Name";
			accountDetails.A1_BankAccount = "654321";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			TestObjectCreator.AUDBankAccount.AB_AutoDDRFormat = "ANZ";

			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testPayment.Validation.ValidateAV_OH();
			AssertHasWarning(testPayment.AV_OHInfo,
				$"Organization {TestObjectCreator.AALSHI.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - BSB Number is in incorrect format. It should be in XXX-XXX format" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");

			accountDetails.A1_BankBsb = "123-456";
			testPayment.Validation.ValidateAV_OH();
			AssertNoWarnings(testPayment.AV_OHInfo);
		}

		public void TestCheckAV_PaymentType()
		{
			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);

			Factory.Save();

			testPayment.AV_PaymentType = ReceiptTypes.DirectDebitLine;
			AssertHasError(testPayment.AV_PaymentTypeInfo, "Enter a valid Payment Method.");

			testPayment.AV_PaymentType = ReceiptTypes.Cash;
			AssertNoError(testPayment.AV_PaymentTypeInfo, "Enter a valid Payment Method.");

			testPayment.Header.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "12345");
			testPayment.AV_PaymentType = ReceiptTypes.eNettDirectDebit;
			AssertNoError(testPayment.AV_PaymentTypeInfo, eNettHelper.NoEnettBankInformationOnOrganisationError);
			OrgCusCode code = testPayment.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.eNettRegistrationNumber, GlbCompany.CurrentCompany.Country);
			testPayment.Header.CustomsCodes.Remove(code);
			code.Delete();
			testPayment.AV_PaymentType = ReceiptTypes.Cash;
			testPayment.AV_PaymentType = ReceiptTypes.eNettDirectDebit;
			AssertHasError(testPayment.AV_PaymentTypeInfo, eNettHelper.NoEnettBankInformationOnOrganisationError);
			AssertHasError(testPayment.AV_PaymentTypeInfo, "Bank account is not registered for ComPay.");

			Assert("Prerequisite: bank account shouldn't be registered for ComPay at this point", !AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.Value.ContainsBankAccount(testPayment.BankAccount.PK));
			testPayment.AV_PaymentType = ReceiptTypes.Cash;
			testPayment.AV_PaymentType = ReceiptTypes.eNettDirectDebit;
			AssertHasError(testPayment.AV_PaymentTypeInfo, "Bank account is not registered for ComPay.");
			ENettRegisteredBankAccountCollection col = new ENettRegisteredBankAccountCollection();
			ENettRegisteredBankAccount item = col.AddNew();
			item.BankAccountPK = testPayment.BankAccount.PK;
			AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, col);
			Assert("Prerequisite: bank account should now be registered for ComPay", AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.Value.ContainsBankAccount(testPayment.BankAccount.PK));
			testPayment.AV_PaymentType = ReceiptTypes.Cash;
			testPayment.AV_PaymentType = ReceiptTypes.eNettDirectDebit;
			AssertNoError(testPayment.AV_PaymentTypeInfo, "Bank account is not registered for ComPay.");
		}

		public void TestCheckAV_PaymentTypeSecurity_ARPaymentApprovalWithoutAuthorisation()
		{
			ARPaymentApprovalWithoutAuthorisation aRPay = Factory.New<ARPaymentApprovalWithoutAuthorisation>();

			aRPay.AV_PaymentType = ReceiptTypes.Cash;
			Env.Security.NewReceivablesPaymentCheque.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cash;
			Env.Security.NewReceivablesPaymentCheque.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewReceivablesPaymentCash.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.Cash;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewReceivablesPaymentCash.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.Cash;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewReceivablesPaymentCreditCard.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.CreditCard;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewReceivablesPaymentCreditCard.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.CreditCard;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewReceivablesPaymentDirectDebit.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.DirectDebit;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewReceivablesPaymentDirectDebit.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.DirectDebit;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewReceivablesPaymentEFT.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.EFT;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewReceivablesPaymentEFT.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.EFT;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewReceivablesPaymentSFT.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.ScheduledEFT;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewReceivablesPaymentSFT.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.ScheduledEFT;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewReceivablesPaymentCRQ.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.CollectionRequest;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewReceivablesPaymentCRQ.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.CollectionRequest;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");
		}

		public void TestCheckAV_PaymentTypeSecurity_ARPaymentApprovalWithAuthorisation()
		{
			ARPaymentApprovalWithAuthorisation aRPay = Factory.New<ARPaymentApprovalWithAuthorisation>();

			aRPay.AV_PaymentType = ReceiptTypes.Cash;
			Env.Security.ARPaymentProcessingNewCheque.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cash;
			Env.Security.ARPaymentProcessingNewCheque.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.ARPaymentProcessingNewCash.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.Cash;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.ARPaymentProcessingNewCash.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.Cash;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.ARPaymentProcessingNewCreditCard.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.CreditCard;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.ARPaymentProcessingNewCreditCard.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.CreditCard;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.ARPaymentProcessingNewDirectDebit.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.DirectDebit;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.ARPaymentProcessingNewDirectDebit.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.DirectDebit;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.ARPaymentProcessingNewEFT.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.EFT;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.ARPaymentProcessingNewEFT.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.EFT;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.ARPaymentProcessingNewSFT.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.ScheduledEFT;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.ARPaymentProcessingNewSFT.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.ScheduledEFT;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.ARPaymentProcessingNewCRQ.IsAllowed = true;
			aRPay.AV_PaymentType = ReceiptTypes.CollectionRequest;
			Assert("Receipt Type should have no errors", !aRPay.AV_PaymentTypeInfo.HasErrors());
			aRPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.ARPaymentProcessingNewCRQ.IsAllowed = false;
			aRPay.AV_PaymentType = ReceiptTypes.CollectionRequest;
			AssertHasError(aRPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");
		}

		public void TestCheckAV_PaymentTypeSecurity_APPaymentApprovalWithoutAuthorisation()
		{
			APPaymentApprovalWithoutAuthorisation aPPay = Factory.New<APPaymentApprovalWithoutAuthorisation>();

			aPPay.AV_PaymentType = ReceiptTypes.Cash;
			Env.Security.NewPayablesPaymentCheque.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cash;
			Env.Security.NewPayablesPaymentCheque.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCash.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.Cash;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewPayablesPaymentCash.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.Cash;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.CreditCard;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.CreditCard;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.DirectDebit;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.DirectDebit;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentEFT.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.EFT;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewPayablesPaymentEFT.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.EFT;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentSFT.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.ScheduledEFT;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewPayablesPaymentSFT.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.ScheduledEFT;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCRQ.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.CollectionRequest;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.NewPayablesPaymentCRQ.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.CollectionRequest;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");
		}

		public void TestCheckAV_PaymentTypeSecurity_APPaymentApprovalWithAuthorisation()
		{
			APPaymentApprovalWithAuthorisation aPPay = Factory.New<APPaymentApprovalWithAuthorisation>();

			aPPay.AV_PaymentType = ReceiptTypes.Cash;
			Env.Security.APPaymentProcessingNewCheque.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cash;
			Env.Security.APPaymentProcessingNewCheque.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.APPaymentProcessingNewCash.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.Cash;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.APPaymentProcessingNewCash.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.Cash;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.APPaymentProcessingNewCreditCard.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.CreditCard;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.APPaymentProcessingNewCreditCard.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.CreditCard;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.APPaymentProcessingNewDirectDebit.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.DirectDebit;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.APPaymentProcessingNewDirectDebit.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.DirectDebit;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.APPaymentProcessingNewEFT.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.EFT;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.APPaymentProcessingNewEFT.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.EFT;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.APPaymentProcessingNewSFT.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.ScheduledEFT;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.APPaymentProcessingNewSFT.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.ScheduledEFT;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.APPaymentProcessingNewCRQ.IsAllowed = true;
			aPPay.AV_PaymentType = ReceiptTypes.CollectionRequest;
			Assert("Receipt Type should have no errors", !aPPay.AV_PaymentTypeInfo.HasErrors());
			aPPay.AV_PaymentType = ReceiptTypes.Cheque;
			Env.Security.APPaymentProcessingNewCRQ.IsAllowed = false;
			aPPay.AV_PaymentType = ReceiptTypes.CollectionRequest;
			AssertHasError(aPPay.AV_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");
		}

		public void TestCheckAV_AK()
		{
			string warningSamePrinterMessage = AccChequeBook.WarningChequeBookWithSamePrinterMessageStart + "'Book1'" + AccChequeBook.WarningSamePrinterMessageEnd;

			BusinessObject printer = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook.AK_AB = testBank.PK;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			chequeBook.AK_Code = "Book1";
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printer.PK;

			AccChequeBook book2 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			book2.AK_AB = testBank.PK;
			book2.AK_GB = chequeBook.AK_GB;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;

			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);
			testPayment.AV_PaymentType = ReceiptTypes.Cheque;
			testPayment.AV_AB = book2.AK_AB;
			testPayment.AV_AK = book2.PK;
			Assert("Should have warning about another Cheque Book with the same printer", testPayment.AV_AKInfo.HasWarning(warningSamePrinterMessage));

			book2.AK_SQ = Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>().PK;
			testPayment.Validation.ValidateAV_AK();
			AssertHasError(testPayment.AV_AKInfo, "Auto printing of check is not configured properly.");

			chequeBook.BankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			testPayment.Validation.ValidateAV_AK();
			AssertNoErrors(testPayment.AV_AKInfo);

			Env.Security.PrintCheque.IsAllowed = false;
			testPayment.Validation.ValidateAV_AK();
			AssertHasError(testPayment.AV_AKInfo, "You do not have the permission to print Check. Please Contact System Administrator.");

			Env.Security.PrintCheque.IsAllowed = true;
			testPayment.Validation.ValidateAV_AK();
			AssertNoErrors(testPayment.AV_AKInfo);

			Env.Security.PrintCheque.IsAllowed = false;
			book2.AK_AutoPrintCheque = ZBool.False;
			testPayment.Validation.ValidateAV_AK();
			AssertNoErrors(testPayment.AV_AKInfo);
		}

		public void TestAccountDetailsNotFound()
		{
			APPaymentApprovalWithAuthorisation testPayment = SetupPaymentForTesting(TestObjectCreator.AALSHI, TestObjectCreator.AUDBankAccount);
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			testPayment.Validation.ValidateAV_OH();

			AssertNoWarnings(testPayment.AV_OHInfo);
			AssertHasErrors("Payee/OrgHeader should have errors", testPayment.AV_OHInfo);
			string expectedMessage = string.Format("An AP Bank Account could not be found with currency {0} and payment type DDR for the payee {1}.\r\n\r\nPlease set up an AP Account for the organization {1} under the AP Details tab, by right-clicking this grid and selecting \"Edit Payment Organization Detail\", with the currency {0} and payment type of DDR.", Core.Constants.CurrencyCodes.Australia, testPayment.Header.OH_Code);
			AssertContains("Incorrect error message", expectedMessage, testPayment.AV_OHInfo.GetErrors().GetFirstMessage());
		}

		public void TestAV_AKHasAutoAllocationValidation()
		{
			APPaymentApprovalWithAuthorisation testPayment = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			AccChequeBook newChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testPayment.AV_AB = newChequeBook.AK_AB;
			testPayment.AV_AK = newChequeBook.PK;

			Assert("Should be no errors so far", !testPayment.AV_AKInfo.HasErrors());

			newChequeBook.AK_IsActive = ZBool.False;
			testPayment.AV_AK = ZGuid.Empty;
			testPayment.AV_AK = newChequeBook.PK;
			AssertHasError(testPayment.AV_AKInfo, AccChequeBookAutoAllocationValidation.ChequeBookIsInActiveMessage);
		}

		public void TestValidateCreditCardSecurityCode()
		{
			var payment = (PaymentApprovalBase)Factory.New(GetValidationBizoType());
			AssertType(ExpectValidationType, payment.Validation);
			payment.AV_PaymentType = ReceiptTypes.eNettCreditCard;
			payment.CreditCardSecurityCode = "1234";
			AssertNoErrors(payment.CreditCardSecurityCodeInfo);

			using (payment.GetValidationSuspender())
			{
				payment.CreditCardSecurityCode = "";
			}
			payment.RunPreSaveValidation();
			var expectedMessage = "Card Security Code must be 3 or 4 digits in length.";
			AssertHasError(payment.CreditCardSecurityCodeInfo, expectedMessage);

			payment.CreditCardSecurityCode = "123";
			AssertNoErrors(payment.CreditCardSecurityCodeInfo);

			payment.CreditCardSecurityCode = "12";
			AssertHasError(payment.CreditCardSecurityCodeInfo, expectedMessage);
		}

		public void TestValidateMatchStatus()
		{
			var payment = (PaymentApprovalBase)Factory.New(GetValidationBizoType());
			AssertType(ExpectValidationType, payment.Validation);

			payment.MatchStatus = "XXX";
			Assert(!((ICodeDescriptionPairListProvider)AccountingConfigurationRegistry.Instance.MatchStatus).CodeDescriptionPairList.ContainsCode("XXX"));

			payment.RunPreSaveValidation();
			var shouldValidateMatchStatusAndReasonCode = !payment.IsPostWithoutMatching && payment.HasPaymentMatchingBaseObjectBeenCreated;
			AssertMatchStatusAndReasonCodeError(shouldValidateMatchStatusAndReasonCode, payment.MatchStatusInfo, "Enter a valid selection.");
		}

		public void TestValidateMatchStatusReasonCode()
		{
			var payment = (PaymentApprovalBase)Factory.New(GetValidationBizoType());
			AssertType(ExpectValidationType, payment.Validation);

			payment.MatchStatusReasonCode = "XXX";

			Assert(!((ICodeDescriptionPairListProvider)AccountingConfigurationRegistry.Instance.MatchStatusReason).CodeDescriptionPairList.ContainsCode("XXX"));

			using (payment.SuspendValidationTesting())
			{
				var matchStatusReasonCodeInfo = payment.MatchStatusReasonCodeInfo;
				var validator = (PaymentApprovalValidation)payment.Validation;

				matchStatusReasonCodeInfo.ClearAllNotifications();

				var shouldValidateMatchStatusAndReasonCode = !payment.IsPostWithoutMatching && payment.HasPaymentMatchingBaseObjectBeenCreated;

				validator.ValidateMatchStatusReasonCode();
				AssertMatchStatusAndReasonCodeError(shouldValidateMatchStatusAndReasonCode, matchStatusReasonCodeInfo, "Enter a valid selection.");

				matchStatusReasonCodeInfo.ClearAllNotifications();
				payment.MatchStatus = "UAC";
				payment.MatchStatusReasonCode = ZString.Empty;
				validator.ValidateMatchStatusReasonCode();
				AssertMatchStatusAndReasonCodeError(shouldValidateMatchStatusAndReasonCode, matchStatusReasonCodeInfo, "Please enter a value.");

				Assert(((ICodeDescriptionPairListProvider)AccountingConfigurationRegistry.Instance.MatchStatusReason).CodeDescriptionPairList.ContainsCode("ADV"));

				matchStatusReasonCodeInfo.ClearAllNotifications();
				payment.MatchStatus = ZString.Empty;
				payment.MatchStatusReasonCode = "ADV";
				validator.ValidateMatchStatusReasonCode();
				AssertMatchStatusAndReasonCodeError(shouldValidateMatchStatusAndReasonCode, matchStatusReasonCodeInfo, AccountingMatchStatusReasonCodeErrorMessage.MatchStatusReasonCodeShouldNotSpecified);
			}
		}

		void AssertMatchStatusAndReasonCodeError(ZBool shouldValidateMatchStatusAndReasonCode, ZPropertyInfo propertyInfo, ZString errorMessage)
		{
			if (shouldValidateMatchStatusAndReasonCode)
			{
				AssertHasError(propertyInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(propertyInfo);
			}
		}

		public void TestValidateOSPartialPaymentAmount()
		{
			TestObjectCreator testDataCreator = new TestObjectCreator(Factory);
			OrgHeader org1 = testDataCreator.AALSHI;
			PaymentApprovalBase aPPaymentApprovalToTest = (PaymentApprovalBase)Factory.NewWithValidTestData(typeof(APPaymentApprovalWithoutAuthorisation));
			aPPaymentApprovalToTest.AV_OH = org1.PK;
			aPPaymentApprovalToTest.AV_Amount = 90M;

			IMatchingCollection matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(aPPaymentApprovalToTest);

			IMatching transactionAsMatching = aPPaymentApprovalToTest;

			transactionAsMatching.OSPartialPaymentAmount = 50M;

			AssertType(typeof(PaymentApprovalValidation), aPPaymentApprovalToTest.Validation);

			((PaymentApprovalValidation)aPPaymentApprovalToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(!transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between 0 and 90"));

			transactionAsMatching.OSPartialPaymentAmount = 120M;

			((PaymentApprovalValidation)aPPaymentApprovalToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between 0 and 90"));

			transactionAsMatching.OSPartialPaymentAmount = -120M;

			((PaymentApprovalValidation)aPPaymentApprovalToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between 0 and 90"));

			aPPaymentApprovalToTest.AV_Amount = -90M;

			transactionAsMatching.OSPartialPaymentAmount = -50M;

			((PaymentApprovalValidation)aPPaymentApprovalToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(!transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between -90 and 0"));

			transactionAsMatching.OSPartialPaymentAmount = -120M;

			((PaymentApprovalValidation)aPPaymentApprovalToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between -90 and 0"));

			transactionAsMatching.OSPartialPaymentAmount = 120M;

			((PaymentApprovalValidation)aPPaymentApprovalToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between -90 and 0"));
		}

		public void TestCheckAV_PostDateNotInFuture()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();

			foreach (Type typeToTest in new[] { typeof(APPaymentApprovalWithAuthorisation), typeof(APPaymentApprovalWithoutAuthorisation),
					 typeof(ARPaymentApprovalWithAuthorisation), typeof(ARPaymentApprovalWithoutAuthorisation) })
			{
				var testPaymentApproval = (PaymentApprovalBase)Factory.NewWithValidTestData(typeToTest);

				AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				testPaymentApproval.AV_PostDate = ZDateTime.Now.AddDays(1);
				AssertHasError("AV_PostDate", testPaymentApproval.AV_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

				testPaymentApproval.AV_PostDate = ZDateTime.Now;
				AssertNoErrors("AV_PostDate", testPaymentApproval.AV_PostDateInfo);

				AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				TestObjectCreator.ResetSecurityCore();
				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

				testPaymentApproval.AV_PostDate = ZDateTime.Now.AddDays(2);
				AssertHasError("AV_PostDate", testPaymentApproval.AV_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);

				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
				testPaymentApproval.AV_PostDate = ZDateTime.Now.AddDays(3);
				AssertNoErrors("AV_PostDate", testPaymentApproval.AV_PostDateInfo);
			}
		}

		public void TestCheckAV_PostDate()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();

			foreach (Type typeToTest in new[] { typeof(APPaymentApprovalWithAuthorisation), typeof(APPaymentApprovalWithoutAuthorisation),
					 typeof(ARPaymentApprovalWithAuthorisation), typeof(ARPaymentApprovalWithoutAuthorisation) })
			{
				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

				var apPaymentApprovalToTest = (PaymentApprovalBase)Factory.NewWithValidTestData(typeToTest);
				apPaymentApprovalToTest.AV_OH = TestObjectCreator.AALSHI.PK;
				apPaymentApprovalToTest.AV_Amount = 90M;

				apPaymentApprovalToTest.AV_PostDate = ZDateTime.Empty;
				apPaymentApprovalToTest.Validation.ValidateAV_PostDate();
				Assert("Should have error about missing post date", apPaymentApprovalToTest.AV_PostDateInfo.HasError("Please enter a Post Date."));
				apPaymentApprovalToTest.AV_PostDate = ZDateTime.Invalid;
				apPaymentApprovalToTest.Validation.ValidateAV_PostDate();
				Assert("Should have error about invalid post date", apPaymentApprovalToTest.AV_PostDateInfo.HasError("Enter a valid Post Date."));

				AssertAV_PostDateMessage(0, apPaymentApprovalToTest, "", false);
				AssertAV_PostDateMessage(-2, apPaymentApprovalToTest, "You do not have security rights to back date the Post Date.\r\nThe Post Date will automatically be updated to Today's Date when you post these transactions.\r\n\r\nAlternatively, set the Registry 'Allow Back Posting Sub Ledger Transaction' to YES and ensure that you are allowed to back post transactions in Staff and Resources before posting these transactions.", false);

				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

				AssertAV_PostDateMessage(0, apPaymentApprovalToTest, "", false);
				AssertAV_PostDateMessage(-2, apPaymentApprovalToTest, "", false);

				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;

				AssertAV_PostDateMessage(0, apPaymentApprovalToTest, "", false);
				AssertAV_PostDateMessage(-2, apPaymentApprovalToTest, "", false);

				if (typeToTest == typeof(APPaymentApprovalWithoutAuthorisation) || typeToTest == typeof(ARPaymentApprovalWithoutAuthorisation))
				{
					apPaymentApprovalToTest.AV_Status = PaymentApprovalStatus.Posted;
					AssertAV_PostDateMessage(-2, apPaymentApprovalToTest, "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing", false);
				}

				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

				AssertAV_PostDateMessage(0, apPaymentApprovalToTest, "", false);
				AssertAV_PostDateMessage(-2, apPaymentApprovalToTest, "You do not have security rights to back date the Post Date.\r\nThe Post Date will automatically be updated to Today's Date when you post these transactions.\r\n\r\nAlternatively, set the Registry 'Allow Back Posting Sub Ledger Transaction' to YES and ensure that you are allowed to back post transactions in Staff and Resources before posting these transactions.", false);
			}
		}

		public void TestBranchDepartmentCombinationValidation_PaymentApprovalValidation()
		{
			var bizObj = (PaymentApprovalBase)Factory.NewWithValidTestData(typeof(APPaymentApprovalWithoutAuthorisation));

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch) => { bizObj.AV_GB = branch; }, GlbDepartment.CurrentDepartment, bizObj.AV_GBInfo);
		}

		void AssertAV_PostDateMessage(int daysFromToday, PaymentApprovalBase paymentApproval, string message, bool isError)
		{
			paymentApproval.AV_PostDate = ZDateTime.Now.AddDays(daysFromToday);
			paymentApproval.Validation.ValidateAV_PostDate();
			string assertionMsg = string.Format("Should have {0} about post date is in {1}",
				string.IsNullOrEmpty(message) ? "no problem" : (isError ? "error" : "warning"),
				daysFromToday < 0 ? "past" : (daysFromToday == 0 ? "present" : "future"));
			bool result;
			if (!string.IsNullOrEmpty(message))
			{
				var c = paymentApproval.AV_PostDateInfo.GetWarnings().GetFirstMessage();
				result = isError ? paymentApproval.AV_PostDateInfo.HasError(message) : paymentApproval.AV_PostDateInfo.HasWarning(message);
			}
			else
			{
				result = !paymentApproval.AV_PostDateInfo.HasErrors() && !paymentApproval.AV_PostDateInfo.HasWarnings();
			}
			Assert(assertionMsg, result);
		}

		public void TestNoChangesAllowedWhenPaymentHasActiveDeal_AV_OH()
		{
			var payment = CreatePaymentApprovalWithDeal();
			AssertNoChangesAllowedWhenPaymentHasActiveDeal(payment, payment.AV_OHInfo, () => payment.AV_OH = TestObjectCreator.ABIGAS.PK);
		}

		public void TestNoChangesAllowedWhenPaymentHasActiveDeal_AV_AB()
		{
			var payment = CreatePaymentApprovalWithDeal();
			AssertNoChangesAllowedWhenPaymentHasActiveDeal(payment, payment.AV_ABInfo, () => payment.AV_AB = TestObjectCreator.EURBankAccount.PK);
		}

		public void TestNoChangesAllowedWhenPaymentHasActiveDeal_AV_Amount()
		{
			var payment = CreatePaymentApprovalWithDeal();
			AssertNoChangesAllowedWhenPaymentHasActiveDeal(payment, payment.AV_AmountInfo, () => payment.AV_Amount = 1000m);
		}

		public void TestNoChangesAllowedWhenPaymentHasActiveDeal_AV_RX_NKPaymentCurrency()
		{
			var payment = CreatePaymentApprovalWithDeal();
			AssertNoChangesAllowedWhenPaymentHasActiveDeal(payment, payment.AV_RX_NKPaymentCurrencyInfo, () => payment.AV_RX_NKPaymentCurrency = CurrencyCodes.Azerbaijan);
		}

		public void TestCheckForPaymentBatch_AV_Status()
		{
			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();

			var payment1 = (PaymentApprovalBase)Factory.NewWithValidTestData(GetValidationBizoType());
			payment1.InitializeForPaymentBatch(() => false);
			payment1.AV_APB_PaymentBatch = batch.PK;

			var payment2 = (PaymentApprovalBase)Factory.NewWithValidTestData(GetValidationBizoType());
			payment2.InitializeForPaymentBatch(() => false);
			payment2.AV_APB_PaymentBatch = batch.PK;

			var payment3 = (PaymentApprovalBase)Factory.NewWithValidTestData(GetValidationBizoType());
			payment3.InitializeForPaymentBatch(() => false);
			payment3.AV_APB_PaymentBatch = batch.PK;

			var payment4 = (PaymentApprovalBase)Factory.NewWithValidTestData(GetValidationBizoType());
			payment4.InitializeForPaymentBatch(() => false);
			payment4.AV_APB_PaymentBatch = batch.PK;

			var payment5 = (PaymentApprovalBase)Factory.NewWithValidTestData(GetValidationBizoType());
			payment5.AV_APB_PaymentBatch = batch.PK;

			payment4.AV_PaymentType = batch.APB_PaymentType;
			payment4.AV_PaymentDate = batch.APB_PaymentDate;
			payment4.AV_AB = batch.APB_AB;
			payment4.AV_AK = batch.APB_AK;

			Factory.Save();

			AssertEquals("Percondition", true, payment1.IsEditOrViewPaymentBatch);
			AssertEquals("Percondition", true, payment2.IsEditOrViewPaymentBatch);
			AssertEquals("Percondition", true, payment3.IsEditOrViewPaymentBatch);
			AssertEquals("Percondition", true, payment4.IsEditOrViewPaymentBatch);
			AssertEquals("Percondition", false, payment5.IsEditOrViewPaymentBatch);

			AssertEquals("Percondition", true, payment1.IsDiscrepancyWithPaymentBatch);
			AssertEquals("Percondition", true, payment2.IsDiscrepancyWithPaymentBatch);
			AssertEquals("Percondition", true, payment3.IsDiscrepancyWithPaymentBatch);
			AssertEquals("Percondition", false, payment4.IsDiscrepancyWithPaymentBatch);
			AssertEquals("Percondition", true, payment5.IsDiscrepancyWithPaymentBatch);

			payment1.AV_Status = PaymentApprovalStatus.Cancelled;
			payment2.AV_Status = PaymentApprovalStatus.Posted;
			payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			payment4.AV_Status = PaymentApprovalStatus.Cancelled;
			payment5.AV_Status = PaymentApprovalStatus.Cancelled;

			var notificationExpectedToBeFound = "Payment approval has status CAN-Canceled/PST-Posted, please remove this payment approval via Right Click > Remove in order to save Payment Batch record";
			AssertHasError(payment1.AV_StatusInfo, notificationExpectedToBeFound);
			AssertHasError(payment2.AV_StatusInfo, notificationExpectedToBeFound);
			AssertNoErrors(payment3.AV_StatusInfo);
			AssertNoErrors(payment4.AV_StatusInfo);
			AssertNoErrors(payment5.AV_StatusInfo);
		}

		public void TestCheckForPaymentBatch_AV_PaymentDate()
		{
			AssertCheckForPaymentBatch((batch, payment1, payment2, payment3) =>
			{
				var dateTime = ZDateTime.Now;
				payment1.AV_PaymentDate = payment3.AV_PaymentDate = dateTime.AddDays(1);
				payment2.AV_PaymentDate = batch.APB_PaymentDate = dateTime;

				AssertHasError(payment1.AV_PaymentDateInfo, "Payment approval detail does not match payment batch header - please reconcile Payment Date with payment batch header manually or by re-entering header details.");
				AssertNoErrors(payment2.AV_PaymentDateInfo);
				AssertNoErrors(payment3.AV_PaymentDateInfo);
			});
		}

		public void TestCheckForPaymentBatch_AV_PaymentType()
		{
			AssertCheckForPaymentBatch((batch, payment1, payment2, payment3) =>
			{
				payment1.AV_PaymentType = payment3.AV_PaymentType = ReceiptTypes.CreditCard;
				payment2.AV_PaymentType = batch.APB_PaymentType = ReceiptTypes.Cash;

				AssertHasError(payment1.AV_PaymentTypeInfo, "Payment approval detail does not match payment batch header - please reconcile Payment Method with payment batch header manually or by re-entering header details.");
				AssertNoErrors(payment2.AV_PaymentTypeInfo);
				AssertNoErrors(payment3.AV_PaymentTypeInfo);
			});
		}

		public void TestCheckForPaymentBatch_AV_AB()
		{
			AssertCheckForPaymentBatch((batch, payment1, payment2, payment3) =>
			{
				var bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();
				var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();

				payment1.AV_AB = payment3.AV_AB = bankAccount1.PK;
				payment2.AV_AB = batch.APB_AB = bankAccount2.PK;

				AssertHasError(payment1.AV_ABInfo, "Payment approval detail does not match payment batch header - please reconcile Bank Account with payment batch header manually or by re-entering header details.");
				AssertNoErrors(payment2.AV_ABInfo);
				AssertNoErrors(payment3.AV_ABInfo);
			});
		}

		public void TestCheckForPaymentBatch_AV_AK()
		{
			AssertCheckForPaymentBatch((batch, payment1, payment2, payment3) =>
			{
				var bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();
				var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
				var chequeBook1 = Factory.NewWithValidTestData<AccChequeBook>();
				var chequeBook2 = Factory.NewWithValidTestData<AccChequeBook>();
				chequeBook1.AK_AB = bankAccount1.PK;
				chequeBook2.AK_AB = bankAccount2.PK;

				payment1.AV_AB = payment3.AV_AB = bankAccount1.PK;
				payment1.AV_AK = payment3.AV_AK = chequeBook1.PK;

				payment2.AV_AB = batch.APB_AB = bankAccount2.PK;
				payment2.AV_AK = batch.APB_AK = chequeBook2.PK;

				var notificationExpectedToBeFound = "Payment approval detail does not match payment batch header - please reconcile Check Book with payment batch header manually or by re-entering header details.";
				AssertHasError(payment1.AV_AKInfo, notificationExpectedToBeFound);
				AssertNoErrors(payment2.AV_AKInfo);
				AssertNoErrors(payment3.AV_AKInfo);
			});
		}

		PaymentApprovalBase CreatePaymentApprovalWithDeal()
		{
			var deal = TestObjectCreator.CreateEPaymentDeal();
			Factory.Save();
			var payment = Factory.Load<PaymentApprovalBase>(deal.Quote.PaymentApproval.PK);
			return payment;
		}

		protected void AssertCheckForPaymentBatch(Action<AccPaymentBatch, PaymentApprovalBase, PaymentApprovalBase, PaymentApprovalBase> assertAction)
		{
			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();

			var payment1 = (PaymentApprovalBase)Factory.NewWithValidTestData(GetValidationBizoType());
			payment1.InitializeForPaymentBatch(() => false);
			payment1.AV_APB_PaymentBatch = batch.PK;

			var payment2 = (PaymentApprovalBase)Factory.NewWithValidTestData(GetValidationBizoType());
			payment2.InitializeForPaymentBatch(() => false);
			payment2.AV_APB_PaymentBatch = batch.PK;

			var payment3 = (PaymentApprovalBase)Factory.NewWithValidTestData(GetValidationBizoType());
			payment3.AV_APB_PaymentBatch = batch.PK;

			Factory.Save();

			AssertEquals("Percondition", true, payment1.IsEditOrViewPaymentBatch);
			AssertEquals("Percondition", true, payment2.IsEditOrViewPaymentBatch);
			AssertEquals("Percondition", false, payment3.IsEditOrViewPaymentBatch);

			assertAction(batch, payment1, payment2, payment3);
		}

		void AssertNoChangesAllowedWhenPaymentHasActiveDeal(PaymentApprovalBase payment, ZPropertyInfo info,  Action changeProperyValue)
		{
			Assert(!info.HasChanges);

			foreach (var status in DealStatusCodes.ActiveStatusCodes)
			{
				payment.CurrentDeal.AED_Status = status;
				payment.RunPreSaveValidation();
				AssertNoError(info, PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);
			}
			foreach (var status in DealStatusCodes.InactiveStatusCodes)
			{
				payment.CurrentDeal.AED_Status = status;
				payment.RunPreSaveValidation();
				AssertNoError(info, PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);
			}

			changeProperyValue.Invoke();
			Assert(info.HasChanges);

			foreach (var status in DealStatusCodes.ActiveStatusCodes)
			{
				payment.CurrentDeal.AED_Status = status;
				payment.RunPreSaveValidation();
				AssertHasError(info, PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);
			}
			foreach (var status in DealStatusCodes.InactiveStatusCodes)
			{
				payment.CurrentDeal.AED_Status = status;
				payment.RunPreSaveValidation();
				AssertNoError(info, PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);
			}
		}

		public void TestCheckAV_PaymentTypeIsValidWhenBankAccountOfTypeCashIsSelected()
		{
			var testBank = TestObjectCreator.CreateBankAccount("TEST-BANK", "TEST BANK TYPE", TestObjectCreator.AUD, null, AccountTypeCodeDescriptionPairList.Codes.BNK);
			var testCash = TestObjectCreator.CreateBankAccount("TEST_CASH", "TEST CASH TYPE", TestObjectCreator.AUD, null, AccountTypeCodeDescriptionPairList.Codes.CSH);

			var payment = Factory.New<APPaymentApprovalWithAuthorisation>();
			AssertNoErrors(payment.AV_PaymentTypeInfo);

			payment.AV_AB = testBank.PK;
			AssertNoErrors(payment.AV_PaymentTypeInfo);

			payment.AV_AB = testCash.PK;
			AssertNoErrors(payment.AV_PaymentTypeInfo);

			payment.AV_PaymentType = ReceiptTypes.Cheque;
			AssertHasError("Should have error", payment.AV_PaymentTypeInfo, "For Cash Account, please select CSH - Cash Payment Method.");

			payment.AV_PaymentType = ReceiptTypes.Cash;
			AssertNoErrors(payment.AV_PaymentTypeInfo);
		}

		#region Implementation

		protected abstract Type GetValidationBizoType();

		protected virtual Type ExpectValidationType => typeof(PaymentApprovalValidation);

		APPaymentApprovalWithAuthorisation SetupPaymentForTesting(OrgHeader organisation, AccBankAccount bankAccount)
		{
			bankAccount.AB_AllowAutoDDR = ZBool.True;
			bankAccount.AB_AutoDDRFormat = Core.Constants.DDRFileFormat.ASB;
			APPaymentApprovalWithAuthorisation testPayment = Factory.New<APPaymentApprovalWithAuthorisation>();

			testPayment.AV_AB = bankAccount.PK;
			testPayment.AV_OH = organisation.PK;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			return testPayment;
		}

		GlbBranch SetupNewNewZelandCompanyAndBranch()
		{
			RefCountry newZeland = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.NewZealand));
			GlbCompany newZelandCompany = Factory.NewWithValidTestData<GlbCompany>();
			newZelandCompany.GC_RN_NKCountryCode = newZeland.Code;
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = newZelandCompany.PK;
			Factory.Save();
			return newBranch;
		}

		protected override void SetUp()
		{
			base.SetUp();

			fTestBank = Factory.NewWithValidTestData<AccBankAccount>();
			fTestBank.AB_ChequeNumDigits = (ZByte)5;
			fTestChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			fTestChequeBook.AK_AB = fTestBank.PK;
			fTestChequeBook.AK_StartNo = 10000;
			fTestChequeBook.AK_LastNo = 99999;

			TestOrgHeader = TestObjectCreator.AALSHI;
			Factory.Save();
		}

		AccBankAccount fTestBank;
		AccChequeBook fTestChequeBook;
		OrgHeader TestOrgHeader;

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

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		#region PrepareForTestChequeNumberInUse

		void PrepareForTestChequeNumberInUse()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job testJob = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			Charge charge = creator.CreateCharge(testJob, creator.CC1, "Charge 1", creator.AUD, 1m, creator.Creditor1, creator.AUD, 1m, creator.Agent);
			charge.JR_RX_NKCostCurrency = creator.AUD.RX_Code;
			charge.JR_OSCostExRate = 1m;
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
			((IMatching)reversedPayment).CurrentMatchGroup.AddNew().AP_AH = reversedPayment.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(reversedPayment);
		}

		APPaymentApprovalWithAuthorisation CreatePaymentForJobShipmentWithInvoices(int numberOfInvoices, string chequeNumber)
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			APPaymentApprovalWithAuthorisation payment = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, fTestChequeBook.BankAccount, fTestChequeBook);
			payment.AV_ChequeOrReference = chequeNumber;

			PaymentApprovalItem paymentItem = null;
			InvoicingBase invoice = null;
			for (int i = 0; i < numberOfInvoices; i++)
			{
				invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
				invoice.AH_TransactionNum = i.ToString();

				InvoicingLineBase invoiceLine;
				for (int j = 0; j < 5; j++)
				{
					invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 10);
					TestObjectCreator.CreateCharge(invoiceLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
				}

				paymentItem = TestObjectCreator.CreatePaymentApprovalItem(payment, invoice);
			}

			return payment;
		}

		#endregion

		AccBankAccount fNZDBankAccount;
		public AccBankAccount NZDBankAccount
		{
			get
			{
				if (fNZDBankAccount == null)
				{
					AccGLHeader header = Factory.New<AccGLHeader>();
					header.AG_AccountNum = "ZNZDAcc";
					header.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
					header.AG_IsActive = ZBool.True;
					fNZDBankAccount = fTestObjectCreator.CreateBankAccount("ZHSBCNZD", "HSBC NZD ACCT", "HSBD", "NZD", NZD, "123456", "12345678", header);
				}
				return fNZDBankAccount;
			}
		}

		RefCurrency fNZD;
		public RefCurrency NZD
		{
			get
			{
				if (fNZD == null)
				{
					fNZD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
				}
				return fNZD;
			}
		}

		#endregion

	}
}
