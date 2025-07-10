using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn.Testing;

sealed class InvoiceLinePreviousDocumentsUserControlUCC6Test : TestCaseWithFactory
{
	public void TestControls_PrevDocsGroupBox()
	{
		using (var control = new InvoiceLinePreviousDocumentsUserControlUCC6())
		{
			var prevDocsGroupBox = control.FindSingle<ZGroupBox>("PrevDocsGroupBox");
			CombineAssertions(() =>
			{
				AssertEquals("PrevDocsGroupBox: Visible", true, prevDocsGroupBox.Visible);
				AssertEquals("PrevDocsTypeDropEdit", false, control.FindSingle<ZDropEdit>("PrevDocsTypeDropEdit").Visible);
				AssertEquals("FullTypeCodeFindBox: Visible", true, prevDocsGroupBox.FindSingle<ZCodeFindBox>("FullTypeCodeFindBox").Visible);
				AssertEquals("PrevDocsReferenceTextBox: Visible", true, prevDocsGroupBox.FindSingle<ZTextBox>("PrevDocsReferenceTextBox").Visible);
				AssertEquals("ItemNumberCalcEdit: Visible", true, prevDocsGroupBox.FindSingle<ZCalcEdit>("ItemNumberCalcEdit").Visible);
				AssertEquals("UnitOfQuantityDropEdit: Visible", true, prevDocsGroupBox.FindSingle<ZDropEdit>("UnitOfQuantityDropEdit").Visible);
				AssertEquals("QuantityCalcEdit: Visible", true, prevDocsGroupBox.FindSingle<ZCalcEdit>("QuantityCalcEdit").Visible);
				AssertEquals("PackQuantityCalcEdit: Visible", true, prevDocsGroupBox.FindSingle<ZCalcEdit>("PackQuantityCalcEdit").Visible);
				AssertEquals("TypeOfPackagesDropEdit: Visible", true, prevDocsGroupBox.FindSingle<ZDropEdit>("TypeOfPackagesDropEdit").Visible);
				AssertEquals("DescriptionTextBox: Visible", false, prevDocsGroupBox.FindSingle<ZTextBox>("DescriptionTextBox").Visible);

				AssertEquals("FullTypeCodeFindBox: TabIndex", 0, prevDocsGroupBox.FindSingle<ZCodeFindBox>("FullTypeCodeFindBox").TabIndex);
				AssertEquals("PrevDocsReferenceTextBox: TabIndex", 1, prevDocsGroupBox.FindSingle<ZTextBox>("PrevDocsReferenceTextBox").TabIndex);
				AssertEquals("ItemNumberCalcEdit: TabIndex", 2, prevDocsGroupBox.FindSingle<ZCalcEdit>("ItemNumberCalcEdit").TabIndex);
				AssertEquals("UnitOfQuantityDropEdit: TabIndex", 7, prevDocsGroupBox.FindSingle<ZDropEdit>("UnitOfQuantityDropEdit").TabIndex);
				AssertEquals("QuantityCalcEdit: TabIndex", 6, prevDocsGroupBox.FindSingle<ZCalcEdit>("QuantityCalcEdit").TabIndex);
				AssertEquals("PackQuantityCalcEdit: TabIndex", 3, prevDocsGroupBox.FindSingle<ZCalcEdit>("PackQuantityCalcEdit").TabIndex);
				AssertEquals("TypeOfPackagesDropEdit: TabIndex", 4, prevDocsGroupBox.FindSingle<ZDropEdit>("TypeOfPackagesDropEdit").TabIndex);

				AssertEquals("PrevDocsGroupBox: Caption", "[UCC 2/1] Previous documents", prevDocsGroupBox.CaptionResourceString.Caption);
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

		using (var control = new InvoiceLinePreviousDocumentsUserControlUCC6())
		{
			var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
			previousDocumentGrid.SetDataBinding(collection, string.Empty);
			control.Show();

			CombineAssertions(() =>
			{
				var columnNames = previousDocumentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
				AssertSequencesEqual("Columns", new[] { AutoCusSupportingInfo.Schema.CSI_Code, AutoCusSupportingInfo.Schema.CSI_ReferenceNumber, AutoCusSupportingInfo.Schema.CSI_ItemNumber, AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity, AutoCusSupportingInfo.Schema.CSI_Quantity, AutoCusSupportingInfo.Schema.CSI_PackQty, AutoCusSupportingInfo.Schema.CSI_PackType }, columnNames);
				var codeColumnStyle = ((ZCodeFindBoxColumnStyleInfo)previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Code));
				AssertEquals("CSI_Code: Width", 70, codeColumnStyle.Width);
				var referenceNumberColumnStyle = ((ZTextBoxColumnStyleInfo)previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_ReferenceNumber));
				AssertEquals("CSI_ReferenceNumber: Width", 250, referenceNumberColumnStyle.Width);
				AssertEquals("CSI_ItemNumber: Width", 50, ((ZTextBoxColumnStyleInfo)previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_ItemNumber)).Width);
				AssertEquals("CSI_UnitOfQuantity: Width", 50, ((ZDropEditColumnStyleInfo)previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity)).Width);
				AssertEquals("CSI_Quantity: Width", 100, ((ZCalcEditColumnStyleInfo)previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Quantity)).Width);

				AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Normal, codeColumnStyle.CharacterCasing);
				AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, referenceNumberColumnStyle.CharacterCasing);

				AssertType<ZCalcEditColumnStyleInfo>(previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_PackQty));
				AssertType<ZDropEditColumnStyleInfo>(previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_PackType));
			});
		}
	}
}
