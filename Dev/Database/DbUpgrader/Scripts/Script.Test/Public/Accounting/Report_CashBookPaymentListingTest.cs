using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_CashBookPaymentListing))]
	class Report_CashBookPaymentListingTest : DbCreateScriptTest
	{
		[ExpectNoExceptions()]
		public void TestChequePaymentWhenNoChequeOrReferenceNumber()
		{
			InsertBankAccount();
			InsertChequeBook();
			InsertChequePaymentsWithEmtpyReferenceNumber();
			AssertRowsReturned(@"select * from report_Cashbookpaymentlisting ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')", 2);
		}

		public void TestDDLPaymentsAppearWhenFilteringForDDRType()
		{
			InsertBankAccount();
			InsertDDLPayment();

			AssertRowsReturned(@"select * from report_Cashbookpaymentlisting ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')", 1);

			AssertRowsReturned(@"select * from report_Cashbookpaymentlisting ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC') WHERE ReceiptType = 'DDR'", 1);

			AssertRowsReturned(@"select * from report_Cashbookpaymentlisting ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC') WHERE ReceiptType = 'CHQ'", 0);
		}

		public void TestPaymentsWithPayeeNames()
		{
			InsertBankAccount();
			InsertPayments();
			var command = Db.Connection.Command(@"select * from report_Cashbookpaymentlisting ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			var table = new DataTable();
			command.NewDataAdapter().Fill(table);
			AssertEquals(4, table.Rows.Count);

			var rows = table.Rows.Cast<DataRow>().Select(x =>
				new
				{
					PayeeName = x["PayeeName"].ToString(),
					Type = x["Type"].ToString(),
					Ledger = x["Ledger"].ToString()
				}).ToArray();

			Assert(rows.Any(x => x.Ledger == "AR" && x.PayeeName == "JAY SCHULZ" && x.Type == "PAY"));
			Assert(rows.Any(x => x.Ledger == "AP" && x.PayeeName == "JAY SCHULZ" && x.Type == "PAY"));
			Assert(rows.Any(x => x.Ledger == "AR" && x.PayeeName == "Payee ABC" && x.Type == "DPY"));
			Assert(rows.Any(x => x.Ledger == "AP" && x.PayeeName == "Payee DEF" && x.Type == "DPY"));
		}

		static void AssertRowsReturned(string sQL, int numberOfRowsToAssert)
		{
			var command = Db.Connection.Command(sQL);
			var table = new DataTable();
			command.NewDataAdapter().Fill(table);
			string error = string.Format("Should have had {0} number of rows for SQL: \r\n{1}", numberOfRowsToAssert, sQL);
			AssertEquals(error, numberOfRowsToAssert, table.Rows.Count);
		}

		static void InsertDDLPayment()
		{
			string insertPaymentSQL = @"INSERT INTO dbo.AccTransactionHeader
(
	AH_PK,
	AH_AB,
	AH_AgePeriod,
	AH_CashBasisGSTIndicator,
	AH_CashBasisGSTRealisedToGL,
	AH_ChequeDrawer,
	AH_ChequeOrReference,
	AH_ComplianceSubType,
	AH_ConsolidatedInvoiceRef,
	AH_Desc,
	AH_DrawerBank,
	AH_DrawerBranch,
	AH_ExchangeRate,
	AH_ExportBatchNumber,
	AH_GB,
	AH_GC,
	AH_GE,
	AH_GSTAmount,
	AH_InvoiceAmount,
	AH_InvoiceApproved,
	AH_InvoiceDate,
	AH_InvoicePrinted,
	AH_InvoiceTerm,
	AH_InvoiceTermDays,
	AH_IsCancelled,
	AH_Ledger,
	AH_NotAllocated,
	AH_NumberOfSupportingDocuments,
	AH_OH,
	AH_OSTotal,
	AH_OutstandingAmount,
	AH_POST1,
	AH_POST2,
	AH_POST3,
	AH_POST4,
	AH_PostDate,
	AH_PostedInternal,
	AH_PostedToEFT,
	AH_PostPeriod,
	AH_PostToGL,
	AH_ReceiptBatchNo,
	AH_ReceiptType,
	AH_RequisitionStatus,
	AH_RX_NKTransactionCurrency,
	AH_SystemCreateTimeUtc,
	AH_SystemCreateUser,
	AH_TransactionCategory,
	AH_TransactionCount,
	AH_TransactionCreatedByMatching,
	AH_TransactionNum,
	AH_TransactionReference,
	AH_TransactionType,
	AH_WithholdingTax
)
VALUES
(
	'9463e397-cb86-4e23-ab64-daa764b6d8c9',
	'c3f3b6de-9aac-426d-9c18-9bdec8ded0a6',
	0,
	0,
	0,
	N'',
	'1',
	'',
	'',
	N'AP PAYMENT',
	N'',
	N'',
	1,
	0,
	'27A55065-AC88-4EC3-8BED-E575E79172CB',
	'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
	'86bb1c22-0865-4685-996e-d56cbd136491',
	0,
	200,
	1,
	'2012-02-01 19:08:00.000',
	0,
	'',
	0,
	0,
	'AP',
	0,
	1,
	'C3F842EF-3BE5-448C-BED3-0017B232C624',
	200,
	200,
	0,
	0,
	0,
	0,
	'2012-02-01 19:08:00.000',
	0,
	0,
	0,
	0,
	'',
	'DDL',
	'',
	'NZD',
	'2012-02-01 19:16:00.000',
	'E',
	'',
	1,
	0,
	'AKLBRN00000003',
	'',
	'PAY',
	0
)";
			Db.Connection.ExecuteNonQuery(insertPaymentSQL);
		}

		static void InsertBankAccount()
		{
			string insertBankAccountSQL = @"INSERT INTO dbo.AccBankAccount
(
	AB_PK,
	AB_AccountEFTUserID,
	AB_AccountNum,
	AB_AccountNumber,
	AB_AccountType,
	AB_AG,
	AB_AllowAutoDDR,
	AB_AutoDDRFormat,
	AB_BankAbbreviation,
	AB_BankAccountName,
	AB_BankAddress,
	AB_BankName,
	AB_BSB,
	AB_ChequeNumDigits,
	AB_ClosingBalance,
	AB_ClosingOSBalance,
	AB_Code,
	AB_DebitCreditCardExpiry,
	AB_DebitCreditCardName,
	AB_DebitCreditCardNumber,
	AB_Desc,
	AB_DetailedDepositSlip,
	AB_GC,
	AB_IsActive,
	AB_IsDefaultReceiptBankAccount,
	AB_OpenBalance,
	AB_OpenOSBalance,
	AB_OpenPeriod,
	AB_RN_NKBankAccountCountry,
	AB_RX_NKAccountCurrency,
	AB_ShowDetailsOnDirectDebits,
	AB_StatementBalance,
	AB_SWIFT,
	AB_SystemCreateTimeUtc,
	AB_SystemCreateUser,
	AB_SystemLastEditTimeUtc,
	AB_SystemLastEditUser
)
VALUES
(
	'c3f3b6de-9aac-426d-9c18-9bdec8ded0a6',
	'',
	'19813.28651',
	'',
	'BNK',
	'62A5472B-482A-47CC-A479-3085E6DF7D64',
	0,
	'',
	'TST',
	N'NEW ZEALAND TEST COMPANY',
	N'ADDRESS',
	N'NAME',
	'',
	6,
	0,
	0,
	'TST',
	'',
	N'',
	'',
	N'DESC',
	1,
	'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
	1,
	0,
	0,
	0,
	0,
	'',
	'NZD',
	1,
	0,
	'',
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)";
			Db.Connection.ExecuteNonQuery(insertBankAccountSQL);
		}

		static void InsertChequeBook()
		{
			string insertBankAccountSQL = @"INSERT INTO dbo.AccChequeBook
(
	AK_PK,
	AK_Code,
	AK_Desc,
	AK_StartNo,
	AK_CurrentNo,
	AK_LastNo,
	AK_AB,
	AK_GB,
	AK_SQ,
	AK_AutoPrintCheque,
	AK_IsActive
)
VALUES
(
	'DDED3303-529B-4AFD-88DA-5FA0F8EF7754',
	'CB1',
	'Check Book 1',
	'1',
	'1',
	'100',
	'c3f3b6de-9aac-426d-9c18-9bdec8ded0a6',
	'27a55065-ac88-4ec3-8bed-e575e79172cb',
	NULL,
	0,
	1
)";
			Db.Connection.ExecuteNonQuery(insertBankAccountSQL);
		}

		void InsertChequePaymentsWithEmtpyReferenceNumber()
		{
			Db.Connection.ExecuteNonQuery(string.Format(insertPaymentSQL, "AR", "PAY", "", "CHQ", ""));
			Db.Connection.ExecuteNonQuery(string.Format(insertPaymentSQL, "AP", "PAY", "", "CHQ", ""));
		}

		void InsertPayments()
		{
			Db.Connection.ExecuteNonQuery(string.Format(insertPaymentSQL, "AR", "PAY", "", "CSH", "1"));
			Db.Connection.ExecuteNonQuery(string.Format(insertPaymentSQL, "AP", "PAY", "", "CSH", "1"));

			Db.Connection.ExecuteNonQuery(string.Format(insertPaymentSQL, "AR", "DPY", "Payee ABC", "CSH", "1"));
			Db.Connection.ExecuteNonQuery(string.Format(insertPaymentSQL, "AP", "DPY", "Payee DEF", "CSH", "1"));
		}

		const string insertPaymentSQL = @"INSERT INTO dbo.AccTransactionHeader
(
	AH_PK,
	AH_AB,
	AH_AgePeriod,
	AH_CashBasisGSTIndicator,
	AH_CashBasisGSTRealisedToGL,
	AH_ChequeDrawer,
	AH_ChequeOrReference,
	AH_ComplianceSubType,
	AH_ConsolidatedInvoiceRef,
	AH_Desc,
	AH_DrawerBank,
	AH_DrawerBranch,
	AH_ExchangeRate,
	AH_ExportBatchNumber,
	AH_GB,
	AH_GC,
	AH_GE,
	AH_GSTAmount,
	AH_InvoiceAmount,
	AH_InvoiceApproved,
	AH_InvoiceDate,
	AH_InvoicePrinted,
	AH_InvoiceTerm,
	AH_InvoiceTermDays,
	AH_IsCancelled,
	AH_Ledger,
	AH_NotAllocated,
	AH_NumberOfSupportingDocuments,
	AH_OH,
	AH_OSTotal,
	AH_OutstandingAmount,
	AH_POST1,
	AH_POST2,
	AH_POST3,
	AH_POST4,
	AH_PostDate,
	AH_PostedInternal,
	AH_PostedToEFT,
	AH_PostPeriod,
	AH_PostToGL,
	AH_ReceiptBatchNo,
	AH_ReceiptType,
	AH_RequisitionStatus,
	AH_RX_NKTransactionCurrency,
	AH_SystemCreateTimeUtc,
	AH_SystemCreateUser,
	AH_TransactionCategory,
	AH_TransactionCount,
	AH_TransactionCreatedByMatching,
	AH_TransactionNum,
	AH_TransactionReference,
	AH_TransactionType,
	AH_WithholdingTax
)
VALUES
(
	NEWID(),
	'c3f3b6de-9aac-426d-9c18-9bdec8ded0a6',
	0,
	0,
	0,
	N'{2}',
	'{4}',
	'',
	'',
	N'{0} PAYMENT',
	N'',
	N'',
	1,
	0,
	'27A55065-AC88-4EC3-8BED-E575E79172CB',
	'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
	'86bb1c22-0865-4685-996e-d56cbd136491',
	0,
	200,
	1,
	'2012-02-01 19:08:00.000',
	0,
	'',
	0,
	0,
	'{0}',
	0,
	1,
	'C3F842EF-3BE5-448C-BED3-0017B232C624',
	200,
	200,
	0,
	0,
	0,
	0,
	'2012-02-01 19:08:00.000',
	0,
	0,
	0,
	0,
	'',
	'{3}',
	'',
	'NZD',
	'2012-02-01 19:16:00.000',
	'E',
	'',
	1,
	0,
	'AKLBRN00000003',
	'',
	'{1}',
	0
)";
	}
}

