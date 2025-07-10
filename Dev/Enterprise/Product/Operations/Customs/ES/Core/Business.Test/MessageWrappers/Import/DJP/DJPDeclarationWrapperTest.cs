using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DJPDeclarationWrapperTest : WrapperHelperTest<DJPDeclarationWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("CusEntryHeader", () => new DJPDeclarationWrapper(null));
				AssertExceptionThrown<ArgumentOutOfRangeException>("No MergerLines", () => new DJPDeclarationWrapper(Factory.New<CusEntryHeader>()));
			});
		}

		public void TestMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MovementReferenceNumber, ZDateTime.Today);
			AssertEquals("Expected filled MRN", MovementReferenceNumber, wrapper.MRN);
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

				var supdoc4 = entryInstruction.SupportingDocuments.AddNew();
				supdoc4.CSI_Code = "9004";
				supdoc4.CSI_Procedure = "A";

				var supdoc5 = entryInstruction.SupportingDocuments.AddNew();
				supdoc5.CSI_Code = "9005";
				supdoc5.CSI_Procedure = "R";

				var supdoc6 = entryInstruction.SupportingDocuments.AddNew();
				supdoc6.CSI_Code = "9006";

				var supdoc7 = invoiceHeader.SupportingDocuments.AddNew();
				supdoc7.CSI_Code = "9007";

				var supdoc8 = invoiceLine.SupportingDocuments.AddNew();
				supdoc8.CSI_Code = "9008";

				wrapper = new DJPDeclarationWrapper(entryHeader);
				var documents = wrapper.Documents;

				AssertEquals("Expected filled Documents with only the documents in declaration and entryInstruction with procedure (A, R, N)", 4, documents.Count);
				AssertSame("Cached Documents", wrapper.Documents, documents);
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "2203001012";

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = new DJPDeclarationWrapper(entryHeader);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = new DJPDeclarationWrapper(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		DJPDeclarationWrapper wrapper;

		protected override DJPDeclarationWrapper GetProvider() => wrapper;
	}
}
