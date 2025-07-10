using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting.PaymentBatchChequeNumberAllocator;

namespace Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting.Testing
{
	[TestedType(typeof(PaymentBatchChequeNumberAllocator))]
	public class PaymentBatchChequeNumberAllocatorBaseTest : BaseAllocatorTest
	{
		public override void TestGetFactoriesWithAllocationCodeToBeCalledOnSaving_Core()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APPayment newPayment = newFactory.NewWithValidTestData<APPayment>();
			TransactionHeaderCollection payments = new TransactionHeaderCollection(newFactory);
			payments.Add(newPayment);

			var allocator = new PaymentBatchChequeNumberAllocator(payments, newPayment.Factory);
			ITransactionParticipant[] factories = allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving();
			AssertEquals("Factories array should contain 2 elements", 2, factories.Length);
			AssertEquals("First element is main Factory", newFactory, factories[0]);
			AssertEquals("Second element is SaveInTransactionWithRollBackAction ", typeof(SaveInTransactionWithRollBackAction), factories[1].GetType());

			allocator = new PaymentBatchChequeNumberAllocator(payments, newPayment.Factory);
			factories = allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(newFactory);
			AssertEquals("Factories array should contain 2 elements", 2, factories.Length);
			AssertEquals("First element is main Factory", newFactory, factories[0]);
			AssertEquals("Second element is SaveInTransactionWithRollBackAction ", typeof(SaveInTransactionWithRollBackAction), factories[1].GetType());
		}

