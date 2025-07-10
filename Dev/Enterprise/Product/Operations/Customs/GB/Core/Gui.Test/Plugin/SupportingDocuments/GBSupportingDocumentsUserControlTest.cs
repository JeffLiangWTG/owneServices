using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Plugin.Testing
{
	class GBSupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(GBSupportingDocumentsUserControl), typeof(JobDeclaration));
		}

		public void TestSupportingDocumentsFieldsControlType()
		{
			SupportingDocumentsControlTestHelper.AssertSupportingDocumentsFieldsControlType(typeof(GBSupportingDocumentsUserControl), typeof(GBSupportingDocumentsFieldsControl));
		}

		public void TestGridColumns()
		{
			SupportingDocumentsControlTestHelper.AssertGridColumns(Factory, typeof(GBSupportingDocumentsUserControl), orderedColumnNamesAndColumnStyleTypes);
		}

		public void TestCaptionRenderingEnabled()
		{
			SupportingDocumentsControlTestHelper.AssertCaptionRenderingEnabledForUserControlAndFieldsControl(typeof(GBSupportingDocumentsUserControl));
		}

		public void TestBottomPanelMinimumHeight()
		{
			using (var control = new GBSupportingDocumentsUserControl())
			{
				control.Show();
				var panel = control.FindSingleOrDefault<ZPanel>("BottomPanel");
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(230), panel.MinimumSize.Height);
			}
		}

		public void TestGridColumnStyleProperties()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var collection = new SupportingDocumentCollection(supportingDocument);
			using (var control = new GBSupportingDocumentsUserControl())
			{
				var grid = control.SupportingDocumentsGrid;
				grid.SetDataBinding(collection, "");
				control.Show();
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Code).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity).CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity2: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity2).CharacterCasing);
					AssertEquals("CSI_RX_NKCurrency: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_RX_NKCurrency).CharacterCasing);
					AssertEquals("CSI_Description: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Description).CharacterCasing);
					AssertEquals("CSI_Availability: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Availability).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber2: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber2).CharacterCasing);
					AssertEquals("CSI_Quantity2: Decimals", 5, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity2)).Decimals);
					AssertEquals("CSI_Actions: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Actions).CharacterCasing);
					AssertEquals("CSI_Quantity: BindToDecimalPlaces", "QuantityDecimalPlaces", ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity)).BindToDecimalPlaces);
					AssertEquals("CSI_Value: Decimals", 5, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Value)).Decimals);
					var csi_SubTypeColumn = grid.GetColumnStyle(SupportingDocument.Schema.CSI_SubType);
					AssertEquals("CSI_SubType: CharacterCasing", CharacterCasing.Upper, csi_SubTypeColumn.CharacterCasing);
					Assert("CSI_SubType: Mandatory", csi_SubTypeColumn.IsMandatory);
				});
			}
		}

		IEnumerable<(string ColumnName, Type ColumnType)> orderedColumnNamesAndColumnStyleTypes => new (string, Type)[]
		{
			(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
			(SupportingDocument.Schema.CSI_Actions, typeof(ZDropEditColumnStyle)),
			(SupportingDocument.Schema.CSI_Availability, typeof(ZDropEditColumnStyle)),
			(SupportingDocument.Schema.CSI_SubType, typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle)),
			(SupportingDocument.Schema.CSI_UnitOfQuantity, typeof(ZMultiControlColumnStyle)),
			(SupportingDocument.Schema.CSI_Quantity2, typeof(ZCalcEditColumnStyle)),
			(SupportingDocument.Schema.CSI_UnitOfQuantity2, typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_Value, typeof(ZCalcEditColumnStyle)),
			(SupportingDocument.Schema.CSI_RX_NKCurrency, typeof(ZCodeFindBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyle)),
			(SupportingDocument.Schema.CSI_Description, typeof(ZDropEditColumnStyle)),
			(SupportingDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyle)),
		};
	}
}
