using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ARAPTransactionTypes))]
	public class ARAPTransactionTypesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T302", ARAPTransactionTypes.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ARAPTransactionTypes();
		}
	}

	[TestedType(typeof(ARAPTransactionTypesCollection))]
	public class ARAPTransactionTypesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ARAPTransactionTypesCollection>
	{
		public void TestDefaultValues()
		{
			ARAPTransactionTypesCollection collection = new ARAPTransactionTypesCollection(Factory);
			AssertEquals(21, collection.Count);
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Invoice, collection[0].TransactionTypesCode);
			AssertEquals(TransactionDescription.ARInvoice, collection[0].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.AdjustmentNote, collection[1].TransactionTypesCode);
			AssertEquals(TransactionDescription.ARAdjustment, collection[1].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.CreditNote, collection[2].TransactionTypesCode);
			AssertEquals(TransactionDescription.ARCreditNote, collection[2].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Payment, collection[3].TransactionTypesCode);
			AssertEquals(TransactionDescription.ARPayment, collection[3].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Receipt, collection[4].TransactionTypesCode);
			AssertEquals(TransactionDescription.ARReceipt, collection[4].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Discount, collection[5].TransactionTypesCode);
			AssertEquals(TransactionDescription.ARDiscount, collection[5].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.ExchangeDifference, collection[6].TransactionTypesCode);
			AssertEquals(TransactionDescription.ARExchangeDiff, collection[6].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Overpayment, collection[7].TransactionTypesCode);
			AssertEquals(TransactionDescription.AROverpayment, collection[7].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Journal, collection[8].TransactionTypesCode);
			AssertEquals(TransactionDescription.ARJournal, collection[8].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsReceivable + TransactionTypes.Transfer, collection[9].TransactionTypesCode);
			AssertEquals(TransactionDescription.ARTransfer, collection[9].TransactionTypesName);
			AssertEquals(TransactionTypes.Contra, collection[10].TransactionTypesCode);
			AssertEquals(TransactionDescription.Contra, collection[10].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Invoice, collection[11].TransactionTypesCode);
			AssertEquals(TransactionDescription.APInvoice, collection[11].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.AdjustmentNote, collection[12].TransactionTypesCode);
			AssertEquals(TransactionDescription.APAdjustment, collection[12].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.CreditNote, collection[13].TransactionTypesCode);
			AssertEquals(TransactionDescription.APCreditNote, collection[13].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Payment, collection[14].TransactionTypesCode);
			AssertEquals(TransactionDescription.APPayment, collection[14].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Receipt, collection[15].TransactionTypesCode);
			AssertEquals(TransactionDescription.APReceipt, collection[15].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Discount, collection[16].TransactionTypesCode);
			AssertEquals(TransactionDescription.APDiscount, collection[16].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.ExchangeDifference, collection[17].TransactionTypesCode);
			AssertEquals(TransactionDescription.APExchangeDiff, collection[17].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Overpayment, collection[18].TransactionTypesCode);
			AssertEquals(TransactionDescription.APOverpayment, collection[18].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Journal, collection[19].TransactionTypesCode);
			AssertEquals(TransactionDescription.APJournal, collection[19].TransactionTypesName);
			AssertEquals(LedgerTypes.AccountsPayable + TransactionTypes.Transfer, collection[20].TransactionTypesCode);
			AssertEquals(TransactionDescription.APTransfer, collection[20].TransactionTypesName);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ARAPTransactionTypes();
		}

		protected override ARAPTransactionTypesCollection GetCollectionToTest()
		{
			return new ARAPTransactionTypesCollection(Factory);
		}
	}
}
