using System;
using System.Data;
using CargoWise.Types;
using Enterprise.DataConverters.Accounting.Cyber2;
using Enterprise.DataConverters.Testing.DataImporters.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataConverters.Testing.Accounting.Cyber2
{
	class InvoiceDataImporterTest : LedgerDataImporterBase
	{
		public override void TestGetsFirstRecordFromSampleFileAndFillsItInCorrectly()
		{
			AssertEquals("DataImporter.ReadData(path)", true, Importer.ReadData());
			AssertEquals("Reader.HasRecordsNotProcessedYet after reading data", true, Importer.HasRecordsNotProcessedYet);

			var writer = (LedgerWriter)Importer.GetNextDataWriter(Factory);

			AssertEquals("Account", writer.Account);
			AssertEquals("Branch", writer.Branch);
			AssertEquals("Number", writer.Reference);
			AssertEquals("AUD", writer.Currency);
			AssertEquals(100.0m, writer.ForeignBalance);
			AssertEquals(150.0m, writer.LocalBalance);
			AssertEquals(new ZDateTime(2005, 7, 27), writer.Date);
			AssertEquals(new ZDateTime(2005, 8, 27), writer.DueDate);
			AssertEquals(LedgerTypes.AccountsReceivable, writer.Ledger);
		}

		public void TestGetNextDataWriterHasLongDoubleMoneyValue()
		{
			Importer.ReadData();
			AssertNotNull(Importer);
			AssertNotNull(Importer.Table);
			AssertNotNull(Importer.Table.Rows);
			Assert(Importer.Table.Rows.Count > 0);
			Importer.Table.Rows[0][Cyber2Schema.Invoice.LocalBalance] = Convert.ToString(90.1680000000061);
			Importer.Table.Rows[0][Cyber2Schema.Invoice.ForeignBalance] = Convert.ToString(70.1680000000061);
			var writer = (LedgerWriter)Importer.GetNextDataWriter(Factory);

			AssertEquals((ZDecimal)90.17, writer.LocalBalance);
			AssertEquals((ZDecimal)70.17, writer.ForeignBalance);
		}

		protected override int RecordCountInTestData
		{
			get { return 1; }
		}

		protected override ZString ExpectedSqlText
		{
			get
			{
				return new ZString(@"select accountID, branch, number, balance, balancef, Curr_name1, doc_date, datedue, amt_local, amt_foreig, d_c_flag" +
					  " from acc_invoices where (balance>= 0.005 or balance <= -0.005) and d_c_flag = 'C' order by accountid");
			}
		}

		protected new InvoiceDataImporter Importer
		{
			get { return (InvoiceDataImporter)base.Importer; }
		}

		protected override DataImporter GetDataImporter(ProgressLogger logger)
		{
			return new InvoiceImporterForTesting(logger, "", true, "C");
		}

		#region class InvoiceImporterForTesting
		sealed internal class InvoiceImporterForTesting : InvoiceDataImporter
		{
			public InvoiceImporterForTesting(ProgressLogger logger, ZString dataSourcePath, bool toCSV, ZString accountType)
				: base(logger, "", toCSV, accountType)
			{
			}

			protected override DataTable GetDataTableFromSqlText()
			{
				return new DataTableCreator().GetTable("Invoice");
			}
		}
		#endregion
	}
}
