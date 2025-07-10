using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.Universal.Testing;
using CusEntryLine = Enterprise.Customs.FR.Business.Declaration.CusEntryLine;
using LineMerger = Enterprise.Customs.FR.Business.Declaration.LineMerger;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	public class SupportingDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestEntryHeaderD48AmountInFirstEntryLineOnly()
		{
			var helper1 = new UniversalReferenceTestDataHelper(Factory);
			helper1.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			var helper2 = new UniversalReferenceTestDataHelper(Factory);
			helper2.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2203001010";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6000000010";

			var headerSupportingDocument = declaration.SupportingDocuments.AddNew();
			headerSupportingDocument.CSI_Code = "0001";
			headerSupportingDocument.CSI_ReferenceNumber = "AAAAAAAA";
			headerSupportingDocument.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			headerSupportingDocument.CSI_Quantity3 = 4;
			headerSupportingDocument.CSI_Value = 1000m;

			var lineSupportingDocument = invoiceLine1.SupportingDocuments.AddNew();
			lineSupportingDocument.CSI_Code = "0001";
			lineSupportingDocument.CSI_ReferenceNumber = "AAAAAAAA";
			lineSupportingDocument.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			lineSupportingDocument.CSI_Quantity3 = 10;
			lineSupportingDocument.CSI_Value = 2000m;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var entryLine1 = entryHeader.MergedLines[0];
			var entryLine2 = entryHeader.MergedLines[1];

			var wrapper1 = new SupportingDocumentWrapper(headerSupportingDocument, entryLine1) as ISupportingDocumentOnly;
			AssertEquals("D48 amount should match supporting document because entry line number is 1 whatever supporting document parent may be", 1000m, wrapper1.D48Amount);
			AssertEquals("", (sbyte)4, wrapper1.D48Deadline);
			AssertEquals("", true, wrapper1.IsD48AndNotClosed);

			var wrapper2 = new SupportingDocumentWrapper(headerSupportingDocument, entryLine2) as ISupportingDocumentOnly;
			AssertEquals("D48 amount should be 0 because entry line number is not 1 and parent is not an invoice line", 0m, wrapper2.D48Amount);
			AssertEquals("", (sbyte)4, wrapper2.D48Deadline);
			AssertEquals("", false, wrapper2.IsD48AndNotClosed);

			var wrapper3 = new SupportingDocumentWrapper(lineSupportingDocument, entryLine2) as ISupportingDocumentOnly;
			AssertEquals("D48 amount should match supporting document whatever the entry line number, because parent is an invoice line", 2000m, wrapper3.D48Amount);
			AssertEquals("", (sbyte)10, wrapper3.D48Deadline);
			AssertEquals("", true, wrapper3.IsD48AndNotClosed);
		}

		public void TestImputationSheetsGrouping()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2700", isImport: true, hasPermitAttribute: true));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203001010";

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "2700";
			supportingDocument1.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument1.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument1.CSI_Description = "ORANGES";
			supportingDocument1.CSI_AdditionalDescription = "ORANGE_1";
			supportingDocument1.CSI_LineNo = 1;

			var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "2700";
			supportingDocument2.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument2.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument2.CSI_Description = "ORANGES";
			supportingDocument1.CSI_AdditionalDescription = "ORANGE_2";
			supportingDocument2.CSI_LineNo = 2;

			var supportingDocument3 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "2700";
			supportingDocument3.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument3.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument3.CSI_Description = "ORANGES";
			supportingDocument1.CSI_AdditionalDescription = "ORANGE_3";
			supportingDocument3.CSI_LineNo = 3;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			var supDocToWrap = (SupportingDocument)entryLine.SupportingDocumentsToCustoms.ToList().First();

			var wrapper = new SupportingDocumentWrapper(supDocToWrap, entryLine) as ISupportingDocumentOnly;
			AssertEquals("Imputation sheets count is 3 because all 3 documents are basically the same with 3 distinct imputations.", 3, wrapper.ImputationsSheets.Count());
		}

		public void TestImputationSheets()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2700", isImport: true, hasPermitAttribute: true));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203001010";

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "2700";
			supportingDocument1.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument1.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument1.CSI_Description = "ORANGES";
			supportingDocument1.CSI_LineNo = 0;

			var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "2700";
			supportingDocument2.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument2.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument2.CSI_Description = "APPLES";
			supportingDocument2.CSI_LineNo = 1;

			var supportingDocument3 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "2700";
			supportingDocument3.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument3.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument3.CSI_Description = "PEARS";
			supportingDocument3.CSI_LineNo = 2;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			AssertEquals("Entry line merged supporting documents count is 3 because descriptions of documents differ.", 3, entryLine.SupportingDocuments.Count());
			AssertEquals("Entry line supporting documents to customs count is 1 because merge is done on code, reference and date, which are the same for all documents.", 1, entryLine.SupportingDocumentsToCustoms.Count());

			var supDocToWrap = (SupportingDocument)entryLine.SupportingDocumentsToCustoms.ToList().First();

			var wrapper = new SupportingDocumentWrapper(supDocToWrap, entryLine) as ISupportingDocumentOnly;
			AssertEquals("Imputation sheets count is only 2 because first document is not recognized as valid imputationsheet (CSI_LineNo is 0)", 2, wrapper.ImputationsSheets.Count());
		}

		public void TestIsD48()
		{
			var helper1 = new UniversalReferenceTestDataHelper(Factory);
			helper1.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			var helper2 = new UniversalReferenceTestDataHelper(Factory);
			helper2.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var sd = declaration.SupportingDocuments.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew() as CusEntryLine;
			var wrapper = new SupportingDocumentWrapper(sd, entryLine) as ISupportingDocumentOnly;

			sd.CSI_Code = new ZString("0001");
			AssertEquals("1.Is D48", true, sd.IsD48);
			sd.CSI_Code = new ZString("2044");
			AssertEquals("2.Is Not D48", false, sd.IsD48);
		}

		public void TestD48Deadline()
		{
			var helper1 = new UniversalReferenceTestDataHelper(Factory);
			helper1.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			var helper2 = new UniversalReferenceTestDataHelper(Factory);
			helper2.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew() as CusEntryLine;
			var document = declaration.SupportingDocuments.AddNew();
			document.CSI_Code = new ZString("0001");
			document.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			var wrapper = new SupportingDocumentWrapper(document, entryLine) as ISupportingDocumentOnly;

			document.CSI_Quantity3 = 0;
			AssertEquals((sbyte)0, wrapper.D48Deadline);
			document.CSI_Quantity3 = 1;
			AssertEquals((sbyte)1, wrapper.D48Deadline);
			document.CSI_Quantity3 = 0.1;
			AssertEquals((sbyte)0, wrapper.D48Deadline);
			document.CSI_Quantity3 = 1.6;
			AssertEquals((sbyte)1, wrapper.D48Deadline);
		}
	}
}
