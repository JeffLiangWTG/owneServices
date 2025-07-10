using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryLineCollection))]
	class CusEntryLineCollectionTest : Customs.Business.Testing.CusEntryLineCollectionAbstractTest<CusEntryLine, CusEntryLineCollection>
	{
		public void TestHasALineWithPUPIndicator()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();

			CusEntryHeader entry1 = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();

			CusEntryHeader entry2 = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			line1.JI_CL = entryLine1.PK;
			line2.JI_CL = entryLine2.PK;

			AssertEquals("Entry1 HasALineWithPUPIndicator", false, entry1.MergedLines.HasALineWithPUPIndicator);
			AssertEquals("Entry2 HasALineWithPUPIndicator", false, entry2.MergedLines.HasALineWithPUPIndicator);

			line2.AddInfo.ZA_PUP = "Y";
			line1.AddInfo.ZA_PUP = "N";
			AssertEquals("Entry1 HasALineWithPUPIndicator", false, entry1.MergedLines.HasALineWithPUPIndicator);
			AssertEquals("Entry2 HasALineWithPUPIndicator", true, entry2.MergedLines.HasALineWithPUPIndicator);
		}

		public void TestAreAllCPQuestionsAnswered()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("All mandatory questions are answered", true, entryHeader.MergedLines.AreAllCPQuestionsAnswered);

			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CMRCusEntryCPDec question = entryLine.Questions.AddNew();
			AssertEquals("Not answered yet", false, question.IsAnswered);
			AssertEquals("All mandatory questions are not answered", false, entryHeader.MergedLines.AreAllCPQuestionsAnswered);

			question.ON_AnswerCode = "Y";
			AssertEquals("Answered now", true, question.IsAnswered);
			AssertEquals("All mandatory questions are answered", true, entryHeader.MergedLines.AreAllCPQuestionsAnswered);

			CMRCusEntryCPDec q2 = entryLine.Questions.AddNew();
			AssertEquals("All mandatory questions are not answered", false, entryHeader.MergedLines.AreAllCPQuestionsAnswered);
			q2.ON_AnswerCode = "N";
			AssertEquals("All mandatory questions are answered", true, entryHeader.MergedLines.AreAllCPQuestionsAnswered);
		}

		public void TestIsRefundLikely()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			var mockEntryLine1 = Factory.NewMoq<CusEntryLine>();
			mockEntryLine1.Setup(m => m.TotalDutyTaxAdvisedInLastClearanceMessage).Returns(new ZDecimal(100m));
			mockEntryLine1.Setup(m => m.CurrentTotalDutyTax).Returns(new ZDecimal(101m));
			entryHeader.MergedLines.Add(mockEntryLine1.Object);
			AssertEquals("IsRefundLikely", false, entryHeader.MergedLines.IsRefundLikely);

			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			AssertEquals("IsCustomsChargePaid", true, entryHeader.IsCustomsChargePaid);
			AssertEquals("IsRefundLikely", false, entryHeader.MergedLines.IsRefundLikely);

			var mockEntryLine2 = Factory.NewMoq<CusEntryLine>();
			mockEntryLine2.Setup(m => m.TotalDutyTaxAdvisedInLastClearanceMessage).Returns(new ZDecimal(100m));
			mockEntryLine2.Setup(m => m.CurrentTotalDutyTax).Returns(new ZDecimal(95m));
			entryHeader.MergedLines.Add(mockEntryLine2.Object);
			AssertEquals("IsRefundLikely", true, entryHeader.MergedLines.IsRefundLikely);
		}

		public void TestHasRefundReason()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.RefundReasonCode = "";
			AssertEquals("HasRefundReason", false, entryHeader.MergedLines.HasRefundReason);

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.RefundReasonCode = "FA";
			AssertEquals("HasRefundReason", true, entryHeader.MergedLines.HasRefundReason);
		}

		public void TestIsABNQuotedForLCTAndWET()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceLine.AddInfo.ZA_WETQ = "Y";
			invoiceLine2.AddInfo.ZA_WETQ = "Y";
			AssertEquals("IsABNQuoted for WET", true, entryHeader.MergedLines.IsABNQuotedForLCTAndWET);

			invoiceLine.AddInfo.ZA_WETQ = "Y";
			invoiceLine2.AddInfo.ZA_WETQ = "N";
			AssertEquals("IsABNQuoted for WET", true, entryHeader.MergedLines.IsABNQuotedForLCTAndWET);

			invoiceLine.AddInfo.ZA_WETQ = "N";
			AssertEquals("Is not ABNQuoted for WET", false, entryHeader.MergedLines.IsABNQuotedForLCTAndWET);
		}

		public void TestSortByLineNumberAndParentTrailer()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CusEntryHeader header = factory.New<CusEntryHeader>();
			CusEntryLineCollection collection = new CusEntryLineCollection(header, factory);
			TestHelperCusEntryLine line1 = factory.New<TestHelperCusEntryLine>();
			collection.Add(line1);
			TestHelperCusEntryLine line2 = factory.New<TestHelperCusEntryLine>();
			collection.Add(line2);
			TestHelperCusEntryLine line3 = factory.New<TestHelperCusEntryLine>();
			collection.Add(line3);
			line1.CL_LineNumber = 2;
			line1.fIsParent = false;
			line1.fIsTrailer = true;

			line2.CL_LineNumber = 2;
			line2.fIsParent = true;
			line2.fIsTrailer = false;

			line3.CL_LineNumber = 1;
			line3.fIsParent = true;
			line3.fIsTrailer = false;

			AssertEquals("Item1", line1, collection[0]);
			AssertEquals("Item2", line2, collection[1]);
			AssertEquals("Item3", line3, collection[2]);

			//Collection.SortByLineNumberAndParentTrailer();
			collection.CustomSort();

			AssertEquals("Item1", line3, collection[0]);
			AssertEquals("Item2", line2, collection[1]);
			AssertEquals("Item3", line1, collection[2]);
		}

		public void TestSortByLineNumberAndParentTrailerAfterSave()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobDeclaration declaration = factory.New<JobDeclaration>();

			JobComInvoiceHeader jHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine jLine1 = jHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine jLine2 = jHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine jLine3 = jHeader.JobComInvoiceLines.AddNew();
			CusEntryHeader header = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLineCollection collection = new CusEntryLineCollection(header, factory);
			TestHelperCusEntryLine line1 = factory.New<TestHelperCusEntryLine>();
			collection.Add(line1);
			TestHelperCusEntryLine line2 = factory.New<TestHelperCusEntryLine>();
			collection.Add(line2);
			TestHelperCusEntryLine line3 = factory.New<TestHelperCusEntryLine>();
			collection.Add(line3);

			jLine1.JI_CL = line1.PK;
			jLine2.JI_CL = line2.PK;
			jLine3.JI_CL = line3.PK;

			line1.CL_LineNumber = 3;

			line2.CL_LineNumber = 2;

			line3.CL_LineNumber = 1;

			AssertEquals("Item1", line1, collection[0]);
			AssertEquals("Item2", line2, collection[1]);
			AssertEquals("Item3", line3, collection[2]);

			factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			CusEntryHeader loadedHeader = newFactory.Load<CusEntryHeader>(header.PK);
			//LoadedHeader.MergedLines.SortByLineNumberAndParentTrailer();
			loadedHeader.MergedLines.CustomSort();

			AssertEquals("Item1", line3.CL_LineNumber, loadedHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("Item2", line2.CL_LineNumber, loadedHeader.MergedLines[1].CL_LineNumber);
			AssertEquals("Item3", line1.CL_LineNumber, loadedHeader.MergedLines[2].CL_LineNumber);
		}

		protected override CusEntryLineCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			return new CusEntryLineCollection(header, Factory);
		}

		public class TestHelperCusEntryLine : CusEntryLine
		{
			public TestHelperCusEntryLine(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override bool IsParent
			{
				get
				{
					return fIsParent;
				}
			}
			public bool fIsParent;

			public override bool IsTrailer
			{
				get
				{
					return fIsTrailer;
				}
			}
			public bool fIsTrailer;
		}
	}
}
