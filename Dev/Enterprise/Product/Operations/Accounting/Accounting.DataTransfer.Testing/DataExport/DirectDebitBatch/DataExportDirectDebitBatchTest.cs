using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	internal abstract class DataExportDirectDebitBatchTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCurrencyCaching()
		{
			DirectDebitBatchHeader.AH_RX_NKTransactionCurrency = "USD";
			var export = GetNewBusinessObject() as DataExportDirectDebitBatch;
			AssertNotNull(export.Currency);

			export.Currency.Delete();
			AssertNull(export.Currency);

			export.AH_RX_NKTransactionCurrency = "AUD";
			AssertNotNull(export.Currency);
			AssertEquals("New Currecny Code", "AUD", export.Currency.RX_Code);
		}

		#region Implementation

		public void TestConstructor()
		{
			DirectDebitBatchHeader header = DirectDebitBatchHeader;
			DataExportDirectDebitBatch dataExportHeader = (DataExportDirectDebitBatch)GetNewBusinessObject();
			AssertEquals("Header", header, dataExportHeader.Header);
			AssertEquals("AH_AB", header.AH_AB, dataExportHeader.AH_AB);
			AssertEquals("AH_ChequeOrReference", header.AH_ChequeOrReference, dataExportHeader.AH_ChequeOrReference);
			AssertEquals("AH_GB", header.AH_GB, dataExportHeader.AH_GB);
			AssertEquals("AH_GE", header.AH_GE, dataExportHeader.AH_GE);
			AssertEquals("AH_InvoiceAmount", header.AH_InvoiceAmount, dataExportHeader.AH_InvoiceAmount);
			AssertEquals("AH_OSExTaxAmount", header.AH_OSExTaxAmount, dataExportHeader.AH_OSExTaxAmount);
			AssertEquals("AH_PostDate", header.AH_PostDate, dataExportHeader.AH_PostDate);
			AssertEquals("AH_InvoiceDate", header.AH_InvoiceDate, dataExportHeader.AH_InvoiceDate);
			AssertEquals("AH_ReceiptType", header.AH_ReceiptType, dataExportHeader.AH_ReceiptType);
			AssertEquals("AH_RX_NKTransactionCurrency", header.AH_RX_NKTransactionCurrency, dataExportHeader.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_TransactionNum", header.AH_TransactionNum, dataExportHeader.AH_TransactionNum);
		}

		DirectDebitBatchHeader fDirectDebitBatchHeader;
		protected DirectDebitBatchHeader DirectDebitBatchHeader
		{
			get
			{
				if (fDirectDebitBatchHeader == null)
				{
					fDirectDebitBatchHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
				}
				return fDirectDebitBatchHeader;
			}
		}

		#endregion
	}
}
