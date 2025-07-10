using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExportEntryLineWrapper))]
	sealed class ExportEntryLineWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entryLine = new ExportEntryLine();
			var invoiceLines = new List<ExportInvoiceLine>();
			entryLine.InvoiceLines = invoiceLines.ToArray();
			return new ExportEntryLineWrapper(entryLine, ZDecimal.Zero, ZBool.False, Factory);
		}

		public void TestExportEntryLineFull()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();
			var wrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;

			var exportEntryLineItems1 = wrapper.EntryLineItems[0];
			var exportEntryLineItems1entryLine = exportEntryLineItems1.EntryLine;
			AssertEquals("8429521022", exportEntryLineItems1entryLine.HSCode);
			AssertEquals("USED EXCAVATOR", exportEntryLineItems1entryLine.HSDescription);
			AssertEquals("HYUNDAI ROBEX3000LC-7A", exportEntryLineItems1entryLine.TradeName);
			AssertEquals("상표명", exportEntryLineItems1entryLine.BrandName);
			AssertEquals("999999999", exportEntryLineItems1entryLine.InvoiceNo);
			AssertEquals("KR", exportEntryLineItems1entryLine.CountryOfOrigin);
			AssertEquals("A", exportEntryLineItems1entryLine.CountryOfOriginDeterminationRule);
			AssertEquals("N", exportEntryLineItems1entryLine.CountryOfOriginLabelLocation);
			AssertEquals("Y", exportEntryLineItems1entryLine.CertificateOfOriginIssued);
			AssertEquals("106", exportEntryLineItems1entryLine.FTAType);
			AssertEquals(360m, exportEntryLineItems1entryLine.NetWeightInKG);
			AssertEquals("CT", exportEntryLineItems1entryLine.QtyUnit);
			AssertEquals(1235m, exportEntryLineItems1entryLine.Qty);
			AssertEquals(3, exportEntryLineItems1entryLine.PackQty);
			AssertEquals("CT", exportEntryLineItems1entryLine.PackType);
			AssertEquals(25987293m, exportEntryLineItems1entryLine.CustomsValue);
			AssertEquals("A", exportEntryLineItems1entryLine.ImportDeclarationNumber);
			AssertEquals("001", exportEntryLineItems1entryLine.ImportEntryLineNo);
			AssertEquals("N", exportEntryLineItems1entryLine.SkipManifestReporting);
			AssertEquals("A", exportEntryLineItems1entryLine.PreApprovalType);
			AssertEquals("KR00101010101", exportEntryLineItems1entryLine.PreApprovalNo);
			AssertEquals(new ZDateTime(2018, 01, 01), exportEntryLineItems1entryLine.PreApprovalEffectiveFromDate);
			AssertEquals(new ZDateTime(2018, 09, 01), exportEntryLineItems1entryLine.PreApprovalEffectiveToDate);
			AssertEquals("8429.52-1022", exportEntryLineItems1.FormattedHSCode);
			AssertEquals(19979m, exportEntryLineItems1.CustomsValueUSD);
			AssertEquals("A(001)", exportEntryLineItems1.FormattedImportDeclarationNumber);
			AssertEquals("NO-A-대상\r\n식품등의 수입신고확인증", exportEntryLineItems1.GAApprovalDocumentContents1);
			AssertEquals("899999999-A-대상\r\n발급서류명2", exportEntryLineItems1.GAApprovalDocumentContents2);
			AssertEquals("", exportEntryLineItems1.GAApprovalDocumentContents3);
			AssertEquals("", exportEntryLineItems1.GAApprovalDocumentContents4);

			var exportEntryLineItems2 = wrapper.EntryLineItems[1];
			var exportEntryLineItems2entryLine = exportEntryLineItems2.EntryLine;
			AssertEquals("899999999", exportEntryLineItems2entryLine.HSCode);
			AssertEquals("품명2", exportEntryLineItems2entryLine.HSDescription);
			AssertEquals("거래품명2", exportEntryLineItems2entryLine.TradeName);
			AssertEquals("상표명2", exportEntryLineItems2entryLine.BrandName);
			AssertEquals("899999999", exportEntryLineItems2entryLine.InvoiceNo);
			AssertEquals("KR", exportEntryLineItems2entryLine.CountryOfOrigin);
			AssertEquals("A", exportEntryLineItems2entryLine.CountryOfOriginDeterminationRule);
			AssertEquals("N", exportEntryLineItems2entryLine.CountryOfOriginLabelLocation);
			AssertEquals("Y", exportEntryLineItems2entryLine.CertificateOfOriginIssued);
			AssertEquals("105", exportEntryLineItems2entryLine.FTAType);
			AssertEquals(1299999.99m, exportEntryLineItems2entryLine.NetWeightInKG);
			AssertEquals("CT", exportEntryLineItems2entryLine.QtyUnit);
			AssertEquals(1235m, exportEntryLineItems2entryLine.Qty);
			AssertEquals(5, exportEntryLineItems2entryLine.PackQty);
			AssertEquals("CT", exportEntryLineItems2entryLine.PackType);
			AssertEquals(8599999.99m, exportEntryLineItems2entryLine.CustomsValue);
			AssertEquals("A", exportEntryLineItems2entryLine.ImportDeclarationNumber);
			AssertEquals("001", exportEntryLineItems2entryLine.ImportEntryLineNo);
			AssertEquals("N", exportEntryLineItems2entryLine.SkipManifestReporting);
			AssertEquals("A", exportEntryLineItems2entryLine.PreApprovalType);
			AssertEquals("KR01231313", exportEntryLineItems2entryLine.PreApprovalNo);
			AssertEquals(new ZDateTime(2019, 01, 01), exportEntryLineItems2entryLine.PreApprovalEffectiveFromDate);
			AssertEquals(new ZDateTime(2019, 07, 01), exportEntryLineItems2entryLine.PreApprovalEffectiveToDate);
			AssertEquals("899999999", exportEntryLineItems2.FormattedHSCode);
			AssertEquals(6612m, exportEntryLineItems2.CustomsValueUSD);
			AssertEquals("A(001)", exportEntryLineItems2.FormattedImportDeclarationNumber);
			AssertEquals("", exportEntryLineItems2.GAApprovalDocumentContents1);
			AssertEquals("", exportEntryLineItems2.GAApprovalDocumentContents2);
			AssertEquals("", exportEntryLineItems2.GAApprovalDocumentContents3);
			AssertEquals("", exportEntryLineItems2.GAApprovalDocumentContents4);
		}

		public CusEntryHeader EntryLineVisibilitySetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			return entry;
		}

		public void TestExportEntryLineVisibility1Line1Spec()
		{
			var entry = EntryLineVisibilitySetUp();

			var wrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;
			var exportEntryLineItems = wrapper.EntryLineItems[0];

			AssertEquals("Y", exportEntryLineItems.IsFirstEntryLineHavingOneInvoiceLine);
		}

		public void TestExportEntryLineVisibility1Line2Spec()
		{
			var entry = EntryLineVisibilitySetUp();

			var invoiceLine2 = entry.Declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entry.MergedLines[0].PK;

			var wrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;
			var exportEntryLineItems = wrapper.EntryLineItems[0];

			AssertEquals("N", exportEntryLineItems.IsFirstEntryLineHavingOneInvoiceLine);
		}

		public void TestExportEntryLineVisibility1LineOver()
		{
			var entry = EntryLineVisibilitySetUp();

			var invoiceLine2 = entry.Declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entry.MergedLines[0].PK;

			var entryLine2 = entry.MergedLines.AddNew();
			var invoiceLine3 = entry.Declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;

			var wrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;
			var exportEntryLineItems = wrapper.EntryLineItems[1];

			AssertEquals(ZString.Empty, exportEntryLineItems.IsFirstEntryLineHavingOneInvoiceLine);
		}
	}
}
