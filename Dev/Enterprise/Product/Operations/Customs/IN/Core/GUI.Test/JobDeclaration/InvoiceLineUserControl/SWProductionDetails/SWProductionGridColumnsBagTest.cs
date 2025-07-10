using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SWProductionGridColumnsBag))]
sealed class SWProductionGridColumnsBagTest : TestCase
{
	public void TestColumns()
	{
		var controlBag = SWProductionGridColumnsBag.Instance;
		LayoutTestHelper.AssertGridColumn<ZCalcEditColumnStyleInfo>(controlBag.LineNoCalcEditColumn, SWProduction.Schema.CSI_LineNo, 50);
		LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.ReferenceNumberTextBoxColumn, SWProduction.Schema.CSI_ReferenceNumber, 100);
		LayoutTestHelper.AssertGridColumn<ZCalcEditColumnStyleInfo>(controlBag.BatchQuantityColumn, SWProduction.Schema.CSI_Quantity, 100, info => AssertEquals("BatchQuantityColumn MaxValue", 9999999999.999999m, info.MaxValue));
		LayoutTestHelper.AssertGridColumn<ZDropEditColumnStyleInfo>(controlBag.UnitOfQuantityColumn, SWProduction.Schema.CSI_UnitOfQuantity, 100);
		LayoutTestHelper.AssertGridColumn<ZDateEditColumnStyleInfo>(controlBag.ManufacturingDateColumn, SWProduction.Schema.CSI_DateOfIssue, 100, column => AssertEquals(ZDateTimePickerFormat.Short, column.DateTimeFormat));
		LayoutTestHelper.AssertGridColumn<ZDateEditColumnStyleInfo>(controlBag.ExpiryDateColumn, SWProduction.Schema.CSI_DateOfExpiry, 100, column => AssertEquals(ZDateTimePickerFormat.Short, column.DateTimeFormat));
		LayoutTestHelper.AssertGridColumn<ZDateTimeOffsetEditColumnStyleInfo>(controlBag.BestBeforeDateColumn, SWProduction.Schema.CSI_EffectiveDate, 100);
	}
}
