using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	public class SupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(SupportingDocumentsUserControlType, typeof(JobDeclaration));
		}

		public void TestSupportingDocumentsFieldsControlType()
		{
			SupportingDocumentsControlTestHelper.AssertSupportingDocumentsFieldsControlType(SupportingDocumentsUserControlType, SupportingDocumentsFieldsControlType);
		}

		public void TestSupportingDocumentsFieldsControlDockStyle()
		{
			using (SupportingDocumentsUserControl control = (SupportingDocumentsUserControl)Activator.CreateInstance(SupportingDocumentsUserControlType))
			{
				AssertEquals(DockStyle.Fill, control.SupportingDocumentsFieldsControl.Dock);
			}
		}

		public void TestCaptionRenderingEnabled()
		{
			SupportingDocumentsControlTestHelper.AssertCaptionRenderingEnabledForUserControlAndFieldsControl(SupportingDocumentsUserControlType);
		}

		public void TestBottomPanelContainsSupportingDocumentsFieldsControl()
		{
			using (SupportingDocumentsUserControl control = (SupportingDocumentsUserControl)Activator.CreateInstance(SupportingDocumentsUserControlType))
			{
				control.Show();
				var panel = control.FindSingleOrDefault<ZPanel>("BottomPanel");
				AssertNotNull(panel.FindSingleOrDefault<SupportingDocumentsFieldsControl>("SupportingDocumentsFieldsControl"));
			}
		}

		public void TestBottomPanelMinimumHeight()
		{
			using (SupportingDocumentsUserControl control = (SupportingDocumentsUserControl)Activator.CreateInstance(SupportingDocumentsUserControlType))
			{
				control.Show();
				var panel = control.FindSingleOrDefault<ZPanel>("BottomPanel");
				AssertEquals(181, panel.MinimumSize.Height);
			}
		}

		public void TestGridColumns()
		{
			SupportingDocumentsControlTestHelper.AssertGridColumns(Factory, SupportingDocumentsUserControlType, OrderedColumnNamesAndColumnStyleTypes);
		}

		public void TestGridColumnStyleProperties()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var collection = new SupportingDocumentCollection(supportingDocument);
			using (SupportingDocumentsUserControl control = (SupportingDocumentsUserControl)Activator.CreateInstance(SupportingDocumentsUserControlType))
			{
				var grid = control.SupportingDocumentsGrid;
				grid.SetDataBinding(collection, "");
				control.Show();

				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Code).CharacterCasing);
					AssertEquals("CSI_DateOfIssue: Format", ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfIssue)).DateTimeFormat);
					AssertEquals("CSI_DateOfExpiry: Format", ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfExpiry)).DateTimeFormat);
					AssertEquals("CSI_Status: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Status).CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity2: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity2).CharacterCasing);
					AssertEquals("CSI_RX_NKCurrency: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_RX_NKCurrency).CharacterCasing);
					AssertEquals("CSI_Quantity2: Decimals", 5, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity2)).Decimals);
					AssertEquals("CSI_Value: Decimals", 5, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Value)).Decimals);
					var csi_Quantity = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity);
					AssertEquals("CSI_Quantity: Decimals", 5, csi_Quantity.Decimals);
					AssertEquals("CSI_Quantity: BindToDecimalPlaces", "QuantityDecimalPlaces", csi_Quantity.BindToDecimalPlaces);
					var csi_ReferenceNumber = (ZMultiControlColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber);
					AssertEquals("CSI_ReferenceNumber: FieldTypeColumnName", "ReferenceNumberFieldType", csi_ReferenceNumber.FieldTypeColumnName);
					AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, csi_ReferenceNumber.CharacterCasing);
					var csi_UnoitOfQuantity = grid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity);
					if (csi_UnoitOfQuantity is ZMultiControlColumnStyleInfo multiControlColumnStyleInfo)
					{
						AssertEquals("CSI_UnitOfQuantity: FieldTypeColumnName", "UnitOfQuantityFieldType", multiControlColumnStyleInfo.FieldTypeColumnName);
					}
					AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Normal, csi_UnoitOfQuantity.CharacterCasing);
				});
			}
		}

		protected virtual Type SupportingDocumentsUserControlType => typeof(SupportingDocumentsUserControl);

		protected virtual Type SupportingDocumentsFieldsControlType => typeof(SupportingDocumentsFieldsControl);

		protected virtual IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnNamesAndColumnStyleTypes => new (string, Type)[]
		{
			(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
			(CSI_CodeDescription, typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
			(SupportingDocument.Schema.CSI_Status, typeof(ZDropEditColumnStyle)),
			(SupportingDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle)),
			(SupportingDocument.Schema.CSI_UnitOfQuantity, typeof(ZMultiControlColumnStyle)),
			(SupportingDocument.Schema.CSI_Quantity2, typeof(ZCalcEditColumnStyle)),
			(SupportingDocument.Schema.CSI_UnitOfQuantity2, typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_Value, typeof(ZCalcEditColumnStyle)),
			(SupportingDocument.Schema.CSI_RX_NKCurrency, typeof(ZCodeFindBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyle))
		};

		const string CSI_CodeDescription = "CSI_CodeDescription";
	}
}
