using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	abstract class TransactionHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<TransactionHeaderProcessTaskCollection>
	{
		protected override TransactionHeaderProcessTaskCollection GetCollectionToTestCore()
		{
			var transactionHeader = Factory.NewWithValidTestData(GetExpectedBusinessObjectTypeForTransactionHeader()) as TransactionHeader;
			return new TransactionHeaderProcessTaskCollection(transactionHeader);
		}

		protected abstract Type GetExpectedBusinessObjectTypeForTransactionHeader();
	}

	#region AR/AP/UA Invoice

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForARInvoice : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(ARInvoice);
	}

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForAPInvoice : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(APInvoice);
	}

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForUAInvoice : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(UAInvoice);
	}

	#endregion

	#region AR/AP/UA CreditNote

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForARCreditNote : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(ARCreditNote);
	}

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForAPCreditNote : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(APCreditNote);
	}

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForUACreditNote : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(UACreditNote);
	}

	#endregion

	#region AR/AP AdjustmentNote

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForARAdjustmentNote : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(ARAdjustmentNote);
	}

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForAPAdjustmentNote : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(APAdjustmentNote);
	}

	#endregion

	#region AR/AP Receipt

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForARReceipt : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(ARReceipt);
	}

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForAPReceipt : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(APReceipt);
	}

	#endregion

	#region AR/AP Payment

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForARPayment : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(ARPayment);
	}

	[TestedType(typeof(TransactionHeaderProcessTaskCollection))]
	class TransactionHeaderProcessTaskCollectionTest_ForAPPayment : TransactionHeaderProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForTransactionHeader() => typeof(APPayment);
	}

	#endregion
}
