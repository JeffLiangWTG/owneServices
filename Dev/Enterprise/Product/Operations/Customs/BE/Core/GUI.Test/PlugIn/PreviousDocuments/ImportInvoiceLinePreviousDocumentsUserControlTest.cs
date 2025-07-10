using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn.Testing;

sealed class ImportInvoiceLinePreviousDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestControls_PrevDocsGroupBox()
	{
		using (var control = new ImportInvoiceLinePreviousDocumentsUserControl())
		{
			var prevDocsGroupBox = control.FindSingle<ZGroupBox>("PrevDocsGroupBox");
			CombineAssertions(() =>
			{
				AssertEquals("PrevDocsGroupBox: Visible", true, prevDocsGroupBox.Visible);
				AssertEquals("PrevDocsTypeDropEdit", false, control.FindSingle<ZDropEdit>("PrevDocsTypeDropEdit").Visible);
				AssertEquals("FullTypeCodeFindBox: Visible", true, prevDocsGroupBox.FindSingle<ZCodeFindBox>("FullTypeCodeFindBox").Visible);
				AssertEquals("PrevDocsReferenceTextBox: Visible", true, prevDocsGroupBox.FindSingle<ZTextBox>("PrevDocsReferenceTextBox").Visible);
				AssertEquals("ItemNumberCalcEdit: Visible", true, prevDocsGroupBox.FindSingle<ZCalcEdit>("ItemNumberCalcEdit").Visible);
				AssertEquals("UnitOfQuantity2DropEdit: Visible", true, prevDocsGroupBox.FindSingle<ZDropEdit>("UnitOfQuantity2DropEdit").Visible);
				AssertEquals("Quantity2CalcEdit: Visible", true, prevDocsGroupBox.FindSingle<ZCalcEdit>("Quantity2CalcEdit").Visible);
				AssertEquals("UnitOfQuantityDropEdit: Visible", true, prevDocsGroupBox.FindSingle<ZDropEdit>("UnitOfQuantityDropEdit").Visible);
				AssertEquals("QuantityCalcEdit: Visible", true, prevDocsGroupBox.FindSingle<ZCalcEdit>("QuantityCalcEdit").Visible);
				AssertEquals("DescriptionTextBox: Visible", true, prevDocsGroupBox.FindSingle<ZTextBox>("DescriptionTextBox").Visible);

				AssertEquals("FullTypeCodeFindBox: TabIndex", 0, prevDocsGroupBox.FindSingle<ZCodeFindBox>("FullTypeCodeFindBox").TabIndex);
				AssertEquals("PrevDocsReferenceTextBox: TabIndex", 1, prevDocsGroupBox.FindSingle<ZTextBox>("PrevDocsReferenceTextBox").TabIndex);
				AssertEquals("ItemNumberCalcEdit: TabIndex", 2, prevDocsGroupBox.FindSingle<ZCalcEdit>("ItemNumberCalcEdit").TabIndex);
				AssertEquals("UnitOfQuantity2DropEdit: TabIndex", 3, prevDocsGroupBox.FindSingle<ZDropEdit>("UnitOfQuantity2DropEdit").TabIndex);
				AssertEquals("Quantity2CalcEdit: TabIndex", 4, prevDocsGroupBox.FindSingle<ZCalcEdit>("Quantity2CalcEdit").TabIndex);
				AssertEquals("UnitOfQuantityDropEdit: TabIndex", 5, prevDocsGroupBox.FindSingle<ZDropEdit>("UnitOfQuantityDropEdit").TabIndex);
				AssertEquals("QuantityCalcEdit: TabIndex", 6, prevDocsGroupBox.FindSingle<ZCalcEdit>("QuantityCalcEdit").TabIndex);
				AssertEquals("DescriptionTextBox: TabIndex", 7, prevDocsGroupBox.FindSingle<ZTextBox>("DescriptionTextBox").TabIndex);
			});
		}
	}

	public void TestDocumentsGridColumns()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var collection = new PreviousDocumentCollection(invoiceLine);

		using (var control = new ImportInvoiceLinePreviousDocumentsUserControl())
		{
			var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
			previousDocumentGrid.SetDataBinding(collection, string.Empty);
			control.Show();

			CombineAssertions(() =>
			{
				var columnNames = previousDocumentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
				AssertSequencesEqual("Columns", new[] { AutoCusSupportingInfo.Schema.CSI_Code, AutoCusSupportingInfo.Schema.CSI_ReferenceNumber, AutoCusSupportingInfo.Schema.CSI_ItemNumber, AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity2, AutoCusSupportingInfo.Schema.CSI_Quantity2, AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity, AutoCusSupportingInfo.Schema.CSI_Quantity, AutoCusSupportingInfo.Schema.CSI_Description }, columnNames);
				var codeColumnStyle = previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Code);
				AssertEquals("CSI_Code: Width", 70, codeColumnStyle.Width);
				var referenceNumberColumnStyle = previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_ReferenceNumber);
				AssertEquals("CSI_ReferenceNumber: Width", 250, referenceNumberColumnStyle.Width);
				AssertEquals("CSI_ItemNumber: Width", 50, previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_ItemNumber).Width);
				AssertEquals("CSI_UnitOfQuantity2: Width", 60, ((ZDropEditColumnStyleInfo)previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity2)).Width);
				AssertEquals("CSI_Quantity2: Width", 80, ((ZCalcEditColumnStyleInfo)previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Quantity2)).Width);
				AssertEquals("CSI_UnitOfQuantity: Width", 50, previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity).Width);
				AssertEquals("CSI_Quantity: Width", 100, previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Quantity).Width);
				var descriptionColumnStyle = previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Description);
				AssertEquals("CSI_Description: Width", 180, descriptionColumnStyle.Width);

				AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Normal, codeColumnStyle.CharacterCasing);
				AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, referenceNumberColumnStyle.CharacterCasing);
				AssertEquals("CSI_Description: CharacterCasing", CharacterCasing.Normal, descriptionColumnStyle.CharacterCasing);
			});
		}
	}
}
