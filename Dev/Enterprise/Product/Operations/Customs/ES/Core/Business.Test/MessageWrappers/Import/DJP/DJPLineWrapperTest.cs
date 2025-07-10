using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DJPLineWrapperTest : WrapperHelperTest<DJPLineWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("CusEntryLine", () => new DJPLineWrapper(null));
				AssertExceptionThrown<ArgumentOutOfRangeException>("No InvoiceLines", () => new DJPLineWrapper(Factory.New<CusEntryLine>()));
			});
		}

		public void TestLineNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals("Expected filled LineNumber", 2, wrapper.LineNumber);
		}

		public void TestDocuments()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Documents list", 0, wrapper.Documents.Count);

				var supdoc1 = declaration.SupportingDocuments.AddNew();
				supdoc1.CSI_Code = "9001";
				supdoc1.CSI_Procedure = "A";

				var supdoc2 = declaration.SupportingDocuments.AddNew();
				supdoc2.CSI_Code = "9002";
				supdoc2.CSI_Procedure = "R";

				var supdoc3 = declaration.SupportingDocuments.AddNew();
				supdoc3.CSI_Code = "9003";

				var supdoc4 = invoiceHeader.SupportingDocuments.AddNew();
				supdoc4.CSI_Code = "9004";
				supdoc4.CSI_Procedure = "A";

				var supdoc5 = invoiceHeader.SupportingDocuments.AddNew();
				supdoc5.CSI_Code = "9005";
				supdoc5.CSI_Procedure = "N";

				var supdoc6 = invoiceHeader.SupportingDocuments.AddNew();
				supdoc6.CSI_Code = "9006";

				var supdoc7 = invoiceLine1.SupportingDocuments.AddNew();
				supdoc7.CSI_Code = "9007";
				supdoc7.CSI_Procedure = "A";

				var supdoc8 = invoiceLine1.SupportingDocuments.AddNew();
				supdoc8.CSI_Code = "9008";
				supdoc8.CSI_Procedure = "R";

				var supdoc9 = invoiceLine1.SupportingDocuments.AddNew();
				supdoc9.CSI_Code = "9009";

				var supdoc10 = invoiceLine2.SupportingDocuments.AddNew();
				supdoc10.CSI_Code = "9010";
				supdoc10.CSI_Procedure = "A";

				var supdoc11 = invoiceLine2.SupportingDocuments.AddNew();
				supdoc11.CSI_Code = "9011";
				supdoc11.CSI_Procedure = "N";

				var supdoc12 = invoiceLine2.SupportingDocuments.AddNew();
				supdoc12.CSI_Code = "90012";

				wrapper = new DJPLineWrapper(entryLine);
				var documents = wrapper.Documents;

				AssertEquals("Expected filled Documents with only the documents in invoiceHeader and invoiceLines with procedure (A, R, N)", 6, documents.Count);
				AssertSame("Cached Documents", wrapper.Documents, documents);
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

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = new DJPLineWrapper(entryLine);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;
		CusEntryLine entryLine;
		DJPLineWrapper wrapper;

		protected override DJPLineWrapper GetProvider() => wrapper;
	}
}
