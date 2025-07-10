using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class Box40AmendmentLineWrapperTest : WrapperHelperTest<Box40AmendmentLineWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("CusEntryLine", () => new Box40AmendmentLineWrapper(null));
				AssertExceptionThrown<ArgumentOutOfRangeException>("No MergedLines", () => new Box40AmendmentLineWrapper(Factory.New<CusEntryLine>()));

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.FillWithValidTestData();
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

				AssertExceptionThrown<ArgumentNullException>("No Documents", () => new Box40AmendmentLineWrapper(entryLine));
			});
		}

		public void TestLineNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals("Expected filled LineNumber", 2, wrapper.LineNumber);
		}

		public void TestPrecedentDocumentType()
		{
			AssertEquals("Expected filled PrecedentDocumentType", EntryLineData.PrevDocSubType, wrapper.PrecedentDocumentType);
		}

		public void TestPrecedentDocumentClass()
		{
			AssertEquals("Expected filled PrecedentDocumentClass", EntryLineData.PrevDocCode, wrapper.PrecedentDocumentClass);
		}

		public void TestPrecedentDocumentReference()
		{
			CombineAssertions(() =>
			{
				previousDoc.CSI_ReferenceNumber = "Reference";
				previousDoc.CSI_Code = "SUM";
				previousDoc.CSI_LineNo = 1;
				wrapper = new Box40AmendmentLineWrapper(entryLine);
				AssertEquals("For SUM previous documents reference number must have line number attached when not 0", "Reference00001", wrapper.PrecedentDocumentReference);

				previousDoc.CSI_ReferenceNumber = "Reference2";
				previousDoc.CSI_LineNo = 0;
				wrapper = new Box40AmendmentLineWrapper(entryLine);
				AssertEquals("For SUM previous documents reference number must not have line number attached when 0", "Reference2", wrapper.PrecedentDocumentReference);

				previousDoc.CSI_ReferenceNumber = "Reference3";
				previousDoc.CSI_Code = "IRR";
				previousDoc.CSI_LineNo = 5;
				wrapper = new Box40AmendmentLineWrapper(entryLine);
				AssertEquals("For non SUM previous documents reference number must not have line number attached", "Reference3", wrapper.PrecedentDocumentReference);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();

			previousDoc = invoiceLine.PreviousDocuments.AddNew();
			previousDoc.CSI_SubType = EntryLineData.PrevDocSubType;
			previousDoc.CSI_Code = EntryLineData.PrevDocCode;
			previousDoc.CSI_ReferenceNumber = EntryLineData.PrevDocRefNum;

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = new Box40AmendmentLineWrapper(entryLine);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		PreviousDocument previousDoc;
		Box40AmendmentLineWrapper wrapper;

		protected override Box40AmendmentLineWrapper GetProvider() => wrapper;
	}
}
