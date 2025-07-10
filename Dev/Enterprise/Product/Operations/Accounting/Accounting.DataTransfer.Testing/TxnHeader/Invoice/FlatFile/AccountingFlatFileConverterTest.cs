using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.IO;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile.Testing
{
	public abstract class AccountingFlatFileConverterTest : TestCaseWithFactory
	{
		protected abstract AccountingFlatFileConverter GetConverter
		{
			get;
		}

		ZString GetRows(IValueObject value, AccountingFlatFileConverter converter)
		{
			ZString result = ZString.Empty;
			CsvFlatFileFormat format = new CsvFlatFileFormat();

			using (Stream fileInMemory = new MemoryStream())
			{
				StreamWriter writer = new StreamWriter(fileInMemory);
				StreamReader reader = new StreamReader(fileInMemory);

				converter.ExportFlatFile(value, format, writer);
				writer.Flush();

				fileInMemory.Position = 0;

				using (FileLineIterator file = new FileLineIterator(fileInMemory))
				{
					foreach (string line in file)
					{
						result += line + System.Environment.NewLine;
					}
				}
			}

			return result;
		}

		public void TestWipsPostingAndWipReversalsAreExportedDifferently()
		{
			Xsd.WipOrAccrual wip = new Xsd.WipOrAccrual();
			wip.LineType = Xsd.WipOrAccrualLineType.REV;

			wip.PostOrReverse = Xsd.WipOrAccrualPostOrReverse.P;
			wip.PostOrReverseDate = new ZDateTime(2005, 01, 01, 11, 00, 00);
			wip.PostOrReversePeriod = "200501";
			ZString wipPostingRows = GetRows(wip, GetConverter);

			wip.PostOrReverse = Xsd.WipOrAccrualPostOrReverse.R;
			wip.PostOrReverseDate = new ZDateTime(2005, 02, 01, 11, 00, 00);
			wip.PostOrReversePeriod = "200502";
			ZString wipReversingRows = GetRows(wip, GetConverter);

			ZString message = "The result of exporting a wip should be different if we are processing the posting or reversing";
			message += System.Environment.NewLine + System.Environment.NewLine + "Posting Result: " + System.Environment.NewLine;
			message += wipPostingRows;
			message += System.Environment.NewLine + System.Environment.NewLine + "Reversing Result: " + System.Environment.NewLine;
			message += wipReversingRows;
			Assert(message, wipPostingRows != wipReversingRows);

			Assert("Result of Processing an exported WipPosting should not be ZString.Empty: ", !wipPostingRows.IsEmpty);
			Assert("Result of Processing an exported WipReversing should not be ZString.Empty: ", !wipReversingRows.IsEmpty);
		}

		public void TestAccrualsPostingAndAccrualsReversalsAreExportedDifferently()
		{
			Xsd.WipOrAccrual postingAccrual = new Xsd.WipOrAccrual();
			postingAccrual.LineType = Xsd.WipOrAccrualLineType.CST;
			postingAccrual.PostOrReverse = Xsd.WipOrAccrualPostOrReverse.P;
			ZString accrualPostingRows = GetRows(postingAccrual, GetConverter);

			Xsd.WipOrAccrual reversingAccrual = new Xsd.WipOrAccrual();
			postingAccrual.LineType = Xsd.WipOrAccrualLineType.CST;
			postingAccrual.PostOrReverse = Xsd.WipOrAccrualPostOrReverse.R;
			ZString accrualReversingRows = GetRows(reversingAccrual, GetConverter);

			ZString message = "The result of exporting an accrual should be different if we are processing the posting or reversing";
			message += System.Environment.NewLine + System.Environment.NewLine;
			message += "Posting Result: " + System.Environment.NewLine + accrualPostingRows + System.Environment.NewLine + System.Environment.NewLine;
			message += "Reversing Result: " + System.Environment.NewLine + accrualReversingRows;
			Assert(message, accrualPostingRows != accrualReversingRows);

			Assert("Result of Processing an exported AccrualPosting should not be ZString.Empty: ", !accrualPostingRows.IsEmpty);
			Assert("Result of Processing an exported AccrualReversing should not be ZString.Empty: ", !accrualReversingRows.IsEmpty);
		}

		public void TestInvoiceCountIsIncreasedWhenExportingAnInvoice()
		{
			AccountingFlatFileConverter converter = GetConverter;
			AssertTransactionsProcessed(converter, 0, 0, 0, 0, 0, 0, 0);

			if (converter.CheckThatAllTransactionsAreExported)
			{
				GetRows(GetARInvoice, converter);
				AssertTransactionsProcessed(converter, 1, 0, 0, 0, 0, 0, 0);

				GetRows(GetAPInvoice, converter);
				AssertTransactionsProcessed(converter, 2, 0, 0, 0, 0, 0, 0);
			}
		}

		public void TestCreditNoteCountIsIncreasedWhenExportingACreditNote()
		{
			AccountingFlatFileConverter converter = GetConverter;
			AssertTransactionsProcessed(converter, 0, 0, 0, 0, 0, 0, 0);

			if (converter.CheckThatAllTransactionsAreExported)
			{
				GetRows(GetARCreditNote, converter);
				AssertTransactionsProcessed(converter, 0, 1, 0, 0, 0, 0, 0);

				GetRows(GetAPCreditNote, converter);
				AssertTransactionsProcessed(converter, 0, 2, 0, 0, 0, 0, 0);
			}
		}

		public void TestAdjustmentNoteCountIsIncreasedWhenExportingAnAdjustmentNote()
		{
			AccountingFlatFileConverter converter = GetConverter;
			AssertTransactionsProcessed(converter, 0, 0, 0, 0, 0, 0, 0);

			if (converter.CheckThatAllTransactionsAreExported)
			{
				GetRows(GetARAdjustmentNote, converter);
				AssertTransactionsProcessed(converter, 0, 0, 1, 0, 0, 0, 0);

				GetRows(GetAPAdjustmentNote, converter);
				AssertTransactionsProcessed(converter, 0, 0, 2, 0, 0, 0, 0);
			}
		}

		public void TestWipPostingCountIsIncreasedWhenExportingAWipPosting()
		{
			AccountingFlatFileConverter converter = GetConverter;
			AssertTransactionsProcessed(converter, 0, 0, 0, 0, 0, 0, 0);

			if (converter.CheckThatAllTransactionsAreExported)
			{
				GetRows(GetWipPosting, converter);
				AssertTransactionsProcessed(converter, 0, 0, 0, 1, 0, 0, 0);

				GetRows(GetWipPosting, converter);
				AssertTransactionsProcessed(converter, 0, 0, 0, 2, 0, 0, 0);
			}
		}

		public void TestWipReversingCountIsIncreasedWhenExportingAWipRevsering()
		{
			AccountingFlatFileConverter converter = GetConverter;
			AssertTransactionsProcessed(converter, 0, 0, 0, 0, 0, 0, 0);

			if (converter.CheckThatAllTransactionsAreExported)
			{
				GetRows(GetWipReversing, converter);
				AssertTransactionsProcessed(converter, 0, 0, 0, 0, 1, 0, 0);

				GetRows(GetWipReversing, converter);
				AssertTransactionsProcessed(converter, 0, 0, 0, 0, 2, 0, 0);
			}
		}

		public void TestAccrualPostingCountIsIncreasedWhenExportingAAccrualPosting()
		{
			AccountingFlatFileConverter converter = GetConverter;
			AssertTransactionsProcessed(converter, 0, 0, 0, 0, 0, 0, 0);

			if (converter.CheckThatAllTransactionsAreExported)
			{
				GetRows(GetAccrualPosting, converter);
				AssertTransactionsProcessed(converter, 0, 0, 0, 0, 0, 1, 0);

				GetRows(GetAccrualPosting, converter);
				AssertTransactionsProcessed(converter, 0, 0, 0, 0, 0, 2, 0);
			}
		}

		public void TestAccrualReversingCountIsIncreasedWhenExportingAAccrualRevsering()
		{
			AccountingFlatFileConverter converter = GetConverter;
			AssertTransactionsProcessed(converter, 0, 0, 0, 0, 0, 0, 0);

			if (converter.CheckThatAllTransactionsAreExported)
			{
				GetRows(GetAccrualReversing, converter);
				AssertTransactionsProcessed(converter, 0, 0, 0, 0, 0, 0, 1);

				GetRows(GetAccrualReversing, converter);
				AssertTransactionsProcessed(converter, 0, 0, 0, 0, 0, 0, 2);
			}
		}

		#region Implementation

		Xsd.TxnHeader GetARInvoice
		{
			get
			{
				Xsd.TxnHeader txnHeader = GetTxnHeaderForTest();
				txnHeader.Ledger = Xsd.TxnLedgerType.AR;
				txnHeader.TxnType = Xsd.TxnType.INV;
				return txnHeader;
			}
		}

		Xsd.TxnHeader GetAPInvoice
		{
			get
			{
				Xsd.TxnHeader txnHeader = GetTxnHeaderForTest();
				txnHeader.Ledger = Xsd.TxnLedgerType.AP;
				txnHeader.TxnType = Xsd.TxnType.INV;
				return txnHeader;
			}
		}

		Xsd.TxnHeader GetARCreditNote
		{
			get
			{
				Xsd.TxnHeader txnHeader = GetTxnHeaderForTest();
				txnHeader.Ledger = Xsd.TxnLedgerType.AR;
				txnHeader.TxnType = Xsd.TxnType.CRD;
				return txnHeader;
			}
		}

		Xsd.TxnHeader GetAPCreditNote
		{
			get
			{
				Xsd.TxnHeader txnHeader = GetTxnHeaderForTest();
				txnHeader.Ledger = Xsd.TxnLedgerType.AP;
				txnHeader.TxnType = Xsd.TxnType.CRD;
				return txnHeader;
			}
		}

		Xsd.TxnHeader GetARAdjustmentNote
		{
			get
			{
				Xsd.TxnHeader txnHeader = GetTxnHeaderForTest();
				txnHeader.Ledger = Xsd.TxnLedgerType.AR;
				txnHeader.TxnType = Xsd.TxnType.ADJ;
				return txnHeader;
			}
		}

		Xsd.TxnHeader GetAPAdjustmentNote
		{
			get
			{
				Xsd.TxnHeader txnHeader = GetTxnHeaderForTest();
				txnHeader.Ledger = Xsd.TxnLedgerType.AP;
				txnHeader.TxnType = Xsd.TxnType.ADJ;
				return txnHeader;
			}
		}

		protected virtual Xsd.TxnHeader GetTxnHeaderForTest()
		{
			return new Xsd.TxnHeader();
		}

		Xsd.WipOrAccrual GetWipPosting
		{
			get
			{
				Xsd.WipOrAccrual wipOrAccrual = GetWipOrAccrualForTest();
				wipOrAccrual.LineType = Xsd.WipOrAccrualLineType.REV;
				wipOrAccrual.PostOrReverse = Xsd.WipOrAccrualPostOrReverse.P;
				return wipOrAccrual;
			}
		}

		Xsd.WipOrAccrual GetWipReversing
		{
			get
			{
				Xsd.WipOrAccrual wipOrAccrual = GetWipOrAccrualForTest();
				wipOrAccrual.LineType = Xsd.WipOrAccrualLineType.REV;
				wipOrAccrual.PostOrReverse = Xsd.WipOrAccrualPostOrReverse.R;
				return wipOrAccrual;
			}
		}

		Xsd.WipOrAccrual GetAccrualPosting
		{
			get
			{
				Xsd.WipOrAccrual wipOrAccrual = GetWipOrAccrualForTest();
				wipOrAccrual.LineType = Xsd.WipOrAccrualLineType.CST;
				wipOrAccrual.PostOrReverse = Xsd.WipOrAccrualPostOrReverse.P;
				return wipOrAccrual;
			}
		}

		Xsd.WipOrAccrual GetAccrualReversing
		{
			get
			{
				Xsd.WipOrAccrual wipOrAccrual = GetWipOrAccrualForTest();
				wipOrAccrual.LineType = Xsd.WipOrAccrualLineType.CST;
				wipOrAccrual.PostOrReverse = Xsd.WipOrAccrualPostOrReverse.R;
				return wipOrAccrual;
			}
		}

		protected virtual Xsd.WipOrAccrual GetWipOrAccrualForTest()
		{
			Xsd.WipOrAccrual wipOrAccrual = new Xsd.WipOrAccrual();
			wipOrAccrual.PostOrReverseDate = new ZDateTime(2005, 01, 01, 11, 0, 0);
			wipOrAccrual.PostOrReversePeriod = "200501";
			return wipOrAccrual;
		}

		protected void AssertTransactionsProcessed(AccountingFlatFileConverter converter, int invoices, int creditNotes, int adjustmentNotes,
			int wipPosting, int wipReversing, int accrualPosting, int accrualReversing)
		{
			AssertNotNull("AccountingFlatFileConverter should not be null", converter);
			AssertEquals("Invoices Processed", invoices, converter.NumberOfInvoicesProcessed);
			AssertEquals("Credit Notes Processed", creditNotes, converter.NumberOfCreditNotesProcessed);
			AssertEquals("Adjustment Notes Processed", adjustmentNotes, converter.NumberOfAdjustmentNotesProcessed);
			AssertEquals("Wip Posting Processed", wipPosting, converter.NumberOfWipPostingProcessed);
			AssertEquals("Wip Reversing Processed", wipReversing, converter.NumberOfWipReversingProcessed);
			AssertEquals("Accrual Posting Processed", accrualPosting, converter.NumberOfAccrualPostingProcessed);
			AssertEquals("Accrual Reversing Processed", accrualReversing, converter.NumberOfAccrualReversingProcessed);
		}

		#endregion
	}
}
