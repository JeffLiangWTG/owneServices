using Enterprise.Accounting.Business.DataInterface;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers
{
	[TestedType(typeof(DocChinaJournalListingLine))]
	sealed class DocChinaJournalListingLineTest : DocumentWrapperTestCase
	{
		public void TestVoucherDescription()
		{
			AssertEquals("Voucher Description", DocChinaJournalListingLineToTest.VoucherDescription);
		}

		public void TestCreditAmount()
		{
			AssertEquals(70m, DocChinaJournalListingLineToTest.CreditAmount);
		}

		public void TestDebitAmount()
		{
			AssertEquals(120m, DocChinaJournalListingLineToTest.DebitAmount);
		}

		public void TestOriginalAmount()
		{
			AssertEquals(170m, DocChinaJournalListingLineToTest.OriginalAmount);
		}

		public void TestVoucherDate()
		{
			AssertEquals("2004-03-06", DocChinaJournalListingLineToTest.VoucherDate);
		}

		public void TestVoucherNumber()
		{
			AssertEquals("23842", DocChinaJournalListingLineToTest.VoucherNumber);
		}

		public void TestVoucherType()
		{
			AssertEquals("INV", DocChinaJournalListingLineToTest.VoucherType);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocChinaJournalListingLine.New(new ChinaJournal(), Factory) };
		}

		ChinaJournal fChinaJournal;

		ChinaJournal ChinaJournal
		{
			get
			{
				if (fChinaJournal == null)
				{
					fChinaJournal = new ChinaJournal();
					SetChinaJournalLine(fChinaJournal);
				}
				return fChinaJournal;
			}
		}

		DocChinaJournalListingLine fDocChinaJournalListingLineTest;

		DocChinaJournalListingLine DocChinaJournalListingLineToTest
		{
			get { return fDocChinaJournalListingLineTest ?? (fDocChinaJournalListingLineTest = DocChinaJournalListingLine.New(ChinaJournal, Factory)); }
		}

		void SetChinaJournalLine(ChinaJournal line)
		{
			line.VoucherDescription = "Voucher Description";
			line.GLAccountNumber = "1010.101";
			line.Attachments = 1;
			line.VoucherDate = "2004-03-06";
			line.VoucherNumber = "23842";
			line.VoucherType = "INV";
			line.CurrencyCode = "USD";
			line.DebitAmountLocalCurrency = 120m;
			line.CreditAmountLocalCurrency = 70m;
			line.OriginalAmount = 170m;
		}

		#endregion
	}
}
