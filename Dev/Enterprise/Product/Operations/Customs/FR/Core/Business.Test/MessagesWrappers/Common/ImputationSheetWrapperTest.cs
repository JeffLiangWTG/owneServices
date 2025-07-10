using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Declaration.Testing;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	sealed class ImputationSheetWrapperTest : TestCaseWithFactory
	{
		public void TestWrapperUnitOfQuantity_ShowMappedUnitIfMappingExist()
		{
			SupportingDocumentTest.SetUpRefDataForSupportDocumentUnitOfQuantityTest(Factory);

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "CODE1";
			supportingDocument1.CSI_UnitOfQuantity = "KGM";
			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var suppDoc = (SupportingDocument)entryLine.SupportingDocuments.First();
			var wrapper = new ImputationSheetWrapper(suppDoc, entryLine);
			AssertEquals("Show mapped unit if the map of current unit has been configured for current CSI_Code", "kilo", wrapper.ImputationUnit);
		}

		public void TestWrapperUnitOfQuantity_ShowOriginalUnitIfNoMapping()
		{
			SupportingDocumentTest.SetUpRefDataForSupportDocumentUnitOfQuantityTest(Factory);

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "CODE1";
			supportingDocument1.CSI_UnitOfQuantity = "MTR";
			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var suppDoc = (SupportingDocument)entryLine.SupportingDocuments.First();
			var wrapper = new ImputationSheetWrapper(suppDoc, entryLine);
			AssertEquals("Show orginal unit code if the current unit is not configured in mapping", "MTR", wrapper.ImputationUnit);
		}

		public void TestWrapperUnitOfQuantity_ShowOriginalUnitIfCodeNoUQMapTypeAttribute()
		{
			SupportingDocumentTest.SetUpRefDataForSupportDocumentUnitOfQuantityTest(Factory);

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "CODE3";
			supportingDocument1.CSI_UnitOfQuantity = "KGM";
			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var suppDoc = (SupportingDocument)entryLine.SupportingDocuments.First();
			var wrapper = new ImputationSheetWrapper(suppDoc, entryLine);
			AssertEquals("Show orginal unit code if the current CSI_Code has no mapping configuration", "KGM", wrapper.ImputationUnit);
		}

		public void TestWrapperWriteOutPropertiesWhenValuesAreZeroised()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 0m;

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_LineNo = 1;
			supportingDocument1.CSI_Description = "Oranges";
			supportingDocument1.CSI_AdditionalDescription = "Blablabla";
			supportingDocument1.CSI_UnitOfQuantity = "KGM";
			supportingDocument1.CSI_Quantity = 0m;
			supportingDocument1.CSI_Quantity2 = 0m;
			supportingDocument1.CSI_UnitOfQuantity2 = "DTN";
			supportingDocument1.CSI_RX_NKCurrency = "USD";
			supportingDocument1.CSI_Value = 0m;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var suppDoc = (SupportingDocument)entryLine.SupportingDocuments.First();
			var wrapper = new ImputationSheetWrapper(suppDoc, entryLine);

			AssertEquals(false, wrapper.ShouldWriteImputationAmount);
			AssertEquals(false, wrapper.ShouldWriteImputationQuantity);
			AssertEquals(false, wrapper.ShouldWriteProvisionalWeight);
			AssertEquals(false, wrapper.ShouldWriteNetWeight);
		}

		public void TestWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 123.0m;

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_LineNo = 1;
			supportingDocument1.CSI_Description = "Oranges";
			supportingDocument1.CSI_AdditionalDescription = "Blablabla";
			supportingDocument1.CSI_UnitOfQuantity = "KGM";
			supportingDocument1.CSI_Quantity = 10m;
			supportingDocument1.CSI_Quantity2 = 33m;
			supportingDocument1.CSI_UnitOfQuantity2 = "DTN";
			supportingDocument1.CSI_RX_NKCurrency = "USD";
			supportingDocument1.CSI_Value = 1000m;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var suppDoc = (SupportingDocument)entryLine.SupportingDocuments.First();
			var wrapper = new ImputationSheetWrapper(suppDoc, entryLine);

			AssertEquals("1", wrapper.LineNumber);
			AssertEquals("Product reference", "Blablabla", wrapper.ProductReference);
			AssertEquals("Product name", "Oranges", wrapper.ProductName);
			AssertEquals("Net Weight", 123.0m, wrapper.NetWeight);
			AssertEquals("Imputation Unit", "KGM", wrapper.ImputationUnit);
			AssertEquals("Imputation Quantity", 10m, wrapper.ImputationQuantity);
			AssertEquals("Provisional Weight", 33m, wrapper.ProvisionalWeight.Weight);
			AssertEquals("Provisional Weight Unit", "DTN", wrapper.ProvisionalWeight.Unit);
			AssertEquals("Amount", "USD", wrapper.ImputationCurrency);
			AssertEquals("Amount", 1000m, wrapper.ImputationAmount);
			AssertEquals(true, wrapper.ShouldWriteImputationAmount);
			AssertEquals(true, wrapper.ShouldWriteImputationQuantity);
			AssertEquals(true, wrapper.ShouldWriteProvisionalWeight);
			AssertEquals(true, wrapper.ShouldWriteNetWeight);
		}

		public void TestWrapperDecimalPlaces()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Quantity = 12.3456m;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var suppDoc = (SupportingDocument)entryLine.SupportingDocuments.First();
			var wrapper = new ImputationSheetWrapper(suppDoc, entryLine);

			AssertEquals("Imputation Quantity", 12.3456m, wrapper.ImputationQuantity);
		}
	}
}
