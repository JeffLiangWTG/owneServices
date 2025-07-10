using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSection_QRCodeAndAuthorisationDataTest : TestCaseWithFactory
	{
		public void TestQRCodeAndAuthorisationDataSection_InSystemDocumentElements()
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);

			var sectionName = "QR Code and Authorisation Data";
			var section = systemTemplate.TemplateSections.Find(sectionName);
			AssertNotNull($"{sectionName} section should exist in System Document Elements Template", section);

			var excelWorkSheet = ConfigurableTemplateTestHelper.GetExcelWorkSheetFromSection(systemTemplate, sectionName);
			AssertEquals("Section Row Count", 7, excelWorkSheet.RowCount);
			AssertMultilineASCIIEquals("My Section should be copied over.",
@"{A}-[#ConfigurableSection:GEN:Invoice, QR Code and Authorisation Data]
{O}-[<ARInvoice.InvoiceAuthorisationRecordIssuerAuthorizationDataLabel>]   {BU}-[<HideRowIf(""<ARInvoice.QRCodeData>"" == """")>]
{C}-[<QRCode(""<ARInvoice.QRCodeData>"",12,12)>]   {O}-[<ShrinkToFit><ARInvoice.InvoiceAuthorisationRecordIssuerAuthorizationData> ]   {BU}-[<HideRowIf(""<ARInvoice.QRCodeData>"" == """")>]
{O}-[<ARInvoice.InvoiceAuthorisationRecordAuthorisationDataLabel> ]   {BU}-[<HideRowIf(""<ARInvoice.QRCodeData>"" == """")>]
{O}-[<ShrinkToFit><ARInvoice.InvoiceAuthorisationRecordAuthorisationData>]   {BU}-[<HideRowIf(""<ARInvoice.QRCodeData>"" == """")>]
{O}-[<ARInvoice.InvoiceAuthorizationRecordVerificationURLLabel>]   {BU}-[<HideRowIf(""<ARInvoice.QRCodeData>"" == """")>]
{O}-[<ShrinkToFit><ARInvoice.InvoiceAuthorisationRecordVerificationURL>]   {BU}-[<HideRowIf(""<ARInvoice.QRCodeData>"" == """")>]", excelWorkSheet.ToString());

			float[] expectedRowsHeight = new float[] { 15, 15, 33, 15, 33, 15, 36 };
			for (var row = 0; row < excelWorkSheet.RowCount; row++)
			{
				AssertEquals($"Row {row} Height", expectedRowsHeight[row], ConfigurableTemplateTestHelper.GetRowHeightInPoints(excelWorkSheet, row));
			}
		}
	}
}
