using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.PlugIn.Testing
{
	sealed class InvoiceLinePreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var supportingDocument = declaration.PreviousDocuments.AddNew();
			var collection = new PreviousDocumentCollection(supportingDocument);

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControlForTesting())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				control.LineDetailTabControl.SelectedTab = control.PreviousDocumentsTabPage_Exposed;
				var grid = control.Controls.Find("PreviousDocumentsGrid", true).FirstOrDefault() as ZGrid;

				CombineAssertions(() =>
				{
					var visibleColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsVisible && !x.IsUnavailable).Select(x => x.ColumnName).ToArray();
					AssertEquals("visible columns count", 8, visibleColumns.Length);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), "Sequence No", true, 0, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), null, true, 1, visibleColumns); // caption defined in property definition
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyleInfo), null, true, 2, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_PackType, typeof(ZDropEditColumnStyleInfo), "Type Of Package", true, 3, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_PackQty, typeof(ZCalcEditColumnStyleInfo), "Number of Packages", true, 4, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), "Quantity", true, 5, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyleInfo), "Measurement unit and qualifier", true, 6, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_ItemNumber, typeof(ZTextBoxColumnStyleInfo), "Goods Item Identifier", true, 7, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_SubType, typeof(ZDropEditColumnStyleInfo), null, false, 0, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo), null, false, 0, visibleColumns);

					declaration.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaG;
					visibleColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsVisible && !x.IsUnavailable).Select(x => x.ColumnName).ToArray();
					AssertEquals("visible columns count", 5, visibleColumns.Length);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), "Sequence No", true, 0, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), null, true, 1, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_SubType, typeof(ZDropEditColumnStyleInfo), null, true, 2, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyleInfo), null, true, 3, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo), null, true, 4, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_PackType, typeof(ZDropEditColumnStyleInfo), "Type Of Package", false, 0, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_PackQty, typeof(ZCalcEditColumnStyleInfo), "Number of Packages", false, 0, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyleInfo), "Measurement unit and qualifier", false, 0, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), "Quantity", false, 0, visibleColumns);
					AssertColumnDetails(grid, PreviousDocument.Schema.CSI_ItemNumber, typeof(ZTextBoxColumnStyleInfo), "Goods Item Identifier", false, 0, visibleColumns);
				});
			}
		}

		static void AssertColumnDetails(ZGrid grid, string name, Type type, string caption, bool visible, int index, string[] visibleColumns)
		{
			var columnStyle = grid.GetColumnStyle(name);
			AssertEquals(name + " Type", type, columnStyle.GetType());
			AssertEquals(name + " Caption", caption, string.IsNullOrEmpty(columnStyle.Caption) ? columnStyle.CaptionResourceString.Caption : columnStyle.Caption);
			AssertEquals(name + " IsVisible", visible, columnStyle.IsVisible);
			AssertEquals(name + " IsUnavailable", !visible, columnStyle.IsUnavailable);
			AssertEquals(name + " index", index, columnStyle.IsVisible ? Array.IndexOf(visibleColumns, name) : 0);
		}

		class ImportInvoiceLineUserControlForTesting : ImportInvoiceLineUserControl
		{
			internal ZTabPage PreviousDocumentsTabPage_Exposed => PreviousDocumentsTabPage;
		}
	}
}
