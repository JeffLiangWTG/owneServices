using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class Box44LineWrapperTest : WrapperHelperTest<Box44LineWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("CusEntryLine", () => new Box44LineWrapper(null));
				AssertExceptionThrown<ArgumentOutOfRangeException>("No MergedLines", () => new Box44LineWrapper(Factory.New<CusEntryLine>()));
			});
		}

		public void TestLineNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals("Expected filled LineNumber", 2, wrapper.LineNumber);
		}

		public void TestDocumentsAndCertificates()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DocumentsAndCertificates list", 0, wrapper.DocumentsAndCertificates.Count);

				var supdoc1 = declaration.SupportingDocuments.AddNew();
				supdoc1.CSI_Code = "9001";

				var supdoc2 = entryInstruction.SupportingDocuments.AddNew();
				supdoc2.CSI_Code = "9002";

				var supdoc3 = invoice.SupportingDocuments.AddNew();
				supdoc3.CSI_Code = "9003";

				var supdoc4 = invoiceLine.SupportingDocuments.AddNew();
				supdoc4.CSI_Code = "9004";

				var supdoc5 = invoiceLine.SupportingDocuments.AddNew();
				supdoc5.CSI_Code = "9005";

				var supdoc6 = invoiceLine.SupportingDocuments.AddNew();
				supdoc6.CSI_Code = "9006";

				var supdoc7 = Factory.New<SupportingDocument>();
				supdoc7.CSI_ParentID = entryLine.PK;
				supdoc7.CSI_ParentTableCode = entryLine.TablePrefix;
				supdoc7.CSI_Status = "ACC";
				supdoc7.CSI_Code = "9006";

				wrapper = new Box44LineWrapper(entryLine);
				var documents = wrapper.DocumentsAndCertificates;

				AssertEquals("Expected filled DocumentsAndCertificates", 5, documents.Count);
				AssertSame("Cached DocumentsAndCertificates", wrapper.DocumentsAndCertificates, documents);
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

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = new Box44LineWrapper(entryLine);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		Box44LineWrapper wrapper;

		protected override Box44LineWrapper GetProvider() => wrapper;
	}
}
