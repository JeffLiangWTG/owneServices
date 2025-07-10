using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(DirectTransactionsBusinessObject))]
	public class DirectTransactionsBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
		}

		public void TestMoveDirectReceiptsAndPaymentsToBaseCollection()
		{
			DirectTransactionsBusinessObject directTransactions = (DirectTransactionsBusinessObject)GetNewBusinessObject();
			BankReconDirectPayment payment = Factory.NewWithValidTestData<BankReconDirectPayment>();
			BankReconDirectReceipt receipt = Factory.NewWithValidTestData<BankReconDirectReceipt>();
			directTransactions.DirectPayments.Add(payment);
			directTransactions.DirectReceipts.Add(receipt);

			AssertEquals(1, directTransactions.DirectPayments.Count);
			AssertEquals(1, directTransactions.DirectReceipts.Count);
			AssertEquals(0, directTransactions.Headers.Count);

			directTransactions.MoveDirectReceiptsAndPaymentsToBaseCollection();

			AssertEquals(1, directTransactions.DirectPayments.Count);
			AssertEquals(1, directTransactions.DirectReceipts.Count);
			AssertEquals(2, directTransactions.Headers.Count);
		}

		[ExpectExceptionMessage(typeof(ApplicationException), "Trying to add a different direct receipt to same batch. Expecting one to one mapping here")]
		public void TestDirectReceiptToBatchPKMapping()
		{
			DirectTransactionsBusinessObject directTransactions = (DirectTransactionsBusinessObject)GetNewBusinessObject();

			var batchPK = ZGuid.NewZGuid();
			var receiptPK = ZGuid.NewZGuid();
			var unrelatedPK = ZGuid.NewZGuid();

			AssertEquals("Can call receiptbatch pk collection when nothing has been added, and there are no elements", 0, directTransactions.DirectReceiptBatchPKs.Count());
			directTransactions.AddToBatchToDirectReceiptPKMapping(batchPK, receiptPK);
			AssertEquals("One element added", 1, directTransactions.DirectReceiptBatchPKs.Count());
			AssertEquals("Calling GetDirectReceiptForBatch with unrelatedPK gives you empty", ZGuid.Empty, directTransactions.GetDirectReceiptForBatch(unrelatedPK));
			AssertEquals("Calling GetDirectReceiptForBatch with receiptPK gives you empty because we look up based on batch not receipt", ZGuid.Empty, directTransactions.GetDirectReceiptForBatch(receiptPK));
			AssertEquals("Calling GetDirectReceiptForBatch with batchPK gives you receiptPK", receiptPK, directTransactions.GetDirectReceiptForBatch(batchPK));

			directTransactions.AddToBatchToDirectReceiptPKMapping(batchPK, receiptPK);
			AssertEquals("Can add the same batch/receipt combo again, not recorded as another element", 1, directTransactions.DirectReceiptBatchPKs.Count());

			// Expect exception when doing this i.e. different GUID, same batch
			directTransactions.AddToBatchToDirectReceiptPKMapping(batchPK, new Guid());
		}
	}
}
