using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class ExportInvoiceLinePreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestControls_PrevDocsGroupBox()
		{
			using (var control = new ExportInvoiceLinePreviousDocumentsUserControl())
			{
				var prevDocsGroupBox = control.FindSingle<ZGroupBox>("PrevDocsGroupBox");
				CombineAssertions(() =>
				{
					AssertEquals("PrevDocsGroupBox", true, prevDocsGroupBox.Visible);
					AssertEquals("PrevDocsTypeDropEdit", false, control.FindSingle<ZDropEdit>("PrevDocsTypeDropEdit").Visible);
					AssertEquals("CodeFindBox", true, prevDocsGroupBox.FindSingle<ZCodeFindBox>("CodeFindBox").Visible);
					AssertEquals("PrevDocsReferenceTextBox", true, prevDocsGroupBox.FindSingle<ZTextBox>("PrevDocsReferenceTextBox").Visible);
					AssertEquals("ItemNumberCalcEdit", true, prevDocsGroupBox.FindSingle<ZCalcEdit>("ItemNumberCalcEdit").Visible);
					AssertEquals("UnitOfQuantityDropEdit", true, prevDocsGroupBox.FindSingle<ZDropEdit>("UnitOfQuantityDropEdit").Visible);
					AssertEquals("QuantityCalcEdit", true, prevDocsGroupBox.FindSingle<ZCalcEdit>("QuantityCalcEdit").Visible);
					AssertEquals("DescriptionTextBox", true, prevDocsGroupBox.FindSingle<ZTextBox>("DescriptionTextBox").Visible);
				});
			}
		}

		public void TestControls_PrevDocsGroupBox_CharacterCasing()
		{
			using (var control = new ExportInvoiceLinePreviousDocumentsUserControl())
			{
				var prevDocsGroupBox = control.FindSingle<ZGroupBox>("PrevDocsGroupBox");
				AssertEquals("PrevDocsReferenceTextBox", CharacterCasing.Normal, prevDocsGroupBox.FindSingle<ZTextBox>("PrevDocsReferenceTextBox").CharacterCasing);
				AssertEquals("DescriptionTextBox", CharacterCasing.Normal, prevDocsGroupBox.FindSingle<ZTextBox>("DescriptionTextBox").CharacterCasing);
			}
		}

		public void TestDocumentsGridColumns()
		{
			using (var control = new ExportInvoiceLinePreviousDocumentsUserControl())
			{
				var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				control.Show();
				CombineAssertions(() =>
				{
					var columnNames = previousDocumentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
					AssertSequencesEqual("Columns", new[] { PreviousDocument.Schema.CSI_Code, AutoCusSupportingInfo.Schema.CSI_ReferenceNumber, AutoCusSupportingInfo.Schema.CSI_ItemNumber, AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity, AutoCusSupportingInfo.Schema.CSI_Quantity, AutoCusSupportingInfo.Schema.CSI_Description }, columnNames);

					var codeColumnStyle = previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Code);
					AssertType<ZCodeFindBoxColumnStyleInfo>("CSI_Code Type", codeColumnStyle);
					AssertEquals("CSI_Code Width", 70, codeColumnStyle.Width);

					var referenceColumnStyle = previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_ReferenceNumber);
					AssertType<ZTextBoxColumnStyleInfo>("CSI_ReferenceNumber Type", referenceColumnStyle);
					AssertEquals("CSI_ReferenceNumber Width", 250, referenceColumnStyle.Width);

					var itemNumberColumnStyle = previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_ItemNumber);
					AssertType<ZTextBoxColumnStyleInfo>("CSI_ItemNumber Type", itemNumberColumnStyle);
					AssertEquals("CSI_ItemNumber Width", 50, itemNumberColumnStyle.Width);

					var unitColumnStyle = previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity);
					AssertType<ZDropEditColumnStyleInfo>("CSI_UnitOfQuantity Type", unitColumnStyle);
					AssertEquals("CSI_UnitOfQuantity Width", 50, unitColumnStyle.Width);

					var quantityColumnStyle = previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Quantity);
					AssertType<ZCalcEditColumnStyleInfo>("CSI_Quantity Type", quantityColumnStyle);
					AssertEquals("CSI_Quantity Width", 100, quantityColumnStyle.Width);

					var descriptionColumnStyle = previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Description);
					AssertType<ZTextBoxColumnStyleInfo>("CSI_Description Type", descriptionColumnStyle);
					AssertEquals("CSI_Description Width", 180, descriptionColumnStyle.Width);
				});
			}
		}

		public void TestDocumentsGridColumnsCharacterCasing()
		{
			using (var control = new ExportInvoiceLinePreviousDocumentsUserControl())
			{
				var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Code).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber", CharacterCasing.Normal, previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_ReferenceNumber).CharacterCasing);
					AssertEquals("CSI_Description", CharacterCasing.Normal, previousDocumentGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Description).CharacterCasing);
				});
			}
		}
	}
}
