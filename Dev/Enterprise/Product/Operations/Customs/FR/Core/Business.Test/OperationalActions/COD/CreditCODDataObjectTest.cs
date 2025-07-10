using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(CreditCODDataObject))]
	public class CreditCODDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "1";
			entryHeader.CH_BGMReference = "3-B00001000";
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "2";

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			AssertNull(item.ReleasingEntryHeader);

			item.ReleasingEntryReference = "3-B00001000";
			AssertEquals(entryHeader.PK, item.ReleasingEntryHeader.PK);
		}

		public void TestPreviousEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "1";
			entryHeader.CH_BGMReference = "3-B00001000";
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "2";

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			AssertNull(item.PreviousEntryHeader);

			item.PreviousEntryReference = "3-B00001000";
			AssertEquals(entryHeader.PK, item.PreviousEntryHeader.PK);
		}

		public void TestPreviousEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "1";
			entryHeader.CH_BGMReference = "3-B00001000";
			var entryLine11 = entryHeader.AllEntryLines.AddNew();
			entryLine11.CL_LineNumber = 1;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "2";
			var entryLine21 = entryHeader2.AllEntryLines.AddNew();
			entryLine21.CL_LineNumber = 1;

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			AssertNull(item.PreviousEntryLine);

			item.PreviousEntryReference = "3-B00001000";
			item.PreviousEntryLineNo = 1;
			AssertEquals(entryLine11.PK, item.PreviousEntryLine.PK);
		}

		public void TestCreditMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "1";
			entryHeader.CH_BGMReference = "3-B00001000";
			var entryLine11 = entryHeader.AllEntryLines.AddNew();
			entryLine11.CL_LineNumber = 1;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine11.PK;
			invoiceLine.JI_PreviousEntryNumber = "2";
			invoiceLine.JI_PreviousEntryLineNumber = 1;

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.ReleasingEntryReference = "3-B00001000";
			item.PreviousEntryLineNo = 2;
			item.Amount = 5m;
			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			AssertEquals("Credit Previous Entry", item.CreditMethodDescription);
			AssertEquals(ZInt.Zero, item.PreviousEntryLineNo);
			AssertEquals(ZDecimal.Zero, item.Amount);

			item.PreviousEntryLineNo = 2;
			item.Amount = 5m;
			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			AssertEquals("Credit Previous Entry Line", item.CreditMethodDescription);
			AssertEquals(1, item.PreviousEntryLineNo);
			AssertEquals(ZDecimal.Zero, item.Amount);

			item.PreviousEntryLineNo = 2;
			item.Amount = 5m;
			item.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			AssertEquals("Credit Partial Amount from Previous Entry Line", item.CreditMethodDescription);
			AssertEquals(1, item.PreviousEntryLineNo);
			AssertEquals(5m, item.Amount);
		}

		public void TestReleasingEntryNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "1";
			entryHeader.CH_BGMReference = "3-B00001000";
			var entryLine11 = entryHeader.AllEntryLines.AddNew();
			entryLine11.CL_LineNumber = 1;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "2";
			entryHeader2.CH_BGMReference = "3-B00001001";
			var entryLine21 = entryHeader.AllEntryLines.AddNew();
			entryLine21.CL_LineNumber = 1;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine11.PK;
			invoiceLine.JI_PreviousEntryNumber = "2";
			invoiceLine.JI_PreviousEntryLineNumber = 1;

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			item.ReleasingEntryReference = "3-B00001000";
			AssertEquals("3-B00001001", item.PreviousEntryReference);
			AssertEquals(0, item.PreviousEntryLineNo);

			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			item.PreviousEntryReference = "";
			item.ReleasingEntryReference = "0";
			item.ReleasingEntryReference = "3-B00001000";
			AssertEquals("3-B00001001", item.PreviousEntryReference);
			AssertEquals(1, item.PreviousEntryLineNo);

			item.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			item.PreviousEntryReference = "";
			item.PreviousEntryLineNo = 1;
			item.ReleasingEntryReference = "0";
			item.ReleasingEntryReference = "3-B00001000";
			AssertEquals("3-B00001001", item.PreviousEntryReference);
			AssertEquals(1, item.PreviousEntryLineNo);
		}

		public void TestPreviousEntryNo()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			Assert(item.PreviousEntryLineNoInfo.ReadOnly);

			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			Assert(item.PreviousEntryLineNoInfo.ReadOnly);

			item.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			Assert(!item.PreviousEntryLineNoInfo.ReadOnly);

			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			Assert(!item.PreviousEntryLineNoInfo.ReadOnly);
		}

		public void TestAmount()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			Assert(item.AmountInfo.ReadOnly);

			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			Assert(item.AmountInfo.ReadOnly);

			item.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			Assert(!item.AmountInfo.ReadOnly);

			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			Assert(item.AmountInfo.ReadOnly);
		}

		public void TestCurrency()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			Assert(item.CurrencyInfo.ReadOnly);
			AssertEquals("EUR", item.Currency);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			return applicator.FrCreditCODItemApplicators.AddNew();
		}
	}
}
