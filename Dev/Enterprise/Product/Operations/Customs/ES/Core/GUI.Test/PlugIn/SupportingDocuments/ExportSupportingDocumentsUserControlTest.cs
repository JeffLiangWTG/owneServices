using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class ExportSupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(ExportSupportingDocumentsUserControl), typeof(JobDeclaration));
		}

		public void TestGridColumns()
		{
			SupportingDocumentsControlTestHelper.AssertGridColumns(Factory, typeof(ExportSupportingDocumentsUserControl), OrderedColumnNamesAndColumnStyleTypes);
		}

		public void TestCaptionRenderingEnabled()
		{
			SupportingDocumentsControlTestHelper.AssertCaptionRenderingEnabledForUserControlAndFieldsControl(typeof(ExportSupportingDocumentsUserControl));
		}

		public void TestSupportingDocumentsFieldsControlDockStyle()
		{
			using (var control = new ImportSupportingDocumentsUserControl())
			{
				AssertEquals(DockStyle.Fill, control.SupportingDocumentsFieldsControl.Dock);
			}
		}

		public void TestGridColumnStyleProperties()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var collection = new SupportingDocumentCollection(supportingDocument);
			using (var control = new ExportSupportingDocumentsUserControl())
			{
				var grid = control.SupportingDocumentsGrid;
				grid.SetDataBinding(collection, "");
				control.Show();

				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Code).CharacterCasing);
					var csi_ReferenceNumber = (ZMultiControlColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber);
					AssertEquals("CSI_ReferenceNumber: FieldTypeColumnName", "ReferenceNumberFieldType", csi_ReferenceNumber.FieldTypeColumnName);
					AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, csi_ReferenceNumber.CharacterCasing);
					AssertEquals("CSI_Status: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Status).CharacterCasing);
					var csi_Quantity = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity);
					AssertEquals("CSI_Quantity: Decimals", 5, csi_Quantity.Decimals);
					AssertEquals("CSI_Quantity: BindToDecimalPlaces", "QuantityDecimalPlaces", csi_Quantity.BindToDecimalPlaces);
					var csi_UnoitOfQuantity = (ZDropEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity);
					AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Upper, csi_UnoitOfQuantity.CharacterCasing);
					AssertEquals("CSI_Quantity2: Decimals", 5, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity2)).Decimals);
					AssertEquals("CSI_UnitOfQuantity2: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity2).CharacterCasing);
					AssertEquals("CSI_Value: Decimals", 5, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Value)).Decimals);
					AssertEquals("CSI_RX_NKCurrency: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_RX_NKCurrency).CharacterCasing);
					AssertEquals("CSI_DateOfIssue: Format", ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfIssue)).DateTimeFormat);
					AssertEquals("CSI_DateOfExpiry: Format", ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfExpiry)).DateTimeFormat);
					AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_AdditionalDescription).CharacterCasing);
					AssertEquals("CSI_ItemNumber: Decimals", 2, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_ItemNumber)).Decimals);
				});
			}
		}

		IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnNamesAndColumnStyleTypes => new (string, Type)[]
		{
			(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
			("CSI_CodeDescription", typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
			(SupportingDocument.Schema.CSI_Status, typeof(ZDropEditColumnStyle)),
			(SupportingDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle)),
			(SupportingDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyle)),
			(SupportingDocument.Schema.CSI_Quantity2, typeof(ZCalcEditColumnStyle)),
			(SupportingDocument.Schema.CSI_UnitOfQuantity2, typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_Value, typeof(ZCalcEditColumnStyle)),
			(SupportingDocument.Schema.CSI_RX_NKCurrency, typeof(ZCodeFindBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyle)),
			(SupportingDocument.Schema.CSI_AdditionalDescription, typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyle))
		};
	}
}
