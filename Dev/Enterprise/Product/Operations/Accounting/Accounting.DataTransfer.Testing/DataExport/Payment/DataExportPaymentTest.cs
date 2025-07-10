using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	internal abstract class DataExportPaymentTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected abstract BusinessObject GetNewDirectPayment();

		APPayment fPayment;
		protected APPayment Payment
		{
			get
			{
				if (fPayment == null)
				{
					fPayment = Factory.NewWithValidTestData<APPayment>();
				}
				return fPayment;
			}
		}

		DirectPayment fDirectPayment;
		protected DirectPayment DirectPayment
		{
			get
			{
				if (fDirectPayment == null)
				{
					fDirectPayment = Factory.NewWithValidTestData<DirectPayment>();
				}
				return fDirectPayment;
			}
		}

		public void TestConstructor()
		{
			DataExportPayment export = (DataExportPayment)GetNewBusinessObject();
			AssertCommonPaymentProperties(export, Payment);
			AssertPaymentProperties(export, Payment);

			export = (DataExportPayment)GetNewDirectPayment();
			AssertCommonPaymentProperties(export, DirectPayment);
			AssertDirectPaymentProperties(export, DirectPayment);
		}

		public void TestCurrencyCaching()
		{
			var prevValue1 = Payment.AH_RX_NKTransactionCurrency;
			var prevValue2 = DirectPayment.AH_RX_NKTransactionCurrency;
			try
			{
				Payment.AH_RX_NKTransactionCurrency = "USD";
				DirectPayment.AH_RX_NKTransactionCurrency = "USD";
				DataExportPayment export = (DataExportPayment)GetNewBusinessObject();
				AssertNotNull(export.Currency);

				export.Currency.Delete();
				AssertNull(export.Currency);

				export.AH_RX_NKTransactionCurrency = "AUD";
				AssertNotNull(export.Currency);
				AssertEquals("New Currecny Code", "AUD", export.Currency.RX_Code);
			}
			finally
			{
				Payment.AH_RX_NKTransactionCurrency = prevValue1;
				DirectPayment.AH_RX_NKTransactionCurrency = prevValue2;
			}
		}

		void AssertCommonPaymentProperties(DataExportPayment export, TransactionHeader transactionHeader)
		{
			AssertEquals("AH_AB", transactionHeader.AH_AB, export.AH_AB);
			AssertEquals("AH_ChequeOrReference", transactionHeader.AH_ChequeOrReference, export.AH_ChequeOrReference);
			AssertEquals("AH_Ledger", transactionHeader.AH_Ledger, export.AH_Ledger);
			AssertEquals("AH_LocalExTaxAmount", transactionHeader.AH_LocalExTaxAmount, export.AH_LocalExTaxAmount);
			AssertEquals("AH_OH", transactionHeader.AH_OH, export.AH_OH);
			AssertEquals("AH_OSTotalAmount", transactionHeader.AH_OSTotalAmount, export.AH_OSTotalAmount);
			AssertEquals("AH_PostDate", transactionHeader.AH_PostDate, export.AH_PostDate);
			AssertEquals("AH_InvoiceDate", transactionHeader.AH_InvoiceDate, export.AH_InvoiceDate);
			AssertEquals("AH_ReceiptBatchNo", transactionHeader.AH_ReceiptBatchNo, export.AH_ReceiptBatchNo);
			AssertEquals("AH_ReceiptType", transactionHeader.AH_ReceiptType, export.AH_ReceiptType);
			AssertEquals("AH_RX_NKTransactionCurrency", transactionHeader.AH_RX_NKTransactionCurrency, export.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_TransactionNum", transactionHeader.AH_TransactionNum, export.AH_TransactionNum);
			AssertEquals("AH_TransactionType", transactionHeader.AH_TransactionType, export.AH_TransactionType);
			if (transactionHeader.BankAccount != null)
			{
				AssertEquals("BankBSB", transactionHeader.BankAccount.AB_BSB, export.BankBSB);
			}

			IDirectDebitBatchTransaction transaction = (IDirectDebitBatchTransaction)transactionHeader;
			AssertEquals("PayeeName", transaction.AccountTitle, export.PayeeName);
			AssertEquals("BankAccountNumber", transaction.BankAccountNumber, export.BankAccountNumber);
			AssertEquals("PayeeBankAccountNumber", transaction.PayeeBankAccountNumber, export.PayeeBankAccountNumber);
			AssertEquals("PayeeBankBranchName", transaction.PayeeBankBranchName, export.PayeeBankBranchName);
			AssertEquals("PayeeBankAddress1", transaction.PayeeBankAddress1, export.PayeeBankAddress1);
			AssertEquals("PayeeBankAddress2", transaction.PayeeBankAddress2, export.PayeeBankAddress2);
			AssertEquals("PayeeBankAddress3", transaction.PayeeBankAddress3, export.PayeeBankAddress3);
			AssertEquals("PayeeBankBSB", transaction.PayeeBankBSB, export.PayeeBankBSB);

			if (transactionHeader.Header != null)
			{
				var address = transactionHeader.Header.AddressForSendingAPDocuments;
				if (address != null)
				{
					AddressFormatter formatter = new AddressFormatter(Factory, address, GlbCompany.CurrentCompany, false);
					string[] addressParts = formatter.PostalAddress().Split(new string[] { "\n" }, int.MaxValue, StringSplitOptions.None);

					AssertEquals("PayeeAddressPostalCode", address.OA_PostCode, export.PayeeAddressPostalCode);
					AssertEquals("PayeeAddressCity", address.OA_City, export.PayeeAddressCity);
					AssertEquals("PayeeAddressState", address.OA_State, export.PayeeAddressState);

					if (addressParts.Length > 0)
					{
						AssertEquals("PayeeAddress1", addressParts[0], export.PayeeAddress1);
					}

					if (addressParts.Length > 1)
					{
						AssertEquals("PayeeAddress2", addressParts[1], export.PayeeAddress2);
					}

					if (addressParts.Length > 2)
					{
						AssertEquals("PayeeAddress3", addressParts[2], export.PayeeAddress3);
					}
				}
				AssertEquals("PayeeCountryCode", transactionHeader.Header.CountryCode, export.PayeeCountryCode);
			}
			AssertEquals("TransactionHeader", transactionHeader, export.TransactionHeader);
		}

		void AssertDirectPaymentProperties(DataExportPayment export, DirectPayment directPayment)
		{
			AssertEquals("DirectPayment", directPayment, export.DirectPayment);
		}

		void AssertPaymentProperties(DataExportPayment export, APPayment payment)
		{
			AssertEquals("Payment", payment, export.Payment);
			if (payment.AccountDetails != null)
			{
				AssertEquals("PayeeSWIFTID", payment.AccountDetails.A1_BankSwift, export.PayeeSWIFTID);
			}
		}

		#endregion
	}
}
