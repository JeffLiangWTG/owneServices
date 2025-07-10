using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	[TestedType(typeof(DataExportPositivePay))]
	internal abstract class DataExportPositivePayTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCount()
		{
			var positivePay = (DataExportPositivePay)GetNewBusinessObject();

			AssertEquals("Count", 2, positivePay.Count);
		}

		public void TestOSTotalAmount()
		{
			var positivePay = (DataExportPositivePay)GetNewBusinessObject();

			AssertEquals("OSTotalAmount", 301m, positivePay.OSTotalAmount);
		}

		public void TestLocalTotalAmount()
		{
			var positivePay = (DataExportPositivePay)GetNewBusinessObject();

			AssertEquals("LocalTotalAmount", 4020m, positivePay.LocalTotalAmount);
		}

		public void TestTransactionHeaders()
		{
			var positivePay = (DataExportPositivePay)GetNewBusinessObject();

			AssertNotNull("This array should be public so people can use it in IronPython Expressions", positivePay.TransactionHeaders);
			AssertEquals("Count of public TransactionHeaders array", 2, positivePay.TransactionHeaders.Length);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataExportPositivePayHeader(Factory, TransactionHeaders, BankAccount);
		}

		TransactionHeader[] transactionHeaders;
		protected TransactionHeader[] TransactionHeaders
		{
			get
			{
				if (transactionHeaders == null)
				{
					TransactionHeader transactionHeader = Factory.New<APPayment>();
					transactionHeader.AH_OSTotalAmount = 1m;
					transactionHeader.AH_LocalExTaxAmount = 20m;

					TransactionHeader transactionHeader2 = Factory.New<DirectPayment>();
					transactionHeader2.AH_OSTotalAmount = 300m;
					transactionHeader2.AH_LocalExTaxAmount = 4000m;

					transactionHeaders = new TransactionHeader[] { transactionHeader, transactionHeader2 };
				}
				return transactionHeaders;
			}
		}

		AccBankAccount bankAccount;
		protected AccBankAccount BankAccount
		{
			get
			{
				if (bankAccount == null)
				{
					bankAccount = Factory.New<AccBankAccount>();
				}
				return bankAccount;
			}
		}

		#endregion
	}
}
