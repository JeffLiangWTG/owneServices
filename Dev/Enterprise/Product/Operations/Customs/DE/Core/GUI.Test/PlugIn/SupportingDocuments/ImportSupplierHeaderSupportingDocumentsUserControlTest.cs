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
	class ImportSupplierHeaderSupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(ImportSupplierHeaderSupportingDocumentsUserControl), typeof(JobDeclaration));
		}

		public void TestSupportingDocumentsFieldsControlType()
		{
			SupportingDocumentsControlTestHelper.AssertSupportingDocumentsFieldsControlType(typeof(ImportSupplierHeaderSupportingDocumentsUserControl), typeof(ImportSupplierHeaderSupportingDocumentsFieldsControl));
		}

		public void TestGridColumns()
		{
			SupportingDocumentsControlTestHelper.AssertGridColumns(Factory, typeof(ImportSupplierHeaderSupportingDocumentsUserControl), OrderedColumnNamesAndColumnStyleTypes);
		}

		public void TestCaptionRenderingEnabled()
		{
			SupportingDocumentsControlTestHelper.AssertCaptionRenderingEnabledForUserControlAndFieldsControl(typeof(ImportSupplierHeaderSupportingDocumentsUserControl));
		}

		public void TestBottomPanelMinimumHeight()
		{
			using (var control = new ImportSupplierHeaderSupportingDocumentsUserControl())
			{
				control.Show();
				var panel = control.FindSingleOrDefault<ZPanel>("BottomPanel");
				AssertEquals(92, panel.MinimumSize.Height);
			}
		}

		public void TestGridColumnStyleProperties()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var collection = new SupportingDocumentCollection(supportingDocument);
			using (var control = new ImportSupplierHeaderSupportingDocumentsUserControl())
			{
				var grid = control.SupportingDocumentsGrid;
				grid.SetDataBinding(collection, "");
				control.Show();
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Code).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
				});
			}
		}

		IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnNamesAndColumnStyleTypes => new (string, Type)[]
		{
			(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
		};
	}
}
