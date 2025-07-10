using System.IO;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DataConverters.Testing.Base;
using Enterprise.Environment;

namespace Enterprise.DataConverters.Testing.DataWriters
{
	sealed internal class LedgerWriterTest : DataWriterTestCase
	{
		public new void TestSaveRecordToEnterprise()
		{
			var writer = GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(writer);
			writer.SaveRecordToEnterprise(false, Logger);
			AssertEquals("UpdateValuesInLedgerDataWriter", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public override void TestAllFieldsInRecordAreImportedProperly()
		{
			var logger = new ProgressLogger();
			var writer = (LedgerWriter)GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(writer);
			var filePath = ZString.Empty;
			try
			{
				filePath = Env.GetTempFileName(Env.TempPath, "csv");
				writer.AddRecordToCSVFile(filePath, logger);
				using (var reader = new StreamReader(filePath))
				{
					var line = reader.ReadLine();
					AssertEquals(line, ExpectedLine);
				}
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		public void TestRecordDescription()
		{
			var logger = new ProgressLogger();
			var writer = (LedgerWriter)GetNewDataWriter();
			writer.Account = "ACCOUNT";
			writer.Reference = "00001";
			AssertEquals("Transaction: ACCOUNT/00001", writer.RecordDescription);
		}

		public void TestLine()
		{
			var writer = (LedgerWriter)GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(writer);
			AssertEquals(ExpectedLine, writer.CSVOutputLine);
		}

		protected override DataWriter GetNewDataWriter()
		{
			return new LedgerWriter(Factory);
		}

		protected override void FillInRecordWithUniqueAndCompleteDetails(DataWriter writer)
		{
			var ledger = (LedgerWriter)writer;

			ledger.Account = "Account";
			ledger.Reference = "Reference";
			ledger.Branch = "Branch";
			ledger.Currency = "AUD";
			ledger.Department = "Dep";
			ledger.Date = CurrentDate;
			ledger.DueDate = CurrentDate.AddDays(1);
			ledger.ForeignBalance = 100.0m;
			ledger.LocalBalance = 150.0m;
			ledger.Ledger = "C";
		}

		protected override void FillInRecordWithInvalidDetails(DataWriter writer)
		{
			var ledger = (LedgerWriter)writer;
			ledger.LocalBalance = 0.0m;
			ledger.ForeignBalance = 0.0m;
		}

		#region Implementation

		ZDateTime CurrentDate;
		ZString ExpectedLine;
		protected override void SetUp()
		{
			base.SetUp();
			CurrentDate = new ZDateTime(2005, 7, 27);
			ExpectedLine = "Account,Reference,20050727,20050728,AUD,100.0,150.0,Branch,Dep";
		}

		#endregion

	}
}
