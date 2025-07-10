using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn.Testing;

sealed class InvoiceLineSupportingDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestGridColumns()
	{
		SupportingDocumentsControlTestHelper.AssertGridColumns(Factory, typeof(InvoiceLineSupportingDocumentsUserControl), OrderedColumnNamesAndColumnStyleTypes);
	}

	public void TestGridColumnProperties()
	{
		using (var control = new InvoiceLineSupportingDocumentsUserControl())
		{
			var grid = control.SupportingDocumentsGrid;
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Value: Caption", "Amount", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == SupportingDocument.Schema.CSI_Value)?.CaptionResourceString.Caption);
				AssertEquals("CSI_DateOfExpiry: Caption", "Validity Date", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == SupportingDocument.Schema.CSI_DateOfExpiry)?.CaptionResourceString.Caption);
				AssertEquals("CSI_AdditionalDescription: Caption", "Issuing Authority", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == SupportingDocument.Schema.CSI_AdditionalDescription)?.CaptionResourceString.Caption);
				AssertEquals("CSI_UnitOfQuantity: Caption", "Unit of Measure", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == SupportingDocument.Schema.CSI_UnitOfQuantity)?.CaptionResourceString.Caption);
			});
		}
	}

	IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnNamesAndColumnStyleTypes => new (string, Type)[]
	{
		(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
		(SupportingDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle)),
		(SupportingDocument.Schema.CSI_Value, typeof(ZCalcEditColumnStyle)),
		(SupportingDocument.Schema.CSI_RX_NKCurrency, typeof(ZCodeFindBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyle)),
		(SupportingDocument.Schema.CSI_AdditionalDescription, typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_ItemNumber, typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyle))
	};
}
