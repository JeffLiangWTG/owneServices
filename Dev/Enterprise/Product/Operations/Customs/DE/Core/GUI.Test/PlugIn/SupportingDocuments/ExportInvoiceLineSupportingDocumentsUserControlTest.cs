using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class ExportInvoiceLineSupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(ExportInvoiceLineSupportingDocumentsUserControl), typeof(JobDeclaration));
		}

		public void TestSupportingDocumentsFieldsControlType()
		{
			SupportingDocumentsControlTestHelper.AssertSupportingDocumentsFieldsControlType(typeof(ExportInvoiceLineSupportingDocumentsUserControl), typeof(ExportInvoiceLineSupportingDocumentsFieldsControl));
		}

		public void TestGridColumns()
		{
			SupportingDocumentsControlTestHelper.AssertGridColumns(Factory, typeof(ExportInvoiceLineSupportingDocumentsUserControl), OrderedColumnNamesAndColumnStyleTypes);
		}

		public void TestCaptionRenderingEnabled()
		{
			SupportingDocumentsControlTestHelper.AssertCaptionRenderingEnabledForUserControlAndFieldsControl(typeof(ExportInvoiceLineSupportingDocumentsUserControl));
		}

		public void TestBottomPanelMinimumHeight()
		{
			using (var control = new ExportInvoiceLineSupportingDocumentsUserControl())
			{
				control.Show();
				var panel = control.FindSingleOrDefault<ZPanel>("BottomPanel");
				AssertEquals(200, panel.MinimumSize.Height);
			}
		}

		public void TestGridColumnStyleProperties()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var collection = new SupportingDocumentCollection(supportingDocument);
			using (var control = new ExportInvoiceLineSupportingDocumentsUserControl())
			{
				var grid = control.SupportingDocumentsGrid;
				grid.SetDataBinding(collection, "");
				control.Show();

				CombineAssertions(() =>
				{
					AssertEquals("CSI_FullType: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_FullType).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
					AssertEquals("CSI_Description: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Description).CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity).CharacterCasing);
					AssertEquals("CSI_Quantity: BindToDecimalPlaces", "QuantityDecimalPlaces", ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity)).BindToDecimalPlaces);
					AssertEquals("CSI_UnitOfQuantity2: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity2).CharacterCasing);
					AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_AdditionalDescription).CharacterCasing);
					AssertEquals("CSI_Value: Decimals", 2, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Value)).Decimals);
					var csi_ReferenceNumber2 = grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber2);
					AssertEquals("CSI_ReferenceNumber2: Caption", "License/Detail", csi_ReferenceNumber2.CaptionResourceString.Caption);
					AssertEquals("CSI_ReferenceNumber2: CharacterCasing", CharacterCasing.Normal, csi_ReferenceNumber2.CharacterCasing);
				});
			}
		}

		IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnNamesAndColumnStyleTypes => new (string, Type)[]
		{
			(SupportingDocument.Schema.CSI_FullType, typeof(ZCodeFindBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
			(SupportingDocument.Schema.CSI_Description, typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle)),
			(SupportingDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyle)),
			(SupportingDocument.Schema.CSI_UnitOfQuantity2, typeof(ZDropEditColumnStyle)),
			(SupportingDocument.Schema.CSI_Value, typeof(ZCalcEditColumnStyle)),
			(SupportingDocument.Schema.CSI_RX_NKCurrency, typeof(ZCodeFindBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyle)),
			(SupportingDocument.Schema.CSI_AdditionalDescription, typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyle))
		};
	}
}

