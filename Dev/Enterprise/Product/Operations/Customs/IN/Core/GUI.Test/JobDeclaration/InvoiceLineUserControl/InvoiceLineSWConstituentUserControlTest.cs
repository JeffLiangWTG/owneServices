using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(InvoiceLineSWConstituentUserControl))]
sealed class InvoiceLineSWConstituentUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using var control = new InvoiceLineSWConstituentUserControl();
		AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
	}

	public void TestSWConstituentColumns()
	{
		using (var control = new InvoiceLineSWConstituentUserControl())
		{
			var swConsituteGrid = control.SWConstituentGrid;
			CombineAssertions(() =>
			{
				AssertEquals("CSI_LineNo", 100, swConsituteGrid.GetColumnStyle(SWConstituent.Schema.CSI_LineNo).Width);
				AssertType<ZCalcEditColumnStyleInfo>(swConsituteGrid.GetColumnStyle(SWConstituent.Schema.CSI_LineNo));

				AssertEquals("CSI_Description", 100, swConsituteGrid.GetColumnStyle(SWConstituent.Schema.CSI_Description).Width);
				AssertType<ZTextBoxColumnStyleInfo>(swConsituteGrid.GetColumnStyle(SWConstituent.Schema.CSI_Description));

				AssertEquals("CSI_Code", 100, swConsituteGrid.GetColumnStyle(SWConstituent.Schema.CSI_Code).Width);
				AssertType<ZTextBoxColumnStyleInfo>(swConsituteGrid.GetColumnStyle(SWConstituent.Schema.CSI_Code));

				var csiQuantityStyle = (ZCalcEditColumnStyleInfo)swConsituteGrid.GetColumnStyle(SWConstituent.Schema.CSI_Quantity);
				AssertEquals("CSI_Quantity Width", 100, csiQuantityStyle.Width);
				AssertType<ZCalcEditColumnStyleInfo>(csiQuantityStyle);
				AssertEquals("CSI_Quantity MaxValue", 100m, csiQuantityStyle.MaxValue);
				AssertEquals("CSI_Quantity DecimalPlaces", 3, csiQuantityStyle.Decimals);

				var csiQuantity2Style = (ZCalcEditColumnStyleInfo)swConsituteGrid.GetColumnStyle(SWConstituent.Schema.CSI_Quantity2);
				AssertEquals("CSI_Quantity2 Width", 100, csiQuantity2Style.Width);
				AssertType<ZCalcEditColumnStyleInfo>(csiQuantity2Style);
				AssertEquals("CSI_Quantity2 MaxValue", 100m, csiQuantity2Style.MaxValue);
				AssertEquals("CSI_Quantity2 DecimalPlaces", 3, csiQuantity2Style.Decimals);

				AssertEquals("CSI_Status", 80, swConsituteGrid.GetColumnStyle(SWConstituent.Schema.CSI_Status).Width);
				AssertType<ZDropEditColumnStyleInfo>(swConsituteGrid.GetColumnStyle(SWConstituent.Schema.CSI_Status));
			});
		}
	}
}

