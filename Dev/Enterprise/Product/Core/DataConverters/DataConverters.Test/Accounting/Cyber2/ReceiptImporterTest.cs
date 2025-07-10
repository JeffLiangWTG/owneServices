using System;
using System.Data;
using CargoWise.Types;
using Enterprise.DataConverters.Accounting.Cyber2;
using Enterprise.DataConverters.Testing.DataImporters.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataConverters.Testing.Accounting.Cyber2
{
	class ReceiptImporterTest : LedgerDataImporterBase
	{
		public void TestConvertToNegative()
		{
			AssertEquals(1452.23m, Importer.ConvertToNegative(-1452.23m));
			AssertEquals(-279.56m, Importer.ConvertToNegative(279.56m));
		}

		public override void TestGetsFirstRecordFromSampleFileAndFillsItInCorrectly()
		{
			AssertEquals("DataImporter.ReadData(path)", true, Importer.ReadData());
			AssertEquals("Reader.HasRecordsNotProcessedYet after reading data", true, Importer.HasRecordsNotProcessedYet);

			var writer = (LedgerWriter)Importer.GetNextDataWriter(Factory);

			AssertEquals("AccountId", writer.Account);
			AssertEquals("Branch", writer.Branch);
			AssertEquals("AUD", writer.Currency);
			AssertEquals(new ZDateTime(2005, 7, 27), writer.Date);
			AssertEquals(new ZDateTime(2005, 7, 27), writer.DueDate);
			AssertEquals(-100.0m, writer.LocalBalance);
			AssertEquals("1029", writer.Reference);
			AssertEquals(LedgerTypes.AccountsPayable, writer.Ledger);
		}

		public void TestGetNextDataWriterHasLongDoubleMoneyValue()
		{
			Importer.ReadData();
			AssertNotNull(Importer);
			AssertNotNull(Importer.Table);
			AssertNotNull(Importer.Table.Rows);
			Assert(Importer.Table.Rows.Count > 0);
			Importer.Table.Rows[0][Cyber2Schema.Receipt.Balance] = Convert.ToString(90.1680000000061);
			var writer = (LedgerWriter)Importer.GetNextDataWriter(Factory);

			AssertEquals((ZDecimal)(-90.17), writer.LocalBalance);
		}

		protected override int RecordCountInTestData
		{
			get { return 1; }
		}

		protected override ZString ExpectedSqlText
		{
			get { return new ZString("select * from acc_receipts where d_c_flag = 'C' and (balance >= 0.005 or balance <= -0.005) order by accountid"); }
		}

		protected new ReceiptDataImporter Importer
		{
			get { return (ReceiptDataImporter)base.Importer; }
		}

		protected override DataImporter GetDataImporter(ProgressLogger logger)
		{
			return new ReceiptImporterForTesting(logger, "", true, "C");
		}

		#region class ReceiptImporterForTesting
		sealed internal class ReceiptImporterForTesting : ReceiptDataImporter
		{
			public ReceiptImporterForTesting(ProgressLogger logger, ZString dataSourcePath, ZBool importToCSVFile, ZString accountType)
				: base(logger, dataSourcePath, importToCSVFile, accountType)
			{
			}

			protected override DataTable GetDataTableFromSqlText()
			{
				return new DataTableCreator().GetTable("Receipt");
			}
		}
		#endregion
	}
}
