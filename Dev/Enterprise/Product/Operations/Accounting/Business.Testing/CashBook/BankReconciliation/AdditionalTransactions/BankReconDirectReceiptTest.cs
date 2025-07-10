using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectReceipt.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(BankReconDirectReceipt))]
	class BankReconDirectReceiptTest : DirectReceiptTest
	{
		public void TestMakeDepositBatch()
		{
			BankReconDirectReceipt directReceipt = Factory.New<BankReconDirectReceipt>();
			AssertNull("RelatedDepositBatch", directReceipt.RelatedDepositBatch);

			directReceipt.AH_OSTotal = 200;
			directReceipt.AH_InvoiceAmount = 100;
			directReceipt.AH_RX_NKTransactionCurrency = "XXX";
			directReceipt.AH_PostDate = new ZDateTime(2006, 10, 10);
			directReceipt.AH_InvoiceDate = new ZDateTime(2006, 10, 9);
			directReceipt.AH_DueDate = new ZDateTime(2006, 10, 11);
			directReceipt.AH_AB = ZGuid.NewZGuid();
			directReceipt.AH_OH = ZGuid.NewZGuid();

			directReceipt.MakeDepositBatch();
			DepositBatch.DepositBatch batch = directReceipt.RelatedDepositBatch;
			AssertNotNull("RelatedDepositBatch", batch);
			AssertEquals("AH_OSTotal", directReceipt.AH_OSTotal, batch.AH_OSTotal);
			AssertEquals("AH_InvoiceAmount", directReceipt.AH_InvoiceAmount, batch.AH_InvoiceAmount);
			AssertEquals("AH_RX_NKTransactionCurrency", directReceipt.AH_RX_NKTransactionCurrency, batch.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_PostDate", directReceipt.AH_PostDate, batch.AH_PostDate);
			AssertEquals("AH_InvoiceDate", directReceipt.AH_InvoiceDate, batch.AH_InvoiceDate);
			AssertEquals("AH_DueDate", directReceipt.AH_DueDate, batch.AH_DueDate);
			AssertEquals("AH_AB", directReceipt.AH_AB, batch.AH_AB);
			AssertEquals("AH_OH", directReceipt.AH_OH, batch.AH_OH);
		}

		public void TestHandleRelatedDepositBatch()
		{
			BankReconDirectReceipt directReceipt = Factory.NewWithValidTestData<BankReconDirectReceipt>();
			AssertNull("RelatedDepositBatch", directReceipt.RelatedDepositBatch);

			Factory.Save();
			AssertNotNull("RelatedDepositBatch", directReceipt.RelatedDepositBatch);
		}

		public void TestAH_OSTotal()
		{
			BankReconDirectReceipt directReceipt = Factory.New<BankReconDirectReceipt>();
			directReceipt.AH_OSTotal = 100;
			directReceipt.MakeDepositBatch();
			DepositBatch.DepositBatch batch = directReceipt.RelatedDepositBatch;
			AssertNotNull("RelatedDepositBatch", batch);
			AssertEquals("AH_OSTotal", 100m, batch.AH_OSTotal);

			directReceipt.AH_OSTotal = 200;
			AssertEquals("AH_OSTotal", 200m, batch.AH_OSTotal);
		}

		public void TestAH_PostDate()
		{
			BankReconDirectReceipt directReceipt = Factory.New<BankReconDirectReceipt>();
			directReceipt.AH_PostDate = new ZDateTime(2006, 10, 10);
			directReceipt.MakeDepositBatch();
			DepositBatch.DepositBatch batch = directReceipt.RelatedDepositBatch;
			AssertNotNull("RelatedDepositBatch", batch);
			AssertEquals("AH_OSTotal", new ZDateTime(2006, 10, 10), batch.AH_PostDate);

			directReceipt.AH_PostDate = new ZDateTime(2006, 11, 11);
			AssertEquals("AH_OSTotal", new ZDateTime(2006, 11, 11), batch.AH_PostDate);
		}

		public void TestAH_InvoiceDate()
		{
			BankReconDirectReceipt directReceipt = Factory.New<BankReconDirectReceipt>();
			directReceipt.AH_InvoiceDate = new ZDateTime(2006, 10, 10);
			directReceipt.MakeDepositBatch();
			DepositBatch.DepositBatch batch = directReceipt.RelatedDepositBatch;
			AssertNotNull("RelatedDepositBatch", batch);
			AssertEquals("AH_OSTotal", new ZDateTime(2006, 10, 10), batch.AH_InvoiceDate);

			directReceipt.AH_InvoiceDate = new ZDateTime(2006, 11, 11);
			AssertEquals("AH_OSTotal", new ZDateTime(2006, 11, 11), batch.AH_InvoiceDate);
		}

		public void TestValidationType()
		{
			AssertEquals("Validation", typeof(BankReconDirectReceiptValidation), TestBizO.Validation.GetType());
		}

		protected override bool DoesNeedCreatingDepositBatchForTheType(ZString receiptType)
		{
			return true;
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(BankReconDirectReceiptValidation); }
		}
	}
}