		[TestDate(2010, 4, 30)]
		public void TestChequeNumbersGetAllocatedInPaymentCollectionSequence()
		{
			new AccountingPeriodTestHelper().PostPeriodsForEntireYear(ZDateTime.Now.Year);

			AccChequeBook testAutoPrintChequeBook = TestHelper.GetAutoPrintChequeBook(1, 11, 50);
			StmTemplate template = Factory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_DataContext, nameof(Constants.DataContext.Cheques)));
			testAutoPrintChequeBook.BankAccount.AB_SO_ChequeTemplate = template.PK;
			testAutoPrintChequeBook.BankAccount.AB_ChequeNumDigits = 2;
			Factory.Save();

			APPaymentApprovalWithoutAuthorisation newPayment6 = GetAutoAllocatePaymentApproval(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB, true, "6");
			APPaymentApprovalWithoutAuthorisation newPayment7 = GetAutoAllocatePaymentApproval(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB, true, "7");
			APPaymentApprovalWithoutAuthorisation newPayment8 = GetAutoAllocatePaymentApproval(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB, true, "8");
			APPaymentApprovalWithoutAuthorisation newPayment9 = GetAutoAllocatePaymentApproval(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB, true, "9");
			APPaymentApprovalWithoutAuthorisation newPayment10 = GetAutoAllocatePaymentApproval(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB, true, "10");
			APPaymentApprovalWithoutAuthorisation newPayment1 = GetAutoAllocatePaymentApproval(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB, true, "1");
			APPaymentApprovalWithoutAuthorisation newPayment2 = GetAutoAllocatePaymentApproval(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB, true, "2");
			APPaymentApprovalWithoutAuthorisation newPayment3 = GetAutoAllocatePaymentApproval(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB, true, "3");
			APPaymentApprovalWithoutAuthorisation newPayment4 = GetAutoAllocatePaymentApproval(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB, true, "4");
			APPaymentApprovalWithoutAuthorisation newPayment5 = GetAutoAllocatePaymentApproval(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB, true, "5");

			APPaymentApprovalWithoutAuthorisationCollection payments1 = new APPaymentApprovalWithoutAuthorisationCollection(Factory);
			payments1.Add(newPayment1);
			payments1.Add(newPayment2);
			payments1.Add(newPayment3);
			payments1.Add(newPayment4);
			payments1.Add(newPayment5);
			payments1.Add(newPayment6);
			payments1.Add(newPayment7);
			payments1.Add(newPayment8);
			payments1.Add(newPayment9);
			payments1.Add(newPayment10);

			Dictionary<ZGuid, ZString> paymentChequeNumberDictionary = new Dictionary<ZGuid, ZString>();
			int i = 10;
			foreach (var payment in payments1.Cast<APPaymentApprovalWithoutAuthorisation>().OrderBy(x => x.Header.OH_Code))
			{
				paymentChequeNumberDictionary.Add(payment.PK, (++i).ToString());
			}

			PaymentBatchChequeNumberAllocator allocator = new PaymentBatchChequeNumberAllocator(payments1, Factory);
			BusinessObjectFactory.SaveTogether(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving());

			ZQuery query = new ZQuery();
			query.OrderBy = StmPrintJobSchema.Constants.SP_Sequence;
			BusinessObject[] printJobs = Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 10 print jobs", 10, printJobs.Length);

			TransactionHeaderCollection paymentTransactions = new TransactionHeaderCollection(Factory);
			paymentTransactions.Load(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment));

			i = 10;
			foreach (BusinessObject printJob in printJobs)
			{
				APPayment payment = (APPayment)paymentTransactions.FindByPK((ZGuid)printJob[StmPrintJobSchema.SP_ParentGuid]);
				AssertEquals("Cheque Number should be sequential", (++i).ToString(), payment.AH_ChequeOrReference);
			}

			query = new ZQuery();
			var payments = Factory.Load<AccPaymentApproval>(query);
			foreach (var payment in payments)
			{
				AssertEquals("Cheque Number should be sequential", paymentChequeNumberDictionary[payment.PK], payment.AV_ChequeOrReference);
			}
		}

		public void TestPaymentsGetPrintedInChequeNumberSequence()
		{
			AccChequeBook testAutoPrintChequeBook = TestHelper.GetAutoPrintChequeBook(1, 11, 50);
			StmTemplate template = Factory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_DataContext, nameof(Constants.DataContext.Cheques)));
			testAutoPrintChequeBook.BankAccount.AB_SO_ChequeTemplate = template.PK;
			testAutoPrintChequeBook.BankAccount.AB_ChequeNumDigits = 2;
			Factory.Save();

			APPayment newPayment1 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment2 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment3 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment4 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment5 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment6 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment7 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment8 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment9 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment10 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment11 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment12 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment13 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment14 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment15 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment16 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment17 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment18 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment19 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			APPayment newPayment20 = GetAutoAllocatePayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);

			TransactionHeaderCollection payments = new TransactionHeaderCollection(Factory);
			payments.Add(newPayment1);
			payments.Add(newPayment2);
			payments.Add(newPayment3);
			payments.Add(newPayment4);
			payments.Add(newPayment5);
			payments.Add(newPayment6);
			payments.Add(newPayment7);
			payments.Add(newPayment8);
			payments.Add(newPayment9);
			payments.Add(newPayment10);
			payments.Add(newPayment11);
			payments.Add(newPayment12);
			payments.Add(newPayment13);
			payments.Add(newPayment14);
			payments.Add(newPayment15);
			payments.Add(newPayment16);
			payments.Add(newPayment17);
			payments.Add(newPayment18);
			payments.Add(newPayment19);
			payments.Add(newPayment20);

			newPayment1.AH_ChequeOrReference = "11";
			newPayment2.AH_ChequeOrReference = "14";
			newPayment3.AH_ChequeOrReference = "13";
			newPayment4.AH_ChequeOrReference = "12";
			newPayment5.AH_ChequeOrReference = "15";
			newPayment6.AH_ChequeOrReference = "17";
			newPayment7.AH_ChequeOrReference = "16";
			newPayment8.AH_ChequeOrReference = "19";
			newPayment9.AH_ChequeOrReference = "18";
			newPayment10.AH_ChequeOrReference = "21";
			newPayment11.AH_ChequeOrReference = "23";
			newPayment12.AH_ChequeOrReference = "22";
			newPayment13.AH_ChequeOrReference = "20";
			newPayment14.AH_ChequeOrReference = "27";
			newPayment15.AH_ChequeOrReference = "25";
			newPayment16.AH_ChequeOrReference = "26";
			newPayment17.AH_ChequeOrReference = "24";
			newPayment18.AH_ChequeOrReference = "28";
			newPayment19.AH_ChequeOrReference = "29";
			newPayment20.AH_ChequeOrReference = "30";

			payments.Sort("PK"); // pre-allocate the cheque numbers and then screw up the sequence

			PaymentBatchChequeNumberAllocator allocator = new PaymentBatchChequeNumberAllocator(payments, Factory);
			BusinessObjectFactory.SaveTogether(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving());

			ZQuery query = new ZQuery();
			query.OrderBy = StmPrintJobSchema.Constants.SP_Sequence;
			BusinessObject[] printJobs = Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 20 print jobs", 20, printJobs.Length);

			int index = 11;
			foreach (BusinessObject printJob in printJobs)
			{
				APPayment payment = (APPayment)payments.FindByPK((ZGuid)printJob[StmPrintJobSchema.SP_ParentGuid]);
				AssertEquals("Cheque Number should be sequential", index.ToString(), payment.AH_ChequeOrReference);
				index++;
			}
		}

		public APPayment GetAutoAllocatePayment(ZGuid chequeBookPK, ZGuid bankAccountPK)
		{
			APPayment newPayment = Factory.NewWithValidTestData<APPayment>();
			OrgHeader testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			newPayment.AH_OH = testOrgHeader.PK;
			newPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			newPayment.AH_AB = bankAccountPK;
			newPayment.ChequeBook = chequeBookPK;
			newPayment.AH_TransactionNum = ZString.Empty;
			return newPayment;
		}

		public APPaymentApprovalWithoutAuthorisation GetAutoAllocatePaymentApproval(ZGuid chequeBookPK, ZGuid bankAccountPK, bool partOfBatchPosting, string matchTransactionNumber)
		{
			APPaymentApprovalWithoutAuthorisation newPayment = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			OrgHeader testOrgHeader = TestHelper.TestObjectCreator.TestOrganisation;
			newPayment.AV_OH = testOrgHeader.PK;
			newPayment.AV_PaymentType = ReceiptTypes.Cheque;
			newPayment.AV_AB = bankAccountPK;
			newPayment.AV_AK = chequeBookPK;
			newPayment.AV_Amount = 100m;
			newPayment.InitializeForPaymentBatch(() => partOfBatchPosting);
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.FillPaymentMatchTransactions(matchTransactionNumber, typeof(APInvoice), testOrgHeader, newPayment, 100m);
			return newPayment;
		}

		public override void TestCanContinueWithAllocation()
		{
			TransactionHeaderCollection transactions = TestHelper.GetCollectionWith2PaymentsReadyToAutoPrint();
			TestAllocator = new DummyPaymentBatchChequeNumberAllocator(transactions, Factory);
			Assert("Allocation should be enabled by default", ((IChequeNumberAutoAllocation)transactions[0]).IsAutoAllocationEnabled);
			Assert("Allocation should be enabled by default", ((IChequeNumberAutoAllocation)transactions[1]).IsAutoAllocationEnabled);
			Assert("Allocation should be enabled by default", TestAllocator.CanContinueWithAllocation_ForTestOnly);
			Assert("Allocation should not be performed on payment yet", !((IChequeNumberAutoAllocation)transactions[0]).IsAllocationPerformed);
			Assert("Allocation should not be performed on payment yet", !((IChequeNumberAutoAllocation)transactions[1]).IsAllocationPerformed);
			((IChequeNumberAutoAllocation)transactions[1]).AssignChequeNumber("1");
			Assert("Allocation should be disabled as one of the payments has cheque number set", !TestAllocator.CanContinueWithAllocation_ForTestOnly);
			((IChequeNumberAutoAllocation)transactions[1]).AssignChequeNumber("");
			Assert("Allocation should be enabled again", TestAllocator.CanContinueWithAllocation_ForTestOnly);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Allocation should be performed", 1, TestAllocator.AllocationCalled_Counter);
			Assert("Allocation should be performed on payment", ((IChequeNumberAutoAllocation)transactions[0]).IsAllocationPerformed);
			Assert("Allocation should be performed on payment", ((IChequeNumberAutoAllocation)transactions[1]).IsAllocationPerformed);
			Assert("Allocation should be disabled now", !TestAllocator.CanContinueWithAllocation_ForTestOnly);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Another Factory.Save call won't start another auto allocation", 1, TestAllocator.AllocationCalled_Counter);
		}

		public override void TestCanContinueWithPrinting()
		{
			TransactionHeaderCollection transactions = TestHelper.GetCollectionWith2PaymentsReadyToAutoPrint();
			TestAllocator = new DummyPaymentBatchChequeNumberAllocator(transactions, Factory);
			Assert("Printing should be enabled by default", TestAllocator.CanContinueWithPrinting_ForTestOnly);
			Assert("Printing should not be performed on payment yet", !((IChequeNumberAutoAllocation)transactions[0]).ChequeIsAutoPrinted);
			Assert("Printing should not be performed on payment yet", !((IChequeNumberAutoAllocation)transactions[1]).ChequeIsAutoPrinted);
			((IChequeNumberAutoAllocation)transactions[1]).ChequeIsAutoPrinted = ZBool.True;
			Assert("Printing should be disabled as one of the payments has the printing flag set", !TestAllocator.CanContinueWithPrinting_ForTestOnly);
			((IChequeNumberAutoAllocation)transactions[1]).ChequeIsAutoPrinted = ZBool.False;
			Assert("Printing should be enabled again", TestAllocator.CanContinueWithPrinting_ForTestOnly);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Printing should be performed", 1, TestAllocator.PrintingCalled_Counter);
			Assert("Printing should be performed on payment", ((IChequeNumberAutoAllocation)transactions[0]).ChequeIsAutoPrinted);
			Assert("Printing should be performed on payment", ((IChequeNumberAutoAllocation)transactions[1]).ChequeIsAutoPrinted);
			Assert("Printing should be disabled now", !TestAllocator.CanContinueWithPrinting_ForTestOnly);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Another Factory.Save call won't start another auto printing", 1, TestAllocator.PrintingCalled_Counter);
		}

		public override void TestMultipleFactorySavesWillNotCauseANewAllocation()
		{
			TransactionHeaderCollection transactions = TestHelper.GetCollectionWith2PaymentsReadyToAutoPrint();
			TestAllocator = new DummyPaymentBatchChequeNumberAllocator(transactions, Factory);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Cheque number should be autoallocated on payment", "1", transactions[0].AH_ChequeOrReference);
			AssertEquals("Cheque number should be autoallocated on payment", "2", transactions[1].AH_ChequeOrReference);
			Assert("AutoAllocation should be performed", ((IChequeNumberAutoAllocation)transactions[0]).IsAllocationPerformed);
			Assert("AutoAllocation should be performed", ((IChequeNumberAutoAllocation)transactions[1]).IsAllocationPerformed);
			Assert("AutoPrinting should be performed", ((IChequeNumberAutoAllocation)transactions[0]).ChequeIsAutoPrinted);
			Assert("AutoPrinting should be performed", ((IChequeNumberAutoAllocation)transactions[1]).ChequeIsAutoPrinted);
			Assert("Auto printing should be performed", TestAllocator.ChequeWasAutoPrinted);
			AssertEquals("There should be 1 printer passed for auto printing", 1, TestAllocator.PrintersPassedForAutoPrinting.Count);
			AssertEquals("Printer passed for printing", TestAutoPrintChequeBook.AK_SQ, TestAllocator.PrintersPassedForAutoPrinting[0]);
			Assert("Payment is in database", transactions[0].IsInDatabase);
			Assert("Payment is in database", transactions[1].IsInDatabase);
			AssertEquals("Printing called only once", 1, TestAllocator.PrintingCalled_Counter);
			AssertEquals("Allocation called only once", 1, TestAllocator.AllocationCalled_Counter);

			TestAutoPrintChequeBook.Reload();
			AssertEquals("CurrentNo should change on the cheque book", 3m, TestAutoPrintChequeBook.AK_CurrentNo);
			Assert("Cheque book is sitll active", TestAutoPrintChequeBook.AK_IsActive);

			DummyPaymentBatchChequeNumberAllocator anotherAllocator = new DummyPaymentBatchChequeNumberAllocator(transactions, Factory);
			BusinessObjectFactory.SaveTogether(anotherAllocator.GetFactoriesForTest());

			AssertEquals("Printing called only once", 1, TestAllocator.PrintingCalled_Counter);
			AssertEquals("Allocation called only once", 1, TestAllocator.AllocationCalled_Counter);
			AssertEquals("Printing should not be called on another allocator", 0, anotherAllocator.PrintingCalled_Counter);
			AssertEquals("Allocation should not be called on another allocator", 0, anotherAllocator.AllocationCalled_Counter);
			AssertEquals("Cheque number should be autoallocated on payment", "1", transactions[0].AH_ChequeOrReference);
			AssertEquals("Cheque number should be autoallocated on payment", "2", transactions[1].AH_ChequeOrReference);

			TestAutoPrintChequeBook.Reload();
			AssertEquals("CurrentNo should change on the cheque book", 3m, TestAutoPrintChequeBook.AK_CurrentNo);
			Assert("Cheque book is sitll active", TestAutoPrintChequeBook.AK_IsActive);
		}

		public void TestAutoAllocationAndPrintChequesErrorInTransaction()
		{
			var chequeBook = TestHelper.GetAutoPrintChequeBook(1, 1, 10);
			chequeBook.AK_Desc = "TestAutoAllocationAndPrintCheques";
			Factory.Save();

			var payments = new TransactionHeaderCollection(Factory);
			payments.Add(GetAutoAllocatePayment(chequeBook.PK, chequeBook.AK_AB));
			var allocator = new PaymentBatchChequeNumberAllocator(payments, Factory);

			var ex = AssertExceptionThrown<ZCannotSaveException>("throws", () => BusinessObjectFactory.SaveTogether(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving()));
			AssertEquals("Auto cheque printing failed. Please check Auto cheque printing settings. Testing Auto Print Cheques failure.", ex.Message);
		}

		#region Implementation

		DummyPaymentBatchChequeNumberAllocator TestAllocator;
		AccChequeBook TestAutoPrintChequeBook
		{
			get
			{
				return TestHelper.TestAutoPrintChequeBook;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PaymentBatchChequeNumberAllocator(null, Factory);
		}

		#endregion
	}
}
