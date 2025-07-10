using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_TransactionsExportBatchingTest : ScriptTest
	{
		public void TestSimpleRun()
		{
			var invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var invoice2 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var creditNote = (APCreditNote)TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "CRD", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var adjustmentNote = (APAdjustmentNote)TestObjectCreator.CreateInvoiceWithLine(typeof(APAdjustmentNote), "ADJ", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			invoice1.AH_OH = invoice2.AH_OH = creditNote.AH_OH = adjustmentNote.AH_OH = TestObjectCreator.AALSHI.PK;

			var wip1 = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			var wip2 = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc2", 10M);
			var wip3 = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc2", 10M);
			wip1.AL_OH = wip2.AL_OH = wip3.AL_OH = TestObjectCreator.ABIGAS.PK;

			invoice1.AH_ConsolidatedInvoiceRef = "00000001";

			Factory.Save();

			var batchNumber = 1;
			var headerBatchPks = invoice1.PK + "," + invoice2.PK + "," + creditNote.PK + "," + adjustmentNote.PK;
			var wipAccrualPostBatchPks = wip1.PK + "," + wip2.PK;
			var wipAccrualReverseBatchPks = wip1.PK + "," + wip2.PK + "," + wip3.PK;

			RunAccountingTransactionExportBatchCreationScript(batchNumber, headerBatchPks, wipAccrualPostBatchPks, wipAccrualReverseBatchPks);
			Factory.Save();

			var resultTable = RunScript("XLT", 0, 10, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "", "", "");

			var headers = new[] { "Ledger", "Type", "TransactionReference", "Description", "Organisation", "Currency", "OSAmount", "LocalAmount", "Value" };

			var lines = new[]
			{
				new object[] { "AR", "INV", "", "Test Invoice", "AALSHI      ", "AUD", 10m, 10m, "HEX 1" },
				new object[] { "AP", "INV", invoice2.AH_ConsolidatedInvoiceRef, "Test Invoice", "AALSHI      ", "AUD", -10m, -10m, "HEX 1" },
				new object[] { "AP", "CRD", creditNote.AH_ConsolidatedInvoiceRef, "Test Invoice", "AALSHI      ", "AUD", 10m, 10m, "HEX 1" },
				new object[] { "AP", "ADJ", adjustmentNote.AH_ConsolidatedInvoiceRef, "Test Invoice", "AALSHI      ", "AUD", -10m, -10m, "HEX 1" },
				new object[] { "JC", "WIP", "", "Desc1", "ABIGAS      ", "AUD", -10m, -10m, "LEX 1, LRX 1" },
				new object[] { "JC", "WIP", "", "Desc2", "ABIGAS      ", "AUD", -10m, -10m, "LRX 1" },
				new object[] { "JC", "WIP", "", "Desc2", "ABIGAS      ", "AUD", -10m, -10m, "LEX 1, LRX 1" }
			};

			Assert("AR Invoice TransactionReference is empty, but AH_ConsolidatedInvoiceRef is not empty", !string.IsNullOrEmpty(invoice1.AH_ConsolidatedInvoiceRef));
			Assert("AR Invoice TransactionReference is not empty", !string.IsNullOrEmpty(invoice2.AH_ConsolidatedInvoiceRef));
			Assert("AP CreditNote TransactionReference is not empty", !string.IsNullOrEmpty(creditNote.AH_ConsolidatedInvoiceRef));
			Assert("AP AdjustmentNote TransactionReference is not empty", !string.IsNullOrEmpty(adjustmentNote.AH_ConsolidatedInvoiceRef));

			AssertDataTableAllRowsByKeyColumns("TransactionsExportBatching", resultTable, headers, lines, new[] { "Ledger", "Type", "Description", "Value" });
		}

		public void TestRunWithInternalReferenceNo()
		{
			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001000", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			invoice.AH_ConsolidatedInvoiceRef = "Test001";
			var batchNumber = 1;
			RunAccountingTransactionExportBatchCreationScript(batchNumber, invoice.PK.ToString(), "", "");
			Factory.Save();

			var resultTable = RunScript("XLT", 0, 10, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "", "", "");

			var headers = new[] { "Ledger", "Type", "TransactionReference", "Description", "Organisation", "Currency", "OSAmount", "LocalAmount", "Value" };
			var lines = new[]
			{
				new object[] { "AP", "INV", "Test001", "Test Invoice", "AALSHI      ", "AUD", -10m, -10m, "HEX 1" },
			};

			AssertDataTableAllRowsByKeyColumns("TransactionsExportBatching", resultTable, headers, lines, new[] { "Ledger", "Type", "Description", "Value", "TransactionReference" });

			batchNumber = 2;
			RunAccountingTransactionExportCreateBatchScript(batchNumber);
			Factory.Save();

			resultTable = RunScript("XUT", 0, 10, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "", "", "");

			lines = System.Array.Empty<object[]>();
			AssertDataTableAllRowsByKeyColumns("TransactionsExportBatching", resultTable, headers, lines, new[] { "Ledger", "Type", "Description", "Value", "TransactionReference" });
		}

		public void TestBatchNumberAndSequence()
		{
			var wip = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			var reversedwip = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			TestObjectCreator.CreateGenExportBatchSequencePostLine(123, wip.PK, 539);
			TestObjectCreator.CreateGenExportBatchSequenceReverseLine(321, reversedwip.PK, 935);
			Factory.Save();
			AssertBatchTransactionReference("000012300539", wip, "000032100935", reversedwip);

			wip = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			reversedwip = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			TestObjectCreator.CreateGenExportBatchSequencePostLine(1234567, wip.PK, 539);
			TestObjectCreator.CreateGenExportBatchSequenceReverseLine(7654321, reversedwip.PK, 935);
			Factory.Save();
			AssertBatchTransactionReference("123456700539", wip, "765432100935", reversedwip);

			wip = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			reversedwip = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			TestObjectCreator.CreateGenExportBatchSequencePostLine(123456789, wip.PK, 539);
			TestObjectCreator.CreateGenExportBatchSequenceReverseLine(987654321, reversedwip.PK, 935);
			Factory.Save();
			AssertBatchTransactionReference("12345678900539", wip, "98765432100935", reversedwip);

			wip = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			reversedwip = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			TestObjectCreator.CreateGenExportBatchSequencePostLine(123, wip.PK, 53978);
			TestObjectCreator.CreateGenExportBatchSequenceReverseLine(321, reversedwip.PK, 87935);
			Factory.Save();
			AssertBatchTransactionReference("000012353978", wip, "000032187935", reversedwip);

			wip = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			reversedwip = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			TestObjectCreator.CreateGenExportBatchSequencePostLine(123, wip.PK, 539784);
			TestObjectCreator.CreateGenExportBatchSequenceReverseLine(321, reversedwip.PK, 487935);
			Factory.Save();
			AssertBatchTransactionReference("0000123539784", wip, "0000321487935", reversedwip);
		}

		void AssertBatchTransactionReference(string expectedExportBatchTransactionReference, WIP wip, string expectedExportReverseBatchTransactionReference, WIP reversedwip)
		{
			AssertEquals("Export Batch Transaction Reference", expectedExportBatchTransactionReference, wip.ExportBatchTransactionReference);
			AssertEquals("Export Reverse Batch Transaction Reference", expectedExportReverseBatchTransactionReference, reversedwip.ExportReverseBatchTransactionReference);

			var resultTable = RunScript("XLT", 0, 0, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "", "", expectedExportBatchTransactionReference);
			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals(expectedExportBatchTransactionReference, resultTable.Rows[0]["TransactionNum"]);

			resultTable = RunScript("XLT", 0, 0, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "", "", expectedExportReverseBatchTransactionReference);
			AssertEquals(1, resultTable.Rows.Count);
			AssertEquals(expectedExportReverseBatchTransactionReference, resultTable.Rows[0]["TransactionNum"]);
		}

		DataTable RunAccountingTransactionExportBatchCreationScript(int batchNumber, string headersBatchFilterPks, string wIPAccrualPostBatchFilterPKs, string wIPAccrualReverseBatchFilterPKs)
		{
			string sql = string.Format(@"Exec AccountingTransactionExportBatchCreation 
			'{0}', --@CompanyCode
			'{1}', --@BatchNumber
			'{2}', --@HeadersBatchFilterPks
			'{3}', --@WIPAccrualPostBatchFilterPKs
			'{4}'  --@WIPAccrualReverseBatchFilterPKs
			", GlbCompany.CurrentCompany.PK.ToString(), batchNumber, headersBatchFilterPks, wIPAccrualPostBatchFilterPKs, wIPAccrualReverseBatchFilterPKs);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		void RunAccountingTransactionExportCreateBatchScript(int batchNumber)
		{
			string sql = string.Format(@"Exec AccountingTransactionExportCreateBatch 
			'{0}' --@CompanyCode
			", GlbCompany.CurrentCompany.GC_Code);
			var table = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			foreach (DataRow row in table.Rows)
			{
				string sqlText = string.Format(@"INSERT INTO dbo.GenExportBatchSequence (XB_PK, XB_Type, XB_BatchNumber, XB_Sequence, XB_ParentTableCode, XB_ParentID)
								VALUES (NEWID(), '{0}', {1}, {2}, '{3}', '{4}')", row["Type"], batchNumber, row["Sequence"], row["ParentTableCode"], row["ParentID"]);

				using (var cmd = Db.Connection.Command(sqlText))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		DataTable RunScript(string exportBatchType, int batchNumberFrom, int batchNumberTo, ZDateTime postDateFrom, ZDateTime postDateTo, string ledger, string transactionType, string transactionNum)
		{
			var sql = string.Format(@"
SELECT * FROM Report_TransactionsExportBatching(
	'{0}',
	'{1}',				-- ExportBatchType
	{2},				-- BatchNumberFrom
	{3},				-- BatchNumberTo
	'{4}',				-- PostDateFrom
	'{5}',				-- PostDateTo
	'{6}',				-- Ledger
	'{7}',				-- TransactionType
	'{8}'				-- TransactionNum
)",
							GlbCompany.CurrentCompany.PK,			//@CompanyPK
							exportBatchType,						//@ExportBatchType
							batchNumberFrom,						//@BatchNumberFrom
							batchNumberTo,							//@BatchNumberTo
							postDateFrom.ToISO8601String(),			//@PostDateFrom
							postDateTo.ToISO8601String(),			//@PostDateTo
							ledger,									//@Ledger
							transactionType,						//@TransactionType
							transactionNum							//@TransactionNum
	);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
