using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ComplXExportLineWrapperTest : WrapperHelperTest<ComplXExportLineWrapper>
	{
		public void TestGrossWeightInKG()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Weight = 200.4455M;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

				invoiceLine.JI_Procedure = EntryLineData.Procedure;
				AssertEquals("Expected filled GrossWeightInKG when there is NOT a 9VA concession code in Procedure (or AdditionalProcedures) rounded to the upper integer unit", 201M, wrapper.GrossWeightInKG);

				invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.AddProcedure1);
				AssertEquals("Expected filled GrossWeightInKG when there is NOT a 9VA concession code in AdditionalProcedures (or Procedure) rounded to the upper integer unit", 201M, wrapper.GrossWeightInKG);

				invoiceLine.JI_Procedure = EntryLineData.Procedure9VA;
				AssertEquals("Expected empty GrossWeightInKG when there is a 9VA concession code in Procedure (or AdditionalProcedures)", ZDecimal.Zero, wrapper.GrossWeightInKG);

				invoiceLine.JI_Procedure = EntryLineData.Procedure;
				invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.Procedure9VA);
				AssertEquals("Expected empty GrossWeightInKG when there is a 9VA concession code in AdditionalProcedures (or Procedure)", ZDecimal.Zero, wrapper.GrossWeightInKG);
			});
		}

		public void TestNetWeightInKG()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_NetWeight = EntryLineData.NetWeight;
				invoiceLine.JI_NetWeightUQ = "KG";

				invoiceLine.JI_Procedure = EntryLineData.Procedure;
				AssertEquals("Expected filled NetWeightInKG when there is NOT a 9VA concession code in Procedure (or AdditionalProcedures)", EntryLineData.NetWeightRound, wrapper.NetWeightInKG);

				invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.AddProcedure1);
				AssertEquals("Expected filled NetWeightInKG when there is NOT a 9VA concession code in AdditionalProcedures (or Procedure)", EntryLineData.NetWeightRound, wrapper.NetWeightInKG);

				invoiceLine.JI_Procedure = EntryLineData.Procedure9VA;
				AssertEquals("Expected empty NetWeightInKG when there is a 9VA concession code in Procedure (or AdditionalProcedures)", ZDecimal.Zero, wrapper.NetWeightInKG);

				invoiceLine.JI_Procedure = EntryLineData.Procedure;
				invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.Procedure9VA);
				AssertEquals("Expected empty NetWeightInKG when there is a 9VA concession code in AdditionalProcedures (or Procedure)", ZDecimal.Zero, wrapper.NetWeightInKG);
			});
		}

		public void TestDocuments()
		{
			SetSupportingDocumentsRefData();

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Documents list", 0, wrapper.Documents.Count);

				declaration.SupportingDocuments.Add(GetSupportingDoc("AAA", "REF111"));
				declaration.SupportingDocuments.Add(GetSupportingDoc("N380", "REF222"));
				declaration.SupportingDocuments.Add(GetSupportingDoc("N325", "REF333"));
				entryInstruction.SupportingDocuments.Add(GetSupportingDoc("D005", "REF444"));
				entryInstruction.SupportingDocuments.Add(GetSupportingDoc("D008", "REF555"));
				entryInstruction.SupportingDocuments.Add(GetSupportingDoc("N935", "REF666"));
				invoice.SupportingDocuments.Add(GetSupportingDoc("D005", "REF777"));
				invoice.SupportingDocuments.Add(GetSupportingDoc("D008", "REF888"));
				invoice.SupportingDocuments.Add(GetSupportingDoc("N935", "REF999"));
				invoiceLine.SupportingDocuments.Add(GetSupportingDoc("1001", "REF100"));
				invoiceLine.SupportingDocuments.Add(GetSupportingDoc("1003", "REF200"));
				invoiceLine.SupportingDocuments.Add(GetSupportingDoc("1004", "REF300"));

				var supdocEntry1 = GetSupportingDoc("1003", "REF200");
				supdocEntry1.CSI_ParentID = entryLine.PK;
				supdocEntry1.CSI_ParentTableCode = entryLine.TablePrefix;

				wrapper = new ComplXExportLineWrapper(entryLine);
				var documents = wrapper.Documents;

				AssertEquals("Expected filled Documents", 10, documents.Count);
				AssertSame("Cached Documents", wrapper.Documents, documents);
			});
		}

		public void TestDocumentsRepeated()
		{
			SetSupportingDocumentsRefData();

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Documents list", 0, wrapper.Documents.Count);

				invoiceLine.SupportingDocuments.Add(GetSupportingDoc("N380", "REF111"));
				invoiceLine.SupportingDocuments.Add(GetSupportingDoc("N380", "REF111"));

				var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge", true, mergeResult);

				entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
				wrapper = new ComplXExportLineWrapper(entryLine);
				var documents = wrapper.Documents;

				AssertEquals("Expected filled Documents", 1, documents.Count);
				AssertSame("Cached Documents", wrapper.Documents, documents);
			});
		}

		public void TestDeclaredSameInvoiceDocument()
		{
			SetSupportingDocumentsRefData();

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Documents list", 0, wrapper.Documents.Count);
				var supDocInvoice = GetSupportingDoc("N380", "REF111");
				var supDocInvoice2 = GetSupportingDoc("N325", "REF111");
				var supDocNoInvoice = GetSupportingDoc("N740", "REF111");
				invoiceLine.SupportingDocuments.Add(supDocNoInvoice);

				var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge", true, mergeResult);

				entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
				AddCLSupportingDocument(supDocNoInvoice, entryLine);
				wrapper = new ComplXExportLineWrapper(entryLine);
				var documents = wrapper.Documents;
				AssertEquals("No Invoice documents expected empty Documents", 0, documents.Count);

				invoiceLine.SupportingDocuments.Add(supDocInvoice);
				AddCLSupportingDocument(supDocInvoice, entryLine);
				wrapper = new ComplXExportLineWrapper(entryLine);
				documents = wrapper.Documents;
				var documentInWrapper = documents.FirstOrDefault();
				var expectedDocument = new ExportDocumentCommonWrapper(supDocInvoice);

				AssertEquals("Expected filled Documents", 1, documents.Count);
				AssertSame("Cached Documents", wrapper.Documents, documents);
				AssertEquals("The document Name in wrapper is the Invoice Document Name", expectedDocument.Name, documentInWrapper.Name);
				AssertEquals("The document Number in wrapper is the Invoice Document Number", expectedDocument.Number, documentInWrapper.Number);
				AssertEquals("The document Date of Issue in wrapper is the Invoice Document Date of Issue", expectedDocument.DateOfIssue, documentInWrapper.DateOfIssue);
				AssertEquals("The document Date of Expiry in wrapper is the Invoice Document Date of Expiry", expectedDocument.DateOfExpiry, documentInWrapper.DateOfExpiry);

				invoiceLine.SupportingDocuments.Add(supDocInvoice2);
				AddCLSupportingDocument(supDocInvoice2, entryLine);
				wrapper = new ComplXExportLineWrapper(entryLine);
				documents = wrapper.Documents;
				documentInWrapper = documents.FirstOrDefault();
				expectedDocument = new ExportDocumentCommonWrapper(supDocInvoice);

				AssertEquals("With two Invoice Documents, expected filled only one", 1, documents.Count);
				AssertSame("Cached Documents", wrapper.Documents, documents);
				AssertEquals("The document Name in wrapper is the First Invoice Document Name", expectedDocument.Name, documentInWrapper.Name);
				AssertEquals("The document Number in wrapper is the First Invoice Document Number", expectedDocument.Number, documentInWrapper.Number);
				AssertEquals("The document Date of Issue in wrapper is the First Invoice Document Date of Issue", expectedDocument.DateOfIssue, documentInWrapper.DateOfIssue);
				AssertEquals("The document Date of Expiry in wrapper is the First Invoice Document Date of Expiry", expectedDocument.DateOfExpiry, documentInWrapper.DateOfExpiry);
			});
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			wrapper = new ComplXExportLineWrapper(entryLine);
		}

		void AddCLSupportingDocument(SupportingDocument supportingDocument, CusEntryLine entryLine)
		{
			var supDoc = Factory.New<SupportingDocument>();
			supDoc.CSI_Code = supportingDocument.CSI_Code;
			supDoc.CSI_ReferenceNumber = supportingDocument.CSI_ReferenceNumber;
			supDoc.CSI_SubType = supportingDocument.CSI_SubType;
			supDoc.CSI_Quantity = supportingDocument.CSI_Quantity;
			supDoc.CSI_ParentTableCode = entryLine.TablePrefix;
			supDoc.CSI_ParentID = entryLine.PK;
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		ComplXExportLineWrapper wrapper;

		protected override ComplXExportLineWrapper GetProvider() => wrapper;
	}
}
