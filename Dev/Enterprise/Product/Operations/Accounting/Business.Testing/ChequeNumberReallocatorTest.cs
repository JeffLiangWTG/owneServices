using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(ChequeNumberReallocator))]
	public class ChequeNumberReallocatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCaption()
		{
			var transactions = new[] { Factory.New<APPayment>(), Factory.New<APPayment>() };
			ChequeNumberReallocator reallocator = new ChequeNumberReallocator(Factory, TestObjectCreator.CreateChequeBook(10, 10, 20, TestObjectCreator.AUDBankAccount), transactions, false);
			AssertEquals("2 checks will be renumbered.\r\nBelow, please enter / confirm the number of the first check number to be allocated", reallocator.Caption);
			reallocator = new ChequeNumberReallocator(Factory, TestObjectCreator.CreateChequeBook(10, 10, 20, TestObjectCreator.AUDBankAccount), transactions, true);
			AssertEquals("2 checks will be reprinted.\r\nBelow, please enter / confirm the number of the first check in the printer", reallocator.Caption);
		}

		public void TestCheckIsValidForReallocationOfCheckNumbers()
		{
			AccChequeBook checkbook = Factory.NewWithValidTestData<AccChequeBook>();
			checkbook.AK_AutoPrintCheque = true;
			var transactions = new List<TransactionHeader>();
			APPayment apPayment = Factory.NewWithValidTestData<APPayment>();
			transactions.Add(apPayment);
			Assert(!ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			apPayment.ChequeBook = checkbook.PK;
			Assert(ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			ARPayment arPayment = Factory.NewWithValidTestData<ARPayment>();
			arPayment.AH_IsCancelled = true;
			arPayment.ChequeBook = checkbook.PK;
			transactions.Add(arPayment);
			Assert(!ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			arPayment.AH_IsCancelled = false;
			Assert(ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			DirectPayment directPayment = Factory.NewWithValidTestData<DirectPayment>();
			directPayment.AH_ReceiptType = ReceiptTypes.Cash;
			directPayment.ChequeBookPK = checkbook.PK;
			transactions.Add(directPayment);
			Assert(!ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			directPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			Assert(ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.ReceiptPaymentAK_AB = checkbook.PK;
			transactions.Add(apInvoice);
			Assert(!ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			transactions.Remove(apInvoice);
			Assert(ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			checkbook.AK_AutoPrintCheque = false;
			Assert(!ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			checkbook.AK_AutoPrintCheque = true;
			Assert(ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			apPayment.AH_DateClearedInCashbook = ZDateTime.BrettsBirthday;
			Assert(!ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			apPayment.AH_DateClearedInCashbook = ZDateTime.Empty;
			Assert(ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));

			AccChequeBook checkbook2 = Factory.NewWithValidTestData<AccChequeBook>();
			checkbook2.AK_AutoPrintCheque = true;
			apPayment.ChequeBook = checkbook2.PK;
			Assert(!ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(transactions));
		}

		public void TestChequeNumberValidation()
		{
			AccChequeBook checkbook = TestObjectCreator.CreateChequeBook(10, 10, 20, TestObjectCreator.AUDBankAccount);

			APPayment apPayment1 = Factory.NewWithValidTestData<APPayment>();
			apPayment1.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			apPayment1.AH_ReceiptType = ReceiptTypes.Cheque;
			apPayment1.ChequeBook = checkbook.PK;
			apPayment1.AH_ChequeOrReference = "000011";

			APPayment apPayment2 = Factory.NewWithValidTestData<APPayment>();
			apPayment2.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			apPayment2.AH_ReceiptType = ReceiptTypes.Cheque;
			apPayment2.ChequeBook = checkbook.PK;
			apPayment2.AH_ChequeOrReference = "000012";

			APPayment apPayment3 = Factory.NewWithValidTestData<APPayment>();
			apPayment3.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			apPayment3.AH_ReceiptType = ReceiptTypes.Cheque;
			apPayment3.ChequeBook = checkbook.PK;
			apPayment3.AH_ChequeOrReference = "000015";

			Factory.Save();

			var totalhitBefore = Factory.GetTableHitCount(JobChargeSchema.Constants.TableName) + Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName) + Factory.GetTableHitCount(AccPaymentApprovalSchema.Constants.TableName);
			var transactions = new[] { apPayment1, apPayment2 };
			ChequeNumberReallocator reallocator = new ChequeNumberReallocator(Factory, checkbook, transactions, false);
			AssertNoErrors(reallocator.ChequeNumberInfo);
			var totalhitAfter = Factory.GetTableHitCount(JobChargeSchema.Constants.TableName) + Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName) + Factory.GetTableHitCount(AccPaymentApprovalSchema.Constants.TableName);
			Assert("Total DB hit for validation shouldn't be more than 3", (totalhitAfter - totalhitBefore) <= 3);

			reallocator.ChequeNumber = "000012";
			AssertNoErrors(reallocator.ChequeNumberInfo);

			reallocator.ChequeNumber = "000014";
			AssertHasError(reallocator.ChequeNumberInfo, @"Check number 000015 is already in use.");

			reallocator.ChequeNumber = "000016";
			AssertNoErrors(reallocator.ChequeNumberInfo);

			reallocator.ChequeNumber = "000021";
			AssertHasError(reallocator.ChequeNumberInfo, @"Check number 000021 is not contained in the selected check book.
The check number must be between 000010 and 000020.");

			reallocator.ChequeNumber = "000020";
			AssertHasError(reallocator.ChequeNumberInfo, "There are not enough numbers remaining in this check book from this starting point. The last check number for this check book is 000020. Please reduce the number of transactions being printed, or modify the check book setup.");

			reallocator.ChequeNumber = "0012.5";
			AssertHasError(reallocator.ChequeNumberInfo, "Only numbers are allowed in this field.");
		}

		public void TestErrorMessageIsShownWhenConcurrencyErrorOccurs()
		{
			// Arrange
			Factory.RefreshEnabled = false;
			AccChequeBook chequeBook = TestObjectCreator.CreateChequeBook(10, 10, 100, TestObjectCreator.AUDBankAccount);
			APPayment apPayment = Factory.NewWithValidTestData<APPayment>();
			apPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			apPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			apPayment.ChequeBook = chequeBook.PK;
			apPayment.AH_ChequeOrReference = "000011";
			var transactions = new[] { apPayment };
			ChequeNumberReallocator chequeNumberReallocator = new ChequeNumberReallocator(Factory, chequeBook, transactions, false);
			Factory.Save();
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			AccChequeBook chequeBookInNewFactory = newFactory.Load<AccChequeBook>(chequeBook.PK);
			// Act
			chequeBookInNewFactory.AK_CurrentNo++;
			newFactory.Save();
			ZString errorMessage = chequeNumberReallocator.Process((ChequeNumberReallocator.ReprintCheque)null);
			// Assert
			AssertEquals(ChequeNumberReallocator.ChequeNumberCannotBeReAllocatedBecauseOfConcurrencyError, errorMessage);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChequeNumberReallocator(Factory, Factory.New<AccChequeBook>(), System.Array.Empty<TransactionHeader>(), false);
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
