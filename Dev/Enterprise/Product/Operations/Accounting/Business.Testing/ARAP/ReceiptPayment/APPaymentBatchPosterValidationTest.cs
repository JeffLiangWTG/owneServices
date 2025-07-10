using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public class APPaymentBatchPosterValidationTest : AccPaymentBatchValidationTest
	{
		#region APB_AK

		public void TestValidateChequeBook()
		{
			string warningSamePrinterMessage = AccChequeBook.WarningChequeBookWithSamePrinterMessageStart + "'Book2'" + AccChequeBook.WarningSamePrinterMessageEnd;

			var printer = Factory.New<StmPrintQueue>();
			BatchPostingHelper.TestCheques.AK_Code = "Book1";
			BatchPostingHelper.TestCheques.AK_AutoPrintCheque = ZBool.True;
			BatchPostingHelper.TestCheques.AK_SQ = printer.PK;

			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_AK();
			Assert(!BatchPostingHelper.BatchPoster.APB_AKInfo.HasErrors());

			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_AK();
			AssertHasErrors("Check book must be selected", BatchPostingHelper.BatchPoster.APB_AKInfo);

			BatchPostingHelper.BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			BatchPostingHelper.BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
			BatchPostingHelper.TestCheques.AK_AB = ZGuid.Empty;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_AK();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_AKInfo, "This check book does not belong to the bank specified");

			BatchPostingHelper.TestCheques.AK_AB = BatchPostingHelper.TestBank.PK;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_AK();
			Assert("Should be no errors", !BatchPostingHelper.BatchPoster.APB_AKInfo.HasErrors());

			AccChequeBook book2 = Factory.New<AccChequeBook>();
			book2.AK_AB = BatchPostingHelper.TestBank.PK;
			book2.AK_GB = BatchPostingHelper.TestCheques.AK_GB;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;
			Factory.Save();
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_AK();
			Assert("Should have warning about another Cheque Book with the same printer", BatchPostingHelper.BatchPoster.APB_AKInfo.HasWarning(warningSamePrinterMessage));

			using (BatchPostingHelper.BatchPoster.Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				AssertEquals("Percondition", ReceiptTypes.Cheque, BatchPostingHelper.BatchPoster.APB_PaymentType);

				BatchPostingHelper.BatchPoster.APB_AK = ZGuid.Empty;
				BatchPostingHelper.BatchPoster.Validation.ValidateAPB_AK();
				Assert("Should be no errors", !BatchPostingHelper.BatchPoster.APB_AKInfo.HasErrors());

				BatchPostingHelper.BatchPoster.APB_AK = ZGuid.NewZGuid();
				BatchPostingHelper.BatchPoster.Validation.ValidateAPB_AK();
				AssertHasError(BatchPostingHelper.BatchPoster.APB_AKInfo, "Enter a valid selection.");

				BatchPostingHelper.BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
				BatchPostingHelper.BatchPoster.Validation.ValidateAPB_AK();
				Assert("Should be no errors", !BatchPostingHelper.BatchPoster.APB_AKInfo.HasErrors());
			}
		}

		public void TestChequeBookPKHasAutoAllocationValidation()
		{
			var autoPrintChequeBook = BatchPostingHelper.GetAutoPrintChequeBook(Factory, 1, 4, 2);
			var newChequeBook = autoPrintChequeBook;
			newChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AB = newChequeBook.AK_AB;
			BatchPoster.APB_AK = newChequeBook.PK;

			Assert("Should be no errors so far", !BatchPoster.APB_AKInfo.HasErrors());

			newChequeBook.AK_IsActive = ZBool.False;
			BatchPoster.APB_AK = ZGuid.Empty;
			BatchPoster.APB_AK = newChequeBook.PK;
			AssertHasError(BatchPoster.APB_AKInfo, AccChequeBookAutoAllocationValidation.ChequeBookIsInActiveMessage);
		}

		#endregion

		#region APB_ChequeOrReference

		public void TestValidateAPB_ChequeOrReference_SavingPaymentApprovalAsDraft()
		{
			//BatchPostingHelper.SetupDataForPostingTest(false);
			//BatchPostingHelper.BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			//BatchPostingHelper.BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
			BatchPostingHelper.BatchPoster.APB_AK = ZGuid.Empty;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("Percondition", false, BatchPostingHelper.BatchPoster.IsChequeNumberAutoAllocated);
			AssertNullOrEmpty("Percondition", BatchPostingHelper.BatchPoster.APB_ChequeOrReference);

			using (BatchPostingHelper.BatchPoster.Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
				Assert("Should be no errors", !BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo.HasErrors());
			}

			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo, "Please enter a value.");
		}

		public void TestValidateAPB_ChequeOrReference_ValidateMiddleChequeNumber()
		{
			APPaymentApprovalWithAuthorisation testPayment = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_OH = BatchPostingHelper.TestOrg.PK;
			testPayment.AV_AB = BatchPostingHelper.TestBank.PK;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AV_AK = BatchPostingHelper.TestCheques.PK;
			testPayment.AV_ChequeOrReference = "1";
			Factory.Save();

			testPayment = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPayment.AV_OH = BatchPostingHelper.TestOrg.PK;
			testPayment.AV_AB = BatchPostingHelper.TestBank.PK;
			testPayment.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AV_AK = BatchPostingHelper.TestCheques.PK;
			testPayment.AV_ChequeOrReference = "3";
			Factory.Save();

			BatchPostingHelper.BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "2";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			Assert("Should be no errors", !BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo.HasErrors());

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "3";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasErrors("Should have an error", BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo);
		}

		public void TestValidateAPB_ChequeOrReference_ChequeNumberInBookRange()
		{
			BatchPostingHelper.BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "0";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo, GetExpectedErrorMessage(BatchPostingHelper.BatchPoster.APB_ChequeOrReference));

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "4";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			Assert("Valid Cheque Number", !BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo.HasErrors());

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "101";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo, GetExpectedErrorMessage(BatchPostingHelper.BatchPoster.APB_ChequeOrReference));

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = ((decimal)int.MaxValue + 1).ToString();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo, GetExpectedErrorMessage(BatchPostingHelper.BatchPoster.APB_ChequeOrReference));

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "12X45";
			AssertHasError(BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo, "Only numbers are allowed in this field.");

			BatchPostingHelper.AssertBatchPosterWillNotMatchAndPost(BatchPostingHelper.BatchPoster);

			string GetExpectedErrorMessage(string chequeNum)
			{
				return string.Format(@"Check number {0} is not contained in the selected check book.
The check number must be between 000001 and 000100.", chequeNum);
			}
		}

		public void TestValidateAPB_ChequeOrReference_ChequeNumberAlreadyUsed()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var cost = consol.GetApportionments().CostsCollection.TryAddNew();

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_E6 = cost.PK;
			charge.JR_JH = job.PK;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_AB = BatchPostingHelper.TestCheques.BankAccount.PK;
			charge.JR_ChequeNo = "000001";

			var payment1 = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment1.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			payment1.AV_AB = BatchPostingHelper.TestCheques.BankAccount.PK;
			payment1.AV_AK = BatchPostingHelper.TestCheques.PK;
			payment1.AV_ChequeOrReference = "000002";

			BatchPostingHelper.BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPostingHelper.BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000001";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo, "Check number 000001 is already used on Consol " + consol.JK_UniqueConsignRef + ".");

			charge.JR_E6 = ZGuid.Empty;

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "";
			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000001";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo, "Check number 000001 is already used on Job " + job.JH_JobNum + ".");

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000002";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo, "Check number 000002 is already in use.");

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000003";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			Assert("Valid Cheque Number", !BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestValidateAPB_ChequeOrReference_ChequeNumberInUse()
		{
			BatchPostingHelper.PrepareForTestChequeNumberInUse();

			BatchPostingHelper.BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000001";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasErrors("Cheque number is used by JobCharge", BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo);

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000002";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasErrors("Cheque number is used by Payment Approval", BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo);

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000003";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertNoErrors("Hot Cheque with this cheque number", BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo);

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000004";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasErrors("AP Payment with this cheque number", BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo);

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000005";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasErrors("Direct Payment with this cheque number", BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo);

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000006";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertNoErrors("Reversed Payment with this cheque number", BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo);

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000007";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertNoErrors("Valid cheque number", BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo);
		}

		public void TestValidateAPB_ChequeOrReference_CancelledChequeNumber()
		{
			var newFactory = new BusinessObjectFactory();
			APPayment testPayment = newFactory.NewWithValidTestData<APPayment>();
			testPayment.AH_AB = BatchPostingHelper.TestBank.PK;
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.ChequeBook = BatchPostingHelper.TestCheques.PK;
			testPayment.AH_ChequeOrReference = "000001";
			Assert("Valid cheque number", !testPayment.AH_ChequeOrReferenceInfo.HasErrors());
			testPayment.AH_IsCancelled = true;
			((IMatching)testPayment).CurrentMatchGroup.AddNew().AP_AH = testPayment.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(testPayment);
			newFactory.Save();

			BatchPostingHelper.BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000001";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			Assert("Sould be no errors, the number of Cancelled Payment", !BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo.HasErrors());

			testPayment.AH_IsCancelled = false;
			newFactory.Save();

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000001";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();
			AssertHasErrors("Cheque number is in use", BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo);
		}

		public void TestValidateAPB_ChequeOrReference_NotEnoughAccessibleNumbers()
		{
			BatchPostingHelper.SetupDataForPostingTest(false);
			BatchPostingHelper.BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			BatchPostingHelper.BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000099";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();

			AssertEquals("Preconditoin", 3, BatchPostingHelper.BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("Preconditoin", 3, BatchPostingHelper.BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.Count());
			AssertEquals("Preconditoin", 2m, BatchPostingHelper.TestCheques.AK_LastNo - ZDecimal.Parse(BatchPostingHelper.BatchPoster.APB_ChequeOrReference) + 1);
			var expectedErrorMsg = @"The accessible count of check numbers in the selected check book is less than the payment count.
Please change Start Reference No or select another Check Book.";
			AssertHasError(BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo, expectedErrorMsg);

			BatchPostingHelper.PaymentApproval1.AV_Status = PaymentApprovalStatus.Cancelled;
			BatchPostingHelper.PaymentApproval2.AV_Status = PaymentApprovalStatus.Posted;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();

			AssertEquals("Preconditoin", 3, BatchPostingHelper.BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("Preconditoin", 1, BatchPostingHelper.BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.Count());
			AssertEquals("Preconditoin", 2m, BatchPostingHelper.TestCheques.AK_LastNo - ZDecimal.Parse(BatchPostingHelper.BatchPoster.APB_ChequeOrReference) + 1);
			AssertNoErrors(BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo);

			BatchPostingHelper.PaymentApproval1.AV_Status = PaymentApprovalStatus.FullyApproved;
			BatchPostingHelper.PaymentApproval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000098";
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_ChequeOrReference();

			AssertEquals("Preconditoin", 3, BatchPostingHelper.BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("Preconditoin", 3, BatchPostingHelper.BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.Count());
			AssertEquals("Preconditoin", 3m, BatchPostingHelper.TestCheques.AK_LastNo - ZDecimal.Parse(BatchPostingHelper.BatchPoster.APB_ChequeOrReference) + 1);
			AssertNoErrors(BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo);
		}

		public void TestValidateAPB_ChequeOrReference_NumDigits()
		{
			BatchPostingHelper.BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			BatchPostingHelper.BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "000001";
			Assert("Valid cheque number", !BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo.HasErrors());

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "1234567";
			Assert("Invalid cheque number", BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo.HasErrors());

			BatchPostingHelper.BatchPoster.APB_ChequeOrReference = "999";
			Assert("Invalid cheque number", BatchPostingHelper.BatchPoster.APB_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestChequeNumberHasBeenUsed()
		{
			BatchPostingHelper.SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
			BatchPoster.APB_ChequeOrReference = "2";
			Factory.Save();

			BatchPostingHelper.PaymentApproval1.UnRegisterEditableChildObject(BatchPostingHelper.PaymentApproval1.MatchingBaseObject);
			BatchPostingHelper.PaymentApproval1.MatchingBaseObject.MatchedTransactions.AddTransactionThatMustBeMatched(BatchPostingHelper.PaymentApproval1);
			BatchPostingHelper.PaymentApproval1.MatchingBaseObject.MatchAndClearTransactions();
			Factory.Save();

			AssertEquals(true, BatchPostingHelper.PaymentApproval1.IsPosted);
			AssertNotEquals(true, BatchPostingHelper.PaymentApproval2.IsPosted);
			AssertNotEquals(true, BatchPostingHelper.PaymentApproval3.IsPosted);

			BatchPoster.RunPreSaveValidation();
			AssertNoErrors("No error about ChequeNumberHasBeenUsed", BatchPoster.APB_ChequeOrReferenceInfo);
		}

		public void TestChequeOrReferenceValidationForAutoAllocationMode()
		{
			var autoPrintChequeBook = BatchPostingHelper.GetAutoPrintChequeBook(Factory, 1, 4, 2);
			var testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;

			BatchPoster.APB_AB = testChequeBook.AK_AB;
			BatchPoster.APB_AK = testChequeBook.PK;
			BatchPoster.APB_ChequeOrReference = "BLAH!";
			AssertHasErrors("Cheque Number should have an error", BatchPoster.APB_ChequeOrReferenceInfo);

			BatchPoster.APB_AB = testChequeBook.AK_AB;
			BatchPoster.APB_AK = testChequeBook.PK;
			BatchPoster.APB_ChequeOrReference = "15.5";
			AssertHasErrors("Cheque Number should have an error", BatchPoster.APB_ChequeOrReferenceInfo);

			BatchPoster.APB_AB = autoPrintChequeBook.AK_AB;
			BatchPoster.APB_AK = autoPrintChequeBook.PK;
			BatchPoster.APB_ChequeOrReference = "BLAH!";
			AssertNoErrors("Should not have any errors as now in autoallocation mode", BatchPoster.APB_ChequeOrReferenceInfo);

			BatchPostingHelper.SetupDataForPostingTest();
			autoPrintChequeBook.AK_CurrentNo = 4;
			BatchPoster.APB_AB = autoPrintChequeBook.AK_AB;
			BatchPoster.APB_AK = autoPrintChequeBook.PK;
			AssertEquals("Preconditoin", 3, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("Preconditoin", 3, BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.Count());
			AssertEquals("Preconditoin", 1m, autoPrintChequeBook.AK_CurrentNo - autoPrintChequeBook.AK_LastNo + 1);
			var expectedErrorMsg = @"The accessible count of free check numbers is less than the payment count.
Please change the Current Number of the current Check Book or select another Check Book.";
			AssertHasError("ChequeOrReferenceInfo should have an error as there is no enough free numbers for posting", BatchPoster.APB_AKInfo, expectedErrorMsg);

			BatchPostingHelper.PaymentApproval1.AV_Status = PaymentApprovalStatus.Cancelled;
			BatchPostingHelper.PaymentApproval2.AV_Status = PaymentApprovalStatus.Posted;
			BatchPoster.APB_AK = autoPrintChequeBook.PK;
			AssertEquals("Preconditoin", 3, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("Preconditoin", 1, BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.Count());
			AssertEquals("Preconditoin", 1m, autoPrintChequeBook.AK_CurrentNo - autoPrintChequeBook.AK_LastNo + 1);
			AssertNoErrors("Cancelled or posted transaction does not consume cheque number", BatchPoster.APB_AKInfo);
		}

		#endregion

		#region APB_PaymentType

		public void TestValidatePaymentType()
		{
			var creditNote = Factory.NewWithValidTestData<APCreditNote>();

			BatchPostingHelper.BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			BatchPostingHelper.BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebitLine;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_PaymentType();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo, "Enter a valid selection.");

			BatchPostingHelper.BatchPoster.APB_PaymentType = ZString.Empty;
			Assert("Should have an error as the value can not be empty", BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());

			BatchPostingHelper.BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Should be no errors", !BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());

			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.eNettDirectDebit;
			Assert("Should be no errors if only invoices in matching collection", !BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());

			BatchPostingHelper.BatchPoster.MatchingCollection.Add(creditNote);
			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			Assert("Should be no errors if only invoices in matching collection", !BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());

			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.eNettDirectDebit;
			Assert("Should have error if collection contains non INV transaction", BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());
			AssertHasError(BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo, "A ComPay payment can only be matched to AP invoices.");
		}

		public void TestValidatePayment_CheckPaymentTypeSecurity()
		{
			BatchPostingHelper.BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			BatchPostingHelper.BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;

			Env.Security.NewPayablesPaymentCheque.IsAllowed = true;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			Assert("Receipt Type should have no errors", !BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCheque.IsAllowed = false;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_PaymentType();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCash.IsAllowed = true;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			Assert("Receipt Type should have no errors", !BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCash.IsAllowed = false;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_PaymentType();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = true;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.CreditCard;
			Assert("Receipt Type should have no errors", !BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = false;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_PaymentType();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = true;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.DirectDebit;
			Assert("Receipt Type should have no errors", !BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = false;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_PaymentType();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentEFT.IsAllowed = true;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
			Assert("Receipt Type should have no errors", !BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentEFT.IsAllowed = false;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_PaymentType();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentSFT.IsAllowed = true;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.ScheduledEFT;
			Assert("Receipt Type should have no errors", !BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentSFT.IsAllowed = false;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_PaymentType();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");

			Env.Security.NewPayablesPaymentCRQ.IsAllowed = true;
			BatchPostingHelper.BatchPoster.APB_PaymentType = ReceiptTypes.CollectionRequest;
			Assert("Receipt Type should have no errors", !BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo.HasErrors());
			Env.Security.NewPayablesPaymentCRQ.IsAllowed = false;
			BatchPostingHelper.BatchPoster.Validation.ValidateAPB_PaymentType();
			AssertHasError(BatchPostingHelper.BatchPoster.APB_PaymentTypeInfo, "You do not have appropriate security rights to select this payment type.");
		}

		#endregion

		#region APB_PostDate

		public void TestValidatePostDate()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_PostDate = ZDateTime.Empty;
			Assert("Should have an error", BatchPoster.APB_PostDateInfo.HasErrors());

			BatchPoster.APB_PostDate = ZDateTime.Now.AddDays(1);
			AssertHasError(BatchPoster.APB_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			BatchPoster.APB_PostDate = ZDateTime.Now;
			AssertNoErrors("Should Be no errors", BatchPoster.APB_PostDateInfo);
			BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, -1, 1, 2);

			BatchPoster.APB_PostDate = ZDateTime.Now.AddDays(-1);
			AssertHasWarning(BatchPoster.APB_PostDateInfo, "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems " +
			   System.Environment.NewLine + " - Financial Reports" +
			   System.Environment.NewLine + " - Sub-Ledger Reports" +
			   System.Environment.NewLine + " - Bank Reconciliation" +
			   System.Environment.NewLine + " - Reversing");
		}

		public void TestCheckPostDate()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

			BatchPoster.APB_PostDate = ZDateTime.Empty;
			Assert("Should have error about missing post date", BatchPoster.APB_PostDateInfo.HasError("Please enter a value."));

			AssertPostDateMessage(0, BatchPoster, "", false);
			AssertPostDateMessage(-2, BatchPoster, "The post date cannot be in the past", true);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			AssertPostDateMessage(0, BatchPoster, "", false);
			AssertPostDateMessage(-2, BatchPoster, "", false);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;

			AssertPostDateMessage(0, BatchPoster, "", false);
			AssertPostDateMessage(-2, BatchPoster, "", false);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

			AssertPostDateMessage(0, BatchPoster, "", false);
			AssertPostDateMessage(-2, BatchPoster, "The post date cannot be in the past", true);
		}

		public void TestCheckPostDateNotInFuture()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			BatchPoster.APB_PostDate = ZDateTime.Now.AddDays(1);
			AssertHasError("PostDate", BatchPoster.APB_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			BatchPoster.APB_PostDate = ZDateTime.Now;
			AssertNoErrors("PostDate", BatchPoster.APB_PostDateInfo);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

			BatchPoster.APB_PostDate = ZDateTime.Now.AddDays(2);
			AssertHasError("PostDate", BatchPoster.APB_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			BatchPoster.APB_PostDate = ZDateTime.Now.AddDays(3);
			AssertNoErrors("PostDate", BatchPoster.APB_PostDateInfo);
		}

		void AssertPostDateMessage(int daysFromToday, APPaymentBatchPoster batchPoster, string message, bool isError)
		{
			batchPoster.APB_PostDate = ZDateTime.Now.AddDays(daysFromToday);
			string assertionMsg = string.Format("Should have {0} about post date is in {1}",
				string.IsNullOrEmpty(message) ? "no problem" : (isError ? "error" : "warning"),
				daysFromToday < 0 ? "past" : (daysFromToday == 0 ? "present" : "future"));
			bool result;
			if (!string.IsNullOrEmpty(message))
			{
				result = isError ? batchPoster.APB_PostDateInfo.HasError(message) : batchPoster.APB_PostDateInfo.HasWarning(message);
			}
			else
			{
				result = !batchPoster.APB_PostDateInfo.HasErrors() && !batchPoster.APB_PostDateInfo.HasWarnings();
			}
			Assert(assertionMsg, result);
		}

		#endregion

		#region APB_PaymentDate

		public void TestValidateDate()
		{
			BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_PaymentDate = ZDateTime.Empty;
			Assert("Should have an error", BatchPoster.APB_PaymentDateInfo.HasErrors());

			BatchPoster.APB_PaymentDate = ZDateTime.Now;
			AssertNoErrors("Should Be no errors", BatchPoster.APB_PaymentDateInfo);
		}

		#endregion

		#region APB_AB

		public void TestValidateBankAccountPK()
		{
			BatchPoster.APB_AB = ZGuid.Empty;
			Assert("Should have an error", BatchPoster.APB_ABInfo.HasErrors());
			BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			AssertNoErrors("Should Be no errors", BatchPoster.APB_ABInfo);

			BatchPostingHelper.TestBank.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			var ofxAccount = TestObjectCreator.CreateBankAccount("OFX", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			ofxAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			ofxAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;

			var expectedError1 = "This bank account is an E-Payment Account. Please set Payment Type to EPA - E-Payment.";
			var expectedError2 = "Bank Account is not an E-Payment Account.";

			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			AssertNoErrorContaining(BatchPoster.APB_ABInfo, expectedError1);
			AssertNoErrorContaining(BatchPoster.APB_ABInfo, expectedError2);

			BatchPoster.APB_AB = ofxAccount.PK;
			AssertHasError(BatchPoster.APB_ABInfo, expectedError1);
			AssertNoErrorContaining(BatchPoster.APB_ABInfo, expectedError2);

			BatchPoster.APB_PaymentType = ReceiptTypes.EPayment;
			BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			AssertNoErrorContaining(BatchPoster.APB_ABInfo, expectedError1);
			AssertHasError(BatchPoster.APB_ABInfo, expectedError2);

			BatchPoster.APB_AB = ofxAccount.PK;
			AssertNoErrorContaining(BatchPoster.APB_ABInfo, expectedError1);
			AssertNoErrorContaining(BatchPoster.APB_ABInfo, expectedError2);
		}

		public void TestValidateBankAccountPK_PaymentApprovalWithActiveDeal()
		{
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			AssertNoError(BatchPoster.APB_ABInfo, PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);

			var payment = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().First();
			TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Cancelled, payment);
			Factory.Save();
			BatchPoster.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertNoError(BatchPoster.APB_ABInfo, PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);

			var activeDeal = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued, payment);
			activeDeal.AED_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(10); // to ensure this is newer than previous deal.
			Factory.Save();
			BatchPoster.APB_AB = TestObjectCreator.AUDBankAccount2.PK;
			AssertHasError(BatchPoster.APB_ABInfo, PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);
		}

		#endregion

		#region ValidateBalance

		public void TestValidateBalance()
		{
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			var payment = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().First();

			AssertEquals(0m, BatchPoster.Balance);
			AssertEquals(94m, payment.AV_Amount);
			AssertEquals(94m, payment.AV_Calc_LocalAmount);
			AssertEquals(1m, payment.AV_PayExRate);
			AssertEquals("AUD", payment.AV_RX_NKPaymentCurrency);
			AssertNoError(BatchPoster.BalanceInfo, "The balance must equal 0");

			payment.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;
			payment.AV_Calc_LocalAmount = 100;

			AssertEquals(6m, BatchPoster.Balance);
			AssertEquals(70.5m, payment.AV_Amount);
			AssertEquals(100m, payment.AV_Calc_LocalAmount);
			AssertEquals(0.705m, payment.AV_PayExRate);
			AssertEquals("USD", payment.AV_RX_NKPaymentCurrency);
			AssertHasError(BatchPoster.BalanceInfo, "The balance must equal 0");

			using (BatchPoster.Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				payment.AV_Calc_LocalAmount = 101m;
				AssertEquals("Percondition: The balance not equal 0", 7m, BatchPoster.Balance);
				AssertEquals("Should be no errors", 0, BatchPoster.Notifications.ToList().Count);
			}

			payment.AV_Calc_LocalAmount = 94m;
			AssertEquals("Should be no errors", 0, BatchPoster.Notifications.ToList().Count);
		}

		public void TestValidateBalanceWhenPaymentApprovalIsCancelled()
		{
			var apInv = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, ZDateTime.Today);
			BatchPostingHelper.TestObjectCreator.CreateInvoiceLine(apInv, TestObjectCreator.AUD, 1m, 1000m, 0m, 0m);
			var transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(apInv);
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_ChequeOrReference = "0012345";
			BatchPoster.MatchTransactions();
			Factory.Save();

			AssertEquals(1, BatchPoster.PaymentApprovalCollection.Count);
			BatchPoster.PaymentApprovalCollection[0].AV_Amount = 2m;
			BatchPoster.PaymentApprovalCollection[0].AV_Status = PaymentApprovalStatus.Cancelled;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			BatchPoster = newFactory.Load<APPaymentBatchPoster>(BatchPoster.PK);
			BatchPoster.LoadPayments();
			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);
			AssertEquals(true, BatchPoster.PaymentForBinding.IsCancelled);
			AssertEquals(-998m, BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);

			BatchPoster.RunPreSaveValidation();
			AssertNoErrors("Balance not 0, should not have error", BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.BalanceInfo);
			AssertNoErrors("Balance not 0, should not have error", BatchPoster.BalanceInfo);
		}

		public void TestValidateBalanceWhenPaymentApprovalIsDeleted()
		{
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			var payment = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().First();
			Factory.Save();

			AssertNoError(BatchPoster.BalanceInfo, "The balance must equal 0");

			payment.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;
			payment.AV_Calc_LocalAmount = 100;
			var validator = (APPaymentBatchPosterValidation)BatchPoster.Validation;

			validator.ValidateBalance();
			AssertHasError(BatchPoster.BalanceInfo, "The balance must equal 0");

			var newFactry = new BusinessObjectFactory();
			var paymentInNewFactory = newFactry.Load<APPaymentApprovalWithoutAuthorisation>(payment.PK);
			paymentInNewFactory.Delete();
			newFactry.Save();

			AssertNoExceptionThrown("Expect no exception for deleted Payment.", () => validator.ValidateBalance());
			AssertNoError(BatchPoster.BalanceInfo, "The balance must equal 0");
		}

		#endregion

		#region ValidateOSOutstandingamount

		[TestDate(2012, 02, 01)]
		public void TestValidateOSOutstandingamount()
		{
			var creator = new TestObjectCreator(Factory);
			var newFactory = new BusinessObjectFactory();

			var aPInvoiceToTest = creator.CreateAPInvoice<APInvoice>("000001", creator.AUD, 1m, 90m, 0m, 0m, 90m, 0m, 0m, BatchPostingHelper.TestOrg);
			var aPInvoiceToTest2 = creator.CreateAPInvoice<APInvoice>("000002", creator.AUD, 1m, 50m, 0m, 0m, 50m, 0m, 0m, BatchPostingHelper.TestOrg);
			Factory.Save();

			var creatorForNewFactory = new TestObjectCreator(newFactory);
			var bankAccount = creatorForNewFactory.AUDBankAccount;
			var checkBook = creatorForNewFactory.AUDChequeBook;

			var payment1 = newFactory.NewWithValidTestData<ARPaymentApprovalWithAuthorisation>();
			payment1.AV_Amount = 40M;
			payment1.AV_AB = bankAccount.PK;
			var item1 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item1.A2_PaymentThisRun = 40M;
			item1.A2_AH = aPInvoiceToTest.PK;
			item1.A2_AV = payment1.PK;

			var payment2 = newFactory.NewWithValidTestData<ARPaymentApprovalWithAuthorisation>();
			payment2.AV_Amount = 40M;
			payment2.AV_AB = bankAccount.PK;
			payment2.AV_AK = checkBook.PK;
			payment2.AV_ChequeOrReference = "Test223";
			var item2 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item2.A2_PaymentThisRun = 40M;
			item2.A2_AH = aPInvoiceToTest2.PK;
			item2.A2_AV = payment2.PK;

			newFactory.Save();

			var transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(aPInvoiceToTest);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			BatchPoster.APB_AB = bankAccount.PK;
			BatchPoster.APB_AK = checkBook.PK;

			aPInvoiceToTest.AH_OutstandingAmount = 10m;

			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);
			IMatching transactionAsMatching = aPInvoiceToTest;

			AssertEquals("Should not contain the error", false, transactionAsMatching.OSOutstandingAmountInfo.HasErrors());

			aPInvoiceToTest.AH_OutstandingAmount = 0m;
			BatchPoster.RunPreSaveValidation();
			AssertEquals("Should contain the error", true, transactionAsMatching.OSOutstandingAmountInfo.HasErrors());
			Assert(transactionAsMatching.OSOutstandingAmountInfo.GetErrors().ContainsNotificationContaining(@"This transaction is fully paid by the following Unapproved payment(s):
Pay. Date   Bank Account   Check Book   Check/Reference   Amount
01-Feb-12  ZHSBCAUD               N/A                N/A          40.00 AUD"));
		}

		public void TestValidateOSOutstandingamountWhenTransactionCollectionISNull()
		{
			BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 1, 1, 2);

			BatchPoster = new BusinessObjectFactory().Load<APPaymentBatchPoster>(BatchPoster.PK);
			AssertNull("Percondition", BatchPoster.TransactionCollection);
			AssertNoExceptionThrown(() => BatchPoster.RunPreSaveValidationCore_ForTestOnly());
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			BatchPostingHelper.PrepareForBaseTest();
			BatchPostingHelper.BatchPoster.APB_AB = BatchPostingHelper.TestBank.PK;
			BatchPostingHelper.BatchPoster.APB_AK = BatchPostingHelper.TestCheques.PK;
		}

		APPaymentBatchPoster BatchPoster
		{
			get { return BatchPostingHelper.BatchPoster; }
			set { BatchPostingHelper.BatchPoster = value; }
		}

		TestObjectCreator TestObjectCreator
		{
			get { return BatchPostingHelper.TestObjectCreator; }
		}

		PaymentBatchPostingTestHelper BatchPostingHelper => fBatchPostingHelper ?? (fBatchPostingHelper = new PaymentBatchPostingTestHelper(Factory));
		PaymentBatchPostingTestHelper fBatchPostingHelper;
	}
}
