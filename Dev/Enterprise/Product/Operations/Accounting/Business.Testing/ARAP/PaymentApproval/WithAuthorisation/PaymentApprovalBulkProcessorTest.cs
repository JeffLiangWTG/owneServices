using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(PaymentApprovalBulkProcessor))]
	public class PaymentApprovalBulkProcessorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPostPaymentApprovalsDoesNotLoadTransactionsFromDB()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = 6;
			AccChequeBook testAutoPrintChequeBook = GetAutoPrintChequeBook();
			testAutoPrintChequeBook.AK_StartNo = 1;
			testAutoPrintChequeBook.AK_LastNo = 4;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 6;
			testChequeBook.AK_CurrentNo = 5;
			OrgHeader testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			testChequeBook.AK_AB = testBank.PK;

			PaymentApprovalForTest approval1 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval1", ReceiptTypes.Cheque, testAutoPrintChequeBook.AK_AB, testAutoPrintChequeBook, 100M);
			PaymentApprovalForTest approval2 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval2", ReceiptTypes.Cheque, testAutoPrintChequeBook.AK_AB, testAutoPrintChequeBook, 100M);
			PaymentApprovalForTest approval3 = GetNewPaymentApproval(PaymentApprovalStatus.AwaitingApproval, "Approval3", ReceiptTypes.Cheque, testAutoPrintChequeBook.AK_AB, testAutoPrintChequeBook, 100M);

			var approvals = new List<PaymentApprovalBase>() { approval1, approval2, approval3 };

			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
			PaymentApprovalBulkProcessor processor = new PaymentApprovalBulkProcessor(Factory);

			int beforeHit = Factory.GetTableHitCount(Enterprise.ZArchitecture.Schema.AccTransactionHeaderSchema.Constants.TableName);
			processor.PostPaymentApprovals(approvals);
			int afterHit = Factory.GetTableHitCount(Enterprise.ZArchitecture.Schema.AccTransactionHeaderSchema.Constants.TableName);
			AssertEquals("AccTransactionHeader table should not be loaded", beforeHit, afterHit);
		}

		public void TestGetCheckNumberUpdatedMessage()
		{
			AssertEquals("Check number has been updated and check was not reprinted. Was 1 Now 2", PaymentApprovalBulkProcessor.GetCheckNumberUpdatedMessage("1", "2", false));
			AssertEquals("Check number has been updated during reprinting. Was 1 Now 2", PaymentApprovalBulkProcessor.GetCheckNumberUpdatedMessage("1", "2", true));
		}

		public void TestPopulateChequeNumbersOnPaymentApprovals()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = 2;
			AccChequeBook testAutoPrintChequeBook = GetAutoPrintChequeBook();
			testAutoPrintChequeBook.AK_StartNo = 1;
			testAutoPrintChequeBook.AK_LastNo = 2;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 3;
			testChequeBook.AK_AB = testBank.PK;
			Factory.Save();

			PaymentApprovalForTest approval1 = GetNewPaymentApproval(PaymentApprovalStatus.AwaitingApproval, "Approval1", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			PaymentApprovalForTest approval2 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval2", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			PaymentApprovalForTest approval3 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval3");
			// has errors
			PaymentApprovalForTest approval4 = GetNewPaymentApproval(PaymentApprovalStatus.Posted, "Approval4", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			PaymentApprovalForTest approval5 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval5", ReceiptTypes.Cheque, testAutoPrintChequeBook.AK_AB, testAutoPrintChequeBook, 100M);
			Assert("Auto Allocation should be enabled on Approval5", ((IChequeNumberAutoAllocation)approval5).IsAutoAllocationEnabled);
			PaymentApprovalForTest approval6 = GetNewPaymentApproval(PaymentApprovalStatus.AwaitingApproval, "Approval6", ReceiptTypes.Cheque, testAutoPrintChequeBook.AK_AB, testAutoPrintChequeBook, 100M);
			Assert("Auto Allocation should be enabled on Approval6", ((IChequeNumberAutoAllocation)approval6).IsAutoAllocationEnabled);

			var approvals = new List<PaymentApprovalBase>() { approval1, approval2, approval3, approval4, approval5, approval6 };

			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
			PaymentApprovalBulkProcessor processor = new PaymentApprovalBulkProcessor(Factory);
			processor.PopulateChequeNumbersOnPaymentApprovals(approvals);

			AssertEquals("Approvals Successfully Processed", 2, processor.ApprovalsSuccessfullyProcessed_ForTestOnly.Count);
			Assert("Approval1 is Successfully Processed", processor.ApprovalsSuccessfullyProcessed_ForTestOnly.Contains(approval1.PK));
			Assert("Approval2 is Successfully Processed", processor.ApprovalsSuccessfullyProcessed_ForTestOnly.Contains(approval2.PK));
			Assert("Approval3 is Successfully Processed", !processor.ApprovalsSuccessfullyProcessed_ForTestOnly.Contains(approval3.PK));
			Assert("Approval4 is Successfully Processed", !processor.ApprovalsSuccessfullyProcessed_ForTestOnly.Contains(approval4.PK));

			AssertEquals("Current chequeNo on cheque book should change", 3m, testChequeBook.AK_CurrentNo);
			AssertEquals("Current chequeNo on auto allocate cheque book should not change", 1m, testAutoPrintChequeBook.AK_CurrentNo);

			AssertResultsForPaymentApproval("Approval1", approval1, false, true);
			AssertEquals("Approval1 AV_ChequeOrReference", "01", approval1.AV_ChequeOrReference);
			AssertResultsForPaymentApproval("Approval2", approval2, false, true);
			AssertEquals("Approval2 AV_ChequeOrReference", "02", approval2.AV_ChequeOrReference);
			AssertResultsForPaymentApproval("Approval3", approval3, false, false); // HasError
			Assert("Approval3 AV_ChequeOrReference", approval3.AV_ChequeOrReference.IsEmpty);
			AssertResultsForPaymentApproval("Approval4", approval4, false, false); // IsPosted
			Assert("Approval4 AV_ChequeOrReference", approval4.AV_ChequeOrReference.IsEmpty);
			AssertResultsForPaymentApproval("Approval5", approval5, false, false); // IsPosted
			Assert("Approval5 AV_ChequeOrReference", approval5.AV_ChequeOrReference.IsEmpty);
			AssertResultsForPaymentApproval("Approval6", approval6, false, false); // IsPosted
			Assert("Approval6 AV_ChequeOrReference", approval6.AV_ChequeOrReference.IsEmpty);

			AssertEquals("There should not be any DB hits", 0, Factory_SavingCounter);
		}

		public void TestPostPaymentApprovals_WithEmptyChequeNumber()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = 6;
			AccChequeBook testAutoPrintChequeBook = GetAutoPrintChequeBook();
			testAutoPrintChequeBook.AK_StartNo = 1;
			testAutoPrintChequeBook.AK_LastNo = 4;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 6;
			testChequeBook.AK_CurrentNo = 5;
			OrgHeader testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			testChequeBook.AK_AB = testBank.PK;

			PaymentApprovalForTest approval1 = GetNewPaymentApproval(PaymentApprovalStatus.AwaitingApproval, "Approval1", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			PaymentApprovalForTest approval2 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval2", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			PaymentApprovalForTest approval3 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval3");
			// has errors
			PaymentApprovalForTest approval4 = GetNewPaymentApproval(PaymentApprovalStatus.Posted, "Approval4", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			PaymentApprovalForTest approval5 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval5", ReceiptTypes.Cheque, testAutoPrintChequeBook.AK_AB, testAutoPrintChequeBook, 100M);
			Assert("Auto Allocation should be enabled on Approval5", ((IChequeNumberAutoAllocation)approval5).IsAutoAllocationEnabled);
			TestObjectCreator.FillPaymentMatchTransactions("5", typeof(APInvoice), TestOrgHeader, approval5, 100M);

			var approvals = new List<PaymentApprovalBase>() { approval1, approval2, approval3, approval4, approval5 };

			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
			PaymentApprovalBulkProcessor processor = new PaymentApprovalBulkProcessor(Factory);
			processor.PostPaymentApprovals(approvals);

			AssertEquals("Approvals Successfully Processed", 1, processor.ApprovalsSuccessfullyProcessed_ForTestOnly.Count);

			AssertResultsForPaymentApproval("Approval1", approval1, false, false); // AwaitingApproval
			AssertResultsForPaymentApproval("Approval2", approval2, false, false); // Empty Cheque Number
			AssertResultsForPaymentApproval("Approval3", approval3, false, false); // HasError
			AssertResultsForPaymentApproval("Approval4", approval4, false, false); // IsPosted
			AssertResultsForPaymentApproval("Approval5", approval5, true, false); // AutoAllocated

			AssertEquals(1, processor.PaymentsForAutoAllocation.Count);
			Assert(processor.PaymentsForAutoAllocation.Contains(approval5.NewPayment));
			AssertEquals(0, processor.OtherPayments.Count);
			AssertEquals("There should not be any DB hits", 0, Factory_SavingCounter);
		}

		public void TestPostPaymentApprovals_WithFilledChequeNumber()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = 6;
			AccChequeBook testAutoPrintChequeBook = GetAutoPrintChequeBook();
			testAutoPrintChequeBook.AK_StartNo = 1;
			testAutoPrintChequeBook.AK_LastNo = 4;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 6;
			testChequeBook.AK_CurrentNo = 5;
			OrgHeader testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			testChequeBook.AK_AB = testBank.PK;

			PaymentApprovalForTest approval1 = GetNewPaymentApproval(PaymentApprovalStatus.AwaitingApproval, "Approval1", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			approval1.AV_ChequeOrReference = "000001";

			PaymentApprovalForTest approval2 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval2", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			approval2.AV_ChequeOrReference = "000002";
			TestObjectCreator.FillPaymentMatchTransactions("2", typeof(APInvoice), TestOrgHeader, approval2, 100M);

			PaymentApprovalForTest approval3 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval3");
			// has errors
			approval3.AV_ChequeOrReference = "000003";

			PaymentApprovalForTest approval4 = GetNewPaymentApproval(PaymentApprovalStatus.Posted, "Approval4", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			approval4.AV_ChequeOrReference = "000004";

			PaymentApprovalForTest approval5 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval5", ReceiptTypes.Cheque, testAutoPrintChequeBook.AK_AB, testAutoPrintChequeBook, 100M);
			Assert("Auto Allocation should be enabled on Approval5", ((IChequeNumberAutoAllocation)approval5).IsAutoAllocationEnabled);
			TestObjectCreator.FillPaymentMatchTransactions("5", typeof(APInvoice), TestOrgHeader, approval5, 100M);

			var approvals = new List<PaymentApprovalBase>() { approval1, approval2, approval3, approval4, approval5 };

			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
			PaymentApprovalBulkProcessor processor = new PaymentApprovalBulkProcessor(Factory);
			processor.PostPaymentApprovals(approvals);
			AssertEquals("There should not be any DB hits yet", 0, Factory_SavingCounter);
			processor.SaveChanges();
			AssertEquals("There should not be only 1 DB hit", 1, Factory_SavingCounter);

			AssertEquals("Approvals Successfully Processed", 2, processor.ApprovalsSuccessfullyProcessed_ForTestOnly.Count);
			AssertEquals("CurrentNo should not change on the chequebook", 5m, testChequeBook.AK_CurrentNo);
			AssertEquals("CurrentNo should not change on the chequebook", 1m, testAutoPrintChequeBook.AK_CurrentNo);

			AssertResultsForPaymentApproval("Approval1", approval1, false, false); // AwaitingApproval
			AssertResultsForPaymentApproval("Approval2", approval2, true, false);
			AssertEquals("Approval2 AV_ChequeOrReference", "000002", approval2.AV_ChequeOrReference);
			AssertResultsForPaymentApproval("Approval3", approval3, false, false); // HasError
			AssertResultsForPaymentApproval("Approval4", approval4, false, false); // IsPosted
			AssertResultsForPaymentApproval("Approval5", approval5, true, false);
			Assert("AV_ChequeOrReference on Approval5 should remain empty", approval5.AV_ChequeOrReference.IsEmpty);

			AssertEquals(1, processor.PaymentsForAutoAllocation.Count);
			Assert(processor.PaymentsForAutoAllocation.Contains(approval5.NewPayment));
			AssertEquals(1, processor.OtherPayments.Count);
			Assert(processor.OtherPayments.Contains(approval2.NewPayment));
		}

		public void TestPopulateChequeNumberAndPostPaymentApprovals()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = 6;
			AccChequeBook testAutoPrintChequeBook = GetAutoPrintChequeBook();
			testAutoPrintChequeBook.AK_StartNo = 1;
			testAutoPrintChequeBook.AK_LastNo = 4;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 7;
			testChequeBook.AK_CurrentNo = 5;
			testChequeBook.AK_AB = testBank.PK;
			Factory.Save();

			PaymentApprovalForTest approval1 = GetNewPaymentApproval(PaymentApprovalStatus.AwaitingApproval, "Approval1", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			PaymentApprovalForTest approval2 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval2", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			TestObjectCreator.FillPaymentMatchTransactions("2", typeof(APInvoice), TestOrgHeader, approval2, 100M);
			PaymentApprovalForTest approval3 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval3");
			// has errors
			PaymentApprovalForTest approval4 = GetNewPaymentApproval(PaymentApprovalStatus.Posted, "Approval4", ReceiptTypes.Cheque, testBank.PK, testChequeBook, 100M);
			PaymentApprovalForTest approval5 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval5", ReceiptTypes.Cheque, testAutoPrintChequeBook.AK_AB, testAutoPrintChequeBook, 100M);
			Assert("Auto Allocation should be enabled on Approval5", ((IChequeNumberAutoAllocation)approval5).IsAutoAllocationEnabled);
			TestObjectCreator.FillPaymentMatchTransactions("5", typeof(APInvoice), TestOrgHeader, approval5, 100M);
			PaymentApprovalForTest approval6 = GetNewPaymentApproval(PaymentApprovalStatus.AwaitingApproval, "Approval6", ReceiptTypes.Cheque, testAutoPrintChequeBook.AK_AB, testAutoPrintChequeBook, 100M);
			Assert("Auto Allocation should be enabled on Approval6", ((IChequeNumberAutoAllocation)approval6).IsAutoAllocationEnabled);

			var approvals = new List<PaymentApprovalBase>() { approval1, approval2, approval3, approval4, approval5, approval6 };

			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
			PaymentApprovalBulkProcessor processor = new PaymentApprovalBulkProcessor(Factory);
			processor.PopulateChequeNumberAndPostPaymentApprovals(approvals);
			AssertEquals("There should not be any DB hits yet", 0, Factory_SavingCounter);
			processor.SaveChanges();
			AssertEquals("There should not be only 1 DB hit", 1, Factory_SavingCounter);

			AssertEquals("Approvals Successfully Processed", 3, processor.ApprovalsSuccessfullyProcessed_ForTestOnly.Count);
			AssertEquals("CurrentNo should change on the chequebook", 7m, testChequeBook.AK_CurrentNo);
			AssertEquals("CurrentNo should not change on the chequebook", 1m, testAutoPrintChequeBook.AK_CurrentNo);

			AssertResultsForPaymentApproval("Approval1", approval1, false, true); // AwaitingApproval
			AssertEquals("Approval2 AV_ChequeOrReference", "000005", approval1.AV_ChequeOrReference);
			AssertResultsForPaymentApproval("Approval2", approval2, true, true);
			AssertEquals("Approval2 AV_ChequeOrReference", "000006", approval2.AV_ChequeOrReference);
			AssertResultsForPaymentApproval("Approval3", approval3, false, false); // HasError
			Assert("Approval3 AV_ChequeOrReference", approval3.AV_ChequeOrReference.IsEmpty);
			AssertResultsForPaymentApproval("Approval4", approval4, false, false); // IsPosted
			Assert("Approval4 AV_ChequeOrReference", approval4.AV_ChequeOrReference.IsEmpty);
			AssertResultsForPaymentApproval("Approval5", approval5, true, false);
			Assert("AV_ChequeOrReference on Approval5 should remain empty", approval5.AV_ChequeOrReference.IsEmpty);
			AssertResultsForPaymentApproval("Approval6", approval6, false, false);
			Assert("AV_ChequeOrReference on Approval6 should remain empty", approval6.AV_ChequeOrReference.IsEmpty);

			AssertEquals(1, processor.PaymentsForAutoAllocation.Count);
			Assert(processor.PaymentsForAutoAllocation.Contains(approval5.NewPayment));
			AssertEquals(1, processor.OtherPayments.Count);
			Assert(processor.OtherPayments.Contains(approval2.NewPayment));
		}

		[TestDate(2014, 1, 1)]
		public void TestPostDateIsChangedOnSaving()
		{
			PeriodManagement.PeriodManager periodManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2014, 1, 1));
			ZDateTime creationTime = new ZDateTime(TestDateAttribute.Date);
			ZDateTime editTime = creationTime.AddDays(1).AddHours(2).AddMinutes(3);

			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = 6;
			AccChequeBook testAutoPrintChequeBook = GetAutoPrintChequeBook();
			testAutoPrintChequeBook.AK_StartNo = 1;
			testAutoPrintChequeBook.AK_LastNo = 4;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 6;
			testChequeBook.AK_CurrentNo = 5;
			OrgHeader testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			testChequeBook.AK_AB = testBank.PK;

			PaymentApprovalForTest approval1 = GetNewPaymentApproval(PaymentApprovalStatus.FullyApproved, "Approval1", ReceiptTypes.Cheque, testAutoPrintChequeBook.AK_AB, testAutoPrintChequeBook, 100M);
			var approvals = new List<PaymentApprovalBase>() { approval1 };
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestOrgHeader, approval1, 100M);
			approval1.MatchingBaseObject.MatchDate = editTime;

			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
			PaymentApprovalBulkProcessor processor = new PaymentApprovalBulkProcessor(Factory);

			AssertEquals("Precondition: AV_PostDate", creationTime, approval1.AV_PostDate);

			TestDateAttribute.Date = editTime.ToDateTime();
			AssertNotEquals("Approval1 should not be posted", PaymentApprovalStatus.Posted, approval1.AV_Status);
			processor.PostPaymentApprovals(approvals);

			AssertEquals("AV_PostDate", editTime, approval1.AV_PostDate);
			AssertNoNotifications("Approval1 post date should have changed to todays date", approval1.AV_PostDateInfo);
			AssertEquals("Approval1 should be posted", PaymentApprovalStatus.Posted, approval1.AV_Status);
		}

		#region Implementation

		protected override void SetUp()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			base.SetUp();

			TestOrgHeader = new TestObjectCreator(Factory).AALSHI;
			TestOrgHeader.CompanyData.OB_IsCreditor = true;
			Factory.Save();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		public TestObjectCreator TestObjectCreator;

		void Factory_Saving(BusinessObjectFactory factory)
		{
			Factory_SavingCounter++;
		}
		ZInt Factory_SavingCounter;

		AccChequeBook GetAutoPrintChequeBook()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		OrgHeader TestOrgHeader;

		PaymentApprovalForTest GetNewPaymentApproval(ZString status, ZString paymentComment)
		{
			PaymentApprovalForTest approval = Factory.New<PaymentApprovalForTest>();
			approval.AV_OH = TestOrgHeader.PK;
			approval.AV_PaymentComment = paymentComment;
			approval.AV_Status = status;

			return approval;
		}

		PaymentApprovalForTest GetNewPaymentApproval(ZString status, ZString paymentComment, ZString paymentType, ZGuid testBankPK, AccChequeBook testChequeBook, ZDecimal aV_Amount)
		{
			PaymentApprovalForTest approval = Factory.New<PaymentApprovalForTest>();
			approval.AV_OH = TestOrgHeader.PK;
			approval.AV_PaymentType = paymentType;
			approval.AV_AB = testBankPK;
			approval.AV_AK = testChequeBook.PK;
			approval.AV_PaymentComment = paymentComment;
			approval.AV_Amount = aV_Amount;
			approval.AV_Status = status;

			return approval;
		}

		void AssertResultsForPaymentApproval(ZString description, PaymentApprovalForTest approval, bool posted, bool chequeNoPopulated)
		{
			AssertEquals("Validation should be run for " + description, true, approval.RunPrePostingValidationHasBeenCalled);
			AssertEquals("CreateNewPayment should be run for " + description, posted, approval.PaymentCreatedHasBeenCalled);
			//AssertEquals("PopulateChequeNumberFromCheque should be run for " + Description, ChequeNoPopulated, Approval.PopulateChequeNumberFromChequeBookHasBeenCalled);
		}

		public class PaymentApprovalForTest : APPaymentApprovalWithAuthorisation
		{
			public PaymentApprovalForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool PopulateChequeNumberFromChequeBookHasBeenCalled
			{
				get { return fPopulateChequeNumberFromChequeBookHasBeenCalled; }
			}

			bool fPopulateChequeNumberFromChequeBookHasBeenCalled;

			protected override void PopulateChequeNumberFromChequeBookCore()
			{
				if (AV_ChequeOrReference.IsEmpty)
				{
					fPopulateChequeNumberFromChequeBookHasBeenCalled = true;
				}
				base.PopulateChequeNumberFromChequeBookCore();
			}

			public bool PaymentCreatedHasBeenCalled
			{
				get { return fPaymentCreatedHasBeenCalled; }
			}

			bool fPaymentCreatedHasBeenCalled;

			protected override void CreateNewPaymentCore()
			{
				fPaymentCreatedHasBeenCalled = true;
				base.CreateNewPaymentCore();
			}

			public bool RunPrePostingValidationHasBeenCalled
			{
				get { return fRunPrePostingValidationHasBeenCalled; }
			}

			bool fRunPrePostingValidationHasBeenCalled;

			public override void RunPrePostingValidation()
			{
				fRunPrePostingValidationHasBeenCalled = true;
				base.RunPrePostingValidation();
			}
		}

		#endregion
	}
}
