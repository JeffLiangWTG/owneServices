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
	class ExportSupplierHeaderSupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(ExportSupplierHeaderSupportingDocumentsUserControl), typeof(JobDeclaration));
		}

		public void TestSupportingDocumentsFieldsControlType()
		{
			SupportingDocumentsControlTestHelper.AssertSupportingDocumentsFieldsControlType(typeof(ExportSupplierHeaderSupportingDocumentsUserControl), typeof(ExportSupplierHeaderSupportingDocumentsFieldsControl));
		}

		public void TestGridColumns()
		{
			SupportingDocumentsControlTestHelper.AssertGridColumns(Factory, typeof(ExportSupplierHeaderSupportingDocumentsUserControl), OrderedColumnNamesAndColumnStyleTypes);
		}

		public void TestCaptionRenderingEnabled()
		{
			SupportingDocumentsControlTestHelper.AssertCaptionRenderingEnabledForUserControlAndFieldsControl(typeof(ExportSupplierHeaderSupportingDocumentsUserControl));
		}

		public void TestBottomPanelMinimumHeight()
		{
			using (var control = new ExportSupplierHeaderSupportingDocumentsUserControl())
			{
				control.Show();
				var panel = control.FindSingleOrDefault<ZPanel>("BottomPanel");
				AssertEquals(132, panel.MinimumSize.Height);
			}
		}

		public void TestGridColumnStyleProperties()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var collection = new SupportingDocumentCollection(supportingDocument);
			using (var control = new ExportSupplierHeaderSupportingDocumentsUserControl())
			{
				var grid = control.SupportingDocumentsGrid;
				grid.SetDataBinding(collection, "");
				control.Show();

				CombineAssertions(() =>
				{
					AssertEquals("CSI_FullType: CharacterCasing", CharacterCasing.Normal, ((ZCodeFindBoxColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_FullType)).CharacterCasing);
					var csi_ReferenceNumber = (ZMultiControlColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber);
					AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, csi_ReferenceNumber.CharacterCasing);
					AssertEquals("CSI_ReferenceNumber: MaxLength", 35, csi_ReferenceNumber.MaxLengthOverride);
					var csi_AdditionalDescription = (ZTextBoxColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_AdditionalDescription);
					AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, csi_AdditionalDescription.CharacterCasing);
					AssertEquals("CSI_AdditionalDescription: MaxLength", 70, csi_AdditionalDescription.MaxLengthOverride);
					AssertEquals("CSI_AdditionalDescription: CaptionResourceString", "Issuing Authority", csi_AdditionalDescription.CaptionResourceString.Caption);
					var csi_ItemNumber = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_ItemNumber);
					AssertEquals("CSI_ItemNumber: MaxLength", 5, csi_ItemNumber.MaxLengthOverride);
					AssertEquals("CSI_ItemNumber: Decimals", 0, csi_ItemNumber.Decimals);
					AssertEquals("CSI_ItemNumber: CaptionResourceString", "Document Line Item Number", csi_ItemNumber.CaptionResourceString.Caption);
				});
			}
		}

		IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnNamesAndColumnStyleTypes => new (string, Type)[]
		{
			(SupportingDocument.Schema.CSI_FullType, typeof(ZCodeFindBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyle)),
			(SupportingDocument.Schema.CSI_AdditionalDescription, typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyle))
		};
	}
}

