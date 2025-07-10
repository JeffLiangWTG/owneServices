using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn.Testing;

class InvoiceHeaderSupportingDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceType()
	{
		SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(InvoiceHeaderSupportingDocumentsUserControl), typeof(JobDeclaration));
	}

	public void TestSupportingDocumentsFieldsControlType()
	{
		SupportingDocumentsControlTestHelper.AssertSupportingDocumentsFieldsControlType(typeof(InvoiceHeaderSupportingDocumentsUserControl), typeof(InvoiceHeaderSupportingDocumentsFieldsControl));
	}

	public void TestGridColumns()
	{
		SupportingDocumentsControlTestHelper.AssertGridColumns(Factory, typeof(InvoiceHeaderSupportingDocumentsUserControl), OrderedColumnNamesAndColumnStyleTypes);
	}

	public void TestCaptionRenderingEnabled()
	{
		SupportingDocumentsControlTestHelper.AssertCaptionRenderingEnabledForUserControlAndFieldsControl(typeof(InvoiceHeaderSupportingDocumentsUserControl));
	}

	public void TestBottomPanelMinimumHeight()
	{
		using (var control = new InvoiceHeaderSupportingDocumentsUserControl())
		{
			control.Show();
			var panel = control.FindSingle<ZPanel>("BottomPanel");
			AssertEquals(200, panel.MinimumSize.Height);
		}
	}

	public void TestGridColumnStyleProperties()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		var collection = new SupportingDocumentCollection(supportingDocument);
		using (var control = new InvoiceHeaderSupportingDocumentsUserControl())
		{
			var grid = control.SupportingDocumentsGrid;
			grid.SetDataBinding(collection, "");
			control.Show();

			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Code).CharacterCasing);
				AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
				var additionalDescriptionColumnStyle = grid.GetColumnStyle(SupportingDocument.Schema.CSI_AdditionalDescription);
				AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, additionalDescriptionColumnStyle.CharacterCasing);
				AssertEquals("CSI_AdditionalDescription: CaptionResourceString.Caption", "Issuing Authority", additionalDescriptionColumnStyle.CaptionResourceString.Caption);
			});
		}
	}

	static IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnNamesAndColumnStyleTypes => new[]
	{
		(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
		(SupportingDocument.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyle)),
		(SupportingDocument.Schema.CSI_AdditionalDescription, typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyle))
	};
}

